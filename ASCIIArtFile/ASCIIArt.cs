using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace AAP
{
    public class ASCIIArt
    {
        public static readonly char EMPTYCHARACTER = ' '; //Figure Space
        public static readonly int VERSION = 3;

        public int CreatedInVersion = 3;
        private int updatedInVersion = 3;
        public int UpdatedInVersion { get => updatedInVersion; }

        public List<ArtLayer> ArtLayers { get; private set; } = new();

        private int width = -1;
        public int Width 
        { 
            get => width; 
            set
            {
                if (width == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Width));

                width = value;

                OnSizeChanged?.Invoke(Width, Height);
            }
        }
        private int height = -1;
        public int Height 
        { 
            get => height; 
            set
            {
                if (height == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Height));

                height = value;

                OnSizeChanged?.Invoke(Width, Height);
            }
        }

        public delegate void CroppedEvent(ASCIIArt art);
        public event CroppedEvent? OnCropped;

        public delegate void SizeChangedEvent(int width, int height);
        public event SizeChangedEvent? OnSizeChanged;

        public delegate void ArtLayerAddedEvent(int index, ArtLayer artLayer);
        public event ArtLayerAddedEvent? OnArtLayerAdded;

        public delegate void ArtLayerRemovedEvent(int index);
        public event ArtLayerRemovedEvent? OnArtLayerRemoved;

        public delegate void ArtLayerPropertiesChangedEvent(int index, ArtLayer artLayer, bool updateCanvas = false);
        public event ArtLayerPropertiesChangedEvent? OnArtLayerPropertiesChanged;

        public bool Changed { get; set; } = false;

        public ASCIIArt()
        {

        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;

            Changed = true;
        }

        #region Layers
        public void InsertLayer(int index, ArtLayer artLayer)
        {
            ArtLayers.Insert(index, artLayer);
            OnArtLayerAdded?.Invoke(index, ArtLayers[index]);

            Changed = true;
        }

        public void AddLayer(ArtLayer artLayer)
        {
            ArtLayers.Add(artLayer);
            OnArtLayerAdded?.Invoke(ArtLayers.Count - 1, artLayer);

            Changed = true;
        }

        public void RemoveLayer(int index)
        {
            if (ArtLayers.Count <= index) 
                return;

            ArtLayers.RemoveAt(index);
            OnArtLayerRemoved?.Invoke(index);

            Changed = true;
        }

        public void SetLayerIndexName(int index, string layerName)
        {
            if (ArtLayers.Count <= index)
                return;

            ArtLayers[index].Name = layerName;
            OnArtLayerPropertiesChanged?.Invoke(index, ArtLayers[index], false);

            Changed = true;
        }

        public void SetLayerIndexVisibility(int index, bool visible)
        {
            if (ArtLayers.Count <= index)
                return;

            ArtLayers[index].Visible = visible;
            OnArtLayerPropertiesChanged?.Invoke(index, ArtLayers[index], true);

            Changed = true;
        }
        #endregion

        /// <summary>
        /// This method is very expensive, please use GetLineString(y) when you can instead.
        /// </summary>
        /// <param name="bgWorker"></param>
        /// <returns>Full ASCII art string, each line separated by \n</returns>
        public string GetArtString(BackgroundWorker? bgWorker = null)
        {
            string art = "";

            for (int y = 0; y < Height; y++)
            {
                art += GetLineString(y, bgWorker);

                art += "\n";
            }

            return art;
        }

        public string GetLineString(int y, BackgroundWorker? bgWorker = null)
        {
            char?[] visibleArtLineArray = new char?[Width];

            for (int i = 0; i < ArtLayers.Count; i++)
                if (ArtLayers[i].Visible)
                    for (int x = 0; x < Width; x++)
                    {
                        char? character = ArtLayers[i].Data[x][y];

                        if (character == null)
                            continue;

                        visibleArtLineArray[x] = character.Value;
                    }

            string line = "";

            for (int x = 0; x < Width; x++)
                line += visibleArtLineArray[x] == null ? EMPTYCHARACTER : visibleArtLineArray[x].ToString();

            return line;
        }

        #region Tool Functions
        public void Crop(Rect cropRect)
        {
            Console.WriteLine(cropRect.ToString());

            SetSize((int)cropRect.Width, (int)cropRect.Height);

            if (ArtLayers.Count == 0)
                return;

            List<ArtLayer> layers = new();

            for (int i = 0; i < ArtLayers.Count; i++)
            {
                ArtLayer newArtLayer = new(ArtLayers[i].Name, Width, Height);

                newArtLayer.Visible = ArtLayers[i].Visible;

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        newArtLayer.Data[x][y] = ArtLayers[i].Data[(int)cropRect.X + x][(int)cropRect.Y + y];

                layers.Add(newArtLayer);
            }

            ArtLayers = layers;

            OnCropped?.Invoke(this);

            Changed = true;

            return;
        }
        #endregion
    }
}
