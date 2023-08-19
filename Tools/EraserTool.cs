using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class EraserTool: Tool, ISizeSelectable, IStayInsideSelectionProperty, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Eraser;

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

        private bool stayInsideSelection = true;
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

        public EraserTool(int size)
        {
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            EraseCharacter(startArtPos);
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            EraseCharacter(currentArtPos);
        }

        public void EraseCharacter(Point artPos)
           => App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, null, artPos, StayInsideSelection);
    }
}
