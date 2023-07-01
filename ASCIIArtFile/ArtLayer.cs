using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class ArtLayer
    {
        public char?[][] Data;
        public string Name = "";
        public bool Visible = true;

        public ArtLayer(string name, int width, int height)
        {
            Name = name;
            Data = new char?[width][];
            for (int x = 0; x < width; x++)
                Data[x] = new char?[height];
        }

        public string GetArtString()
        {
            string art = "";

            int width = Data.Length;

            if (width == 0)
                return art;

            int height = Data[0].Length;

            if (height == 0)
                return art;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    art += Data[x][y] == null ? ASCIIArt.EMPTYCHARACTER : Data[x][y].ToString();

                art += "\n";
            }

            return art;
        }

    }

    public struct ArtLayerFile
    {
        public string ArtLayerString;
        public string Name = "";
        public bool Visible = true;

        public ArtLayerFile(string name, bool visible, string artLayerString)
        {
            Name = name;
            Visible = visible;
            ArtLayerString = artLayerString;
        }
    }
}
