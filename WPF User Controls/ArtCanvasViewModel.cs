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
        private int textSize = ASCIIArtCanvasVisual.DefaultCanvasTextSize;
        public int TextSize
        {
            get => textSize;
            set
            {
                textSize = Math.Clamp(value, 4, 128);
                PropertyChanged?.Invoke(this, new(nameof(TextSize)));
            }
        }

        private int highlightThickness = ASCIIArtCanvasVisual.DefaultHighlightRectThickness;
        public int HighlightThickness
        {
            get => highlightThickness;
            set
            {
                highlightThickness = Math.Clamp(value, 1, 12);
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

        public ICommand EnlargeTextSizeCommand { get; private set; }
        public ICommand ShrinkTextSizeCommand { get; private set; }
        public ICommand ResetTextSizeCommand { get; private set; }
        public ICommand IncreaseHighlightThicknessCommand { get; private set; }
        public ICommand DecreaseHighlightThicknessCommand { get; private set; }
        public ICommand ResetHighlightThicknessCommand { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ArtCanvasViewModel()
        {
            EnlargeTextSizeCommand = new ActionCommand(EnlargeTextSize);
            ShrinkTextSizeCommand = new ActionCommand(ShrinkTextSize);
            ResetTextSizeCommand = new ActionCommand(ResetTextSize);
            IncreaseHighlightThicknessCommand = new ActionCommand(IncreaseHighlightThickness);
            DecreaseHighlightThicknessCommand = new ActionCommand(DecreaseHighlightThickness);
            ResetHighlightThicknessCommand = new ActionCommand(ResetHighlightThickness);
        }

        public void EnlargeTextSize(object? parameter = null)
            => TextSize += 2;

        public void ShrinkTextSize(object? parameter = null)
            => TextSize -= 2;

        public void ResetTextSize(object? parameter = null)
            => TextSize = ASCIIArtCanvasVisual.DefaultCanvasTextSize;

        public void IncreaseHighlightThickness(object? parameter = null)
            => HighlightThickness += 1;

        public void DecreaseHighlightThickness(object? parameter = null)
            => HighlightThickness -= 1;

        public void ResetHighlightThickness(object? parameter = null)
            => HighlightThickness = ASCIIArtCanvasVisual.DefaultHighlightRectThickness;
    }
}
