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
using AAP.Timelines;
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class ArtCanvasViewModel : INotifyPropertyChanged
    {
        private double textSize = ASCIIArtCanvasVisual.DefaultCanvasTextSize;
        public double TextSize
        {
            get => textSize;
            set
            {
                if (textSize == Math.Clamp(value, 1, 128))
                    return;

                textSize = Math.Clamp(value, 1, 128);
                PropertyChanged?.Invoke(this, new(nameof(TextSize)));
            }
        }

        private double highlightThickness = ASCIIArtCanvasVisual.DefaultHighlightRectThickness;
        public double HighlightThickness
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

        private bool showGrid = true;
        public bool ShowGrid
        {
            get => showGrid;
            set
            {
                if (showGrid == value)
                    return;

                showGrid = value;
                PropertyChanged?.Invoke(this, new(nameof(ShowGrid)));
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

        private int selectedLayerID = -1;
        public int SelectedLayerID
        {
            get => selectedLayerID;
            set
            {
                SelectedLayer = value != -1 && CurrentArt != null ? CurrentArt.ArtLayers[value] : null;

                if (selectedLayerID == value)
                    return;

                selectedLayerID = value;
                PropertyChanged?.Invoke(this, new(nameof(SelectedLayerID)));
            }
        }

        private ArtLayer? selectedLayer = null;
        public ArtLayer? SelectedLayer
        {
            get => selectedLayer;
            set
            {
                if (selectedLayer == value)
                    return;

                selectedLayer = value;
                PropertyChanged?.Invoke(this, new(nameof(SelectedLayer)));
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
            => TextSize += 1;

        public void ShrinkTextSize(object? parameter = null)
            => TextSize -= 1;

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
