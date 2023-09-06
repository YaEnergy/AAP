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

        public void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string[] txtLines = File.ReadAllLines(FilePath);

            if (txtLines.Length <= 0)
                throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): txt file contains no lines!");

            ObservableCollection<char> characters = new();

            foreach (string line in txtLines)
                foreach (char character in line.ToCharArray())
                    if (!CharacterPalette.InvalidCharacters.Contains(character))
                        characters.Add(character);
                    else
                        throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): .txt file contains invalid character {character}!");

            FileObject.Name = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
            FileObject.Characters = characters;
        }

        public async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            Task<string[]> readLinesTask = File.ReadAllLinesAsync(FilePath);
            string[] txtLines = await readLinesTask;

            if (txtLines.Length <= 0)
                throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): txt file contains no lines!");

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Getting characters...", true));
            ObservableCollection<char> characters = new();

            foreach (string line in txtLines)
                foreach (char character in line.ToCharArray())
                    if (!CharacterPalette.InvalidCharacters.Contains(character))
                        characters.Add(character);
                    else
                        throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): .txt file contains invalid character {character}!");

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Finishing up...", true));
            FileObject.Name = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
            FileObject.Characters = characters;
        }

        public void Export()
        {
            string charString = "";

            foreach (char character in FileObject.Characters)
                charString += character;

            using StreamWriter swText = File.CreateText(FilePath);

            swText.Write(charString);
        }

        public async Task ExportAsync(BackgroundTaskToken? taskToken = null)
        {
            string charString = "";

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating string...", true));
            foreach (char character in FileObject.Characters)
                charString += character;

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Writing to file...", true));
            using StreamWriter swText = File.CreateText(FilePath);

            await swText.WriteAsync(charString);
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

        public void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string tempFilePath = Path.GetTempFileName();

            FileStream fs = File.Create(tempFilePath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                output.CopyTo(fs);

            fs.Close();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            CharacterPalette? importedPalette = js.Deserialize<CharacterPalette>(jr) ?? throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): imported palette is null!");

            foreach (char invalidCharacter in CharacterPalette.InvalidCharacters)
                if (importedPalette.Characters.Contains(invalidCharacter))
                    throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): file contains invalid character {invalidCharacter}!");

            jr.CloseInput = true;
            jr.Close();

            File.Delete(tempFilePath);

            FileObject.Name = importedPalette.Name;
            FileObject.Characters = importedPalette.Characters;
            FileObject.IsPresetPalette = importedPalette.IsPresetPalette;
        }

        public async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Decompressing file...", true));
            FileStream fs = File.Create(tempFilePath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                await output.CopyToAsync(fs);

            fs.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            Task<CharacterPalette?> deserializeTask = Task.Run(() => js.Deserialize<CharacterPalette>(jr));
            CharacterPalette? importedPalette = await deserializeTask;

            if (importedPalette == null)
                throw new Exception("No character palette could be imported!");

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Checking for invalid characters...", true));
            foreach (char invalidCharacter in CharacterPalette.InvalidCharacters)
                if (importedPalette.Characters.Contains(invalidCharacter))
                    throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): file contains invalid character {invalidCharacter}!");

            jr.CloseInput = true;
            jr.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));
            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Finishing up...", true));
            FileObject.Name = importedPalette.Name;
            FileObject.Characters = importedPalette.Characters;
            FileObject.IsPresetPalette = importedPalette.IsPresetPalette;
        }

        public void Export()
        {
            string tempFilePath = Path.GetTempFileName();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);
            js.Serialize(sw, FileObject);
            sw.Close();

            using (FileStream fs = File.OpenRead(tempFilePath))
                using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                    fs.CopyTo(output);

            File.Delete(tempFilePath);
        }

        public async Task ExportAsync(BackgroundTaskToken? taskToken = null)
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
                using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                    await fs.CopyToAsync(output);

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting uncompressed path", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;
        }
    }
}
