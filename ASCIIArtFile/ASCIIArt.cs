﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AAP.Timelines;
using Newtonsoft.Json;

namespace AAP
{
    public class ASCIIArt : ITimelineObject
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
                UnsavedChanges = true;
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
                UnsavedChanges = true;
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

        public delegate void ArtLayerAddedEvent(int index, ArtLayer artLayer);
        /// <summary>
        /// Invoked when a layer is added.
        /// </summary>
        public event ArtLayerAddedEvent? OnArtLayerAdded;

        public delegate void ArtLayerRemovedEvent(int index, ArtLayer artLayer);
        /// <summary>
        /// Invoked when a layer is removed.
        /// </summary>
        public event ArtLayerRemovedEvent? OnArtLayerRemoved;

        public delegate void ArtLayerPropertiesChangedEvent(int index, ArtLayer artLayer, bool updateCanvas = false);
        /// <summary>
        /// Invoked when a layer has their properties changed.
        /// </summary>
        public event ArtLayerPropertiesChangedEvent? OnArtLayerPropertiesChanged;

        public delegate void UnsavedChangesChangedEvent(ASCIIArt art, bool unsavedChanges);
        /// <summary>
        /// Invoked when the UnsavedChanges bool changes.
        /// </summary>
        public event UnsavedChangesChangedEvent? OnUnsavedChangesChanged;

        public event ITimelineObject.CopiedPropertiesOfEvent? OnCopiedPropertiesOf;

        private bool unsavedChanges = false;
        [JsonIgnore]
        public bool UnsavedChanges
        {
            get => unsavedChanges;
            set 
            { 
                unsavedChanges = value;

                OnUnsavedChangesChanged?.Invoke(this, unsavedChanges);
                OnChanged?.Invoke(this);
            }
        }

        public delegate void ChangedEvent(ASCIIArt art);
        /// <summary>
        /// Invoked every time the art changes.
        /// </summary>
        public event ChangedEvent? OnChanged;

        public ASCIIArt()
        {

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
            }

            if (this.width != width)
            {
                this.width = width;
                changedSize = true;
            }

            if (changedSize)
            {
                UnsavedChanges = true;
                OnSizeChanged?.Invoke(width, height);
            }
        }

        #region Layers
        public void InsertLayer(int index, ArtLayer artLayer)
        {
            ArtLayers.Insert(index, artLayer);
            OnArtLayerAdded?.Invoke(index, ArtLayers[index]);

            artLayer.OffsetChanged += OnArtLayerOffsetChanged;//Listen to art layer offset changes

            UnsavedChanges = true;
        }

        public void AddLayer(ArtLayer artLayer)
        {
            ArtLayers.Add(artLayer);
            OnArtLayerAdded?.Invoke(ArtLayers.Count - 1, artLayer);

            artLayer.OffsetChanged += OnArtLayerOffsetChanged;
            //Listen to art layer offset changes

            UnsavedChanges = true;
        }

        public void RemoveLayer(int index)
        {
            if (ArtLayers.Count <= index) 
                return;

            ArtLayer artLayer = ArtLayers[index];
            ArtLayers.RemoveAt(index);
            OnArtLayerRemoved?.Invoke(index, artLayer);

            artLayer.OffsetChanged -= OnArtLayerOffsetChanged;//Stop listening to art layer offset changes

            UnsavedChanges = true;
        }

        public void SetLayerIndexName(int index, string layerName)
        {
            if (ArtLayers.Count <= index)
                return;

            ArtLayers[index].Name = layerName;
            OnArtLayerPropertiesChanged?.Invoke(index, ArtLayers[index], false);

            UnsavedChanges = true;
        }

        public void SetLayerIndexVisibility(int index, bool visible)
        {
            if (ArtLayers.Count <= index)
                return;

            ArtLayers[index].Visible = visible;
            OnArtLayerPropertiesChanged?.Invoke(index, ArtLayers[index], true);

            UnsavedChanges = true;
        }
        #endregion

        /// <summary>
        /// Gets all line strings in the art and adds them together, each line is separated by a linebreak character.
        /// </summary>
        /// <param name="bgWorker"></param>
        /// <returns>Full ASCII art string, each line separated by \n</returns>
        public string GetArtString(BackgroundWorker? bgWorker = null)
        {
            string art = "";

            for (int y = 0; y < Height; y++)
                art += GetLineString(y, bgWorker) + "\n";

            return art;
        }

        /// <summary>
        /// Gets the visible ASCII text of a single line.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="bgWorker"></param>
        /// <returns>String of line (y)</returns>
        public string GetLineString(int y, BackgroundWorker? bgWorker = null)
        {
            char?[] visibleArtLineArray = new char?[Width];
            string line = "";

            if (Width == 0)
                return line;

            for (int i = 0; i < ArtLayers.Count; i++)
            {
                ArtLayer artLayer = ArtLayers[i];
                if (artLayer.Visible)
                    for (int x = 0; x < Width; x++)
                    {
                        if (x > artLayer.OffsetX + artLayer.Width) //from here on x-positions will never be on the canvas
                            break;

                        if (!artLayer.IsPointVisible(x, y))
                            continue;

                        char? character = artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY];

                        if (character == null)
                            continue;

                        visibleArtLineArray[x] = character.Value;
                    }
            }

            for (int x = 0; x < Width; x++)
                line += visibleArtLineArray[x] == null ? EMPTYCHARACTER : visibleArtLineArray[x].ToString();

            return line;
        }

        #region Tool Functions
        public void Crop(Rect cropRect)
        {
            SetSize((int)cropRect.Width, (int)cropRect.Height);

            if (ArtLayers.Count == 0)
                return;

            //List<ArtLayer> layers = new();

            for (int i = 0; i < ArtLayers.Count; i++)
            {
                ArtLayer artLayer = ArtLayers[i];

                artLayer.Offset = new(artLayer.Offset.X - cropRect.Location.X, artLayer.Offset.Y - cropRect.Location.Y);
                /*ArtLayer newArtLayer = new(ArtLayers[i].Name, Width, Height);

                newArtLayer.Visible = ArtLayers[i].Visible;

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        newArtLayer.Data[x][y] = ArtLayers[i].Data[(int)cropRect.X + x][(int)cropRect.Y + y];

                layers.Add(newArtLayer);*/
            }

            //ArtLayers = layers;

            OnCropped?.Invoke(this);

            UnsavedChanges = true;

            return;
        }

        #endregion

        #region Interface implementation
        public void CopyPropertiesOf(object obj)
        {
            if (obj is not ASCIIArt toCopy)
                return;
            
            ArtLayers.Clear();

            for (int i = 0; i < toCopy.ArtLayers.Count; i++)
                ArtLayers.Add((ArtLayer)toCopy.ArtLayers[i].Clone());

            SetSize(toCopy.Width, toCopy.Height);

            UnsavedChanges = true;

            OnCopiedPropertiesOf?.Invoke(obj);
        }

        public object Clone()
        {
            ASCIIArt clone = new();

            clone.UnsavedChanges = UnsavedChanges;
            clone.SetSize(Width, Height);

            for (int i = 0; i < ArtLayers.Count; i++)
                clone.ArtLayers.Add((ArtLayer)ArtLayers[i].Clone());

            return clone;
        }
        #endregion

        #region Events
        private void OnArtLayerOffsetChanged(ArtLayer artLayer, Point oldOffset, Point newOffset)
            => UnsavedChanges = true;
        #endregion
    }
}
