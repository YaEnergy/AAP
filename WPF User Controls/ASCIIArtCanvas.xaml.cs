using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AAP
{
    /// <summary>
    /// Interaction logic for ASCIIArtCanvas.xaml
    /// </summary>
    public partial class ASCIIArtCanvas : System.Windows.Controls.UserControl
    {
        private readonly static int DefaultCanvasTextSize = 12;
        private readonly static int DefaultHighlightRectThickness = 4;

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

                InvalidateVisual();
            } 
        }

        public Typeface ArtFont = new("Consolas");
        private SolidColorBrush ArtBrush = System.Windows.Media.Brushes.Black;
        private System.Windows.Point ArtOffset = new(8, 8);

        private int textSize = 16;
        public int TextSize
        {
            get => textSize;
            private set
            {
                textSize = Math.Clamp(value, 4, 128);

                UpdateCanvasSize();
                InvalidateVisual();

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
                highlightRectThickness = Math.Clamp(value, 1, 12);
                
                InvalidateVisual();
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

                InvalidateVisual();
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

                InvalidateVisual();
            }
        }

        private ASCIIArt? displayArt;

        public ASCIIArt? DisplayArt
        {
            get => displayArt;
            set
            {
                displayArt = value;

                if (displayArt != null)
                {
                    displayArt.OnSizeChanged += (width, height) => UpdateCanvasSize();
                }

                UpdateCanvasSize();
                InvalidateVisual();
            }
        }

        public ASCIIArtCanvas()
        {
            InitializeComponent();

            ArtBrush.Freeze();

            UpdateCanvasSize();
            InvalidateVisual();

        }

        public System.Windows.Point GetArtMatrixPoint(System.Windows.Point canvasPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            System.Windows.Size nonOffsetCanvasSize = new(CanvasSize.Width - ArtOffset.X * 2, CanvasSize.Height - ArtOffset.Y * 2);

            //(nonOffsetCanvasSize.Width / DisplayArt.Width) may not be entirely accurate
            System.Windows.Point artMatrixPos = new(Math.Floor((canvasPosition.X - ArtOffset.X) / (nonOffsetCanvasSize.Width / DisplayArt.Width)), Math.Floor((canvasPosition.Y - ArtOffset.Y) / (nonOffsetCanvasSize.Height / DisplayArt.Height)));

            return artMatrixPos;
        }

        public Rect GetCanvasCharacterRectangle(System.Windows.Point artMatrixPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            System.Windows.Size nonOffsetCanvasSize = new(CanvasSize.Width - ArtOffset.X * 2, CanvasSize.Height - ArtOffset.Y * 2);

            //throw new Exception((nonOffsetCanvasSize.Width / DisplayArt.Width).ToString() + " | " + (nonOffsetCanvasSize.Height / DisplayArt.Height).ToString());

            System.Windows.Point canvasPos = new(artMatrixPosition.X * (nonOffsetCanvasSize.Width / DisplayArt.Width) + ArtOffset.X / 2, artMatrixPosition.Y * (nonOffsetCanvasSize.Height / DisplayArt.Height) + ArtOffset.Y);

            return new(canvasPos.X, canvasPos.Y, (nonOffsetCanvasSize.Width / DisplayArt.Width), (nonOffsetCanvasSize.Height / DisplayArt.Height));
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
                throw new NullReferenceException(nameof(DisplayArt));

            mouseHighlightRect = GetArtCanvasRectangle(new(artMatrixPosition, new System.Windows.Size(1, 1)));

            InvalidateVisual();
        }

        private void UpdateCanvasSize()
        {
            FormattedText artText = DisplayArt == null ? new(EmptyDisplayArtText, CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, ArtBrush, 1) : new(DisplayArt.GetArtString(), CultureInfo.InvariantCulture, FlowDirection, ArtFont, TextSize, ArtBrush, 1);

            //CanvasSize = new(width > 0 ? width * TextSize + ArtOffset.X * 2 : 0, height > 0 ? height * LineHeight + ArtOffset.Y * 2 : 0);
            CanvasSize = new(artText.WidthIncludingTrailingWhitespace + ArtOffset.X * 2, (DisplayArt == null ? 1 : DisplayArt.Height) * LineHeight + ArtOffset.Y * 2);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (DisplayArt == null)
                return;

            HighlightArtMatrixPosition(GetArtMatrixPoint(e.GetPosition(this)));
            //mouseHighlightRect = GetArtCanvasRectangle(new(GetArtMatrixPoint(e.GetPosition(this)), new System.Windows.Size(1, 1)));
            //Mouse Highlight Rect stuff here
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            //If no display art
            if (DisplayArt == null)
            {
                FormattedText noFileOpenText = new(EmptyDisplayArtText, cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);

                drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new(System.Windows.Media.Brushes.Black, 1), new(CanvasSize));
                drawingContext.DrawText(noFileOpenText, new(ArtOffset.X / 2, ArtOffset.Y));

                return;
            }

            //Background
            System.Windows.Media.Pen backgroundPen = new(System.Windows.Media.Brushes.Black, 1);
            backgroundPen.Freeze();

            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, backgroundPen, new(CanvasSize));




            //Art
            string artString = DisplayArt.GetArtString();
            string[] lines = artString.Split('\n');

            /*using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(System.Windows.Media.Brushes.Yellow, new(System.Windows.Media.Brushes.Black, 1), new(CanvasSize));
                for (int y = 0; y < lines.Length; y++)//lines.Length; y++)
                {
                    char[] characters = lines[y].ToCharArray();

                    for (int x = 0; x < characters.Length; x++)
                    {
                        //System.Windows.Point screenPoint = PointToScreen(new(ArtOffset.X, LineHeight * y + ArtOffset.Y));
                        FormattedText lineText = new(characters[x].ToString(), cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);
                        drawingContext.DrawText(lineText, new(TextSize * x + ArtOffset.X, LineHeight * y + ArtOffset.Y));
                    }


                    //FormattedText lineText = new(lines[y].ToString(), cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);
                    //drawingContext.DrawText(lineText, new(ArtOffset.X, LineHeight * y + ArtOffset.Y));
                }
            }
            Drawing dr = drawingVisual.Drawing;
            drawingContext.DrawDrawing(dr);*/

            if (lines.Length < 0)
                return;

            for (int y = 0; y < lines.Length; y++)//lines.Length; y++)
            {
                /*char[] characters = lines[y].ToCharArray();

                for (int x = 0; x < characters.Length; x++)
                {
                    //System.Windows.Point screenPoint = PointToScreen(new(ArtOffset.X, LineHeight * y + ArtOffset.Y));
                    FormattedText lineText = new(characters[x].ToString(), cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);
                    drawingContext.DrawText(lineText, new(TextSize * x + ArtOffset.X, LineHeight * y + ArtOffset.Y));
                }*/
                

                FormattedText lineText = new(lines[y].ToString(), cultureInfo, FlowDirection, ArtFont, TextSize, ArtBrush, 1);
                drawingContext.DrawText(lineText, new(ArtOffset.X, LineHeight * y + ArtOffset.Y));
            }


            //Selection Highlight
            if (selectionRect != Rect.Empty)
                drawingContext.DrawRectangle(null, new(System.Windows.Media.Brushes.Yellow, HighlightRectThickness), selectionRect);

            //Mouse Highlight
            if(mouseHighlightRect != Rect.Empty)
                drawingContext.DrawRectangle(null, new(System.Windows.Media.Brushes.Blue, HighlightRectThickness), mouseHighlightRect);
        }
    }
}
