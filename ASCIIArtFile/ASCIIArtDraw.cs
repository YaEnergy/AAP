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
        
        private bool CanDrawOn(int layerIndex, Point position)
        {
            if (Art.ArtLayers.Count == 0) //No layers
                return false;

            if (layerIndex < 0) //Invalid Layer index
                return false;

            if (!Art.ArtLayers[layerIndex].Visible)  //Layer is hidden
                return false;

            if (position.X < 0 || position.Y < 0 || position.X >= Art.Width || position.Y >= Art.Height) //Position out of bounds
                return false;

            return true; //No issues
        }

        public void DrawCharacter(int layerIndex, char? character, Point position)
        {
            if (!CanDrawOn(layerIndex, position))
                return;

            if (Art.ArtLayers[layerIndex].Data[(int)position.X][(int)position.Y] == character)
                return;

            Art.ArtLayers[layerIndex].Data[(int)position.X][(int)position.Y] = character;
            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, new Point[] { position });
        }

        public void DrawLine(int layerIndex, char? character, Point startPosition, Point endPosition)
        {
            throw new NotImplementedException();
            /*if (layerIndex < 0)
                return;

            if (Art.ArtLayers.Count == 0)
                return;

            if (artMatrixPosition.X < 0 || artMatrixPosition.Y < 0 || artMatrixPosition.X >= Art.Width || artMatrixPosition.Y >= Art.Height)
                return;

            Art.ArtLayers[layerIndex].Data[artMatrixPosition.X][artMatrixPosition.Y] = character;

            OnArtChanged?.Invoke(layerIndex, artMatrixPosition, character);*/
        }

        public void DrawRectangle(int layerIndex, char? character, Rect rectangle)
        {
            List<Point> updatedPositions = new();

            for (int x = (int)rectangle.Left; x < (int)rectangle.Right; x++)
                for (int y = (int)rectangle.Top; y < (int)rectangle.Bottom; y++)
                    if (CanDrawOn(layerIndex, new(x, y) ))
                    {
                        updatedPositions.Add(new(x, y));
                        Art.ArtLayers[layerIndex].Data[x][y] = character;
                    }

            Art.UnsavedChanges = true;

            OnDrawArt?.Invoke(layerIndex, character, updatedPositions.ToArray());
        }
    }
}
