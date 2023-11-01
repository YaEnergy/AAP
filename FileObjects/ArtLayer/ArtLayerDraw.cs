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
                DrawFilledCircle(character, x, y, BrushThickness);

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

            DrawFilledCircle(character, x, y, BrushThickness);
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
                        DrawFilledCircle(character, x, startY, BrushThickness);
                        DrawFilledCircle(character, x, startY + height - 1, BrushThickness);
                    }

                    for (int y = startY; y < startY + height; y++)
                    {
                        DrawFilledCircle(character, startX, y, BrushThickness);
                        DrawFilledCircle(character, startX + width - 1, y, BrushThickness);
                    }

                    break;
                case true:

                    for (int x = startX; x < startX + width; x++)
                        for (int y = startY; y < startY + height; y++)
                            if (x == startX || x == startX + width - 1 || y == startY || y == startY + height - 1)
                                DrawFilledCircle(character, x, y, BrushThickness);
                            else
                                DrawCharacter(character, x, y);

                    break;
            }
        }

        public void DrawRectangle(char? character, Rect rectangle, bool filled = false)
            => DrawRectangle(character, (int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Width, (int)rectangle.Height, filled);

        public void DrawCircle(char? character, int centerX, int centerY, int radius)
        {
            if (radius == 0)
            {
                DrawFilledCircle(character, centerX, centerY, BrushThickness);
                return;
            }

            int decision = 3 - (2 * radius);

            int x = 0;
            int y = radius;

            void DrawOctants(int x, int y)
            {
                DrawFilledCircle(character, centerX + x, centerY + y, BrushThickness);
                DrawFilledCircle(character, centerX + y, centerY + x, BrushThickness);
                DrawFilledCircle(character, centerX - y, centerY + x, BrushThickness);
                DrawFilledCircle(character, centerX - x, centerY + y, BrushThickness);
                DrawFilledCircle(character, centerX - x, centerY - y, BrushThickness);
                DrawFilledCircle(character, centerX - y, centerY - x, BrushThickness);
                DrawFilledCircle(character, centerX + y, centerY - x, BrushThickness);
                DrawFilledCircle(character, centerX + x, centerY - y, BrushThickness);
            }

            DrawOctants(x, y);
            while (y >= x)
            {
                x++;

                if (decision < 0)
                {
                    decision += (4 * x) + 6;
                }
                else if (decision >= 0)
                {
                    y--;
                    decision += 4 * (x - y) + 10;
                }

                DrawOctants(x, y);
            }
        }

        public void DrawCircle(char? character, Point center, int radius)
            => DrawCircle(character, (int)center.X, (int)center.Y, radius);

        public void DrawFilledCircle(char? character, int centerX, int centerY, int radius)
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

        public void DrawFilledCircle(char? character, Point center, int radius)
            => DrawFilledCircle(character, (int)center.X, (int)center.Y, radius);

        public void DrawEllipse(char? character, int centerX, int centerY, int width, int height, bool filled = false)
        {
            int x = 0;
            int y = height;

            void DrawOctants(int x, int y)
            {
                DrawFilledCircle(character, centerX + x, centerY + y, BrushThickness);
                DrawFilledCircle(character, centerX + y, centerY + x, BrushThickness);
                DrawFilledCircle(character, centerX - y, centerY + x, BrushThickness);
                DrawFilledCircle(character, centerX - x, centerY + y, BrushThickness);
                DrawFilledCircle(character, centerX - x, centerY - y, BrushThickness);
                DrawFilledCircle(character, centerX - y, centerY - x, BrushThickness);
                DrawFilledCircle(character, centerX + y, centerY - x, BrushThickness);
                DrawFilledCircle(character, centerX + x, centerY - y, BrushThickness);
            }

            throw new NotImplementedException();
        }

        public void DrawEllipse(char? character, Point center, Size size, bool filled = false)
            => DrawEllipse(character, (int)center.X, (int)center.Y, (int)size.Width, (int)size.Height);
    }
}
