using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class CharacterPalette
    {
        private static readonly string EXTENSION = ".aappal";
        private static readonly char[] INVALIDCHARACTERS = new char[] { ASCIIArt.EMPTYCHARACTER };

        public readonly string Name;
        public readonly char[] Characters;

        public CharacterPalette(string name, List<char> characters)
        {
            Name = name;
            Characters = characters.ToArray();
        }

        public static CharacterPalette? ImportFilePath(string path, BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(path);

            if (!fileInfo.Exists)
                return null;

            CharacterPalette? characterPalette;

            switch (fileInfo.Extension)
            {
                case ".txt":
                    string[] txtLines = File.ReadAllLines(fileInfo.FullName);

                    if (txtLines.Length <= 0)
                        throw new Exception($"CharacterPalette.ImportFilePath(path: {path}): txt file contains no lines!");

                    List<char> characters = new();

                    foreach (string line in txtLines)
                        foreach (char character in line.ToCharArray())
                            if (!INVALIDCHARACTERS.Contains(character))
                                characters.Add(character);
                            else
                                throw new Exception($"CharacterPalette.ImportFilePath(path: {path}): .txt file contains invalid character {character}!");

                    characterPalette = new(fileInfo.Name.Replace(fileInfo.Extension, ""), characters);

                    return characterPalette;
                case ".aappal":
                    JsonSerializer js = JsonSerializer.CreateDefault();
                    StreamReader sr = File.OpenText(path);
                    JsonTextReader jr = new(sr);

                    characterPalette = js.Deserialize<CharacterPalette>(jr);

                    if (characterPalette == null)
                        throw new Exception($"CharacterPalette.ImportFilePath(path: {path}): character palette is null!");

                    foreach (char invalidCharacter in INVALIDCHARACTERS)
                        if (characterPalette.Characters.Contains(invalidCharacter))
                            throw new Exception($"CharacterPalette.ImportFilePath(path: {path}): .txt file contains invalid character {invalidCharacter}!");

                    jr.CloseInput = true;
                    jr.Close();

                    return characterPalette;
                default:
                    throw new Exception($"CharacterPalette.ImportFilePath(path: {path}): no case for extension {fileInfo.Extension} exists!");
            }
        }

        public FileInfo ExportTo(string path, BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(path);

            switch (fileInfo.Extension)
            {
                case ".txt":
                    string charString = "";

                    foreach (char character in Characters)
                        charString += character;

                    StreamWriter swText = File.CreateText(path);
                    swText.Write(charString);

                    swText.Close();

                    break;
                case ".aappal":
                    JsonSerializer js = JsonSerializer.CreateDefault();
                    StreamWriter swAAPPAL = File.CreateText(path);

                    js.Serialize(swAAPPAL, this);

                    swAAPPAL.Close();

                    break;
                default:
                    throw new Exception($"CharacterPalette.ExportTo(path: {path}): no case for extension {fileInfo.Extension} exists!");
            }

            return fileInfo;
        }

        public override string ToString()
            => Name;
    }
}
