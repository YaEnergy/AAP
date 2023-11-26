using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AAP.Timelines;
using Newtonsoft.Json;

namespace AAP
{
    [Serializable]
    public class ASCIIArt : ITimelineObject, INotifyPropertyChanged
    {
        public static readonly char EMPTYCHARACTER = ' ';//' '; //Figure Space
        public static readonly int VERSION = 3;

        public int CreatedInVersion = 3;
        private readonly int updatedInVersion = 3;
        public int UpdatedInVersion { get => updatedInVersion; }

        private readonly ObservableCollection<ArtLayer> artLayers = new();
        public ObservableCollection<ArtLayer> ArtLayers
        {
            get => artLayers;
        }

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

                PropertyChanged?.Invoke(this, new(nameof(Width)));
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

                PropertyChanged?.Invoke(this, new(nameof(Height)));
            }
        }

        public delegate void CroppedEvent(ASCIIArt art);
        /// <summary>
        /// Invoked when the art is cropped.
        /// </summary>
        public event CroppedEvent? OnCropped;

        public delegate void SizeChangedEvent(int width, int height);
        /// <summary>
        /// Invoked when the art size changed.
        /// </summary>
        public event SizeChangedEvent? OnSizeChanged;

        public delegate void ArtLayerListChangeEvent(int index, ArtLayer artLayer);
        /// <summary>
        /// Invoked when a layer is added.
        /// </summary>
        public event ArtLayerListChangeEvent? OnArtLayerAdded;

        /// <summary>
        /// Invoked when a layer is removed.
        /// </summary>
        public event ArtLayerListChangeEvent? OnArtLayerRemoved;

        /// <summary>
        /// Invoked when a layer's index in the artLayers changes.
        /// </summary>
        public event ArtLayerListChangeEvent? OnArtLayerIndexChanged;

        public delegate void OnArtUpdatedEvent(ASCIIArt art);
        /// <summary>
        /// Invoked when the visible art mostly likely changed.
        /// </summary>
        public event OnArtUpdatedEvent? OnArtUpdated;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ASCIIArt()
        {
            ArtLayers.CollectionChanged += ArtLayerCollectionChanged;
        }

        private void ArtLayerCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if(e.NewItems != null)
                        foreach (ArtLayer layer in e.NewItems)
                            OnArtLayerAdded?.Invoke(e.NewStartingIndex, layer);
                    
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (ArtLayer layer in e.OldItems)
                            OnArtLayerRemoved?.Invoke(e.OldStartingIndex, layer);

                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldItems != null)
                        foreach (ArtLayer layer in e.OldItems)
                            OnArtLayerIndexChanged?.Invoke(e.OldStartingIndex, layer);

                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (e.NewItems != null)
                        foreach (ArtLayer layer in e.NewItems)
                            OnArtLayerAdded?.Invoke(e.NewStartingIndex, layer);

                    if (e.OldItems != null)
                        foreach (ArtLayer layer in e.OldItems)
                            OnArtLayerRemoved?.Invoke(e.OldStartingIndex, layer);

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Sets the width and height of the ASCII Art, and invoking OnSizeChanged.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetSize(int width, int height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            bool changedSize = false;

            if (this.height != height)
            {
                this.height = height;
                changedSize = true;
                PropertyChanged?.Invoke(this, new(nameof(Height)));
            }

            if (this.width != width)
            {
                this.width = width;
                changedSize = true;
                PropertyChanged?.Invoke(this, new(nameof(Width)));
            }

            if (changedSize)
                OnSizeChanged?.Invoke(width, height);
        }

        /// <summary>
        /// Gets all line strings in the art and adds them together, each line is separated by a linebreak character.
        /// </summary>
        /// <returns>Full ASCII art string, each line separated by \n</returns>
        public string GetArtString()
        {
            string art = "";

            for (int y = 0; y < Height; y++)
                art += GetLineString(y) + "\n";

            return art;
        }

        /// <summary>
        /// Gets the visible ASCII text of a single line.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="bgWorker"></param>
        /// <returns>String of line (y)</returns>
        public string GetLineString(int y)
        {
            string line = "";

            if (Width == 0)
                return line;

            for (int x = 0; x < Width; x++)
                line += GetCharacter(x, y) ?? EMPTYCHARACTER;

            return line;
        }

        #region Drawing

        /// <summary>
        /// Gets the visible ASCII char at the specified position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>char?</returns>
        public char? GetCharacter(int x, int y)
        {
            for (int i = 0; i < ArtLayers.Count; i++)
            {
                ArtLayer artLayer = ArtLayers[i];

                if (!artLayer.Visible)
                    continue;

                if (!artLayer.IsCanvasPointVisible(x, y))
                    continue;

                char? character = artLayer.GetCharacter(x - artLayer.OffsetX, y - artLayer.OffsetY);
                if (character != null)
                    return character;
            }

            return null;
        }

        /// <summary>
        /// Gets the visible ASCII char at the specified position
        /// </summary>
        /// <param name="point"></param>
        /// <returns>char?</returns>
        public char? GetCharacter(Point point)
            => GetCharacter((int)point.X, (int)point.Y);

        public void SetCharacter(int layerIndex, int x, int y, char? character)
        {
            if (layerIndex < 0 || layerIndex >= ArtLayers.Count)
                throw new IndexOutOfRangeException($"{nameof(layerIndex)} index outside of bounds (index: {layerIndex})");

            ArtLayer layer = ArtLayers[layerIndex];
            layer.SetCharacter(x - layer.OffsetX, y - layer.OffsetY, character);
        }

        public void SetCharacter(int layerIndex, Point point, char? character)
            => SetCharacter(layerIndex, (int)point.X, (int)point.Y, character);
        #endregion

        /// <summary>
        /// Gets the sum of the areas of all layers.
        /// </summary>
        public int GetTotalArtArea()
        {
            int area = 0;

            foreach (ArtLayer artLayer in ArtLayers)
                area += artLayer.Width * artLayer.Height;

            return area;
        }

        #region Tool Functions
        public void Crop(Rect cropRect)
        {
            SetSize((int)cropRect.Width, (int)cropRect.Height);

            if (ArtLayers.Count == 0)
                return;

            for (int i = 0; i < ArtLayers.Count; i++)
            {
                ArtLayer artLayer = ArtLayers[i];

                artLayer.Offset = new(artLayer.Offset.X - cropRect.Location.X, artLayer.Offset.Y - cropRect.Location.Y);
            }

            OnCropped?.Invoke(this);

            OnArtUpdated?.Invoke(this);

            return;
        }

        #endregion

        #region Interface implementation
        public void CopyPropertiesOf(object obj)
        {
            if (obj is not ASCIIArt toCopy)
                return;


            for (int i = 0; i < Math.Max(ArtLayers.Count, toCopy.ArtLayers.Count); i++)
            {
                if (i >= ArtLayers.Count) //Readd layer
                {
                    ArtLayer artLayer = toCopy.ArtLayers[i];
                    ArtLayers.Add(artLayer);
                    OnArtLayerAdded?.Invoke(i, artLayer);
                }
                else if (i >= toCopy.ArtLayers.Count) //Reremove layer
                {
                    ArtLayer artLayer = ArtLayers[i];
                    ArtLayers.Remove(artLayer);
                    OnArtLayerRemoved?.Invoke(i, artLayer);
                }
                else
                    ArtLayers[i].CopyPropertiesOf(toCopy.ArtLayers[i]);
            }

            SetSize(toCopy.Width, toCopy.Height);
            
            OnArtUpdated?.Invoke(this);
        }

        public object Clone()
        {
            ASCIIArt clone = new();

            clone.SetSize(Width, Height);

            for (int i = 0; i < ArtLayers.Count; i++)
                clone.ArtLayers.Add((ArtLayer)ArtLayers[i].Clone());

            return clone;
        }
        #endregion

        /// <summary>
        /// Invokes the OnArtUpdated event
        /// </summary>
        public void Update()
            => OnArtUpdated?.Invoke(this);
    }
}
