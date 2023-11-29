using AAP.BackgroundTasks;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AAP.Files
{
    public class AppSettingsEncoder : FileObjectEncoder<AppSettings>
    {
        public AppSettingsEncoder(AppSettings fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
#if DEBUG
            ConsoleLogger.Inform("--Encode Settings--");
            FileObject.Log();
#endif

            string tempFilePath = Path.GetTempFileName();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            js.Serialize(sw, FileObject);

            sw.Flush();
            sw.Close();

            using (StreamReader srlog = File.OpenText(tempFilePath))
            {
                ConsoleLogger.Log(srlog.ReadToEnd());
            }

            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(EncodeStream, CompressionLevel.SmallestSize);
                fs.CopyTo(output);

                fs.Flush();
                output.Flush();

                output.Dispose();
            }

            File.Delete(tempFilePath);
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
#if DEBUG
            ConsoleLogger.Inform("--Encode Settings--");
            FileObject.Log();
#endif

            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            Task serializeTask = Task.Run(() => js.Serialize(sw, FileObject));

            await serializeTask;

            await sw.FlushAsync();
            sw.Close();

            using (StreamReader srlog = File.OpenText(tempFilePath))
            {
                ConsoleLogger.Log(srlog.ReadToEnd());
            }

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(EncodeStream, CompressionLevel.SmallestSize);
                await fs.CopyToAsync(output);

                await fs.FlushAsync();
                await output.FlushAsync();

                await output.DisposeAsync();
            }

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting uncompressed path", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;
        }
    }

    public class AppSettingsDecoder : FileObjectDecoder<AppSettings>
    {
        public AppSettingsDecoder(Stream stream) : base(stream)
        {

        }

        public override AppSettings Decode()
        {
            string tempFilePath = Path.GetTempFileName();

            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(DecodeStream, CompressionMode.Decompress);
            output.CopyTo(fs);

            output.Flush();
            fs.Flush();

            fs.Dispose();

            using (StreamReader srlog = File.OpenText(tempFilePath))
            {
                ConsoleLogger.Log(srlog.ReadToEnd());
            }

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            AppSettings imported = js.Deserialize<AppSettings>(jr)!;
            jr.CloseInput = true;
            jr.Close();

            File.Delete(tempFilePath);

            return imported;
        }

        public override async Task<AppSettings> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Decompressing art file...", true));
            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(DecodeStream, CompressionMode.Decompress);
            await output.CopyToAsync(fs);

            await output.FlushAsync();
            await fs.FlushAsync();

            await fs.DisposeAsync();

            using (StreamReader srlog = File.OpenText(tempFilePath))
            {
                ConsoleLogger.Log(srlog.ReadToEnd());
            }

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed art file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            Task<AppSettings?> deserializeTask = Task.Run(() => js.Deserialize<AppSettings>(jr));
            AppSettings imported = (await deserializeTask)!;

            jr.CloseInput = true;
            jr.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;

            return imported;
        }
    }
}
