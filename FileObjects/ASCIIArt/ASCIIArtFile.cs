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

        public static BackgroundTask? OpenAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath) + ": " + filePath);

            FileInfo file = new(filePath);

            BackgroundWorker bgWorker = new();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += OpenWork;
            bgWorker.RunWorkerCompleted += OpenWorkComplete;

            ConsoleLogger.Log("Open File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void OpenWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                ConsoleLogger.Log($"Open File: importing file from path... {file.FullName}");

                bgWorker.ReportProgress(30, new BackgroundTaskUpdateArgs("Importing art...", true));

                if (!file.Exists)
                    throw new FileNotFoundException("Tried to open non-existant file", file.FullName);

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

                ConsoleLogger.Log($"Open File: Imported file!");
                AAPFile.Import(bgWorker);

                if (bgWorker.CancellationPending)
                {
                    args.Cancel = true;
                    return;
                }

                if (art.Width * art.Height > App.MaxArtArea)
                    throw new Exception($"Art Area is too large! Max: {App.MaxArtArea} characters ({art.Width * art.Height} characters)");

                ConsoleLogger.Inform($"\nOpen File: \nFILE INFO\nFile Path: {file.FullName}\nSize: {art.Width}x{art.Height}\nVisible Art Area: {art.Width * art.Height}\nTotal Art Layers: {art.ArtLayers.Count}\nTotal Area: {art.GetTotalArtArea()}\nCreated In Version: {art.CreatedInVersion}\nFile Size: {file.Length / 1024} kb\nExtension: {file.Extension}\nLast Write Time: {file.LastWriteTime.ToLocalTime().ToLongTimeString()} {file.LastWriteTime.ToLocalTime().ToLongDateString()}");

                bgWorker.ReportProgress(90, new BackgroundTaskUpdateArgs("Finishing up art...", true));

                ASCIIArtFile artFile = new(art);
                artFile.UnsavedChanges = !saved;
                artFile.SavePath = file.Extension.ToLower() == ".aaf" ? file.FullName : null;

                args.Result = artFile;
            }

            void OpenWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    ConsoleLogger.Error("Open File: ", e.Error);
                else if (e.Cancelled)
                    ConsoleLogger.Inform("Open File: Art file open was cancelled");
                else
                {
                    if (e.Result is not ASCIIArtFile)
                        throw new Exception("Open File: BackgroundWorker Result is not of type ASCIIArtFile!");
                    
                    ConsoleLogger.Log($"Open File Path: opened file!");
                }
            }

            return new($"Opening {file.Name}...", bgWorker);
        }

        public BackgroundTask? SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(SavePath))
                throw new NullReferenceException("SavePath is null or white space!");

            BackgroundWorker bgWorker = new();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += SaveWork;
            bgWorker.RunWorkerCompleted += SaveWorkComplete;

            ConsoleLogger.Log("Save File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void SaveWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    return;

                ConsoleLogger.Log("Save File: Saving art file to " + SavePath);
                bgWorker.ReportProgress(90, new BackgroundTaskUpdateArgs("Exporting as .aaf file...", true));

                AAFASCIIArt aafASCIIArt = new(Art, SavePath);

                bool exportSuccess = aafASCIIArt.Export(bgWorker);

                if (bgWorker.CancellationPending && !exportSuccess)
                {
                    args.Cancel = true;
                    return;
                }

                args.Result = new FileInfo(SavePath);
            }

            void SaveWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    ConsoleLogger.Error("Save File: ", e.Error);
                    UnsavedChanges = true;
                }
                else if (e.Cancelled)
                {
                    ConsoleLogger.Inform("Save File: Art file save cancelled");
                    UnsavedChanges = true;
                }
                else
                {
                    if (e.Result is not FileInfo fileInfo)
                        ConsoleLogger.Warn("Save File: Art file save was successful, but result is not file info");
                    else
                        ConsoleLogger.Log("Save File: Art file saved to " + fileInfo.FullName + "!");

                    UnsavedChanges = false;
                }
            }


            return new($"Saving to {new FileInfo(SavePath).Name}...", bgWorker);
        }

        public BackgroundTask? ExportAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new NullReferenceException("filePath is null or white space!");

            BackgroundWorker bgWorker = new();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += ExportWork;
            bgWorker.RunWorkerCompleted += ExportWorkComplete;

            ConsoleLogger.Log("Export File: Running BackgroundWorker");
            bgWorker.RunWorkerAsync();

            void ExportWork(object? sender, DoWorkEventArgs args)
            {
                if (sender is not BackgroundWorker bgWorker)
                    throw new Exception("Sender is not a background worker!");

                ConsoleLogger.Log("Export File: Exporting art file to " + filePath);

                FileInfo fileInfo = new(filePath);
                bgWorker.ReportProgress(90, new BackgroundTaskUpdateArgs($"Exporting as {fileInfo.Extension} file...", true));
                IAAPFile<ASCIIArt> AAPFile;

                switch (fileInfo.Extension)
                {
                    case ".txt":
                        AAPFile = new TXTASCIIArt(Art, fileInfo.FullName);
                        break;
                    case ".aaf":
                        AAPFile = new AAFASCIIArt(Art, fileInfo.FullName);
                        break;
                    default:
                        throw new Exception("Unknown file extension!");
                }

                bool exportSuccess = AAPFile.Export(bgWorker);

                if (bgWorker.CancellationPending && !exportSuccess)
                {
                    args.Cancel = true;
                    return;
                }

                args.Result = fileInfo;
            }

            void ExportWorkComplete(object? sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                    ConsoleLogger.Error("Export File: ", e.Error);
                else if (e.Cancelled)
                    ConsoleLogger.Inform("Export File: Art file export cancelled");
                else if (e.Result is FileInfo fileInfo)
                    ConsoleLogger.Log("Export File: Art file exported to " + fileInfo.FullName + "!");
                else
                    ConsoleLogger.Warn("Export File: Art file export was successful, but result is not file info");
            }

            return new($"Exporting to {new FileInfo(filePath).Name}...", bgWorker);
        }

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
