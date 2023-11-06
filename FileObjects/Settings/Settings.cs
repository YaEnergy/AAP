using AAP.BackgroundTasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace AAP.Files
{
    [Serializable]
    public class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Default { get; } = new();

        private bool darkMode = false;
        public bool DarkMode
        {
            get => darkMode;
            set
            {
                if (darkMode == value)
                    return;

                darkMode = value;

                PropertyChanged?.Invoke(this, new(nameof(DarkMode)));
            }
        }

        private bool toolPreviews = true;
        public bool ToolPreviews
        {
            get => toolPreviews;
            set
            {
                if (toolPreviews == value)
                    return;

                toolPreviews = value;

                PropertyChanged?.Invoke(this, new(nameof(ToolPreviews)));
            }
        }

        private string canvasTypefaceSource = "Consolas";
        public string CanvasTypefaceSource
        {
            get => canvasTypefaceSource;
            set
            {
                if (canvasTypefaceSource == value)
                    return;

                canvasTypefaceSource = value;

                PropertyChanged?.Invoke(this, new(nameof(CanvasTypefaceSource)));
            }
        }

        private bool autosaveFiles = true;
        public bool AutosaveFiles
        {
            get => autosaveFiles;
            set
            {
                if (autosaveFiles == value)
                    return;

                autosaveFiles = value;
                
                PropertyChanged?.Invoke(this, new(nameof(AutosaveFiles)));
            }
        }

        private TimeSpan autosaveInterval = new(0, 4, 0);
        public TimeSpan AutosaveInterval
        {
            get => autosaveInterval;
            set
            {
                if (autosaveInterval == value)
                    return;

                autosaveInterval = value;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveInterval)));
            }
        }

        private string languageName = "English";
        public string LanguageName
        {
            get => languageName;
            set
            {
                if (languageName == value)
                    return;

                languageName = value;

                PropertyChanged?.Invoke(this, new(nameof(LanguageName)));
            }
        }

        private List<string> autosaveFilePaths = new();
        public List<string> AutosaveFilePaths
        {
            get => autosaveFilePaths;
            set
            {
                if (autosaveFilePaths == value)
                    return;

                autosaveFilePaths = value;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveFilePaths)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AppSettings()
        {

        }

        public void Log()
        {
            ConsoleLogger.Inform("Language Name: " + this.LanguageName);
            ConsoleLogger.Inform("Dark mode: " + this.DarkMode);
            ConsoleLogger.Inform("Tool previews: " + this.ToolPreviews);
            ConsoleLogger.Inform("Autosave files: " + this.AutosaveFiles);
            ConsoleLogger.Inform("Autosave interval: " + this.AutosaveInterval);
            ConsoleLogger.Inform("Canvas family font name: " + this.CanvasTypefaceSource);

            string files = "";
            foreach (string filePath in this.AutosaveFilePaths)
                files += filePath + "\n";

            ConsoleLogger.Inform("Autosave paths: " + files);
        }

        public void Reset()
        {
            DarkMode = Default.DarkMode;
            ToolPreviews = Default.ToolPreviews;
            CanvasTypefaceSource = Default.CanvasTypefaceSource;
            AutosaveFiles = Default.AutosaveFiles;
            AutosaveInterval = Default.AutosaveInterval;
            LanguageName = Default.LanguageName;
        }

        public void Encode(Stream stream)
        {
            string tempFilePath = Path.GetTempFileName();

            StreamWriter sw = File.CreateText(tempFilePath);

#if DEBUG
            ConsoleLogger.Inform("--Encode Settings--");
            Log();
#endif

            string jsonString = JsonSerializer.Serialize(this);
            sw.WriteLine(jsonString);

            sw.Flush();
            sw.Dispose();

            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(stream, CompressionLevel.SmallestSize);
                fs.CopyTo(output);

                fs.Flush();
                output.Flush();

                output.Dispose();
            }

            File.Delete(tempFilePath);
        }

        public static AppSettings Decode(Stream stream)
        {
            string tempFilePath = Path.GetTempFileName();

            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(stream, CompressionMode.Decompress);
            output.CopyTo(fs);

            fs.Flush();
            fs.Position = 0;

            AppSettings imported = JsonSerializer.Deserialize<AppSettings>(fs) ?? throw new Exception("No settings could be imported!");
            fs.Close();

            File.Delete(tempFilePath);

            return imported;
        }

        public async Task EncodeAsync(Stream stream, BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            FileStream jfs = File.OpenWrite(tempFilePath);

#if DEBUG
            ConsoleLogger.Inform("--Encode Async Settings--");
            Log();
#endif

            await JsonSerializer.SerializeAsync(jfs, this);

            await jfs.FlushAsync();
            jfs.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));

            using (FileStream gfs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(stream, CompressionLevel.SmallestSize);
                gfs.CopyTo(output);

                gfs.Flush();
                output.Flush();

                output.Dispose();
            }

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting uncompressed path", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;
        }

        public static async Task<AppSettings> DecodeAsync(Stream stream, BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Decompressing file...", true));
            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(stream, CompressionMode.Decompress);
            await output.CopyToAsync(fs);

            await fs.FlushAsync();
            await output.FlushAsync();
            
            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed file...", true));

            fs.Position = 0;

            AppSettings imported = await JsonSerializer.DeserializeAsync<AppSettings>(fs) ?? throw new Exception("No settings could be imported!");
            fs.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;

            return imported;
        }
    }
}
