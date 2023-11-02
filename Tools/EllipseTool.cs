using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class EllipseTool: Tool, ICharacterSelectable, ISizeSelectable, IStayInsideSelectionProperty, IFillProperty, IPreviewable<ArtLayer?>, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Ellipse;

        private int size = 1;
        public int Size
        {
            get => size;
            set
            {
                if (size == value)
                    return;

                size = value;

                PropertyChanged?.Invoke(this, new(nameof(Size)));
            }
        }

        private char? character = '/';
        public char? Character
        {
            get => character;
            set
            {
                if (character == value)
                    return;

                character = value;

                PropertyChanged?.Invoke(this, new(nameof(Character)));
            }
        }

        private bool stayInsideSelection = false;
        public bool StayInsideSelection
        {
            get => stayInsideSelection;
            set
            {
                if (stayInsideSelection == value)
                    return;

                stayInsideSelection = value;

                PropertyChanged?.Invoke(this, new(nameof(StayInsideSelection)));
            }
        }

        private bool fill = false;
        public bool Fill
        {
            get => fill;
            set
            {
                if (fill == value)
                    return;

                fill = value;

                PropertyChanged?.Invoke(this, new(nameof(Fill)));
            }
        }

        private ArtLayer? preview = null;
        public ArtLayer? Preview
        {
            get => preview;
            set
            {
                if (preview == value)
                    return;

                preview = value;

                PropertyChanged?.Invoke(this, new(nameof(Preview)));
                OnPreviewChanged?.Invoke(preview);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PreviewChangedEvent? OnPreviewChanged;

        public EllipseTool(char? character, int size)
        {
            Character = character;
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            UpdatePreview(startArtPos, startArtPos);
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            UpdatePreview(startArtPos, currentArtPos);
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            Preview = null;

            DrawEllipse(startArtPos, endArtPos);

            App.CurrentArtFile?.Art.Update();
            App.CurrentArtFile?.ArtTimeline.NewTimePoint();
        }

        public void DrawEllipse(Point startArtPos, Point endArtPos)
        {
            if (App.CurrentArtFile == null)
                return;

            App.CurrentArtFile.ArtDraw.StayInsideSelection = StayInsideSelection;
            App.CurrentArtFile.ArtDraw.BrushThickness = Size - 1;

            int centerX = (int)startArtPos.X;
            int centerY = (int)startArtPos.Y;

            int radiusX = (int)(endArtPos.X - startArtPos.X);
            int radiusY = (int)(endArtPos.Y - startArtPos.Y);

            App.CurrentArtFile.ArtDraw.DrawEllipse(App.CurrentLayerID, Character, centerX, centerY, radiusX, radiusY, Fill);
        }

        public void UpdatePreview(Point startArtPos, Point endArtPos)
        {
            if (App.CurrentArtFile == null)
            {
                Preview = null;
                return;
            }

            ArtLayer layer = App.CurrentArtFile.Art.ArtLayers[App.CurrentLayerID];

            int offset = Size == 1 ? 0 : Math.Max(Size - 2, 1);

            int centerX = (int)startArtPos.X;
            int centerY = (int)startArtPos.Y;

            int radiusX = (int)Math.Max(endArtPos.X - startArtPos.X, startArtPos.X - endArtPos.X);
            int radiusY = (int)Math.Max(endArtPos.Y - startArtPos.Y, startArtPos.Y - endArtPos.Y);

            ArtLayer previewLayer = new("Preview", Math.Min(radiusX * 2 + offset * 2 + 1, layer.Width), Math.Min(radiusY * 2 + offset * 2 + 1, layer.Height), Math.Max(centerX - radiusX - offset, layer.OffsetX), Math.Max(centerY - radiusY - offset, layer.OffsetY));

            ArtLayerDraw layerDraw = new(previewLayer)
            {
                BrushThickness = Size - 1,
                StayInsideSelection = StayInsideSelection
            };

            layerDraw.DrawEllipse(Character, centerX - previewLayer.OffsetX, centerY - previewLayer.OffsetY, radiusX, radiusY, Fill);

            Preview = previewLayer;
        }
    }
}
