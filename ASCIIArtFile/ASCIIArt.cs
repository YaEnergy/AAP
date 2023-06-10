using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AAP
{
    public class ASCIIArt
    {
        public static readonly char EMPTYCHARACTER = ' '; //Figure Space
        private static readonly string EXTENSION = ".aaf";

        public readonly int CreatedInVersion = 0;
        private int updatedInVersion = 0;
        public int UpdatedInVersion { get => updatedInVersion; }

        public List<ArtLayer> ArtLayers = new();
        public readonly int Width = 1;
        public readonly int Height = 1;

        public delegate void ArtChangedEvent(int layerIndex, Point artMatrixPosition, char character);
        public event ArtChangedEvent? OnArtChanged;

        public delegate void ArtLayerListChangedEvent(List<ArtLayer> artLayers);
        public event ArtLayerListChangedEvent? OnArtLayerListChanged;

        public ASCIIArt(int width, int height, int updatedinVersion, int createdinVersion) 
        {
            CreatedInVersion = createdinVersion;
            updatedInVersion = updatedinVersion;
            Width = width;
            Height = height;
        }

        #region Layers
        public void InsertLayer(int index, ArtLayer artLayer)
        {
            ArtLayers.Insert(index, artLayer);
            OnArtLayerListChanged?.Invoke(ArtLayers);
        }

        public void AddLayer(ArtLayer artLayer)
        {
            ArtLayers.Add(artLayer);
            OnArtLayerListChanged?.Invoke(ArtLayers);
        }

        public void RemoveLayer(int index)
        {
            if (ArtLayers.Count == 0) 
                return;

            ArtLayers.RemoveAt(index);
            OnArtLayerListChanged?.Invoke(ArtLayers);
        }
        #endregion

        public string GetArtString(BackgroundWorker? bgWorker = null)
        {
            Dictionary<Point, char> visibleArtMatrix = new();

            for (int i = 0; i < ArtLayers.Count; i++)
                if (ArtLayers[i].Visible)
                    for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                        {
                            char? character = ArtLayers[i].Data[x][y];

                            if (character == null)
                                continue;

                            visibleArtMatrix[new(x, y)] = character.Value;
                        }

            string art = "";

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Point coord = new(x, y);

                    art += !visibleArtMatrix.ContainsKey(coord) ? ASCIIArt.EMPTYCHARACTER : visibleArtMatrix[coord].ToString();
                }

                art += "\n";
            }

            return art;
        }

        #region File
        public FileInfo WriteTo(string path)
        {
            if (!path.EndsWith(EXTENSION))
                path += EXTENSION;

            updatedInVersion = ASCIIArtFile.Version;
                
            List<ArtLayerFile> artLayerFiles = new();
            
            for(int i = 0; i < ArtLayers.Count; i++)
                artLayerFiles.Add(new(ArtLayers[i].Name, ArtLayers[i].Visible, ArtLayers[i].GetArtString()));

            ASCIIArtFile artFile = new(Width, Height, UpdatedInVersion, CreatedInVersion, artLayerFiles);

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(path);

            js.Serialize(sw, artFile);

            sw.Close();

            return new(path);
        }

        public static ASCIIArt? ImportFilePath(string path, BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(path);

            if (!fileInfo.Exists)
                return null;

            ASCIIArt? art;

            switch (fileInfo.Extension)
            {
                case ".txt":
                    string[] txtLines = File.ReadAllLines(fileInfo.FullName);

                    if (txtLines.Length <= 0)
                        throw new Exception($"ASCIIArtFile.ImportFile(path: {path}): txt file contains no lines!");

                    int txtWidth = 0;
                    int txtHeight = txtLines.Length;

                    //Get total width
                    foreach(string line in txtLines)
                        if (line.Length > txtWidth)
                            txtWidth = line.Length;

                    art = new(txtWidth, txtHeight, ASCIIArtFile.Version, ASCIIArtFile.Version);
                    ArtLayer txtArtLayer = new("Imported Art", art.Width, art.Height);

                    for(int y = 0; y < txtHeight; y++)
                    {
                        char[] chars = txtLines[y].ToCharArray();
                        for (int x = 0; x < txtWidth; x++)
                            txtArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x];
                    }

                    art.ArtLayers.Add(txtArtLayer);

                    return art;
                case ".aaf":
                    JsonSerializer js = JsonSerializer.CreateDefault();
                    StreamReader sr = File.OpenText(path);
                    JsonTextReader jr = new(sr);

                    ASCIIArtFile artFile = js.Deserialize<ASCIIArtFile>(jr);
                    art = new(artFile.Width, artFile.Height, artFile.UpdatedInVersion, artFile.CreatedInVersion);

                    for(int i = 0; i < artFile.ArtLayers.Count; i++)
                    {
                        Console.WriteLine("Art layer name: " + artFile.ArtLayers[i].Name);
                        ArtLayer aafArtLayer = new(artFile.ArtLayers[i].Name, art.Width, art.Height);

                        aafArtLayer.Visible = artFile.ArtLayers[i].Visible;

                        string[] aafLines = artFile.ArtLayers[i].ArtLayerString.Split("\n");

                        for (int y = 0; y < art.Height; y++)
                        {
                            char[] chars = aafLines[y].ToCharArray();
                            for (int x = 0; x < art.Width; x++)
                                aafArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x];
                        }

                        art.ArtLayers.Insert(i, aafArtLayer);
                    }

                    jr.CloseInput = true;
                    jr.Close();

                    return art;
                default:
                    throw new Exception($"ASCIIArtFile.ImportFile(path: {path}): no case for extension {fileInfo.Extension} exists!");
            }
        }

        public FileInfo ExportTo(string path, BackgroundWorker? bgWorker = null)
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
        #endregion

        #region Tool Functions
        public void Draw(int layerIndex, Point artMatrixPosition, char character)
        {
            if (ArtLayers.Count == 0)
                return;

            if (artMatrixPosition.X < 0 || artMatrixPosition.Y < 0 || artMatrixPosition.X >= Width || artMatrixPosition.Y >= Height)
                return;

            ArtLayers[layerIndex].Data[artMatrixPosition.X][artMatrixPosition.Y] = character;

            OnArtChanged?.Invoke(layerIndex, artMatrixPosition, character);
        }

        public ASCIIArt Crop(Rectangle cropRect)
        {
            ASCIIArt cropArt = new(cropRect.Width, cropRect.Height, UpdatedInVersion, CreatedInVersion);

            Console.WriteLine(cropRect.ToString());

            if (ArtLayers.Count == 0)
                return cropArt;

            for (int i = 0; i < ArtLayers.Count; i++)
            {
                ArtLayer newArtLayer = new(ArtLayers[i].Name, cropArt.Width, cropArt.Height);

                newArtLayer.Visible = ArtLayers[i].Visible;

                for (int x = 0; x < cropArt.Width; x++)
                    for (int y = 0; y < cropArt.Height; y++)
                        newArtLayer.Data[x][y] = ArtLayers[i].Data[cropRect.X + x][cropRect.Y + y];

                cropArt.ArtLayers.Add(newArtLayer);
            }

            return cropArt;
        }
        #endregion
    }

    public struct ASCIIArtFile
    {
        public static readonly int Version = 2;

        public readonly int CreatedInVersion = 0;
        public readonly int UpdatedInVersion = 0;

        public readonly List<ArtLayerFile> ArtLayers = new();
        public readonly int Width = 1;
        public readonly int Height = 1;
        public ASCIIArtFile(int width, int height, int updatedinVersion, int createdinVersion, List<ArtLayerFile> artLayers) 
        {
            Width = width;
            Height = height;
            UpdatedInVersion = updatedinVersion;
            CreatedInVersion = createdinVersion;
            ArtLayers = artLayers;
        }  
    }
}
