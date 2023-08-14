using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class MoveTool: Tool
    {
        public override ToolType Type { get; protected set; } = ToolType.Move;

        private Point startLayerOffset = new();
        
        public MoveTool()
        {
            
        }


        public override void ActivateStart(Point location)
        {
            if (App.CurrentArt == null)
                return;

            base.ActivateStart(location);
            startLayerOffset = App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset;
        }

        public override void ActivateUpdate(Point location)
        {
            if (App.CurrentArt == null)
                return;

            base.ActivateUpdate(location);

            Point newLayerOffset = new(startLayerOffset.X + (location.X - StartPoint.X), startLayerOffset.Y + (location.Y - StartPoint.Y));

            if (App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset == newLayerOffset) //Layer offset remains the same, don't update.
                return;

            App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset = newLayerOffset;

            App.CurrentArt.Update();
        }

        public override void ActivateEnd()
        {
            if (App.CurrentArt == null)
                return;

            base.ActivateEnd();

            if (App.CurrentArt.ArtLayers[App.CurrentLayerID].Offset == startLayerOffset) //Layer offset remains the same, don't update.
                return;

            App.CurrentArt.Update();
        }
    }
}
