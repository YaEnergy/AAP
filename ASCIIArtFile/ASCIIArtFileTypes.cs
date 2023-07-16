using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

        public void Import(ASCIIArt art, BackgroundWorker? bgWorker = null)
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
            art.CreatedInVersion = ASCIIArt.VERSION;
            ArtLayer txtArtLayer = new("Imported Art", art.Width, art.Height);

            for (int y = 0; y < txtHeight; y++)
            {
                char[] chars = txtLines[y].ToCharArray();
                for (int x = 0; x < txtWidth; x++)
                    txtArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x];
            }

            art.ArtLayers.Add(txtArtLayer);

            art.UnsavedChanges = true;
        }

        public void Export(ASCIIArt art, BackgroundWorker? bgWorker = null)
        {
            bgWorker?.ReportProgress(0, "Getting art string...");
            string artString = art.GetArtString(bgWorker);

            bgWorker?.ReportProgress(50, "Writing to file...");
            StreamWriter sw = File.CreateText(FilePath);
            sw.Write(artString);

            sw.Close();
        }
    }

    public class AAFASCIIArt : IAAPFile<ASCIIArt>
    {
        private static readonly string UncompressedExportPath = @$"{App.ApplicationDataFolderPath}\uncompressedExport";
        public readonly string FilePath;

        public AAFASCIIArt(string filePath)
            => this.FilePath = filePath;

        public void Import(ASCIIArt art, BackgroundWorker? bgWorker = null)
        {
            FileStream fs = File.Create(UncompressedExportPath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                output.CopyTo(fs);

            fs.Close();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(UncompressedExportPath);
            JsonTextReader jr = new(sr);

            ASCIIArt? importedArt = js.Deserialize<ASCIIArt>(jr);
            if (importedArt != null)
            {
                art.CreatedInVersion = importedArt.CreatedInVersion;
                art.SetSize(importedArt.Width, importedArt.Height);

                for (int i = 0; i < importedArt.ArtLayers.Count; i++)
                    art.ArtLayers.Insert(i, importedArt.ArtLayers[i]);
            }
            else
                throw new Exception("No art could be imported!");

            /*ASCIIArtFile artFile = js.Deserialize<ASCIIArtFile>(jr);
            art.CreatedInVersion = artFile.CreatedInVersion;
            art.SetSize(artFile.Width, artFile.Height);

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
            }*/

            jr.CloseInput = true;
            jr.Close();

            File.Delete(UncompressedExportPath);

            art.UnsavedChanges = false;
        }

        public void Export(ASCIIArt art, BackgroundWorker? bgWorker = null)
        {
            bgWorker?.ReportProgress(50, "Writing to uncompressed file...");
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(UncompressedExportPath);
            js.Serialize(sw, art);
            sw.Close();

            bgWorker?.ReportProgress(100, "Writing to file path as compressed file...");
            using (FileStream fs = File.OpenRead(UncompressedExportPath))
                using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                    fs.CopyTo(output);

            File.Delete(UncompressedExportPath);
        }
    }
}
