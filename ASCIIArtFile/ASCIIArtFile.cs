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

            return new FileInfo(path);
        }

        public static ASCIIArtFile? ReadFrom(string path) 
        {
            if (!path.EndsWith(EXTENSION))
                return null;

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(path);
            JsonTextReader jr = new JsonTextReader(sr);

            ASCIIArtFile? artFile = js.Deserialize<ASCIIArtFile>(jr);

            jr.CloseInput = true;
            jr.Close();

            return artFile;
        }
    }
}
