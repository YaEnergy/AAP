using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class ArtLayer
    {
        public char?[][] Data;
        public string Name = "";

        public ArtLayer(string name, int width, int height) 
        {
            Name = name;
            Data = new char?[width][];
            for (int x = 0; x < width; x++)
                Data[x] = new char?[height];
        }
    }
}
