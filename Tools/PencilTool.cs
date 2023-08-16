using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class PencilTool: Tool, ICharacterSelectable
    {
        public override ToolType Type { get; } = ToolType.Draw;
       
        public int Size = 1;

        public char? Character { get; set; } = '/';

        public PencilTool(char? character, int size)
        {
            Character = character;
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawCharacter(startArtPos);
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            DrawCharacter(currentArtPos);
        }

        public void DrawCharacter(Point artPos)
            => App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, Character, artPos);
    }
}
