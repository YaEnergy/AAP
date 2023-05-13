using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Policy;
using Microsoft.Win32;

namespace AAP
{
    public static class MainProgram
    {
        public static readonly string ProgramTitle = "ASCII Art Program";
        public static readonly string Version = "v0.0.1";

        public static readonly int MaxArtArea = 9800;

        private static MainForm mainForm = new();

        public static readonly string DefaultArtFilesDirectoryPath = $@"{Application.LocalUserAppDataPath}\Saves";
        public static readonly string AutoSaveDirectoryPath = $@"{Application.LocalUserAppDataPath}\Autosaves";

        private static ASCIIArtFile? currentArtFile;
        public static ASCIIArtFile? CurrentArtFile { get => currentArtFile; set => currentArtFile = value; }

        private static string? currentFilePath;
        public static string? CurrentFilePath { get => currentFilePath; set { currentFilePath = value; OnCurrentFilePathChanged?.Invoke(value); } }
        public delegate void CurrentFilePathChangedEvent(string? filePath);
        public static event CurrentFilePathChangedEvent? OnCurrentFilePathChanged;

        private static Tool currentTool = new SelectTool();
        public static Tool CurrentTool { get => currentTool; set => currentTool = value; }

        private static Rectangle selected = Rectangle.Empty;
        public static Rectangle Selected { get => selected; set => selected = value; }


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

            mainForm.DisplayArtFile(artFile);
        }

        public static void OpenFile(FileInfo file)
        {
            if (!file.Exists)
                return;

            try
            {
                ASCIIArtFile? artFile = ASCIIArtFile.ImportFilePath(file.FullName);

                if (artFile == null)
                {
                    Console.WriteLine("Open File Path: current art file is null!");
                    throw new NullReferenceException("Open File Path: current art file is null!");
                }

                if (artFile.Width * artFile.Height > MaxArtArea)
                {
                    Console.WriteLine($"Open File Path: File too large! (>{MaxArtArea} characters) ({artFile.Width * artFile.Height} characters)");
                    throw new Exception($"Art Area is too large! Max: {MaxArtArea} characters ({artFile.Width * artFile.Height} characters)");
                }

                Console.WriteLine("Opened art file path: " + file.FullName);
                Console.WriteLine("Created in version: " + artFile.CreatedInVersion);
                Console.WriteLine("Art size: " + artFile.Width + "x" + artFile.Height);

                CurrentArtFile = artFile;
                CurrentFilePath = file.Extension == ".aaf" ? file.FullName : "";

                mainForm.DisplayArtFile(artFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Open File Path: An error has occurred while opening art file ({file.FullName})! Exception: {ex.Message}");
                MessageBox.Show($"An error has occurred while opening art file ({file.FullName})! Exception: {ex.Message}", "Open File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            CurrentFilePath = path;

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