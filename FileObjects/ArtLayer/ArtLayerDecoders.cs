using AAP.BackgroundTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AAP.Files
{
    public class TextArtLayerDecoder : FileObjectDecoder<ArtLayer>
    {
        public TextArtLayerDecoder(Stream stream) : base(stream)
        {
            
        }

        public override ArtLayer Decode()
        {
            StreamReader sr = new(DecodeStream);

            List<string> textLines = new();

            while (!sr.EndOfStream)
            {
                string? line = sr.ReadLine();
                if (line != null)
                    textLines.Add(line);
            }

            if (textLines.Count <= 0)
                throw new Exception($"Stream contains no lines!");

            int txtWidth = 0;
            int txtHeight = textLines.Count;

            //Get max width
            foreach (string line in textLines)
                if (line.Length > txtWidth)
                    txtWidth = line.Length;

            ArtLayer textArtLayer = new("Imported Art", txtWidth, txtHeight);

            for (int y = 0; y < txtHeight; y++)
            {
                char[] chars = textLines[y].ToCharArray();
                for (int x = 0; x < txtWidth; x++)
                    textArtLayer.SetCharacter(x, y, x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x]);
            }

            return textArtLayer;
        }

        public override async Task<ArtLayer> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Reading lines...", true));

            StreamReader sr = new(DecodeStream);

            List<string> textLines = new();

            while (!sr.EndOfStream)
            {
                string? line = await sr.ReadLineAsync();
                if (line != null)
                    textLines.Add(line);
            }

            if (textLines.Count <= 0)
                throw new Exception($"Stream contains no lines!");

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating layer...", true));

            int txtWidth = 0;
            int txtHeight = textLines.Count;

            //Get max width
            foreach (string line in textLines)
                if (line.Length > txtWidth)
                    txtWidth = line.Length;

            ArtLayer textArtLayer = new("Imported Art", txtWidth, txtHeight);

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating lines...", true));

            for (int y = 0; y < txtHeight; y++)
            {
                char[] chars = textLines[y].ToCharArray();
                for (int x = 0; x < txtWidth; x++)
                    textArtLayer.SetCharacter(x, y, x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x]);
            }

            return textArtLayer;
        }
    }

    public abstract class ImageArtLayerDecoder : FileObjectDecoder<ArtLayer>
    {
        // Decode Option Properties

        /// <summary>
        /// Converter used to convert Images into Art Layers.
        /// </summary>
        public ImageArtLayerConverter Converter { get; set; }

        public ImageArtLayerDecoder(ImageArtLayerConverter converter, Stream stream) : base(stream)
        {
            Converter = converter;
        }

        public override abstract ArtLayer Decode();
        public override abstract Task<ArtLayer> DecodeAsync(BackgroundTaskToken? taskToken = null);
    }

    public class BitmapArtLayerDecoder : ImageArtLayerDecoder
    {
        public BitmapArtLayerDecoder(ImageArtLayerConverter converter, Stream stream) : base(converter, stream)
        {

        }

        public override ArtLayer Decode()
        {
            BmpBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (decoder.Frames.Count != 1)
                throw new Exception($"Image file must only have 1 frame! ({decoder.Frames.Count} frames)");

            BitmapSource grayscaleBitmap = decoder.Frames[0];
            Color[,] pixelColors = Bitmap.GetPixelColors(grayscaleBitmap);
            
            return Converter.ToArtLayer(grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight, pixelColors);
        }

        public override async Task<ArtLayer> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Decoding...", true));
            BmpBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (decoder.Frames.Count != 1)
                throw new Exception($"Image file must only have 1 frame! ({decoder.Frames.Count} frames)");

            BitmapSource grayscaleBitmap = decoder.Frames[0];

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Getting colors...", true));
            Color[,] pixelColors = await Task.Run(() => Bitmap.GetPixelColors(grayscaleBitmap));

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating layer...", true));
            ArtLayer importedLayer = await Task.Run(() => Converter.ToArtLayer(grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight, pixelColors));

            return importedLayer;
        }
    }

    public class PngArtLayerDecoder : ImageArtLayerDecoder
    {
        public PngArtLayerDecoder(ImageArtLayerConverter converter, Stream stream) : base(converter, stream)
        {

        }

        public override ArtLayer Decode()
        {
            PngBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (decoder.Frames.Count != 1)
                throw new Exception($"Image file must only have 1 frame! ({decoder.Frames.Count} frames)");

            BitmapSource grayscaleBitmap = decoder.Frames[0];
            Color[,] pixelColors = Bitmap.GetPixelColors(grayscaleBitmap);

            return Converter.ToArtLayer(grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight, pixelColors);
        }

        public override async Task<ArtLayer> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Decoding...", true));
            PngBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (decoder.Frames.Count != 1)
                throw new Exception($"Image file must only have 1 frame! ({decoder.Frames.Count} frames)");

            BitmapSource grayscaleBitmap = decoder.Frames[0];

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Getting colors...", true));
            Color[,] pixelColors = await Task.Run(() => Bitmap.GetPixelColors(grayscaleBitmap));

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating layer...", true));
            ArtLayer importedLayer = await Task.Run(() => Converter.ToArtLayer(grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight, pixelColors));

            return importedLayer;
        }
    }

    public class JpegArtLayerDecoder : ImageArtLayerDecoder
    {
        public JpegArtLayerDecoder(ImageArtLayerConverter converter, Stream stream) : base(converter, stream)
        {

        }

        public override ArtLayer Decode()
        {
            JpegBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (decoder.Frames.Count != 1)
                throw new Exception($"Image file must only have 1 frame! ({decoder.Frames.Count} frames)");

            BitmapSource grayscaleBitmap = decoder.Frames[0];
            Color[,] pixelColors = Bitmap.GetPixelColors(grayscaleBitmap);

            return Converter.ToArtLayer(grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight, pixelColors);
        }

        public override async Task<ArtLayer> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Decoding...", true));
            JpegBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (decoder.Frames.Count != 1)
                throw new Exception($"Image file must only have 1 frame! ({decoder.Frames.Count} frames)");

            BitmapSource grayscaleBitmap = decoder.Frames[0];

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Getting colors...", true));
            Color[,] pixelColors = await Task.Run(() => Bitmap.GetPixelColors(grayscaleBitmap));

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating layer...", true));
            ArtLayer importedLayer = await Task.Run(() => Converter.ToArtLayer(grayscaleBitmap.PixelWidth, grayscaleBitmap.PixelHeight, pixelColors));

            return importedLayer;
        }
    }

    public class GifArtLayerArrayDecoder : FileObjectDecoder<ArtLayer[]>
    {
        /// <summary>
        /// Converter used to convert Images into Art Layers.
        /// </summary>
        public ImageArtLayerConverter Converter { get; set; }

        public GifArtLayerArrayDecoder(ImageArtLayerConverter converter, Stream stream) : base(stream)
        {
            Converter = converter;
        }

        public override ArtLayer[] Decode()
        {
            GifBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            if (decoder.Frames.Count == 0)
                throw new Exception($"Image file must have atleast 1 frame! ({decoder.Frames.Count} frames)");

            ArtLayer[] layerFrames = new ArtLayer[decoder.Frames.Count];
            for (int i = 0; i < decoder.Frames.Count; i++)
            {
                BitmapSource bitmap = decoder.Frames[i];
                Color[,] pixelColors = Bitmap.GetPixelColors(bitmap);

                layerFrames[i] = Converter.ToArtLayer(bitmap.PixelWidth, bitmap.PixelHeight, pixelColors);
            }

            return layerFrames;
        }

        public override async Task<ArtLayer[]> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Decoding...", true));

            List<Color[,]> pixelColorsFrames = new();
            await Task.Run(() =>
            {
                GifBitmapDecoder decoder = new(DecodeStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

                if (decoder.Frames.Count == 0)
                    throw new Exception($"Image file must have atleast 1 frame! ({decoder.Frames.Count} frames)");

                for (int i = 0; i < decoder.Frames.Count; i++)
                {
                    BitmapSource bitmap = decoder.Frames[i];
                    bitmap.Freeze();

                    taskToken?.ReportProgress((int)((double)i / decoder.Frames.Count * 100), new($"Getting colors... ({i}/{decoder.Frames.Count} frames)", false));

                    pixelColorsFrames.Add(Bitmap.GetPixelColors(bitmap));
                }
            });
            

            ArtLayer[] layerFrames = new ArtLayer[pixelColorsFrames.Count];
            for (int i = 0; i < pixelColorsFrames.Count; i++)
            {
                int width = pixelColorsFrames[i].GetUpperBound(0) + 1;
                int height = pixelColorsFrames[i].GetUpperBound(1) + 1;

                taskToken?.ReportProgress((int)((double)i / pixelColorsFrames.Count * 100), new($"Creating layer... ({i}/{pixelColorsFrames.Count} frames)", false));
                
                ArtLayer importedLayer = await Task.Run(() => Converter.ToArtLayer(width, height, pixelColorsFrames[i]));
                layerFrames[i] = importedLayer;
            }

            return layerFrames;
        }
    }
}
