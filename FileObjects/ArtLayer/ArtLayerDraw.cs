using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class ArtLayerDraw
    {
        private ArtLayer Layer { get; }

        public bool StayInsideSelection { get; set; } = false;

        public int BrushThickness { get; set; } = 1;

        public delegate void ArtDrawEvent(ArtLayer layer, char? character, int x, int y);
        public event ArtDrawEvent? DrewCharacter;

        public ArtLayerDraw(ArtLayer layer)
            => Layer = layer;

        public bool CanDrawOn(int x, int y)
        {
            if (!Layer.IsLayerPointVisible(x, y)) //Point out of bounds of layer
                return false;

            if (App.SelectedArt != Rect.Empty && StayInsideSelection && (x + Layer.OffsetX < App.SelectedArt.Left || x + Layer.OffsetX >= App.SelectedArt.Right || y + Layer.OffsetY < App.SelectedArt.Top || y + Layer.OffsetY >= App.SelectedArt.Bottom))
                return false;

            return true; //No issues
        }

        public bool CanDrawOn(Point position)
            => CanDrawOn((int)position.X, (int)position.Y);

        #region Basics
        public void DrawCharacter(char? character, int x, int y)
        {
            if (!CanDrawOn(x, y))
                return;

            if (Layer.GetCharacter(x, y) == character)
                return;

            Layer.SetCharacter(x, y, character);

            DrewCharacter?.Invoke(Layer, character, x, y);
        }

        public void DrawCharacter(char? character, Point position)
            => DrawCharacter(character, (int)position.X, (int)position.Y);

        public void DrawBrush(char? character, int centerX, int centerY, int radius)
        {
            switch (radius)
            {
                case 0:
                    DrawCharacter(character, centerX, centerY);
                    break;
                case 1:
                    DrawCharacter(character, centerX, centerY);
                    DrawCharacter(character, centerX + 1, centerY);
                    DrawCharacter(character, centerX - 1, centerY);
                    DrawCharacter(character, centerX, centerY + 1);
                    DrawCharacter(character, centerX, centerY - 1);
                    break;
                default:
                    for (int x = -radius; x <= radius; x++)
                        for (int y = -radius; y <= radius; y++)
                            if (x * x + y * y < radius * radius)
                                DrawCharacter(character, centerX + x, centerY + y);
                    break;
            }
        }

        public void DrawBrush(char? character, Point center, int radius)
            => DrawBrush(character, (int)center.X, (int)center.Y, radius);
        #endregion

        #region Shapes
        public void DrawLine(char? character, int startX, int startY, int endX, int endY)
        {
            //Implementation of Bresenham's Line Algorithm
            //Couldn't figure this one out on my own :(

            int dx = Math.Abs(endX - startX);
            int stepX = startX < endX ? 1 : -1;

            int dy = -Math.Abs(endY - startY);
            int stepY = startY < endY ? 1 : -1;

            int x = startX;
            int y = startY;

            int error = dx + dy;

            while (x != endX || y != endY)
            {
                DrawBrush(character, x, y, BrushThickness);

                if (error * 2 >= dy)
                {
                    error += dy;

                    if (x != endX)
                        x += stepX;
                }

                if (error * 2 <= dx)
                {
                    error += dx;

                    if (y != endY)
                        y += stepY;
                }
            }

            DrawBrush(character, x, y, BrushThickness);
        }

        public void DrawLine(char? character, Point point1, Point point2)
            => DrawLine(character, (int)point1.X, (int)point1.Y, (int)point2.X, (int)point2.Y);

        public void DrawRectangle(char? character, int startX, int startY, int width, int height, bool filled = false)
        {
            switch (filled)
            {
                case false:

                    for (int x = startX; x < startX + width; x++)
                    {
                        DrawBrush(character, x, startY, BrushThickness);
                        DrawBrush(character, x, startY + height - 1, BrushThickness);
                    }

                    for (int y = startY; y < startY + height; y++)
                    {
                        DrawBrush(character, startX, y, BrushThickness);
                        DrawBrush(character, startX + width - 1, y, BrushThickness);
                    }

                    break;
                case true:

                    for (int x = startX; x < startX + width; x++)
                        for (int y = startY; y < startY + height; y++)
                            if (x == startX || x == startX + width - 1 || y == startY || y == startY + height - 1)
                                DrawBrush(character, x, y, BrushThickness);
                            else
                                DrawCharacter(character, x, y);

                    break;
            }
        }

        public void DrawRectangle(char? character, Rect rectangle, bool filled = false)
            => DrawRectangle(character, (int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Width, (int)rectangle.Height, filled);

        public void DrawEllipse(char? character, int centerX, int centerY, int radiusX, int radiusY, bool filled = false)
        {
            if (radiusY == 0)
            {
                DrawLine(character, centerX - radiusX, centerY, centerX + radiusX, centerY);
                return;
            }

            //Implementation of Bresenham's Ellipse Algorithm
            // i can't do this on my own lmao

            int x = 0;
            int y = radiusY;

            void Draw4WaySymmetry(int x, int y)
            {
                DrawBrush(character, centerX + x, centerY + y, BrushThickness);
                DrawBrush(character, centerX - x, centerY + y, BrushThickness);
                DrawBrush(character, centerX + x, centerY - y, BrushThickness);
                DrawBrush(character, centerX - x, centerY - y, BrushThickness);

                if (filled)
                {
                    //Fill in horizontal lines because of 2 way horizontal symmetry!
                    for (int circumferencePosX = -x; circumferencePosX < x; circumferencePosX++)
                    {
                        DrawCharacter(character, centerX + circumferencePosX, centerY + y);
                        DrawCharacter(character, centerX + circumferencePosX, centerY - y);
                    }
                }
            }

            double decision1 = (radiusY * radiusY) - (radiusX * radiusX * radiusY) + (0.25f * radiusX * radiusX);
            double decisionX = 0;
            double decisionY = 2 * radiusX * radiusX * y;

            //Region 1
            while (decisionX < decisionY)
            {
                Draw4WaySymmetry(x, y);

                x++;
                decisionX += 2 * radiusY * radiusY;

                if (decision1 < 0)
                {
                    decision1 += decisionX + (radiusY * radiusY);
                }
                else if (decision1 >= 0)
                {
                    y--;
                    decisionY -= 2 * radiusX * radiusX;
                    decision1 += decisionX - decisionY + (radiusY * radiusY);
                }
            }

            //Region 2

            double decision2 = ((radiusY * radiusY) * ((x + 0.5f) * (x + 0.5f))) + ((radiusX * radiusX) * ((y - 1) * (y - 1))) - (radiusX * radiusX * radiusY * radiusY);

            while (y >= 0)
            {
                Draw4WaySymmetry(x, y);

                y--;
                decisionY -= 2 * radiusX * radiusX;

                // Checking and updating parameter
                // value based on algorithm
                if (decision2 > 0)
                {
                    decision2 += (radiusX * radiusX) - decisionY;
                }
                else
                {
                    x++;
                    decisionX += 2 * radiusY * radiusY;
                    decision2 += decisionX - decisionY + (radiusX * radiusX);
                }
            }
        }

        public void DrawEllipse(char? character, Point center, Size size, bool filled = false)
            => DrawEllipse(character, (int)center.X, (int)center.Y, (int)(size.Width / 2), (int)(size.Height / 2), filled);
        #endregion
    }
}
