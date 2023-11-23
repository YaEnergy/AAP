using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class RectangleTool: Tool, ICharacterSelectable, ISizeSelectable, IStayInsideSelectionProperty, IFillProperty, IPreviewable<ArtLayer?>, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Rectangle;

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

        private ArtLayerDraw? layerDraw = null;

        private Point lastPreviewStartArtPos = new(-1, -1);
        private Point lastPreviewEndArtPos = new(-1, -1);

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PreviewChangedEvent? OnPreviewChanged;

        public RectangleTool(char? character, int size)
        {
            Character = character;
            Size = size;

            App.OnCurrentLayerIDChanged += OpenLayerDraw;
        }

        private void OpenLayerDraw(int id)
        {
            if (App.CurrentArtFile == null)
                return;

            layerDraw = id == -1 ? null : new(App.CurrentArtFile.Art.ArtLayers[id]);
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

            DrawRectangle(startArtPos, endArtPos);

            App.CurrentArtFile?.Art.Update();
            App.CurrentArtFile?.ArtTimeline.NewTimePoint();
        }

        public void DrawRectangle(Point startArtPos, Point endArtPos)
        {
            if (App.CurrentArtFile == null || layerDraw == null)
                return;

            layerDraw.StayInsideSelection = StayInsideSelection;
            layerDraw.BrushThickness = Size;

            int startX = (int)(endArtPos.X > startArtPos.X ? startArtPos.X : endArtPos.X);
            int startY = (int)(endArtPos.Y > startArtPos.Y ? startArtPos.Y : endArtPos.Y);

            int width = (int)(endArtPos.X > startArtPos.X ? endArtPos.X - startArtPos.X + 1 : startArtPos.X - endArtPos.X + 1);
            int height = (int)(endArtPos.Y > startArtPos.Y ? endArtPos.Y - startArtPos.Y + 1 : startArtPos.Y - endArtPos.Y + 1);

            layerDraw.DrawRectangle(Character, startX, startY, width, height, Fill);
        }

        public void UpdatePreview(Point startArtPos, Point endArtPos)
        {
            if (App.CurrentArtFile == null || !App.Settings.ToolPreviews)
            {
                Preview = null;
                return;
            }

            if (startArtPos == lastPreviewStartArtPos && endArtPos == lastPreviewEndArtPos)
                return;

            lastPreviewStartArtPos = startArtPos;
            lastPreviewEndArtPos = endArtPos;

            ArtLayer layer = App.CurrentArtFile.Art.ArtLayers[App.CurrentLayerID];

            int offset = Size == 1 ? 0 : Math.Max(Size - 2, 1);

            int startX = (int)Math.Min(startArtPos.X, endArtPos.X);
            int startY = (int)Math.Min(startArtPos.Y, endArtPos.Y);

            int width = (int)Math.Max(endArtPos.X - startArtPos.X, startArtPos.X - endArtPos.X) + 1;
            int height = (int)Math.Max(endArtPos.Y - startArtPos.Y, startArtPos.Y - endArtPos.Y) + 1;

            int left = Math.Max(startX - offset, layer.OffsetX);
            int right = Math.Min(startX + width + offset, layer.OffsetX + layer.Width);
            int top = Math.Max(startY - offset, layer.OffsetY);
            int bottom = Math.Min(startY + height + offset, layer.OffsetY + layer.Height);

            //If preview layer is outside of canvas or layer, no preview is needed
            if (left >= layer.OffsetX + layer.Width || top >= layer.OffsetY + layer.Height || right < layer.OffsetX || bottom < layer.OffsetY || left >= App.CurrentArtFile.Art.Width || top >= App.CurrentArtFile.Art.Height || right < 0 || bottom < 0)
            {
                Preview = null;
                return;
            }

            if (right - left <= 0 || bottom - top <= 0)
            {
                Preview = null;
                return;
            }

            ArtLayer previewLayer = new("Preview", right - left, bottom - top, left, top);

            ArtLayerDraw layerDraw = new(previewLayer)
            {
                BrushThickness = Size,
                StayInsideSelection = StayInsideSelection
            };

            layerDraw.DrawRectangle(Character, startX - previewLayer.OffsetX, startY - previewLayer.OffsetY, width, height, Fill);

            Preview = previewLayer;
        }
    }
}
