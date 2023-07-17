using AAP.Timelines;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class ArtLayer : ICloneable
    {
        public char?[][] Data;

        public string Name = "";
        public bool Visible = true;

        private int offsetX = 0;
        public int OffsetX
        {
            get => offsetX;
            set
            {
                if (offsetX == value)
                    return;

                offsetX = value;
                OffsetChanged?.Invoke(this, OffsetX, OffsetY);
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

                offsetY = value;
                OffsetChanged?.Invoke(this, OffsetX, OffsetY);
            }
        }

        [JsonIgnore]
        public Point Offset
        {
            get => new(OffsetX, OffsetY);
            set
            {
                bool changed = false;
                
                if (offsetX != value.X)
                {
                    changed = true;
                    offsetX = (int)value.X;
                }

                if (offsetY != value.Y)
                {
                    changed = true;
                    offsetY = (int)value.Y;
                }

                if (changed)
                    OffsetChanged?.Invoke(this,OffsetX, OffsetY);
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

        public delegate void OffsetChangedEvent(ArtLayer layer, int offsetX, int offsetY);
        public event OffsetChangedEvent? OffsetChanged;
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

        public object Clone()
        {
            ArtLayer cloneLayer = new(Name, Width, Height, OffsetX);

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
            if (x - OffsetX < 0 || y - OffsetY < 0 || x - OffsetX > Width || y - OffsetY > Height) //Is point outside layer
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
        /// Converts points on layers to points on canvas
        /// </summary>
        /// <param name="x">X-position on layer</param>
        /// <param name="y">Y-position on layer</param>
        /// <returns>Point on canvas</returns>
        public Point GetCanvasPoint(int x, int y)
            => new(x + OffsetX, y + OffsetY);
    }
}
