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

        public void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Reading text...", true));
            string[] txtLines = File.ReadAllLines(FilePath);

            if (txtLines.Length <= 0)
                throw new Exception($"ASCIIArtFile.ImportFile(path: {FilePath}): txt file contains no lines!");

            int txtWidth = 0;
            int txtHeight = txtLines.Length;

            //Get total width
            foreach (string line in txtLines)
                if (line.Length > txtWidth)
                    txtWidth = line.Length;

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Creating art...", true));
            FileObject.SetSize(txtWidth, txtHeight);
            FileObject.CreatedInVersion = ASCIIArt.VERSION;
            ArtLayer txtArtLayer = new("Imported Art", FileObject.Width, FileObject.Height);

            for (int y = 0; y < txtHeight; y++)
            {
                bgWorker?.ReportProgress((int)((double)(y + 1) / txtHeight * 100), new BackgroundTaskUpdateArgs("Creating lines...", false));
                char[] chars = txtLines[y].ToCharArray();
                for (int x = 0; x < txtWidth; x++)
                    txtArtLayer.Data[x][y] = x >= chars.Length ? null : chars[x] == ASCIIArt.EMPTYCHARACTER ? null : chars[x];
            }

            FileObject.ArtLayers.Add(txtArtLayer);
        }

        public bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            bgWorker?.ReportProgress(0, new BackgroundTaskUpdateArgs("Getting art string...", true));
            string artString = FileObject.GetArtString();

            if (bgWorker != null)
            {
                if (bgWorker.CancellationPending)
                    return false;

                bgWorker.WorkerSupportsCancellation = false;
            }

            bgWorker?.ReportProgress(50, new BackgroundTaskUpdateArgs("Writing to file...", true));
            StreamWriter sw = File.CreateText(FilePath);
            sw.Write(artString);

            sw.Close();

            return true;
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

        public void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            string tempFilePath = Path.GetTempFileName();

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Decompressing art file...", true));
            FileStream fs = File.Create(tempFilePath);

            using (GZipStream output = new(File.Open(FilePath, FileMode.Open), CompressionMode.Decompress))
                output.CopyTo(fs);

            fs.Close();

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Deserializing decompressed art file...", true));

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

            bgWorker?.ReportProgress(100, new BackgroundTaskUpdateArgs("Deleting decompressed path...", true));
            File.Delete(tempFilePath);
        }

        public bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            string tempFilePath = Path.GetTempFileName();

            bgWorker?.ReportProgress(33, new BackgroundTaskUpdateArgs("Writing as uncompressed file...", true));
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamWriter sw = File.CreateText(tempFilePath);
            js.Serialize(sw, FileObject);
            sw.Close();

            if (bgWorker != null)
            {
                if (bgWorker.CancellationPending)
                {
                    File.Delete(tempFilePath);
                    return false;
                }

                bgWorker.WorkerSupportsCancellation = false;
            }

            bgWorker?.ReportProgress(66, new BackgroundTaskUpdateArgs("Decompressing uncompressed file to file path...", true));
            using (FileStream fs = File.OpenRead(tempFilePath))
            using (GZipStream output = new(File.Create(FilePath), CompressionLevel.SmallestSize))
                fs.CopyTo(output);

            bgWorker?.ReportProgress(100, new BackgroundTaskUpdateArgs("Deleting uncompressed path", true));
            File.Delete(tempFilePath);

            return true;
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
            Typeface artTypeface = new(Properties.Settings.Default.CanvasTypefaceSource);
            double defaultWidth = new FormattedText("A", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, artTypeface, ExportOptions.TextSize, textBrush, 1).Width;

            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                FormattedText[] columnTexts = new FormattedText[FileObject.Width];
                double[] columnWidths = new double[FileObject.Width];
                double totalWidth = 0;

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

            return BitmapFrame.Create(bmp);
        }

        public abstract void Import(BackgroundWorker? bgWorker = null);

        public abstract bool Export(BackgroundWorker? bgWorker = null);
    }

    public class BitmapASCIIArt : ImageASCIIArt
    {
        public BitmapASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions) : base(art, filePath, exportOptions)
        {

        }

        public override void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;
            
            BitmapFrame bmp = GetArtBitmapFrame();

            BmpBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);
            
            using (FileStream fs = File.Create(FilePath))
                encoder.Save(fs);

            return true;
        }
    }

    public class PngASCIIArt : ImageASCIIArt
    {
        public PngASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions) : base(art, filePath, exportOptions)
        {

        }

        public override void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            BitmapFrame bmp = GetArtBitmapFrame();

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);

            using (FileStream fs = File.Create(FilePath))
                encoder.Save(fs);

            return true;
        }
    }

    public class JpegASCIIArt : ImageASCIIArt
    {
        public JpegASCIIArt(ASCIIArt art, string filePath, ImageASCIIArtExportOptions exportOptions) : base(art, filePath, exportOptions)
        {

        }

        public override void Import(BackgroundWorker? bgWorker = null)
        {
            FileInfo fileInfo = new(FilePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            throw new NotImplementedException();
        }

        public override bool Export(BackgroundWorker? bgWorker = null)
        {
            if (bgWorker != null)
                if (bgWorker.CancellationPending)
                    return false;

            BitmapFrame bmp = GetArtBitmapFrame();

            JpegBitmapEncoder encoder = new();
            encoder.Frames.Add(bmp);

            using (FileStream fs = File.Create(FilePath))
                encoder.Save(fs);

            return true;
        }
    }
}
