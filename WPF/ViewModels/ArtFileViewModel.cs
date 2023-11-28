using AAP.BackgroundTasks;
using AAP.FileObjects;
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
        public ICommand ImportFileCommand { get; set; }
        public ICommand EditFileCommand { get; set; }
        public ICommand CloseOpenFileCommand { get; set; }

        public ICommand DeleteSelectedCommand { get; private set; }
        public ICommand SelectCanvasCommand { get; private set; }
        public ICommand SelectLayerCommand { get; private set; }
        public ICommand CancelSelectionCommand { get; private set; }

        public ICommand CropArtCommand { get; private set; }
        public ICommand CropLayerCommand { get; private set; }
        public ICommand FitAllLayersWithinArtCommand { get; private set; }

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
            ImportFileCommand = new ActionCommand(async (parameter) => { if (CurrentArtFile != null) await ImportFileAsync(); });
            EditFileCommand = new ActionCommand((parameter) => EditFile());
            CloseOpenFileCommand = new ActionCommand(async (parameter) => await CloseOpenFileAsync(parameter));

            DeleteSelectedCommand = new ActionCommand((parameter) => App.DeleteSelection());
            SelectCanvasCommand = new ActionCommand((parameter) => App.SelectCanvas());
            SelectLayerCommand = new ActionCommand((parameter) => App.SelectLayer());
            CancelSelectionCommand = new ActionCommand((parameter) => App.CancelArtSelection());

            CropArtCommand = new ActionCommand((parameter) => App.CropArtFileToSelected());
            CropLayerCommand = new ActionCommand((parameter) => App.CropCurrentArtLayerToSelected());
            FitAllLayersWithinArtCommand = new ActionCommand((parameter) => FitAllLayersWithinArt());

            UndoCommand = new ActionCommand((parameter) => CurrentArtFile?.ArtTimeline.Rollback());
            RedoCommand = new ActionCommand((parameter) => CurrentArtFile?.ArtTimeline.Rollforward());

            CutCommand = new ActionCommand((parameter) => App.CutSelectedArt());
            CopyCommand = new ActionCommand((parameter) => App.CopySelectedArtToClipboard());
            PasteCommand = new ActionCommand((parameter) => App.PasteLayerFromClipboard());

            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content

        private string fileMenuContent = App.Language.GetString("FileMenu");
        public string FileMenuContent => fileMenuContent;

        private string newFileContent = App.Language.GetString("NewFile");
        public string NewFileContent => newFileContent;

        private string openFileContent = App.Language.GetString("OpenFile");
        public string OpenFileContent => openFileContent;

        private string saveFileContent = App.Language.GetString("SaveFile");
        public string SaveFileContent => saveFileContent;

        private string saveAsFileContent = App.Language.GetString("SaveAsFile");
        public string SaveAsFileContent => saveAsFileContent;

        private string exportFileContent = App.Language.GetString("ExportFile");
        public string ExportFileContent => exportFileContent;

        private string copyArtToClipboardContent = App.Language.GetString("CopyClipboardArt");
        public string CopyArtToClipboardContent => copyArtToClipboardContent;

        private string importLayerContent = App.Language.GetString("ImportLayer");
        public string ImportLayerContent => importLayerContent;

        private string editFileContent = App.Language.GetString("EditFile");
        public string EditFileContent => editFileContent;

        private string editMenuContent = App.Language.GetString("EditMenu");
        public string EditMenuContent => editMenuContent;

        private string undoContent = App.Language.GetString("Undo");
        public string UndoContent => undoContent;

        private string redoContent = App.Language.GetString("Redo");
        public string RedoContent => redoContent;

        private string cutSelectionContent = App.Language.GetString("CutSelection");
        public string CutSelectionContent => cutSelectionContent;

        private string copySelectionContent = App.Language.GetString("CopySelection");
        public string CopySelectionContent => copySelectionContent;

        private string pasteLayerContent = App.Language.GetString("PasteLayer");
        public string PasteLayerContent => pasteLayerContent;

        private string deleteSelectionContent = App.Language.GetString("DeleteSelection");
        public string DeleteSelectionContent => deleteSelectionContent;

        private string selectCanvasContent = App.Language.GetString("SelectCanvas");
        public string SelectCanvasContent => selectCanvasContent;

        private string selectLayerContent = App.Language.GetString("SelectLayer");
        public string SelectLayerContent => selectLayerContent;

        private string cancelSelectionContent = App.Language.GetString("CancelSelection");
        public string CancelSelectionContent => cancelSelectionContent;

        private string cropCanvasContent = App.Language.GetString("CropCanvas");
        public string CropCanvasContent => cropCanvasContent;

        private string cropLayerContent = App.Language.GetString("CropLayer");
        public string CropLayerContent => cropLayerContent;

        private string fitAllLayersContent = App.Language.GetString("FitLayersInCanvas");
        public string FitAllLayersContent => fitAllLayersContent;

        private void OnLanguageChanged(Language language)
        {
            fileMenuContent = App.Language.GetString("FileMenu");
            newFileContent = App.Language.GetString("NewFile");
            openFileContent = App.Language.GetString("OpenFile");
            saveFileContent = App.Language.GetString("SaveFile");
            saveAsFileContent = App.Language.GetString("SaveAsFile");
            exportFileContent = App.Language.GetString("ExportFile");
            copyArtToClipboardContent = App.Language.GetString("CopyClipboardArt");
            importLayerContent = App.Language.GetString("ImportLayer");
            editFileContent = App.Language.GetString("EditFile");
            pasteLayerContent = App.Language.GetString("PasteLayer");

            editMenuContent = App.Language.GetString("EditMenu");
            undoContent = App.Language.GetString("Undo");
            redoContent = App.Language.GetString("Redo");
            cutSelectionContent = App.Language.GetString("CutSelection");
            copySelectionContent = App.Language.GetString("CopySelection");
            pasteLayerContent = App.Language.GetString("PasteLayer");
            deleteSelectionContent = App.Language.GetString("DeleteSelection");
            selectCanvasContent = App.Language.GetString("SelectCanvas");
            selectLayerContent = App.Language.GetString("SelectLayer");
            cancelSelectionContent = App.Language.GetString("CancelSelection");
            cropCanvasContent = App.Language.GetString("CropCanvas");
            cropLayerContent = App.Language.GetString("CropLayer");
            fitAllLayersContent = App.Language.GetString("FitLayersInCanvas");

            PropertyChanged?.Invoke(this, new(nameof(FileMenuContent)));
            PropertyChanged?.Invoke(this, new(nameof(NewFileContent)));
            PropertyChanged?.Invoke(this, new(nameof(OpenFileContent)));
            PropertyChanged?.Invoke(this, new(nameof(SaveFileContent)));
            PropertyChanged?.Invoke(this, new(nameof(SaveAsFileContent)));
            PropertyChanged?.Invoke(this, new(nameof(ExportFileContent)));
            PropertyChanged?.Invoke(this, new(nameof(CopyArtToClipboardContent)));
            PropertyChanged?.Invoke(this, new(nameof(ImportLayerContent)));
            PropertyChanged?.Invoke(this, new(nameof(EditFileContent)));

            PropertyChanged?.Invoke(this, new(nameof(EditMenuContent)));
            PropertyChanged?.Invoke(this, new(nameof(UndoContent)));
            PropertyChanged?.Invoke(this, new(nameof(RedoContent)));
            PropertyChanged?.Invoke(this, new(nameof(CutSelectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(CopySelectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(PasteLayerContent)));
            PropertyChanged?.Invoke(this, new(nameof(DeleteSelectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(SelectCanvasContent)));
            PropertyChanged?.Invoke(this, new(nameof(SelectLayerContent)));
            PropertyChanged?.Invoke(this, new(nameof(CancelSelectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(CropCanvasContent)));
            PropertyChanged?.Invoke(this, new(nameof(CropLayerContent)));
            PropertyChanged?.Invoke(this, new(nameof(FitAllLayersContent)));
        }
        #endregion

        /// <summary>
        /// Displays a ASCIIArt Dialog that can update a ASCIIArt object.
        /// </summary>
        /// <returns>The result of the dialog</returns>
        public static bool? ShowASCIIArtDialog(ASCIIArt art, string closeMessage)
        {
            static bool IsValidASCIIArtSize(Size size)
                => size.Width % 1 == 0 && size.Height % 1 == 0 && size.Width >= 1 && size.Height >= 1;

            string dialogTitle = App.Language.GetString("ASCIIArt");
            string sizePropertyName = App.Language.GetString("Size");

            string invalidSizeMessage = App.Language.GetString("InvalidSizeMessage");
            string tooLargeFileMessage = App.Language.GetString("File_TooLargeMessage");

            bool successful = false;

            while (!successful)
            {
                PropertiesWindow artWindow = new(dialogTitle, closeMessage);
                artWindow.AddProperty(sizePropertyName, artWindow.CreateInputSizeProperty("Size", new(art.Width, art.Height), IsValidASCIIArtSize));
                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return result;

                if (artWindow.GetProperty("Size") is not Size size || !IsValidASCIIArtSize(size))
                {
                    MessageBox.Show(invalidSizeMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                int width = (int)size.Width;
                int height = (int)size.Height;

                if (width * height > App.MaxArtArea)
                    MessageBox.Show(string.Format(tooLargeFileMessage, App.MaxArtArea, width * height), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                else if (width == 0 || height == 0)
                    MessageBox.Show(invalidSizeMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
            string dialogTitle = App.Language.GetString("ImageASCIIArtImportOptions");
            string importButtonContent = App.Language.GetString("ImportFile");

            string invertBrightnessPropertyName = App.Language.GetString("InvertBrightness");
            string scalePropertyName = App.Language.GetString("Scale");

            string charactersPropertyName = App.Language.GetString("ImageArtLayerConverter_Characters");
            string charactersPropertyInfo = App.Language.GetString("ImageArtLayerConverter_Characters_Info");

            string invalidPropertyNameErrorMessage = App.Language.GetString("Error_DefaultInvalidPropertyMessage");
            string invalidCharactersPropertyErrorMessage = App.Language.GetString("Error_ImageArtLayerConverter_Characters_Invalid");

            bool successful = false;

            string defaultCharactersString = "";
            foreach (char character in options.ImageArtLayerConverter.Characters)
                defaultCharactersString += character;

            while (!successful)
            {
                PropertiesWindow artWindow = new(dialogTitle, importButtonContent);
                artWindow.AddProperty(invertBrightnessPropertyName, artWindow.CreateBoolProperty("Invert", options.ImageArtLayerConverter.Invert));
                artWindow.AddProperty(scalePropertyName, artWindow.CreateSliderProperty("Scale", 0.01, 4, options.Scale, 0.01));

                artWindow.AddProperty(charactersPropertyName, artWindow.CreateInputStringProperty("Characters", defaultCharactersString));
                artWindow.AddLabel(charactersPropertyInfo, 12, 2);

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return result;

                if (artWindow.GetProperty("Invert") is not bool invert)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, invertBrightnessPropertyName), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("Scale") is not double scale)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, scalePropertyName), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("Characters") is not string charactersString || charactersString.Length < 1)
                {
                    MessageBox.Show(invalidCharactersPropertyErrorMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                ImageArtLayerConverter converter = new();
                converter.Invert = invert;
                converter.Characters = charactersString.ToCharArray();

                options.ImageArtLayerConverter = converter;
                options.Scale = scale;

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
            string dialogTitle = App.Language.GetString("ImageASCIIArtExportOptions");
            string exportButtonContent = App.Language.GetString("ExportFile");

            string textSizePropertyName = App.Language.GetString("TextSize");
            string colorsCategoryName = App.Language.GetString("Colors");
            string backgroundColorPropertyName = App.Language.GetString("BackgroundColor");
            string textColorPropertyName = App.Language.GetString("TextColor");

            string invalidPropertyNameErrorMessage = App.Language.GetString("Error_DefaultInvalidPropertyMessage");
            string invalidTextSizeErrorMessage = App.Language.GetString("Error_InvalidImageExportTextSizeMessage");

            bool successful = false;

            while (!successful)
            {
                PropertiesWindow artWindow = new(dialogTitle, exportButtonContent);
                artWindow.AddProperty(textSizePropertyName, artWindow.CreateInputDoubleProperty("TextSize", 12));
                artWindow.AddCategory(colorsCategoryName);
                artWindow.AddProperty(backgroundColorPropertyName, artWindow.CreateColorProperty("Background", Colors.White), 1);
                artWindow.AddProperty(textColorPropertyName, artWindow.CreateColorProperty("Text", Colors.Black), 1);

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return result;

                if (artWindow.GetProperty("TextSize") is not double textSize || textSize < 1 || textSize > 256)
                {
                    MessageBox.Show(invalidTextSizeErrorMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("Background") is not Color backgroundColor)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, backgroundColorPropertyName), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("Text") is not Color textColor)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, textColorPropertyName), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), NewFileContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            ASCIIArt art = new();
            art.SetSize(32, 16);
            art.Version = ASCIIArt.ARTVERSION;

            ConsoleLogger.Log("Version: " + art.Version);

            bool? result = ShowASCIIArtDialog(art, App.Language.GetString("Create"));

            if (result != true)
                return;

            art.ArtLayers.Add(new(App.Language.GetString("Default_Layers_Background"), art.Width, art.Height));
            App.SetArtAsNewFile(art);
        }

        public async Task OpenFilePathAsync(string filePath)
        {
            FileInfo fileInfo = new(filePath);

            ASCIIArtDecodeOptions? importOptions = null;
            if (fileInfo.Extension == ".png" || fileInfo.Extension == ".bmp" || fileInfo.Extension == ".jpg" || fileInfo.Extension == ".jpeg" || fileInfo.Extension == ".gif")
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
                BackgroundTaskToken bgTask = new(string.Format(App.Language.GetString("File_OpenBusy"), fileInfo.Name));
                Task<ASCIIArtFile> task = ASCIIArtFile.OpenAsync(filePath, importOptions, bgTask);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                CanUseTool = false;

                artFile = await task;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show(string.Format(App.Language.GetString("File_OpenFailedMessage"), ex.Message), OpenFileContent, MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;

            CanUseTool = true;

            if (artFile == null)
                return;

            if (artFile.Art.Version != ASCIIArt.ARTVERSION)
                MessageBox.Show(string.Format(App.Language.GetString("File_DifferentVersionMessage"), artFile.Art.Version, ASCIIArt.ARTVERSION), OpenFileContent, MessageBoxButton.OK, MessageBoxImage.Warning);

            int fullArtArea = artFile.Art.GetTotalArtArea();

            if (fullArtArea < artFile.Art.Width * artFile.Art.Height)
                fullArtArea = artFile.Art.Width * artFile.Art.Height;

            if (fullArtArea >= App.WarningLargeArtArea)
            {
                string message = string.Format(App.Language.GetString("File_LargeFileWarningMessage"), fullArtArea, App.WarningLargeArtArea);

                MessageBoxResult msgBoxResult = MessageBox.Show(message, OpenFileContent, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (msgBoxResult == MessageBoxResult.No)
                    return;
            }

            OpenArtFiles.Add(artFile);
            CurrentArtFile = artFile;
        }

        public async Task OpenFileAsync()
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), OpenFileContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new()
            {
                Title = OpenFileContent,
                Filter = App.Language.GetString("SupportedFiles") + " (*.aaf;*.txt;*.png;*.bmp;*.jpg;*.jpeg;*.gif)|*.aaf;*.txt;*.png;*.bmp;*.jpg;*.jpeg;*.gif|ASCII Art Files (*.aaf)|*.aaf|Text Files (*.txt)|*.txt|Image Files (*.png;*.bmp;*.jpg;*.jpeg;*.gif)|*.png;*.bmp;*.jpg;*.jpeg;*.gif",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = App.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };

            bool? result = openFileDialog.ShowDialog();

            if (result != true)
                return;

            await OpenFilePathAsync(openFileDialog.FileName);
        }

        public async Task ImportFileAsync()
        {
            if (CurrentArtFile == null)
                return;

            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), ImportLayerContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new()
            {
                Title = ImportLayerContent,
                Filter = App.Language.GetString("SupportedFiles") + " (*.txt;*.png;*.bmp;*.jpg;*.jpeg;*.gif)|*.txt;*.png;*.bmp;*.jpg;*.jpeg;*.gif|Text Files (*.txt)|*.txt|Image Files (*.png;*.bmp;*.jpg;*.jpeg;*.gif)|*.png;*.bmp;*.jpg;*.jpeg;*.gif",
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
            if (fileInfo.Extension == ".png" || fileInfo.Extension == ".bmp" || fileInfo.Extension == ".jpg" || fileInfo.Extension == ".jpeg" || fileInfo.Extension == ".gif")
            {
                ImageASCIIArtDecodeOptions imageImportOptions = new();
                bool? createdImportOptions = ShowImageASCIIArtImportOptionsDialog(imageImportOptions);

                if (createdImportOptions != true)
                    return;

                importOptions = imageImportOptions;
            }

            try
            {
                BackgroundTaskToken bgTask = new(string.Format(App.Language.GetString("File_ImportBusy"), fileInfo.Name));
                Task task = CurrentArtFile.ImportFileAsync(fileInfo.FullName, importOptions, bgTask);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                CanUseTool = false;

                await task;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show(string.Format(App.Language.GetString("File_ImportFailedMessage"), ex.Message), ImportLayerContent, MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;

            CanUseTool = true;
        }

        private async Task SaveFileToPathAsync(ASCIIArtFile artFile, string savePath)
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), SaveFileContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            artFile.SavePath = savePath;

            try
            {
                FileInfo fileInfo = new(savePath);

                BackgroundTaskToken? bgTask = new(string.Format(App.Language.GetString("File_SaveBusy"), fileInfo.Name));
                Task task = artFile.SaveAsync(bgTask);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                CanUseTool = false;

                await task;

                MessageBox.Show(string.Format(App.Language.GetString("File_SaveSuccessMessage"), fileInfo.Name), SaveFileContent, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show(string.Format(App.Language.GetString("File_SaveFailedMessage"), ex.Message), SaveFileContent, MessageBoxButton.OK, MessageBoxImage.Error);

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
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), SaveFileContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (file.SavePath == null)
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Title = SaveFileContent,
                    Filter = App.Language.GetString("ASCIIArt") + " (*.aaf)|*.aaf",
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
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), SaveAsFileContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = SaveAsFileContent,
                Filter = App.Language.GetString("ASCIIArt") + " (*.aaf)|*.aaf",
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
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), ExportFileContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = ExportFileContent,
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

                BackgroundTaskToken bgTask = new(string.Format(App.Language.GetString("File_ExportBusy"), fileInfo.Name));
                Task task = artFile.ExportAsync(savePath, exportOptions, bgTask);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                CanUseTool = false;

                await task;

                MessageBoxResult msgResult = MessageBox.Show(string.Format(App.Language.GetString("File_ExportSuccessMessage"), fileInfo.Name), ExportFileContent, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (msgResult == MessageBoxResult.Yes)
                    Process.Start("explorer.exe", savePath);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show(string.Format(App.Language.GetString("File_ExportFailedMessage"), ex.Message), ExportFileContent, MessageBoxButton.OK, MessageBoxImage.Error);
                
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

            MessageBox.Show(App.Language.GetString("CopyClipboardArt_Message"), CopyArtToClipboardContent, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void EditFile()
        {
            if (CurrentArtFile == null)
                return;

            ShowASCIIArtDialog(CurrentArtFile.Art, App.Language.GetString("Edit"));
        }

        public async Task CloseOpenFileAsync(object? parameter)
        {
            if (parameter is not ASCIIArtFile file)
                throw new Exception("parameter is not ASCIIArtFile!");

            string closeFileTitle = App.Language.GetString("File_Close");

            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), closeFileTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (file.UnsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show(App.Language.GetString("File_CloseWithoutSavingWarningMessage"), closeFileTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

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

            MessageBoxResult result = MessageBox.Show(App.Language.GetString("FitLayersInCanvas_WarningMessage"), FitAllLayersContent, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                App.FitAllLayersWithinArt();
        }
    }
}
