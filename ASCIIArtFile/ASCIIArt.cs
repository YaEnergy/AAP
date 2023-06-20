using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

        public List<ArtLayer> ArtLayers { get; private set; } = new();
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;

        public delegate void ArtChangedEvent(int layerIndex, Point artMatrixPosition, char? character);
        public event ArtChangedEvent? OnArtChanged;

        public delegate void ArtLayerListChangedEvent(List<ArtLayer> artLayers);
        public event ArtLayerListChangedEvent? OnArtLayerListChanged;

        public delegate void ArtLayerAddedEvent(int index, ArtLayer artLayer);
        public event ArtLayerAddedEvent? OnArtLayerAdded;

        public delegate void ArtLayerRemovedEvent(int index);
        public event ArtLayerRemovedEvent? OnArtLayerRemoved;

        public delegate void ArtLayerPropertiesChangedEvent(int index, ArtLayer artLayer, bool updateCanvas = false);
        public event ArtLayerPropertiesChangedEvent? OnArtLayerPropertiesChanged;

        public ASCIIArt(int width, int height, int updatedinVersion, int createdinVersion) 
        {
            CreatedInVersion = createdinVersion;
            updatedInVersion = updatedinVersion;
            Width = width;
            Height = height;
        }

        public ASCIIArt()
        {

        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        #region Layers
        public void InsertLayer(int index, ArtLayer artLayer)
        {
            ArtLayers.Insert(index, artLayer);
            OnArtLayerAdded?.Invoke(index, ArtLayers[index]);
        }

        public void AddLayer(ArtLayer artLayer)
        {
            ArtLayers.Add(artLayer);
            OnArtLayerAdded?.Invoke(ArtLayers.Count - 1, artLayer);
        }

        public void RemoveLayer(int index)
        {
            if (ArtLayers.Count <= index) 
                return;

            ArtLayers.RemoveAt(index);
            OnArtLayerRemoved?.Invoke(index);
        }

        public void SetLayerIndexName(int index, string layerName)
        {
            if (ArtLayers.Count <= index)
                return;

            ArtLayers[index].Name = layerName;
            OnArtLayerPropertiesChanged?.Invoke(index, ArtLayers[index], false);
        }

        public void SetLayerIndexVisibility(int index, bool visible)
        {
            if (ArtLayers.Count <= index)
                return;

            ArtLayers[index].Visible = visible;
            OnArtLayerPropertiesChanged?.Invoke(index, ArtLayers[index], true);
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

        #region Tool Functions
        public void Draw(int layerIndex, Point artMatrixPosition, char? character)
        {
            if (layerIndex < 0)
                return;

            if (ArtLayers.Count == 0)
                return;

            if (artMatrixPosition.X < 0 || artMatrixPosition.Y < 0 || artMatrixPosition.X >= Width || artMatrixPosition.Y >= Height)
                return;

            ArtLayers[layerIndex].Data[artMatrixPosition.X][artMatrixPosition.Y] = character;

            OnArtChanged?.Invoke(layerIndex, artMatrixPosition, character);
        }

        public void Crop(Rectangle cropRect)
        {
            Console.WriteLine(cropRect.ToString());

            SetSize(cropRect.Width, cropRect.Height);

            if (ArtLayers.Count == 0)
                return;

            List<ArtLayer> layers = new();

            for (int i = 0; i < ArtLayers.Count; i++)
            {
                ArtLayer newArtLayer = new(ArtLayers[i].Name, Width, Height);

                newArtLayer.Visible = ArtLayers[i].Visible;

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        newArtLayer.Data[x][y] = ArtLayers[i].Data[cropRect.X + x][cropRect.Y + y];

                layers.Add(newArtLayer);
            }

            ArtLayers = layers;

            OnArtLayerListChanged?.Invoke(layers);

            return;
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
