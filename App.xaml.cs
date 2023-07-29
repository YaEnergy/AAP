using AAP.Timelines;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AAP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string ProgramTitle = "ASCII Art Program";
        public static readonly string Version = "v0.0.1";

        public static readonly int MaxArtArea = 10000000;
        public static readonly int WarningIncrediblyLargeArtArea = 1000000;
        public static readonly int WarningLargeArtArea = 500000;
        public readonly static int MaxCharacterPaletteCharacters = 200;

        public static readonly string ApplicationDataFolderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\AAP\AAP";

        public static readonly string DefaultArtFilesDirectoryPath = $@"{ApplicationDataFolderPath}\Saves";
        public static readonly string CharacterPaletteDirectoryPath = $@"{ApplicationDataFolderPath}\CharacterPalettes";
        public static readonly string AutoSaveDirectoryPath = $@"{ApplicationDataFolderPath}\Autosaves";

        private static ObjectTimeline? currentArtTimeline;
        public static ObjectTimeline? CurrentArtTimeline { get => currentArtTimeline; }

        private static ASCIIArtDraw? currentArtDraw;
        public static ASCIIArtDraw? CurrentArtDraw { get => currentArtDraw; }

        private static ASCIIArt? currentArt;
        public static ASCIIArt? CurrentArt
        {
            get => currentArt;
            set
            {
                currentArt = value;
                currentArtDraw = value == null ? null : new(value);
                currentArtTimeline = value == null ? null : new(value, 25);
                OnCurrentArtChanged?.Invoke(currentArt, currentArtDraw, currentArtTimeline);
            }
        }
        public delegate void CurrentArtChangedEvent(ASCIIArt? art, ASCIIArtDraw? artDraw, ObjectTimeline? artTimeline);
        public static event CurrentArtChangedEvent? OnCurrentArtChanged;

        private static string? currentFilePath;
        public static string? CurrentFilePath { get => currentFilePath; set { currentFilePath = value; OnCurrentFilePathChanged?.Invoke(value); } }
        public delegate void CurrentFilePathChangedEvent(string? filePath);
        public static event CurrentFilePathChangedEvent? OnCurrentFilePathChanged;

        private static Rect selected = Rect.Empty;
        public static Rect Selected { get => selected; set { selected = value; OnSelectionChanged?.Invoke(value); } }
        public delegate void OnSelectionChangedEvent(Rect selection);
        public static event OnSelectionChangedEvent? OnSelectionChanged;

        private static int currentLayerID = 0;
        public static int CurrentLayerID { get => currentLayerID; set { currentLayerID = value; OnCurrentLayerIDChanged?.Invoke(value); } }
        public delegate void CurrentLayerIDChangedEvent(int currentLayerID);
        public static event CurrentLayerIDChangedEvent? OnCurrentLayerIDChanged;

        private static List<Tool> tools = new();
        public static List<Tool> Tools { get => tools; set => tools = value; }

        private static Tool? currentTool = null;
        public static Tool? CurrentTool { get => currentTool; }

        private static ToolType currentToolType = ToolType.None;
        public static ToolType CurrentToolType
        {
            get => currentToolType;
            set
            {
                if (currentTool != null)
                {
                    currentTool.OnActivateEnd -= OnToolActivateEnd;
                }

                currentToolType = value;

                if (value != ToolType.None)
                {
                    foreach (Tool tool in Tools)
                        if (tool.Type == currentToolType)
                            currentTool = tool;
                }
                else
                    currentTool = null;

                if(currentTool != null)
                {
                    currentTool.OnActivateEnd += OnToolActivateEnd;
                }

                OnCurrentToolChanged?.Invoke(currentTool);

                Console.WriteLine("Selected ToolType: " + value.ToString());
                Console.WriteLine("Selected Tool: " + currentTool?.ToString());

            }
        }
        public delegate void CurrentToolChangedEvent(Tool? tool);
        public static event CurrentToolChangedEvent? OnCurrentToolChanged;

        private static CharacterPalette currentCharacterPalette = CharacterPalette.ImportFilePath(@"Resources\PresetCharacterPalettes\Main ASCII Characters.txt") ?? new("Unknown", new List<char>());
        public static CharacterPalette CurrentCharacterPalette { get => currentCharacterPalette; set { currentCharacterPalette = value; OnCurrentCharacterPaletteChanged?.Invoke(value); } }
        public delegate void OnCurrentCharacterPaletteChangedEvent(CharacterPalette palette);
        public static event OnCurrentCharacterPaletteChangedEvent? OnCurrentCharacterPaletteChanged;

        private static List<CharacterPalette> characterPalettes = new();
        public static List<CharacterPalette> CharacterPalettes { get => characterPalettes; set => characterPalettes = value; }
        public delegate void OnAvailableCharacterPalettesChangedEvent(List<CharacterPalette> palette);
        public static event OnAvailableCharacterPalettesChangedEvent? OnAvailableCharacterPalettesChanged;

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
            Mutex mutex = new(false, ProgramTitle + "_" + Environment.UserName);
            if (!mutex.WaitOne(0, false)) //If another instance is already running, quit
            {
                System.Windows.MessageBox.Show("There is already an instance of AAP running!", "AAP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            App app = new();
            app.InitializeComponent();

            app.Exit += OnApplicationExit;
            app.DispatcherUnhandledException += (sender, e) => OnThreadException(sender, e);

#if RELEASE || WPF_RELEASE
            TextWriter oldOut = Console.Out;
            StreamWriter logSR = File.CreateText(ApplicationDataFolderPath + @"\log.txt");
            logSR.AutoFlush = true;

            logSR.WriteLine("--CONSOLE LOG--\n");

            Console.SetOut(logSR);
            Console.SetError(logSR);
#endif

            //Folders
            if (!Directory.Exists(DefaultArtFilesDirectoryPath))
            {
                DirectoryInfo defaultArtFilesDirInfo = Directory.CreateDirectory(DefaultArtFilesDirectoryPath);
                Console.WriteLine($"Created directory {defaultArtFilesDirInfo.FullName}");
            }

            if (!Directory.Exists(CharacterPaletteDirectoryPath))
            {
                DirectoryInfo characterPaletteDirInfo = Directory.CreateDirectory(CharacterPaletteDirectoryPath);
                Console.WriteLine($"Created directory {characterPaletteDirInfo.FullName}");
            }

            if (!Directory.Exists(AutoSaveDirectoryPath))
            {
                DirectoryInfo autoSaveDirInfo = Directory.CreateDirectory(AutoSaveDirectoryPath);
                Console.WriteLine($"Created directory {autoSaveDirInfo.FullName}");
            }

            //Tools
            Tools.Add(new DrawTool(ToolType.Draw, '|', 1));
            Tools.Add(new DrawTool(ToolType.Eraser, null, 1));
            Tools.Add(new SelectTool());
            Tools.Add(new MoveTool());
            Tools.Add(new BucketTool('|'));
            Tools.Add(new TextTool());

            CurrentToolType = ToolType.None;

            //Preset Character Palettes
            foreach (FileInfo presetFileInfo in new DirectoryInfo(@"Resources\PresetCharacterPalettes").GetFiles())
            {
                string presetCharacterPaletteFilePath = @$"{CharacterPaletteDirectoryPath}\{presetFileInfo.Name.Replace(".txt", ".aappal")}";

                CharacterPalette? characterPalette = CharacterPalette.ImportFilePath(presetFileInfo.FullName);

                if (characterPalette == null)
                    continue;

                FileInfo fileInfo = characterPalette.ExportTo(presetCharacterPaletteFilePath);

                Console.WriteLine($"Created Preset Character Palette File: {fileInfo.FullName}");
            }

            //Character Palettes
            foreach (FileInfo fileInfo in new DirectoryInfo(CharacterPaletteDirectoryPath).GetFiles())
            {
                CharacterPalette? characterPalette = CharacterPalette.ImportFilePath(fileInfo.FullName);

                if (characterPalette == null)
                    continue;

                characterPalettes.Add(characterPalette);

                Console.WriteLine($"Loaded Character Palette File: {fileInfo.FullName}");
            }


            OnAvailableCharacterPalettesChanged?.Invoke(CharacterPalettes);

            CurrentCharacterPalette = CharacterPalettes[0];

            OnCurrentArtChanged += OnCurrentArtFileChanged;

            Console.WriteLine("Set up complete!");

            Console.WriteLine("LOADING WINDOW HIDDEN");

            if (args.Length == 1)
                if (File.Exists(args[0]))
                {
                    FileInfo fileInfo = new(args[0]);
                    try
                    {
                        Exception? exception = null;

                        switch (fileInfo.Extension)
                        {
                            case ".aaf":
                                OpenArtFileAsync(fileInfo);
                                break;
                            case ".aappal":
                                exception = ImportCharacterPalette(fileInfo);
                                break;
                            case ".txt":
                                OpenArtFileAsync(fileInfo);
                                break;
                            default:
                                Console.WriteLine($"Open File On Start Up: Unknown extension: {fileInfo.Extension}!");
                                throw new Exception($"Unknown extension: {fileInfo.Extension}");
                        }

                        if (exception != null)
                            throw exception;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open {fileInfo.Name}. Exception : {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

            GC.Collect();

            app.Run();

#if RELEASE || WPF_RELEASE
            Console.SetOut(oldOut);
            Console.SetError(oldOut);

            logSR.Close();
#endif
            mutex.Close();
        }

        #region Application Events
        private static void OnApplicationExit(object? sender, ExitEventArgs args)
        {
            //Ask to save

            Console.WriteLine("\n--APPLICATION EXIT--\n");
            Console.Out.Close();
        }

        private static void OnThreadException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"\n--UNHANDLED EXCEPTION--\n\n{e.Exception}\n\n--END EXCEPTION--\n");

            MessageBoxResult result = MessageBox.Show($"It seems AAP has run into an unhandled exception, and must close! If this keeps occuring, please inform the creator of AAP! Exception: {e.Exception.Message}\nOpen full log?", App.ProgramTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
                Process.Start("explorer.exe", ApplicationDataFolderPath + @"\log.txt");

            Current.Shutdown(-1);
        }
        #endregion

        #region Files
        public static void NewFile(ASCIIArt? artFile)
        {
            Console.WriteLine("CurrentLayerID gets set to 0 instead of -1 when creating new files (REMOVE WHEN LAYER SELECTION IS FINISHED)");
            CurrentLayerID = 0; //-1; For testing
            CurrentArt = artFile;
            CurrentFilePath = null;
        }

        public static BackgroundWorker? CreateNewArtFileAsync(int width, int height)
        {
            BackgroundWorker bgWorker = new();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += CreateWork;
            bgWorker.RunWorkerCompleted += CreateWorkComplete;

            Console.WriteLine("Create Art: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void CreateWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                Console.WriteLine($"Create Art: creating art...");
                bgWorker.ReportProgress(33, new BackgroundTaskState("Creating art...", true));

                ASCIIArt art = new();
                art.SetSize(width, height);

                bgWorker.ReportProgress(66, new BackgroundTaskState("Creating background layer...", true));
                art.AddLayer(new("Background", width, height, 0, 0));

                bgWorker.ReportProgress(90, new BackgroundTaskState("Finishing up art...", true));
                art.UnsavedChanges = true;

                args.Result = art;
            }

            void CreateWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    Console.WriteLine("Create Art: ERROR: " + e.Error.ToString());
                else if (e.Cancelled)
                    Console.WriteLine("Create Art: Art file creation was cancelled");
                else
                {
                    if (e.Result is not ASCIIArt art)
                        throw new Exception("Create Art: BackgroundWorker Result is not of type ASCIIArt!");

                    Console.WriteLine("CurrentLayerID gets set to 0 instead of -1 when opening files (REMOVE WHEN LAYER SELECTION IS FINISHED)");
                    CurrentLayerID = 0; //-1; For testing
                    CurrentArt = art;
                    Console.WriteLine($"Create Art: created new art!");
                }
            }

            return bgWorker;
        }


        public static BackgroundWorker? OpenArtFileAsync(FileInfo file)
        {
            BackgroundWorker bgWorker = new();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += OpenWork;
            bgWorker.RunWorkerCompleted += OpenWorkComplete;

            Console.WriteLine("Open File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void OpenWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                Console.WriteLine($"Open File: importing file from path... {file.FullName}");

                bgWorker.ReportProgress(30, new BackgroundTaskState("Importing art...", true));

                if (!file.Exists)
                    throw new FileNotFoundException("Tried to open non-existant file", file.FullName);

                IAAPFile<ASCIIArt> AAPFile;
                ASCIIArt art = new();

                switch (file.Extension)
                {
                    case ".txt":
                        AAPFile = new TextASCIIArt(file.FullName);
                        art.UnsavedChanges = true;
                        break;
                    case ".aaf":
                        AAPFile = new AAFASCIIArt(file.FullName);
                        art.UnsavedChanges = false;
                        break;
                    default:
                        throw new Exception("Unknown file extension!");
                }

                Console.WriteLine($"Open File: Imported file!");
                AAPFile.Import(art, bgWorker);

                if (art.Width * art.Height > MaxArtArea)
                    throw new Exception($"Art Area is too large! Max: {MaxArtArea} characters ({art.Width * art.Height} characters)");

                Console.WriteLine($"\nOpen File: \nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nLayer Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.Width * art.Height * art.ArtLayers.Count}\nCreated In Version: {art.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

                bgWorker.ReportProgress(90, new BackgroundTaskState("Finishing up art...", true));
                args.Result = art;
            }

            void OpenWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    Console.WriteLine("Open File: ERROR: " + e.Error.ToString());
                else if (e.Cancelled)
                    Console.WriteLine("Open File: Art file open was cancelled");
                else
                {
                    if (e.Result is not ASCIIArt art)
                        throw new Exception("Open File: BackgroundWorker Result is not of type ASCIIArt!");

                    Console.WriteLine("CurrentLayerID gets set to 0 instead of -1 when opening files (REMOVE WHEN LAYER SELECTION IS FINISHED)");
                    CurrentLayerID = 0; //-1; For testing
                    CurrentArt = art;
                    CurrentFilePath = file.Extension == ".aaf" ? file.FullName : null;
                    Console.WriteLine($"Open File Path: opened file!");
                }
            }

            return bgWorker;
        }

        public static BackgroundWorker? SaveArtFileToPathAsync(string path)
        {
            if (CurrentArt == null)
                return null;

            if (path == null)
                return null;

            ASCIIArt art = CurrentArt;
            CurrentFilePath = path;

            BackgroundWorker bgWorker = new();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += SaveWork;
            bgWorker.RunWorkerCompleted += SaveWorkComplete;

            Console.WriteLine("Save File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void SaveWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                Console.WriteLine("Save File: Saving art file to " + path);
                bgWorker.ReportProgress(90, new BackgroundTaskState("Exporting as .aaf file...", true));

                AAFASCIIArt aafASCIIArt = new(path);
                aafASCIIArt.Export(art, bgWorker);

                args.Result = new FileInfo(path);
            }

            void SaveWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Console.WriteLine("Save File: ERROR: " + e.Error.ToString());
                    art.UnsavedChanges = true;
                }
                else if (e.Cancelled)
                {
                    Console.WriteLine("Save File: Art file save cancelled");
                    art.UnsavedChanges = true;
                }
                else
                {
                    if (e.Result is not FileInfo fileInfo)
                        Console.WriteLine("Save File: Art file save was successful, but result is not file info");
                    else
                        Console.WriteLine("Save File: Art file saved to " + fileInfo.FullName + "!");

                    art.UnsavedChanges = false;
                }
            }


            return bgWorker;
        }

        public static BackgroundWorker? ExportArtFileToPathAsync(string path)
        {
            if (CurrentArt == null)
                return null;

            if (path == null)
                return null;

            ASCIIArt art = CurrentArt;
            BackgroundWorker bgWorker = new();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += ExportWork;
            bgWorker.RunWorkerCompleted += ExportWorkComplete;

            Console.WriteLine("Export File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void ExportWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    throw new Exception("Sender is not a background worker!");

                Console.WriteLine("Export File: Exporting art file to " + path);

                FileInfo fileInfo = new(path);
                bgWorker.ReportProgress(90, new BackgroundTaskState($"Exporting as {fileInfo.Extension} file...", true));
                IAAPFile<ASCIIArt> AAPFile;

                switch (fileInfo.Extension)
                {
                    case ".txt":
                        AAPFile = new TextASCIIArt(fileInfo.FullName);
                        break;
                    case ".aaf":
                        AAPFile = new AAFASCIIArt(fileInfo.FullName);
                        break;
                    default:
                        throw new Exception("Unknown file extension!");
                }

                AAPFile.Export(art, bgWorker);

                args.Result = fileInfo;
            }

            void ExportWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    Console.WriteLine("Export File: ERROR: " + e.Error.ToString());
                else if (e.Cancelled)
                    Console.WriteLine("Export File: Art file export cancelled");
                else
                {
                    if (e.Result is not FileInfo fileInfo)
                        Console.WriteLine("Export File: Art file export was successful, but result is not file info");
                    else
                        Console.WriteLine("Export File: Art file exported to " + fileInfo.FullName + "!");
                }
            }

            return bgWorker;
        }

        public static void CopyArtFileToClipboard()
        {
            if (CurrentArt == null)
                return;

            Console.WriteLine("Copy Art To Clipboard: Copying art file to clipboard...");
            string artString = CurrentArt.GetArtString();

            Clipboard.SetText(artString);
            Console.WriteLine("Copy Art To Clipboard: Copied art file to clipboard!");
        }
        #endregion
        #region Art
        public static void OnCurrentArtFileChanged(ASCIIArt? art, ASCIIArtDraw? artDraw, ObjectTimeline? artTimeline)
        {
            Selected = Rect.Empty;
        }

        private static void OnCurrentArtArtLayerPropertiesChanged(int index, ArtLayer layer, bool updateCanvas)
            => CurrentArtTimeline?.NewTimePoint();

        private static void OnToolActivateEnd(Tool tool, Point position)
        {
            if (tool.Type == ToolType.Draw || tool.Type == ToolType.Eraser || tool.Type == ToolType.Move)
                currentArtTimeline?.NewTimePoint();
        }

        public static void CropArtFileToSelected()
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rect.Empty)
                return;

            CurrentArt.Crop(Selected);

            Selected = new(0, 0, Selected.Width, Selected.Height);

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void CropCurrentArtLayerToSelected()
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rect.Empty)
                return;

            if (CurrentLayerID == -1)
                return;

            CurrentArt.ArtLayers[CurrentLayerID].Crop(Selected);

            CurrentArtTimeline?.NewTimePoint();

            CurrentArt.Update();
        }

        public static void FillSelectedWith(char? character)
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rect.Empty)
                return;

            CurrentArtDraw?.DrawRectangle(CurrentLayerID, character, Selected);

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void SelectArt()
        {
            if (CurrentArt == null)
            {
                Selected = Rect.Empty;
                return;
            }

            Selected = new(0, 0, CurrentArt.Width, CurrentArt.Height);
        }

        public static void SelectLayer()
        {
            if (CurrentArt == null)
            {
                Selected = Rect.Empty;
                return;
            }

            if (CurrentLayerID == -1)
            {
                Selected = Rect.Empty;
                return;
            }

            ArtLayer layer = CurrentArt.ArtLayers[CurrentLayerID];
            Selected = new(layer.Offset, layer.Size);
        }

        public static void CancelSelection()
            => Selected = Rect.Empty;

        #endregion
        #region Character Palettes
        public static Exception? ImportCharacterPalette(FileInfo file)
        {
            if (!file.Exists)
                return new FileNotFoundException("Failed to open non-existant file", file.FullName);

            try
            {
                Console.WriteLine($"Import Character Palette: importing character palette from path... {file.FullName}");

                CharacterPalette? palette = CharacterPalette.ImportFilePath(file.FullName);

                if (palette == null)
                {
                    Console.WriteLine("Import Character Palette: current character palette file is null!");
                    throw new NullReferenceException("Current character palette file is null!");
                }

                if (palette.Characters.Length > MaxCharacterPaletteCharacters)
                {
                    Console.WriteLine($"Import Character Palette: More than or {MaxCharacterPaletteCharacters} characters!) ({palette.Characters.Length} characters)");
                    throw new Exception($"Character Palette is too large! Max: {MaxCharacterPaletteCharacters} characters ({palette.Characters.Length} characters)");
                }

                if (CharacterPalettes.Contains(palette))
                    throw new Exception($"Character Palette already exists! {palette.Name}");

                string characterPaletteFolderFilePath = @$"{CharacterPaletteDirectoryPath}\{file.Name.Replace(file.Extension, ".aappal")}";

                if (File.Exists(characterPaletteFolderFilePath))
                    throw new Exception($"Character Palette File with name: {file.Name.Replace(file.Extension, ".aappal")} already exists in the character palettes folder!");

                palette.ExportTo(characterPaletteFolderFilePath);

                CharacterPalettes.Add(palette);

                Console.WriteLine($"Import Character Palette: Imported character palette!");
                Console.WriteLine($"\nFILE INFO\nFile Path: {file.FullName}\nTotal Characters: {palette.Characters.Length}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import Character Palette: An error has occurred while importing character palette ({file.FullName})! Exception: {ex}");
                return ex;
            }

            OnAvailableCharacterPalettesChanged?.Invoke(CharacterPalettes);

            return null;
        }

        public static void SelectCharacterTool(char? character)
        {
            if (CurrentTool == null)
                return;

            switch(CurrentTool.Type)
            {
                case ToolType.Draw:
                    if (CurrentTool is not DrawTool drawTool)
                        return;

                    drawTool.Character = character;

                    break;
                case ToolType.Bucket:
                    if (CurrentTool is not BucketTool bucketTool)
                        return;

                    bucketTool.Character = character;

                    break;
                default:
                    Console.WriteLine("Current Tool cannot select characters!");
                    break;
            }

            Console.WriteLine("Selected Character: " + character.ToString());
        }
        #endregion
        #region Layers

        public static void AddArtLayer()
        {
            if (CurrentArt == null)
                return;

            CurrentArt.InsertLayer(CurrentLayerID + 1, new("Layer", CurrentArt.Width, CurrentArt.Height));
            CurrentLayerID += 1;

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void DuplicateCurrentArtLayer()
        {
            if (CurrentArt == null)
                return;

            if (CurrentArt.ArtLayers.Count == 0)
                return;

            ArtLayer currentArtLayer = CurrentArt.ArtLayers[CurrentLayerID];
            ArtLayer duplicateArtLayer = new(currentArtLayer.Name + " copy", CurrentArt.Width, CurrentArt.Height)
            {
                Visible = currentArtLayer.Visible
            };

            for (int x = 0; x < CurrentArt.Width; x++)
                for (int y = 0; y < CurrentArt.Height; y++)
                    duplicateArtLayer.Data[x][y] = currentArtLayer.Data[x][y];

            CurrentArt.InsertLayer(CurrentLayerID + 1, duplicateArtLayer);
            CurrentLayerID += 1;

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void RemoveCurrentArtLayer()
        {
            if (CurrentArt == null)
                return;

            if (CurrentArt.ArtLayers.Count == 0)
                return;

            CurrentLayerID -= 1;
            CurrentArt.RemoveLayer(CurrentLayerID + 1);

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void SetArtLayerName(ArtLayer layer, string name)
        {
            layer.Name = name;

            CurrentArtTimeline?.NewTimePoint();
        }

        public static void SetArtLayerVisibility(ArtLayer layer, bool visible)
        {
            layer.Visible = visible;

            CurrentArtTimeline?.NewTimePoint();
        }

        #endregion
    }
}
