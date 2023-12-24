using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Palette used to convert images. The further into this array, the less dense the character should be.
        /// </summary>
        public char[] Characters { get; set; } = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ".ToCharArray();

        public ImageArtLayerConverter()
        {

        }

        public ArtLayer ToArtLayer(Color[,] pixels)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            ArtLayer layer = new(App.Language.GetString("Default_Layer_Imported"), width, height);

            for (int artX = 0; artX < width; artX++)
            {
                for (int artY = 0; artY < height; artY++)
                {
                    Color color = pixels[artX, artY];

                    if (color.A < 40)
                        layer.SetCharacter(artX, artY, null);
                    else
                    {
                        double luminance = Invert ? 1 - Bitmap.GetLuminanceOf(color) : Bitmap.GetLuminanceOf(color);

                        int index = (int)((Characters.Length - 1) * luminance);
                        char? character = Characters[index] == ASCIIArt.EMPTYCHARACTER ? null : Characters[index];
                        layer.SetCharacter(artX, artY, character);
                    }
                }
            }

            return layer;
        }
    }
}
