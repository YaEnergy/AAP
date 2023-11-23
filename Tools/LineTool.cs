using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class LineTool: Tool, ICharacterSelectable, ISizeSelectable, IStayInsideSelectionProperty, IPreviewable<ArtLayer?>, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Line;

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

        public LineTool(char? character)
        {
            Character = character;

            App.OnCurrentLayerIDChanged += OpenLayerDraw;
        }

        private void OpenLayerDraw(int id)
        {
            if (App.CurrentArtFile == null)
                return;

            if (id == -1)
                layerDraw = null;

            layerDraw = new(App.CurrentArtFile.Art.ArtLayers[id]);
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

            DrawLine(startArtPos, endArtPos);
            App.CurrentArtFile?.ArtTimeline.NewTimePoint();

            App.CurrentArtFile?.Art.Update();
        }

        public void DrawLine(Point startArtPos, Point endArtPos)
        {
            if (App.CurrentArtFile == null || layerDraw == null)
                return;

            layerDraw.StayInsideSelection = StayInsideSelection;
            layerDraw.BrushThickness = Size;

            layerDraw.DrawLine(Character, startArtPos, endArtPos);
        }

        public void UpdatePreview(Point start, Point end)
        {
            if (App.CurrentArtFile == null || !App.Settings.ToolPreviews)
            {
                Preview = null;
                return;
            }

            if (start == lastPreviewStartArtPos && end == lastPreviewEndArtPos)
                return;

            lastPreviewStartArtPos = start;
            lastPreviewEndArtPos = end;

            ArtLayer layer = App.CurrentArtFile.Art.ArtLayers[App.CurrentLayerID];

            int offset = Size == 1 ? 0 : Math.Max(Size - 2, 1);

            int left = Math.Max((int)Math.Min(start.X, end.X) - offset, layer.OffsetX);
            int right = Math.Min((int)Math.Max(start.X, end.X) + offset + 1, layer.OffsetX + layer.Width);
            int top = Math.Max((int)Math.Min(start.Y, end.Y) - offset, layer.OffsetY);
            int bottom = Math.Min((int)Math.Max(start.Y, end.Y) + offset + 1, layer.OffsetY + layer.Height);
            
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

            layerDraw.DrawLine(Character, previewLayer.GetLayerPoint(start), previewLayer.GetLayerPoint(end));

            Preview = previewLayer;
        }
    }
}
