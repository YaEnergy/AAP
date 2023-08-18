using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class PencilTool: Tool, ICharacterSelectable, ISizeSelectable, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public PencilTool(char? character, int size)
        {
            Character = character;
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawCharacter(startArtPos);
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            DrawCharacter(currentArtPos);
        }

        public void DrawCharacter(Point artPos)
            => App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, Character, artPos);
    }
}
