using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AAP.UI.Controls
{
    public class ASCIIArtViewer : FrameworkElement
    {
        public readonly static double MinTextSize = 1;
        public readonly static double MaxTextSize = 128;
        public readonly static double DefaultTextSize = 16;

        private readonly static string EmptyDisplayArtText = "No art to display!";
        private double DefaultColumnWidth => new FormattedText("A", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, ArtFont, TextSize, Text, 1).Width;

        private readonly DrawingVisual backgroundVisual = new();
        private readonly List<DrawingVisual> displayArtVisuals = new();

        protected readonly List<int> changedColumns = new();

        private double[] columnWidths = new double[1];
        private string[] columnStrings = new string[1];

        #region Viewer Display Properties
        private Point ArtOffset = new(8, 8);

        public static readonly DependencyProperty TextSizeProperty =
       DependencyProperty.Register(
           name: "TextSize",
           propertyType: typeof(double),
           ownerType: typeof(ASCIIArtViewer),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: DefaultTextSize, OnTextSizePropertyChangedCallBack));

        public double TextSize
        {
            get => (double)GetValue(TextSizeProperty);
            set
            {
                if (TextSize == Math.Clamp(value, MinTextSize, MaxTextSize))
                    return;

                SetValue(TextSizeProperty, Math.Clamp(value, MinTextSize, MaxTextSize));
            }
        }

        public double LineHeight { get => TextSize * 1.5; }

        public static readonly DependencyProperty ArtFontProperty =
       DependencyProperty.Register(
           name: "ArtFont",
           propertyType: typeof(Typeface),
           ownerType: typeof(ASCIIArtViewer),
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

        public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(
            name: "BorderThickness",
            propertyType: typeof(double),
            ownerType: typeof(ASCIIArtViewer),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1d, OnThicknessPropertyChangedCallBack));

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

        public static readonly DependencyProperty GridLineThicknessProperty =
        DependencyProperty.Register(
            name: "GridLineThickness",
            propertyType: typeof(double),
            ownerType: typeof(ASCIIArtViewer),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1d, OnThicknessPropertyChangedCallBack));

        public double GridLineThickness
        {
            get => (double)GetValue(GridLineThicknessProperty);
            set
            {
                if (GridLineThickness == value)
                    return;

                SetValue(GridLineThicknessProperty, value);
            }
        }

        public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(
            name: "ShowGrid",
            propertyType: typeof(bool),
            ownerType: typeof(ASCIIArtViewer),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: true, OnShowGridPropertyChangedCallBack));

        public bool ShowGrid
        {
            get => (bool)GetValue(ShowGridProperty);
            set
            {
                if (ShowGrid == value)
                    return;

                SetValue(ShowGridProperty, value);
            }
        }
        #endregion
        #region Brushes
        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            name: "Text",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtViewer),
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
            ownerType: typeof(ASCIIArtViewer),
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
            ownerType: typeof(ASCIIArtViewer),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Black, OnBackgroundBrushPropertyChangedCallBack));

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

        public static readonly DependencyProperty GridProperty =
        DependencyProperty.Register(
            name: "Grid",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtViewer),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.LightGray, OnBackgroundBrushPropertyChangedCallBack));

        public Brush Grid
        {
            get => (Brush)GetValue(GridProperty);
            set
            {
                if (Grid == value)
                    return;

                SetValue(GridProperty, value);
            }
        }
        #endregion

        public static readonly DependencyProperty DisplayArtProperty =
        DependencyProperty.Register(
            name: "DisplayArt",
            propertyType: typeof(ASCIIArt),
            ownerType: typeof(ASCIIArtViewer),
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

        protected readonly VisualCollection _children;

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        #region Converting between Art Matrix & Art Canvas
        public Point GetArtMatrixPoint(Point canvasPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            Size nonOffsetCanvasSize = new(Width - ArtOffset.X * 2, Height - ArtOffset.Y * 2);

            double defaultWidth = DefaultColumnWidth;

            int artPosX = 0;
            int artPosY = (int)Math.Floor((canvasPosition.Y - ArtOffset.Y) / (nonOffsetCanvasSize.Height / DisplayArt.Height));
            double canvasPosX = ArtOffset.X;

            if (canvasPosition.X < canvasPosX) //Out of bounds
                artPosX = (int)Math.Floor((canvasPosition.X - ArtOffset.X) / defaultWidth);

            while (canvasPosX < canvasPosition.X)
            {
                if (artPosX >= DisplayArt.Width) //Out of bounds x
                    canvasPosX += defaultWidth;
                else
                    canvasPosX += columnWidths[artPosX];

                if (canvasPosX < canvasPosition.X)
                    artPosX++;
            }

            return new(artPosX, artPosY);
        }

        public Rect GetCanvasCharacterRectangle(Point artMatrixPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            double defaultWidth = DefaultColumnWidth;

            int artPosX = (int)artMatrixPosition.X;

            double canvasPosX = ArtOffset.X;
            double canvasPosY = artMatrixPosition.Y * LineHeight + ArtOffset.Y;

            if (artPosX < 0)
                canvasPosX += defaultWidth * artPosX;
            else
            {
                for (int x = 0; x < artPosX; x++)
                {
                    if (x >= columnWidths.Length)
                        canvasPosX += defaultWidth;
                    else
                        canvasPosX += columnWidths[x];
                }
            }

            double width = artPosX >= columnWidths.Length || artPosX < 0 ? defaultWidth : columnWidths[artPosX];

            return new(canvasPosX, canvasPosY, width, LineHeight);
        }

        public Rect GetArtCanvasRectangle(Rect artMatrixRectangle)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            Rect startRectangle = GetCanvasCharacterRectangle(artMatrixRectangle.Location);
            Rect endRectangle = GetCanvasCharacterRectangle(new(artMatrixRectangle.Location.X + artMatrixRectangle.Size.Width, artMatrixRectangle.Location.Y + artMatrixRectangle.Size.Height));

            double startX = startRectangle.X < endRectangle.X ? startRectangle.X : endRectangle.X;
            double startY = startRectangle.Y < endRectangle.Y ? startRectangle.Y : endRectangle.Y;
            double sizeX = startRectangle.X < endRectangle.X ? endRectangle.X - startRectangle.X : startRectangle.X - endRectangle.X;
            double sizeY = startRectangle.Y < endRectangle.Y ? endRectangle.Y - startRectangle.Y : startRectangle.Y - endRectangle.Y;

            return new(startX, startY, sizeX, sizeY);
        }
        #endregion

        #region Drawing
        protected virtual char GetDisplayArtCharacter(int x, int y)
            => DisplayArt?.GetCharacter(x, y) ?? ASCIIArt.EMPTYCHARACTER;

        protected char GetDisplayArtCharacter(Point point)
            => GetDisplayArtCharacter((int)point.X, (int)point.Y);

        protected void UpdateColumnWidth(int x)
        {
            if (DisplayArt == null)
                return;

            if (x < 0 || x >= DisplayArt.Width)
                return;

            string columnString = "";

            for (int y = 0; y < DisplayArt.Height; y++)
                columnString += GetDisplayArtCharacter(x, y) + "\n";

            FormattedText text = new(columnString, CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, Text, 1);
            columnWidths[x] = text.WidthIncludingTrailingWhitespace;
        }

        /// <summary>
        /// Draws the background with an updated viewer size
        /// </summary>
        protected void UpdateBackground()
        {
            if (DisplayArt != null)
            {
                double width = ArtOffset.X * 2;
                foreach (double columnWidth in columnWidths)
                    width += columnWidth;

                Width = width;
            }
            else
            {
                FormattedText artText = new(EmptyDisplayArtText, CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, Text, 1);
                Width = artText.WidthIncludingTrailingWhitespace + ArtOffset.X * 2;
            }

            Height = (DisplayArt == null ? 1 : DisplayArt.Height) * LineHeight + ArtOffset.Y * 2;

            DrawBackground();
        }

        /// <summary>
        /// Draws the background without updating the viewer size
        /// </summary>
        protected void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            dc.DrawRectangle(Background, new(Border, BorderThickness), new(0, 0, Width, Height));

            if (DisplayArt != null && ShowGrid)
            {
                Pen gridLinePen = new(Grid, GridLineThickness);

                double defaultWidth = DefaultColumnWidth;

                double posX = ArtOffset.X;
                for (int x = 0; x <= DisplayArt.Width; x++)
                {
                    dc.DrawLine(gridLinePen, new(posX, ArtOffset.Y), new(posX, Height - ArtOffset.Y));

                    if (x >= columnWidths.Length)
                        posX += defaultWidth;
                    else
                        posX += columnWidths[x];
                }

                for (int y = 0; y <= DisplayArt.Height; y++)
                    dc.DrawLine(gridLinePen, new(ArtOffset.X, ArtOffset.Y + LineHeight * y), new(Width - ArtOffset.X, ArtOffset.Y + LineHeight * y));
            }
        }

        protected void ResetDisplayArtVisuals()
        {
            _children.RemoveRange(1, displayArtVisuals.Count);
            displayArtVisuals.Clear();
        }

        protected void CreateDisplayArtVisuals()
        {
            if (DisplayArt == null)
                throw new NullReferenceException("DisplayArt is null!");

            columnWidths = new double[DisplayArt.Width];

            for (int x = 0; x < DisplayArt.Width; x++)
            {
                DrawingVisual columnVisual = new();
                displayArtVisuals.Insert(x, columnVisual);
                _children.Insert(1 + x, columnVisual);
            }
        }

        /// <summary>
        /// Draws Display Art, updating the viewer if there any neccessary changes
        /// </summary>
        protected virtual void DrawDisplayArt()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            if (DisplayArt == null)
            {
                ResetDisplayArtVisuals();

                DrawingVisual lineVisual = new();
                DrawingContext dc = lineVisual.RenderOpen();

                FormattedText noFileOpenText = new(EmptyDisplayArtText, cultureInfo, FlowDirection, ArtFont, TextSize, Text, 1);

                dc.DrawText(noFileOpenText, new(ArtOffset.X, ArtOffset.Y));

                displayArtVisuals.Insert(0, lineVisual);
                _children.Insert(1, lineVisual);
                dc.Close();

                UpdateBackground();
            }
            else
            {
                if (displayArtVisuals.Count != DisplayArt.Width)
                {
                    ResetDisplayArtVisuals();

                    CreateDisplayArtVisuals();
                }

                double offsetX = ArtOffset.X;
                double defaultWidth = DefaultColumnWidth;

                for (int x = 0; x < DisplayArt.Width; x++)
                {
                    DrawingVisual columnVisual = displayArtVisuals[x];

                    DrawingContext dc = columnVisual.RenderOpen();

                    string columnString = "";
                    for (int y = 0; y < DisplayArt.Height; y++)
                        columnString += GetDisplayArtCharacter(x, y) + "\n";

                    FormattedText charText = new(columnString, cultureInfo, FlowDirection, ArtFont, TextSize, Text, 1);
                    charText.LineHeight = LineHeight;
                    dc.DrawText(charText, new(0, 0));

                    if (!string.IsNullOrWhiteSpace(columnString))
                        columnWidths[x] = charText.WidthIncludingTrailingWhitespace;
                    else
                        columnWidths[x] = defaultWidth;

                    columnVisual.Offset = new(offsetX, ArtOffset.Y);
                    offsetX += columnWidths[x];

                    dc.Close();

                    BitmapCacheBrush bitmapCache = new(columnVisual);

                    columnVisual.CacheMode = bitmapCache.BitmapCache;
                }

                UpdateBackground();
            }

            stopwatch.Stop();
            ConsoleLogger.Inform("Drew full canvas! (" + stopwatch.ElapsedMilliseconds + " ms)");

            OnViewerDrawn();
        }

        /// <summary>
        /// Only draws changed lines of the DisplayArt
        /// </summary>
        protected virtual void UpdateDisplayArt()
        {
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            if (changedColumns.Count <= 0)
                return;

            Stopwatch stopwatch = new();
            stopwatch.Start();

            Brush TextBrush = (Brush)Text.GetCurrentValueAsFrozen();

            int lowestColumnNumWidthChanged = -1;
            foreach (int x in changedColumns)
            {
                DrawingVisual columnVisual = displayArtVisuals[x];
                DrawingContext dc = columnVisual.RenderOpen();

                string columnString = "";
                for (int y = 0; y < DisplayArt.Height; y++)
                    columnString += GetDisplayArtCharacter(x, y) + "\n";

                FormattedText charText = new(columnString, cultureInfo, FlowDirection, ArtFont, TextSize, TextBrush, 1);
                charText.LineHeight = LineHeight;
                dc.DrawText(charText, new(0, 0));

                if (columnWidths[x] != charText.WidthIncludingTrailingWhitespace)
                {
                    columnWidths[x] = charText.WidthIncludingTrailingWhitespace;

                    if (lowestColumnNumWidthChanged > x || lowestColumnNumWidthChanged == -1)
                        lowestColumnNumWidthChanged = x;
                }

                dc.Close();
            }

            if (lowestColumnNumWidthChanged != -1) //Update background and art column visual offsets if a column width has changed
            {
                UpdateBackground();

                double visualPosX = ArtOffset.X;

                for (int x = 0; x < lowestColumnNumWidthChanged; x++)
                    visualPosX += columnWidths[x];

                for (int x = lowestColumnNumWidthChanged; x < DisplayArt.Width; x++)
                {
                    DrawingVisual columnVisual = displayArtVisuals[x];
                    columnVisual.Offset = new(visualPosX, ArtOffset.Y);
                    visualPosX += columnWidths[x];
                }
            }

            changedColumns.Clear();

            stopwatch.Stop();
            ConsoleLogger.Inform("Updated canvas! (" + stopwatch.ElapsedMilliseconds + " ms)");

            OnViewerDrawn();
        }

        protected virtual void OnViewerDrawn()
        {
            return;
        }
        #endregion

        #region Art Event Implementations
        private void DisplayArtUpdated(ASCIIArt art)
        {
            UpdateDisplayArt();
        }

        private void DisplayArtArtLayerAdded(int index, ArtLayer artLayer)
        {
            if (DisplayArt == null)
                return;

            artLayer.CharacterChanged += DisplayArtArtLayerCharacterChanged;
            artLayer.DataChanged += DisplayArtArtLayerDataChanged;
            artLayer.VisibilityChanged += DisplayArtArtLayerVisibilityChanged;
            artLayer.OffsetChanged += DisplayArtArtLayerOffsetChanged;
            artLayer.Cropped += DisplayArtArtLayerCropped;

            for (int x = artLayer.OffsetX; x < artLayer.OffsetX + artLayer.Width; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }
        }

        private void DisplayArtArtLayerIndexChanged(int index, ArtLayer artLayer)
        {
            if (DisplayArt == null)
                return;

            for (int x = artLayer.OffsetX; x < artLayer.OffsetX + artLayer.Width; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }
        }

        private void DisplayArtArtLayerRemoved(int index, ArtLayer artLayer)
        {
            if (DisplayArt == null)
                return;

            artLayer.CharacterChanged -= DisplayArtArtLayerCharacterChanged;
            artLayer.DataChanged -= DisplayArtArtLayerDataChanged;
            artLayer.VisibilityChanged -= DisplayArtArtLayerVisibilityChanged;
            artLayer.OffsetChanged -= DisplayArtArtLayerOffsetChanged;
            artLayer.Cropped -= DisplayArtArtLayerCropped;

            for (int x = artLayer.OffsetX; x < artLayer.OffsetX + artLayer.Width; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }
        }

        private void DisplayArtArtLayerCharacterChanged(ArtLayer layer, int x, int y)
        {
            if (DisplayArt == null)
                return;

            if (!changedColumns.Contains(x + layer.OffsetX) && x + layer.OffsetX >= 0 && x + layer.OffsetX < DisplayArt.Width)
                changedColumns.Add(x + layer.OffsetX);
        }

        private void DisplayArtArtLayerDataChanged(ArtLayer artLayer, char?[][] oldData, char?[][] newData)
        {
            if (DisplayArt == null)
                return;

            for (int x = artLayer.OffsetX; x < artLayer.OffsetX + oldData.Length; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }

            for (int x = artLayer.OffsetX; x < artLayer.OffsetX + newData.Length; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }
        }

        private void DisplayArtArtLayerVisibilityChanged(ArtLayer artLayer, bool visible)
        {
            if (DisplayArt == null)
                return;

            for (int x = artLayer.OffsetX; x < artLayer.OffsetX + artLayer.Width; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }
        }

        private void DisplayArtArtLayerOffsetChanged(ArtLayer artLayer, Point oldOffset, Point newOffset)
        {
            if (DisplayArt == null)
                return;

            int oldOffsetX = (int)oldOffset.X;

            for (int x = oldOffsetX; x < oldOffsetX + artLayer.Width; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }

            int newOffsetX = (int)newOffset.X;

            for (int x = newOffsetX; x < newOffsetX + artLayer.Width; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }
        }

        private void DisplayArtArtLayerCropped(ArtLayer artLayer, Rect oldRect, Rect newRect)
        {
            if (DisplayArt == null)
                return;

            int oldOffsetX = (int)oldRect.X;
            int oldWidth = (int)oldRect.Width;

            for (int x = oldOffsetX; x < oldOffsetX + oldWidth; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }

            int newOffsetX = (int)newRect.X;
            int newWidth = (int)newRect.Width;

            for (int x = newOffsetX; x < newOffsetX + newWidth; x++)
            {
                if (x < 0)
                    continue;

                if (x >= DisplayArt.Width)
                    break;

                if (!changedColumns.Contains(x))
                    changedColumns.Add(x);
            }
        }

        private void DisplayArtSizeChanged(int width, int height)
        {
            UpdateBackground();
            DrawDisplayArt();
        }
        #endregion

        #region Dependancy Properties Changed Callbacks
        protected virtual void OnDisplayArtChanged(ASCIIArt? oldArt, ASCIIArt? newArt)
        {
            if (oldArt != null)
            {
                oldArt.OnArtLayerAdded -= DisplayArtArtLayerAdded;
                oldArt.OnArtLayerIndexChanged -= DisplayArtArtLayerIndexChanged;
                oldArt.OnArtLayerRemoved -= DisplayArtArtLayerRemoved;
                oldArt.OnSizeChanged -= DisplayArtSizeChanged;
                oldArt.OnArtUpdated -= DisplayArtUpdated;

                foreach (ArtLayer layer in oldArt.ArtLayers)
                {
                    layer.CharacterChanged -= DisplayArtArtLayerCharacterChanged;
                    layer.DataChanged -= DisplayArtArtLayerDataChanged;
                    layer.VisibilityChanged -= DisplayArtArtLayerVisibilityChanged;
                    layer.OffsetChanged -= DisplayArtArtLayerOffsetChanged;
                    layer.Cropped -= DisplayArtArtLayerCropped;
                }
            }

            if (newArt != null)
            {
                newArt.OnArtLayerAdded += DisplayArtArtLayerAdded;
                newArt.OnArtLayerIndexChanged += DisplayArtArtLayerIndexChanged;
                newArt.OnArtLayerRemoved += DisplayArtArtLayerRemoved;
                newArt.OnSizeChanged += DisplayArtSizeChanged;
                newArt.OnArtUpdated += DisplayArtUpdated;

                foreach (ArtLayer layer in newArt.ArtLayers)
                {
                    layer.CharacterChanged += DisplayArtArtLayerCharacterChanged;
                    layer.DataChanged += DisplayArtArtLayerDataChanged;
                    layer.VisibilityChanged += DisplayArtArtLayerVisibilityChanged;
                    layer.OffsetChanged += DisplayArtArtLayerOffsetChanged;
                    layer.Cropped += DisplayArtArtLayerCropped;
                }
            }

            DrawDisplayArt();
        }

        private static void OnDisplayArtPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtViewer viewer)
                return;

            ASCIIArt? oldDisplayArt = (ASCIIArt?)e.OldValue;
            ASCIIArt? newDisplayArt = (ASCIIArt?)e.NewValue;

            viewer.OnDisplayArtChanged(oldDisplayArt, newDisplayArt);
        }

        private static void OnTextBrushPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtViewer viewer)
                return;

            viewer.DrawDisplayArt();
        }

        private static void OnBackgroundBrushPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtViewer viewer)
                return;

            viewer.DrawBackground();
        }

        private static void OnThicknessPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtViewer viewer)
                return;

            viewer.DrawBackground();
        }

        private static void OnShowGridPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtViewer viewer)
                return;

            viewer.DrawBackground();
        }

        private static void OnTextSizePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtViewer viewer)
                return;

            viewer.DrawDisplayArt();
        }

        private static void OnArtFontPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtViewer viewer)
                return;

            viewer.DrawDisplayArt();
        }
        #endregion

        public ASCIIArtViewer()
        {
            _children = new(this)
            {
                backgroundVisual,
                //art visuals
            };

            RequestBringIntoView += (sender, e) => e.Handled = true; // This causes the scrollviewers to not automatically scroll, thanks WPF >:(

            UpdateBackground();
            DrawDisplayArt();
        }
    }

}
