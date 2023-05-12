using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AAP
{
    public class ASCIIArtFile
    {
        private static readonly string EXTENSION = ".aaf";

        public readonly string CreatedInVersion = "???";
        protected string UpdatedInVersion = "???";

        public List<ArtLayer> ArtLayers = new();
        public readonly int Width = 1;
        public readonly int Height = 1;

        public ASCIIArtFile(int width, int height, string updatedinVersion, string createdinVersion) 
        {
            CreatedInVersion = createdinVersion;
            UpdatedInVersion = updatedinVersion;
            Width = width;
            Height = height;
        }

        public ArtLayer AddBackgroundLayer()
        {
            ArtLayer backgroundLayer = new("Background", Width, Height);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    backgroundLayer.Data[x][y] = '█';

            ArtLayers.Add(backgroundLayer);

            return backgroundLayer;
        }

        public FileInfo WriteTo(string path)
        {
            if (!path.EndsWith(EXTENSION))
                path += EXTENSION;

            UpdatedInVersion = MainProgram.Version;

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(path);

            js.Serialize(sw, this);

            sw.Close();

            return new(path);
        }

        public string GetArtString()
        {
            Dictionary<Point, char?> visibleArtMatrix = new();

            for (int i = 0; i < ArtLayers.Count; i++)
                if (ArtLayers[i].Visible)
                    for(int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                        {
                            char? character = ArtLayers[i].Data[x][y];

                            if (character == null) 
                                continue;

                            visibleArtMatrix.Add(new(x, y), character.Value);
                        }

            string art = "";

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Point coord = new(x, y);
                    art += visibleArtMatrix[coord] == null ? " " : visibleArtMatrix[coord];
                }

                art += "\n";
            }

            return art;
        }

        public static ASCIIArtFile? ImportFilePath(string path)
        {
            FileInfo fileInfo = new(path);

            if (!fileInfo.Exists)
                return null;

            ASCIIArtFile? artFile;

            switch (fileInfo.Extension)
            {
                case ".txt":
                    string[] lines = File.ReadAllLines(fileInfo.FullName);

                    if (lines.Length <= 0)
                        throw new Exception($"ASCIIArtFile.ImportFile(path: {path}): txt file contains no lines!");

                    artFile = new(lines[0].Length, lines.Length, MainProgram.Version, MainProgram.Version);
                    ArtLayer artLayer = new("Imported Art", artFile.Width, artFile.Height);

                    for(int y = 0; y < artFile.Height; y++)
                    {
                        if (lines[y].Length != artFile.Width)
                            throw new Exception($"ASCIIArtFile.ImportFile(path: {path}): txt file line {y} has a length of {lines[y].Length} characters, which is not equal to the amount of characters in the first line! ({artFile.Width}");

                        char[] chars = lines[y].ToCharArray();
                        for (int x = 0; x < artFile.Width; x++)
                            artLayer.Data[x][y] = chars[x];
                    }

                    artFile.ArtLayers.Add(artLayer);

                    return artFile;
                case ".aaf":
                    JsonSerializer js = JsonSerializer.CreateDefault();
                    StreamReader sr = File.OpenText(path);
                    JsonTextReader jr = new(sr);

                    artFile = js.Deserialize<ASCIIArtFile>(jr);

                    jr.CloseInput = true;
                    jr.Close();

                    return artFile;
                default:
                    throw new Exception($"ASCIIArtFile.ImportFile(path: {path}): no case for extension {fileInfo.Extension} exists!");
            }
        }

        public FileInfo ExportTo(string path)
        {
            FileInfo fileInfo = new(path);

            switch(fileInfo.Extension)
            {
                case ".txt":
                    string art = GetArtString();

                    StreamWriter sw = File.CreateText(path);
                    sw.Write(art);

                    sw.Close();
                    break;
                default:
                    throw new Exception($"ASCIIArtFile.ExportTo(path: {path}): no case for extension {fileInfo.Extension} exists!");
            }

            return fileInfo;
        }

        public void CopyToClipboard()
            => Clipboard.SetText(GetArtString());
    }
}
