using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AAP.UI.Controls
{
    public class ASCIIArtCanvas : ASCIIArtViewer, IInputElement
    {
        public readonly static double MinHighlightRectThickness = 1;
        public readonly static double MaxHighlightRectThickness = 12;
        public readonly static double DefaultHighlightRectThickness = 2;

        private readonly DrawingVisual displayLayerHighlightVisual = new();
        private readonly DrawingVisual selectionHighlightsVisual = new();

        #region Canvas Properties
        public static readonly DependencyProperty DisplayLayerProperty =
        DependencyProperty.Register(
            name: "DisplayLayer",
            propertyType: typeof(ArtLayer),
            ownerType: typeof(ASCIIArtCanvas),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, OnDisplayLayerPropertyChangedCallBack));

        public ArtLayer? DisplayLayer
        {
            get => (ArtLayer?)GetValue(DisplayLayerProperty);
            set
            {
                if (DisplayLayer == value)
                    return;

                SetValue(DisplayLayerProperty, value);
            }
        }

        public static readonly DependencyProperty SelectionHighlightRectProperty =
       DependencyProperty.Register(
           name: "SelectionHighlightRect",
           propertyType: typeof(Rect),
           ownerType: typeof(ASCIIArtCanvas),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: Rect.Empty, OnSelectionHighlightRectPropertyChangedCallBack));

        public Rect SelectionHighlightRect
        {
            get => (Rect)GetValue(SelectionHighlightRectProperty);
            set
            {
                if (SelectionHighlightRect == value)
                    return;

                SetValue(SelectionHighlightRectProperty, value);
            }
        }

        private Rect mouseHighlightRect = Rect.Empty;
        protected Rect MouseHighlightRect
        {
            get => mouseHighlightRect;
            set
            {
                if (MouseHighlightRect == value)
                    return;

                mouseHighlightRect = value;

                DrawSelectionHighlights();
            }
        }

        public static readonly DependencyProperty HighlightRectThicknessProperty =
       DependencyProperty.Register(
           name: "HighlightRectThickness",
           propertyType: typeof(double),
           ownerType: typeof(ASCIIArtCanvas),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: DefaultHighlightRectThickness, OnHighlightRectThicknessPropertyChangedCallBack));

        public double HighlightRectThickness
        {
            get => (double)GetValue(HighlightRectThicknessProperty);
            set
            {
                if (HighlightRectThickness == Math.Clamp(value, MinHighlightRectThickness, MaxHighlightRectThickness))
                    return;

                SetValue(HighlightRectThicknessProperty, Math.Clamp(value, MinHighlightRectThickness, MaxHighlightRectThickness));
            }
        }

        public static readonly DependencyProperty ToolProperty =
        DependencyProperty.Register(
        name: "Tool",
        propertyType: typeof(Tool),
        ownerType: typeof(ASCIIArtCanvas),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public Tool? Tool
        {
            get => (Tool?)GetValue(ToolProperty);
            set
            {
                if (Tool == value)
                    return;

                SetValue(ToolProperty, value);
            }
        }

        public static readonly DependencyProperty CanUseToolProperty =
        DependencyProperty.Register(
            name: "CanUseTool",
            propertyType: typeof(bool),
            ownerType: typeof(ASCIIArtCanvas),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: true));

        public bool CanUseTool
        {
            get => (bool)GetValue(CanUseToolProperty);
            set
            {
                if (CanUseTool == value)
                    return;

                SetValue(CanUseToolProperty, value);
            }
        }

        public static readonly DependencyProperty ShowToolPreviewsProperty =
        DependencyProperty.Register(
            name: "ShowToolPreviews",
            propertyType: typeof(bool),
            ownerType: typeof(ASCIIArtCanvas),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: true));

        public bool ShowToolPreviews
        {
            get => (bool)GetValue(ShowToolPreviewsProperty);
            set
            {
                if (ShowToolPreviews == value)
                    return;

                SetValue(ShowToolPreviewsProperty, value);
            }
        }
        #endregion

        #region Brushes
        public static readonly DependencyProperty ArtSelectionHighlightProperty =
        DependencyProperty.Register(
            name: "ArtSelectionHighlight",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvas),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Orange, OnSelectionHighlightBrushChangedCallBack));

        public Brush ArtSelectionHighlight
        {
            get => (Brush)GetValue(ArtSelectionHighlightProperty);
            set
            {
                if (ArtSelectionHighlight == value)
                    return;

                SetValue(ArtSelectionHighlightProperty, value);
            }
        }

        public static readonly DependencyProperty MouseSelectionHighlightProperty =
        DependencyProperty.Register(
            name: "MouseSelectionHighlight",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvas),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Blue, OnSelectionHighlightBrushChangedCallBack));

        public Brush MouseSelectionHighlight
        {
            get => (Brush)GetValue(MouseSelectionHighlightProperty);
            set
            {
                if (MouseSelectionHighlight == value)
                    return;

                SetValue(MouseSelectionHighlightProperty, value);
            }
        }

        public static readonly DependencyProperty DisplayLayerHighlightProperty =
        DependencyProperty.Register(
            name: "DisplayLayerHighlight",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvas),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Gray, OnDisplayLayerHighlightBrushChangedCallBack));

        public Brush DisplayLayerHighlight
        {
            get => (Brush)GetValue(DisplayLayerHighlightProperty);
            set
            {
                if (DisplayLayerHighlight == value)
                    return;

                SetValue(DisplayLayerHighlightProperty, value);
            }
        }
        #endregion

        #region Drawing
        protected override char GetDisplayArtCharacter(int x, int y)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt) + " is null!");

            ArtLayer? previewLayer = Tool is IPreviewable<ArtLayer?> previewableTool && ShowToolPreviews ? previewableTool.Preview : null;

            if (previewLayer != null && x >= previewLayer.OffsetX && x < previewLayer.OffsetX + previewLayer.Width && y >= previewLayer.OffsetY && y < previewLayer.OffsetY + previewLayer.Height)
                return previewLayer.GetCharacter(x - previewLayer.OffsetX, y - previewLayer.OffsetY) ?? DisplayArt?.GetCharacter(x, y) ?? ASCIIArt.EMPTYCHARACTER;
            else
                return DisplayArt?.GetCharacter(x, y) ?? ASCIIArt.EMPTYCHARACTER;
        }

        /// <summary>
        /// Draws the highlights, such as the MouseHighlightRect & SelectionHighlightRect (excluding display layer highlight)
        /// </summary>
        protected void DrawSelectionHighlights()
        {
            using DrawingContext dc = selectionHighlightsVisual.RenderOpen();

            if (DisplayArt == null)
                return;

            //Selection Highlight
            if (SelectionHighlightRect != Rect.Empty)
                dc.DrawRectangle(null, new(ArtSelectionHighlight, HighlightRectThickness), GetArtCanvasRectangle(SelectionHighlightRect));

            //Mouse Highlight
            if (MouseHighlightRect != Rect.Empty)
                dc.DrawRectangle(null, new(MouseSelectionHighlight, HighlightRectThickness), GetArtCanvasRectangle(MouseHighlightRect));
        }

        /// <summary>
        /// Draws the display layer highlight
        /// </summary>
        protected void DrawDisplayLayerHighlight()
        {
            using DrawingContext dc = displayLayerHighlightVisual.RenderOpen();

            if (DisplayArt == null)
                return;

            //Display Layer Size Highlight
            if (DisplayLayer != null)
            {
                Pen rectPen = new(DisplayLayerHighlight, HighlightRectThickness);
                rectPen.DashStyle = DisplayLayer.Visible ? DashStyles.Dash : DashStyles.Dot;
                rectPen.Freeze();

                dc.DrawRectangle(null, rectPen, GetArtCanvasRectangle(new(DisplayLayer.Offset, DisplayLayer.Size)));
            }
        }

        protected override void OnViewerDrawn()
        {
            if (DisplayArt == null)
                return;

            ArtLayer? previewLayer = Tool is IPreviewable<ArtLayer?> previewableTool && ShowToolPreviews ? previewableTool.Preview : null;

            //Preview layer always changes
            if (previewLayer != null)
            {
                for (int x = previewLayer.OffsetX; x < previewLayer.OffsetX + previewLayer.Width; x++)
                {
                    if (x < 0)
                        continue;

                    if (x >= DisplayArt.Width)
                        break;

                    if (!changedColumns.Contains(x))
                        changedColumns.Add(x);
                }
            }
        }
        #endregion

        #region Highlights
        public void HighlightArtMatrixPosition(Point artMatrixPosition)
        {
            if (DisplayArt == null)
            {
                MouseHighlightRect = Rect.Empty;
                return;
            }

            MouseHighlightRect = artMatrixPosition.X < DisplayArt.Width && artMatrixPosition.X >= 0 && artMatrixPosition.Y < DisplayArt.Height && artMatrixPosition.Y >= 0 ? new(artMatrixPosition, new Size(1, 1)) : Rect.Empty;
        }

        public void SelectArtMatrixRect(Rect artMatrixRect)
        {
            if (DisplayArt == null)
            {
                SelectionHighlightRect = Rect.Empty;
                return;
            }

            SelectionHighlightRect = artMatrixRect;
        }
        #endregion

        #region Tool Functions
        private void ToolActivateStart(object? sender, MouseEventArgs e)
        {
            if (!CanUseTool)
                return;

            if (App.CurrentLayerID < 0)
            {
                MessageBox.Show(App.Language.GetString("Tool_NoLayerSelectedMessage"), App.Language.GetString("Tool_NoLayerSelectedCaption"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MouseMove += ToolActivateUpdate;
            MouseUp += ToolActivateEnd;
            MouseLeave += ToolActivateEnd;

            if (Tool is IPreviewable<ArtLayer?> previewableTool)
                previewableTool.OnPreviewChanged += OnToolPreviewChanged;

            Tool?.ActivateStart(GetArtMatrixPoint(e.GetPosition(this)));
        }

        private void ToolActivateUpdate(object? sender, MouseEventArgs e)
            => Tool?.ActivateUpdate(GetArtMatrixPoint(e.GetPosition(this)));

        private void ToolActivateEnd(object? sender, MouseEventArgs e)
        {
            MouseMove -= ToolActivateUpdate;
            MouseUp -= ToolActivateEnd;
            MouseLeave -= ToolActivateEnd;

            if (Tool is IPreviewable<ArtLayer?> previewableTool)
                previewableTool.OnPreviewChanged -= OnToolPreviewChanged;

            Tool?.ActivateEnd();
        }

        private void OnToolPreviewChanged(object? preview)
        {
            if (DisplayArt == null)
                return;

            if (preview == null)
                UpdateDisplayArt();

            if (preview is not ArtLayer previewLayer)
                return;

            for (int x = previewLayer.OffsetX; x < previewLayer.OffsetX + previewLayer.Width; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }

            UpdateDisplayArt();
        }
        #endregion

        #region Display Layer Event Implementations
        private void DisplayLayerOffsetChanged(ArtLayer layer, Point oldOffset, Point newOffset)
            => DrawDisplayLayerHighlight();

        private void DisplayLayerDataChanged(ArtLayer layer, char?[][] oldData, char?[][] newData)
            => DrawDisplayLayerHighlight();

        private void DisplayLayerVisibilityChanged(ArtLayer layer, bool visible)
            => DrawDisplayLayerHighlight();
        #endregion

        #region Dependancy Properties Changed Callbacks
        protected override void OnDisplayArtChanged(ASCIIArt? oldArt, ASCIIArt? newArt)
        {
            base.OnDisplayArtChanged(oldArt, newArt);

            if (oldArt != null && newArt == null)
                MouseLeftButtonDown -= ToolActivateStart;
            else if (oldArt == null && newArt != null)
                MouseLeftButtonDown += ToolActivateStart;
        }

        private static void OnDisplayLayerPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvas canvas)
                return;

            canvas.DrawDisplayLayerHighlight();

            ArtLayer? oldDisplayLayer = (ArtLayer?)e.OldValue;
            ArtLayer? newDisplayLayer = (ArtLayer?)e.NewValue;

            if (oldDisplayLayer != null)
            {
                oldDisplayLayer.OffsetChanged -= canvas.DisplayLayerOffsetChanged;
                oldDisplayLayer.DataChanged -= canvas.DisplayLayerDataChanged;
                oldDisplayLayer.VisibilityChanged -= canvas.DisplayLayerVisibilityChanged;
            }

            if (newDisplayLayer != null)
            {
                newDisplayLayer.OffsetChanged += canvas.DisplayLayerOffsetChanged;
                newDisplayLayer.DataChanged += canvas.DisplayLayerDataChanged;
                newDisplayLayer.VisibilityChanged += canvas.DisplayLayerVisibilityChanged;
            }
        }

        private static void OnSelectionHighlightRectPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvas canvas)
                return;

            canvas.DrawSelectionHighlights();
        }

        private static void OnHighlightRectThicknessPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvas canvas)
                return;

            canvas.DrawSelectionHighlights();
            canvas.DrawDisplayLayerHighlight();
        }

        private static void OnSelectionHighlightBrushChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvas canvas)
                return;

            canvas.DrawSelectionHighlights();
        }

        private static void OnDisplayLayerHighlightBrushChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvas canvas)
                return;

            canvas.DrawDisplayLayerHighlight();
        }
        #endregion

        #region Canvas Events
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (DisplayArt == null)
                return;

            HighlightArtMatrixPosition(GetArtMatrixPoint(e.GetPosition(this)));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
            => MouseHighlightRect = Rect.Empty;

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (e.Handled)
                return;

            if (Tool is not ITextInput textInputTool)
                return;

            textInputTool.OnTextInput(e.Text);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (Tool is not IKeyInput keyInputTool)
                return;

            keyInputTool.OnPressedKey(e.Key, e.KeyboardDevice.Modifiers);
        }

        private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            DrawSelectionHighlights();
            DrawDisplayLayerHighlight();
        }
        #endregion

        public ASCIIArtCanvas() : base()
        {
            _children.Add(displayLayerHighlightVisual);
            _children.Add(selectionHighlightsVisual);

            SizeChanged += OnSizeChanged;

            DrawSelectionHighlights();
            DrawDisplayLayerHighlight();
        }

    }
}
