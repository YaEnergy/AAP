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

        private double scale = 1;
        /// <summary>
        /// A value between 0 and 1 (excluding 0) that determines the scale of the imported layer
        /// </summary>
        public double Scale
        {
            get => scale;
            set
            {
                if (scale == value)
                    return;

                if (scale > 1 || scale <= 0)
                    throw new ArgumentException("Scale must be between 0 and 1 and not equal to 0!");

                scale = value;
            }
        }

        public ImageArtLayerConverter()
        {

        }

        public ArtLayer ToArtLayer(Color[,] pixels)
        {
            int ogWidth = pixels.GetLength(0);
            int ogHeight = pixels.GetLength(1);

            int artWidth = (int)(ogWidth * Scale);
            int artHeight = (int)(ogHeight * Scale / 2);

            ArtLayer layer = new("Imported Layer", artWidth, artHeight);

            int stepX = (int)Math.Ceiling((double)ogWidth / artWidth);
            int stepY = (int)Math.Ceiling((double)ogHeight / artHeight);

            for (int artX = 0; artX < artWidth; artX++)
            {
                for (int artY = 0; artY < artHeight; artY++)
                {
                    int totalColors = 0;
                    double totalLuminance = 0;
                    double totalAlpha = 0;

                    for (int ogX = 0; ogX < stepX; ogX++)
                        for (int ogY = 0; ogY < stepY; ogY++)
                            if (artX * stepX + ogX < ogWidth && artY * stepY + ogY < ogHeight)
                            {
                                Color color = pixels[artX * stepX + ogX, artY * stepY + ogY];
                                totalColors++;
                                totalLuminance += Invert ? 100 - Bitmap.GetLuminanceOf(color) * 100 : Bitmap.GetLuminanceOf(color) * 100;
                                totalAlpha += color.A;
                            }

                    double averageAlpha = totalAlpha / totalColors;
                    if (averageAlpha < 40)
                    {
                        layer.SetCharacter(artX, artY, null);
                        continue;
                    }

                    double averageLuminance = totalLuminance / totalColors;

                    if (averageLuminance < 5)
                        layer.SetCharacter(artX, artY, '#');
                    else if (averageLuminance < 10)
                        layer.SetCharacter(artX, artY, '$');
                    else if (averageLuminance < 15)
                        layer.SetCharacter(artX, artY, '%');
                    else if (averageLuminance < 20)
                        layer.SetCharacter(artX, artY, '8');
                    else if (averageLuminance < 25)
                        layer.SetCharacter(artX, artY, '*');
                    else if (averageLuminance < 30)
                        layer.SetCharacter(artX, artY, '0');
                    else if (averageLuminance < 35)
                        layer.SetCharacter(artX, artY, '1');
                    else if (averageLuminance < 40)
                        layer.SetCharacter(artX, artY, '?');
                    else if (averageLuminance < 45)
                        layer.SetCharacter(artX, artY, '-');
                    else if (averageLuminance < 50)
                        layer.SetCharacter(artX, artY, '~');
                    else if (averageLuminance < 55)
                        layer.SetCharacter(artX, artY, 'i');
                    else if (averageLuminance < 60)
                        layer.SetCharacter(artX, artY, '!');
                    else if (averageLuminance < 65)
                        layer.SetCharacter(artX, artY, 'l');
                    else if (averageLuminance < 70)
                        layer.SetCharacter(artX, artY, 'I');
                    else if (averageLuminance < 75)
                        layer.SetCharacter(artX, artY, ';');
                    else if (averageLuminance < 80)
                        layer.SetCharacter(artX, artY, ':');
                    else if (averageLuminance < 85)
                        layer.SetCharacter(artX, artY, ',');
                    else if (averageLuminance < 90)
                        layer.SetCharacter(artX, artY, '"');
                    else if (averageLuminance < 95)
                        layer.SetCharacter(artX, artY, '`');
                    else if (averageLuminance <= 100)
                        layer.SetCharacter(artX, artY, null);
                }
            }

            return layer;
        }
    }
}
