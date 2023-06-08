using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace AAP
{
    public static class MainProgram
    {
        public static readonly string ProgramTitle = "ASCII Art Program";
        public static readonly string Version = "v0.0.1";

        public static readonly int MaxArtArea = 90000;
        public readonly static int MaxCharacterPaletteCharacters = 200;

        private static MainForm mainForm = new();

        private static readonly string ApplicationDataFolderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{Application.CompanyName}\{Application.ProductName}";

        public static readonly string DefaultArtFilesDirectoryPath = $@"{ApplicationDataFolderPath}\Saves";
        public static readonly string CharacterPaletteDirectoryPath = $@"{ApplicationDataFolderPath}\CharacterPalettes";
        public static readonly string AutoSaveDirectoryPath = $@"{ApplicationDataFolderPath}\Autosaves";

        private static ASCIIArt? currentArt;
        public static ASCIIArt? CurrentArt { get => currentArt; set { currentArt = value; OnCurrentArtChanged?.Invoke(value); } }
        public delegate void CurrentArtChangedEvent(ASCIIArt? art);
        public static event CurrentArtChangedEvent? OnCurrentArtChanged;

        private static string? currentFilePath;
        public static string? CurrentFilePath { get => currentFilePath; set { currentFilePath = value; OnCurrentFilePathChanged?.Invoke(value); } }
        public delegate void CurrentFilePathChangedEvent(string? filePath);
        public static event CurrentFilePathChangedEvent? OnCurrentFilePathChanged;

        private static Rectangle selected = Rectangle.Empty;
        public static Rectangle Selected { get => selected; set { selected = value; OnSelectionChanged?.Invoke(value); } }
        public delegate void OnSelectionChangedEvent(Rectangle selection);
        public static event OnSelectionChangedEvent? OnSelectionChanged;

        private static int currentLayerID = 0;
        public static int CurrentLayerID { get => currentLayerID; set => currentLayerID = value; }

        private static Dictionary<ToolType, Tool> tools = new();
        public static Dictionary<ToolType, Tool> Tools { get => tools; set => tools = value; }

        private static Tool currentTool = new DrawTool('|', 1);
        public static Tool CurrentTool { get => currentTool; }

        private static ToolType currentToolType = ToolType.Draw;
        public static ToolType CurrentToolType { 
            get => currentToolType; 
            set 
            {
                Tools[currentToolType] = currentTool;

                currentToolType = value;

                currentTool = Tools[value];

                Console.WriteLine("Selected ToolType: " + value.ToString());
                Console.WriteLine("Selected Tool: " + currentTool.ToString());

                OnCurrentToolTypeChanged?.Invoke(value);
            } 
        }
        public delegate void CurrentToolTypeChangedEvent(ToolType type);
        public static event CurrentToolTypeChangedEvent? OnCurrentToolTypeChanged;

        private static CharacterPalette currentCharacterPalette = CharacterPalette.ImportFilePath(@"PresetCharacterPalettes\Main ASCII Characters.txt") ?? new("Unknown", new List<char>());
        public static CharacterPalette CurrentCharacterPalette { get => currentCharacterPalette; set { currentCharacterPalette = value; OnCurrentCharacterPaletteChanged?.Invoke(value); } }
        public delegate void OnCurrentCharacterPaletteChangedEvent(CharacterPalette palette);
        public static event OnCurrentCharacterPaletteChangedEvent? OnCurrentCharacterPaletteChanged;

        private static List<CharacterPalette> characterPalettes = new();
        public static List<CharacterPalette> CharacterPalettes { get => characterPalettes; set => characterPalettes = value; }
        public delegate void OnAvailableCharacterPalettesChangedEvent(List<CharacterPalette> palette);
        public static event OnAvailableCharacterPalettesChangedEvent? OnAvailableCharacterPalettesChanged;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Mutex mutex = new(false, ProgramTitle);
            if (!mutex.WaitOne(0, false)) //If another instance is already running, quit
            {
                MessageBox.Show("There is already an instance of AAP running!", "AAP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AAPLoadingForm loadingForm = new();
            loadingForm.Show();

            loadingForm.UpdateInfo(0, "Setting up...");

            Application.ApplicationExit += OnApplicationExit;
            Application.ThreadException += OnThreadException;

            TextWriter oldOut = Console.Out;
            StreamWriter logSR = File.CreateText(ApplicationDataFolderPath + @"\log.txt");
            logSR.AutoFlush = true;

            logSR.WriteLine("--CONSOLE LOG--\n");

            Console.SetOut(logSR);
            Console.SetError(logSR);

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
            Tools.Add(ToolType.Draw, new DrawTool('|', 1));
            Tools.Add(ToolType.Eraser, new DrawTool(ASCIIArt.EMPTYCHARACTER, 1));
            Tools.Add(ToolType.Select, new SelectTool());
            Tools.Add(ToolType.Move, new MoveTool(MoveToolMode.Select));
            Tools.Add(ToolType.Text, new TextTool(8));

            CurrentToolType = ToolType.Draw;

            //Preset Character Palettes
            foreach (FileInfo presetFileInfo in new DirectoryInfo(@"PresetCharacterPalettes").GetFiles())
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

            OnCurrentArtChanged += (art) => Selected = Rectangle.Empty; //Set selection to nothing if art file changes

            Console.WriteLine("Set up complete!");

            loadingForm.Hide();

            if (args.Length == 1)
                if (File.Exists(args[0]))
                {
                    FileInfo fileInfo = new(args[0]);
                    switch(fileInfo.Extension)
                    {
                        case ".aaf":
                            OpenFile(fileInfo);
                            break;
                        case ".aappal":
                            ImportCharacterPalette(fileInfo);
                            break;
                        case ".txt":
                            OpenFile(fileInfo);
                            break;
                        default:
                            MessageBox.Show($"{fileInfo.Extension} can not be opened with AAP.", "Open File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Console.WriteLine($"Open File On Start Up: Unknown extension: {fileInfo.Extension}!");
                            break;
                    }
                }

            GC.Collect();

            Application.Run(mainForm);

            Console.SetOut(oldOut);
            Console.SetError(oldOut);

            logSR.Close();
            mutex.Close();
        }

        #region Application Events
        private static void OnApplicationExit(object? sender, EventArgs args)
        {
            //Ask to save

            Console.WriteLine("\n--APPLICATION EXIT--\n");
        }

        private static void OnThreadException(object? sender, ThreadExceptionEventArgs args)
        {
            Console.WriteLine($"\n--UNHANDLED EXCEPTION--\n\n{args.Exception}\n--END EXCEPTION--\n");

            DialogResult result = MessageBox.Show($"It seems AAP has run into an unhandled exception, and must close! If this keeps occuring, please inform the creator of AAP! Exception: {args.Exception.Message}\nOpen full log?", MainProgram.ProgramTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
                Process.Start("explorer.exe", ApplicationDataFolderPath + @"\log.txt");

            Application.Exit();
        }
        #endregion
        #region Files
        public static void NewFile(ASCIIArt artFile)
        {
            CurrentArt = artFile;
            CurrentFilePath = "";
        }

        public static Exception? OpenFile(FileInfo file)
        {
            if (!file.Exists)
                return new FileNotFoundException("Failed to open non-existant file", file.FullName);

            try
            {
                Console.WriteLine($"Open File Path: importing file from path... {file.FullName}");

                ASCIIArt? art = ASCIIArt.ImportFilePath(file.FullName);

                if (art == null)
                {
                    Console.WriteLine("Open File Path: current art file is null!");
                    throw new NullReferenceException("Current art file is null!");
                }

                if (art.Width * art.Height > MaxArtArea)
                {
                    Console.WriteLine($"Open File Path: File too large! (>{MaxArtArea} characters) ({art.Width * art.Height} characters)");
                    throw new Exception($"Art Area is too large! Max: {MaxArtArea} characters ({art.Width * art.Height} characters)");
                }

                Console.WriteLine($"Open File Path: Imported file!");
                Console.WriteLine($"\nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nArea: {art.Width*art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nCreated In Version: {art.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

                CurrentArt = art;
                CurrentFilePath = file.Extension == ".aaf" ? file.FullName : "";
                Console.WriteLine($"Open File Path: opened file!");
            }
            catch(Exception ex)
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
            bgWorker.WorkerReportsProgress = false;
            bgWorker.DoWork += SaveWork;

            bgWorker.RunWorkerAsync();

            void SaveWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                if (CurrentArt == null)
                    return;

                Console.WriteLine("Save File: Saving art file to " + path);

                FileInfo fileInfo = CurrentArt.WriteTo(path);

                Console.WriteLine("Save File: Art file saved to " + path + "!");

                args.Result = fileInfo;
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
            bgWorker.WorkerReportsProgress = false;
            bgWorker.DoWork += ExportWork;

            bgWorker.RunWorkerAsync();

            void ExportWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    throw new Exception("Sender is not a background worker!");

                if (CurrentArt == null)
                    throw new Exception("Current Art File is null!");

                Console.WriteLine("Export File: Exporting art file to " + path);

                FileInfo fileInfo = CurrentArt.ExportTo(path, bgWorker);

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

            Clipboard.SetText(artString);
            Console.WriteLine("Copy Art To Clipboard: Copied art file to clipboard!");
        }
        #endregion
        #region Art
        public static void CropArtFileToSelected()
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rectangle.Empty)
                return;

            CurrentArt = CurrentArt.Crop(Selected);

            Selected = Rectangle.Empty;
        }

        public static void FillSelectedArtWith(char character)
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rectangle.Empty)
                return;

            for (int x = Selected.X; x < Selected.X + Selected.Width; x++)
                for (int y = Selected.Y; y < Selected.Y + Selected.Height; y++)
                    CurrentArt.Draw(CurrentLayerID, new(x, y), character);
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
    }
}