using AAP.BackgroundTasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Files
{
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
            ConsoleLogger.Inform("Dark mode: " + this.DarkMode);
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
            CanvasTypefaceSource = Default.CanvasTypefaceSource;
            AutosaveFiles = Default.AutosaveFiles;
            AutosaveInterval = Default.AutosaveInterval;
            LanguageName = Default.LanguageName;
        }

        public void Encode(Stream stream)
        {
            string tempFilePath = Path.GetTempFileName();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

#if DEBUG
            ConsoleLogger.Inform("--Encode Settings--");
            Log();
#endif

            js.Serialize(sw, this);

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

            output.Flush();
            fs.Flush();
            fs.Close();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            AppSettings imported = js.Deserialize<AppSettings>(jr) ?? throw new Exception("No settings could be imported!");
            jr.CloseInput = true;
            jr.Close();

            File.Delete(tempFilePath);

            return imported;
        }

        public async Task EncodeAsync(Stream stream, BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

#if DEBUG
            ConsoleLogger.Inform("--Encode Async Settings--");
            Log();
#endif

            Task serializeTask = Task.Run(() => js.Serialize(sw, this));

            await serializeTask;

            await sw.FlushAsync();
            await sw.DisposeAsync();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(stream, CompressionLevel.SmallestSize);
                await fs.CopyToAsync(output);

                await fs.FlushAsync();
                await output.FlushAsync();

                await output.DisposeAsync();
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

            fs.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            Task<AppSettings?> deserializeTask = Task.Run(() => js.Deserialize<AppSettings>(jr));
            AppSettings imported = await deserializeTask ?? throw new Exception("No settings could be imported!");

            jr.CloseInput = true;
            jr.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;

            return imported;
        }
    }
}
