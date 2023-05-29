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

        private static MainForm mainForm = new();

        public static readonly string DefaultArtFilesDirectoryPath = $@"{Application.UserAppDataPath}\Saves";
        public static readonly string AutoSaveDirectoryPath = $@"{Application.UserAppDataPath}\Autosaves";

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
                return;

            Console.Title = ProgramTitle + " Console";

            AAPLoadingForm loadingForm = new();
            loadingForm.Show();

            loadingForm.UpdateInfo(0, "Setting up...");

            if (!Directory.Exists(DefaultArtFilesDirectoryPath))
            {
                DirectoryInfo defaultArtFilesDirInfo = Directory.CreateDirectory(DefaultArtFilesDirectoryPath);
                Console.WriteLine($"Created directory {defaultArtFilesDirInfo.FullName}");
            }

            if (!Directory.Exists(AutoSaveDirectoryPath))
            {
                DirectoryInfo autoSaveDirInfo = Directory.CreateDirectory(AutoSaveDirectoryPath);
                Console.WriteLine($"Created directory {autoSaveDirInfo.FullName}");
            }

            Tools.Add(ToolType.Draw, new DrawTool('|', 1));
            Tools.Add(ToolType.Eraser, new DrawTool(ASCIIArt.EMPTYCHARACTER, 1));
            Tools.Add(ToolType.Select, new SelectTool());
            Tools.Add(ToolType.Move, new MoveTool(MoveToolMode.Select));
            Tools.Add(ToolType.Text, new TextTool(8));

            CurrentToolType = ToolType.Draw;

            OnCurrentArtChanged += (art) => Selected = Rectangle.Empty; //Set selection to nothing if art file changes

            Console.WriteLine("Set up complete!");

            loadingForm.Hide();

            if (args.Length == 1)
                if (File.Exists(args[0]))
                    OpenFile(new(args[0]));

            Application.Run(mainForm);

            mutex.Close();
        }

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

        public static void CropArtFileToSelected()
        {
            if (CurrentArt == null)
                return;

            if (Selected == Rectangle.Empty)
                return;

            CurrentArt = CurrentArt.Crop(Selected);

            Selected = Rectangle.Empty;
        }
    }
}