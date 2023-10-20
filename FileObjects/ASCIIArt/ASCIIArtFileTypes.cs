using AAP.BackgroundTasks;
using AAP.Files;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AAP.Files
{
    public class TextASCIIArtEncoder : FileObjectEncoder<ASCIIArt>
    {
        public TextASCIIArtEncoder(ASCIIArt fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            string artString = FileObject.GetArtString();

            StreamWriter sw = new(EncodeStream);
            sw.AutoFlush = true;

            sw.Write(artString);
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Getting art string...", true));

            Task<string> artStringTask = Task.Run(() => FileObject.GetArtString());
            string artString = await artStringTask;

            taskToken?.ReportProgress(50, new BackgroundTaskProgressArgs("Writing to file...", true));

            StreamWriter sw = new(EncodeStream);
            sw.AutoFlush = true;

            await sw.WriteAsync(artString);
        }
    }

    public class TextASCIIArtDecoder : FileObjectDecoder<ASCIIArt>
    {
        public TextASCIIArtDecoder(Stream stream) : base(stream)
        {
            
        }

        public override ASCIIArt Decode()
        {
            TextArtLayerDecoder layerDecoder = new(DecodeStream);

            ArtLayer txtArtLayer = layerDecoder.Decode();

            ASCIIArt textArt = new();

            textArt.SetSize(txtArtLayer.Width, txtArtLayer.Height);
            textArt.CreatedInVersion = ASCIIArt.VERSION;

            textArt.ArtLayers.Add(txtArtLayer);

            return textArt;
        }

        public override async Task<ASCIIArt> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            TextArtLayerDecoder layerDecoder = new(DecodeStream);

            ArtLayer textArtLayer = await layerDecoder.DecodeAsync(taskToken);

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating art...", true));

            ASCIIArt textArt = new();

            textArt.SetSize(textArtLayer.Width, textArtLayer.Height);
            textArt.CreatedInVersion = ASCIIArt.VERSION;

            textArt.ArtLayers.Add(textArtLayer);

            return textArt;
        }
    }

    #region Image Decoders & Encoders
    public abstract class ImageASCIIArtEncoder : FileObjectEncoder<ASCIIArt>
    {
        public ImageASCIIArtEncodeOptions EncodeOptions { get; set; } = new();

        public ImageASCIIArtEncoder(ASCIIArt fileObject, Stream stream) : base(fileObject, stream)
        {
            
        }

        public BitmapFrame GetArtBitmapFrame()
        {
            DrawingVisual drawingVisual = new();

            Brush textBrush = new SolidColorBrush(EncodeOptions.TextColor);
            textBrush.Freeze();

            Brush backgroundBrush = new SolidColorBrush(EncodeOptions.BackgroundColor);
            backgroundBrush.Freeze();

            Typeface artTypeface = new(Properties.Settings.Default.CanvasTypefaceSource);
            double defaultWidth = new FormattedText("A", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, EncodeOptions.TextSize, textBrush, 1).Width;

            FormattedText[] columnTexts = new FormattedText[FileObject.Width];
            double[] columnWidths = new double[FileObject.Width];
            double totalWidth = 0;

            Stopwatch sw = Stopwatch.StartNew();

            for (int x = 0; x < FileObject.Width; x++)
            {
                string columnString = "";
                for (int y = 0; y < FileObject.Height; y++)
                    columnString += (FileObject.GetCharacter(x, y) ?? ASCIIArt.EMPTYCHARACTER) + "\n";

                FormattedText columnText = new(columnString, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, EncodeOptions.TextSize, textBrush, 1);
                columnText.LineHeight = EncodeOptions.TextSize * 1.5;
                columnTexts[x] = columnText;

                if (string.IsNullOrWhiteSpace(columnString))
                {
                    columnWidths[x] = defaultWidth;
                    totalWidth += defaultWidth;
                }
                else
                {
                    columnWidths[x] = columnText.WidthIncludingTrailingWhitespace;
                    totalWidth += columnText.WidthIncludingTrailingWhitespace;
                }
            }

            DrawingContext dc = drawingVisual.RenderOpen();

            dc.DrawRectangle(backgroundBrush, null, new(0, 0, totalWidth, EncodeOptions.TextSize * 1.5 * FileObject.Height));

            double posX = 0;
            for (int x = 0; x < FileObject.Width; x++)
            {
                dc.DrawText(columnTexts[x], new(posX, 0));

                posX += columnWidths[x];
            }

            dc.Close();

            sw.Stop();
            ConsoleLogger.Log($"Drew frame columns in {sw.ElapsedMilliseconds} ms");

            int width = (int)drawingVisual.ContentBounds.Width;
            int height = (int)drawingVisual.ContentBounds.Height;

            sw.Restart();

            RenderTargetBitmap bmp = new(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            bmp.Freeze();

            sw.Stop();
            ConsoleLogger.Log($"Rendered frame in {sw.ElapsedMilliseconds} ms");

            BitmapFrame frame = BitmapFrame.Create(bmp);
            frame.Freeze();

            return frame;
        }

        public override abstract void Encode();

        public override abstract Task EncodeAsync(BackgroundTaskToken? taskToken = null);
    }

    public abstract class ImageASCIIArtDecoder : FileObjectDecoder<ASCIIArt>
    {
        public ImageASCIIArtDecodeOptions DecodeOptions { get; set; }

        public ImageASCIIArtDecoder(ImageASCIIArtDecodeOptions options, Stream stream) : base(stream)
        {
            DecodeOptions = options;
        }

        public override abstract ASCIIArt Decode();

        public override abstract Task<ASCIIArt> DecodeAsync(BackgroundTaskToken? taskToken = null);
    }

    public class BitmapASCIIArtEncoder : ImageASCIIArtEncoder
    {
        public BitmapASCIIArtEncoder(ASCIIArt fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            BitmapFrame bmp = GetArtBitmapFrame();

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            BmpBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);

            encoder.Save(EncodeStream);
            EncodeStream.Flush();
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Task<BitmapFrame> getBitmapFrameTask = Task.Run(GetArtBitmapFrame);
            BitmapFrame bmp = await getBitmapFrameTask;

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            await Task.Run(() =>
            {
                BmpBitmapEncoder encoder = new();
                encoder.Frames.Add(bmp);

                encoder.Save(EncodeStream);
            });

            await EncodeStream.FlushAsync();
        }
    }

    public class BitmapASCIIArtDecoder : ImageASCIIArtDecoder
    {
        public BitmapASCIIArtDecoder(ImageASCIIArtDecodeOptions options, Stream stream) : base(options, stream)
        {

        }

        public override ASCIIArt Decode()
        {
            BitmapArtLayerDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer layer = layerDecoder.Decode();

            ASCIIArt art = new();

            art.SetSize(layer.Width, layer.Height);
            art.CreatedInVersion = ASCIIArt.VERSION;

            art.ArtLayers.Add(layer);

            return art;
        }

        public override async Task<ASCIIArt> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            BitmapArtLayerDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer layer = await layerDecoder.DecodeAsync(taskToken);

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating art...", true));

            ASCIIArt art = new();

            art.SetSize(layer.Width, layer.Height);
            art.CreatedInVersion = ASCIIArt.VERSION;

            art.ArtLayers.Add(layer);

            return art;
        }
    }

    public class PngASCIIArtEncoder : ImageASCIIArtEncoder
    {
        public PngASCIIArtEncoder(ASCIIArt fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            BitmapFrame bmp = GetArtBitmapFrame();

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);

            encoder.Save(EncodeStream);
            EncodeStream.Flush();
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Task<BitmapFrame> getBitmapFrameTask = Task.Run(GetArtBitmapFrame);
            BitmapFrame bmp = await getBitmapFrameTask;

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            await Task.Run(() =>
            {
                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(bmp);

                encoder.Save(EncodeStream);
            });

            await EncodeStream.FlushAsync();
        }
    }

    public class PngASCIIArtDecoder : ImageASCIIArtDecoder
    {
        public PngASCIIArtDecoder(ImageASCIIArtDecodeOptions options, Stream stream) : base(options, stream)
        {

        }

        public override ASCIIArt Decode()
        {
            PngArtLayerDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer layer = layerDecoder.Decode();

            ASCIIArt art = new();

            art.SetSize(layer.Width, layer.Height);
            art.CreatedInVersion = ASCIIArt.VERSION;

            art.ArtLayers.Add(layer);

            return art;
        }

        public override async Task<ASCIIArt> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            PngArtLayerDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer layer = await layerDecoder.DecodeAsync(taskToken);

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating art...", true));

            ASCIIArt art = new();

            art.SetSize(layer.Width, layer.Height);
            art.CreatedInVersion = ASCIIArt.VERSION;

            art.ArtLayers.Add(layer);

            return art;
        }
    }

    public class JpegASCIIArtEncoder : ImageASCIIArtEncoder
    {
        public JpegASCIIArtEncoder(ASCIIArt fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            BitmapFrame bmp = GetArtBitmapFrame();

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            JpegBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);

            encoder.Save(EncodeStream);
            EncodeStream.Flush();
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Task<BitmapFrame> getBitmapFrameTask = Task.Run(GetArtBitmapFrame);
            BitmapFrame bmp = await getBitmapFrameTask;

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            await Task.Run(() =>
            {
                JpegBitmapEncoder encoder = new();
                encoder.Frames.Add(bmp);

                encoder.Save(EncodeStream);
            });

            await EncodeStream.FlushAsync();
        }
    }

    public class JpegASCIIArtDecoder : ImageASCIIArtDecoder
    {
        public JpegASCIIArtDecoder(ImageASCIIArtDecodeOptions options, Stream stream) : base(options, stream)
        {

        }

        public override ASCIIArt Decode()
        {
            JpegArtLayerDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer layer = layerDecoder.Decode();

            ASCIIArt art = new();

            art.SetSize(layer.Width, layer.Height);
            art.CreatedInVersion = ASCIIArt.VERSION;

            art.ArtLayers.Add(layer);

            return art;
        }

        public override async Task<ASCIIArt> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            JpegArtLayerDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer layer = await layerDecoder.DecodeAsync(taskToken);

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating art...", true));

            ASCIIArt art = new();

            art.SetSize(layer.Width, layer.Height);
            art.CreatedInVersion = ASCIIArt.VERSION;

            art.ArtLayers.Add(layer);

            return art;
        }
    }

    public class GifASCIIArtEncoder : FileObjectEncoder<ASCIIArt>
    {
        public ImageASCIIArtEncodeOptions EncodeOptions { get; set; } = new();

        public GifASCIIArtEncoder(ASCIIArt fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public BitmapFrame GetCanvasArtLayerBitmapFrame(int layerIndex, double minWidth = 0)
        {
            DrawingVisual drawingVisual = new();

            Brush textBrush = new SolidColorBrush(EncodeOptions.TextColor);
            textBrush.Freeze();

            Brush backgroundBrush = new SolidColorBrush(EncodeOptions.BackgroundColor);
            backgroundBrush.Freeze();

            Typeface artTypeface = new(Properties.Settings.Default.CanvasTypefaceSource);
            double defaultWidth = new FormattedText("A", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, EncodeOptions.TextSize, textBrush, 1).Width;

            ArtLayer layer = FileObject.ArtLayers[layerIndex];

            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                FormattedText[] columnTexts = new FormattedText[FileObject.Width];
                double[] columnWidths = new double[FileObject.Width];
                double totalWidth = 0;

                for (int x = 0; x < FileObject.Width; x++)
                {
                    string columnString = "";
                    for (int y = 0; y < FileObject.Height; y++)
                    {
                        if (layer.IsPointVisible(x, y))
                            columnString += (layer.GetCharacter(x - layer.OffsetX, y - layer.OffsetY) ?? ASCIIArt.EMPTYCHARACTER) + "\n";
                        else
                            columnString += ASCIIArt.EMPTYCHARACTER + "\n";
                    }


                    FormattedText columnText = new(columnString, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, EncodeOptions.TextSize, textBrush, 1);
                    columnText.LineHeight = EncodeOptions.TextSize * 1.5;
                    columnTexts[x] = columnText;

                    if (string.IsNullOrWhiteSpace(columnString))
                    {
                        columnWidths[x] = defaultWidth;
                        totalWidth += defaultWidth;
                    }
                    else
                    {
                        columnWidths[x] = columnText.WidthIncludingTrailingWhitespace;
                        totalWidth += columnText.WidthIncludingTrailingWhitespace;
                    }
                }

                dc.DrawRectangle(backgroundBrush, null, new(0, 0, Math.Max(totalWidth, minWidth), EncodeOptions.TextSize * 1.5 * FileObject.Height));

                double posX = 0;
                for (int x = 0; x < FileObject.Width; x++)
                {
                    dc.DrawText(columnTexts[x], new(posX, 0));

                    posX += columnWidths[x];
                }
            }

            int width = (int)drawingVisual.ContentBounds.Width;
            int height = (int)drawingVisual.ContentBounds.Height;

            RenderTargetBitmap bmp = new(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            BitmapFrame frame = BitmapFrame.Create(bmp);
            frame.Freeze();

            return frame;
        }

        public override void Encode()
        {
            double minWidth = 0;
            GifBitmapEncoder encoder = new();
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < FileObject.ArtLayers.Count; i++)
            {
                stopwatch.Restart();
                BitmapFrame bmp = GetCanvasArtLayerBitmapFrame(i, minWidth);

                if (minWidth < bmp.Width)
                    minWidth = bmp.Width;

                encoder.Frames.Add(bmp);

                stopwatch.Stop();

                ConsoleLogger.Log($"Drew gif frame {i + 1} in {stopwatch.ElapsedMilliseconds} ms!");
            }

            using (MemoryStream ms = new())
            {
                encoder.Save(ms);
                
                var fileBytes = ms.ToArray();
                // This is the NETSCAPE2.0 Application Extension.
                var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };
                var newBytes = new List<byte>();
                newBytes.AddRange(fileBytes.Take(13));
                newBytes.AddRange(applicationExtension);
                newBytes.AddRange(fileBytes.Skip(13));
                EncodeStream.Write(newBytes.ToArray());

                EncodeStream.Flush();
            }
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            double minWidth = 0;
            List<BitmapFrame> frames = new();

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < FileObject.ArtLayers.Count; i++)
            {
                stopwatch.Restart();
                taskToken?.ReportProgress((int)((double)i / FileObject.ArtLayers.Count * 100), new($"Drawing frames... ({i}/{FileObject.ArtLayers.Count})", false));

                Task<BitmapFrame> getBitmapFrameTask = Task.Run(() => GetCanvasArtLayerBitmapFrame(i, minWidth));
                BitmapFrame bmp = await getBitmapFrameTask;

                if (minWidth < bmp.Width)
                    minWidth = bmp.Width;

                frames.Add(bmp);

                stopwatch.Stop();

                ConsoleLogger.Log($"Drew gif frame {i + 1} in {stopwatch.ElapsedMilliseconds} ms!");
            }

            taskToken?.ReportProgress(100, new($"Writing to file...", true));

            await Task.Run(async () =>
            {
                GifBitmapEncoder encoder = new();
                encoder.Frames = frames;

                using (MemoryStream ms = new())
                {
                    encoder.Save(ms);

                    var fileBytes = ms.ToArray();
                    // This is the NETSCAPE2.0 Application Extension.
                    var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };
                    var newBytes = new List<byte>();
                    newBytes.AddRange(fileBytes.Take(13));
                    newBytes.AddRange(applicationExtension);
                    newBytes.AddRange(fileBytes.Skip(13));
                    await EncodeStream.WriteAsync(newBytes.ToArray());

                    await EncodeStream.FlushAsync();
                }
            });
        }
    }

    public class GifASCIIArtDecoder : FileObjectDecoder<ASCIIArt>
    {
        public ImageASCIIArtDecodeOptions DecodeOptions { get; set; }

        public GifASCIIArtDecoder(ImageASCIIArtDecodeOptions options, Stream stream) : base(stream)
        {
            DecodeOptions = options;
        }

        public override ASCIIArt Decode()
        {
            GifArtLayerArrayDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer[] layers = layerDecoder.Decode();

            if (layers.Length <= 0)
                throw new Exception("Gif contains no frames!");

            ASCIIArt art = new();

            int artWidth = 0;
            int artHeight = 0;

            foreach (ArtLayer layer in layers) 
            {
                if (layer.Width > artWidth)
                    artWidth = layer.Width;

                if (layer.Height > artHeight)
                    artHeight = layer.Height;
            }

            art.SetSize(artWidth, artHeight);
            art.CreatedInVersion = ASCIIArt.VERSION;

            for (int i = 0; i < layers.Length; i++)
                art.ArtLayers.Add(layers[i]);

            return art;
        }

        public override async Task<ASCIIArt> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            GifArtLayerArrayDecoder layerDecoder = new(DecodeOptions.ImageArtLayerConverter, DecodeStream, DecodeOptions.Scale);

            ArtLayer[] layers = await layerDecoder.DecodeAsync(taskToken);

            if (layers.Length <= 0)
                throw new Exception("Gif contains no frames!");

            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Creating art...", true));

            ASCIIArt art = new();

            int artWidth = 0;
            int artHeight = 0;

            foreach (ArtLayer layer in layers)
            {
                if (layer.Width > artWidth)
                    artWidth = layer.Width;

                if (layer.Height > artHeight)
                    artHeight = layer.Height;
            }

            art.SetSize(artWidth, artHeight);
            art.CreatedInVersion = ASCIIArt.VERSION;

            for (int i = 0; i < layers.Length; i++)
                art.ArtLayers.Add(layers[i]);

            return art;
        }
    }
    #endregion

    public class AAFASCIIArtEncoder : FileObjectEncoder<ASCIIArt>
    {
        public AAFASCIIArtEncoder(ASCIIArt fileObject, Stream stream) : base(fileObject, stream)
        {

        }

        public override void Encode()
        {
            string tempFilePath = Path.GetTempFileName();
            
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            js.Serialize(sw, FileObject);

            sw.Flush();
            sw.Close();

            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(EncodeStream, CompressionLevel.SmallestSize);
                fs.CopyTo(output);

                fs.Flush();
                output.Flush();

                output.Dispose();
            }

            File.Delete(tempFilePath);
        }

        public override async Task EncodeAsync(BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            Task serializeTask = Task.Run(() => js.Serialize(sw, FileObject));

            await serializeTask;

            await sw.FlushAsync();
            sw.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
            {
                GZipStream output = new(EncodeStream, CompressionLevel.SmallestSize);
                await fs.CopyToAsync(output);

                await fs.FlushAsync();
                await output.FlushAsync();

                await output.DisposeAsync();
            }

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting uncompressed path", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;
        }
    }

    public class AAFASCIIArtDecoder : FileObjectDecoder<ASCIIArt>
    {
        public AAFASCIIArtDecoder(Stream stream) : base(stream)
        {

        }

        public override ASCIIArt Decode()
        {
            string tempFilePath = Path.GetTempFileName();

            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(DecodeStream, CompressionMode.Decompress);
            output.CopyTo(fs);

            fs.Close();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            ASCIIArt importedArt = js.Deserialize<ASCIIArt>(jr) ?? throw new Exception("No art could be imported!");
            jr.CloseInput = true;
            jr.Close();

            File.Delete(tempFilePath);

            return importedArt;
        }

        public override async Task<ASCIIArt> DecodeAsync(BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Decompressing art file...", true));
            FileStream fs = File.Create(tempFilePath);

            GZipStream output = new(DecodeStream, CompressionMode.Decompress);
            await output.CopyToAsync(fs);

            fs.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed art file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            Task<ASCIIArt?> deserializeTask = Task.Run(() => js.Deserialize<ASCIIArt>(jr));
            ASCIIArt importedArt = await deserializeTask ?? throw new Exception("No art could be imported!");

            jr.CloseInput = true;
            jr.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;

            return importedArt;
        }
    }

    public abstract class ASCIIArtEncodeOptions
    {
        
    }

    public class ImageASCIIArtEncodeOptions : ASCIIArtEncodeOptions, INotifyPropertyChanged
    {
        private Color backgroundColor = Colors.White;
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if (backgroundColor == value)
                    return;

                backgroundColor = value;

                PropertyChanged?.Invoke(this, new(nameof(BackgroundColor)));
            }
        }

        private Color textColor = Colors.Black;
        public Color TextColor
        {
            get => textColor;
            set
            {
                if (textColor == value)
                    return;

                textColor = value;

                PropertyChanged?.Invoke(this, new(nameof(TextColor)));
            }
        }

        private double textSize = 12;
        public double TextSize
        {
            get => textSize;
            set
            {
                if (textSize == value)
                    return;

                textSize = value;

                PropertyChanged?.Invoke(this, new(nameof(TextSize)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageASCIIArtEncodeOptions()
        {

        }

        public ImageASCIIArtEncodeOptions(Color backgroundColor, Color textColor, double textSize)
        {
            BackgroundColor = backgroundColor;
            TextColor = textColor;
            TextSize = textSize;
        }
    }

    public abstract class ASCIIArtDecodeOptions
    {

    }

    public class ImageASCIIArtDecodeOptions : ASCIIArtDecodeOptions, INotifyPropertyChanged
    {
        private ImageArtLayerConverter imageArtLayerConverter = new();
        public ImageArtLayerConverter ImageArtLayerConverter
        {
            get => imageArtLayerConverter;
            set
            {
                if (imageArtLayerConverter == value)
                    return;

                imageArtLayerConverter = value;

                PropertyChanged?.Invoke(this, new(nameof(ImageArtLayerConverter)));
            }
        }

        private double scale = 1;
        /// <summary>
        /// A value that determines the scale of the imported layer
        /// </summary>
        public double Scale
        {
            get => scale;
            set
            {
                if (scale == value)
                    return;

                if (scale == 0)
                    throw new ArgumentException("Scale can not be equal to 0!");

                scale = value;

                PropertyChanged?.Invoke(this, new(nameof(Scale)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageASCIIArtDecodeOptions()
        {

        }

        public ImageASCIIArtDecodeOptions(ImageArtLayerConverter imageLayerConverter, double scale)
        {
            imageArtLayerConverter = imageLayerConverter;
            this.scale = scale;
        }
    }
}
