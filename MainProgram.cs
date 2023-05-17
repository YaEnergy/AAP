using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using Microsoft.Win32;

namespace AAP
{
    public static class MainProgram
    {
        public static readonly string ProgramTitle = "ASCII Art Program";
        public static readonly string Version = "v0.0.1";

        public static readonly int MaxArtArea = 90000;

        private static MainForm mainForm = new();

        public static readonly string DefaultArtFilesDirectoryPath = $@"{Application.LocalUserAppDataPath}\Saves";
        public static readonly string AutoSaveDirectoryPath = $@"{Application.LocalUserAppDataPath}\Autosaves";

        private static ASCIIArtFile? currentArtFile;
        public static ASCIIArtFile? CurrentArtFile { get => currentArtFile; set { currentArtFile = value; OnCurrentArtFileChanged?.Invoke(value); } }
        public delegate void CurrentArtFileChangedEvent(ASCIIArtFile? artFile);
        public static event CurrentArtFileChangedEvent? OnCurrentArtFileChanged;

        private static string? currentFilePath;
        public static string? CurrentFilePath { get => currentFilePath; set { currentFilePath = value; OnCurrentFilePathChanged?.Invoke(value); } }
        public delegate void CurrentFilePathChangedEvent(string? filePath);
        public static event CurrentFilePathChangedEvent? OnCurrentFilePathChanged;

        private static Tool currentTool = new DrawTool();
        public static Tool CurrentTool { get => currentTool; set => currentTool = value; }

        private static Rectangle selected = Rectangle.Empty;
        public static Rectangle Selected { get => selected; set => selected = value; }

        private static int currentLayerID = 0;
        public static int CurrentLayerID { get => currentLayerID; set => currentLayerID = value; }


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

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

            Console.WriteLine("Set up complete!");

            loadingForm.Hide();

            Application.Run(mainForm);
        }

        public static void NewFile(ASCIIArtFile artFile)
        {
            CurrentArtFile = artFile;
            CurrentFilePath = "";
        }

        public static void OpenFile(FileInfo file)
        {
            if (!file.Exists)
                return;

            Console.WriteLine($"Open File Path: importing file from path... {file.FullName}");

            ASCIIArtFile? artFile = ASCIIArtFile.ImportFilePath(file.FullName);

            if (artFile == null)
            {
                Console.WriteLine("Open File Path: current art file is null!");
                throw new NullReferenceException("Current art file is null!");
            }

            if (artFile.Width * artFile.Height > MaxArtArea)
            {
                Console.WriteLine($"Open File Path: File too large! (>{MaxArtArea} characters) ({artFile.Width * artFile.Height} characters)");
                throw new Exception($"Art Area is too large! Max: {MaxArtArea} characters ({artFile.Width * artFile.Height} characters)");
            }

            CurrentArtFile = artFile;
            CurrentFilePath = file.Extension == ".aaf" ? file.FullName : "";

            Console.WriteLine($"Open File Path: Imported file!");
            Console.Write($"\nFILE INFO\nFile Path: {file.FullName}\nSize: {artFile.Width}x{artFile.Height}\nArea: {artFile.Width*artFile.Height}\nTotal Art Layers: {artFile.ArtLayers.Count}\nCreated In Version: {artFile.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

            /*
                Console.WriteLine($"Open File Path: An error has occurred while opening art file ({file.FullName})! Exception: {ex.Message}");
                MessageBox.Show($"An error has occurred while opening art file ({file.FullName})! Exception: {ex.Message}", "Open File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            */
        }

        public static BackgroundWorker? SaveArtFileToPathAsync(string path)
        {
            if (CurrentArtFile == null) 
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

                if (CurrentArtFile == null)
                    return;

                Console.WriteLine("Save File: Saving art file to " + path);

                FileInfo fileInfo = CurrentArtFile.WriteTo(path);

                Console.WriteLine("Save File: Art file saved to " + path + "!");

                args.Result = fileInfo;
            }

            return bgWorker;
        }

        public static BackgroundWorker? ExportArtFileToPathAsync(string path)
        {
            if (CurrentArtFile == null)
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

                if (CurrentArtFile == null)
                    throw new Exception("Current Art File is null!");

                Console.WriteLine("Export File: Exporting art file to " + path);

                FileInfo fileInfo = CurrentArtFile.ExportTo(path, bgWorker);

                Console.WriteLine("Export File: Art file exported to " + path + "!");

                args.Result = fileInfo;
            }

            return bgWorker;
        }

        public static void CopyArtFileToClipboard()
        {
            if (CurrentArtFile == null)
                return;

            Console.WriteLine("Copy Art To Clipboard: Copying art file to clipboard...");
            string artString = CurrentArtFile.GetArtString();

            Clipboard.SetText(artString);
            Console.WriteLine("Copy Art To Clipboard: Copied art file to clipboard!");
        }
    }
}