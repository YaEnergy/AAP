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
        
        /// <summary>
        /// Constructor used by the JsonDeserializer
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        [JsonConstructor]
        public ArtLayer(string name, char?[][] data)
        {
            Name = name;
            Data = data;
        }
        
        public ArtLayer(string name, int width, int height)
        {
            if (width <= 0)
                throw new Exception("Art Layer Constructor - width can not be smaller than or equal to 0!");

            if (height <= 0)
                throw new Exception("Art Layer Constructor - height can not be smaller than or equal to 0!");

            Name = name;
            Data = new char?[width][];
            for (int x = 0; x < width; x++)
                Data[x] = new char?[height];
        }

        public object Clone()
        {
            ArtLayer cloneLayer = new(Name, Width, Height);

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

    }
}
