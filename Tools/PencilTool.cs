using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class PencilTool: Tool, ICharacterSelectable, ISizeSelectable, IStayInsideSelectionProperty, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Draw;

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public PencilTool(char? character, int size)
        {
            Character = character;
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawCircle(startArtPos);

            App.CurrentArtFile?.Art.Update();
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            DrawCircle(currentArtPos);

            App.CurrentArtFile?.Art.Update();
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            App.CurrentArtFile?.ArtTimeline.NewTimePoint();
        }

        public void DrawCircle(Point artPos)
        {
            if (App.CurrentArtFile == null)
                return;

            App.CurrentArtFile.ArtDraw.StayInsideSelection = StayInsideSelection;

            App.CurrentArtFile.ArtDraw.DrawFilledCircle(App.CurrentLayerID, Character, artPos, Size - 1);
        }
    }
}
