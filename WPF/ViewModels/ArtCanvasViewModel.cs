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
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class ArtCanvasViewModel : INotifyPropertyChanged
    {
        private int textSize = ASCIIArtCanvasVisual.DefaultCanvasTextSize;
        public int TextSize
        {
            get => textSize;
            set
            {
                if (textSize == Math.Clamp(value, 4, 128))
                    return;

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
                if (highlightThickness == Math.Clamp(value, 1, 12))
                    return;

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
                if (currentArt == value)
                    return;

                currentArt = value;
                HasArtOpen = value != null;
                PropertyChanged?.Invoke(this, new(nameof(CurrentArt)));
            }
        }

        private bool hasArtOpen = false;
        public bool HasArtOpen
        {
            get => hasArtOpen;
            private set
            {
                if (hasArtOpen == value)
                    return;

                hasArtOpen = value;
                PropertyChanged?.Invoke(this, new(nameof(HasArtOpen)));
            }
        }

        private ASCIIArtDraw? currentArtDraw = null;
        public ASCIIArtDraw? CurrentArtDraw
        {
            get => currentArtDraw;
            set
            {
                if (currentArtDraw == value)
                    return;

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
                if (currentTool == value)
                    return;

                currentTool = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentTool)));
            }
        }

        private bool canUseTool = true;
        public bool CanUseTool
        {
            get => canUseTool;
            set
            {
                if (canUseTool == value)
                    return;

                canUseTool = value;
                PropertyChanged?.Invoke(this, new(nameof(CanUseTool)));
            }
        }

        private bool hasSelected = false;
        public bool HasSelected
        {
            get => hasSelected;
            private set
            {
                if (hasSelected == value)
                    return;

                hasSelected = value;
                PropertyChanged?.Invoke(this, new(nameof(HasSelected)));
            }
        }

        private Rect selected = Rect.Empty;
        public Rect Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                    return;


                selected = value;
                HasSelected = value != Rect.Empty;
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
