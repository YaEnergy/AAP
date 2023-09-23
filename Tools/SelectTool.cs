using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class SelectTool: Tool
    {
        public override ToolType Type { get; } = ToolType.Select;

        public SelectTool() 
        {
            
        }

        protected override void UseStart(Point startArtPos)
        {
            if (App.CurrentArtFile == null)
                return;

            Select(startArtPos, startArtPos);
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            if (App.CurrentArtFile == null)
                return;

            Select(startArtPos, currentArtPos);
        }

        public static void Select(Point startArtPos, Point endArtPos)
        {
            if (App.CurrentArtFile == null)
                return;

            //Keep points within canvas
            startArtPos = new(Math.Clamp(startArtPos.X, 0, App.CurrentArtFile.Art.Width - 1), Math.Clamp(startArtPos.Y, 0, App.CurrentArtFile.Art.Height - 1));
            endArtPos = new(Math.Clamp(endArtPos.X, 0, App.CurrentArtFile.Art.Width - 1), Math.Clamp(endArtPos.Y, 0, App.CurrentArtFile.Art.Height - 1));

            int startX = (int)Math.Clamp(endArtPos.X > startArtPos.X ? startArtPos.X : endArtPos.X, 0, App.CurrentArtFile.Art.Width - 1);
            int startY = (int)Math.Clamp(endArtPos.Y > startArtPos.Y ? startArtPos.Y : endArtPos.Y, 0, App.CurrentArtFile.Art.Height - 1);

            int sizeX = (int)Math.Clamp(endArtPos.X > startArtPos.X ? endArtPos.X - startArtPos.X + 1 : startArtPos.X - endArtPos.X + 1, 0, App.CurrentArtFile.Art.Width - Math.Min(startArtPos.X, endArtPos.X));
            int sizeY = (int)Math.Clamp(endArtPos.Y > startArtPos.Y ? endArtPos.Y - startArtPos.Y + 1 : startArtPos.Y - endArtPos.Y + 1, 0, App.CurrentArtFile.Art.Height - Math.Min(startArtPos.Y, endArtPos.Y));

            App.SelectedArt = new(startX, startY, sizeX, sizeY);
        }
    }
}
