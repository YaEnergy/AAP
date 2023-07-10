﻿using System;
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
using System.Windows.Threading;

namespace AAP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static readonly string ProgramTitle = "ASCII Art Program";
        public static readonly string Version = "v0.0.1";

        public static readonly int MaxArtArea = 100000000;
        public static readonly int WarningIncrediblyLargeArtArea = 1000000;
        public static readonly int WarningLargeArtArea = 500000;
        public readonly static int MaxCharacterPaletteCharacters = 200;

        public static readonly string ApplicationDataFolderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\AAP\AAP";

        public static readonly string DefaultArtFilesDirectoryPath = $@"{ApplicationDataFolderPath}\Saves";
        public static readonly string CharacterPaletteDirectoryPath = $@"{ApplicationDataFolderPath}\CharacterPalettes";
        public static readonly string AutoSaveDirectoryPath = $@"{ApplicationDataFolderPath}\Autosaves";

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
                OnCurrentArtChanged?.Invoke(currentArt, currentArtDraw);
            }
        }
        public delegate void CurrentArtChangedEvent(ASCIIArt? art, ASCIIArtDraw? artDraw);
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
                currentToolType = value;

                if (value != ToolType.None)
                {
                    foreach (Tool tool in Tools)
                        if (tool.Type == currentToolType)
                            currentTool = tool;
                }
                else
                    currentTool = null;
                
                
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
            Mutex mutex = new(false, ProgramTitle);
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
            Tools.Add(new MoveTool(MoveToolMode.Select));
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

            OnCurrentArtChanged += (art, artDraw) => Selected = Rect.Empty; //Set selection to nothing if art file changes

            Console.WriteLine("Set up complete!");

            Console.WriteLine("LOADING WINDOW HIDDEN");

            if (args.Length == 1)
                if (File.Exists(args[0]))
                {
                    FileInfo fileInfo = new(args[0]);
                    try
                    {
                        Exception? exception;

                        switch (fileInfo.Extension)
                        {
                            case ".aaf":
                                exception = OpenFile(fileInfo);
                                break;
                            case ".aappal":
                                exception = ImportCharacterPalette(fileInfo);
                                break;
                            case ".txt":
                                exception = OpenFile(fileInfo);
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
                        System.Windows.MessageBox.Show($"Failed to open {fileInfo.Name}. Exception : {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            MessageBoxResult result = System.Windows.MessageBox.Show($"It seems AAP has run into an unhandled exception, and must close! If this keeps occuring, please inform the creator of AAP! Exception: {e.Exception.Message}\nOpen full log?", MainProgram.ProgramTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);

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

        public static Exception? OpenFile(FileInfo file)
        {
            if (!file.Exists)
                return new FileNotFoundException("Failed to open non-existant file", file.FullName);

            try
            {
                Console.WriteLine($"Open File Path: importing file from path... {file.FullName}");

                IAAPFile<ASCIIArt> AAPFile;
                ASCIIArt art = new();

                switch (file.Extension)
                {
                    case ".txt":
                        AAPFile = new TextASCIIArt(file.FullName);
                        break;
                    case ".aaf":
                        AAPFile = new AAFASCIIArt(file.FullName);
                        break;
                    default:
                        throw new Exception("Unknown file extension!");
                }

                AAPFile.Import(art);

                if (art.Width * art.Height > MaxArtArea)
                {
                    Console.WriteLine($"Open File Path: File too large! (>{MaxArtArea} characters) ({art.Width * art.Height} characters)");
                    throw new Exception($"Art Area is too large! Max: {MaxArtArea} characters ({art.Width * art.Height} characters)");
                }

                Console.WriteLine($"Open File Path: Imported file!");
                Console.WriteLine($"\nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nLayer Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.Width * art.Height * art.ArtLayers.Count}\nCreated In Version: {art.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

                Console.WriteLine("CurrentLayerID gets set to 0 instead of -1 when opening files (REMOVE WHEN LAYER SELECTION IS FINISHED)");
                CurrentLayerID = 0; //-1; For testing
                CurrentArt = art;
                CurrentFilePath = file.Extension == ".aaf" ? file.FullName : null;
                Console.WriteLine($"Open File Path: opened file!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Open File Path: An error has occurred while importing art file ({file.FullName})! Exception: {ex}");
                return ex;
            }

            return null;
        }

        public static BackgroundWorker? SaveArtFileToPathAsync(string path)
        {
            if (CurrentArt == null)
                return null;

            if (path == null)
                return null;

            CurrentFilePath = path;

            BackgroundWorker bgWorker = new();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += SaveWork;

            bgWorker.RunWorkerAsync();

            void SaveWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                if (CurrentArt == null)
                    return;

                Console.WriteLine("Save File: Saving art file to " + path);

                AAFASCIIArt aafASCIIArt = new(path);
                aafASCIIArt.Export(CurrentArt, bgWorker);

                Console.WriteLine("Save File: Art file saved to " + path + "!");

                args.Result = new FileInfo(path);
            }

            return bgWorker;
        }

        public static BackgroundWorker? ExportArtFileToPathAsync(string path)
        {
            if (CurrentArt == null)
                return null;

            if (path == null)
                return null;

            BackgroundWorker bgWorker = new();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += ExportWork;

            bgWorker.RunWorkerAsync();

            void ExportWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    throw new Exception("Sender is not a background worker!");

                if (CurrentArt == null)
                    throw new Exception("Current Art File is null!");

                Console.WriteLine("Export File: Exporting art file to " + path);

                FileInfo fileInfo = new(path);
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

                AAPFile.Export(CurrentArt, bgWorker);

                Console.WriteLine("Export File: Art file exported to " + path + "!");

                args.Result = fileInfo;
            }

            return bgWorker;
        }

        public static void CopyArtFileToClipboard()
        {
            if (CurrentArt == null)
                return;

            Console.WriteLine("Copy Art To Clipboard: Copying art file to clipboard...");
            string artString = CurrentArt.GetArtString();

            System.Windows.Clipboard.SetText(artString);
            Console.WriteLine("Copy Art To Clipboard: Copied art file to clipboard!");
        }
        #endregion
        #region Art
        public static void CropArtFileToSelected()
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rect.Empty)
                return;

            Console.WriteLine("CropArtFileToSelected converts Rect to Rectangle! Update needed");
            CurrentArt.Crop(new((int)Selected.X, (int)Selected.Y, (int)Selected.Width, (int)Selected.Height));

            Selected = Rect.Empty;
        }

        public static void FillSelectedArtWith(char? character)
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rect.Empty)
                return;

            Console.WriteLine("FillSelectedArtWith converts Rect to Rectangle! Update needed");
            CurrentArtDraw?.DrawRectangle(CurrentLayerID, character, new((int)Selected.X, (int)Selected.Y, (int)Selected.Width, (int)Selected.Height));

            /*for (int x = Selected.X; x < Selected.X + Selected.Width; x++)
                for (int y = Selected.Y; y < Selected.Y + Selected.Height; y++)
                    CurrentArt.Draw(CurrentLayerID, new(x, y), character);*/
        }
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
        #endregion
        #region Layers

        public static void AddArtLayer()
        {
            if (CurrentArt == null)
                return;

            CurrentArt.InsertLayer(CurrentLayerID + 1, new("Layer", CurrentArt.Width, CurrentArt.Height));
            CurrentLayerID += 1;
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
        }

        public static void RemoveCurrentArtLayer()
        {
            if (CurrentArt == null)
                return;

            if (CurrentArt.ArtLayers.Count == 0)
                return;

            CurrentLayerID -= 1;
            CurrentArt.RemoveLayer(CurrentLayerID + 1);
        }

        public static void SetCurrentArtLayerName(string name)
        {
            if (CurrentArt == null) return;

            if (CurrentArt.ArtLayers.Count == 0)
                return;

            CurrentArt.ArtLayers[CurrentLayerID].Name = name;
        }

        #endregion
    }
}