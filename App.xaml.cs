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
        public static readonly DirectoryInfo? ExecutableDirectory = Environment.ProcessPath != null ? Directory.GetParent(Environment.ProcessPath) : null;
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

        private static int currentLayerID = -1;
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

                ConsoleLogger.Log("Selected ToolType: " + value.ToString());
                ConsoleLogger.Log("Selected Tool: " + currentTool?.ToString());

            }
        }
        public delegate void CurrentToolChangedEvent(Tool? tool);
        public static event CurrentToolChangedEvent? OnCurrentToolChanged;

        private static CharacterPalette currentCharacterPalette = new();
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

            if (ExecutableDirectory == null)
                throw new NullReferenceException(nameof(ExecutableDirectory));

            if (!ExecutableDirectory.Exists)
                throw new Exception(nameof(ExecutableDirectory) + " doesn't exist!");

            App app = new();
            app.InitializeComponent();

            app.Exit += OnApplicationExit;
            app.DispatcherUnhandledException += (sender, e) => OnThreadException(sender, e);
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;

#if RELEASE
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

            //Tools
            Tools.Add(new DrawTool(ToolType.Draw, '|', 1));
            Tools.Add(new DrawTool(ToolType.Eraser, null, 1));
            Tools.Add(new SelectTool());
            Tools.Add(new MoveTool());
            Tools.Add(new BucketTool('|'));
            Tools.Add(new TextTool());

            CurrentToolType = ToolType.None;

            //Preset Character Palettes
            foreach (FileInfo presetFileInfo in new DirectoryInfo($@"{ExecutableDirectory.FullName}\Resources\PresetCharacterPalettes").GetFiles())
            {
                string presetCharacterPaletteFilePath = @$"{CharacterPaletteDirectoryPath}\{presetFileInfo.Name.Replace(".txt", ".aappal")}";

                CharacterPalette palette = new();
                TXTCharacterPalette presetTxTCharacterPalette = new(palette, presetFileInfo.FullName);
                presetTxTCharacterPalette.Import();

                AAPPALCharacterPalette aappalCharacterPalette = new(palette, presetCharacterPaletteFilePath);
                aappalCharacterPalette.Export();

                FileInfo fileInfo = new(presetCharacterPaletteFilePath);

                ConsoleLogger.Log($"Created Preset Character Palette File: {fileInfo.FullName}");
            }

            //Character Palettes
            foreach (FileInfo fileInfo in new DirectoryInfo(CharacterPaletteDirectoryPath).GetFiles())
            {
                CharacterPalette palette = new();

                IAAPFile<CharacterPalette> fileCharacterPalette;
                switch(fileInfo.Extension.ToLower())
                {
                    case ".aappal":
                        fileCharacterPalette = new AAPPALCharacterPalette(palette, fileInfo.FullName);
                        break;
                    case ".txt":
                        fileCharacterPalette = new TXTCharacterPalette(palette, fileInfo.FullName);
                        break;
                    default:
                        ConsoleLogger.Warn("Invalid character palette extension " + fileInfo.Extension);
                        continue;
                }

                fileCharacterPalette.Import();

                characterPalettes.Add(palette);

                ConsoleLogger.Log($"Loaded Character Palette File: {fileInfo.FullName}");
            }

            OnAvailableCharacterPalettesChanged?.Invoke(CharacterPalettes);

            CurrentCharacterPalette = CharacterPalettes[0];

            OnCurrentArtChanged += OnCurrentArtFileChanged;

            ConsoleLogger.Log("Set up complete!");

            ConsoleLogger.Log("LOADING WINDOW HIDDEN");

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
                            case ".txt":
                                OpenArtFileAsync(fileInfo);
                                break;
                            default:
                                ConsoleLogger.Log($"Open File On Start Up: Unknown extension: {fileInfo.Extension}!");
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
        public static void NewFile(ASCIIArt? artFile)
        {
            CurrentArt = artFile;
            CurrentLayerID = -1;
            CurrentFilePath = null;
        }

        public static BackgroundWorker? CreateNewArtFileAsync(int width, int height)
        {
            BackgroundWorker bgWorker = new();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += CreateWork;
            bgWorker.RunWorkerCompleted += CreateWorkComplete;

            ConsoleLogger.Log("Create Art: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void CreateWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                ConsoleLogger.Log($"Create Art: creating art...");
                bgWorker.ReportProgress(33, new BackgroundTaskState("Creating art...", true));

                ASCIIArt art = new();
                art.SetSize(width, height);

                bgWorker.ReportProgress(66, new BackgroundTaskState("Creating background layer...", true));
                art.ArtLayers.Add(new("Background", width, height, 0, 0));

                bgWorker.ReportProgress(90, new BackgroundTaskState("Finishing up art...", true));
                art.UnsavedChanges = true;

                args.Result = art;
            }

            void CreateWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    ConsoleLogger.Error("Create Art: ", e.Error);
                else if (e.Cancelled)
                    ConsoleLogger.Inform("Create Art: Art file creation was cancelled");
                else
                {
                    if (e.Result is not ASCIIArt art)
                        throw new Exception("Create Art: BackgroundWorker Result is not of type ASCIIArt!");

                    CurrentArt = art;
                    CurrentLayerID = -1;
                    ConsoleLogger.Log($"Create Art: created new art!");
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

            ConsoleLogger.Log("Open File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void OpenWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                ConsoleLogger.Log($"Open File: importing file from path... {file.FullName}");

                bgWorker.ReportProgress(30, new BackgroundTaskState("Importing art...", true));

                if (!file.Exists)
                    throw new FileNotFoundException("Tried to open non-existant file", file.FullName);

                IAAPFile<ASCIIArt> AAPFile;
                ASCIIArt art = new();

                switch (file.Extension.ToLower())
                {
                    case ".txt":
                        AAPFile = new TXTASCIIArt(art, file.FullName);
                        art.UnsavedChanges = true;
                        break;
                    case ".aaf":
                        AAPFile = new AAFASCIIArt(art, file.FullName);
                        art.UnsavedChanges = false;
                        break;
                    default:
                        throw new Exception("Unknown file extension!");
                }

                ConsoleLogger.Log($"Open File: Imported file!");
                AAPFile.Import(bgWorker);

                if (art.Width * art.Height > MaxArtArea)
                    throw new Exception($"Art Area is too large! Max: {MaxArtArea} characters ({art.Width * art.Height} characters)");

                ConsoleLogger.Inform($"\nOpen File: \nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nLayer Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.Width * art.Height * art.ArtLayers.Count}\nCreated In Version: {art.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

                bgWorker.ReportProgress(90, new BackgroundTaskState("Finishing up art...", true));
                args.Result = art;
            }

            void OpenWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    ConsoleLogger.Error("Open File: ", e.Error);
                else if (e.Cancelled)
                    ConsoleLogger.Inform("Open File: Art file open was cancelled");
                else
                {
                    if (e.Result is not ASCIIArt art)
                        throw new Exception("Open File: BackgroundWorker Result is not of type ASCIIArt!");
                    
                    CurrentArt = art;
                    CurrentLayerID = -1;
                    CurrentFilePath = file.Extension == ".aaf" ? file.FullName : null;
                    ConsoleLogger.Log($"Open File Path: opened file!");
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

            ConsoleLogger.Log("Save File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void SaveWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                ConsoleLogger.Log("Save File: Saving art file to " + path);
                bgWorker.ReportProgress(90, new BackgroundTaskState("Exporting as .aaf file...", true));

                AAFASCIIArt aafASCIIArt = new(art, path);
                aafASCIIArt.Export(bgWorker);

                args.Result = new FileInfo(path);
            }

            void SaveWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    ConsoleLogger.Error("Save File: ", e.Error);
                    art.UnsavedChanges = true;
                }
                else if (e.Cancelled)
                {
                    ConsoleLogger.Inform("Save File: Art file save cancelled");
                    art.UnsavedChanges = true;
                }
                else
                {
                    if (e.Result is not FileInfo fileInfo)
                        ConsoleLogger.Warn("Save File: Art file save was successful, but result is not file info");
                    else
                        ConsoleLogger.Log("Save File: Art file saved to " + fileInfo.FullName + "!");

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

            ConsoleLogger.Log("Export File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void ExportWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    throw new Exception("Sender is not a background worker!");

                ConsoleLogger.Log("Export File: Exporting art file to " + path);

                FileInfo fileInfo = new(path);
                bgWorker.ReportProgress(90, new BackgroundTaskState($"Exporting as {fileInfo.Extension} file...", true));
                IAAPFile<ASCIIArt> AAPFile;

                switch (fileInfo.Extension)
                {
                    case ".txt":
                        AAPFile = new TXTASCIIArt(art, fileInfo.FullName);
                        break;
                    case ".aaf":
                        AAPFile = new AAFASCIIArt(art, fileInfo.FullName);
                        break;
                    default:
                        throw new Exception("Unknown file extension!");
                }

                AAPFile.Export(bgWorker);

                args.Result = fileInfo;
            }

            void ExportWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    ConsoleLogger.Error("Export File: ", e.Error);
                else if (e.Cancelled)
                    ConsoleLogger.Inform("Export File: Art file export cancelled");
                else
                {
                    if (e.Result is not FileInfo fileInfo)
                        ConsoleLogger.Warn("Export File: Art file export was successful, but result is not file info");
                    else
                        ConsoleLogger.Log("Export File: Art file exported to " + fileInfo.FullName + "!");
                }
            }

            return bgWorker;
        }

        public static void CopyArtFileToClipboard()
        {
            if (CurrentArt == null)
                return;

            ConsoleLogger.Log("Copy Art To Clipboard: Copying art file to clipboard...");
            string artString = CurrentArt.GetArtString();

            Clipboard.SetText(artString);
            ConsoleLogger.Log("Copy Art To Clipboard: Copied art file to clipboard!");
        }
        #endregion
        #region Art
        public static void OnCurrentArtFileChanged(ASCIIArt? art, ASCIIArtDraw? artDraw, ObjectTimeline? artTimeline)
        {
            Selected = Rect.Empty;
        }

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
                    ConsoleLogger.Warn("Current Tool cannot select characters!");
                    break;
            }

            ConsoleLogger.Log("Selected Character: " + character.ToString());
        }
        #endregion
        #region Layers

        public static void AddArtLayer()
        {
            if (CurrentArt == null)
                return;

            CurrentArt.ArtLayers.Insert(CurrentLayerID + 1, new("Layer", CurrentArt.Width, CurrentArt.Height));
            CurrentLayerID += 1;

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
            ArtLayer duplicateArtLayer = new(currentArtLayer.Name + " copy", CurrentArt.Width, CurrentArt.Height)
            {
                Visible = currentArtLayer.Visible
            };

            for (int x = 0; x < CurrentArt.Width; x++)
                for (int y = 0; y < CurrentArt.Height; y++)
                    duplicateArtLayer.Data[x][y] = currentArtLayer.Data[x][y];

            CurrentArt.ArtLayers.Insert(CurrentLayerID + 1, duplicateArtLayer);
            CurrentLayerID += 1;

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

            CurrentLayerID -= 1;
            CurrentArt.ArtLayers.RemoveAt(CurrentLayerID + 1);

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
    }
}
