using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AAP
{
    public class ArtCanvasViewModel : INotifyPropertyChanged
    {
        private int textSize = 12;
        public int TextSize
        {
            get => textSize;
            set
            {
                textSize = value;
                PropertyChanged?.Invoke(this, new(nameof(TextSize)));
            }
        }

        private int highlightThickness = 4;
        public int HighlightThickness
        {
            get => highlightThickness;
            set
            {
                highlightThickness = value;
                PropertyChanged?.Invoke(this, new(nameof(HighlightThickness)));
            }
        }

        private ASCIIArt? currentArt = null;
        public ASCIIArt? CurrentArt
        {
            get => currentArt;
            set
            {
                currentArt = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentArt)));
            }
        }

        private ASCIIArtDraw? currentArtDraw = null;
        public ASCIIArtDraw? CurrentArtDraw
        {
            get => currentArtDraw;
            set
            {
                currentArtDraw = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentArtDraw)));
            }
        }

        private Tool? currentTool = null;
        public Tool? CurrentTool 
        { 
            get => currentTool;
            set
            {
                currentTool = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentTool)));
            }
        }

        private Rect selected = Rect.Empty;
        public Rect Selected
        {
            get => selected;
            set 
            { 
                selected = value;
                PropertyChanged?.Invoke(this, new(nameof(Selected)));
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public ArtCanvasViewModel()
        {
            //PropertyChanged +=  (a, e) => EnlargeTextSize();
        }

        public void EnlargeTextSize()
            => TextSize += 2;

        public void ShrinkTextSize()
            => TextSize -= 2;

        public void ResetTextSize()
            => TextSize = 12;

        public void IncreaseHighlightThickness()
            => HighlightThickness += 1;

        public void DecreaseHighlightThickness()
            => HighlightThickness -= 1;

        public void ResetHighlightThickness()
            => HighlightThickness = 4;
    }
}
