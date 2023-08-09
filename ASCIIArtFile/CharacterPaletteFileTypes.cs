using AAP.BackgroundTasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

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
            List<char> characters = new();

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

        public void Export(BackgroundWorker? bgWorker = null)
        {
            string charString = "";

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Creating string...", true));
            foreach (char character in FileObject.Characters)
                charString += character;

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Writing to file...", true));
            using StreamWriter swText = File.CreateText(FilePath);

            swText.Write(charString);
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

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(FilePath);
            JsonTextReader jr = new(sr);

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Deserializing file...", true));
            CharacterPalette? importedPalette = js.Deserialize<CharacterPalette>(jr) ?? throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): imported palette is null!");

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Checking for invalid characters...", true));
            foreach (char invalidCharacter in CharacterPalette.InvalidCharacters)
                if (importedPalette.Characters.Contains(invalidCharacter))
                    throw new Exception($"CharacterPalette.ImportFilePath(path: {FilePath}): file contains invalid character {invalidCharacter}!");

            jr.CloseInput = true;
            jr.Close();

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Finishing up...", true));
            FileObject.Name = importedPalette.Name;
            FileObject.Characters = importedPalette.Characters;
        }

        public void Export(BackgroundWorker? bgWorker = null)
        {
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter swAAPPAL = File.CreateText(FilePath);

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Serializing to file...", true));
            js.Serialize(swAAPPAL, FileObject);

            swAAPPAL.Close();
        }
    }
}
