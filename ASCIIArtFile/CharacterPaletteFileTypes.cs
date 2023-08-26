using AAP.BackgroundTasks;
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

namespace AAP
{
    public class TXTCharacterPalette : IAAPFile<CharacterPalette>
    {
        public CharacterPalette FileObject { get; set; }
        public string FilePath { get; set; }

        public TXTCharacterPalette(CharacterPalette palette, string filePath)
        {
            FileObject = palette;
            FilePath = filePath;
        }

        public void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Reading file...", true));
            string[] txtLines = File.ReadAllLines(FilePath);

            if (txtLines.Length <= 0)
                throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): txt file contains no lines!");

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Getting characters...", true));
            ObservableCollection<char> characters = new();

            foreach (string line in txtLines)
                foreach (char character in line.ToCharArray())
                    if (!CharacterPalette.InvalidCharacters.Contains(character))
                        characters.Add(character);
                    else
                        throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): .txt file contains invalid character {character}!");

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Finishing up...", true));
            FileObject.Name = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
            FileObject.Characters = characters;
        }

        public bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            string charString = "";

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Creating string...", true));
            foreach (char character in FileObject.Characters)
                charString += character;

            if (bgWorker != null)
            {
                if (bgWorker.CancellationPending)
                    return false;

                bgWorker.WorkerSupportsCancellation = false;
            }

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Writing to file...", true));
            using StreamWriter swText = File.CreateText(FilePath);

            swText.Write(charString);

            return true;
        }
    }

    public class AAPPALCharacterPalette : IAAPFile<CharacterPalette>
    {
        public CharacterPalette FileObject { get; set; }
        public string FilePath { get; set; }

        public AAPPALCharacterPalette(CharacterPalette palette, string filePath)
        {
            FileObject = palette;
            FilePath = filePath;
        }

        public void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string tempFilePath = Path.GetTempFileName();

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Decompressing file...", true));
            FileStream fs = File.Create(tempFilePath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                output.CopyTo(fs);

            fs.Close();

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Deserializing decompressed file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            CharacterPalette? importedPalette = js.Deserialize<CharacterPalette>(jr) ?? throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): imported palette is null!");

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Checking for invalid characters...", true));
            foreach (char invalidCharacter in CharacterPalette.InvalidCharacters)
                if (importedPalette.Characters.Contains(invalidCharacter))
                    throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): file contains invalid character {invalidCharacter}!");

            jr.CloseInput = true;
            jr.Close();

            bgWorker?.ReportProgress(100, new BackgroundTaskUpdateArgs("Deleting decompressed path...", true));
            File.Delete(tempFilePath);

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Finishing up...", true));
            FileObject.Name = importedPalette.Name;
            FileObject.Characters = importedPalette.Characters;
            FileObject.IsPresetPalette = importedPalette.IsPresetPalette;
        }

        public bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            string tempFilePath = Path.GetTempFileName();

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);
            js.Serialize(sw, FileObject);
            sw.Close();

            if (bgWorker != null)
            {
                if (bgWorker.CancellationPending)
                {
                    File.Delete(tempFilePath);
                    return false;
                }

                bgWorker.WorkerSupportsCancellation = false;
            }

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
            using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                fs.CopyTo(output);

            bgWorker?.ReportProgress(100, new BackgroundTaskUpdateArgs("Deleting uncompressed path", true));
            File.Delete(tempFilePath);

            return true;
        }
    }
}
