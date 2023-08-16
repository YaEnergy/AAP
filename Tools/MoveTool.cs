using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class MoveTool: Tool
    {
        public override ToolType Type { get; } = ToolType.Move;

        private Point startLayerOffset = new();
        
        public MoveTool()
        {
            
        }

        protected override void UseStart(Point startArtPos)
        {
            if (App.CurrentArt == null)
                return;
            
            startLayerOffset = App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset;
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            if (App.CurrentArt == null)
                return;

            Point newLayerOffset = new(startLayerOffset.X + (currentArtPos.X - startArtPos.X), startLayerOffset.Y + (currentArtPos.Y - startArtPos.Y));

            if (App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset == newLayerOffset) //Layer offset remains the same, don't update.
                return;

            App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset = newLayerOffset;

            App.CurrentArt.Update();
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            if (App.CurrentArt == null)
                return;

            if (App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset == startLayerOffset) //Layer offset remains the same, don't update.
                return;

            App.CurrentArt.Update();
        }
    }
}
