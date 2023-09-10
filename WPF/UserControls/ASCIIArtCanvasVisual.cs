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
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;

namespace AAP.UI.Controls
{
    public class ASCIIArtCanvasVisual : FrameworkElement, IInputElement
    {
        public readonly static double MinCanvasTextSize = 1;
        public readonly static double MinHighlightRectThickness = 1;

        public readonly static double MaxCanvasTextSize = 128;
        public readonly static double MaxHighlightRectThickness = 12;

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

        public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(
            name: "BorderThickness",
            propertyType: typeof(double),
            ownerType: typeof(ASCIIArtCanvasVisual),
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

        public static readonly DependencyProperty GridProperty =
        DependencyProperty.Register(
            name: "Grid",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvasVisual),
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

        public static readonly DependencyProperty GridLineThicknessProperty =
        DependencyProperty.Register(
            name: "GridLineThickness",
            propertyType: typeof(double),
            ownerType: typeof(ASCIIArtCanvasVisual),
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
            ownerType: typeof(ASCIIArtCanvasVisual),
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

        public static readonly DependencyProperty ArtSelectionHighlightProperty =
        DependencyProperty.Register(
            name: "ArtSelectionHighlight",
            propertyType: typeof(Brush),
            ownerType: typeof(ASCIIArtCanvasVisual),
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
            ownerType: typeof(ASCIIArtCanvasVisual),
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
            ownerType: typeof(ASCIIArtCanvasVisual),
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

        private readonly List<int> changedColumns = new();
        
        private double[] columnWidths = new double[1];

        private double defaultColumnWidth => new FormattedText("A", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, ArtFont, TextSize, Text, 1).Width;

        #region Converting between Art Matrix & Art Canvas
        public Point GetArtMatrixPoint(Point canvasPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            Size nonOffsetCanvasSize = new(Width - ArtOffset.X * 2, Height - ArtOffset.Y * 2);

            double defaultWidth = defaultColumnWidth;

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

                if(canvasPosX < canvasPosition.X)
                    artPosX++;
            }

            return new(artPosX, artPosY);
        }

        public Rect GetCanvasCharacterRectangle(Point artMatrixPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            double defaultWidth = defaultColumnWidth;

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
        protected void UpdateColumnWidth(int x)
        {
            if (DisplayArt == null)
                return;

            if (x < 0 || x >= DisplayArt.Width)
                return;

            string columnString = "";

            for (int y = 0; y < DisplayArt.Height; y++)
                columnString += DisplayArt.GetCharacter(x, y) + "\n";

            FormattedText text = new(columnString, CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, Text, 1);
            columnWidths[x] = text.WidthIncludingTrailingWhitespace;
        }

        /// <summary>
        /// Draws the background with an updated canvas size
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
        /// Draws the background without updating the canvas size
        /// </summary>
        protected void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            dc.DrawRectangle(Background, new(Border, BorderThickness), new(0, 0, Width, Height));

            if (DisplayArt != null && ShowGrid)
            {
                Pen gridLinePen = new(Grid, GridLineThickness);

                double defaultWidth = defaultColumnWidth;

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
            _children.RemoveRange(1, _children.Count - 3);
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
        /// Draws Display Art, updating the canvas if there any neccessary changes
        /// </summary>
        protected void DrawDisplayArt()
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
                DrawSelectionHighlights();
                DrawDisplayLayerHighlight();
            }
            else
            {
                if (displayArtVisuals.Count != DisplayArt.Width)
                {
                    ResetDisplayArtVisuals();

                    CreateDisplayArtVisuals();
                }

                double offsetX = ArtOffset.X;
                double defaultWidth = defaultColumnWidth;

                for (int x = 0; x < DisplayArt.Width; x++)
                {
                    DrawingVisual columnVisual = displayArtVisuals[x];

                    DrawingContext dc = columnVisual.RenderOpen();

                    string columnString = "";
                    for (int y = 0; y < DisplayArt.Height; y++)
                        columnString += (DisplayArt.GetCharacter(x, y) ?? ASCIIArt.EMPTYCHARACTER) + "\n";

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
                }

                UpdateBackground();
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
                    columnString += (DisplayArt.GetCharacter(x, y) ?? ASCIIArt.EMPTYCHARACTER) + "\n";

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

        private void OnArtDrawDrewCharacter(int layerIndex, char? character, int x, int y)
        {
            if (DisplayArt == null)
                return;

            if (!changedColumns.Contains(x) && x >= 0 && x < DisplayArt.Width)
                changedColumns.Add(x);
        }

        private void DisplayArtArtLayerAdded(int index, ArtLayer artLayer)
        {
            if (DisplayArt == null)
                return;

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

            ASCIIArt? oldDisplayArt = (ASCIIArt?)e.OldValue;
            ASCIIArt? newDisplayArt = (ASCIIArt?)e.NewValue;

            if (oldDisplayArt != null)
            {
                oldDisplayArt.OnArtLayerAdded -= canvas.DisplayArtArtLayerAdded;
                oldDisplayArt.OnArtLayerIndexChanged -= canvas.DisplayArtArtLayerIndexChanged;
                oldDisplayArt.OnArtLayerRemoved -= canvas.DisplayArtArtLayerRemoved;
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
            
            canvas.DrawDisplayArt();
        }

        private static void OnDisplayArtDrawPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            ASCIIArtDraw? oldDisplayArtDraw = (ASCIIArtDraw?)e.OldValue;
            ASCIIArtDraw? newDisplayArtDraw = (ASCIIArtDraw?)e.NewValue;

            if (oldDisplayArtDraw != null)
            {
                oldDisplayArtDraw.DrewCharacter -= canvas.OnArtDrawDrewCharacter;
            }

            if (newDisplayArtDraw != null)
            {
                newDisplayArtDraw.DrewCharacter += canvas.OnArtDrawDrewCharacter;
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

        private static void OnThicknessPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawBackground();
        }

        private static void OnShowGridPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawBackground();
        }

        private static void OnSelectionHighlightBrushChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawSelectionHighlights();
        }

        private static void OnDisplayLayerHighlightBrushChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawDisplayLayerHighlight();
        }

        private static void OnTextSizePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawDisplayArt();
            canvas.DrawSelectionHighlights();
            canvas.DrawDisplayLayerHighlight();
        }

        private static void OnArtFontPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawDisplayArt();
            canvas.DrawSelectionHighlights();
            canvas.DrawDisplayLayerHighlight();
        }
        #endregion
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

        public ASCIIArtCanvasVisual()
        {
            _children = new(this)
            {
                backgroundVisual,
                //art visuals
                displayLayerHighlightVisual,
                selectionHighlightsVisual
            };
            
            RequestBringIntoView += (sender, e) => e.Handled = true; // This causes the scrollviewers to not automatically scroll, thanks WPF >:(
            SizeChanged += OnSizeChanged;

            UpdateBackground();
            DrawDisplayArt();
            DrawSelectionHighlights();
            DrawDisplayLayerHighlight();
        }
    }
}
