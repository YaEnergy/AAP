using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class DrawTool: Tool
    {
        public char? Character = '/';
        public int Size = 1;

        public DrawTool(ToolType type, char? character, int size)
        {
            Type = type;
            Character = character;
            Size = size;
        }

        public override void ActivateStart(Point artMatrixPosition)
        {
            base.ActivateStart(artMatrixPosition);

            Draw(artMatrixPosition);
        }

        public override void ActivateUpdate(Point artMatrixPosition)
        {
            Draw(artMatrixPosition);
        }

        public void Draw(Point artMatrixPosition)
            => App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, Character, artMatrixPosition);//MainProgram.CurrentArt?.Draw(MainProgram.CurrentLayerID, artMatrixPosition, Character);
    }
}
