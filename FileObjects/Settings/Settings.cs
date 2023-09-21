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

        public event PropertyChangedEventHandler? PropertyChanged;

        public AppSettings()
        {

        }

        public void Reset()
        {
            DarkMode = Default.DarkMode;
            CanvasTypefaceSource = Default.CanvasTypefaceSource;
        }

        public void Encode(Stream stream)
        {
            string tempFilePath = Path.GetTempFileName();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            js.Serialize(sw, this);
            sw.Close();

            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(stream, CompressionLevel.SmallestSize);
                fs.CopyTo(output);

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

            Task serializeTask = Task.Run(() => js.Serialize(sw, this));

            await serializeTask;

            sw.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(stream, CompressionLevel.SmallestSize);
                await fs.CopyToAsync(output);

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
