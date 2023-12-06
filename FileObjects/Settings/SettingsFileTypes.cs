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

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = new(EncodeStream);

            js.Serialize(sw, FileObject);

            sw.Flush();
            sw.Close();
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
#if DEBUG
            ConsoleLogger.Inform("--Encode Settings--");
            FileObject.Log();
#endif

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = new(EncodeStream);

            Task serializeTask = Task.Run(() => js.Serialize(sw, FileObject));

            await serializeTask;

            await sw.FlushAsync();
            sw.Close();
        }
    }

    public class AppSettingsDecoder : FileObjectDecoder<AppSettings>
    {
        public AppSettingsDecoder(Stream stream) : base(stream)
        {

        }

        public override AppSettings Decode()
        {
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = new(DecodeStream);

            JsonTextReader jr = new(sr);

            AppSettings imported = js.Deserialize<AppSettings>(jr)!;
            jr.CloseInput = true;
            jr.Close();

            return imported;
        }

        public override async Task<AppSettings> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = new(DecodeStream);
            JsonTextReader jr = new(sr);

            Task<AppSettings?> deserializeTask = Task.Run(() => js.Deserialize<AppSettings>(jr));
            AppSettings imported = (await deserializeTask)!;

            jr.CloseInput = true;
            jr.Close();

            return imported;
        }
    }
}
