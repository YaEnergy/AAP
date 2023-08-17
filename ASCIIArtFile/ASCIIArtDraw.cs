using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace AAP
{
    public class ASCIIArtDraw
    {
        private ASCIIArt Art { get; }

        public delegate void ArtDrawEvent(int layerIndex, char? character, Point[] positions);
        public event ArtDrawEvent? OnDrawArt;

        public ASCIIArtDraw(ASCIIArt art)
            => Art = art;

        private bool CanDrawOn(int layerIndex, int x, int y)
        {
            if (Art.ArtLayers.Count == 0) //No layers
                return false;

            if (layerIndex < 0) //Invalid Layer index
                return false;

            if (!Art.ArtLayers[layerIndex].Visible)  //Layer is hidden
                return false;

            if (!Art.ArtLayers[layerIndex].IsPointVisible(x, y)) //Point out of bounds of layer
                return false;

            return true; //No issues
        }

        private bool CanDrawOn(int layerIndex, Point position)
        {
            if (Art.ArtLayers.Count == 0) //No layers
                return false;

            if (layerIndex < 0) //Invalid Layer index
                return false;

            if (!Art.ArtLayers[layerIndex].Visible)  //Layer is hidden
                return false;

            if (!Art.ArtLayers[layerIndex].IsPointVisible(position)) //Point out of bounds of layer
                return false;

            return true; //No issues
        }

        public void DrawCharacter(int layerIndex, char? character, Point position)
        {
            if (!CanDrawOn(layerIndex, position))
                return;

            ArtLayer layer = Art.ArtLayers[layerIndex];
            Point layerPoint = layer.GetLayerPoint(position);

            if (layer.Data[(int)layerPoint.X][(int)layerPoint.Y] == character)
                return;

            layer.Data[(int)layerPoint.X][(int)layerPoint.Y] = character;
            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, new Point[] { position });

            Art.Update();
        }

        public void DrawLine(int layerIndex, char? character, Point point1, Point point2)
        {
            List<Point> updatedPositions = new();
            ArtLayer artLayer = Art.ArtLayers[layerIndex];

            int startX = (int)point1.X;
            int endX = (int)point2.X;

            int startY = (int)point1.Y;
            int endY = (int)point2.Y;

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
                if (CanDrawOn(layerIndex, x, y))
                {
                    updatedPositions.Add(new(x, y));
                    artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY] = character;
                }

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

            if (CanDrawOn(layerIndex, x, y))
            {
                updatedPositions.Add(new(x, y));
                artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY] = character;
            }

            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, updatedPositions.ToArray());

            Art.Update();
        }

        public void DrawRectangle(int layerIndex, char? character, Rect rectangle)
        {
            List<Point> updatedPositions = new();

            ArtLayer artLayer = Art.ArtLayers[layerIndex];

            for (int x = (int)rectangle.Left; x < (int)rectangle.Right; x++)
                for (int y = (int)rectangle.Top; y < (int)rectangle.Bottom; y++)
                {
                    if (CanDrawOn(layerIndex, x, y))
                    {
                        updatedPositions.Add(new(x, y));
                        artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY] = character;
                    }
                }

            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, updatedPositions.ToArray());

            Art.Update();
        }
    }
}
