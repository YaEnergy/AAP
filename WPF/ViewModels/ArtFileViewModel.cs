﻿using AAP.BackgroundTasks;
using AAP.Files;
using AAP.Timelines;
using AAP.UI.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AAP.UI.ViewModels
{
    public class ArtFileViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ASCIIArtFile> openArtFiles = new();
        public ObservableCollection<ASCIIArtFile> OpenArtFiles
        {
            get => openArtFiles;
            set
            {
                if (openArtFiles == value)
                    return;

                openArtFiles = value;

                PropertyChanged?.Invoke(this, new(nameof(OpenArtFiles)));
            }
        }

        private ASCIIArtFile? currentArtFile = null;
        public ASCIIArtFile? CurrentArtFile
        {
            get => currentArtFile;
            set
            {
                if (currentArtFile == value)
                    return;

                currentArtFile = value;
                HasArtOpen = value != null;
                PropertyChanged?.Invoke(this, new(nameof(CurrentArtFile)));
            }
        }

        private bool hasArtOpen = false;
        public bool HasArtOpen
        {
            get => hasArtOpen;
            private set
            {
                if (hasArtOpen == value)
                    return;

                hasArtOpen = value;
                PropertyChanged?.Invoke(this, new(nameof(HasArtOpen)));
            }
        }

        private Tool? currentTool = null;
        public Tool? CurrentTool
        {
            get => currentTool;
            set
            {
                if (currentTool == value)
                    return;

                currentTool = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentTool)));
            }
        }

        private bool canUseTool = true;
        public bool CanUseTool
        {
            get => canUseTool;
            set
            {
                if (canUseTool == value)
                    return;

                canUseTool = value;
                PropertyChanged?.Invoke(this, new(nameof(CanUseTool)));
            }
        }

        private bool hasSelected = false;
        public bool HasSelected
        {
            get => hasSelected;
            set
            {
                if (hasSelected == value)
                    return;

                hasSelected = value;
                PropertyChanged?.Invoke(this, new(nameof(HasSelected)));
            }
        }

        private BackgroundTaskToken? currentBackgroundTaskToken = null;
        public BackgroundTaskToken? CurrentBackgroundTaskToken
        {
            get => currentBackgroundTaskToken;
            set
            {
                if (currentBackgroundTaskToken == value)
                    return;

                currentBackgroundTaskToken = value;

                PropertyChanged?.Invoke(this, new(nameof(CurrentBackgroundTaskToken)));
            }
        }

        public ICommand NewFileCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand SaveFileCommand { get; set; }
        public ICommand SaveAsFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        public ICommand CopyArtToClipboardCommand { get; set; }
        public ICommand EditFileCommand { get; set; }
        public ICommand CloseOpenFileCommand { get; set; }

        public ICommand DeleteSelectedCommand { get; private set; }
        public ICommand SelectArtCommand { get; private set; }
        public ICommand SelectLayerCommand { get; private set; }
        public ICommand CancelSelectionCommand { get; private set; }

        public ICommand CropArtCommand { get; private set; }
        public ICommand CropLayerCommand { get; private set; }
        public ICommand FitAllLayersWithinArtCommand { get; private set; }

        public ICommand FillSelectionCommand { get; private set; }

        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }

        public ICommand CutCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand PasteCommand { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ArtFileViewModel() 
        {
            NewFileCommand = new ActionCommand((parameter) => NewFile());
            OpenFileCommand = new ActionCommand(async (parameter) => await OpenFileAsync());
            SaveFileCommand = new ActionCommand(async (parameter) => { if (CurrentArtFile != null) await SaveFileAsync(CurrentArtFile); });
            SaveAsFileCommand = new ActionCommand(async (parameter) => { if (CurrentArtFile != null) await SaveAsFileAsync(CurrentArtFile); });
            ExportFileCommand = new ActionCommand(async (parameter) => { if (CurrentArtFile != null) await ExportFileAsync(CurrentArtFile); });
            CopyArtToClipboardCommand = new ActionCommand((parameter) => CopyCurrentArtToClipboard());
            EditFileCommand = new ActionCommand((parameter) => EditFile());
            CloseOpenFileCommand = new ActionCommand(async (parameter) => await CloseOpenFileAsync(parameter));

            DeleteSelectedCommand = new ActionCommand((parameter) => App.FillSelectedWith(null));
            SelectArtCommand = new ActionCommand((parameter) => App.SelectArt());
            SelectLayerCommand = new ActionCommand((parameter) => App.SelectLayer());
            CancelSelectionCommand = new ActionCommand((parameter) => App.CancelArtSelection());

            CropArtCommand = new ActionCommand((parameter) => App.CropArtFileToSelected());
            CropLayerCommand = new ActionCommand((parameter) => App.CropCurrentArtLayerToSelected());
            FitAllLayersWithinArtCommand = new ActionCommand((parameter) => FitAllLayersWithinArt());

            FillSelectionCommand = new ActionCommand((parameter) => FillSelection());

            UndoCommand = new ActionCommand((parameter) => CurrentArtFile?.ArtTimeline.Rollback());
            RedoCommand = new ActionCommand((parameter) => CurrentArtFile?.ArtTimeline.Rollforward());

            CutCommand = new ActionCommand((parameter) => App.CutSelectedArt());
            CopyCommand = new ActionCommand((parameter) => App.CopySelectedArtToClipboard());
            PasteCommand = new ActionCommand((parameter) => App.PasteLayerFromClipboard());
        }

        /// <summary>
        /// Displays a ASCIIArt Dialog that can update a ASCIIArt object.
        /// </summary>
        /// <returns>The result of the dialog</returns>
        public static bool? ShowASCIIArtDialog(ASCIIArt art, string closeMessage)
        {
            bool successful = false;

            while (!successful)
            {
                PropertiesWindow artWindow = new("ASCII Art", closeMessage);
                artWindow.AddSizeIntProperty("Size", new(art.Width, art.Height));
                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return result;

                if (artWindow.GetProperty("Size") is not Size size)
                {
                    MessageBox.Show($"Invalid size!", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (size.Width % 1 != 0 && size.Height % 1 != 0)
                {
                    MessageBox.Show($"Width & height must be above 0 and natural numbers!", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                int width = (int)size.Width;
                int height = (int)size.Height;

                if (width * height > App.MaxArtArea)
                    MessageBox.Show($"Canvas is too large! Max: {App.MaxArtArea} characters ({width * height} characters)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (width == 0 || height == 0)
                    MessageBox.Show($"Width & height must be above 0 and natural numbers!", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    art.SetSize(width, height);

                    successful = true;
                }
            }

            return successful;
        }

        /// <summary>
        /// Displays a Image ASCII Art Import Options Dialog that can update a ImageASCIIArtDecodeOptions object.
        /// </summary>
        /// <returns>The result of the dialog</returns>
        public static bool? ShowImageASCIIArtImportOptionsDialog(ImageASCIIArtDecodeOptions options)
        {
            bool successful = false;

            while (!successful)
            {
                PropertiesWindow artWindow = new("Image ASCII Art Import Options", "Import");
                artWindow.AddBoolProperty("Invert Brightness", false);
                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return result;

                if (artWindow.GetProperty("Invert Brightness") is not bool invert)
                {
                    MessageBox.Show($"Invalid Invert Brightness!", "Image ASCII Art Import Options", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                ImageArtLayerConverter converter = new();
                converter.Invert = invert;

                options.ImageArtLayerConverter = converter;

                successful = true;
            }

            return successful;
        }

        /// <summary>
        /// Displays a Image ASCII Art Export Options Dialog that can update a ImageASCIIArtEncodeOptions object.
        /// </summary>
        /// <returns>The result of the dialog</returns>
        public static bool? ShowImageASCIIArtExportOptionsDialog(ImageASCIIArtEncodeOptions options)
        {
            bool successful = false;

            while (!successful)
            {
                PropertiesWindow artWindow = new("Image ASCII Art Export Options", "Export");
                artWindow.AddDoubleProperty("Text Size", 12);
                artWindow.AddCategory("Colors");
                artWindow.AddColorProperty("Background", Colors.White);
                artWindow.AddColorProperty("Text", Colors.Black);

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return result;

                if (artWindow.GetProperty("Text Size") is not double textSize || textSize < 1 || textSize > 256)
                {
                    MessageBox.Show($"Invalid Text Size! Text Size must be between 1 and 256", "Image ASCII Art Export Options", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("Background") is not Color backgroundColor)
                {
                    MessageBox.Show($"Invalid Background Color!", "Image ASCII Art Export Options", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("Text") is not Color textColor)
                {
                    MessageBox.Show($"Invalid Text Color!", "Image ASCII Art Export Options", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                options.BackgroundColor = backgroundColor;
                options.TextColor = textColor;
                options.TextSize = textSize;

                successful = true;
            }

            return successful;
        }

        public void NewFile()
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to create a new file.", "New File", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            ASCIIArt art = new();
            art.SetSize(32, 16);

            bool? result = ShowASCIIArtDialog(art, "Create");

            if (result != true)
                return;

            art.ArtLayers.Add(new("Background", art.Width, art.Height));
            App.SetArtAsNewFile(art);
        }

        public async Task OpenFileAsync()
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to open a file.", "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new()
            {
                Title = "Open ASCII Art File",
                Filter = "ASCII Art Files (*.aaf)|*.aaf|Text Files (*.txt)|*.txt|Image Files (*.png;*.bmp;*.jpg;*.gif)|*.png;*.bmp;*.jpg;*.gif",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = App.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };

            bool? result = openFileDialog.ShowDialog();

            if (result != true)
                return;

            FileInfo fileInfo = new(openFileDialog.FileName);

            ASCIIArtDecodeOptions? importOptions = null;
            if (fileInfo.Extension == ".png" || fileInfo.Extension == ".bmp" || fileInfo.Extension == ".jpg" || fileInfo.Extension == ".gif")
            {
                ImageASCIIArtDecodeOptions imageImportOptions = new();
                bool? createdImportOptions = ShowImageASCIIArtImportOptionsDialog(imageImportOptions);

                if (createdImportOptions != true)
                    return;

                importOptions = imageImportOptions;
            }

            ASCIIArtFile? artFile = null;
            try
            {
                BackgroundTaskToken bgTask = new($"Opening {fileInfo.Name}...");
                Task<ASCIIArtFile> task = ASCIIArtFile.OpenAsync(openFileDialog.FileName, importOptions, bgTask);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                CanUseTool = false;

                artFile = await task;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to open art file! Exception message: {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;

            CanUseTool = true;

            if (artFile == null)
                return;

            int fullArtArea = artFile.Art.GetTotalArtArea();

            if (fullArtArea < artFile.Art.Width * artFile.Art.Height)
                fullArtArea = artFile.Art.Width * artFile.Art.Height;

            if (fullArtArea >= App.WarningLargeArtArea)
            {
                string message = $"The art you're trying to create/edit has an total art area of {fullArtArea} characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters.\nThis might take a long time to load and save, and can be performance heavy.\nAre you sure you want to continue?";

                if (fullArtArea >= App.WarningIncrediblyLargeArtArea)
                    message = $"The art you're to trying to create/edit has a total art area of {fullArtArea} characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters and above the less recommended area limit of {App.WarningIncrediblyLargeArtArea} characters.\nThis might take a VERY long time to load and save, and can be INCREDIBLY performance heavy.\nAre you SURE you want to continue?";

                MessageBoxResult msgBoxResult = MessageBox.Show(message, "ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (msgBoxResult == MessageBoxResult.No)
                    return;
            }

            OpenArtFiles.Add(artFile);
            CurrentArtFile = artFile;
        }

        private async Task SaveFileToPathAsync(ASCIIArtFile artFile, string savePath)
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save file.", "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            artFile.SavePath = savePath;

            try
            {
                FileInfo fileInfo = new(savePath);

                BackgroundTaskToken? bgTask = new($"Saving to {fileInfo.Name}...");
                Task task = artFile.SaveAsync(bgTask);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                CanUseTool = false;

                await task;

                MessageBox.Show($"Saved art file to {fileInfo.Name}!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to save art file! Exception message: {ex.Message}", "Save File", MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;

            CanUseTool = true;
        }

        public async Task SaveFileAsync(ASCIIArtFile file)
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save.", "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (file.SavePath == null)
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Title = "Save ASCII Art File",
                    Filter = "ASCII Art File (*.aaf)|*.aaf",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    CreatePrompt = false,
                    OverwritePrompt = true,
                    InitialDirectory = App.DefaultArtFilesDirectoryPath,
                    ValidateNames = true
                };

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                    file.SavePath = saveFileDialog.FileName;
                else
                    return;
            }

            await SaveFileToPathAsync(file, file.SavePath);
        }

        public async Task SaveAsFileAsync(ASCIIArtFile file)
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save as a new file.", "Save As File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Save ASCII Art File",
                Filter = "ASCII Art File (*.aaf)|*.aaf",
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                InitialDirectory = App.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
                file.SavePath = saveFileDialog.FileName;
            else
                return;

            await SaveFileToPathAsync(file, file.SavePath);
        }

        public async Task ExportFileAsync(ASCIIArtFile artFile)
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to export file.", "Export File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Export ASCII Art File",
                Filter = "Text Files (*.txt)|*.txt|Bitmap (*.bmp)|*.bmp|PNG Image (*.png)|*.png|Jpeg Image (*.jpg)|*.jpg|Graphics Interchange Format (*.gif)|*.gif",
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                InitialDirectory = App.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };

            bool? result = saveFileDialog.ShowDialog();
            if (result != true)
                return;

            string savePath = saveFileDialog.FileName;

            ASCIIArtEncodeOptions? exportOptions = null;
            string extension = Path.GetExtension(savePath).ToLower();
            if (extension == ".png" || extension == ".bmp" || extension == ".jpg" || extension == ".gif")
            {
                ImageASCIIArtEncodeOptions imageExportOptions = new();
                bool? createdExportOptions = ShowImageASCIIArtExportOptionsDialog(imageExportOptions);

                if (createdExportOptions != true)
                    return;

                exportOptions = imageExportOptions;
            }

            try
            {
                FileInfo fileInfo = new(savePath);

                BackgroundTaskToken bgTask = new($"Exporting to {fileInfo.Name}...");
                Task task = artFile.ExportAsync(savePath, exportOptions, bgTask);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                CanUseTool = false;

                await task;

                MessageBoxResult msgResult = MessageBox.Show($"Exported art file to {fileInfo.Name}! Open file?", "Export File", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (msgResult == MessageBoxResult.Yes)
                    Process.Start("explorer.exe", savePath);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to export art file! Exception message: {ex.Message}", "Export File", MessageBoxButton.OK, MessageBoxImage.Error);
                
                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;

            CanUseTool = true;
        }

        public void CopyCurrentArtToClipboard()
        {
            if (CurrentArtFile == null)
                return;

            string artString = CurrentArtFile.Art.GetArtString();
            Clipboard.SetText(artString);
        }

        public void FillSelection()
        {
            if (CurrentTool is not ICharacterSelectable characterSelectableTool)
                return;

            if (!CanUseTool)
                return;

            App.FillSelectedWith(characterSelectableTool.Character);
        }

        public void EditFile()
        {
            if (CurrentArtFile == null)
                return;

            ShowASCIIArtDialog(CurrentArtFile.Art, "Edit");
        }

        public async Task CloseOpenFileAsync(object? parameter)
        {
            if (parameter is not ASCIIArtFile file)
                throw new Exception("parameter is not ASCIIArtFile!");

            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to close a file.", "Close File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (file.UnsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show("You've made some changes that haven't been saved.\nWould you like to save?", "Save ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                    await SaveFileAsync(file);
            }

            if (CurrentArtFile == file)
                CurrentArtFile = null;

            OpenArtFiles.Remove(file);

            file.Dispose();
        }

        public void FitAllLayersWithinArt()
        {
            if (CurrentArtFile == null)
                return;

            MessageBoxResult result = MessageBox.Show("This action can remove layers. Are you sure?", "Crop All Layers To Fit Within Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                App.FitAllLayersWithinArt();
        }
    }
}
