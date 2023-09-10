using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class LineTool: Tool, ICharacterSelectable, ISizeSelectable, IStayInsideSelectionProperty, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public LineTool(char? character)
        {
            Character = character;   
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawCircle(startArtPos);

            App.CurrentArtFile?.Art.Update();
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            DrawLine(startArtPos, endArtPos);
            App.CurrentArtTimeline?.NewTimePoint();

            App.CurrentArtFile?.Art.Update();
        }

        public void DrawCircle(Point artPos)
            => App.CurrentArtDraw?.DrawFilledCircle(App.CurrentLayerID, Character, artPos, Size - 1, StayInsideSelection);

        public void DrawLine(Point startArtPos, Point endArtPos)
            => App.CurrentArtDraw?.DrawLine(App.CurrentLayerID, Character, startArtPos, endArtPos, Size - 1, StayInsideSelection);
    }
}
