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
using System.Windows.Media;
using AAP.FileObjects;
using AAP.Timelines;
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class ArtCanvasViewModel : INotifyPropertyChanged
    {
        private Typeface canvasTypeface = new("Consolas");
        public Typeface CanvasTypeface
        {
            get => canvasTypeface;
            set
            {
                if (canvasTypeface == value)
                    return;

                canvasTypeface = value;

                PropertyChanged?.Invoke(this, new(nameof(CanvasTypeface)));
            }
        }

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

        private bool showToolPreviews = true;
        public bool ShowToolPreviews
        {
            get => showToolPreviews;
            set
            {
                if (showToolPreviews == value)
                    return;

                showToolPreviews = value;
                PropertyChanged?.Invoke(this, new(nameof(ShowToolPreviews)));
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

            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content

        private string canvasTextSizeFormat = App.Language.GetString("CanvasTextSize");
        public string CanvasTextSizeFormat => canvasTextSizeFormat;

        private string increaseTextSizeContent = App.Language.GetString("CanvasIncreaseTextSize");
        public string IncreaseTextSizeContent => increaseTextSizeContent;

        private string decreaseTextSizeContent = App.Language.GetString("CanvasDecreaseTextSize");
        public string DecreaseTextSizeContent => decreaseTextSizeContent;

        private string resetTextSizeContent = App.Language.GetString("CanvasResetTextSize");
        public string ResetTextSizeContent => resetTextSizeContent;

        private string highlightThicknessFormat = App.Language.GetString("CanvasHighlightThickness");
        public string HighlightThicknessFormat => highlightThicknessFormat;

        private string increaseThicknessContent = App.Language.GetString("CanvasIncreaseHighlightThickness");
        public string IncreaseThicknessContent => increaseThicknessContent;

        private string decreaseThicknessContent = App.Language.GetString("CanvasDecreaseHighlightThickness");
        public string DecreaseThicknessContent => decreaseThicknessContent;

        private string resetThicknessContent = App.Language.GetString("CanvasResetHighlightThickness");
        public string ResetThicknessContent => resetThicknessContent;

        private string showGridContent = App.Language.GetString("CanvasGridVisibility");
        public string ShowGridContent => showGridContent;

        private void OnLanguageChanged(Language language)
        {
            canvasTextSizeFormat = language.GetString("CanvasTextSize");
            increaseTextSizeContent = language.GetString("CanvasIncreaseTextSize");
            decreaseTextSizeContent = language.GetString("CanvasDecreaseTextSize");
            resetTextSizeContent = language.GetString("CanvasResetTextSize");
            highlightThicknessFormat = language.GetString("CanvasHighlightThickness");
            increaseThicknessContent = language.GetString("CanvasIncreaseHighlightThickness");
            decreaseThicknessContent = language.GetString("CanvasDecreaseHighlightThickness");
            resetThicknessContent = language.GetString("CanvasResetHighlightThickness");
            showGridContent = language.GetString("CanvasGridVisibility");

            PropertyChanged?.Invoke(this, new(nameof(CanvasTextSizeFormat)));
            PropertyChanged?.Invoke(this, new(nameof(TextSize)));
            PropertyChanged?.Invoke(this, new(nameof(IncreaseTextSizeContent)));
            PropertyChanged?.Invoke(this, new(nameof(DecreaseTextSizeContent)));
            PropertyChanged?.Invoke(this, new(nameof(ResetTextSizeContent)));
            PropertyChanged?.Invoke(this, new(nameof(HighlightThicknessFormat)));
            PropertyChanged?.Invoke(this, new(nameof(HighlightThickness)));
            PropertyChanged?.Invoke(this, new(nameof(IncreaseThicknessContent)));
            PropertyChanged?.Invoke(this, new(nameof(DecreaseThicknessContent)));
            PropertyChanged?.Invoke(this, new(nameof(ResetThicknessContent)));
            PropertyChanged?.Invoke(this, new(nameof(ShowGridContent)));
        }

        #endregion

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
