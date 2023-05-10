using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class Canvas
    {
        public readonly Label CanvasPanel;

        public Canvas(Label canvasPanel)
        {
            CanvasPanel = canvasPanel;
        }

        public void DisplayArtFile(ASCIIArtFile artFile)
        {
            string artString = "";

            for (int y = 0; y < artFile.Height; y++)
            {
                for (int x = 0; x < artFile.Width; x++)
                    artString += artFile.ArtLayers[0].Data[x][y];
                
                artString += "\n";
            }

            CanvasPanel.Text = artString;
        }
    }
}
