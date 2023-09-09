﻿using AAP.BackgroundTasks;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AAP
{
    public class TXTASCIIArt : IAAPFile<ASCIIArt>
    {
        public ASCIIArt FileObject { get; set; }
        public string FilePath { get; set; }

        public TXTASCIIArt(ASCIIArt art, string filePath)
        {
            FileObject = art;
            FilePath = filePath;
        }

        public void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string[] txtLines = File.ReadAllLines(FilePath);

            if (txtLines.Length <= 0)
                throw new Exception($"ASCIIArtFile.ImportFile(path: {FilePath}): txt file contains no lines!");

            int txtWidth = 0;
            int txtHeight = txtLines.Length;

            //Get total width
            foreach (string line in txtLines)
                if (line.Length > txtWidth)
                    txtWidth = line.Length;

            FileObject.SetSize(txtWidth, txtHeight);
            FileObject.CreatedInVersion = ASCIIArt.VERSION;
            ArtLayer txtArtLayer = new("Imported Art", FileObject.Width, FileObject.Height);

            for (int y = 0; y < txtHeight; y++)
            {
                char[] chars = txtLines[y].ToCharArray();
                for (int x = 0; x < txtWidth; x++)
                    txtArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x];
            }

            FileObject.ArtLayers.Add(txtArtLayer);
        }

        public async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Reading text...", true));

            Task<string[]> readLinesTask = File.ReadAllLinesAsync(FilePath);
            string[] txtLines = await readLinesTask;

            if (txtLines.Length <= 0)
                throw new Exception($"ASCIIArtFile.ImportFile(path: {FilePath}): txt file contains no lines!");

            int txtWidth = 0;
            int txtHeight = txtLines.Length;

            //Get total width
            foreach (string line in txtLines)
                if (line.Length > txtWidth)
                    txtWidth = line.Length;

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Creating art...", true));
            FileObject.SetSize(txtWidth, txtHeight);
            FileObject.CreatedInVersion = ASCIIArt.VERSION;
            ArtLayer txtArtLayer = new("Imported Art", FileObject.Width, FileObject.Height);

            BackgroundTaskProgressArgs progressArgs = new("Creating lines...", false);
            Task lineCreation = Task.Run(() => 
            {
                for (int y = 0; y < txtHeight; y++)
                {
                    taskToken?.ReportProgress((int)((double)(y + 1) / txtHeight * 100), progressArgs);
                    char[] chars = txtLines[y].ToCharArray();
                    for (int x = 0; x < txtWidth; x++)
                        txtArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x];
                }
            });

            FileObject.ArtLayers.Add(txtArtLayer);
        }

        public void Export()
        {
            string artString = FileObject.GetArtString();

            StreamWriter sw = File.CreateText(FilePath);
            sw.Write(artString);

            sw.Close();
        }

        public async Task ExportAsync(BackgroundTaskToken? taskToken = null)
        {
            taskToken?.ReportProgress(0, new BackgroundTaskProgressArgs("Getting art string...", true));

            Task<string> artStringTask = Task.Run(() => FileObject.GetArtString());
            string artString = await artStringTask;

            taskToken?.ReportProgress(50, new BackgroundTaskProgressArgs("Writing to file...", true));
            StreamWriter sw = File.CreateText(FilePath);
            await sw.WriteAsync(artString);

            sw.Close();
        }
    }

    public class AAFASCIIArt : IAAPFile<ASCIIArt>
    {
        public ASCIIArt FileObject { get; set; }
        public string FilePath { get; set; }

        public AAFASCIIArt(ASCIIArt art, string filePath)
        {
            FileObject = art;
            FilePath = filePath;
        }

        public void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string tempFilePath = Path.GetTempFileName();

            FileStream fs = File.Create(tempFilePath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                output.CopyTo(fs);

            fs.Close();

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            ASCIIArt? importedArt = js.Deserialize<ASCIIArt>(jr);
            if (importedArt != null)
            {
                FileObject.CreatedInVersion = importedArt.CreatedInVersion;
                FileObject.SetSize(importedArt.Width, importedArt.Height);

                for (int i = 0; i < importedArt.ArtLayers.Count; i++)
                    FileObject.ArtLayers.Insert(i, importedArt.ArtLayers[i]);
            }
            else
                throw new Exception("No art could be imported!");

            jr.CloseInput = true;
            jr.Close();

            File.Delete(tempFilePath);
        }

        public async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Decompressing art file...", true));
            FileStream fs = File.Create(tempFilePath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                await output.CopyToAsync(fs);

            fs.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Deserializing decompressed art file...", true));

            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = File.OpenText(tempFilePath);
            JsonTextReader jr = new(sr);

            Task<ASCIIArt?> deserializeTask = Task.Run(() => js.Deserialize<ASCIIArt>(jr));
            ASCIIArt? importedArt = await deserializeTask;

            if (importedArt != null)
            {
                FileObject.CreatedInVersion = importedArt.CreatedInVersion;
                FileObject.SetSize(importedArt.Width, importedArt.Height);

                for (int i = 0; i < importedArt.ArtLayers.Count; i++)
                    FileObject.ArtLayers.Insert(i, importedArt.ArtLayers[i]);
            }
            else
                throw new Exception("No art could be imported!");

            jr.CloseInput = true;
            jr.Close();

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting decompressed path...", true));

            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;
        }

        public void Export()
        {
            string tempFilePath = Path.GetTempFileName();
            
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            js.Serialize(sw, FileObject);
            sw.Close();

            using (FileStream fs = File.OpenRead(tempFilePath))
                using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                    fs.CopyTo(output);

            File.Delete(tempFilePath);

        }

        public async Task ExportAsync(BackgroundTaskToken? taskToken = null)
        {
            string tempFilePath = Path.GetTempFileName();

            taskToken?.ReportProgress(33, new BackgroundTaskProgressArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);

            Task serializeTask = Task.Run(() => js.Serialize(sw, FileObject));

            await serializeTask;

            sw.Close();

            taskToken?.ReportProgress(66, new BackgroundTaskProgressArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
                using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                    await fs.CopyToAsync(output);

            taskToken?.ReportProgress(100, new BackgroundTaskProgressArgs("Deleting uncompressed path", true));
            
            Task deleteTask = Task.Run(() => File.Delete(tempFilePath));

            await deleteTask;
        }
    }

    public abstract class ASCIIArtExportOptions
    {
        
    }

    public class ImageASCIIArtExportOptions : ASCIIArtExportOptions, INotifyPropertyChanged
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

        public ImageASCIIArtExportOptions()
        {

        }

        public ImageASCIIArtExportOptions(Color backgroundColor, Color textColor, double textSize)
        {
            BackgroundColor = backgroundColor;
            TextColor = textColor;
            TextSize = textSize;
        }
    }

    public abstract class ImageASCIIArt : IAAPFile<ASCIIArt>
    {
        public ASCIIArt FileObject { get; set; }
        public string FilePath { get; set; }

        public ImageASCIIArtExportOptions ExportOptions { get; set; } = new();

        public ImageASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions)
        {
            FileObject = art;
            FilePath = filePath;
            ExportOptions = exportOptions;
        }

        public BitmapFrame GetArtBitmapFrame()
        {
            DrawingVisual drawingVisual = new();

            Brush textBrush = new SolidColorBrush(ExportOptions.TextColor);
            textBrush.Freeze();

            Brush backgroundBrush = new SolidColorBrush(ExportOptions.BackgroundColor);
            backgroundBrush.Freeze();

            Typeface artTypeface = new(Properties.Settings.Default.CanvasTypefaceSource);
            double defaultWidth = new FormattedText("A", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, ExportOptions.TextSize, textBrush, 1).Width;

            FormattedText[] columnTexts = new FormattedText[FileObject.Width];
            double[] columnWidths = new double[FileObject.Width];
            double totalWidth = 0;

            Stopwatch sw = Stopwatch.StartNew();

            for (int x = 0; x < FileObject.Width; x++)
            {
                string columnString = "";
                for (int y = 0; y < FileObject.Height; y++)
                    columnString += (FileObject.GetCharacter(x, y) ?? ASCIIArt.EMPTYCHARACTER) + "\n";

                FormattedText columnText = new(columnString, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, ExportOptions.TextSize, textBrush, 1);
                columnText.LineHeight = ExportOptions.TextSize * 1.5;
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

            dc.DrawRectangle(backgroundBrush, null, new(0, 0, totalWidth, ExportOptions.TextSize * 1.5 * FileObject.Height));

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

        public abstract void Import();

        public abstract void Export();

        public abstract Task ImportAsync(BackgroundTaskToken? taskToken = null);

        public abstract Task ExportAsync(BackgroundTaskToken? taskToken = null);
    }

    public class BitmapASCIIArt : ImageASCIIArt
    {
        public BitmapASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions) : base(art, filePath, exportOptions)
        {

        }

        public override void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override void Export()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            BitmapFrame bmp = GetArtBitmapFrame();

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            BmpBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);
            
            using (FileStream fs = File.Create(FilePath))
                encoder.Save(fs);
        }

        public override async Task ExportAsync(BackgroundTaskToken? taskToken = null)
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
                using (FileStream fs = File.Create(FilePath)) 
                    encoder.Save(fs); 
            });
        }
    }

    public class PngASCIIArt : ImageASCIIArt
    {
        public PngASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions) : base(art, filePath, exportOptions)
        {

        }

        public override void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override void Export()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            BitmapFrame bmp = GetArtBitmapFrame();

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);

            using (FileStream fs = File.Create(FilePath))
                encoder.Save(fs);
        }

        public override async Task ExportAsync(BackgroundTaskToken? taskToken = null)
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

                using (FileStream fs = File.Create(FilePath))
                    encoder.Save(fs);
            });
        }
    }

    public class JpegASCIIArt : ImageASCIIArt
    {
        public JpegASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions) : base(art, filePath, exportOptions)
        {

        }

        public override void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override void Export()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            BitmapFrame bmp = GetArtBitmapFrame();

            stopwatch.Stop();
            ConsoleLogger.Log($"Drew frame in {stopwatch.ElapsedMilliseconds} ms!");

            JpegBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);

            using (FileStream fs = File.Create(FilePath))
                encoder.Save(fs);
        }

        public override async Task ExportAsync(BackgroundTaskToken? taskToken = null)
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

                using (FileStream fs = File.Create(FilePath))
                    encoder.Save(fs);
            });
        }
    }

    public class GifASCIIArt : ImageASCIIArt
    {
        public GifASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions) : base(art, filePath, exportOptions)
        {

        }

        public BitmapFrame GetCanvasArtLayerBitmapFrame(int layerIndex)
        {
            DrawingVisual drawingVisual = new();

            Brush textBrush = new SolidColorBrush(ExportOptions.TextColor);
            Typeface artTypeface = new(Properties.Settings.Default.CanvasTypefaceSource);
            double defaultWidth = new FormattedText("A", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, ExportOptions.TextSize, textBrush, 1).Width;

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


                    FormattedText columnText = new(columnString, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, ExportOptions.TextSize, textBrush, 1);
                    columnText.LineHeight = ExportOptions.TextSize * 1.5;
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

                dc.DrawRectangle(new SolidColorBrush(ExportOptions.BackgroundColor), null, new(0, 0, totalWidth, ExportOptions.TextSize * 1.5 * FileObject.Height));

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

        public override void Import()
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override async Task ImportAsync(BackgroundTaskToken? taskToken = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override void Export()
        {
            GifBitmapEncoder encoder = new();
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < FileObject.ArtLayers.Count; i++)
            {
                stopwatch.Restart();
                BitmapFrame bmp = GetCanvasArtLayerBitmapFrame(i);
                encoder.Frames.Add(bmp);

                stopwatch.Stop();

                ConsoleLogger.Log($"Drew gif frame {i + 1} in {stopwatch.ElapsedMilliseconds} ms!");
            }

            using (FileStream fs = File.Create(FilePath))
                encoder.Save(fs);
        }

        public override async Task ExportAsync(BackgroundTaskToken? taskToken = null)
        {
            List<BitmapFrame> frames = new();

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < FileObject.ArtLayers.Count; i++)
            {
                stopwatch.Restart();
                taskToken?.ReportProgress((int)((double)i / FileObject.ArtLayers.Count * 100), new($"Drawing frames... ({i}/{FileObject.ArtLayers.Count})", false));

                Task<BitmapFrame> getBitmapFrameTask = Task.Run(() => GetCanvasArtLayerBitmapFrame(i));
                frames.Add(await getBitmapFrameTask);

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
                    await File.WriteAllBytesAsync(FilePath, newBytes.ToArray());
                }
            });
        }
    }
}
