using AAP.Timelines;
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

        private int width = 0;
        private int height = 0;

        public ArtLayer(string name, int width, int height)
        {
            this.width = width;
            this.height = height;

            Name = name;
            Data = new char?[width][];
            for (int x = 0; x < width; x++)
                Data[x] = new char?[height];
        }

        public object Clone()
        {
            ArtLayer cloneLayer = new(Name, width, height);

            if (width == 0 || height == 0)
                return cloneLayer;

            cloneLayer.Visible = Visible;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    cloneLayer.Data[x][y] = Data[x][y];

            return cloneLayer;
        }

        public string GetArtString()
        {
            string art = "";

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    art += Data[x][y] == null ? ASCIIArt.EMPTYCHARACTER : Data[x][y].ToString();

                art += "\n";
            }

            return art;
        }

    }
}
