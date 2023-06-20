using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class TextASCIIArt : IAAPFile<ASCIIArt>
    {
        public readonly string FilePath;

        public TextASCIIArt(string filePath)
            => this.FilePath = filePath;
        public void Import(ASCIIArt art)
        {
            string[] txtLines = File.ReadAllLines(FilePath);

            if (txtLines.Length <= 0)
                throw new Exception($"ASCIIArtFile.ImportFile(path: {FilePath}): txt file contains no lines!");

            int txtWidth = 0;
            int txtHeight = txtLines.Length;

            //Get total width
            foreach (string line in txtLines)
                if (line.Length > txtWidth)
                    txtWidth = line.Length;

            art.SetSize(txtWidth, txtHeight); //= new(txtWidth, txtHeight, ASCIIArtFile.Version, ASCIIArtFile.Version);
            ArtLayer txtArtLayer = new("Imported Art", art.Width, art.Height);

            for (int y = 0; y < txtHeight; y++)
            {
                char[] chars = txtLines[y].ToCharArray();
                for (int x = 0; x < txtWidth; x++)
                    txtArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x];
            }

            art.ArtLayers.Add(txtArtLayer);
        }

        public void Export(ASCIIArt art)
        {
            string artString = art.GetArtString();

            StreamWriter sw = File.CreateText(FilePath);
            sw.Write(artString);

            sw.Close();
        }
    }

    public class AAFASCIIArt : IAAPFile<ASCIIArt>
    {
        public readonly string FilePath;

        public AAFASCIIArt(string filePath)
            => this.FilePath = filePath;
        public void Import(ASCIIArt art)
        {
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(FilePath);
            JsonTextReader jr = new(sr);

            ASCIIArtFile artFile = js.Deserialize<ASCIIArtFile>(jr);
            art.SetSize(artFile.Width, artFile.Height);
            //art = new(artFile.Width, artFile.Height, artFile.UpdatedInVersion, artFile.CreatedInVersion);

            for (int i = 0; i < artFile.ArtLayers.Count; i++)
            {
                Console.WriteLine("Art layer name: " + artFile.ArtLayers[i].Name);
                ArtLayer aafArtLayer = new(artFile.ArtLayers[i].Name, art.Width, art.Height);

                aafArtLayer.Visible = artFile.ArtLayers[i].Visible;

                string[] aafLines = artFile.ArtLayers[i].ArtLayerString.Split("\n");

                for (int y = 0; y < art.Height; y++)
                {
                    char[] chars = aafLines[y].ToCharArray();
                    for (int x = 0; x < art.Width; x++)
                        aafArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x];
                }

                art.ArtLayers.Insert(i, aafArtLayer);
            }

            jr.CloseInput = true;
            jr.Close();
        }

        public void Export(ASCIIArt art)
        {
            List<ArtLayerFile> artLayerFiles = new();

            for (int i = 0; i < art.ArtLayers.Count; i++)
                artLayerFiles.Add(new(art.ArtLayers[i].Name, art.ArtLayers[i].Visible, art.ArtLayers[i].GetArtString()));

            ASCIIArtFile artFile = new(art.Width, art.Height, ASCIIArtFile.Version, art.CreatedInVersion, artLayerFiles);

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(FilePath);

            js.Serialize(sw, artFile);

            sw.Close();
        }
    }
}
