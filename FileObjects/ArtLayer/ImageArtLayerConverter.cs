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

        public ArtLayer ToArtLayer(Color[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            ArtLayer layer = new("Imported Layer", width, height);

            for (int artX = 0; artX < width; artX++)
            {
                for (int artY = 0; artY < height; artY++)
                {
                    Color color = pixels[artX, artY];
                    double luminance = Invert ? 100 - Bitmap.GetLuminanceOf(color) * 100 : Bitmap.GetLuminanceOf(color) * 100;

                    if (color.A < 40)
                    {
                        layer.SetCharacter(artX, artY, null);
                        continue;
                    }

                    if (luminance < 5)
                        layer.SetCharacter(artX, artY, '#');
                    else if (luminance < 10)
                        layer.SetCharacter(artX, artY, '$');
                    else if (luminance < 15)
                        layer.SetCharacter(artX, artY, '%');
                    else if (luminance < 20)
                        layer.SetCharacter(artX, artY, '8');
                    else if (luminance < 25)
                        layer.SetCharacter(artX, artY, '*');
                    else if (luminance < 30)
                        layer.SetCharacter(artX, artY, '0');
                    else if (luminance < 35)
                        layer.SetCharacter(artX, artY, '1');
                    else if (luminance < 40)
                        layer.SetCharacter(artX, artY, '?');
                    else if (luminance < 45)
                        layer.SetCharacter(artX, artY, '-');
                    else if (luminance < 50)
                        layer.SetCharacter(artX, artY, '~');
                    else if (luminance < 55)
                        layer.SetCharacter(artX, artY, 'i');
                    else if (luminance < 60)
                        layer.SetCharacter(artX, artY, '!');
                    else if (luminance < 65)
                        layer.SetCharacter(artX, artY, 'l');
                    else if (luminance < 70)
                        layer.SetCharacter(artX, artY, 'I');
                    else if (luminance < 75)
                        layer.SetCharacter(artX, artY, ';');
                    else if (luminance < 80)
                        layer.SetCharacter(artX, artY, ':');
                    else if (luminance < 85)
                        layer.SetCharacter(artX, artY, ',');
                    else if (luminance < 90)
                        layer.SetCharacter(artX, artY, '"');
                    else if (luminance < 95)
                        layer.SetCharacter(artX, artY, '`');
                    else if (luminance <= 100)
                        layer.SetCharacter(artX, artY, null);
                }
            }

            return layer;
        }
    }
}
