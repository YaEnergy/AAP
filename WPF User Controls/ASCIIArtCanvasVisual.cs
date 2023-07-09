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

namespace AAP
{
    public class ASCIIArtCanvasVisual : FrameworkElement
    {
        public readonly static int DefaultCanvasTextSize = 16;
        public readonly static int DefaultHighlightRectThickness = 4;

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

        private readonly static string EmptyDisplayArtText = "No art to display!";

        private System.Windows.Size canvasSize = new(0, 0);
        public System.Windows.Size CanvasSize
        {
            get => canvasSize;
            private set
            {
                canvasSize = value;

                Width = canvasSize.Width;
                Height = canvasSize.Height;
            }
        }

        public Typeface ArtFont = new("Consolas");
        private SolidColorBrush ArtBrush = System.Windows.Media.Brushes.Black;
        private System.Windows.Point ArtOffset = new(8, 8);
        private readonly System.Windows.Media.Pen backgroundPen = new(System.Windows.Media.Brushes.Black, 1);

        private int textSize = 16;
        public int TextSize
        {
            get => textSize;
            set
            {
                textSize = value;

                UpdateCanvasSize();
                DrawDisplayArt();
                DrawHighlights();

                Console.WriteLine("Canvas Text Size: " + textSize);
            }
        }

        public double LineHeight { get => textSize * 1.5; }

        private int highlightRectThickness = 2;
        public int HighlightRectThickness
        {
            get => highlightRectThickness;
            private set
            {
                highlightRectThickness = value;

                DrawHighlights();
            }
        }

        private Rect mouseHighlightRect = Rect.Empty;
        public Rect MouseHighlightRect
        {
            get => mouseHighlightRect;
            set
            {
                if (value == mouseHighlightRect)
                    return;

                mouseHighlightRect = value;

                DrawHighlights();
            }
        }

        private Rect selectionRect = Rect.Empty;
        public Rect SelectionRect
        {
            get => selectionRect;
            set
            {
                if (value == selectionRect)
                    return;

                selectionRect = value;

                DrawHighlights();
            }
        }

        public static readonly DependencyProperty DisplayArtProperty =
        DependencyProperty.Register(
            name: "DisplayArt",
            propertyType: typeof(ASCIIArt),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public ASCIIArt? DisplayArt
        {
            get => (ASCIIArt?)GetValue(DisplayArtProperty);
            set
            {
                SetValue(DisplayArtProperty, value);

                if (value != null)
                {
                    value.OnSizeChanged += (width, height) => UpdateCanvasSize();
                    MouseLeftButtonDown += ToolActivateStart;
                }
                else
                    MouseLeftButtonDown -= ToolActivateStart;

                UpdateCanvasSize();
                DrawDisplayArt();
            }
        }

        public static readonly DependencyProperty DisplayArtDrawProperty =
        DependencyProperty.Register(
            name: "DisplayArtDraw",
            propertyType: typeof(ASCIIArtDraw),
            ownerType: typeof(ASCIIArtCanvasVisual),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public ASCIIArtDraw? DisplayArtDraw
        {
            get => (ASCIIArtDraw?)GetValue(DisplayArtDrawProperty);
            set
            {
                SetValue(DisplayArtDrawProperty, value);

                if (value != null)
                {
                    value.OnDrawArt += OnArtDraw;
                }
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
            set => SetValue(ToolProperty, value);
        }

        private readonly List<int> changedLines = new();

        private ArtCanvasViewModel? viewModel;

        public bool CanUseTool { get; set; } = true;

        public System.Windows.Point GetArtMatrixPoint(System.Windows.Point canvasPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            System.Windows.Size nonOffsetCanvasSize = new(CanvasSize.Width - ArtOffset.X * 2, CanvasSize.Height - ArtOffset.Y * 2);

            System.Windows.Point artMatrixPos = new(Math.Floor((canvasPosition.X - ArtOffset.X) / (nonOffsetCanvasSize.Width / DisplayArt.Width)), Math.Floor((canvasPosition.Y - ArtOffset.Y) / (nonOffsetCanvasSize.Height / DisplayArt.Height)));

            return artMatrixPos;
        }

        public Rect GetCanvasCharacterRectangle(System.Windows.Point artMatrixPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            System.Windows.Size nonOffsetCanvasSize = new(CanvasSize.Width - ArtOffset.X * 2, CanvasSize.Height - ArtOffset.Y * 2);

            System.Windows.Point canvasPos = new(artMatrixPosition.X * (nonOffsetCanvasSize.Width / DisplayArt.Width) + ArtOffset.X, artMatrixPosition.Y * (nonOffsetCanvasSize.Height / DisplayArt.Height) + ArtOffset.Y);

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

        public void HighlightArtMatrixPosition(System.Windows.Point artMatrixPosition)
        {
            if (DisplayArt == null)
            {
                mouseHighlightRect = Rect.Empty;
                return;
            }

            mouseHighlightRect = artMatrixPosition.X < DisplayArt.Width && artMatrixPosition.X >= 0 && artMatrixPosition.Y < DisplayArt.Height && artMatrixPosition.Y >= 0 ? new(artMatrixPosition, new System.Windows.Size(1, 1)) : Rect.Empty;

            DrawHighlights();
        }

        public void SelectArtMatrixRect(Rect artMatrixRect)
        {
            if (DisplayArt == null)
            {
                selectionRect = Rect.Empty;
                return;
            }

            selectionRect = artMatrixRect;

            DrawHighlights();
        }


        protected void UpdateCanvasSize()
        {
            FormattedText artText = DisplayArt == null ? new(EmptyDisplayArtText, CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, ArtBrush, 1) : new(DisplayArt.GetArtString(), CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, ArtBrush, 1);
            
            CanvasSize = new(artText.WidthIncludingTrailingWhitespace + ArtOffset.X * 2, (DisplayArt == null ? 1 : DisplayArt.Height) * LineHeight + ArtOffset.Y * 2);
            
            DrawBackground();
        }

        private void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            dc.DrawRectangle(System.Windows.Media.Brushes.White, backgroundPen, new(CanvasSize));
        }
        
        private void DrawDisplayArt()
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

                    string lineString = DisplayArt.GetLineString(y);
                    FormattedText lineText = new(lineString.ToString(), cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);
                    dc.DrawText(lineText, new(0, 0));

                    displayArtVisuals.Insert(y, lineVisual);
                    _children.Insert(1 + y, lineVisual);
                }

            stopwatch.Stop();
            Console.WriteLine("Drew full canvas! (" + stopwatch.ElapsedMilliseconds + " ms)");
        }

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
                Console.WriteLine(y.ToString());

                string lineString = DisplayArt.GetLineString(y);
                FormattedText lineText = new(lineString, cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);

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

        protected void DrawHighlights()
        {
            using DrawingContext dc = highlightVisual.RenderOpen();

            //Selection Highlight
            if (selectionRect != Rect.Empty)
                dc.DrawRectangle(null, new(System.Windows.Media.Brushes.Yellow, HighlightRectThickness), GetArtCanvasRectangle(selectionRect));

            //Mouse Highlight
            if (mouseHighlightRect != Rect.Empty)
                dc.DrawRectangle(null, new(System.Windows.Media.Brushes.Blue, HighlightRectThickness), GetArtCanvasRectangle(mouseHighlightRect));
        }

        #region Tool Functions
        private void ToolActivateStart(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!CanUseTool)
                return;

            if (App.CurrentLayerID < 0)
            {
                System.Windows.MessageBox.Show("Please select a layer!", "No Layer Selected!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MouseMove += ToolActivateUpdate;
            MouseUp += ToolActivateEnd;

            System.Windows.Point artMatrixPosition = GetArtMatrixPoint(e.GetPosition(this));

            //Still uses System.Drawing.Point instead of System.Windows.Point
            Tool?.ActivateStart(new((int)artMatrixPosition.X, (int)artMatrixPosition.Y));
        }

        private void ToolActivateUpdate(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Point artMatrixPosition = GetArtMatrixPoint(e.GetPosition(this));

            //Still uses System.Drawing.Point instead of System.Windows.Point
            Tool?.ActivateUpdate(new((int)artMatrixPosition.X, (int)artMatrixPosition.Y));
        }

        private void ToolActivateEnd(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            MouseMove -= ToolActivateUpdate;
            MouseUp -= ToolActivateEnd;
        }
        #endregion

        //Still uses System.Drawing.Point instead of System.Windows.Point
        private void OnArtDraw(int layerIndex, char? character, System.Drawing.Point[] positions)
        {
            foreach(System.Drawing.Point point in positions)
                if (!changedLines.Contains(point.Y))
                    changedLines.Add(point.Y);

            UpdateDisplayArt();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ArtCanvasViewModel viewModel)
                return;

            switch (e.PropertyName)
            {
                case "CurrentArt":
                    DisplayArt = viewModel.CurrentArt;
                    break;
                case "CurrentArtDraw":
                    DisplayArtDraw = viewModel.CurrentArtDraw;
                    break;
                case "CurrentTool":
                    Tool = viewModel.CurrentTool;
                    break;
                case "CanUseTool":
                    CanUseTool = viewModel.CanUseTool;
                    break;
                case "HighlightThickness":
                    HighlightRectThickness = viewModel.HighlightThickness;
                    break;
                case "TextSize":
                    TextSize = viewModel.TextSize;
                    break;
                case "Selected":
                    SelectArtMatrixRect(viewModel.Selected);
                    break;
                default:
                    break;
            }
        }

        private void OnMouseMove(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DisplayArt == null)
                return;

            HighlightArtMatrixPosition(GetArtMatrixPoint(e.GetPosition(this)));
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = (ArtCanvasViewModel)DataContext;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
                DisplayArt = viewModel.CurrentArt;
                DisplayArtDraw = viewModel.CurrentArtDraw;
                Tool = viewModel.CurrentTool;
                CanUseTool = viewModel.CanUseTool;
                HighlightRectThickness = viewModel.HighlightThickness;
                TextSize = viewModel.TextSize;
                SelectArtMatrixRect(viewModel.Selected);
            }
        }

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

            UpdateCanvasSize();
            DrawDisplayArt();
            DrawHighlights();

            DataContextChanged += OnDataContextChanged;
            viewModel = (ArtCanvasViewModel)DataContext;
            if (viewModel != null)
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }
}
