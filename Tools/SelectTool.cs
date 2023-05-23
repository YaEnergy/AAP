using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class SelectTool: Tool
    {
        public override void ActivateEnd()
        {
            MainProgram.Selected = new Rectangle(StartPoint, new(EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y));
        }
    }
}
