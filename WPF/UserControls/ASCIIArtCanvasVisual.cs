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

namespace AAP.UI.Controls
{
    public class ASCIIArtCanvasVisual : FrameworkElement
    {
        public readonly static int DefaultCanvasTextSize = 16;
        public readonly static int DefaultHighlightRectThickness = 4;
        private readonly static string EmptyDisplayArtText = "No art to display!";

        private readonly VisualCollection _children;

        private readonly DrawingVisual backgroundVisual = new();
        private readonly List<DrawingVisual> displayArtVisuals = new();
        private readonly DrawingVisual highlightVisual = new();

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }
        
        private SolidColorBrush ArtBrush = Brushes.Black;
        private Point ArtOffset = new(8, 8);
        private readonly Pen backgroundPen = new(Brushes.Black, 1);

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

        public static readonly DependencyProperty TextSizeProperty =
       DependencyProperty.Register(
           name: "TextSize",
           propertyType: typeof(int),
           ownerType: typeof(ASCIIArtCanvasVisual),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: 16, OnTextSizePropertyChangedCallBack));

        public int TextSize
        {
            get => (int)GetValue(TextSizeProperty);
            set
            {
                if (TextSize == value)
                    return;

                SetValue(TextSizeProperty, value);
/*
                UpdateCanvasSize();
                DrawDisplayArt();
                DrawHighlights();

                Console.WriteLine("Canvas Text Size: " + TextSize);*/
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

                DrawHighlights();
            }
        }

        public static readonly DependencyProperty HighlightRectThicknessProperty =
       DependencyProperty.Register(
           name: "HighlightRectThickness",
           propertyType: typeof(int),
           ownerType: typeof(ASCIIArtCanvasVisual),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: 2, OnHighlightRectThicknessPropertyChangedCallBack));

        public int HighlightRectThickness
        {
            get => (int)GetValue(HighlightRectThicknessProperty);
            set
            {
                if (HighlightRectThickness == value)
                    return;

                SetValue(HighlightRectThicknessProperty, value);
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
            FormattedText artText = DisplayArt == null ? new(EmptyDisplayArtText, CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, ArtBrush, 1) : new(DisplayArt.GetArtString(), CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, ArtBrush, 1);

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

            dc.DrawRectangle(Brushes.White, backgroundPen, new(0, 0, Width, Height));
        }

        /// <summary>
        /// Draws all lines of the DisplayArt
        /// </summary>
        protected void DrawDisplayArt()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            
            _children.RemoveRange(1, _children.Count - 2);

            if (DisplayArt == null)
            {
                DrawingVisual lineVisual = new();
                DrawingContext dc = lineVisual.RenderOpen();

                FormattedText noFileOpenText = new(EmptyDisplayArtText, cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);

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
                    
                    FormattedText lineText = new(DisplayArt.GetLineString(y), cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);
                    dc.DrawText(lineText, new(0, 0));

                    displayArtVisuals.Insert(y, lineVisual);
                    _children.Insert(1 + y, lineVisual);
                }

            stopwatch.Stop();
            Console.WriteLine("Drew full canvas! (" + stopwatch.ElapsedMilliseconds + " ms)");
        }

        /// <summary>
        /// Only draws changed lines of the DisplayArt
        /// </summary>
        protected void UpdateDisplayArt()
        {
#if WPF_DEBUG || DEBUG
            Stopwatch stopwatch = new();
            stopwatch.Start();
#endif

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            if (DisplayArt == null) 
                throw new NullReferenceException(nameof(DisplayArt));

            if (changedLines.Count < 0)
                return;

            foreach (int y in changedLines)
            {
                FormattedText lineText = new(DisplayArt.GetLineString(y), cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);

                DrawingVisual lineVisual = displayArtVisuals[y];
                using DrawingContext dc = lineVisual.RenderOpen();
                dc.DrawText(lineText, new(0, 0));
            }

            changedLines.Clear();

#if WPF_DEBUG || DEBUG
            stopwatch.Stop();
            Console.WriteLine("Updated canvas! (" + stopwatch.ElapsedMilliseconds + " ms)");
#endif
        }

        /// <summary>
        /// Draws the highlights, such as the MouseHighlightRect & SelectionHighlightRect
        /// </summary>
        protected void DrawHighlights()
        {
            using DrawingContext dc = highlightVisual.RenderOpen();

            //Selection Highlight
            if (SelectionHighlightRect != Rect.Empty)
                dc.DrawRectangle(null, new(Brushes.Orange, HighlightRectThickness), GetArtCanvasRectangle(SelectionHighlightRect));

            //Mouse Highlight
            if (MouseHighlightRect != Rect.Empty)
                dc.DrawRectangle(null, new(Brushes.Blue, HighlightRectThickness), GetArtCanvasRectangle(MouseHighlightRect));
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
        private void OnArtDraw(int layerIndex, char? character, Point[] positions)
        {
            foreach(Point point in positions)
                if (!changedLines.Contains((int)point.Y))
                    changedLines.Add((int)point.Y);

            UpdateDisplayArt();
        }

        private void DisplayArtArtLayerAdded(int index, ArtLayer artLayer)
            => DrawDisplayArt();

        private void DisplayArtArtLayerRemoved(int index)
            => DrawDisplayArt();

        private void DisplayArtArtLayerPropertiesChanged(int index, ArtLayer artLayer, bool updateCanvas)
        {
            if(updateCanvas)
                DrawDisplayArt();
        }

        private void DisplayArtCopiedPropertiesOf(object obj)
        {
            UpdateBackground();
            DrawDisplayArt();
        }

        private void DisplayArtCropped(ASCIIArt art)
            => DrawDisplayArt();

        private void DisplayArtSizeChanged(int width, int height)
            => UpdateBackground();
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
                oldDisplayArt.OnArtLayerRemoved -= canvas.DisplayArtArtLayerRemoved;
                oldDisplayArt.OnArtLayerPropertiesChanged -= canvas.DisplayArtArtLayerPropertiesChanged;
                oldDisplayArt.OnCopiedPropertiesOf -= canvas.DisplayArtCopiedPropertiesOf;
                oldDisplayArt.OnCropped -= canvas.DisplayArtCropped;
                oldDisplayArt.OnSizeChanged -= canvas.DisplayArtSizeChanged;
            }

            if (newDisplayArt == null)
                canvas.MouseLeftButtonDown -= canvas.ToolActivateStart;
            else
            {
                newDisplayArt.OnArtLayerAdded += canvas.DisplayArtArtLayerAdded;
                newDisplayArt.OnArtLayerRemoved += canvas.DisplayArtArtLayerRemoved;
                newDisplayArt.OnArtLayerPropertiesChanged += canvas.DisplayArtArtLayerPropertiesChanged;
                newDisplayArt.OnCopiedPropertiesOf += canvas.DisplayArtCopiedPropertiesOf;
                newDisplayArt.OnCropped += canvas.DisplayArtCropped;
                newDisplayArt.OnSizeChanged += canvas.DisplayArtSizeChanged;

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

        private static void OnTextSizePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.UpdateBackground();
            canvas.DrawDisplayArt();
            canvas.DrawHighlights();
        }

        private static void OnArtFontPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.UpdateBackground();
            canvas.DrawDisplayArt();
            canvas.DrawHighlights();
        }

        private static void OnSelectionHighlightRectPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;
            
            canvas.DrawHighlights();
        }

        private static void OnHighlightRectThicknessPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ASCIIArtCanvasVisual canvas)
                return;

            canvas.DrawHighlights();
        }
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
            ArtBrush.Freeze();
            backgroundPen.Freeze();

            _children = new(this)
            {
                backgroundVisual,
                //art visuals
                highlightVisual
            };
            
            MouseMove += OnMouseMove;
            MouseLeave += OnMouseLeave;

            UpdateBackground();
            DrawDisplayArt();
            DrawHighlights();
        }
    }
}
