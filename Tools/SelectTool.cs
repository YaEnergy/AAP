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
            if (MainProgram.CurrentArt == null)
                return;

            artMatrixPosition = new(Math.Clamp(artMatrixPosition.X, 0, MainProgram.CurrentArt.Width), Math.Clamp(artMatrixPosition.Y, 0, MainProgram.CurrentArt.Height));

            int startX = Math.Clamp(artMatrixPosition.X > StartPoint.X ? StartPoint.X : artMatrixPosition.X, 0, MainProgram.CurrentArt.Width);
            int startY = Math.Clamp(artMatrixPosition.Y > StartPoint.Y ? StartPoint.Y : artMatrixPosition.Y, 0, MainProgram.CurrentArt.Height);

            int sizeX = Math.Clamp(artMatrixPosition.X > StartPoint.X ? artMatrixPosition.X - StartPoint.X + 1 : StartPoint.X - artMatrixPosition.X + 1, 0, MainProgram.CurrentArt.Width - Math.Min(StartPoint.X, artMatrixPosition.X));
            int sizeY = Math.Clamp(artMatrixPosition.Y > StartPoint.Y ? artMatrixPosition.Y - StartPoint.Y + 1 : StartPoint.Y - artMatrixPosition.Y + 1, 0, MainProgram.CurrentArt.Height - Math.Min(StartPoint.Y, artMatrixPosition.Y));

            MainProgram.Selected = new(startX, startY, sizeX, sizeY);
        }
    }
}
