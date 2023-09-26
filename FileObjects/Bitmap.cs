using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AAP.Files
{
    /// <summary>
    /// Provides static methods for BitmapSources
    /// </summary>
    public static class Bitmap
    {
        public static BitmapSource ConvertToGrayscale(BitmapSource bitmap)
            => new FormatConvertedBitmap(bitmap, bitmap.Format, BitmapPalettes.BlackAndWhiteTransparent, 0);

        public static Color GetPixelColor(int x, int y, int width, int height, Color[] pixels)
        {
            if (width <= 0)
                throw new ArgumentException(nameof(width) + " can not be less than or equal to 0!");

            if (height <= 0)
                throw new ArgumentException(nameof(height) + " can not be less than or equal to 0!");

            if (x < 0 || x >= width)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= height)
                throw new ArgumentOutOfRangeException(nameof(y));

            return pixels[x + y * height];
        }

        public static Color GetPixelColor(int x, int y, BitmapSource bitmap, Color[] pixels)
            => GetPixelColor(x, y, bitmap, pixels);

        public static Color[,] GetPixelColors(BitmapSource bitmap)
        {
            if (bitmap.Format != PixelFormats.Bgra32)
                bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);

            byte[] colorBytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            Color[,] colors = new Color[bitmap.PixelWidth, bitmap.PixelHeight];

            bitmap.CopyPixels(colorBytes, bitmap.PixelWidth * 4, 0);
            for (int x = 0; x < bitmap.PixelWidth; x++)
                for (int y = 0; y < bitmap.PixelHeight; y++)
                {
                    byte b = colorBytes[(x + y * bitmap.PixelWidth) * 4];
                    byte g = colorBytes[(x + y * bitmap.PixelWidth) * 4 + 1];
                    byte r = colorBytes[(x + y * bitmap.PixelWidth) * 4 + 2];
                    byte a = colorBytes[(x + y * bitmap.PixelWidth) * 4 + 3];

                    Color color = Color.FromArgb(a, r, g, b);
                    colors[x, y] = color;
                }

            return colors;
        }

        public static TransformedBitmap ScaleBitmap(BitmapSource bitmap, double scaleX, double scaleY)
            => new(bitmap, new ScaleTransform(scaleX, scaleY));

        /// <summary>
        /// Calculate the luminance of a given PixelColor
        /// </summary>
        /// <param name="color"></param>
        /// <returns>The luminace of given color, from 0-1</returns>
        public static double GetLuminanceOf(Color color)
            => 0.2126 * ((double)color.R / 255) + 0.7152 * ((double)color.G / 255) + 0.0722 * ((double)color.B / 255);

        /// <summary>
        /// Calculate the perceived lightness of a given luminance value
        /// </summary>
        /// <param name="luminance"></param>
        /// <returns>The perceived lightness of given luminance, from 0-100</returns>
        public static double GetPerceivedLightnessOf(double luminance)
        {
            if (luminance <= (216 / 24389)) // The CIE standard states 0.008856 but 216/24389 is the intent for 0.008856451679036
                return luminance * (24389 / 27);  // The CIE standard states 903.3, but 24389/27 is the intent, making 903.296296296296296
            else
                return Math.Pow(luminance, (1 / 3)) * 116 - 16;
        }

        /// <summary>
        /// Calculate the perceived lightness of a given PixelColor
        /// </summary>
        /// <param name="color"></param>
        /// <returns>The perceived lightness of given luminance, from 0-100</returns>
        public static double GetPerceivedLightnessOf(Color color)
            => GetPerceivedLightnessOf(GetLuminanceOf(color));
    }
}
