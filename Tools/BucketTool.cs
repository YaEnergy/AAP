using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class BucketTool: Tool, ICharacterSelectable, IEightDirectionalProperty, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Bucket;

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

        private bool eightDirectional = false;
        public bool EightDirectional
        {
            get => eightDirectional;
            set
            {
                if (eightDirectional == value)
                    return;

                eightDirectional = value;

                PropertyChanged?.Invoke(this, new(nameof(EightDirectional)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public BucketTool(char? character)
        {
            Character = character;
        }

        protected override void UseStart(Point startArtPos)
        {
            FillArea(startArtPos);
        }

        public void FillArea(Point artPos)
            => App.CurrentArtDraw?.FloodFillArtPosWithCharacter(App.CurrentLayerID, Character, artPos, EightDirectional);
            
    }
}
