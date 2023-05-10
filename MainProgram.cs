using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;

namespace AAP
{
    public class MainProgram
    {
        public static readonly string ProgramTitle = "ASCII Art Program";
        public static readonly string Version = "v0.0.1";

        private static MainForm mainForm = new();

        public static readonly string DefaultArtFilesDirectoryPath = $@"{Application.LocalUserAppDataPath}\Saves";
        public static readonly string AutoSaveDirectoryPath = $@"{Application.LocalUserAppDataPath}\Autosaves";

        public static ASCIIArtFile? CurrentArtFile;
        public static string? CurrentFilePath;

        public static Tool CurrentTool = new SelectTool();
        public static Rectangle Selected = Rectangle.Empty;

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

        public static void OpenFilePath(string path)
        {
            ASCIIArtFile? artFile = ASCIIArtFile.ReadFrom(path);
            
            if (artFile == null)
            {
                Console.WriteLine("Open File Path: art file is null!");
                return;
            }

            Console.WriteLine("Opened art file path: " + path);
            Console.WriteLine("Created in version: " + artFile.CreatedInVersion);
            Console.WriteLine("Art size: " + artFile.Width + "x" + artFile.Height);

            CurrentArtFile = artFile;
            CurrentFilePath = path;

            mainForm.DisplayArtFile(artFile);
        }

        public static void SaveFileToPathAsync(ASCIIArtFile artFile, string path)
        {
            if (path == null)
                return;

            CurrentFilePath = path;

            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = false;
            bgWorker.DoWork += SaveWork;
            bgWorker.RunWorkerCompleted += SaveComplete;

            bgWorker.RunWorkerAsync();

            void SaveComplete(object? sender, RunWorkerCompletedEventArgs args)
            {
                if (args.Cancelled)
                    Console.WriteLine("Art file save to " + path + " cancelled!");
                else if (args.Error != null)
                    Console.WriteLine("An error has occurred while saving art file to " + path + "! exception: " + args.Error.Message);
                else
                    Console.WriteLine("Art file saved to " + path + "!");
            }

            void SaveWork(object? sender, DoWorkEventArgs args)
            {
                BackgroundWorker? bgWorker = sender as BackgroundWorker;

                if (bgWorker == null)
                    return;

                Console.WriteLine("Saving art file to " + path);
                FileInfo fileInfo = artFile.WriteTo(path);

                Process.Start("explorer.exe", fileInfo.DirectoryName == null ? DefaultArtFilesDirectoryPath : fileInfo.DirectoryName);
            }
        }
    }
}