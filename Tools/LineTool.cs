using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class LineTool: Tool, ICharacterSelectable, ISizeSelectable, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public LineTool(char? character)
        {
            Character = character;   
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawCharacter(startArtPos);
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            DrawLine(startArtPos, endArtPos);
        }

        public void DrawCharacter(Point artPos)
            => App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, Character, artPos);

        public void DrawLine(Point startArtPos, Point endArtPos)
            => App.CurrentArtDraw?.DrawLine(App.CurrentLayerID, Character, startArtPos, endArtPos);
    }
}
