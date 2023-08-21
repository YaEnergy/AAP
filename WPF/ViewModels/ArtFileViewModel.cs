using AAP.BackgroundTasks;
using AAP.Timelines;
using AAP.UI.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP.UI.ViewModels
{
    public class ArtFileViewModel : INotifyPropertyChanged
    {
        private ASCIIArt? currentArt = null;
        public ASCIIArt? CurrentArt
        {
            get => currentArt;
            set
            {
                if (currentArt == value)
                    return;

                currentArt = value;
                HasArtOpen = value != null;
                PropertyChanged?.Invoke(this, new(nameof(CurrentArt)));
            }
        }

        private ObjectTimeline? currentArtTimeline = null;
        public ObjectTimeline? CurrentArtTimeline
        {
            get => currentArtTimeline;
            set
            {
                if (currentArtTimeline == value)
                    return;

                currentArtTimeline = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentArtTimeline)));
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

        private BackgroundTask? currentBackgroundTask = null;
        public BackgroundTask? CurrentBackgroundTask
        {
            get => currentBackgroundTask;
            set
            {
                if (currentBackgroundTask == value)
                    return;

                currentBackgroundTask = value;
                BackgroundTaskVisibility = CurrentBackgroundTask == null ? Visibility.Collapsed : Visibility.Visible;

                PropertyChanged?.Invoke(this, new(nameof(CurrentBackgroundTask)));
            }
        }

        private Visibility backgroundTaskVisibility = Visibility.Collapsed;
        public Visibility BackgroundTaskVisibility
        {
            get => backgroundTaskVisibility;
            set
            {
                if (backgroundTaskVisibility == value)
                    return;

                backgroundTaskVisibility = value;

                PropertyChanged?.Invoke(this, new(nameof(BackgroundTaskVisibility)));
            }
        }

        public ICommand? NewFileCommand { get; set; }
        public ICommand? OpenFileCommand { get; set; }
        public ICommand? SaveFileCommand { get; set; }
        public ICommand? SaveAsFileCommand { get; set; }
        public ICommand? ExportFileCommand { get; set; }
        public ICommand? CopyArtToClipboardCommand { get; set; }

        public ICommand DeleteSelectedCommand { get; private set; }
        public ICommand SelectArtCommand { get; private set; }
        public ICommand SelectLayerCommand { get; private set; }
        public ICommand CancelSelectionCommand { get; private set; }

        public ICommand CropArtCommand { get; private set; }
        public ICommand CropLayerCommand { get; private set; }

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
            OpenFileCommand = new ActionCommand((parameter) => OpenFile());
            SaveFileCommand = new ActionCommand((parameter) => SaveFile());
            SaveAsFileCommand = new ActionCommand((parameter) => SaveAsFile());
            ExportFileCommand = new ActionCommand((parameter) => ExportFile());
            CopyArtToClipboardCommand = new ActionCommand((parameter) => CopyArtToClipboard());

            DeleteSelectedCommand = new ActionCommand((parameter) => App.FillSelectedWith(null));
            SelectArtCommand = new ActionCommand((parameter) => App.SelectArt());
            SelectLayerCommand = new ActionCommand((parameter) => App.SelectLayer());
            CancelSelectionCommand = new ActionCommand((parameter) => App.CancelArtSelection());

            CropArtCommand = new ActionCommand((parameter) => App.CropArtFileToSelected());
            CropLayerCommand = new ActionCommand((parameter) => App.CropCurrentArtLayerToSelected());

            FillSelectionCommand = new ActionCommand((parameter) => FillSelection());

            UndoCommand = new ActionCommand((parameter) => App.CurrentArtTimeline?.Rollback());
            RedoCommand = new ActionCommand((parameter) => App.CurrentArtTimeline?.Rollforward());

            CutCommand = new ActionCommand((parameter) => App.CutSelectedArt());
            CopyCommand = new ActionCommand((parameter) => App.CopySelectedArtToClipboard());
            PasteCommand = new ActionCommand((parameter) => App.PasteLayerFromClipboard());
        }

        #region Background Task Work
        private BackgroundTask? NewFileAsync()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to create a new file.", "New File", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            NewASCIIArtDialogWindow newASCIIArtDialogWindow = new();
            newASCIIArtDialogWindow.ShowDialog();

            if (newASCIIArtDialogWindow.DialogResult == true)
            {
                BackgroundTask? bgTask = App.CreateNewArtFileAsync(newASCIIArtDialogWindow.ArtWidth, newASCIIArtDialogWindow.ArtHeight);

                if (bgTask == null)
                    return bgTask;

                CanUseTool = false;

                bgTask.Worker.RunWorkerCompleted += BackgroundCreateComplete;

                CurrentBackgroundTask = bgTask;

                return bgTask;
            }

            return null;
        }

        private BackgroundTask? OpenFileAsync()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to open file.", "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            OpenFileDialog openFileDialog = new()
            {
                Title = "Open ASCII Art File",
                Filter = "ASCII Art Files (*.aaf)|*.aaf|Text Files (*.txt)|*.txt",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = App.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                BackgroundTask? bgTask = App.OpenArtFileAsync(new(openFileDialog.FileName));

                if (bgTask == null)
                    return bgTask;

                CanUseTool = false;

                bgTask.Worker.RunWorkerCompleted += BackgroundOpenComplete;

                CurrentBackgroundTask = bgTask;

                return bgTask;
            }

            return null;
        }

        private BackgroundTask? SaveFileAsync(string? savePath)
        {
            if (CurrentArt == null)
                return null;

            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save file.", "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (savePath == null)
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
                    savePath = saveFileDialog.FileName;
                else
                    return null;
            }

            BackgroundTask? bgTask = App.SaveArtFileToPathAsync(savePath);

            if (bgTask == null)
                return bgTask;

            CanUseTool = false;

            bgTask.Worker.RunWorkerCompleted += BackgroundSaveComplete;

            CurrentBackgroundTask = bgTask;

            return bgTask;
        }

        private BackgroundTask? ExportFileAsync()
        {
            if (CurrentArt == null)
                return null;

            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to export file.", "Export File", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Export ASCII Art File",
                Filter = "Text Files (*.txt)|*.txt",
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                InitialDirectory = App.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };

            bool? result = saveFileDialog.ShowDialog();

            string savePath;
            if (result == true)
                savePath = saveFileDialog.FileName;
            else
                return null;

            BackgroundTask? bgTask = App.ExportArtFileToPathAsync(savePath);

            if (bgTask == null)
                return bgTask;

            CanUseTool = false;

            bgTask.Worker.RunWorkerCompleted += BackgroundExportComplete;

            CurrentBackgroundTask = bgTask;

            return bgTask;
        }
        #endregion
        #region Background Task Complete
        void BackgroundSaveComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (CurrentBackgroundTask != null)
                if (bgWorker == CurrentBackgroundTask.Worker)
                {
                    CurrentBackgroundTask = null;
                    CanUseTool = true;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled saving art file!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while saving art file!\nException: " + args.Error.Message, "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Save Art File did not return file info!");

                MessageBox.Show("Saved art file to " + fileInfo.FullName + "!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void BackgroundExportComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (CurrentBackgroundTask != null)
                if (bgWorker == CurrentBackgroundTask.Worker)
                {
                    CurrentBackgroundTask = null;
                    CanUseTool = true;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled exporting art file!", "Export File", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while exporting art file!\nException: " + args.Error.Message, "Export File", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Export Art File did not return file info!");

                MessageBox.Show("Exported art file to " + fileInfo.FullName + "!", "Export File", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start("explorer.exe", fileInfo.DirectoryName ?? App.DefaultArtFilesDirectoryPath);
            }
        }

        void BackgroundOpenComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (CurrentBackgroundTask != null)
                if (bgWorker == CurrentBackgroundTask.Worker)
                {
                    CurrentBackgroundTask = null;
                    CanUseTool = true;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled opening art file!", "Open File", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while opening art file!\nException: " + args.Error.Message, "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void BackgroundCreateComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (CurrentBackgroundTask != null)
                if (bgWorker == CurrentBackgroundTask.Worker)
                {
                    CurrentBackgroundTask = null;
                    CanUseTool = true;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled creating art file!", "Create Art", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while creating art file!\nException: " + args.Error.Message, "Create Art", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        public void NewFile()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to create a new file.", "New File", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            if (CurrentArt != null)
            {
                if (CurrentArt.UnsavedChanges)
                {
                    MessageBoxResult result = MessageBox.Show("You've made some changes that haven't been saved.\nWould you like to save?", "Save ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveFile();

                        if (CurrentBackgroundTask == null)
                            return;

                        CurrentBackgroundTask.Worker.RunWorkerCompleted += (sender, e) => NewFileAsync();

                        return;
                    }
                }
            }

            NewFileAsync();
        }

        public void OpenFile()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to create a new file.", "New File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CurrentArt != null)
            {
                if (CurrentArt.UnsavedChanges)
                {
                    MessageBoxResult result = MessageBox.Show("You've made some changes that haven't been saved.\nWould you like to save?", "Save ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveFile();

                        if (CurrentBackgroundTask == null)
                            return;

                        CurrentBackgroundTask.Worker.RunWorkerCompleted += (sender, e) => OpenFileAsync();

                        return;
                    }
                }
            }

            OpenFileAsync();
        }

        public void SaveFile()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save.", "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileAsync(App.CurrentFilePath);
        }

        public void SaveAsFile()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save as a new file.", "Save As File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileAsync(null);
        }

        public void ExportFile()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save as a new file.", "Export File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ExportFileAsync();
        }

        public void CopyArtToClipboard()
        {
            if (CurrentArt == null)
                return;

            string artString = CurrentArt.GetArtString();
            Clipboard.SetText(artString);
        }

        public void FillSelection()
        {
            if (CurrentTool is not PencilTool drawTool || CurrentTool.Type != ToolType.Draw)
                return;

            if (!CanUseTool)
                return;

            App.FillSelectedWith(drawTool.Character);
        }
    }
}
