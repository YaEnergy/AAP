using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AAP
{
    public partial class Canvas : UserControl
    {
        private readonly static int DefaultCanvasTextSize = 12;
        private readonly static int DefaultHighlightRectangleThickness = 4;

        private Point? oldMousePosition;
        private SizeF trueCanvasSize = new(0f, 0f);

        public Font ArtFont = new("Consolas", 12);
        private SolidBrush ArtBrush = new(Color.Black);
        private Point ArtOffset = new(8, 8);

        private int textSize = 12;
        public int TextSize
        {
            get => textSize;
            private set
            {
                textSize = Math.Clamp(value, 4, 128);

                Invalidate();

                /*if (MainProgram.CurrentArt != null)
                    OnSelectionChanged(MainProgram.Selected);*/

                Console.WriteLine("Canvas Text Size: " + textSize);

                //zoomToolStripMenuItem.Text = $"Zoom: {Math.Truncate((float)canvasTextSize / DefaultCanvasTextSize * 100)}%";
            }
        }

        private int highlightRectangleThickness = 4;
        public int HighlightRectangleThickness
        {
            get => highlightRectangleThickness;
            private set
            {
                void InvalidateRectangles()
                {
                    Invalidate(new Rectangle(highlightRectangle.Location.X - highlightRectangleThickness / 2, highlightRectangle.Location.Y - highlightRectangleThickness / 2, highlightRectangle.Size.Width + highlightRectangleThickness, highlightRectangle.Size.Height + highlightRectangleThickness));
                    Invalidate(new Rectangle(oldHighlightRectangle.Location.X - highlightRectangleThickness / 2, oldHighlightRectangle.Location.Y - highlightRectangleThickness / 2, oldHighlightRectangle.Size.Width + highlightRectangleThickness, oldHighlightRectangle.Size.Height + highlightRectangleThickness));

                    Invalidate(new Rectangle(selectionRectangle.Location.X - highlightRectangleThickness / 2, selectionRectangle.Location.Y - highlightRectangleThickness / 2, selectionRectangle.Size.Width + highlightRectangleThickness, selectionRectangle.Size.Height + highlightRectangleThickness));
                    Invalidate(new Rectangle(oldSelectionRectangle.Location.X - highlightRectangleThickness / 2, oldSelectionRectangle.Location.Y - highlightRectangleThickness / 2, oldSelectionRectangle.Size.Width + highlightRectangleThickness, oldSelectionRectangle.Size.Height + highlightRectangleThickness));
                }

                int oldVal = highlightRectangleThickness;

                if (value < highlightRectangleThickness)
                    InvalidateRectangles();

                highlightRectangleThickness = Math.Clamp(value, 1, 12);

                if (value > oldVal)
                    InvalidateRectangles();

                Update();

                //highlightThicknessNumToolStripMenuItem.Text = $"Highlight Thickness: {Math.Truncate((float)highlightRectangleThickness / DefaultHighlightRectangleThickness * 100)}%";
            }
        }

        private Rectangle oldHighlightRectangle = new();
        private Rectangle highlightRectangle = new();
        public Rectangle HighlightRectangle
        {
            get => highlightRectangle;
            set
            {
                if (value == highlightRectangle)
                    return;

                oldHighlightRectangle = highlightRectangle;
                highlightRectangle = value;

                Invalidate(new Rectangle(highlightRectangle.Location.X - highlightRectangleThickness / 2, highlightRectangle.Location.Y - highlightRectangleThickness / 2, highlightRectangle.Size.Width + highlightRectangleThickness, highlightRectangle.Size.Height + highlightRectangleThickness));
                Invalidate(new Rectangle(oldHighlightRectangle.Location.X - highlightRectangleThickness / 2, oldHighlightRectangle.Location.Y - highlightRectangleThickness / 2, oldHighlightRectangle.Size.Width + highlightRectangleThickness, oldHighlightRectangle.Size.Height + highlightRectangleThickness));
                Update();
            }
        }

        private Rectangle oldSelectionRectangle = new();
        private Rectangle selectionRectangle = new();
        public Rectangle SelectionRectangle
        {
            get => selectionRectangle;
            set
            {
                if (value == selectionRectangle)
                    return;

                oldSelectionRectangle = selectionRectangle;
                selectionRectangle = value;

                Invalidate(new Rectangle(selectionRectangle.Location.X - highlightRectangleThickness / 2, selectionRectangle.Location.Y - highlightRectangleThickness / 2, selectionRectangle.Size.Width + highlightRectangleThickness, selectionRectangle.Size.Height + highlightRectangleThickness));
                Invalidate(new Rectangle(oldSelectionRectangle.Location.X - highlightRectangleThickness / 2, oldSelectionRectangle.Location.Y - highlightRectangleThickness / 2, oldSelectionRectangle.Size.Width + highlightRectangleThickness, oldSelectionRectangle.Size.Height + highlightRectangleThickness));
                Update();
            }
        }

        private ASCIIArt? displayArt = new();

        public ASCIIArt? DisplayArt {
            get => displayArt;
            set
            {
                displayArt = value;
                Invalidate();
            }
        }

        private List<int> changedLines = new();

        public Canvas()
        {
            InitializeComponent();
            
            MainProgram.OnSelectionChanged += SelectArtMatrixRectangle;

            MainProgram.OnCurrentArtChanged += (art, artDraw) =>
            {
                DisplayArt = art;

                if (art != null)
                {
                    changedLines.Capacity = art.Height;
                }

                if (artDraw != null)
                {
                    artDraw.OnDrawArt += (layerIndex, character, positions) =>
                    {
                        foreach (Point point in positions)
                            InvalidateArtMatrixRectangle(new(point, new(1, 1)));

                        Update();
                    }; //for testing
                }
            };
        }

        public Point GetArtMatrixPoint(Point canvasPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            SizeF nonOffsetCanvasSize = trueCanvasSize - new SizeF(ArtOffset.X, ArtOffset.Y);

            //(nonOffsetCanvasSize.Width / DisplayArt.Width) may not be entirely accurate
            PointF artMatrixFloatPos = new((canvasPosition.X + ArtOffset.X) / (nonOffsetCanvasSize.Width / DisplayArt.Width) - 1f, (canvasPosition.Y - ArtOffset.Y) / ArtFont.Height);

            Point artMatrixPos = Point.Truncate(artMatrixFloatPos);

            return artMatrixPos;
        }

        public Rectangle GetCanvasCharacterRectangle(Point artMatrixPosition)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            SizeF nonOffsetCanvasSize = trueCanvasSize - new SizeF(ArtOffset.X, ArtOffset.Y);

            PointF canvasFloatPos = new(artMatrixPosition.X * (nonOffsetCanvasSize.Width / DisplayArt.Width) + 1, artMatrixPosition.Y * ArtFont.Height + ArtOffset.Y);

            Point canvasPos = Point.Truncate(canvasFloatPos);

            return new(canvasPos, TextRenderer.MeasureText(ASCIIArt.EMPTYCHARACTER.ToString(), ArtFont));
        }

        public Rectangle GetArtCanvasRectangle(Rectangle artMatrixRectangle)
        {
            if (DisplayArt == null)
                throw new NullReferenceException(nameof(DisplayArt));

            Rectangle startRectangle = GetCanvasCharacterRectangle(artMatrixRectangle.Location);
            Rectangle endRectangle = GetCanvasCharacterRectangle(artMatrixRectangle.Location + artMatrixRectangle.Size);

            int startX = startRectangle.X < endRectangle.X ? startRectangle.X : endRectangle.X;
            int startY = startRectangle.Y < endRectangle.Y ? startRectangle.Y : endRectangle.Y;
            int sizeX = startRectangle.X < endRectangle.X ? endRectangle.X - startRectangle.X + endRectangle.Width / 2 : startRectangle.X - endRectangle.X;
            int sizeY = startRectangle.Y < endRectangle.Y ? endRectangle.Y - startRectangle.Y : startRectangle.Y - endRectangle.Y;

            return new(new(startX, startY), new(sizeX, sizeY));
        }

        public void InvalidateArtMatrixRectangle(Rectangle artMatrixRectangle)
        {
            Invalidate(GetArtCanvasRectangle(artMatrixRectangle)); //Add line numbers to a list to update

            for(int y = artMatrixRectangle.Top; y < artMatrixRectangle.Bottom; y++)
                if (!changedLines.Contains(y))
                    changedLines.Add(y);
        }

        public void HighlightArtMatrixPosition(Point position)
            => HighlightRectangle = GetArtCanvasRectangle(new(position, new(1, 1)));

        public void SelectArtMatrixRectangle(Rectangle rectangle)
            => SelectionRectangle = GetArtCanvasRectangle(rectangle);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            HighlightArtMatrixPosition(GetArtMatrixPoint(e.Location));
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            HighlightRectangle = Rectangle.Empty;
        }

        public override void Refresh()
        {
            if(displayArt != null)
                for (int y = 0; y < displayArt.Height; y++)
                    if (!changedLines.Contains(y))
                        changedLines.Add(y);

            base.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (TextSize != ArtFont.Size)
            {
                ArtFont.Dispose();
                ArtFont = new("Consolas", TextSize);
            }

            /*if (MainProgram.CurrentArt == null)
            {
                string noFileOpenText = "No File Open!";
                Size = new(TextSize * noFileOpenText.Length, ArtFont.Height);

                e.Graphics.DrawString("No File Open!", ArtFont, ArtBrush, new Point(0, 0));
                return;
            }*/

            e.Graphics.DrawRectangle(new(BackColor, highlightRectangleThickness), oldHighlightRectangle);
            e.Graphics.DrawRectangle(new(BackColor, highlightRectangleThickness), oldSelectionRectangle);

            e.Graphics.DrawRectangle(new(Color.Blue, highlightRectangleThickness), highlightRectangle);
            e.Graphics.DrawRectangle(new(Color.Orange, highlightRectangleThickness), selectionRectangle);

            if (DisplayArt == null)
                return;

            string artString = DisplayArt.GetArtString();
            string[] lines = artString.Split('\n');

            SizeF size = e.Graphics.MeasureString(artString, ArtFont);

            trueCanvasSize = new SizeF(size.Width + ArtOffset.X, ArtFont.Height * DisplayArt.Height + ArtOffset.Y + ArtFont.Height * 2);
            Size = trueCanvasSize.ToSize();

            if (lines.Length < 0)
                return;

            foreach (int changedLine in changedLines)
                e.Graphics.DrawString(lines[changedLine], ArtFont, ArtBrush, new PointF(ArtOffset.X / 2f, ArtFont.Height * changedLine + ArtOffset.Y / 2f));

            /*int startArtMatrixYPosition = Math.Clamp((GetArtMatrixPoint(PointToClient(new(0, fillDock.Location.Y))) ?? Point.Empty).Y, 0, lines.Length - 1);
            int endArtMatrixYPosition = Math.Clamp((GetArtMatrixPoint(PointToClient(new(0, fillDock.Location.Y + fillDock.Size.Height))) ?? Point.Empty).Y, 0, lines.Length - 1);

            for (int y = startArtMatrixYPosition; y < Math.Clamp(endArtMatrixYPosition + 2, 0, lines.Length); y++)
                e.Graphics.DrawString(lines[y], ArtFont, ArtBrush, new PointF(ArtOffset.X / 2f, ArtFont.Height * y + ArtOffset.Y / 2f)); //1 is added so it fits within selection and highlight rectangles better

#if DEBUG
            Console.WriteLine("Started drawing at line " + startArtMatrixYPosition + " and ended at line " + endArtMatrixYPosition);
#endif*/
        }
    }
}
