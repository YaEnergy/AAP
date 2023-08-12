using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

            int startX = Math.Min((int)point1.X, (int)point2.X);
            int endX = Math.Max((int)point1.X, (int)point2.X);

            int startY = Math.Min((int)point1.Y, (int)point2.Y);
            int endY = Math.Max((int)point1.Y, (int)point2.Y);

            if (CanDrawOn(layerIndex, startX, startY))
            {
                updatedPositions.Add(new(startX, startY));
                artLayer.Data[startX - artLayer.OffsetX][startY - artLayer.OffsetY] = character;
            }

            for (int x = startX; x < endX + 1; x++)
            {
                float prevProgress = Math.Abs((x - startX - 1) / (endX - startX));
                float newProgress = Math.Abs((x - startX) / (endX - startX));

                int prevY = (int)(startY + (endY - startY) * prevProgress);
                int newY = (int)(startY + (endY - startY) * newProgress);

                for (int y = Math.Min(prevY + 1, newY); y < newY + 1; y++)
                {
                    if (CanDrawOn(layerIndex, x, y))
                    {
                        updatedPositions.Add(new(x, y));
                        artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY] = character;
                    }
                }
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
