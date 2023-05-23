using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public enum MoveToolMode
    {
        Layer,
        Select
    }
    public class MoveTool: Tool
    {
        public MoveToolMode Mode = MoveToolMode.Layer;

        public MoveTool(MoveToolMode mode)
        {
            Mode = mode;
        }

        public override void ActivateStart(Point location)
        {
            base.ActivateStart(location);
        }
    }
}
