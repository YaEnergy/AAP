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

        public delegate void ArtDrawEvent(int layerIndex, char? character, Point[] positions);
        public event ArtDrawEvent? OnDrawArt;

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

        public void DrawCharacter(int layerIndex, char? character, Point position, bool stayInsideSelection = false)
        {
            if (!CanDrawOn(layerIndex, position, stayInsideSelection))
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

        public void DrawLine(int layerIndex, char? character, Point point1, Point point2, bool stayInsideSelection = false)
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
                if (CanDrawOn(layerIndex, x, y, stayInsideSelection))
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

            if (CanDrawOn(layerIndex, x, y, stayInsideSelection))
            {
                updatedPositions.Add(new(x, y));
                artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY] = character;
            }

            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, updatedPositions.ToArray());

            Art.Update();
        }

        public void DrawRectangle(int layerIndex, char? character, Rect rectangle, bool stayInsideSelection = false)
        {
            List<Point> updatedPositions = new();

            ArtLayer artLayer = Art.ArtLayers[layerIndex];

            for (int x = (int)rectangle.Left; x < (int)rectangle.Right; x++)
                for (int y = (int)rectangle.Top; y < (int)rectangle.Bottom; y++)
                {
                    if (CanDrawOn(layerIndex, x, y, stayInsideSelection))
                    {
                        updatedPositions.Add(new(x, y));
                        artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY] = character;
                    }
                }

            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, updatedPositions.ToArray());

            Art.Update();
        }

        public void FloodFillArtPosWithCharacter(int layerIndex, char? character, Point artPos, bool eightDirectional = false, bool stayInsideSelection = false)
        {
            Stack<Point> positionStack = new();
            List<Point> updatedPositions = new();

            ArtLayer artLayer = Art.ArtLayers[layerIndex];

            if (!CanDrawOn(layerIndex, artPos, stayInsideSelection))
                return;

            char? findCharacter = artLayer.Data[(int)artPos.X - artLayer.OffsetX][(int)artPos.Y - artLayer.OffsetY];
            if (findCharacter == character)
                return; //No changes will be made

            //Flood Fill Algorithm
            positionStack.Push(artPos);

            while (positionStack.Count > 0)
            {
                Point pos = positionStack.Pop();

                int x = (int)pos.X;
                int y = (int)pos.Y;
                
                artLayer.Data[x - artLayer.OffsetX][y - artLayer.OffsetY] = character;
                updatedPositions.Add(pos);

                if (x + 1 - artLayer.OffsetX < artLayer.Width)
                    if (artLayer.Data[x + 1 - artLayer.OffsetX][y - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x + 1, y, stayInsideSelection))
                        positionStack.Push(new(x + 1, y));

                if (x - 1 - artLayer.OffsetX >= 0)
                    if (artLayer.Data[x - 1 - artLayer.OffsetX][y - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x - 1, y, stayInsideSelection))
                        positionStack.Push(new(x - 1, y));

                if (y + 1 - artLayer.OffsetY < artLayer.Height)
                    if (artLayer.Data[x - artLayer.OffsetX][y + 1 - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x, y + 1, stayInsideSelection))
                        positionStack.Push(new(x, y + 1));

                if (y - 1 - artLayer.OffsetY >= 0)
                    if (artLayer.Data[x - artLayer.OffsetX][y - 1 - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x, y - 1, stayInsideSelection))
                        positionStack.Push(new(x, y - 1));

                if (eightDirectional)
                {
                    if (x + 1 - artLayer.OffsetX < artLayer.Width)
                    {
                        if (y + 1 - artLayer.OffsetY < artLayer.Height)
                            if (artLayer.Data[x + 1 - artLayer.OffsetX][y + 1 - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x + 1, y + 1, stayInsideSelection))
                                positionStack.Push(new(x + 1, y + 1));

                        if (y - 1 - artLayer.OffsetY >= 0)
                            if (artLayer.Data[x + 1 - artLayer.OffsetX][y - 1 - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x + 1, y - 1, stayInsideSelection))
                                positionStack.Push(new(x + 1, y - 1));
                    }

                    if (x - 1 - artLayer.OffsetX >= 0)
                    {
                        if (y + 1 - artLayer.OffsetY < artLayer.Height)
                            if (artLayer.Data[x - 1 - artLayer.OffsetX][y + 1 - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x - 1, y + 1, stayInsideSelection))
                                positionStack.Push(new(x - 1, y + 1));

                        if (y - 1 - artLayer.OffsetY >= 0)
                            if (artLayer.Data[x - 1 - artLayer.OffsetX][y - 1 - artLayer.OffsetY] == findCharacter && CanDrawOn(layerIndex, x - 1, y - 1, stayInsideSelection))
                                positionStack.Push(new(x - 1, y - 1));
                    }
                }
            }

            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, updatedPositions.ToArray());

            Art.Update();
        }
    }
}
