using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AAP
{
    public class ASCIIArtDraw
    {
        private ASCIIArt Art { get; }

        public bool StayInsideSelection { get; set; } = false;

        public int BrushThickness { get; set; } = 1;

        public delegate void ArtDrawEvent(int layerIndex, char? character, int x, int y);
        public event ArtDrawEvent? DrewCharacter;

        public ASCIIArtDraw(ASCIIArt art)
            => Art = art;

        public bool CanDrawOn(int layerIndex, int x, int y)
        {
            if (layerIndex < 0 || layerIndex >= Art.ArtLayers.Count) //Invalid Layer index
                return false;

            if (!Art.ArtLayers[layerIndex].Visible)  //Layer is hidden
                return false;

            if (!Art.ArtLayers[layerIndex].IsPointVisible(x, y)) //Point out of bounds of layer
                return false;

            if (App.SelectedArt != Rect.Empty && StayInsideSelection && (x < App.SelectedArt.Left || x >= App.SelectedArt.Right || y < App.SelectedArt.Top || y >= App.SelectedArt.Bottom)) 
                return false;

            return true; //No issues
        }

        public bool CanDrawOn(int layerIndex, Point position)
            => CanDrawOn(layerIndex, (int)position.X, (int)position.Y);

        public void DrawCharacter(int layerIndex, char? character, int x, int y)
        {
            if (!CanDrawOn(layerIndex, x, y))
                return;

            ArtLayer layer = Art.ArtLayers[layerIndex];
            if (layer.GetCharacter(x - layer.OffsetX, y - layer.OffsetY) == character)
                return;

            layer.SetCharacter(x - layer.OffsetX, y - layer.OffsetY, character);

            DrewCharacter?.Invoke(layerIndex, character, x, y);
        }

        public void DrawCharacter(int layerIndex, char? character, Point position)
            => DrawCharacter(layerIndex, character, (int)position.X, (int)position.Y);

        public void DrawLine(int layerIndex, char? character, int startX, int startY, int endX, int endY)
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
                DrawFilledCircle(layerIndex, character, x, y, BrushThickness);

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

            DrawFilledCircle(layerIndex, character, x, y, BrushThickness);
        }

        public void DrawLine(int layerIndex, char? character, Point point1, Point point2)
            => DrawLine(layerIndex, character, (int)point1.X, (int)point1.Y, (int)point2.X, (int)point2.Y);

        public void DrawRectangle(int layerIndex, char? character, int startX, int startY, int width, int height, bool filled = false)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Must be larger than 0.");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Must be larger than 0.");

            switch(filled)
            {
                case false:

                    for (int x = startX; x < startX + width; x++)
                    {
                        DrawFilledCircle(layerIndex, character, x, startY, BrushThickness);
                        DrawFilledCircle(layerIndex, character, x, startY + height - 1, BrushThickness);
                    }

                    for (int y = startY; y < startY + height; y++)
                    {
                        DrawFilledCircle(layerIndex, character, startX, y, BrushThickness);
                        DrawFilledCircle(layerIndex, character, startX + width - 1, y, BrushThickness);
                    }

                    break;
                case true:

                    for (int x = startX; x < startX + width; x++)
                        for (int y = startY; y < startY + height; y++)
                            DrawFilledCircle(layerIndex, character, x, y, BrushThickness);

                    break;
            }
        }

        public void DrawRectangle(int layerIndex, char? character, Rect rectangle, bool filled = false)
            => DrawRectangle(layerIndex, character, (int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Width, (int)rectangle.Height, filled);

        public void DrawCircle(int layerIndex, char? character, int centerX, int centerY, int radius)
        {
            if (radius == 0)
            {
                DrawFilledCircle(layerIndex, character, centerX, centerY, BrushThickness);
                return;
            }

            int decision = 3 - (2 * radius);

            int x = 0;
            int y = radius;

            void DrawOctants(int x, int y)
            {
                DrawFilledCircle(layerIndex, character, centerX + x, centerY + y, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX + y, centerY + x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - y, centerY + x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - x, centerY + y, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - x, centerY - y, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - y, centerY - x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX + y, centerY - x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX + x, centerY - y, BrushThickness);
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

        public void DrawCircle(int layerIndex, char? character, Point center, int radius)
            => DrawCircle(layerIndex, character, (int)center.X, (int)center.Y, radius);

        public void DrawFilledCircle(int layerIndex, char? character, int centerX, int centerY, int radius)
        {
            switch (radius)
            {
                case 0:
                    DrawCharacter(layerIndex, character, centerX, centerY);
                    break;
                case 1:
                    DrawCharacter(layerIndex, character, centerX, centerY);
                    DrawCharacter(layerIndex, character, centerX + 1, centerY);
                    DrawCharacter(layerIndex, character, centerX - 1, centerY);
                    DrawCharacter(layerIndex, character, centerX, centerY + 1);
                    DrawCharacter(layerIndex, character, centerX, centerY - 1);
                    break;
                default:
                    for (int x = -radius; x <= radius; x++)
                        for (int y = -radius; y <= radius; y++)
                            if (x * x + y * y < radius * radius)
                                DrawCharacter(layerIndex, character, centerX + x, centerY + y);
                    break;
            }
        }

        public void DrawFilledCircle(int layerIndex, char? character, Point center, int radius)
            => DrawFilledCircle(layerIndex, character, (int)center.X, (int)center.Y, radius);

        public void DrawEllipse(int layerIndex, char? character, int centerX, int centerY, int width, int height)
        {
            int x = 0;
            int y = height;

            void DrawOctants(int x, int y)
            {
                DrawFilledCircle(layerIndex, character, centerX + x, centerY + y, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX + y, centerY + x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - y, centerY + x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - x, centerY + y, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - x, centerY - y, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX - y, centerY - x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX + y, centerY - x, BrushThickness);
                DrawFilledCircle(layerIndex, character, centerX + x, centerY - y, BrushThickness);
            }

            throw new NotImplementedException();
        }

        public void DrawEllipse(int layerIndex, char? character, Point center, Size size)
            => DrawEllipse(layerIndex, character, (int)center.X, (int)center.Y, (int)size.Width, (int)size.Height);
    }
}
