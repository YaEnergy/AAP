using AAP.BackgroundTasks;
using AAP.Properties;
using AAP.Timelines;
using AAP.UI.Themes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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

        public static readonly string DefaultArtFilesDirectoryPath = $@"{ApplicationDataFolderPath}\Saves";
        public static readonly string CharacterPaletteDirectoryPath = $@"{ApplicationDataFolderPath}\CharacterPalettes";
        public static readonly string AutoSaveDirectoryPath = $@"{ApplicationDataFolderPath}\Autosaves";

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

        public static ASCIIArt? CurrentArt
        {
            get
            {
                if (CurrentArtFile == null)
                    return null;

                return CurrentArtFile.Art;
            }
        }

        public static ASCIIArtDraw? CurrentArtDraw
        {
            get
            {
                if (CurrentArtFile == null)
                    return null;

                return CurrentArtFile.ArtDraw;
            }
        }

        public static ObjectTimeline? CurrentArtTimeline
        {
            get
            {
                if (CurrentArtFile == null)
                    return null;

                return CurrentArtFile.ArtTimeline;
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

        private static CharacterPalette currentCharacterPalette = new();
        public static CharacterPalette CurrentCharacterPalette 
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
        public delegate void OnCurrentCharacterPaletteChangedEvent(CharacterPalette palette);
        public static event OnCurrentCharacterPaletteChangedEvent? OnCurrentCharacterPaletteChanged;

        private static readonly ObservableCollection<CharacterPalette> characterPalettes = new();
        public static ObservableCollection<CharacterPalette> CharacterPalettes 
        { 
            get => characterPalettes; 
        }

        private static bool darkMode = Settings.Default.DarkMode;
        public static bool DarkMode
        {
            get => darkMode;
            set
            {
                if (darkMode == value)
                    return;

                darkMode = value;
                SetTheme(darkMode);

                OnModeChanged?.Invoke(darkMode);
            }
        }

        public delegate void OnModeChangedEvent(bool darkMode);
        public static event OnModeChangedEvent? OnModeChanged;

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

#if RELEASE
                TextWriter oldOut = Console.Out;
                StreamWriter logSR = File.CreateText(ApplicationDataFolderPath + @"\log.txt");
                logSR.AutoFlush = true;

                logSR.WriteLine("--CONSOLE LOG--\n");

                Console.SetOut(logSR);
                Console.SetError(logSR);
#endif
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

                Settings.Default.Upgrade();

                //Tools
                Tools.Add(new PencilTool('|', 1));
                Tools.Add(new EraserTool(1));
                Tools.Add(new SelectTool());
                Tools.Add(new MoveTool());
                Tools.Add(new BucketTool('|'));
                Tools.Add(new TextTool());
                Tools.Add(new LineTool('|'));

                SelectToolType(ToolType.None);

                //Preset Character Palettes
                foreach (FileInfo presetFileInfo in new DirectoryInfo($@"{ExecutableDirectory.FullName}\Resources\PresetCharacterPalettes").GetFiles())
                {
                    string presetCharacterPaletteFilePath = @$"{CharacterPaletteDirectoryPath}\{presetFileInfo.Name.Replace(".txt", ".aappal")}";

                    CharacterPalette palette = new();
                    TXTCharacterPalette presetTxTCharacterPalette = new(palette, presetFileInfo.FullName);
                    presetTxTCharacterPalette.Import();

                    palette.IsPresetPalette = true;

                    characterPalettes.Add(palette);

                    ConsoleLogger.Log($"Imported Preset Character Palette File: {presetCharacterPaletteFilePath}");
                }

                //Character Palettes
                foreach (FileInfo fileInfo in new DirectoryInfo(CharacterPaletteDirectoryPath).GetFiles())
                    if (fileInfo.Extension.ToLower() == ".aappal")
                    {
                        CharacterPalette palette = new();

                        AAPPALCharacterPalette fileCharacterPalette = new(palette, fileInfo.FullName);

                        fileCharacterPalette.Import();

                        characterPalettes.Add(palette);

                        ConsoleLogger.Log($"Loaded Character Palette File: {fileInfo.FullName}");
                    } 

                CurrentCharacterPalette = CharacterPalettes[0];

                OnCurrentArtFileChanged += CurrentArtFileChanged;

                SetTheme(Settings.Default.DarkMode);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBoxResult result = MessageBox.Show($"It seems {ProgramTitle} has run into an exception while starting up, and can't open! If this keeps occuring, please inform the creator of AAP!\nOpen log file?", ProgramTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                    Process.Start("explorer.exe", ApplicationDataFolderPath + @"\log.txt");

                mutex.Close();
                return;
            }

            ConsoleLogger.Log("Set up complete!");

            ConsoleLogger.Log("LOADING WINDOW HIDDEN");

            //This needs to be remade
            /*if (args.Length == 1)
                if (File.Exists(args[0]))
                {
                    FileInfo fileInfo = new(args[0]);
                    try
                    {
                        void BackgroundTaskOpenFinish(object? sender, RunWorkerCompletedEventArgs e)
                        {
                            if (e.Error != null)
                                throw e.Error;
                            else if (e.Cancelled)
                                return;
                            else if (e.Result is ASCIIArtFile artFile)
                            {
                                OpenArtFiles.Add(artFile);
                                CurrentArtFile = artFile;
                            }
                            else
                                throw new Exception("Result is not ASCIIArtFile!");
                        }

                        BackgroundTask? bgTask = ASCIIArtFile.OpenAsync(fileInfo.FullName);
                        if (bgTask != null)
                            bgTask.Worker.RunWorkerCompleted += BackgroundTaskOpenFinish;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open {fileInfo.Name}. Exception : {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }*/

            GC.Collect();

            app.Run();

#if RELEASE
            Console.SetOut(oldOut);
            Console.SetError(oldOut);

            logSR.Close();
#endif
            mutex.Close();
        }

        #region Application Events
        private static void OnApplicationExit(object? sender, ExitEventArgs e)
        {
            Console.WriteLine("\n--APPLICATION EXIT--\n");

            ConsoleLogger.Log("Exited with code {0}", e.ApplicationExitCode);

            Console.Out.Close();
        }

        private static void OnThreadException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"\n--THREAD EXCEPTION--\n");
            ConsoleLogger.Error(e.Exception);
            Console.WriteLine("\n--END THREAD EXCEPTION--\n");

            if (!e.Handled)
            {
                Console.WriteLine("\n--THREAD EXCEPTION UNHANDLED |  SHUTTING DOWN--\n");

                MessageBoxResult result = MessageBox.Show($"It seems AAP has run into an unhandled exception, and must close! If this keeps occuring, please inform the creator of AAP!\nOpen log file?", ProgramTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                    Process.Start("explorer.exe", ApplicationDataFolderPath + @"\log.txt");

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
            if (CurrentArt == null)
                return;

            ConsoleLogger.Log("Copy Art String To Clipboard: Copying art string to clipboard...");
            string artString = CurrentArt.GetArtString();

            Clipboard.SetText(artString);
            ConsoleLogger.Log("Copy Art String To Clipboard: Copied art string to clipboard!");
        }

        public static void CopySelectedArtToClipboard()
        {
            if (CurrentArt == null) 
                return;

            if (CurrentLayerID == -1)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            ConsoleLogger.Log("Copy Selected Art To Clipboard: Copying selected art to clipboard...");
            ArtLayer currentLayerClone = (ArtLayer)CurrentArt.ArtLayers[CurrentLayerID].Clone();

            currentLayerClone.Name = "Clipboard";
            currentLayerClone.Crop(SelectedArt);

            IDataObject dataObject = new DataObject();
            dataObject.SetData(typeof(ArtLayer).FullName, currentLayerClone);

            Clipboard.SetDataObject(dataObject);
            ConsoleLogger.Log("Copy Selected Art To Clipboard: Copied selected art to clipboard!");
        }

        public static void PasteLayerFromClipboard()
        {
            if (CurrentArt == null)
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
            CurrentArt.ArtLayers.Insert(CurrentLayerID + 1, clipboardLayer);
            CurrentLayerID += 1;

            CurrentArtTimeline?.NewTimePoint();
            CurrentArt.Update();
            ConsoleLogger.Log("Paste Layer From Clipboard: Pasted layer from clipboard!");
        }

        public static void CutSelectedArt()
        {
            if (CurrentArt == null)
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
            if (CurrentArt == null)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            CurrentArt.Crop(SelectedArt);

            SelectedArt = new(0, 0, SelectedArt.Width, SelectedArt.Height);

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void CropCurrentArtLayerToSelected()
        {
            if (CurrentArt == null)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            if (CurrentLayerID == -1)
                return;

            CurrentArt.ArtLayers[CurrentLayerID].Crop(SelectedArt);

            CurrentArtTimeline?.NewTimePoint();

            CurrentArt.Update();
        }

        public static void FillSelectedWith(char? character)
        {
            if (CurrentArt == null)
                return;

            if (SelectedArt == Rect.Empty)
                return;

            if (CurrentLayerID == -1)
                return;

            CurrentArtDraw?.DrawRectangle(CurrentLayerID, character, SelectedArt);

            CurrentArtTimeline?.NewTimePoint();
        }
        #endregion
        #region Art Selection
        public static void SelectArt()
        {
            if (CurrentArt == null)
            {
                SelectedArt = Rect.Empty;
                return;
            }

            SelectedArt = new(0, 0, CurrentArt.Width, CurrentArt.Height);
        }

        public static void SelectLayer()
        {
            if (CurrentArt == null)
            {
                SelectedArt = Rect.Empty;
                return;
            }

            if (CurrentLayerID == -1)
            {
                SelectedArt = Rect.Empty;
                return;
            }

            ArtLayer layer = CurrentArt.ArtLayers[CurrentLayerID];
            SelectedArt = new(layer.Offset, layer.Size);
        }

        public static void CancelArtSelection()
            => SelectedArt = Rect.Empty;
        #endregion
        #region Layers
        public static void AddArtLayer()
        {
            if (CurrentArt == null)
                return;

            int layerID = CurrentLayerID == -1 ? 0 : CurrentLayerID;
            CurrentArt.ArtLayers.Insert(layerID, new("New Layer", CurrentArt.Width, CurrentArt.Height));
            CurrentLayerID = layerID;

            CurrentArtTimeline?.NewTimePoint();
            CurrentArt.Update();
        }

        public static void DuplicateCurrentArtLayer()
        {
            if (CurrentArt == null)
                return;

            if (CurrentArt.ArtLayers.Count == 0)
                return;

            if (CurrentLayerID == -1)
                return;

            ArtLayer currentArtLayer = CurrentArt.ArtLayers[CurrentLayerID];
            ArtLayer duplicateArtLayer = (ArtLayer)currentArtLayer.Clone();
            duplicateArtLayer.Name += " copy";

            int layerID = CurrentLayerID == -1 ? 0 : CurrentLayerID;
            CurrentArt.ArtLayers.Insert(layerID, duplicateArtLayer);
            CurrentLayerID = layerID;

            CurrentArtTimeline?.NewTimePoint();
            CurrentArt.Update();
        }

        public static void MergeCurrentArtLayerDown()
        {
            if (CurrentArt == null)
                return;

            if (CurrentArt.ArtLayers.Count - CurrentLayerID <= 1)
                return;

            if (CurrentLayerID == -1)
                return;

            ArtLayer currentArtLayer = CurrentArt.ArtLayers[CurrentLayerID];
            currentArtLayer.MergeDown(CurrentArt.ArtLayers[CurrentLayerID + 1]);

            CurrentArt.ArtLayers.RemoveAt(CurrentLayerID + 1);

            CurrentArtTimeline?.NewTimePoint();
            CurrentArt.Update();
        }

        public static void RemoveCurrentArtLayer()
        {
            if (CurrentArt == null)
                return;

            if (CurrentArt.ArtLayers.Count == 0)
                return;

            if (CurrentLayerID == -1)
                return;

            int layerID = CurrentLayerID;

            if (CurrentArt.ArtLayers.Count - 1 == 0)
                CurrentLayerID = -1;

            CurrentArt.ArtLayers.RemoveAt(layerID);

            if (CurrentArt.ArtLayers.Count != 0)
                CurrentLayerID = Math.Clamp(layerID, -1, CurrentArt.ArtLayers.Count - 1);


            CurrentArtTimeline?.NewTimePoint();
            CurrentArt.Update();
        }

        public static void MoveCurrentArtLayer(int amount)
        {
            if (CurrentArt == null)
                return;

            if (CurrentArt.ArtLayers.Count == 0)
                return;

            if (CurrentLayerID == -1)
                return;

            int newIndex = Math.Clamp(CurrentLayerID + amount, 0, CurrentArt.ArtLayers.Count - 1);

            if (newIndex == CurrentLayerID)
                return; //No changes

            CurrentArt.ArtLayers.Move(CurrentLayerID, newIndex);
            CurrentLayerID = newIndex;

            CurrentArtTimeline?.NewTimePoint();
            CurrentArt.Update();
        }

        public static void SetArtLayerName(ArtLayer layer, string name)
        {
            if (layer.Name == name)
                return;

            layer.Name = name;

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void SetArtLayerVisibility(ArtLayer layer, bool visible)
        {
            if (layer.Visible == visible)
                return;

            layer.Visible = visible;

            CurrentArtTimeline?.NewTimePoint();

            CurrentArt?.Update();
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

            IAAPFile<CharacterPalette> paletteFile = new AAPPALCharacterPalette(palette, path);

            Task exportTask = paletteFile.ExportAsync();

            await exportTask;

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
        private static void SetTheme(bool darkMode)
        {
            Current.Resources.Clear();
            Current.Resources.MergedDictionaries.Clear();

            Uri themeSource = darkMode ? new("/Resources/Themes/DarkTheme.xaml", UriKind.Relative) : new("/Resources/Themes/LightTheme.xaml", UriKind.Relative);

            ResourceDictionary resourceDictionary = new() { Source = themeSource };

            Settings.Default.DarkMode = darkMode;

            Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }
        #endregion
    }
}
