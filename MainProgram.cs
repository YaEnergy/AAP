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

        private static readonly string DefaultArtFilesDirectoryPath = $@"{Application.LocalUserAppDataPath}\Saves";
        private static readonly string AutoSaveDirectoryPath = $@"{Application.LocalUserAppDataPath}\Autosaves";

        public ASCIIArtFile? CurrentArtFile;
        public string? CurrentFilePath;

        public Tool CurrentTool = new SelectTool();
        public Rectangle Selected = Rectangle.Empty;

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

            Canvas canvas = new(mainForm.CanvasArt);
            ASCIIArtFile testArt = new(new Size(50, 30), Version, Version);
            testArt.WriteTo(@$"{DefaultArtFilesDirectoryPath}\testArt");
            canvas.DisplayArtFile(testArt);

/*#if DEBUG //For testing

            ASCIIArtFile massiveArt = new(new Size(9999, 100), Version, Version);
            massiveArt.WriteTo(@$"{DefaultArtFilesDirectoryPath}\massiveArt");

            Process.Start("explorer.exe", DefaultArtFilesDirectoryPath);
#endif*/

            Application.Run(mainForm);
        }
    }
}