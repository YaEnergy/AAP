﻿using AAP.Timelines;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;

namespace AAP
{
    [Serializable]
    public class ArtLayer : ITimelineObject, INotifyPropertyChanged
    {
        private char?[,] data;
        public char?[,] Data
        {
            get => data;
            set
            {
                if (data == value)
                    return;

                char?[,] old = data;
                data = value;

                DataChanged?.Invoke(this, old, data);
                PropertyChanged?.Invoke(this, new(nameof(Data)));
            }
        }

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                if (name == value) 
                    return;

                name = value;

                NameChanged?.Invoke(this, value);
                PropertyChanged?.Invoke(this, new(nameof(Name)));
            }
        }

        private bool visible = true;
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible == value)
                    return;

                visible = value;

                VisibilityChanged?.Invoke(this, value);
                PropertyChanged?.Invoke(this, new(nameof(Visible)));
            }
        }

        private int offsetX = 0;
        public int OffsetX
        {
            get => offsetX;
            set
            {
                if (offsetX == value)
                    return;

                Point oldOffset = new(OffsetX, OffsetY);

                offsetX = value;
                OffsetChanged?.Invoke(this, oldOffset, Offset);
                PropertyChanged?.Invoke(this, new(nameof(OffsetX)));
            }
        }

        private int offsetY = 0;
        public int OffsetY
        {
            get => offsetY;
            set
            {
                if (offsetY == value)
                    return;

                Point oldOffset = new(OffsetX, OffsetY);

                offsetY = value;
                OffsetChanged?.Invoke(this, oldOffset, Offset);
                PropertyChanged?.Invoke(this, new(nameof(OffsetY)));
            }
        }

        [JsonIgnore]
        public Point Offset
        {
            get => new(OffsetX, OffsetY);
            set
            {
                Point oldOffset = new(OffsetX, OffsetY);

                bool changed = false;
                
                if (offsetX != value.X)
                {
                    changed = true;
                    offsetX = (int)value.X;
                    PropertyChanged?.Invoke(this, new(nameof(OffsetX)));
                }

                if (offsetY != value.Y)
                {
                    changed = true;
                    offsetY = (int)value.Y;
                    PropertyChanged?.Invoke(this, new(nameof(OffsetY)));
                }

                if (changed)
                    OffsetChanged?.Invoke(this, oldOffset, Offset);
            }
        }

        public int Width
        {
            get => data.GetLength(0);
        }

        public int Height
        {
            get => data.GetLength(1);
        }
        
        public Size Size
        {
            get => new(Width, Height);
        }

        public delegate void DataChangedEvent(ArtLayer layer, char?[,] oldData, char?[,] newData);
        public event DataChangedEvent? DataChanged;

        public delegate void NameChangedEvent(ArtLayer layer, string name);
        public event NameChangedEvent? NameChanged;

        public delegate void VisibilityChangedEvent(ArtLayer layer, bool visibility);
        public event VisibilityChangedEvent? VisibilityChanged;

        public delegate void OffsetChangedEvent(ArtLayer layer, Point oldOffset, Point newOffset);
        public event OffsetChangedEvent? OffsetChanged;

        public delegate void CharacterChangedEvent(ArtLayer layer, int x, int y);
        public event CharacterChangedEvent? CharacterChanged;

        public delegate void CroppedEvent(ArtLayer layer, Rect oldRect, Rect newRect);
        public event CroppedEvent? Cropped;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Constructor used by the JsonDeserializer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        [JsonConstructor]
        public ArtLayer(string name, char?[,] data, int offsetX = 0, int offsetY = 0)
        {
            Name = name;
            this.data = data;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }
        
        public ArtLayer(string name, int width, int height, int offsetX = 0, int offsetY = 0)
        {
            if (width <= 0)
                throw new Exception("Art Layer Constructor - width can not be smaller than or equal to 0!");

            if (height <= 0)
                throw new Exception("Art Layer Constructor - height can not be smaller than or equal to 0!");

            Name = name;
            data = new char?[width, height];

            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }

        public ArtLayer(string name, Size size, Point offset)
        {
            if ((int)size.Width <= 0)
                throw new Exception("Art Layer Constructor - (int)size.Width can not be smaller than or equal to 0!");

            if ((int)size.Height <= 0)
                throw new Exception("Art Layer Constructor - (int)size.Height can not be smaller than or equal to 0!");

            Name = name;
            data = new char?[(int)size.Width, (int)size.Height];

            offsetX = (int)offset.X;
            offsetY = (int)offset.Y;
        }

        #region Drawing
        public char? GetCharacter(int x, int y)
        {
            if (x < 0 || x >= Width)
                throw new IndexOutOfRangeException($"{nameof(x)} outside of bounds of layer (x: {x})");

            if (y < 0 || y >= Height)
                throw new IndexOutOfRangeException($"{nameof(y)} outside of bounds of layer (y: {y})");

            return Data[x, y];
        }

        public char? GetCharacter(Point point)
            => GetCharacter((int)point.X, (int)point.Y);

        public void SetCharacter(int x, int y, char? character)
        {
            if (x < 0 || x >= Width)
                throw new IndexOutOfRangeException($"{nameof(x)} outside of bounds of layer (x: {x})");

            if (y < 0 || y >= Height)
                throw new IndexOutOfRangeException($"{nameof(y)} outside of bounds of layer (y: {y})");

            if (Data[x, y] != character)
                Data[x, y] = character;

            CharacterChanged?.Invoke(this, x, y);
        }

        public void SetCharacter(Point point, char? character)
            => SetCharacter((int)point.X, (int)point.Y, character);
        #endregion

        public void Crop(Rect cropRect)
        {
            if ((int)cropRect.Width <= 0)
                throw new Exception("Art Layer Crop() - (int)cropRect.Width can not be smaller than or equal to 0!");

            if ((int)cropRect.Height <= 0)
                throw new Exception("Art Layer Crop() - (int)cropRect.Height can not be smaller than or equal to 0!");

            Rect oldArtRect = new(offsetX, offsetY, Width, Height);

            int difOffsetX = offsetX - (int)cropRect.X;
            int difOffsetY = offsetY - (int)cropRect.Y;

            char?[,] newData = new char?[(int)cropRect.Width, (int)cropRect.Height];
            for (int x = 0; x < (int)cropRect.Width; x++)
            {
                if (x - difOffsetX >= Width || x - difOffsetX < 0)
                    continue;

                for (int y = 0; y < (int)cropRect.Height; y++)
                {
                    if (y - difOffsetY < 0)
                        continue;

                    if (y - difOffsetY >= Height)
                        break;

                    newData[x, y] = data[x - difOffsetX, y - difOffsetY] ?? null;
                }
            }

            Data = newData;
            Offset = cropRect.Location;

            Cropped?.Invoke(this, oldArtRect, cropRect);
        }

        public void MergeDown(ArtLayer mergeLayer)
        {
            Rect mergedLayerRect = Rect.Union(new Rect(Offset, Size), new Rect(mergeLayer.Offset, mergeLayer.Size));
            Crop(mergedLayerRect);

            for (int x = 0; x < mergeLayer.Width; x++)
                for (int y = 0; y < mergeLayer.Height; y++)
                    if (GetCharacter(x - OffsetX + mergeLayer.OffsetX, y - OffsetY + mergeLayer.OffsetY) == null)
                        SetCharacter(x - OffsetX + mergeLayer.OffsetX, y - OffsetY + mergeLayer.OffsetY, mergeLayer.GetCharacter(x, y));
        }

        public object Clone()
        {
            ArtLayer cloneLayer = new(Name, Width, Height, OffsetX, OffsetY);

            if (Width == 0 || Height == 0)
                return cloneLayer;

            cloneLayer.Visible = Visible;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    cloneLayer.SetCharacter(x, y, GetCharacter(x, y));

            return cloneLayer;
        }

        public string GetArtString()
        {
            string art = "";

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    art += GetCharacter(x, y) == null ? ASCIIArt.EMPTYCHARACTER : GetCharacter(x, y).ToString();

                art += "\n";
            }

            return art;
        }

        /// <summary>
        /// Returns true if the canvas point is visible on this art layer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsCanvasPointVisible(int x, int y)
            => x - OffsetX >= 0 && x - OffsetX < Width && y - OffsetY >= 0 && y - OffsetY < Height;

        /// <summary>
        /// Returns true if the canvas point is visible on this art layer
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsCanvasPointVisible(Point point)
            => IsCanvasPointVisible((int)point.X, (int)point.Y);

        /// <summary>
        /// Returns true if the layer point is visible on this art layer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsLayerPointVisible(int x, int y)
            => x >= 0 && x < Width && y >= 0 && y < Height;

        /// <summary>
        /// Returns true if the layer point is visible on this art layer
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsLayerPointVisible(Point point)
            => IsCanvasPointVisible((int)point.X, (int)point.Y);

        /// <summary>
        /// Converts points on canvas to points on layer.
        /// </summary>
        /// <param name="x">X-position on canvas</param>
        /// <param name="y">Y-Position on canvas</param>
        /// <returns>Point on layer</returns>
        public Point GetLayerPoint(int x, int y)
            => new(x - OffsetX, y - OffsetY);

        /// <summary>
        /// Converts points on canvas to points on layers.
        /// </summary>
        /// <param name="point">Position on canvas</param>
        /// <returns>Point on layer</returns>
        public Point GetLayerPoint(Point point)
            => new((int)point.X - OffsetX, (int)point.Y - OffsetY);

        /// <summary>
        /// Converts points on layers to points on canvas
        /// </summary>
        /// <param name="x">X-position on layer</param>
        /// <param name="y">Y-position on layer</param>
        /// <returns>Point on canvas</returns>
        public Point GetCanvasPoint(int x, int y)
            => new(x + OffsetX, y + OffsetY);

        /// <summary>
        /// Converts points on layers to points on canvas.
        /// </summary>
        /// <param name="point">Position on layer</param>
        /// <returns>Point on canvas</returns>
        public Point GetCanvasPoint(Point point)
            => new((int)point.X + OffsetX, (int)point.Y + OffsetY);

        public override string ToString()
            => Name;

        public void CopyPropertiesOf(object obj)
        {
            if (obj is not ArtLayer layer)
                throw new ArgumentException("obj is not layer!");

            Name = layer.Name;
            Data = layer.Data;
            Offset = layer.Offset;
            Visible = layer.Visible;
        }
    }
}
