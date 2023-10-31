using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class EllipseTool: Tool, ICharacterSelectable, ISizeSelectable, IStayInsideSelectionProperty, IFillProperty, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public EllipseTool(char? character, int size)
        {
            Character = character;
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            return;
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            return;
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
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

            int width = (int)(endArtPos.X - startArtPos.X + 1);
            int height = (int)(endArtPos.Y - startArtPos.Y + 1);

            App.CurrentArtFile.ArtDraw.DrawEllipse(App.CurrentLayerID, Character, centerX, centerY, width, height, Fill);
        }
    }
}
