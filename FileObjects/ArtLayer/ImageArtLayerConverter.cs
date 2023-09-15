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

                    /*if (perceivedLightness <= 5)
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
                        layer.SetCharacter(x, y / 2, null);*/

                    if (perceivedLightness < 5)
                        layer.SetCharacter(x, y / 2, '#');
                    else if (perceivedLightness < 10)
                        layer.SetCharacter(x, y / 2, '$');
                    else if (perceivedLightness < 15)
                        layer.SetCharacter(x, y / 2, '%');
                    else if (perceivedLightness < 20)
                        layer.SetCharacter(x, y / 2, '8');
                    else if (perceivedLightness < 25)
                        layer.SetCharacter(x, y / 2, '*');
                    else if (perceivedLightness < 30)
                        layer.SetCharacter(x, y / 2, '0');
                    else if (perceivedLightness < 35)
                        layer.SetCharacter(x, y / 2, '1');
                    else if (perceivedLightness < 40)
                        layer.SetCharacter(x, y / 2, '?');
                    else if (perceivedLightness < 45)
                        layer.SetCharacter(x, y / 2, '-');
                    else if (perceivedLightness < 50)
                        layer.SetCharacter(x, y / 2, '~');
                    else if (perceivedLightness < 55)
                        layer.SetCharacter(x, y / 2, 'i');
                    else if (perceivedLightness < 60)
                        layer.SetCharacter(x, y / 2, '!');
                    else if (perceivedLightness < 65)
                        layer.SetCharacter(x, y / 2, 'l');
                    else if (perceivedLightness < 70)
                        layer.SetCharacter(x, y / 2, 'I');
                    else if (perceivedLightness < 75)
                        layer.SetCharacter(x, y / 2, ';');
                    else if (perceivedLightness < 80)
                        layer.SetCharacter(x, y / 2, ':');
                    else if (perceivedLightness < 85)
                        layer.SetCharacter(x, y / 2, ',');
                    else if (perceivedLightness < 90)
                        layer.SetCharacter(x, y / 2, '"');
                    else if (perceivedLightness < 95)
                        layer.SetCharacter(x, y / 2, '`');
                    else if (perceivedLightness <= 100)
                        layer.SetCharacter(x, y / 2, null);
                }

            return layer;
        }
    }
}
