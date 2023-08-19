using AAP.Timelines;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AAP
{
    [Serializable]
    public class ArtLayer : ITimelineObject, INotifyPropertyChanged
    {
        private char?[][] data = new char?[1][];
        public char?[][] Data
        {
            get => data;
            set
            {
                if (data == value)
                    return;

                char?[][] oldData = data;
                data = value;
                DataChanged?.Invoke(this, oldData, value);
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
            get => Data.Length;
        }

        public int Height
        {
            get
            {
                if(Width == 0)
                    throw new Exception("ArtLayer - Can't get height, width is 0.");

                for (int i = 0; i < Data.Length; i++)
                    if (Data[i] != null)
                        return Data[i].Length;

                throw new Exception("ArtLayer - Data does not contain any char? arrays!");
            }
        }
        
        public Size Size
        {
            get => new(Width, Height);
        }

        public delegate void DataChangedEvent(ArtLayer layer, char?[][] oldData, char?[][] newData);
        public event DataChangedEvent? DataChanged;

        public delegate void NameChangedEvent(ArtLayer layer, string name);
        public event NameChangedEvent? NameChanged;

        public delegate void VisibilityChangedEvent(ArtLayer layer, bool visibility);
        public event VisibilityChangedEvent? VisibilityChanged;

        public delegate void OffsetChangedEvent(ArtLayer layer, Point oldOffset, Point newOffset);
        public event OffsetChangedEvent? OffsetChanged;

        public delegate void CroppedEvent(ArtLayer layer, Rect oldRect, Rect newRect);
        public event CroppedEvent? Cropped;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Constructor used by the JsonDeserializer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        [JsonConstructor]
        public ArtLayer(string name, char?[][] data, int offsetX = 0, int offsetY = 0)
        {
            Name = name;
            Data = data;
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
            Data = new char?[width][];
            for (int x = 0; x < width; x++)
                Data[x] = new char?[height];

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
            Data = new char?[(int)size.Width][];
            for (int x = 0; x < (int)size.Width; x++)
                Data[x] = new char?[(int)size.Height];

            offsetX = (int)offset.X;
            offsetY = (int)offset.Y;
        }

        public void Crop(Rect cropRect)
        {
            if ((int)cropRect.Width <= 0)
                throw new Exception("Art Layer Crop() - (int)cropRect.Width can not be smaller than or equal to 0!");

            if ((int)cropRect.Height <= 0)
                throw new Exception("Art Layer Crop() - (int)cropRect.Height can not be smaller than or equal to 0!");

            Rect oldArtRect = new(offsetX, offsetY, Width, Height);

            int difOffsetX = offsetX - (int)cropRect.X;
            int difOffsetY = offsetY - (int)cropRect.Y;

            char?[][] newData = new char?[(int)cropRect.Width][];
            for (int x = 0; x < (int)cropRect.Width; x++)
            {
                newData[x] = new char?[(int)cropRect.Height];

                if (x - difOffsetX >= Width || x - difOffsetX < 0)
                    continue;

                for (int y = 0; y < (int)cropRect.Height; y++)
                {
                    if (y - difOffsetY < 0)
                        continue;

                    if (y - difOffsetY >= Height)
                        break;

                    newData[x][y] = Data[x - difOffsetX][y - difOffsetY] ?? null;
                }
            }

            Data = newData;
            Offset = cropRect.Location;

            Cropped?.Invoke(this, oldArtRect, cropRect);
        }

        public void Merge(ArtLayer mergeLayer)
        {
            Rect mergedLayerRect = Rect.Union(new Rect(Offset, Size), new Rect(mergeLayer.Offset, mergeLayer.Size));
            Crop(mergedLayerRect);

            for (int x = 0; x < mergeLayer.Width; x++)
                for (int y = 0; y < mergeLayer.Height; y++)
                    if (Data[x - OffsetX + mergeLayer.OffsetX][y - OffsetY + mergeLayer.OffsetY] == null)
                        Data[x - OffsetX + mergeLayer.OffsetX][y - OffsetY + mergeLayer.OffsetY] = mergeLayer.Data[x][y];
        }

        public object Clone()
        {
            ArtLayer cloneLayer = new(Name, Width, Height, OffsetX, OffsetY);

            if (Width == 0 || Height == 0)
                return cloneLayer;

            cloneLayer.Visible = Visible;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    cloneLayer.Data[x][y] = Data[x][y];

            return cloneLayer;
        }

        public string GetArtString()
        {
            string art = "";

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    art += Data[x][y] == null ? ASCIIArt.EMPTYCHARACTER : Data[x][y].ToString();

                art += "\n";
            }

            return art;
        }

        public bool IsPointVisible(int x, int y)
        {
            if (x - OffsetX < 0 || y - OffsetY < 0 || x - OffsetX >= Width || y - OffsetY >= Height) //Is point outside layer
                return false;

            return true;
        }

        public bool IsPointVisible(Point point)
        {
            if ((int)point.X - OffsetX < 0 || (int)point.Y - OffsetY < 0 || (int)point.X - OffsetX >= Width || (int)point.Y - OffsetY >= Height) //Is point outside layer
                return false;

            return true;
        }

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
