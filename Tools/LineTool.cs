using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class LineTool: Tool, ICharacterSelectable
    {
        public override ToolType Type { get; } = ToolType.Line;

        public char? Character { get; set; } = '/';

        public int Size = 1;

        public LineTool(char? character)
        {
            Character = character;   
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawCharacter(startArtPos);
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            DrawLine(startArtPos, endArtPos);
        }

        public void DrawCharacter(Point artPos)
            => App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, Character, artPos);

        public void DrawLine(Point startArtPos, Point endArtPos)
            => App.CurrentArtDraw?.DrawLine(App.CurrentLayerID, Character, startArtPos, endArtPos);
    }
}
