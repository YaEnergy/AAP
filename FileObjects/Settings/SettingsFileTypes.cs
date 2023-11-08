using AAP.BackgroundTasks;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace AAP.Files
{
    public class AppSettingsEncoder : FileObjectEncoder<AppSettings>
    {
        public AppSettingsEncoder(AppSettings fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            string tempFilePath = Path.GetTempFileName();

            StreamWriter sw = File.CreateText(tempFilePath);

#if DEBUG
            ConsoleLogger.Inform("--Encode Settings--");
            FileObject.Log();
#endif

            string jsonString = JsonSerializer.Serialize(FileObject);
            sw.WriteLine(jsonString);

            sw.Flush();
            sw.Dispose();

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
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            FileStream jfs = File.OpenWrite(tempFilePath);

#if DEBUG
            ConsoleLogger.Inform("--Encode Async Settings--");
            FileObject.Log();
#endif

            await JsonSerializer.SerializeAsync(jfs, this);

            await jfs.FlushAsync();
            jfs.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));

            using (FileStream gfs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(EncodeStream, CompressionLevel.SmallestSize);
                gfs.CopyTo(output);

                gfs.Flush();
                output.Flush();

                output.Dispose();
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

            fs.Position = 0;

            AppSettings imported = JsonSerializer.Deserialize<AppSettings>(fs)!;
            fs.Close();

            File.Delete(tempFilePath);

            return imported;
        }

        public override async Task<AppSettings> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Decompressing file...", true));
            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(DecodeStream, CompressionMode.Decompress);
            await output.CopyToAsync(fs);

            await fs.FlushAsync();

            output.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed file...", true));

            fs.Position = 0;

            AppSettings imported = (await JsonSerializer.DeserializeAsync<AppSettings>(fs))!;
            fs.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;

            return imported;
        }
    }
}
