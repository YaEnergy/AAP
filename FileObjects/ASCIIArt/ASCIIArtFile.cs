using AAP.BackgroundTasks;
using AAP.Files;
using AAP.Timelines;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AAP
{
    public class ASCIIArtFile: INotifyPropertyChanged, IDisposable
    {
        public ASCIIArt Art { get; }
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
            ArtTimeline = new(Art);

            Art.OnArtLayerAdded += Art_OnArtLayerAdded;
            Art.OnArtLayerIndexChanged += Art_OnArtLayerIndexChanged;
            Art.OnArtLayerRemoved += Art_OnArtLayerRemoved;

            Art.OnSizeChanged += Art_OnSizeChanged;
            Art.OnCropped += Art_OnCropped;

            ArtTimeline.Rolledback += ArtTimeline_TimeTravelled;
            ArtTimeline.Rolledforward += ArtTimeline_TimeTravelled;

            foreach (ArtLayer layer in Art.ArtLayers)
            {
                layer.PropertyChanged += Layer_PropertyChanged;
                layer.CharacterChanged += Layer_CharacterChanged;
            }
        }

        #region Main File Methods
        public static ASCIIArtFile Open(string filePath, ASCIIArtDecodeOptions? importOptions = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath) + ": " + filePath);

            ConsoleLogger.Log($"Open File: Importing file...");

            FileInfo file = new(filePath);

            FileObjectDecoder<ASCIIArt> AAPFile;

            FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);

            bool saved = false;

            switch (file.Extension.ToLower())
            {
                case ".txt":
                    AAPFile = new TextASCIIArtDecoder(stream);
                    break;
                case ".aaf":
                    AAPFile = new AAFASCIIArtDecoder(stream);
                    saved = true;
                    break;
                case ".bmp":
                    if (importOptions is not ImageASCIIArtDecodeOptions bmpImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new BitmapASCIIArtDecoder(bmpImportOptions, stream);
                    break;
                case ".png":
                    if (importOptions is not ImageASCIIArtDecodeOptions pngImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new PngASCIIArtDecoder(pngImportOptions, stream);
                    break;
                case ".jpeg":
                case ".jpg":
                case ".jfif":
                    if (importOptions is not ImageASCIIArtDecodeOptions jpegImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new JpegASCIIArtDecoder(jpegImportOptions, stream);
                    break;
                case ".gif":
                    if (importOptions is not ImageASCIIArtDecodeOptions gifImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new GifASCIIArtDecoder(gifImportOptions, stream);
                    break;
                default:
                    stream.Close();
                    throw new Exception("Unknown file extension!");
            }

            ASCIIArt art = AAPFile.Decode();

            AAPFile.Close();
            ConsoleLogger.Log($"Open File: Imported file!");

            if (art.Width * art.Height > App.MaxArtArea)
                throw new Exception($"Art Area is too large! Max: {App.MaxArtArea} characters ({art.Width * art.Height} characters)");

            ConsoleLogger.Inform($"\nOpen File: \nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nVisible Art Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.GetTotalArtArea()}\nVersion: {art.Version}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

            ASCIIArtFile artFile = new(art);
            artFile.UnsavedChanges = !saved;
            artFile.SavePath = saved ? file.FullName : null;

            return artFile;
        }

        public static async Task<ASCIIArtFile> OpenAsync(string filePath, ASCIIArtDecodeOptions? importOptions = null, BackgroundTaskToken? taskToken = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath) + ": " + filePath);

            ConsoleLogger.Log($"Open File: Importing file...");

            FileInfo file = new(filePath);

            FileObjectDecoder<ASCIIArt> AAPFile;

            FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);

            bool saved = false;

            switch (file.Extension.ToLower())
            {
                case ".txt":
                    AAPFile = new TextASCIIArtDecoder(stream);
                    break;
                case ".aaf":
                    AAPFile = new AAFASCIIArtDecoder(stream);
                    saved = true;
                    break;
                case ".bmp":
                    if (importOptions is not ImageASCIIArtDecodeOptions bmpImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new BitmapASCIIArtDecoder(bmpImportOptions, stream);
                    break;
                case ".png":
                    if (importOptions is not ImageASCIIArtDecodeOptions pngImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new PngASCIIArtDecoder(pngImportOptions, stream);
                    break;
                case ".jpeg":
                case ".jpg":
                case ".jfif":
                    if (importOptions is not ImageASCIIArtDecodeOptions jpegImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new JpegASCIIArtDecoder(jpegImportOptions, stream);
                    break;
                case ".gif":
                    if (importOptions is not ImageASCIIArtDecodeOptions gifImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    AAPFile = new GifASCIIArtDecoder(gifImportOptions, stream);
                    break;
                default:
                    stream.Close();
                    throw new Exception("Unknown file extension!");
            }

            ASCIIArt art = await AAPFile.DecodeAsync(taskToken);

            AAPFile.Close();
            ConsoleLogger.Log($"Open File: Imported file!");

            if (art.Width * art.Height > App.MaxArtArea)
                throw new Exception($"Art Area is too large! Max: {App.MaxArtArea} characters ({art.Width * art.Height} characters)");

            ConsoleLogger.Inform($"\nOpen File: \nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nVisible Art Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.GetTotalArtArea()}\nVersion: {art.Version}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

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

            Art.Version = ASCIIArt.ARTVERSION;
            FileStream stream = File.Create(SavePath);
            AAFASCIIArtEncoder aafASCIIArt = new(Art, stream);

            aafASCIIArt.Encode();
            aafASCIIArt.Close();

            UnsavedChanges = false;

            ConsoleLogger.Log("Save File: Saved art file to " + SavePath);
        }

        public async Task SaveAsync(BackgroundTaskToken? taskToken = null)
        {
            if (string.IsNullOrWhiteSpace(SavePath))
                throw new NullReferenceException("SavePath is null or white space!");

            ConsoleLogger.Log("Save File: Saving art file to " + SavePath);

            Art.Version = ASCIIArt.ARTVERSION;
            FileStream stream = File.Create(SavePath);
            AAFASCIIArtEncoder aafASCIIArt = new(Art, stream);

            await aafASCIIArt.EncodeAsync(taskToken);
            aafASCIIArt.Close();

            UnsavedChanges = false;

            ConsoleLogger.Log("Save File: Saved art file to " + SavePath);
        }

        public void Export(string filePath, ASCIIArtEncodeOptions? exportOptions = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new NullReferenceException("filePath is null or white space!");

            ConsoleLogger.Log("Export File: Exporting art file to " + filePath);

            FileInfo fileInfo = new(filePath);
            FileObjectEncoder<ASCIIArt> AAPFile;
            FileStream stream = File.Create(filePath);

            if (exportOptions is ImageASCIIArtEncodeOptions imageEncodeOptions)
                AAPFile = ASCIIArtEncoderFactory.New(fileInfo.Extension, Art, stream, imageEncodeOptions);
            else
                AAPFile = ASCIIArtEncoderFactory.New(fileInfo.Extension, Art, stream);

            AAPFile.Encode();
            AAPFile.Close();

            ConsoleLogger.Log("Export File: Art file exported to " + fileInfo.FullName + "!");
        }

        public async Task ExportAsync(string filePath, ASCIIArtEncodeOptions? exportOptions = null, BackgroundTaskToken? taskToken = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new NullReferenceException("filePath is null or white space!");

            ConsoleLogger.Log("Export File: Exporting art file to " + filePath);

            FileInfo fileInfo = new(filePath);
            FileObjectEncoder<ASCIIArt> AAPFile;
            FileStream stream = File.Create(filePath);

            switch (fileInfo.Extension.ToLower())
            {
                case ".txt":
                    AAPFile = new TextASCIIArtEncoder(Art, stream);
                    break;
                case ".aaf":
                    AAPFile = new AAFASCIIArtEncoder(Art, stream);
                    break;
                case ".bmp":
                    if (exportOptions is not ImageASCIIArtEncodeOptions bmpExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtEncodeOptions!");

                    AAPFile = new BitmapASCIIArtEncoder(Art, stream) { EncodeOptions = bmpExportOptions };
                    break;
                case ".png":
                    if (exportOptions is not ImageASCIIArtEncodeOptions pngExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtEncodeOptions!");

                    AAPFile = new PngASCIIArtEncoder(Art, stream) { EncodeOptions = pngExportOptions };
                    break;
                case ".jpg":
                    if (exportOptions is not ImageASCIIArtEncodeOptions jpegExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtEncodeOptions!");

                    AAPFile = new JpegASCIIArtEncoder(Art, stream) { EncodeOptions = jpegExportOptions };
                    break;
                case ".gif":
                    if (exportOptions is not ImageASCIIArtEncodeOptions gifExportOptions)
                        throw new Exception("Export Options is not ImageASCIIArtEncodeOptions!");

                    AAPFile = new GifASCIIArtEncoder(Art, stream) { EncodeOptions = gifExportOptions };
                    break;
                default:
                    stream.Close();
                    throw new Exception("Unknown file extension!");
            }

            await AAPFile.EncodeAsync(taskToken);
            AAPFile.Close();

            ConsoleLogger.Log("Export File: Art file exported to " + fileInfo.FullName + "!");
        }
        
        public void ImportFile(string filePath, ASCIIArtDecodeOptions? importOptions = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath) + ": " + filePath);

            ConsoleLogger.Log($"Import File: Importing file...");

            FileInfo file = new(filePath);

            FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);

            switch (file.Extension.ToLower())
            {
                case ".txt":
                    FileObjectDecoder<ArtLayer> textDecoder = new TextArtLayerDecoder(stream);
                    ArtLayer textLayer = textDecoder.Decode();
                    Art.ArtLayers.Add(textLayer);
                    break;
                case ".bmp":
                    if (importOptions is not ImageASCIIArtDecodeOptions bmpImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer> bmpDecoder = new BitmapArtLayerDecoder(bmpImportOptions.ImageArtLayerConverter, stream, bmpImportOptions.Scale);
                    ArtLayer bmpLayer = bmpDecoder.Decode();
                    Art.ArtLayers.Add(bmpLayer);
                    break;
                case ".png":
                    if (importOptions is not ImageASCIIArtDecodeOptions pngImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer> pngDecoder = new PngArtLayerDecoder(pngImportOptions.ImageArtLayerConverter, stream, pngImportOptions.Scale);
                    ArtLayer pngLayer = pngDecoder.Decode();
                    Art.ArtLayers.Add(pngLayer);
                    break;
                case ".jpeg":
                case ".jpg":
                case ".jfif":
                    if (importOptions is not ImageASCIIArtDecodeOptions jpegImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer> jpegDecoder = new JpegArtLayerDecoder(jpegImportOptions.ImageArtLayerConverter, stream, jpegImportOptions.Scale);
                    ArtLayer jpegLayer = jpegDecoder.Decode();
                    Art.ArtLayers.Add(jpegLayer);
                    break;
                case ".gif":
                    if (importOptions is not ImageASCIIArtDecodeOptions gifImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer[]> gifDecoder = new GifArtLayerArrayDecoder(gifImportOptions.ImageArtLayerConverter, stream, gifImportOptions.Scale);
                    ArtLayer[] gifFrameLayers = gifDecoder.Decode();

                    for (int i = 0; i < gifFrameLayers.Length; i++)
                        Art.ArtLayers.Add(gifFrameLayers[i]);

                    break;
                default:
                    stream.Close();
                    throw new Exception("Unknown file extension!");
            }

            Art.Update();
        }

        public async Task ImportFileAsync(string filePath, ASCIIArtDecodeOptions? importOptions = null, BackgroundTaskToken? taskToken = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath) + ": " + filePath);

            ConsoleLogger.Log($"Import File: Importing file...");

            FileInfo file = new(filePath);

            FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);

            switch (file.Extension.ToLower())
            {
                case ".txt":
                    FileObjectDecoder<ArtLayer> textDecoder = new TextArtLayerDecoder(stream);
                    ArtLayer textLayer = await textDecoder.DecodeAsync(taskToken);
                    Art.ArtLayers.Add(textLayer);
                    break;
                case ".bmp":
                    if (importOptions is not ImageASCIIArtDecodeOptions bmpImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer> bmpDecoder = new BitmapArtLayerDecoder(bmpImportOptions.ImageArtLayerConverter, stream, bmpImportOptions.Scale);
                    ArtLayer bmpLayer = await bmpDecoder.DecodeAsync(taskToken);
                    Art.ArtLayers.Add(bmpLayer);
                    break;
                case ".png":
                    if (importOptions is not ImageASCIIArtDecodeOptions pngImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer> pngDecoder = new PngArtLayerDecoder(pngImportOptions.ImageArtLayerConverter, stream, pngImportOptions.Scale);
                    ArtLayer pngLayer = await pngDecoder.DecodeAsync(taskToken);
                    Art.ArtLayers.Add(pngLayer);
                    break;
                case ".jpeg":
                case ".jpg":
                case ".jfif":
                    if (importOptions is not ImageASCIIArtDecodeOptions jpegImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer> jpegDecoder = new JpegArtLayerDecoder(jpegImportOptions.ImageArtLayerConverter, stream, jpegImportOptions.Scale);
                    ArtLayer jpegLayer = await jpegDecoder.DecodeAsync(taskToken);
                    Art.ArtLayers.Add(jpegLayer);
                    break;
                case ".gif":
                    if (importOptions is not ImageASCIIArtDecodeOptions gifImportOptions)
                        throw new Exception("Import Options is not ImageASCIIArtDecodeOptions!");

                    FileObjectDecoder<ArtLayer[]> gifDecoder = new GifArtLayerArrayDecoder(gifImportOptions.ImageArtLayerConverter, stream, gifImportOptions.Scale);
                    ArtLayer[] gifFrameLayers = await gifDecoder.DecodeAsync(taskToken);

                    for (int i = 0; i < gifFrameLayers.Length; i++)
                        Art.ArtLayers.Add(gifFrameLayers[i]);

                    break;
                default:
                    stream.Close();
                    throw new Exception("Unknown file extension!");
            }

            Art.Update();
        }
        #endregion

        public void Dispose()
        {
            Art.OnArtLayerAdded -= Art_OnArtLayerAdded;
            Art.OnArtLayerIndexChanged -= Art_OnArtLayerIndexChanged;
            Art.OnArtLayerRemoved -= Art_OnArtLayerRemoved;

            Art.OnSizeChanged -= Art_OnSizeChanged;
            Art.OnCropped -= Art_OnCropped;

            ArtTimeline.Rolledback -= ArtTimeline_TimeTravelled;
            ArtTimeline.Rolledforward -= ArtTimeline_TimeTravelled;

            foreach (ArtLayer layer in Art.ArtLayers)
            {
                layer.PropertyChanged -= Layer_PropertyChanged;
                layer.CharacterChanged -= Layer_CharacterChanged;
            }

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

        private void Layer_CharacterChanged(ArtLayer layer, int x, int y)
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

        public override string ToString()
        {
            string changedText = UnsavedChanges ? "*" : "";

            string fileName = string.IsNullOrEmpty(SavePath) ? "*.*" : new FileInfo(SavePath).Name;

            return $"{fileName}{changedText} - {Art.Width}x{Art.Height}";
        }
    }
}
