using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class SelectTool: Tool
    {
        public SelectTool() 
        {
            Type = ToolType.Select;
        }

        public override void ActivateUpdate(Point artMatrixPosition)
        {
            if (App.CurrentArt == null)
                return;

            base.ActivateUpdate(artMatrixPosition);

            artMatrixPosition = new(Math.Clamp(artMatrixPosition.X, 0, App.CurrentArt.Width), Math.Clamp(artMatrixPosition.Y, 0, App.CurrentArt.Height));

            int startX = (int)Math.Clamp(artMatrixPosition.X > StartPoint.X ? StartPoint.X : artMatrixPosition.X, 0, App.CurrentArt.Width);
            int startY = (int)Math.Clamp(artMatrixPosition.Y > StartPoint.Y ? StartPoint.Y : artMatrixPosition.Y, 0, App.CurrentArt.Height);

            int sizeX = (int)Math.Clamp(artMatrixPosition.X > StartPoint.X ? artMatrixPosition.X - StartPoint.X + 1 : StartPoint.X - artMatrixPosition.X + 1, 0, App.CurrentArt.Width - Math.Min(StartPoint.X, artMatrixPosition.X));
            int sizeY = (int)Math.Clamp(artMatrixPosition.Y > StartPoint.Y ? artMatrixPosition.Y - StartPoint.Y + 1 : StartPoint.Y - artMatrixPosition.Y + 1, 0, App.CurrentArt.Height - Math.Min(StartPoint.Y, artMatrixPosition.Y));

            App.Selected = new(startX, startY, sizeX, sizeY);
        }
    }
}
