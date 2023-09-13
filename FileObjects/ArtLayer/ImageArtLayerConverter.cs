using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AAP.Files
{
    /// <summary>
    /// Converts images into art layers.
    /// </summary>
    public class ImageArtLayerConverter
    {
        /// <summary>
        /// Specifies whether the brightness of the pixels should be inverted or not.
        /// </summary>
        public bool Invert { get; set; } = false;

        public ImageArtLayerConverter()
        {

        }

        public ArtLayer ToArtLayer(int width, int height, Color[,] pixels)
        {
            ArtLayer layer = new("Imported Layer", width, height / 2);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y += 2)
                {
                    if (y / 2 >= layer.Height)
                        continue;

                    double perceivedLightness;
                    Color color = pixels[x, y];

                    if (color.A == 0)
                    {
                        layer.SetCharacter(x, y / 2, null);
                        continue;
                    }

                    if (y + 1 < height)
                    {
                        Color color2 = pixels[x, y + 1];

                        if (color2.A == 0)
                        {
                            layer.SetCharacter(x, y / 2, null);
                            continue;
                        }

                        perceivedLightness = Invert ? 100 - (Bitmap.GetLuminanceOf(color) + Bitmap.GetLuminanceOf(color2)) / 2 * 100 : (Bitmap.GetLuminanceOf(color) + Bitmap.GetLuminanceOf(color2)) / 2 * 100;
                    }
                    else
                        perceivedLightness = Invert ? 100 - Bitmap.GetLuminanceOf(color) * 100 : Bitmap.GetLuminanceOf(color) * 100;

                    if (perceivedLightness <= 5)
                        layer.SetCharacter(x, y / 2, '█');
                    else if (perceivedLightness <= 25)
                        layer.SetCharacter(x, y / 2, '▓');
                    else if (perceivedLightness <= 45)
                        layer.SetCharacter(x, y / 2, '▒');
                    else if (perceivedLightness <= 65)
                        layer.SetCharacter(x, y / 2, '░');
                    else if (perceivedLightness <= 85)
                        layer.SetCharacter(x, y / 2, '|');
                    else if (perceivedLightness <= 95)
                        layer.SetCharacter(x, y / 2, '.');
                    else if (perceivedLightness <= 100)
                        layer.SetCharacter(x, y / 2, null);
                }

            return layer;
        }
    }
}
