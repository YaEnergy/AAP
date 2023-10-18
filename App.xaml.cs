using AAP.BackgroundTasks;
using AAP.FileObjects;
using AAP.Files;
using AAP.Properties;
using AAP.Timelines;
using AAP.UI.Themes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows.Threading;

namespace AAP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly DirectoryInfo? ExecutableDirectory = Environment.ProcessPath != null ? Directory.GetParent(Environment.ProcessPath) : null;
        public static readonly string ProgramTitle = "Kiara's ASCII Art Program";
        public static readonly string Version = "v0.0.1";

        public static readonly int MaxArtArea = 1600000;
        public static readonly int WarningIncrediblyLargeArtArea = 1000000;
        public static readonly int WarningLargeArtArea = 500000;

        public static readonly string ApplicationDataFolderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\AAP\AAP";

        public static readonly string SettingsPath = $@"{ApplicationDataFolderPath}\settings";
        public static readonly string DefaultArtFilesDirectoryPath = $@"{ApplicationDataFolderPath}\Saves";
        public static readonly string CharacterPaletteDirectoryPath = $@"{ApplicationDataFolderPath}\CharacterPalettes";
        public static readonly string AutoSaveDirectoryPath = $@"{ApplicationDataFolderPath}\Autosaves";

        private static AppSettings settings = AppSettings.Default;
        public static AppSettings Settings
        {
            get => settings;
            set
            {
                if (settings == value)
                    return;

                settings = value;
            }
        }

        private static readonly ObservableCollection<ASCIIArtFile> openArtFiles = new();
        public static ObservableCollection<ASCIIArtFile> OpenArtFiles
        {
            get => openArtFiles;
        }

        private static ASCIIArtFile? currentArtFile;
        public static ASCIIArtFile? CurrentArtFile
        {
            get => currentArtFile;
            set
            {
                if (currentArtFile == value)
                    return;
                
                currentArtFile = value;
                CurrentLayerID = -1;

                OnCurrentArtFileChanged?.Invoke(currentArtFile);
            }
        }
        
        public delegate void CurrentArtFileChangedEvent(ASCIIArtFile? artFile);
        public static event CurrentArtFileChangedEvent? OnCurrentArtFileChanged;

        private static Rect selectedArt = Rect.Empty;
        public static Rect SelectedArt
        { 
            get => selectedArt; 
            set 
            {
                if (selectedArt == value)
                    return;

                selectedArt = value; 
                OnSelectedArtChanged?.Invoke(value); 
            } 
        }
        public delegate void SelectArtEvent(Rect selection);
        public static event SelectArtEvent? OnSelectedArtChanged;

        private static int currentLayerID = -1;
        public static int CurrentLayerID 
        { 
            get => currentLayerID; 
            set 
            { 
                if (currentLayerID == value) 
                    return;

                currentLayerID = value; 
                OnCurrentLayerIDChanged?.Invoke(value); 
            } 
        }
        public delegate void CurrentLayerIDChangedEvent(int currentLayerID);
        public static event CurrentLayerIDChangedEvent? OnCurrentLayerIDChanged;

        private static readonly List<Tool> tools = new();
        public static List<Tool> Tools 
        { 
            get => tools; 
        }

        private static Tool? currentTool = null;
        public static Tool? CurrentTool 
        { 
            get => currentTool;
            private set
            {
                if (currentTool == value) 
                    return;
                
                currentTool = value;

                OnCurrentToolChanged?.Invoke(value);

                ConsoleLogger.Log($"Selected Tool: {value}");
                ConsoleLogger.Log($"Selected ToolType: {(value != null ? value.Type : ToolType.None)}");
            }
        }

        public delegate void CurrentToolChangedEvent(Tool? tool);
        public static event CurrentToolChangedEvent? OnCurrentToolChanged;

        private static CharacterPalette? currentCharacterPalette;
        public static CharacterPalette? CurrentCharacterPalette 
        { 
            get => currentCharacterPalette;
            set 
            {
                if (currentCharacterPalette == value)
                    return;

                currentCharacterPalette = value; 
                OnCurrentCharacterPaletteChanged?.Invoke(value); 
            } 
        }
        public delegate void OnCurrentCharacterPaletteChangedEvent(CharacterPalette? palette);
        public static event OnCurrentCharacterPaletteChangedEvent? OnCurrentCharacterPaletteChanged;

        private static readonly ObservableCollection<CharacterPalette> characterPalettes = new();
        public static ObservableCollection<CharacterPalette> CharacterPalettes 
        { 
            get => characterPalettes; 
        }

        private static readonly System.Threading.Timer autosaveTimer = new(OnAutosaveTimerTick, null, Settings.AutosaveInterval, Settings.AutosaveInterval);
        public static System.Threading.Timer AutosaveTimer
        {
            get => autosaveTimer;
        }

        private static Language language = new();
        public static Language Language
        {
            get => language;
            set
            {
                if (language == value)
                    return;

                language = value;
                OnLanguageChanged?.Invoke(language);
            }
        }

        public delegate void OnLanguageChangedEvent(Language language);
        public static event OnLanguageChangedEvent? OnLanguageChanged;

        public App()
        {
            
        }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [STAThread()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.3.0")]
        public static void Main(string[] args)
        {
            using Mutex mutex = new(false, ProgramTitle + "_" + Environment.UserName);
            if (!mutex.WaitOne(0, false)) //If another instance is already running, quit
            {
                System.Windows.MessageBox.Show($"There is already an instance of {ProgramTitle} running!", ProgramTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                mutex.Close();
                return;
            }
            
            App app = new();
            app.InitializeComponent();

            app.Exit += OnApplicationExit;
            app.DispatcherUnhandledException += (sender, e) => OnThreadException(sender, e);
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;

            SplashScreen splashScreen = new("/Resources/Images/ProgramIcons/icon2_grayscale.png");
            splashScreen.Show(true);

            try
            {
                if (ExecutableDirectory == null)
                    throw new NullReferenceException(nameof(ExecutableDirectory));

                if (!ExecutableDirectory.Exists)
                    throw new Exception(nameof(ExecutableDirectory) + " doesn't exist!");

                if (!Directory.Exists(ApplicationDataFolderPath))
                {
                    DirectoryInfo applicationDataDirInfo = Directory.CreateDirectory(ApplicationDataFolderPath);
                    ConsoleLogger.Log($"Created directory {applicationDataDirInfo.FullName}");
                }

                ConsoleLogger.LogFileOut = File.CreateText(ApplicationDataFolderPath + @"\log.txt");
                ConsoleLogger.LogFileOut.WriteLine($"--CONSOLE LOG {DateTimeOffset.UtcNow.ToString("d")}--\n");

                ConsoleLogger.LogFileError = File.CreateText(ApplicationDataFolderPath + @"\threadErrorLog.txt");
                ConsoleLogger.LogFileError.WriteLine($"--ERROR LOG {DateTimeOffset.UtcNow.ToString("d")}--\n");

                if (!Directory.Exists(DefaultArtFilesDirectoryPath))
                {
                    DirectoryInfo defaultArtFilesDirInfo = Directory.CreateDirectory(DefaultArtFilesDirectoryPath);
                    ConsoleLogger.Log($"Created directory {defaultArtFilesDirInfo.FullName}");
                }

                if (!Directory.Exists(CharacterPaletteDirectoryPath))
                {
                    DirectoryInfo characterPaletteDirInfo = Directory.CreateDirectory(CharacterPaletteDirectoryPath);
                    ConsoleLogger.Log($"Created directory {characterPaletteDirInfo.FullName}");
                }

                if (!Directory.Exists(AutoSaveDirectoryPath))
                {
                    DirectoryInfo autoSaveDirInfo = Directory.CreateDirectory(AutoSaveDirectoryPath);
                    ConsoleLogger.Log($"Created directory {autoSaveDirInfo.FullName}");
                }

                if (File.Exists(SettingsPath))
                {
                    FileStream fs = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read);
                    try
                    {
                        Settings = AppSettings.Decode(fs);
                        ConsoleLogger.Log("Decoded settings");
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.Error(ex);
                        MessageBox.Show("Settings failed to load! Defaulted to default settings.", "Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        fs.Dispose();
                    }
                }
                else
                    ConsoleLogger.Log("No settings file found!");

                Stream languageStream;
                try
                {
                    languageStream = GetResourceStream(new($"/Resources/Languages/{Settings.LanguageName}.json", UriKind.Relative)).Stream;
                }
                catch (Exception ex)
                {
                    Settings.LanguageName = "English";
                    languageStream = GetResourceStream(new($"/Resources/Languages/{Settings.LanguageName}.json", UriKind.Relative)).Stream;

                    ConsoleLogger.Error(ex);
                    MessageBox.Show("Failed to get language resource! Defaulted to English!\nException message: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Language = Language.Decode(languageStream);

                SetTheme(Settings.DarkMode);

                TimeSpan interval = Settings.AutosaveFiles ? Settings.AutosaveInterval : Timeout.InfiniteTimeSpan;
                AutosaveTimer.Change(interval, interval);
                

                //Tools
                Tools.Add(new PencilTool('|', 1));
                Tools.Add(new EraserTool(1));
                Tools.Add(new SelectTool());
                Tools.Add(new MoveTool());
                Tools.Add(new BucketTool('|'));
                Tools.Add(new TextTool());
                Tools.Add(new LineTool('|'));

                SelectToolType(ToolType.None);

                RefreshPalettes();

                OnCurrentArtFileChanged += CurrentArtFileChanged;

                Settings.PropertyChanged += SettingsPropertyChanged;
                OnLanguageChanged += (language) => RefreshPalettes();
            }
            catch (Exception ex)
            {
                if (ConsoleLogger.LogFileError == null)
                {
                    ConsoleLogger.LogFileError = File.CreateText(ApplicationDataFolderPath + @"\threadErrorLog.txt");
                    ConsoleLogger.LogFileError.WriteLine($"--ERROR LOG {DateTimeOffset.UtcNow.TimeOfDay}--\n");
                }

                ConsoleLogger.Error(ex);

                ConsoleLogger.LogFileOut?.Flush();
                ConsoleLogger.LogFileError?.Flush();

                ConsoleLogger.LogFileOut?.Close();
                ConsoleLogger.LogFileError?.Close();

                MessageBoxResult result = MessageBox.Show($"It seems {ProgramTitle} has run into an exception while starting up, and can't open! If this keeps occuring, please inform the creator of AAP!\nOpen log file?", ProgramTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                    Process.Start("explorer.exe", ApplicationDataFolderPath + @"\log.txt");

                mutex.Close();
                return;
            }

            ConsoleLogger.Log("Set up complete!");

            if (args.Length == 1)
            {
                if (File.Exists(args[0]))
                {
                    FileInfo fileInfo = new(args[0]);
                    ConsoleLogger.Log($"Opening file {fileInfo.FullName}");

                    try
                    {
                        ASCIIArtFile artFile = ASCIIArtFile.Open(fileInfo.FullName);
                        OpenArtFiles.Add(artFile);
                        CurrentArtFile = artFile;

                        ConsoleLogger.Log($"Opened file {fileInfo.FullName}!");
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.Error(ex);
                        MessageBox.Show($"Failed to open {fileInfo.Name}. Exception: {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            GC.Collect();

            app.Run();

            ConsoleLogger.LogFileOut?.Flush();
            ConsoleLogger.LogFileError?.Flush();

            ConsoleLogger.LogFileOut?.Close();
            ConsoleLogger.LogFileError?.Close();
            Console.Out.Close();

            mutex.Close();
        }


        #region Application Events
        private static async void OnAutosaveTimerTick(object? sender)
        {
            if (!Settings.AutosaveFiles)
                return;

            while (Settings.AutosaveFilePaths.Count > 25)
            {
                string filePath = Settings.AutosaveFilePaths[0];
                Settings.AutosaveFilePaths.RemoveAt(0);

                if (!File.Exists(filePath))
                {
                    ConsoleLogger.Log("Filepath: " + filePath + " doesn't exist, skipped but still removed from list. (Could have been moved/renamed/deleted)");
                    continue;
                }

                File.Delete(filePath);

                ConsoleLogger.Log("Removed old autosave path: " + filePath);
            }

            DateTime dateTime = DateTime.Now;
            string autosaveFilePath = AutoSaveDirectoryPath + $@"\Autosave_{dateTime.Day}-{dateTime.Month}-{dateTime.Year}_{dateTime.Hour}-{dateTime.Minute}-{dateTime.Second}";
            string ext = ".aaf";

            ConsoleLogger.Log("Autosaving all open files...");

            foreach (ASCIIArtFile artFile in OpenArtFiles)
            {
                if (!artFile.UnsavedChanges)
                {
                    ConsoleLogger.Log($"File {artFile.FileName} skipped because it has no unsaved changes.");
                    continue;
                }

                string finalPath = autosaveFilePath;
                if (File.Exists(finalPath + ext))
                {
                    int saveNumber = 1;
                    while (File.Exists(finalPath + "+" + saveNumber + ext))
                        saveNumber++;

                    finalPath += "+" + saveNumber;
                }

                ConsoleLogger.Log("Autosaving " + artFile.FileName + " to " + finalPath + ext);

                try
                {
                    await artFile.ExportAsync(finalPath + ext, null);
                    Settings.AutosaveFilePaths.Add(finalPath + ext);
                    ConsoleLogger.Log("Autosaved " + artFile.FileName + " to " + finalPath + ext);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Autosave for {artFile.FileName} failed! Exception message: " + ex.Message);
                    ConsoleLogger.Error(ex);
                }
            }

            ConsoleLogger.Log("Autosaved all open files");

            try
            {
                await SaveSettingsAsync();
                ConsoleLogger.Log("Saved settings");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Settings update while autosaving failed! Exception message: " + ex.Message);
                ConsoleLogger.Error(ex);
            }
        }

        private static void OnApplicationExit(object? sender, ExitEventArgs e)
        {
            ConsoleLogger.Log("\n--APPLICATION EXIT--\n");

            ConsoleLogger.Log("Application exited with code {0}", e.ApplicationExitCode);
        }

        private static void OnThreadException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (ConsoleLogger.LogFileError == null)
            {
                ConsoleLogger.LogFileError = File.CreateText(ApplicationDataFolderPath + @"\threadErrorLog.txt");
                ConsoleLogger.LogFileError.WriteLine($"--ERROR LOG {DateTimeOffset.UtcNow:d}--\n");
            }

            ConsoleLogger.Log($"\n--THREAD EXCEPTION--\n");
            ConsoleLogger.Error(e.Exception);
            ConsoleLogger.Log("\n--END THREAD EXCEPTION--\n");

            if (!e.Handled)
            {
                Console.WriteLine("\n--THREAD EXCEPTION UNHANDLED |  SHUTTING DOWN--\n");

                MessageBoxResult result = MessageBox.Show($"It seems AAP has run into an unhandled exception, and must close! If this keeps occuring, please inform the creator of AAP!\nOpen log file?", ProgramTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                    Process.Start("explorer.exe", ApplicationDataFolderPath + @"\threadErrorLog.txt");

                Current.Shutdown(-1);
            }
        }
        #endregion
        #region Files
        public static void CurrentArtFileChanged(ASCIIArtFile? artFile)
            => SelectedArt = Rect.Empty;

        public static void SetArtAsNewFile(ASCIIArt? art)
        {
            if (art != null)
            {
                ASCIIArtFile artFile = new(art);
                artFile.SavePath = null;
                artFile.UnsavedChanges = true;
                OpenArtFiles.Add(artFile);

                CurrentArtFile = artFile;
            }
            else
                CurrentArtFile = null;

            CurrentLayerID = -1;
        }

        public static void CopyArtStringToClipboard()
        {
            if (CurrentArtFile == null)
                return;

            ConsoleLogger.Log("Copy Art String To Clipboard: Copying art string to clipboard...");
            string artString = CurrentArtFile.Art.GetArtString();

            Clipboard.SetText(artString);
            ConsoleLogger.Log("Copy Art String To Clipboard: Copied art string to clipboard!");
        }

        public static void CopySelectedArtToClipboard()
        {
            if (CurrentArtFile == null) 
                return;

            if (CurrentLayerID == -1)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            ConsoleLogger.Log("Copy Selected Art To Clipboard: Copying selected art to clipboard...");
            ArtLayer currentLayerClone = (ArtLayer)CurrentArtFile.Art.ArtLayers[CurrentLayerID].Clone();

            currentLayerClone.Name = "Clipboard";
            currentLayerClone.Crop(SelectedArt);

            IDataObject dataObject = new DataObject();
            dataObject.SetData(typeof(ArtLayer).FullName, currentLayerClone);

            Clipboard.SetDataObject(dataObject);
            ConsoleLogger.Log("Copy Selected Art To Clipboard: Copied selected art to clipboard!");
        }

        public static void PasteLayerFromClipboard()
        {
            if (CurrentArtFile == null)
                return;

            IDataObject? dataObject = Clipboard.GetDataObject();

            if (dataObject == null)
            {
                ConsoleLogger.Inform("No data object on the clipboard!");
                return;
            }

            if (dataObject.GetDataPresent(typeof(ArtLayer).Name))
            {
                ConsoleLogger.Inform("Data present in clipboard is not ArtLayer!");
                return;
            }

            if (dataObject.GetData(typeof(ArtLayer).FullName) is not ArtLayer clipboardLayer)
                return;

            ConsoleLogger.Log("Paste Layer From Clipboard: Pasting layer from clipboard...");
            int layerID = CurrentLayerID == -1 ? 0 : CurrentLayerID;
            CurrentArtFile.Art.ArtLayers.Insert(layerID, clipboardLayer);
            CurrentLayerID = layerID;

            CurrentArtFile.ArtTimeline?.NewTimePoint();
            CurrentArtFile.Art.Update();
            ConsoleLogger.Log("Paste Layer From Clipboard: Pasted layer from clipboard!");
        }

        public static void CutSelectedArt()
        {
            if (CurrentArtFile == null)
                return;

            if (CurrentLayerID == -1)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            CopySelectedArtToClipboard();

            FillSelectedWith(null);
        }
        #endregion
        #region Art
        public static void CropArtFileToSelected()
        {
            if (CurrentArtFile == null)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            CurrentArtFile.Art.Crop(SelectedArt);

            SelectedArt = new(0, 0, SelectedArt.Width, SelectedArt.Height);

            CurrentArtFile.ArtTimeline?.NewTimePoint();
        }

        public static void CropCurrentArtLayerToSelected()
        {
            if (CurrentArtFile == null)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            if (CurrentLayerID == -1)
                return;

            CurrentArtFile.Art.ArtLayers[CurrentLayerID].Crop(SelectedArt);

            CurrentArtFile.ArtTimeline?.NewTimePoint();

            CurrentArtFile.Art.Update();
        }

        public static void FitAllLayersWithinArt()
        {
            if (CurrentArtFile == null)
                return;

            CurrentLayerID = -1;

            Stack<ArtLayer> removingLayers = new();
            foreach (ArtLayer layer in CurrentArtFile.Art.ArtLayers)
            {
                if (layer.OffsetX >= CurrentArtFile.Art.Width || layer.OffsetY >= CurrentArtFile.Art.Height || layer.OffsetX + layer.Width < 0 || layer.OffsetY + layer.Height < 0)
                {
                    removingLayers.Push(layer);
                    continue;
                }

                int offsetX = Math.Clamp(layer.OffsetX, 0, CurrentArtFile.Art.Width - 1);
                int offsetY = Math.Clamp(layer.OffsetY, 0, CurrentArtFile.Art.Height - 1);

                int width = Math.Clamp(layer.Width - Math.Abs(layer.OffsetX - offsetX), 0, CurrentArtFile.Art.Width - offsetX);
                int height = Math.Clamp(layer.Height - Math.Abs(layer.OffsetY - offsetY), 0, CurrentArtFile.Art.Height - offsetY);

                if (width <= 0 || height <= 0)
                {
                    removingLayers.Push(layer);
                    continue;
                }

                layer.Crop(new(offsetX, offsetY, width, height));
            }

            while (removingLayers.Count > 0)
                CurrentArtFile.Art.ArtLayers.Remove(removingLayers.Pop());

            CurrentArtFile.ArtTimeline?.NewTimePoint();

            CurrentArtFile.Art.Update();
        }

        public static void FillSelectedWith(char? character)
        {
            if (CurrentArtFile == null)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            if (CurrentLayerID == -1)
                return;

            CurrentArtFile.ArtDraw?.DrawFilledRectangle(CurrentLayerID, character, SelectedArt);

            CurrentArtFile.ArtTimeline?.NewTimePoint();

            CurrentArtFile.Art?.Update();
        }
        #endregion
        #region Art Selection
        public static void SelectCanvas()
        {
            if (CurrentArtFile == null)
            {
                SelectedArt = Rect.Empty;
                return;
            }

            SelectedArt = new(0, 0, CurrentArtFile.Art.Width, CurrentArtFile.Art.Height);
        }

        public static void SelectLayer()
        {
            if (CurrentArtFile == null)
            {
                SelectedArt = Rect.Empty;
                return;
            }

            if (CurrentLayerID == -1)
            {
                SelectedArt = Rect.Empty;
                return;
            }

            ArtLayer layer = CurrentArtFile.Art.ArtLayers[CurrentLayerID];
            SelectedArt = new(layer.Offset, layer.Size);
        }

        public static void CancelArtSelection()
            => SelectedArt = Rect.Empty;
        #endregion
        #region Layers
        public static void AddArtLayer()
        {
            if (CurrentArtFile == null)
                return;

            int layerID = CurrentLayerID == -1 ? 0 : CurrentLayerID;
            CurrentArtFile.Art.ArtLayers.Insert(layerID, new(Language.GetString("Default_Layers_NewLayer"), CurrentArtFile.Art.Width, CurrentArtFile.Art.Height));
            CurrentLayerID = layerID;

            CurrentArtFile.ArtTimeline?.NewTimePoint();
            CurrentArtFile.Art.Update();
        }

        public static void DuplicateCurrentArtLayer()
        {
            if (CurrentArtFile == null)
                return;

            if (CurrentArtFile.Art.ArtLayers.Count == 0)
                return;

            if (CurrentLayerID == -1)
                return;

            ArtLayer currentArtLayer = CurrentArtFile.Art.ArtLayers[CurrentLayerID];
            ArtLayer duplicateArtLayer = (ArtLayer)currentArtLayer.Clone();
            duplicateArtLayer.Name = string.Format(Language.GetString("Default_Layers_CloneFormat"), duplicateArtLayer.Name);

            int layerID = CurrentLayerID == -1 ? 0 : CurrentLayerID;
            CurrentArtFile.Art.ArtLayers.Insert(layerID, duplicateArtLayer);
            CurrentLayerID = layerID;

            CurrentArtFile.ArtTimeline?.NewTimePoint();
            CurrentArtFile.Art.Update();
        }

        public static void MergeCurrentArtLayerDown()
        {
            if (CurrentArtFile == null)
                return;

            if (CurrentArtFile.Art.ArtLayers.Count - CurrentLayerID <= 1)
                return;

            if (CurrentLayerID == -1)
                return;

            ArtLayer currentArtLayer = CurrentArtFile.Art.ArtLayers[CurrentLayerID];
            currentArtLayer.MergeDown(CurrentArtFile.Art.ArtLayers[CurrentLayerID + 1]);

            CurrentArtFile.Art.ArtLayers.RemoveAt(CurrentLayerID + 1);

            CurrentArtFile.ArtTimeline?.NewTimePoint();
            CurrentArtFile.Art.Update();
        }

        public static void RemoveCurrentArtLayer()
        {
            if (CurrentArtFile == null)
                return;

            if (CurrentArtFile.Art.ArtLayers.Count == 0)
                return;

            if (CurrentLayerID == -1)
                return;

            int layerID = CurrentLayerID;

            if (CurrentArtFile.Art.ArtLayers.Count - 1 == 0)
                CurrentLayerID = -1;

            CurrentArtFile.Art.ArtLayers.RemoveAt(layerID);

            if (CurrentArtFile.Art.ArtLayers.Count != 0)
                CurrentLayerID = Math.Clamp(layerID, -1, CurrentArtFile.Art.ArtLayers.Count - 1);

            CurrentArtFile.ArtTimeline?.NewTimePoint();
            CurrentArtFile.Art.Update();
        }

        public static void MoveCurrentArtLayer(int amount)
        {
            if (CurrentArtFile == null)
                return;

            if (CurrentArtFile.Art.ArtLayers.Count == 0)
                return;

            if (CurrentLayerID == -1)
                return;

            int newIndex = Math.Clamp(CurrentLayerID + amount, 0, CurrentArtFile.Art.ArtLayers.Count - 1);

            if (newIndex == CurrentLayerID)
                return; //No changes

            CurrentArtFile.Art.ArtLayers.Move(CurrentLayerID, newIndex);
            CurrentLayerID = newIndex;

            CurrentArtFile.ArtTimeline?.NewTimePoint();
            CurrentArtFile.Art.Update();
        }

        public static void SetArtLayerName(ArtLayer layer, string name)
        {
            if (layer.Name == name)
                return;

            layer.Name = name;

            CurrentArtFile?.ArtTimeline.NewTimePoint();
        }

        public static void SetArtLayerVisibility(ArtLayer layer, bool visible)
        {
            if (layer.Visible == visible)
                return;

            layer.Visible = visible;

            CurrentArtFile?.ArtTimeline.NewTimePoint();

            CurrentArtFile?.Art.Update();
        }

        #endregion
        #region Tools
        public static void SelectToolType(ToolType toolType)
        {
            foreach (Tool tool in Tools)
                if (tool.Type == toolType)
                {
                    CurrentTool = tool;
                    return;
                }

            CurrentTool = null;
        }

        public static void SelectCharacterTool(char? character)
        {
            if (CurrentTool == null)
            {
                ConsoleLogger.Warn(nameof(CurrentTool) + " is null!");
                return;
            }

            if (CurrentTool is not ICharacterSelectable characterSelectableTool)
            {
                ConsoleLogger.Warn(nameof(CurrentTool) + " can not select characters!");
                return;
            }

            characterSelectableTool.Character = character;

            ConsoleLogger.Log("Selected Character: " + character.ToString());
        }
        #endregion
        #region Palettes
        public static void RefreshPalettes()
        {
            if (ExecutableDirectory == null)
                throw new NullReferenceException(nameof(ExecutableDirectory) + " is null!");

            CharacterPalettes.Clear();

            //Preset Character Palettes
            foreach (FileInfo presetFileInfo in new DirectoryInfo($@"{ExecutableDirectory.FullName}\Resources\PresetCharacterPalettes").GetFiles())
            {
                TextCharacterPaletteDecoder presetTxTCharacterPalette = new(new FileStream(presetFileInfo.FullName, FileMode.Open, FileAccess.Read));
                CharacterPalette palette = presetTxTCharacterPalette.Decode();
                presetTxTCharacterPalette.Close();

                palette.Name = Language.GetString("PresetPalette_" + Path.GetFileNameWithoutExtension(presetFileInfo.FullName));
                palette.IsPresetPalette = true;

                characterPalettes.Add(palette);

                ConsoleLogger.Log($"Imported Preset Character Palette File: {presetFileInfo.FullName}");
            }

            //Character Palettes
            foreach (FileInfo fileInfo in new DirectoryInfo(CharacterPaletteDirectoryPath).GetFiles())
                if (fileInfo.Extension.ToLower() == ".aappal")
                {
                    AAPPALCharacterPaletteDecoder fileCharacterPalette = new(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read));
                    CharacterPalette palette = fileCharacterPalette.Decode();

                    fileCharacterPalette.Close();

                    characterPalettes.Add(palette);

                    ConsoleLogger.Log($"Loaded Character Palette File: {fileInfo.FullName}");
                }

            CurrentCharacterPalette = CharacterPalettes[0];
        }

        public static async Task ExportPaletteFileAsync(CharacterPalette palette)
        {
            if (palette.Name == string.Empty)
                throw new Exception("Invalid Palette Name! Palette name can not be empty!");
            
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

            foreach (char fileNameChar in palette.Name.ToCharArray())
                if (invalidFileNameChars.Contains(fileNameChar))
                {
                    string invalidFileNameCharsString = "";
                    foreach (char invalidFileNameChar in invalidFileNameChars)
                        invalidFileNameCharsString += invalidFileNameChar.ToString();

                    throw new Exception($"Invalid Palette Name! Palette name can not contain any of these characters: {invalidFileNameCharsString}");
                }

            string path = $@"{CharacterPaletteDirectoryPath}\{palette.Name}.aappal";

            FileInfo fileInfo = new(path);

            ConsoleLogger.Log("Export Palette: Exporting art file to " + path);

            FileObjectEncoder<CharacterPalette> paletteFile = new AAPPALCharacterPaletteEncoder(palette, File.Create(fileInfo.FullName));

            Task exportTask = paletteFile.EncodeAsync();

            await exportTask;

            paletteFile.Close();

            if (exportTask.IsFaulted && exportTask.Exception != null)
            {
                ConsoleLogger.Error("Export Palette: ", exportTask.Exception);
                throw exportTask.Exception;
            }
            else
                ConsoleLogger.Log("Export Palette: Palette file exported to " + fileInfo.FullName + "!");
        }

        public static async Task RemovePaletteFileAsync(CharacterPalette palette)
        {
            string path = $@"{CharacterPaletteDirectoryPath}\{palette.Name}.aappal";
            FileInfo fileInfo = new(path);

            if (!fileInfo.Exists)
                throw new FileNotFoundException("Can not remove unknown file!", fileInfo.FullName);

            Task deleteTask = Task.Run(() => File.Delete(path));

            await deleteTask;

            ConsoleLogger.Log("Remove Palette: Palette file removed from " + fileInfo.FullName + "!");
        }

        public static async Task EditPaletteFileAsync(string originalName, CharacterPalette palette)
        {
            if (originalName == palette.Name)
            {
                ConsoleLogger.Log("Edit Palette: Name has not changed, will export normally...");
                Task exportPaletteTask = ExportPaletteFileAsync(palette);
                await exportPaletteTask;

                if (exportPaletteTask.IsFaulted && exportPaletteTask.Exception != null)
                {
                    ConsoleLogger.Error("Edit Palette: ", exportPaletteTask.Exception);
                    throw exportPaletteTask.Exception;
                }
                else
                    ConsoleLogger.Log($"Edit Palette: Palette {palette.Name} file edited!");

                return;
            }

            string removePath = $@"{CharacterPaletteDirectoryPath}\{originalName}.aappal";
            FileInfo removeFileInfo = new(removePath);

            if (!removeFileInfo.Exists)
                throw new FileNotFoundException("Can not remove unknown file!", removeFileInfo.FullName);

            ConsoleLogger.Log("Edit Palette: Removing palette file at " + removePath);
            Task deleteTask = Task.Run(() => File.Delete(removePath));

            await deleteTask;

            if (palette.Name == string.Empty)
                throw new Exception("Invalid Palette Name! Palette name can not be empty!");

            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

            foreach (char fileNameChar in palette.Name.ToCharArray())
                if (invalidFileNameChars.Contains(fileNameChar))
                {
                    string invalidFileNameCharsString = "";
                    foreach (char invalidFileNameChar in invalidFileNameChars)
                        invalidFileNameCharsString += invalidFileNameChar.ToString();

                    throw new Exception($"Invalid Palette Name! Palette name can not contain any of these characters: {invalidFileNameCharsString}");
                }

            string newPath = $@"{CharacterPaletteDirectoryPath}\{palette.Name}.aappal";

            ConsoleLogger.Log("Export Palette: Exporting palette to " + newPath);

            Task editPaletteTask = ExportPaletteFileAsync(palette);

            await editPaletteTask;

            return;
        }
        #endregion
        #region Resources
        public static void SaveSettings()
        {
            using FileStream fs = File.Create(SettingsPath);
            Settings.Encode(fs);
        }

        public static async Task SaveSettingsAsync()
        {
            using FileStream fs = File.Create(SettingsPath);
            await Settings.EncodeAsync(fs);
        }

        private static void SettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not AppSettings changedSettings)
                return;

            switch(e.PropertyName)
            {
                case nameof(changedSettings.DarkMode):
                    SetTheme(changedSettings.DarkMode);
                    break;
                case nameof(changedSettings.AutosaveInterval):
                case nameof(changedSettings.AutosaveFiles):
                    TimeSpan interval = changedSettings.AutosaveFiles ? changedSettings.AutosaveInterval : Timeout.InfiniteTimeSpan;
                    AutosaveTimer.Change(interval, interval);
                    break;
                case nameof(changedSettings.LanguageName):
                    Stream languageStream;
                    try
                    {
                        languageStream = GetResourceStream(new($"/Resources/Languages/{Settings.LanguageName}.json", UriKind.Relative)).Stream;
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.Error(ex);
                        MessageBox.Show("Failed to get language resource! Defaulted to English!\nException message: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        
                        changedSettings.LanguageName = "English";
                        return;
                    }

                    Language = Language.Decode(languageStream);

                    break;
                default:
                    break;
            }
        }

        private static void SetTheme(bool darkMode)
        {
            Current.Resources.Clear();
            Current.Resources.MergedDictionaries.Clear();

            Uri themeSource = darkMode ? new("/Resources/Themes/DarkTheme.xaml", UriKind.Relative) : new("/Resources/Themes/LightTheme.xaml", UriKind.Relative);

            ResourceDictionary resourceDictionary = new() { Source = themeSource };

            Settings.DarkMode = darkMode;

            Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }
        #endregion
    }
}
