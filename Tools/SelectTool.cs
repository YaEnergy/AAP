using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class SelectTool: Tool
    {
        public override void ActivateUpdate(Point artMatrixPosition)
        {
            MainProgram.Selected = new Rectangle(StartPoint, new(artMatrixPosition.X - StartPoint.X, artMatrixPosition.Y - StartPoint.Y));
        }
    }
}
