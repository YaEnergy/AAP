using AAP.BackgroundTasks;
using AAP.Timelines;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AAP
{
    public class ASCIIArtFile: INotifyPropertyChanged, IDisposable
    {
        public ASCIIArt Art { get; }
        public ASCIIArtDraw ArtDraw { get; }
        public ObjectTimeline ArtTimeline { get; }

        private string? savePath;
        public string? SavePath
        {
            get => savePath;
            set
            {
                if (savePath == value)
                    return;

                savePath = value;

                OnSavePathChanged?.Invoke(savePath);
                PropertyChanged?.Invoke(this, new(nameof(SavePath)));
                PropertyChanged?.Invoke(this, new(nameof(FileName)));
            }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SavePath))
                    return "*.*";

                return Path.GetFileName(SavePath);
            }
        }

        public delegate void SavePathChangedEvent(string? path);
        /// <summary>
        /// Invoked when SavePath changes.
        /// </summary>
        public event SavePathChangedEvent? OnSavePathChanged;


        private bool unsavedChanges = false;
        public bool UnsavedChanges
        {
            get => unsavedChanges;
            set
            {
                if (unsavedChanges == value)
                    return;

                unsavedChanges = value;

                OnUnsavedChangesChanged?.Invoke(unsavedChanges);
                PropertyChanged?.Invoke(this, new(nameof(UnsavedChanges)));
            }
        }

        public delegate void UnsavedChangesChangedEvent(bool unsavedChanges);
        /// <summary>
        /// Invoked when the UnsavedChanges bool changes.
        /// </summary>
        public event UnsavedChangesChangedEvent? OnUnsavedChangesChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ASCIIArtFile(ASCIIArt art)
        {
            Art = art;
            ArtDraw = new(Art);
            ArtTimeline = new(Art);

            Art.OnArtLayerAdded += Art_OnArtLayerAdded;
            Art.OnArtLayerIndexChanged += Art_OnArtLayerIndexChanged;
            Art.OnArtLayerRemoved += Art_OnArtLayerRemoved;

            Art.OnSizeChanged += Art_OnSizeChanged;
            Art.OnCropped += Art_OnCropped;

            ArtDraw.OnDrawArt += ArtDraw_OnDrawArt;

            ArtTimeline.Rolledback += ArtTimeline_TimeTravelled;
            ArtTimeline.Rolledforward += ArtTimeline_TimeTravelled;

            foreach (ArtLayer layer in Art.ArtLayers)
                layer.PropertyChanged += Layer_PropertyChanged;
        }

        #region Main File Methods
        public static ASCIIArtFile Open(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath) + ": " + filePath);

            ConsoleLogger.Log($"Open File: Importing file...");

            FileInfo file = new(filePath);

            IAAPFile<ASCIIArt> AAPFile;
            ASCIIArt art = new();

            bool saved = false;

            switch (file.Extension.ToLower())
            {
                case ".txt":
                    AAPFile = new TXTASCIIArt(art, file.FullName);
                    break;
                case ".aaf":
                    AAPFile = new AAFASCIIArt(art, file.FullName);
                    saved = true;
                    break;
                default:
                    throw new Exception("Unknown file extension!");
            }

            AAPFile.Import();
            ConsoleLogger.Log($"Open File: Imported file!");

            if (art.Width * art.Height > App.MaxArtArea)
                throw new Exception($"Art Area is too large! Max: {App.MaxArtArea} characters ({art.Width * art.Height} characters)");

            ConsoleLogger.Inform($"\nOpen File: \nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nVisible Art Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.GetTotalArtArea()}\nCreated In Version: {art.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

            ASCIIArtFile artFile = new(art);
            artFile.UnsavedChanges = !saved;
            artFile.SavePath = saved ? file.FullName : null;

            return artFile;
        }

        public static async Task<ASCIIArtFile> OpenAsync(string filePath, BackgroundTaskToken? taskToken = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath) + ": " + filePath);

            ConsoleLogger.Log($"Open File: Importing file...");

            FileInfo file = new(filePath);

            IAAPFile<ASCIIArt> AAPFile;
            ASCIIArt art = new();

            bool saved = false;

            switch (file.Extension.ToLower())
            {
                case ".txt":
                    AAPFile = new TXTASCIIArt(art, file.FullName);
                    break;
                case ".aaf":
                    AAPFile = new AAFASCIIArt(art, file.FullName);
                    saved = true;
                    break;
                default:
                    throw new Exception("Unknown file extension!");
            }

            await AAPFile.ImportAsync(taskToken);
            ConsoleLogger.Log($"Open File: Imported file!");

            if (art.Width * art.Height > App.MaxArtArea)
                throw new Exception($"Art Area is too large! Max: {App.MaxArtArea} characters ({art.Width * art.Height} characters)");

            ConsoleLogger.Inform($"\nOpen File: \nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nVisible Art Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.GetTotalArtArea()}\nCreated In Version: {art.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

            ASCIIArtFile artFile = new(art);
            artFile.UnsavedChanges = !saved;
            artFile.SavePath = saved ? file.FullName : null;

            return artFile;
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(SavePath))
                throw new NullReferenceException("SavePath is null or white space!");

            ConsoleLogger.Log("Save File: Saving art file to " + SavePath);

            AAFASCIIArt aafASCIIArt = new(Art, SavePath);

            aafASCIIArt.Export();

            UnsavedChanges = false;

            ConsoleLogger.Log("Save File: Saved art file to " + SavePath);
        }

        public async Task SaveAsync(BackgroundTaskToken? taskToken = null)
        {
            if (string.IsNullOrWhiteSpace(SavePath))
                throw new NullReferenceException("SavePath is null or white space!");

            ConsoleLogger.Log("Save File: Saving art file to " + SavePath);

            AAFASCIIArt aafASCIIArt = new(Art, SavePath);

            await aafASCIIArt.ExportAsync(taskToken);

            UnsavedChanges = false;

            ConsoleLogger.Log("Save File: Saved art file to " + SavePath);
        }

        public void Export(string filePath, ASCIIArtExportOptions? exportOptions = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new NullReferenceException("filePath is null or white space!");

            ConsoleLogger.Log("Export File: Exporting art file to " + filePath);

            FileInfo fileInfo = new(filePath);
            IAAPFile<ASCIIArt> AAPFile;

            switch (fileInfo.Extension.ToLower())
            {
                case ".txt":
                    AAPFile = new TXTASCIIArt(Art, fileInfo.FullName);
                    break;
                case ".aaf":
                    AAPFile = new AAFASCIIArt(Art, fileInfo.FullName);
                    break;
                case ".bmp":
                    if (exportOptions is not ImageASCIIArtExportOptions bmpExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new BitmapASCIIArt(Art, fileInfo.FullName, bmpExportOptions);
                    break;
                case ".png":
                    if (exportOptions is not ImageASCIIArtExportOptions pngExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new PngASCIIArt(Art, fileInfo.FullName, pngExportOptions);
                    break;
                case ".jpg":
                    if (exportOptions is not ImageASCIIArtExportOptions jpegExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new JpegASCIIArt(Art, fileInfo.FullName, jpegExportOptions);
                    break;
                case ".gif":
                    if (exportOptions is not ImageASCIIArtExportOptions gifExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new GifASCIIArt(Art, fileInfo.FullName, gifExportOptions);
                    break;
                default:
                    throw new Exception("Unknown file extension!");
            }

            AAPFile.Export();

            ConsoleLogger.Log("Export File: Art file exported to " + fileInfo.FullName + "!");
        }

        public async Task ExportAsync(string filePath, ASCIIArtExportOptions? exportOptions = null, BackgroundTaskToken? taskToken = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new NullReferenceException("filePath is null or white space!");

            ConsoleLogger.Log("Export File: Exporting art file to " + filePath);

            FileInfo fileInfo = new(filePath);
            IAAPFile<ASCIIArt> AAPFile;

            switch (fileInfo.Extension.ToLower())
            {
                case ".txt":
                    AAPFile = new TXTASCIIArt(Art, fileInfo.FullName);
                    break;
                case ".aaf":
                    AAPFile = new AAFASCIIArt(Art, fileInfo.FullName);
                    break;
                case ".bmp":
                    if (exportOptions is not ImageASCIIArtExportOptions bmpExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new BitmapASCIIArt(Art, fileInfo.FullName, bmpExportOptions);
                    break;
                case ".png":
                    if (exportOptions is not ImageASCIIArtExportOptions pngExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new PngASCIIArt(Art, fileInfo.FullName, pngExportOptions);
                    break;
                case ".jpg":
                    if (exportOptions is not ImageASCIIArtExportOptions jpegExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new JpegASCIIArt(Art, fileInfo.FullName, jpegExportOptions);
                    break;
                case ".gif":
                    if (exportOptions is not ImageASCIIArtExportOptions gifExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtExportOptions!");

                    AAPFile = new GifASCIIArt(Art, fileInfo.FullName, gifExportOptions);
                    break;
                default:
                    throw new Exception("Unknown file extension!");
            }

            await AAPFile.ExportAsync(taskToken);

            ConsoleLogger.Log("Export File: Art file exported to " + fileInfo.FullName + "!");
        }
        #endregion

        public void Dispose()
        {
            Art.OnArtLayerAdded -= Art_OnArtLayerAdded;
            Art.OnArtLayerIndexChanged -= Art_OnArtLayerIndexChanged;
            Art.OnArtLayerRemoved -= Art_OnArtLayerRemoved;

            Art.OnSizeChanged -= Art_OnSizeChanged;
            Art.OnCropped -= Art_OnCropped;

            ArtDraw.OnDrawArt -= ArtDraw_OnDrawArt;

            ArtTimeline.Rolledback -= ArtTimeline_TimeTravelled;
            ArtTimeline.Rolledforward -= ArtTimeline_TimeTravelled;

            foreach (ArtLayer layer in Art.ArtLayers)
                layer.PropertyChanged -= Layer_PropertyChanged;

            GC.SuppressFinalize(this);
        }

        private void ArtTimeline_TimeTravelled(ObjectTimeline sender)
        {
            UnsavedChanges = true;
        }

        private void Layer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UnsavedChanges = true;
        }

        private void Art_OnArtLayerAdded(int index, ArtLayer artLayer)
        {
            artLayer.PropertyChanged += Layer_PropertyChanged;

            UnsavedChanges = true;
        }

        private void Art_OnArtLayerIndexChanged(int index, ArtLayer artLayer)
        {
            UnsavedChanges = true;
        }

        private void Art_OnArtLayerRemoved(int index, ArtLayer artLayer)
        {
            artLayer.PropertyChanged -= Layer_PropertyChanged;

            UnsavedChanges = true;
        }

        private void Art_OnCropped(ASCIIArt art)
        {
            UnsavedChanges = true;
        }

        private void Art_OnSizeChanged(int width, int height)
        {
            UnsavedChanges = true;
        }

        private void ArtDraw_OnDrawArt(int layerIndex, char? character, Point[] positions)
        {
            UnsavedChanges = true;
        }

        public override string ToString()
        {
            string changedText = UnsavedChanges ? "*" : "";

            string fileName = string.IsNullOrEmpty(SavePath) ? "*.*" : new FileInfo(SavePath).Name;

            return $"{fileName}{changedText} - {Art.Width}x{Art.Height}";
        }
    }
}
