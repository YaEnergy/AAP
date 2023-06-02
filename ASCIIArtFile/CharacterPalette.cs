using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class CharacterPalette
    {
        public readonly string Name;
        public readonly char[] Characters;

        public CharacterPalette(string name, char[] characters)
        {
            Name = name;
            Characters = characters;
        }

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
                            characters.Add(character);

                    characterPalette = new(fileInfo.Name.Replace(fileInfo.Extension, ""), characters);

                    return characterPalette;
                case ".aappal":
                    JsonSerializer js = JsonSerializer.CreateDefault();
                    StreamReader sr = File.OpenText(path);
                    JsonTextReader jr = new(sr);

                    characterPalette = js.Deserialize<CharacterPalette>(jr);

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
                    throw new NotImplementedException();
                /*string charString

                StreamWriter sw = File.CreateText(path);
                sw.Write(charString);

                sw.Close();
                break;*/
                case ".aappal":
                    throw new NotImplementedException();
                default:
                    throw new Exception($"CharacterPalette.ExportTo(path: {path}): no case for extension {fileInfo.Extension} exists!");
            }

            return fileInfo;
        }
    }
}
