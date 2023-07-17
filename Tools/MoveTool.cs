using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class MoveTool: Tool
    {
        public MoveTool()
        {
            Type = ToolType.Move;
        }

        public override void ActivateStart(Point location)
        {
            base.ActivateStart(location);
        }
    }
}
