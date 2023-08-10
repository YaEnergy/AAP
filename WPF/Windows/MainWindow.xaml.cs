using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AAP.BackgroundTasks;
using AAP.Timelines;
using AAP.UI.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ArtCanvasViewModel artCanvasViewModel { get; set; }

        private BackgroundTask? currentBackgroundTask { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();

            artCanvasViewModel = (ArtCanvasViewModel)FindResource("artCanvasViewModel");

            App.OnCurrentArtChanged += OnCurrentArtChanged;
            App.OnCurrentFilePathChanged += OnCurrentFilePathChanged;
            App.OnCurrentToolChanged += OnCurrentToolChanged;
            App.OnSelectionChanged += OnSelectionChanged;
            App.OnCurrentLayerIDChanged += CurrentLayerIDChanged;
            App.OnAvailableCharacterPalettesChanged += (palettes) => CharacterPaletteSelectionViewModel.Palettes = palettes;

            CharacterPaletteSelectionViewModel.PropertyChanged += CharacterPaletteSelectionViewModelPropertyChanged;
            LayerManagementViewModel.PropertyChanged += LayerSelectionViewModelPropertyChanged;

            artCanvasViewModel.CurrentTool = App.CurrentTool;
            CharacterPaletteSelectionViewModel.Palettes = App.CharacterPalettes;

            OnCurrentArtChanged(App.CurrentArt, App.CurrentArtDraw, App.CurrentArtTimeline);

            UpdateTitle();

            ToolSelectionViewModel.ToolStateBoxes.Add(DrawToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(EraserToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(SelectToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(MoveToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(BucketToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(TextToolStateBox);

            Closing += OnClosing;

            #region Shortcut Commands

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, new((sender, e) => NewFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new((sender, e) => OpenFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new((sender, e) => SaveFileAction())));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.SaveAsShortcut, new((sender, e) => SaveAsFileAction())));

            CommandBindings.Add(new CommandBinding(FileShortcutCommands.ExportAsShortcut, new((sender, e) => ExportAction())));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.CopyToClipboardShortcut, new((sender, e) => CopyArtToClipboardAction())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new((sender, e) => Application.Current.Shutdown())));

            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.EnlargeTextSizeShortcut, new((sender, e) => artCanvasViewModel.EnlargeTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ShrinkTextSizeShortcut, new((sender, e) => artCanvasViewModel.ShrinkTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ResetTextSizeShortcut, new((sender, e) => artCanvasViewModel.ResetTextSize())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, new((sender, e) => App.CurrentArtTimeline?.Rollback())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, new((sender, e) => App.CurrentArtTimeline?.Rollforward())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new((sender, e) => App.CopySelectedArtToClipboard())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, new((sender, e) => App.PasteLayerFromClipboard())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, new((sender, e) => App.FillSelectedWith(null))));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, new((sender, e) => App.SelectArt())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.SelectLayerShortcut, new((sender, e) => App.SelectLayer())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CancelSelectionShortcut, new((sender, e) => App.CancelSelection())));

            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CropArtShortcut, new((sender, e) => App.CropArtFileToSelected())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CropLayerShortcut, new((sender, e) => App.CropCurrentArtLayerToSelected())));

            CommandBindings.Add(new CommandBinding(DrawShortcutCommands.FillSelectionShortcut, new((sender, e) => artCanvasViewModel.FillSelection())));
            #endregion
        }
        
        private void UpdateTitle()
        {
            string sizeText = $"{(App.CurrentArt != null ? App.CurrentArt.Width.ToString() : "*")}x{(App.CurrentArt != null ? App.CurrentArt.Height.ToString() : "*")}";
            string changedText = "";

            if (App.CurrentArt != null)
            {
                sizeText = $"{App.CurrentArt.Width}x{App.CurrentArt.Height}";

                if (App.CurrentArt.UnsavedChanges)
                    changedText = "*";
            }

            string fileName = App.CurrentFilePath == null ? "*.*" : new FileInfo(App.CurrentFilePath).Name;
            
            string fullTitle = $"{App.ProgramTitle} - {fileName}{changedText} - {sizeText}";

            if (fullTitle != Title)
                Title = fullTitle;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (App.CurrentArt == null)
                return;

            if (currentBackgroundTask != null)
            {
                e.Cancel = true;
                
                currentBackgroundTask.Worker.RunWorkerCompleted += (sender, e) => Close();

                BackgroundTaskWindow backgroundTaskWindow = new(new("Waiting for remaining task to finish...", currentBackgroundTask.Worker), true);
                backgroundTaskWindow.Show();
                backgroundTaskWindow.Owner = this;
                
                return;
            }

            if (App.CurrentArt.UnsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show("You've made some changes that haven't been saved.\nWould you like to save?", "Save ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {

                    BackgroundTask? bgTask = SaveFileAction();

                    if (bgTask == null)
                        return;

                    currentBackgroundTask = bgTask;
                    currentBackgroundTask.Worker.RunWorkerCompleted += (sender, e) => Close();
                    e.Cancel = true;

                    return;
                }
            }

        }

        #region App Events
        private void OnCurrentArtChanged(ASCIIArt? art, ASCIIArtDraw? artDraw, ObjectTimeline? artTimeline)
        {
            if (art != null)
                if (art.Width * art.Height * Math.Clamp(art.ArtLayers.Count, 1, int.MaxValue) >= App.WarningLargeArtArea)
                {
                    string message = $"The ASCII Art you're trying to open has an total art area of {art.Width * art.Height * art.ArtLayers.Count} (Area * ArtLayers) characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters.\nThis might take a long time to load and save, and can be performance heavy.\nAre you sure you want to continue?";

                    if (art.Width * art.Height * Math.Clamp(art.ArtLayers.Count, 1, int.MaxValue) >= App.WarningIncrediblyLargeArtArea)
                        message = $"The ASCII Art you're trying to open has an total art area of {art.Width * art.Height * art.ArtLayers.Count} (Area * ArtLayers) characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters and above the less recommended area limit of {App.WarningIncrediblyLargeArtArea} characters.\nThis might take a VERY long time to load and save, and can be INCREDIBLY performance heavy.\nAre you SURE you want to continue?";

                    MessageBoxResult result = MessageBox.Show(message, "ASCII Art Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.No)
                    {
                        App.SetArtAsNewFile(null);
                        return;
                    }
                }

            //Remove old listeners
            if (artCanvasViewModel.CurrentArt != null)
            {
                artCanvasViewModel.CurrentArt.OnUnsavedChangesChanged -= OnArtCanvasViewModelArtUnsavedChangesChanged;
                artCanvasViewModel.CurrentArt.OnSizeChanged -= OnArtCanvasViewModelArtSizeChanged;
            }

            artCanvasViewModel.CurrentArt = art; 
            artCanvasViewModel.CurrentArtDraw = artDraw;
            artCanvasViewModel.CurrentArtTimeline = artTimeline;

            LayerManagementViewModel.Art = art;

            //Add new listeners
            if (artCanvasViewModel.CurrentArt != null)
            {
                artCanvasViewModel.CurrentArt.OnUnsavedChangesChanged += OnArtCanvasViewModelArtUnsavedChangesChanged;
                artCanvasViewModel.CurrentArt.OnSizeChanged += OnArtCanvasViewModelArtSizeChanged;
            }

            UpdateTitle();
        }

        private void OnCurrentFilePathChanged(string? filePath)
            => UpdateTitle();

        private void OnCurrentToolChanged(Tool? tool)
        {
            artCanvasViewModel.CurrentTool = tool;

            CharacterPaletteSelectionViewModel.Visibility = tool?.Type == ToolType.Draw || tool?.Type == ToolType.Bucket ? Visibility.Visible : Visibility.Hidden;

            if (tool == null)
                return;

            switch(tool.Type)
            {
                case ToolType.Draw:
                    if (tool is DrawTool drawTool)
                        CharacterPaletteSelectionViewModel.SelectedCharacter = drawTool.Character;

                    break;
                case ToolType.Bucket:
                    if (tool is BucketTool bucketTool)
                        CharacterPaletteSelectionViewModel.SelectedCharacter = bucketTool.Character;

                    break;
                default:
                    break;
            }
        }

        private void OnSelectionChanged(Rect selected)
            => artCanvasViewModel.Selected = selected;

        private void CurrentLayerIDChanged(int layerID)
            => LayerManagementViewModel.SelectedLayerID = layerID;

        #endregion

        #region Art Canvas View Model Art Events

        private void OnArtCanvasViewModelArtUnsavedChangesChanged(ASCIIArt art, bool unsavedChanges)
            => UpdateTitle();

        private void OnArtCanvasViewModelArtSizeChanged(int width, int height)
            => UpdateTitle();

        #endregion

        #region Background Run Worker Complete Functions
        void BackgroundSaveComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (currentBackgroundTask != null)
                if (bgWorker == currentBackgroundTask.Worker)
                {
                    currentBackgroundTask = null;
                    artCanvasViewModel.CanUseTool = true;
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

            if (currentBackgroundTask != null)
                if (bgWorker == currentBackgroundTask.Worker)
                {
                    currentBackgroundTask = null;
                    artCanvasViewModel.CanUseTool = true;
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

            if (currentBackgroundTask != null)
                if (bgWorker == currentBackgroundTask.Worker)
                {
                    currentBackgroundTask = null;
                    artCanvasViewModel.CanUseTool = true;
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

            if (currentBackgroundTask != null)
                if (bgWorker == currentBackgroundTask.Worker)
                {
                    currentBackgroundTask = null;
                    artCanvasViewModel.CanUseTool = true;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled creating art file!", "Create Art", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while creating art file!\nException: " + args.Error.Message, "Create Art", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        #region Menu Item Actions

        private BackgroundTask? NewFileAction()
        {
            if (currentBackgroundTask != null)
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

                artCanvasViewModel.CanUseTool = false;

                BackgroundTaskWindow backgroundTaskWindow = new(bgTask);
                backgroundTaskWindow.Show();
                backgroundTaskWindow.Owner = this;

                bgTask.Worker.RunWorkerCompleted += BackgroundCreateComplete;

                currentBackgroundTask = bgTask;

                return bgTask;
            }

            return null;
        }

        private BackgroundTask? OpenFileAction()
        {
            if (currentBackgroundTask != null)
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

                artCanvasViewModel.CanUseTool = false;

                BackgroundTaskWindow backgroundTaskWindow = new(bgTask);
                backgroundTaskWindow.Show();
                backgroundTaskWindow.Owner = this;

                bgTask.Worker.RunWorkerCompleted += BackgroundOpenComplete;

                currentBackgroundTask = bgTask;

                return bgTask;
            }

            return null;
        }

        private BackgroundTask? SaveFileAction()
        {
            if (App.CurrentArt == null)
                return null;

            if (currentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save file.", "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string? savePath = App.CurrentFilePath;

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

            artCanvasViewModel.CanUseTool = false;

            BackgroundTaskWindow backgroundTaskWindow = new(bgTask);
            backgroundTaskWindow.Show();
            backgroundTaskWindow.Owner = this;

            bgTask.Worker.RunWorkerCompleted += BackgroundSaveComplete;

            currentBackgroundTask = bgTask;

            return bgTask;
        }

        private BackgroundTask? SaveAsFileAction()
        {
            if (App.CurrentArt == null)
                return null;

            if (currentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to save file.", "Save As File", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
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

            string savePath;
            if (result == true)
                savePath = saveFileDialog.FileName;
            else
                return null;

            BackgroundTask? bgTask = App.SaveArtFileToPathAsync(savePath);

            if (bgTask == null)
                return bgTask;

            artCanvasViewModel.CanUseTool = false;

            BackgroundTaskWindow backgroundTaskWindow = new(bgTask);
            backgroundTaskWindow.Show();
            backgroundTaskWindow.Owner = this;

            bgTask.Worker.RunWorkerCompleted += BackgroundSaveComplete;

            currentBackgroundTask = bgTask;

            return bgTask;
        }

        private BackgroundTask? ExportAction()
        {
            if (App.CurrentArt == null)
                return null;

            if (currentBackgroundTask != null)
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

            artCanvasViewModel.CanUseTool = false;

            BackgroundTaskWindow backgroundTaskWindow = new(bgTask);
            backgroundTaskWindow.Show();
            backgroundTaskWindow.Owner = this;

            bgTask.Worker.RunWorkerCompleted += BackgroundExportComplete;

            currentBackgroundTask = bgTask;

            return bgTask;
        }

        private void CopyArtToClipboardAction()
        {
            if (App.CurrentArt == null)
                return;

            App.CopyArtStringToClipboard();
        }
        #endregion

        #region Click Events

        private void NewFileButton_Click(object sender, RoutedEventArgs e)
            => NewFileAction();

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
            => OpenFileAction();

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
            => SaveFileAction();

        private void SaveAsFileButton_Click(object sender, RoutedEventArgs e)
            => SaveAsFileAction();

        private void ExportButton_Click(object sender, RoutedEventArgs e)
            => ExportAction();

        private void CopyArtToClipboardButton_Click(object sender, RoutedEventArgs e)
            => CopyArtToClipboardAction();

        private void ExitButton_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();

        #endregion

        #region ViewModel Property Changed Events
        private void CharacterPaletteSelectionViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not CharacterPaletteSelectionViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case "SelectedCharacter":
                    App.SelectCharacterTool(vm.SelectedCharacter);
                    break;
                default:
                    break;
            }
        }

        private void LayerSelectionViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not LayerManagementViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case "SelectedLayerID":
                    App.CurrentLayerID = vm.SelectedLayerID;
                    break;
                case "SelectedLayerName":
                    if (vm.SelectedLayer == null)
                        break;

                    if (vm.SelectedLayer.Name == vm.SelectedLayerName)
                        break;

                    App.SetArtLayerName(vm.SelectedLayer, vm.SelectedLayerName);
                    break;
                case "SelectedLayerVisibility":
                    if (vm.SelectedLayer == null)
                        break;

                    if (vm.SelectedLayer.Visible == vm.SelectedLayerVisibility)
                        break;

                    App.SetArtLayerVisibility(vm.SelectedLayer, vm.SelectedLayerVisibility);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
