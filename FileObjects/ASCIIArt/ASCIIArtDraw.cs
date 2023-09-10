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

        public delegate void ArtDrawEvent(int layerIndex, char? character, int x, int y);
        public event ArtDrawEvent? DrewCharacter;

        public ASCIIArtDraw(ASCIIArt art)
            => Art = art;

        private bool CanDrawOn(int layerIndex, int x, int y, bool stayInsideSelection = false)
        {
            if (Art.ArtLayers.Count == 0) //No layers
                return false;

            if (layerIndex < 0) //Invalid Layer index
                return false;

            if (!Art.ArtLayers[layerIndex].Visible)  //Layer is hidden
                return false;

            if (!Art.ArtLayers[layerIndex].IsPointVisible(x, y)) //Point out of bounds of layer
                return false;

            if (App.SelectedArt != Rect.Empty && stayInsideSelection && (x < App.SelectedArt.Left || x >= App.SelectedArt.Right || y < App.SelectedArt.Top || y >= App.SelectedArt.Bottom)) 
                return false;

            return true; //No issues
        }

        private bool CanDrawOn(int layerIndex, Point position, bool stayInsideSelection = false)
        {
            if (Art.ArtLayers.Count == 0) //No layers
                return false;

            if (layerIndex < 0) //Invalid Layer index
                return false;

            if (!Art.ArtLayers[layerIndex].Visible)  //Layer is hidden
                return false;

            if (!Art.ArtLayers[layerIndex].IsPointVisible(position)) //Point out of bounds of layer
                return false;

            if (App.SelectedArt != Rect.Empty && stayInsideSelection && (position.X < App.SelectedArt.Left || position.X >= App.SelectedArt.Right || position.Y < App.SelectedArt.Top || position.Y >= App.SelectedArt.Bottom))
                return false;

            return true; //No issues
        }

        public void DrawCharacter(int layerIndex, char? character, int x, int y, bool stayInsideSelection = false)
        {
            if (!CanDrawOn(layerIndex, x, y, stayInsideSelection))
                return;

            if (Art.ArtLayers[layerIndex].GetCharacter(x, y) == character)
                return;

            Art.SetCharacter(layerIndex, x, y, character);

            DrewCharacter?.Invoke(layerIndex, character, x, y);
        }

        public void DrawCharacter(int layerIndex, char? character, Point position, bool stayInsideSelection = false)
            => DrawCharacter(layerIndex, character, (int)position.X, (int)position.Y, stayInsideSelection);

        public void DrawLine(int layerIndex, char? character, int startX, int startY, int endX, int endY, int thickness, bool stayInsideSelection = false)
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
                DrawFilledCircle(layerIndex, character, x, y, thickness, stayInsideSelection);

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

            DrawFilledCircle(layerIndex, character, x, y, thickness, stayInsideSelection);
        }

        public void DrawLine(int layerIndex, char? character, Point point1, Point point2, int thickness, bool stayInsideSelection = false)
        {
            int startX = (int)point1.X;
            int endX = (int)point2.X;

            int startY = (int)point1.Y;
            int endY = (int)point2.Y;

            DrawLine(layerIndex, character, startX, startY, endX, endY, thickness, stayInsideSelection);
        }

        public void DrawFilledRectangle(int layerIndex, char? character, Rect rectangle, bool stayInsideSelection = false)
        {
            for (int x = (int)rectangle.Left; x < (int)rectangle.Right; x++)
                for (int y = (int)rectangle.Top; y < (int)rectangle.Bottom; y++)
                    DrawCharacter(layerIndex, character, x, y, stayInsideSelection);
        }

        public void DrawCircle(int layerIndex, char? character, int centerX, int centerY, int radius, bool stayInsideSelection = false)
        {
            if (radius == 0)
            {
                DrawCharacter(layerIndex, character, centerX, centerY, stayInsideSelection);
                return;
            }

            int decision = 3 - (2 * radius);

            int x = 0;
            int y = radius;

            void DrawOctants(int x, int y)
            {
                DrawCharacter(layerIndex, character, centerX + x, centerY + y, stayInsideSelection);
                DrawCharacter(layerIndex, character, centerX + y, centerY + x, stayInsideSelection);
                DrawCharacter(layerIndex, character, centerX - y, centerY + x, stayInsideSelection);
                DrawCharacter(layerIndex, character, centerX - x, centerY + y, stayInsideSelection);
                DrawCharacter(layerIndex, character, centerX - x, centerY - y, stayInsideSelection);
                DrawCharacter(layerIndex, character, centerX - y, centerY - x, stayInsideSelection);
                DrawCharacter(layerIndex, character, centerX + y, centerY - x, stayInsideSelection);
                DrawCharacter(layerIndex, character, centerX + x, centerY - y, stayInsideSelection);
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

        public void DrawCircle(int layerIndex, char? character, Point center, int radius, bool stayInsideSelection = false)
        {
            int centerX = (int)center.X;
            int centerY = (int)center.Y;

            DrawCircle(layerIndex, character, centerX, centerY, radius, stayInsideSelection);
        }

        public void DrawFilledCircle(int layerIndex, char? character, int centerX, int centerY, int radius, bool stayInsideSelection = false)
        {
            switch (radius)
            {
                case 0:
                    DrawCharacter(layerIndex, character, centerX, centerY, stayInsideSelection);
                    break;
                case 1:
                    DrawCharacter(layerIndex, character, centerX, centerY, stayInsideSelection);
                    DrawCharacter(layerIndex, character, centerX + 1, centerY, stayInsideSelection);
                    DrawCharacter(layerIndex, character, centerX - 1, centerY, stayInsideSelection);
                    DrawCharacter(layerIndex, character, centerX, centerY + 1, stayInsideSelection);
                    DrawCharacter(layerIndex, character, centerX, centerY - 1, stayInsideSelection);
                    break;
                default:
                    for (int x = -radius; x <= radius; x++)
                        for (int y = -radius; y <= radius; y++)
                            if (x * x + y * y < radius * radius)
                                DrawCharacter(layerIndex, character, centerX + x, centerY + y, stayInsideSelection);
                    break;
            }
        }

        public void DrawFilledCircle(int layerIndex, char? character, Point center, int radius, bool stayInsideSelection = false)
        {
            int centerX = (int)center.X;
            int centerY = (int)center.Y;

            DrawFilledCircle(layerIndex, character, centerX, centerY, radius, stayInsideSelection);
        }

        public void FloodFillArtPosWithCharacter(int layerIndex, char? character, Point artPos, bool eightDirectional = false, bool stayInsideSelection = false)
        {
            Stack<Point> positionStack = new();

            ArtLayer artLayer = Art.ArtLayers[layerIndex];

            if (!CanDrawOn(layerIndex, artPos, stayInsideSelection))
                return;

            char? findCharacter = artLayer.GetCharacter(artLayer.GetLayerPoint(artPos));
            if (findCharacter == character)
                return; //No changes will be made

            //Flood Fill Algorithm
            positionStack.Push(artPos);

            while (positionStack.Count > 0)
            {
                Point pos = positionStack.Pop();

                int x = (int)pos.X;
                int y = (int)pos.Y;

                DrawCharacter(layerIndex, character, x, y);

                if (x + 1 - artLayer.OffsetX < artLayer.Width)
                    if (artLayer.GetCharacter(artLayer.GetLayerPoint(x + 1, y)) == findCharacter && CanDrawOn(layerIndex, x + 1, y, stayInsideSelection))
                        positionStack.Push(new(x + 1, y));

                if (x - 1 - artLayer.OffsetX >= 0)
                    if (artLayer.GetCharacter(artLayer.GetLayerPoint(x - 1, y)) == findCharacter && CanDrawOn(layerIndex, x - 1, y, stayInsideSelection))
                        positionStack.Push(new(x - 1, y));

                if (y + 1 - artLayer.OffsetY < artLayer.Height)
                    if (artLayer.GetCharacter(artLayer.GetLayerPoint(x, y + 1)) == findCharacter && CanDrawOn(layerIndex, x, y + 1, stayInsideSelection))
                        positionStack.Push(new(x, y + 1));

                if (y - 1 - artLayer.OffsetY >= 0)
                    if (artLayer.GetCharacter(artLayer.GetLayerPoint(x, y - 1)) == findCharacter && CanDrawOn(layerIndex, x, y - 1, stayInsideSelection))
                        positionStack.Push(new(x, y - 1));

                if (eightDirectional)
                {
                    if (x + 1 - artLayer.OffsetX < artLayer.Width)
                    {
                        if (y + 1 - artLayer.OffsetY < artLayer.Height)
                            if (artLayer.GetCharacter(artLayer.GetLayerPoint(x + 1, y + 1)) == findCharacter && CanDrawOn(layerIndex, x + 1, y + 1, stayInsideSelection))
                                positionStack.Push(new(x + 1, y + 1));

                        if (y - 1 - artLayer.OffsetY >= 0)
                            if (artLayer.GetCharacter(artLayer.GetLayerPoint(x + 1, y - 1)) == findCharacter && CanDrawOn(layerIndex, x + 1, y - 1, stayInsideSelection))
                                positionStack.Push(new(x + 1, y - 1));
                    }

                    if (x - 1 - artLayer.OffsetX >= 0)
                    {
                        if (y + 1 - artLayer.OffsetY < artLayer.Height)
                            if (artLayer.GetCharacter(artLayer.GetLayerPoint(x - 1, y + 1)) == findCharacter && CanDrawOn(layerIndex, x - 1, y + 1, stayInsideSelection))
                                positionStack.Push(new(x - 1, y + 1));

                        if (y - 1 - artLayer.OffsetY >= 0)
                            if (artLayer.GetCharacter(artLayer.GetLayerPoint(x - 1, y - 1)) == findCharacter && CanDrawOn(layerIndex, x - 1, y - 1, stayInsideSelection))
                                positionStack.Push(new(x - 1, y - 1));
                    }
                }
            }
        }
    }
}
