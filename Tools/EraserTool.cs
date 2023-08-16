using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class EraserTool: Tool
    {
        public override ToolType Type { get; } = ToolType.Eraser;
        
        public int Size = 1;

        public EraserTool(int size)
        {
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            EraseCharacter(startArtPos);
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            EraseCharacter(currentArtPos);
        }

        public static void EraseCharacter(Point artPos)
           => App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, null, artPos);
    }
}
