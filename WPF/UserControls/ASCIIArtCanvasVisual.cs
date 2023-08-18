using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using AAP.UI.ViewModels;
using System.Windows.Input;
using System.Windows.Controls;
using System.Reflection.Emit;

namespace AAP.UI.Controls
{
    public class ASCIIArtCanvasVisual : FrameworkElement
    {
        public readonly static double MinCanvasTextSize = 128;
        public readonly static double MinHighlightRectThickness = 1;

        public readonly static double MaxCanvasTextSize = 128;
        public readonly static double MaxHighlightRectThickness = 1;

        public readonly static double DefaultCanvasTextSize = 16;
        public readonly static double DefaultHighlightRectThickness = 2;

        private readonly static string EmptyDisplayArtText = "No art to display!";

        private readonly VisualCollection _children;

        private readonly DrawingVisual backgroundVisual = new();
        private readonly List<DrawingVisual> displayArtVisuals = new();
        private readonly DrawingVisual displayLayerHighlightVisual = new();
        private readonly DrawingVisual selectionHighlightsVisual = new();

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }


        #region Brushes
        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            name: "Text",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Black, OnTextBrushPropertyChangedCallBack));

        public Brush Text
        {
            get => (Brush)GetValue(TextProperty);
            set
            {
                if (Text == value)
                    return;

                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(
            name: "Background",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.White, OnBackgroundBrushPropertyChangedCallBack));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set
            {
                if (Background == value)
                    return;

                SetValue(BackgroundProperty, value);
            }
        }

        public static readonly DependencyProperty BorderProperty =
        DependencyProperty.Register(
            name: "Border",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Black, OnBorderBrushPropertyChangedCallBack));

        public Brush Border
        {
            get => (Brush)GetValue(BorderProperty);
            set
            {
                if (Border == value)
                    return;

                SetValue(BorderProperty, value);
            }
        }

        public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(
            name: "BorderThickness",
            propertyType: typeof(double),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1d, OnBorderThicknessPropertyChangedCallBack));

        public double BorderThickness
        {
            get => (double)GetValue(BorderThicknessProperty);
            set
            {
                if (BorderThickness == value)
                    return;

                SetValue(BorderThicknessProperty, value);
            }
        }
        #endregion

        private Point ArtOffset = new(8, 8);

        public static readonly DependencyProperty DisplayArtProperty =
        DependencyProperty.Register(
            name: "DisplayArt",
            propertyType: typeof(ASCIIArt),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, OnDisplayArtPropertyChangedCallBack));

        public ASCIIArt? DisplayArt
        {
            get => (ASCIIArt?)GetValue(DisplayArtProperty);
            set
            {
                if (DisplayArt == value)
                    return;

                SetValue(DisplayArtProperty, value);
            }
        }

        public static readonly DependencyProperty DisplayArtDrawProperty =
        DependencyProperty.Register(
            name: "DisplayArtDraw",
            propertyType: typeof(ASCIIArtDraw),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, OnDisplayArtDrawPropertyChangedCallBack));

        public ASCIIArtDraw? DisplayArtDraw
        {
            get => (ASCIIArtDraw?)GetValue(DisplayArtDrawProperty);
            set
            {
                if (DisplayArtDraw == value)
                    return;

                SetValue(DisplayArtDrawProperty, value);
            }
        }

        public static readonly DependencyProperty DisplayLayerProperty =
        DependencyProperty.Register(
            name: "DisplayLayer",
            propertyType: typeof(ArtLayer),
            ownerType: typeof(ASCIIArtCanvasVisual),
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

        public static readonly DependencyProperty TextSizeProperty =
       DependencyProperty.Register(
           name: "TextSize",
           propertyType: typeof(double),
           ownerType: typeof(ASCIIArtCanvasVisual),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: DefaultCanvasTextSize, OnTextSizePropertyChangedCallBack));

        public double TextSize
        {
            get => (double)GetValue(TextSizeProperty);
            set
            {
                if (TextSize == Math.Clamp(value, MinCanvasTextSize, MaxCanvasTextSize))
                    return;

                SetValue(TextSizeProperty, Math.Clamp(value, MinCanvasTextSize, MaxCanvasTextSize));
            }
        }

        public double LineHeight { get => TextSize * 1.5; }

        public static readonly DependencyProperty ArtFontProperty =
       DependencyProperty.Register(
           name: "ArtFont",
           propertyType: typeof(Typeface),
           ownerType: typeof(ASCIIArtCanvasVisual),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Typeface("Consolas"), OnArtFontPropertyChangedCallBack));

        public Typeface ArtFont
        {
            get => (Typeface)GetValue(ArtFontProperty);
            set
            {
                if (ArtFont == value)
                    return;

                SetValue(ArtFontProperty, value);
            }
        }

        public static readonly DependencyProperty SelectionHighlightRectProperty =
       DependencyProperty.Register(
           name: "SelectionHighlightRect",
           propertyType: typeof(Rect),
           ownerType: typeof(ASCIIArtCanvasVisual),
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
           ownerType: typeof(ASCIIArtCanvasVisual),
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
        ownerType: typeof(ASCIIArtCanvasVisual),
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
            ownerType: typeof(ASCIIArtCanvasVisual),
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

        private readonly List<int> changedLines = new();

        #region Converting between Art Matrix & Art Canvas
        public Point GetArtMatrixPoint(Point canvasPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            Size nonOffsetCanvasSize = new(Width - ArtOffset.X * 2, Height - ArtOffset.Y * 2);

            Point artMatrixPos = new(Math.Floor((canvasPosition.X - ArtOffset.X) / (nonOffsetCanvasSize.Width / DisplayArt.Width)), Math.Floor((canvasPosition.Y - ArtOffset.Y) / (nonOffsetCanvasSize.Height / DisplayArt.Height)));

            return artMatrixPos;
        }

        public Rect GetCanvasCharacterRectangle(Point artMatrixPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            Size nonOffsetCanvasSize = new(Width - ArtOffset.X * 2, Height - ArtOffset.Y * 2);

            Point canvasPos = new(artMatrixPosition.X * (nonOffsetCanvasSize.Width / DisplayArt.Width) + ArtOffset.X, artMatrixPosition.Y * (nonOffsetCanvasSize.Height / DisplayArt.Height) + ArtOffset.Y);

            return new(canvasPos.X - (nonOffsetCanvasSize.Width / DisplayArt.Width / 4), canvasPos.Y, (nonOffsetCanvasSize.Width / DisplayArt.Width), (nonOffsetCanvasSize.Height / DisplayArt.Height));
        }

        public Rect GetArtCanvasRectangle(Rect artMatrixRectangle)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            Rect startRectangle = GetCanvasCharacterRectangle(artMatrixRectangle.Location);
            Rect endRectangle = GetCanvasCharacterRectangle(new(artMatrixRectangle.Location.X + artMatrixRectangle.Size.Width, artMatrixRectangle.Location.Y + artMatrixRectangle.Size.Height));

            double startX = startRectangle.X < endRectangle.X ? startRectangle.X : endRectangle.X;
            double startY = startRectangle.Y < endRectangle.Y ? startRectangle.Y : endRectangle.Y;
            double sizeX = startRectangle.X < endRectangle.X ? endRectangle.X - startRectangle.X + endRectangle.Width / 2 : startRectangle.X - endRectangle.X;
            double sizeY = startRectangle.Y < endRectangle.Y ? endRectangle.Y - startRectangle.Y : startRectangle.Y - endRectangle.Y;

            return new(startX, startY, sizeX, sizeY);
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

        #region Drawing
        /// <summary>
        /// Draws the background with an updated canvas size
        /// </summary>
        protected void UpdateBackground()
        {
            FormattedText artText = DisplayArt == null ? new(EmptyDisplayArtText, CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, Text, 1) : new(DisplayArt.GetArtString(), CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, Text, 1);

            Width = artText.WidthIncludingTrailingWhitespace + ArtOffset.X * 2;
            Height = (DisplayArt == null ? 1 : DisplayArt.Height) * LineHeight + ArtOffset.Y * 2;
            
            DrawBackground();
        }

        /// <summary>
        /// Draws the background without updating the canvas size
        /// </summary>
        protected void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            dc.DrawRectangle(Background, new(Border, BorderThickness), new(0, 0, Width, Height));
        }

        /// <summary>
        /// Draws all lines of the DisplayArt
        /// </summary>
        protected void DrawDisplayArt()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            
            _children.RemoveRange(1, _children.Count - 3);

            if (DisplayArt == null)
            {
                DrawingVisual lineVisual = new();
                DrawingContext dc = lineVisual.RenderOpen();

                FormattedText noFileOpenText = new(EmptyDisplayArtText, cultureInfo, FlowDirection, ArtFont, TextSize, Text, 1);

                dc.DrawText(noFileOpenText, new(ArtOffset.X, ArtOffset.Y));

                displayArtVisuals.Insert(0, lineVisual);
                _children.Insert(1, lineVisual);
                dc.Close();
            }
            else
                for (int y = 0; y < DisplayArt.Height; y++)
                {
                    DrawingVisual lineVisual = new();
                    lineVisual.Offset = new(ArtOffset.X, LineHeight * y + ArtOffset.Y);

                    using DrawingContext dc = lineVisual.RenderOpen();
                    
                    FormattedText lineText = new(DisplayArt.GetLineString(y), cultureInfo, FlowDirection, ArtFont, TextSize, Text, 1);
                    dc.DrawText(lineText, new(0, 0));

                    displayArtVisuals.Insert(y, lineVisual);
                    _children.Insert(1 + y, lineVisual);
                }

            stopwatch.Stop();
            ConsoleLogger.Inform("Drew full canvas! (" + stopwatch.ElapsedMilliseconds + " ms)");
        }

        /// <summary>
        /// Only draws changed lines of the DisplayArt
        /// </summary>
        protected void UpdateDisplayArt()
        {
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            if (DisplayArt == null) 
                throw new NullReferenceException(nameof(DisplayArt));

            if (changedLines.Count <= 0)
                return;
            
            Stopwatch stopwatch = new();
            stopwatch.Start();

            foreach (int y in changedLines)
            {
                FormattedText lineText = new(DisplayArt.GetLineString(y), cultureInfo, FlowDirection, ArtFont, TextSize, Text, 1);

                DrawingVisual lineVisual = displayArtVisuals[y];
                using DrawingContext dc = lineVisual.RenderOpen();
                dc.DrawText(lineText, new(0, 0));
            }

            changedLines.Clear();

            stopwatch.Stop();
            ConsoleLogger.Inform("Updated canvas! (" + stopwatch.ElapsedMilliseconds + " ms)");
        }

        /// <summary>
        /// Draws the highlights, such as the MouseHighlightRect & SelectionHighlightRect (excluding display layer highlight)
        /// </summary>
        protected void DrawSelectionHighlights()
        {
            using DrawingContext dc = selectionHighlightsVisual.RenderOpen();

            //Selection Highlight
            if (SelectionHighlightRect != Rect.Empty)
                dc.DrawRectangle(null, new(Brushes.Orange, HighlightRectThickness), GetArtCanvasRectangle(SelectionHighlightRect));

            //Mouse Highlight
            if (MouseHighlightRect != Rect.Empty)
                dc.DrawRectangle(null, new(Brushes.Blue, HighlightRectThickness), GetArtCanvasRectangle(MouseHighlightRect));
        }

        /// <summary>
        /// Draws the display layer highlight
        /// </summary>
        protected void DrawDisplayLayerHighlight()
        {
            using DrawingContext dc = displayLayerHighlightVisual.RenderOpen();

            //Display Layer Size Highlight
            if (DisplayLayer != null)
            {
                Pen rectPen = new(Brushes.Gray, HighlightRectThickness);
                rectPen.DashStyle = DisplayLayer.Visible ? DashStyles.Dash : DashStyles.Dot;
                dc.DrawRectangle(null, rectPen, GetArtCanvasRectangle(new(DisplayLayer.Offset, DisplayLayer.Size)));
            }
        }
        #endregion

        #region Tool Functions
        private void ToolActivateStart(object? sender, MouseEventArgs e)
        {
            if (!CanUseTool)
                return;

            if (App.CurrentLayerID < 0)
            {
                MessageBox.Show("Please select a layer!", "No Layer Selected!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MouseMove += ToolActivateUpdate;
            MouseUp += ToolActivateEnd;
            MouseLeave += ToolActivateEnd;
            
            Tool?.ActivateStart(GetArtMatrixPoint(e.GetPosition(this)));
        }

        private void ToolActivateUpdate(object? sender, MouseEventArgs e)
            => Tool?.ActivateUpdate(GetArtMatrixPoint(e.GetPosition(this)));

        private void ToolActivateEnd(object? sender, MouseEventArgs e)
        {
            MouseMove -= ToolActivateUpdate;
            MouseUp -= ToolActivateEnd;
            MouseLeave -= ToolActivateEnd;

            Tool?.ActivateEnd();
        }
        #endregion

        #region Art Event Implementations
        private void DisplayArtUpdated(ASCIIArt art)
        {
            UpdateDisplayArt();
        }

        private void OnArtDraw(int layerIndex, char? character, Point[] positions)
        {
            if (DisplayArt == null)
                return;

            foreach(Point point in positions)
            {
                int y = (int)point.Y;
                if (!changedLines.Contains(y) && y >= 0 && y < DisplayArt.Height)
                    changedLines.Add(y);
            }
        }

        private void DisplayArtArtLayerAdded(int index, ArtLayer artLayer)
        {
            if (DisplayArt == null)
                return;

            artLayer.DataChanged += DisplayArtArtLayerDataChanged;
            artLayer.VisibilityChanged += DisplayArtArtLayerVisibilityChanged;
            artLayer.OffsetChanged += DisplayArtArtLayerOffsetChanged;
            artLayer.Cropped += DisplayArtArtLayerCropped;

            for (int y = artLayer.OffsetY; y < artLayer.OffsetY + artLayer.Height; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }
        }

        private void DisplayArtArtLayerIndexChanged(int index, ArtLayer artLayer)
        {
            if (DisplayArt == null)
                return;

            for (int y = artLayer.OffsetY; y < artLayer.OffsetY + artLayer.Height; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }
        }

        private void DisplayArtArtLayerRemoved(int index, ArtLayer artLayer)
        {
            if (DisplayArt == null)
                return;

            artLayer.DataChanged -= DisplayArtArtLayerDataChanged;
            artLayer.VisibilityChanged -= DisplayArtArtLayerVisibilityChanged;
            artLayer.OffsetChanged -= DisplayArtArtLayerOffsetChanged;
            artLayer.Cropped -= DisplayArtArtLayerCropped;

            for (int y = artLayer.OffsetY; y < artLayer.OffsetY + artLayer.Height; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }
        }

        private void DisplayArtArtLayerDataChanged(ArtLayer artLayer, char?[][] oldData, char?[][] newData)
        {
            if (DisplayArt == null)
                return;

            if (oldData[0] != null)
            {
                for (int y = artLayer.OffsetY; y < artLayer.OffsetY + oldData[0].Length; y++)
                {
                    if (y < 0)
                        continue;

                    if (y >= DisplayArt.Height)
                        break;

                    if (!changedLines.Contains(y))
                        changedLines.Add(y);
                }
            }

            if (newData[0] != null)
            {
                for (int y = artLayer.OffsetY; y < artLayer.OffsetY + newData[0].Length; y++)
                {
                    if (y < 0)
                        continue;

                    if (y >= DisplayArt.Height)
                        break;
    
                    if (!changedLines.Contains(y))
                        changedLines.Add(y);
                }
            }
        }

        private void DisplayArtArtLayerVisibilityChanged(ArtLayer artLayer, bool visible)
        {
            if (DisplayArt == null)
                return;
            
            for (int y = artLayer.OffsetY; y < artLayer.OffsetY + artLayer.Height; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }
        }

        private void DisplayArtArtLayerOffsetChanged(ArtLayer artLayer, Point oldOffset, Point newOffset)
        {
            if (DisplayArt == null)
                return;

            int oldOffsetY = (int)oldOffset.Y;

            for (int y = oldOffsetY; y < oldOffsetY + artLayer.Height; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }

            int newOffsetY = (int)newOffset.Y;

            for (int y = newOffsetY; y < newOffsetY + artLayer.Height; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }
        }

        private void DisplayArtArtLayerCropped(ArtLayer artLayer, Rect oldRect, Rect newRect)
        {
            if (DisplayArt == null)
                return;

            int oldOffsetY = (int)oldRect.Y;
            int oldHeight = (int)oldRect.Height;

            for (int y = oldOffsetY; y < oldOffsetY + oldHeight; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }

            int newOffsetY = (int)newRect.Y;
            int newHeight = (int)newRect.Height;

            for (int y = newOffsetY; y < newOffsetY + newHeight; y++)
            {
                if (y < 0)
                    continue;

                if (y >= DisplayArt.Height)
                    break;

                if (!changedLines.Contains(y))
                    changedLines.Add(y);
            }
        }

        private void DisplayArtCropped(ASCIIArt art)
            => DrawDisplayArt();

        private void DisplayArtSizeChanged(int width, int height)
            => UpdateBackground();
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
        private static void OnDisplayArtPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.UpdateBackground();
            canvas.DrawDisplayArt();

            ASCIIArt? oldDisplayArt = (ASCIIArt?)e.OldValue;
            ASCIIArt? newDisplayArt = (ASCIIArt?)e.NewValue;

            if (oldDisplayArt != null)
            {
                oldDisplayArt.OnArtLayerAdded -= canvas.DisplayArtArtLayerAdded;
                oldDisplayArt.OnArtLayerIndexChanged -= canvas.DisplayArtArtLayerIndexChanged;
                oldDisplayArt.OnArtLayerRemoved -= canvas.DisplayArtArtLayerRemoved;
                oldDisplayArt.OnCropped -= canvas.DisplayArtCropped;
                oldDisplayArt.OnSizeChanged -= canvas.DisplayArtSizeChanged;
                oldDisplayArt.OnArtUpdated -= canvas.DisplayArtUpdated;

                foreach (ArtLayer layer in oldDisplayArt.ArtLayers)
                {
                    layer.DataChanged -= canvas.DisplayArtArtLayerDataChanged;
                    layer.VisibilityChanged -= canvas.DisplayArtArtLayerVisibilityChanged;
                    layer.OffsetChanged -= canvas.DisplayArtArtLayerOffsetChanged;
                    layer.Cropped -= canvas.DisplayArtArtLayerCropped;
                }
            }

            if (newDisplayArt == null)
                canvas.MouseLeftButtonDown -= canvas.ToolActivateStart;
            else
            {
                newDisplayArt.OnArtLayerAdded += canvas.DisplayArtArtLayerAdded;
                newDisplayArt.OnArtLayerIndexChanged += canvas.DisplayArtArtLayerIndexChanged;
                newDisplayArt.OnArtLayerRemoved += canvas.DisplayArtArtLayerRemoved;
                newDisplayArt.OnCropped += canvas.DisplayArtCropped;
                newDisplayArt.OnSizeChanged += canvas.DisplayArtSizeChanged;
                newDisplayArt.OnArtUpdated += canvas.DisplayArtUpdated;

                foreach (ArtLayer layer in newDisplayArt.ArtLayers)
                {
                    layer.DataChanged += canvas.DisplayArtArtLayerDataChanged;
                    layer.VisibilityChanged += canvas.DisplayArtArtLayerVisibilityChanged;
                    layer.OffsetChanged += canvas.DisplayArtArtLayerOffsetChanged;
                    layer.Cropped += canvas.DisplayArtArtLayerCropped;
                }

                if (oldDisplayArt == null)
                    canvas.MouseLeftButtonDown += canvas.ToolActivateStart;
            }
        }

        private static void OnDisplayArtDrawPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            ASCIIArtDraw? oldDisplayArtDraw = (ASCIIArtDraw?)e.OldValue;
            ASCIIArtDraw? newDisplayArtDraw = (ASCIIArtDraw?)e.NewValue;

            if (oldDisplayArtDraw != null)
            {
                oldDisplayArtDraw.OnDrawArt -= canvas.OnArtDraw;
            }

            if (newDisplayArtDraw != null)
            {
                newDisplayArtDraw.OnDrawArt += canvas.OnArtDraw;
            }
        }

        private static void OnDisplayLayerPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
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
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;
            
            canvas.DrawSelectionHighlights();
        }

        private static void OnHighlightRectThicknessPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawSelectionHighlights();
            canvas.DrawDisplayLayerHighlight();
        }

        #region Draw Property Changed Callbacks
        private static void OnTextBrushPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawDisplayArt();
        }

        private static void OnBackgroundBrushPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawBackground();
        }

        private static void OnBorderBrushPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawBackground();
        }

        private static void OnBorderThicknessPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawBackground();
        }

        private static void OnTextSizePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.UpdateBackground();
            canvas.DrawDisplayArt();
            canvas.DrawSelectionHighlights();
            canvas.DrawDisplayLayerHighlight();
        }

        private static void OnArtFontPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.UpdateBackground();
            canvas.DrawDisplayArt();
            canvas.DrawSelectionHighlights();
            canvas.DrawDisplayLayerHighlight();
        }
        #endregion
        #endregion

        #region Mouse Events
        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (DisplayArt == null)
                return;

            HighlightArtMatrixPosition(GetArtMatrixPoint(e.GetPosition(this)));
        }

        private void OnMouseLeave(object? sender, MouseEventArgs e)
            => MouseHighlightRect = Rect.Empty;
        #endregion

        public ASCIIArtCanvasVisual()
        {
            _children = new(this)
            {
                backgroundVisual,
                //art visuals
                displayLayerHighlightVisual,
                selectionHighlightsVisual
            };
            
            MouseMove += OnMouseMove;
            MouseLeave += OnMouseLeave;

            UpdateBackground();
            DrawDisplayArt();
            DrawSelectionHighlights();
            DrawDisplayLayerHighlight();
        }
    }
}
