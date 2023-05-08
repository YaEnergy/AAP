using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class ArtLayer
    {
        public char[][] Data;
        public string Name = "";

        public ArtLayer(string name, Size size) 
        {
            Name = name;
            Data = new char[size.Width][];
            for (int x = 0; x < size.Width; x++)
                Data[x] = new char[size.Height];
        }
    }
}
