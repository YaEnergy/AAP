using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class DrawTool: Tool
    {
        public char Character = '#';
        public int Size = 1;
        public override void CanvasClick_Start(Point location)
        {
            base.CanvasClick_Start(location);
        }
    }
}
