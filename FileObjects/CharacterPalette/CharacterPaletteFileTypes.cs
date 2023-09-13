using AAP.BackgroundTasks;
using AAP.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Files
{
    public class TextCharacterPaletteEncoder : FileObjectEncoder<CharacterPalette>
    {
        public TextCharacterPaletteEncoder(CharacterPalette fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            string charString = "";

            foreach (char character in FileObject.Characters)
                charString += character;

            StreamWriter sw = new(EncodeStream);

            sw.Write(charString);
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            string charString = "";

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating string...", true));
            foreach (char character in FileObject.Characters)
                charString += character;

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Writing to file...", true));

            StreamWriter sw = new(EncodeStream);

            await sw.WriteAsync(charString);
        }
    }

    public class TextCharacterPaletteDecoder : FileObjectDecoder<CharacterPalette>
    {
        public TextCharacterPaletteDecoder(Stream stream) : base(stream)
        {

        }

        public override CharacterPalette Decode()
        {
            StreamReader sr = new(DecodeStream);

            ObservableCollection<char> characters = new();

            while (!sr.EndOfStream)
            {
                string? line = sr.ReadLine();

                if (line != null)
                    foreach (char character in line.ToCharArray()) 
                        characters.Add(character);
            }

            return new("Imported Palette", characters);
        }

        public override async Task<CharacterPalette> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Getting characters...", true));
            StreamReader sr = new(DecodeStream);

            ObservableCollection<char> characters = new();

            while (!sr.EndOfStream)
            {
                string? line = await sr.ReadLineAsync();

                if (line != null)
                    foreach (char character in line.ToCharArray())
                        characters.Add(character);
            }

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating palette...", true));

            return new("Imported Palette", characters);
        }
    }

    public class AAPPALCharacterPaletteEncoder : FileObjectEncoder<CharacterPalette>
    {
        public AAPPALCharacterPaletteEncoder(CharacterPalette fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            string tempFilePath = Path.GetTempFileName();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            js.Serialize(sw, FileObject);
            sw.Close();

            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(EncodeStream, CompressionLevel.SmallestSize);
                fs.CopyTo(output);
            }

            File.Delete(tempFilePath);
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            Task serializeTask = Task.Run(() => js.Serialize(sw, FileObject));

            await serializeTask;

            sw.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(EncodeStream, CompressionLevel.SmallestSize);
                await fs.CopyToAsync(output);
            }

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting uncompressed path", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;
        }
    }

    public class AAPPALCharacterPaletteDecoder : FileObjectDecoder<CharacterPalette>
    {
        public AAPPALCharacterPaletteDecoder(Stream stream) : base(stream)
        {

        }

        public override CharacterPalette Decode()
        {
            string tempFilePath = Path.GetTempFileName();

            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(DecodeStream, CompressionMode.Decompress);
            output.CopyTo(fs);

            fs.Close();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            CharacterPalette imported = js.Deserialize<CharacterPalette>(jr) ?? throw new Exception("No palette could be imported!");
            jr.CloseInput = true;
            jr.Close();

            File.Delete(tempFilePath);

            return imported;
        }

        public override async Task<CharacterPalette> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Decompressing palette file...", true));
            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(DecodeStream, CompressionMode.Decompress);
            await output.CopyToAsync(fs);

            fs.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed palette file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            Task<CharacterPalette?> deserializeTask = Task.Run(() => js.Deserialize<CharacterPalette>(jr));
            CharacterPalette imported = await deserializeTask ?? throw new Exception("No palette could be imported!");

            jr.CloseInput = true;
            jr.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;

            return imported;
        }
    }
}
