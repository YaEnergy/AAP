﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AAP.Timelines;
using AAP.UI.ViewModels;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ArtCanvasViewModel artCanvasViewModel { get; set; }
        private BackgroundWorker? currentBackgroundWorker { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();

            artCanvasViewModel = (ArtCanvasViewModel)FindResource("artCanvasViewModel");

            App.OnCurrentArtChanged += OnCurrentArtChanged;
            App.OnCurrentFilePathChanged += OnCurrentFilePathChanged;
            App.OnCurrentToolChanged += OnCurrentToolChanged;
            App.OnSelectionChanged += OnSelectionChanged;
            App.OnAvailableCharacterPalettesChanged += (palettes) => CharacterPaletteSelectionViewModel.Palettes = palettes;

            CharacterPaletteSelectionViewModel.PropertyChanged += OnCharacterPaletteSelectionViewModelPropertyChanged;

            artCanvas.Tool = App.CurrentTool;
            CharacterPaletteSelectionViewModel.Palettes = App.CharacterPalettes;

            UpdateTitle();

            ToolSelectionViewModel.ToolStateBoxes.Add(DrawToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(EraserToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(SelectToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(MoveToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(TextToolStateBox);

            #region Shortcut Commands

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, new((sender, e) => NewFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new((sender, e) => OpenFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new((sender, e) => SaveFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, new((sender, e) => SaveAsFileAction())));

            CommandBindings.Add(new CommandBinding(FileShortcutCommands.ExportAsShortcut, new((sender, e) => ExportAction())));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.CopyToClipboardShortcut, new((sender, e) => CopyArtToClipboardAction())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new((sender, e) => Application.Current.Shutdown())));

            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.EnlargeTextSizeShortcut, new((sender, e) => artCanvasViewModel.EnlargeTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ShrinkTextSizeShortcut, new((sender, e) => artCanvasViewModel.ShrinkTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ResetTextSizeShortcut, new((sender, e) => artCanvasViewModel.ResetTextSize())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, new((sender, e) => App.CurrentArtTimeline?.Rollback())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, new((sender, e) => App.CurrentArtTimeline?.Rollforward())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, new((sender, e) => App.FillSelectedWith(null))));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, new((sender, e) => App.SelectAll())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CancelSelectionShortcut, new((sender, e) => App.CancelSelection())));

            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CropArtShortcut, new((sender, e) => App.CropArtFileToSelected())));
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
                        App.NewFile(null);
                        return;
                    }
                }

            //Remove old listeners
            if (artCanvasViewModel.CurrentArt != null)
            {
                artCanvasViewModel.CurrentArt.OnChanged -= OnArtCanvasViewModelArtChanged;
                artCanvasViewModel.CurrentArt.OnCropped -= OnArtCanvasViewModelArtCropped;
                artCanvasViewModel.CurrentArt.OnCopiedPropertiesOf -= OnArtCanvasViewModelArtCopiedPropertiesOf;
            }

            artCanvasViewModel.CurrentArt = art; 
            artCanvasViewModel.CurrentArtDraw = artDraw;
            artCanvasViewModel.CurrentArtTimeline = artTimeline;

            //Add new listeners
            if (artCanvasViewModel.CurrentArt != null)
            {
                artCanvasViewModel.CurrentArt.OnUnsavedChangesChanged += OnArtCanvasViewModelArtUnsavedChangesChanged;
                artCanvasViewModel.CurrentArt.OnChanged += OnArtCanvasViewModelArtChanged;
                artCanvasViewModel.CurrentArt.OnCropped += OnArtCanvasViewModelArtCropped;
                artCanvasViewModel.CurrentArt.OnCopiedPropertiesOf += OnArtCanvasViewModelArtCopiedPropertiesOf;
            }

            UpdateTitle();
        }

        private void OnCurrentFilePathChanged(string? filePath)
            => UpdateTitle();

        private void OnCurrentToolChanged(Tool? tool)
        {
            artCanvasViewModel.CurrentTool = tool;

            CharacterPaletteSelectionViewModel.Visibility = tool?.Type == ToolType.Draw ? Visibility.Visible : Visibility.Hidden;
        }

        private void OnSelectionChanged(Rect selected)
            => artCanvasViewModel.Selected = selected;
        #endregion

        #region Art Canvas View Model Art Events

        private void OnArtCanvasViewModelArtUnsavedChangesChanged(ASCIIArt art, bool unsavedChanges)
            => UpdateTitle();

        private void OnArtCanvasViewModelArtChanged(ASCIIArt art)
            => UpdateTitle();

        private void OnArtCanvasViewModelArtCropped(ASCIIArt art)
            => UpdateTitle();

        private void OnArtCanvasViewModelArtCopiedPropertiesOf(object obj)
            => UpdateTitle();

        #endregion

        #region Background Run Worker Complete Functions
        void BackgroundSaveComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            currentBackgroundWorker = null;
            artCanvasViewModel.CanUseTool = true;

            if (args.Cancelled)
            {
                Console.WriteLine("Save File: Art file save cancelled!");
                MessageBox.Show("Cancelled saving art file!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (args.Error != null)
            {
                Console.WriteLine("Save File: An error has occurred while saving art file! Exception: " + args.Error.Message);
                MessageBox.Show("An error has occurred while saving art file!\nException: " + args.Error.Message, "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Save Art File did not return file info!");

                MessageBox.Show("Saved art file to " + fileInfo.FullName + "!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void BackgroundExportComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            currentBackgroundWorker = null;
            artCanvasViewModel.CanUseTool = true;

            if (args.Cancelled)
            {
                Console.WriteLine("Export File: Art file export cancelled!");
                MessageBox.Show("Cancelled exporting art file!", "Export File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (args.Error != null)
            {
                Console.WriteLine("Export File: An error has occurred while exporting art file! Exception: " + args.Error.Message);
                MessageBox.Show("An error has occurred while exporting art file!\nException: " + args.Error.Message, "Export File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Export Art File did not return file info!");

                MessageBox.Show("Exported art file to " + fileInfo.FullName + "!", "Export File", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start("explorer.exe", fileInfo.DirectoryName ?? App.DefaultArtFilesDirectoryPath);
            }
        }
        #endregion

        #region Menu Item Actions

        private void NewFileAction()
        {
            NewASCIIArtDialogWindow newASCIIArtDialogWindow = new();
            newASCIIArtDialogWindow.ShowDialog();

            if (newASCIIArtDialogWindow.CreatedArt == null)
                return;

            App.NewFile(newASCIIArtDialogWindow.CreatedArt);
        }

        private void OpenFileAction()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
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
                Exception? ex = App.OpenFile(new(openFileDialog.FileName));

                if (ex != null)
                    MessageBox.Show($"An error has occurred while opening art file ({openFileDialog.FileName})! Exception: {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFileAction()
        {
            if (App.CurrentArt == null)
                return;

            if (currentBackgroundWorker != null)
                return;

            string? savePath = App.CurrentFilePath;

            if (savePath == null)
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new()
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
                    return;
            }

            currentBackgroundWorker = App.SaveArtFileToPathAsync(savePath);

            if (currentBackgroundWorker == null)
                return;

            artCanvasViewModel.CanUseTool = false;

            BackgroundTaskWindow backgroundTaskWindow = new(currentBackgroundWorker, $"Saving to {new FileInfo(savePath).Name}");
            backgroundTaskWindow.Show();
            backgroundTaskWindow.Owner = this;

            currentBackgroundWorker.RunWorkerCompleted += BackgroundSaveComplete;
        }

        private void SaveAsFileAction()
        {
            if (App.CurrentArt == null)
                return;

            if (currentBackgroundWorker != null)
                return;

            Microsoft.Win32.SaveFileDialog saveFileDialog = new()
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
                return;

            currentBackgroundWorker = App.SaveArtFileToPathAsync(savePath);

            if (currentBackgroundWorker == null)
                return;

            artCanvasViewModel.CanUseTool = false;

            BackgroundTaskWindow backgroundTaskWindow = new(currentBackgroundWorker, $"Saving to {new FileInfo(savePath).Name}");
            backgroundTaskWindow.Show();
            backgroundTaskWindow.Owner = this;

            currentBackgroundWorker.RunWorkerCompleted += BackgroundSaveComplete;
        }

        private void ExportAction()
        {
            if (App.CurrentArt == null)
                return;

            if (currentBackgroundWorker != null)
                return;

            Microsoft.Win32.SaveFileDialog saveFileDialog = new()
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
                return;

            currentBackgroundWorker = App.ExportArtFileToPathAsync(savePath);

            if (currentBackgroundWorker == null)
                return;

            artCanvasViewModel.CanUseTool = false;

            BackgroundTaskWindow backgroundTaskWindow = new(currentBackgroundWorker, $"Exporting to {new FileInfo(savePath).Name}");
            backgroundTaskWindow.Show();
            backgroundTaskWindow.Owner = this;

            currentBackgroundWorker.RunWorkerCompleted += BackgroundExportComplete;
        }

        private void CopyArtToClipboardAction()
        {
            if (App.CurrentArt == null)
                return;

            App.CopyArtFileToClipboard();
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
        private void OnCharacterPaletteSelectionViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not CharacterPaletteSelectionViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case "SelectedCharacter":
                    App.SelectCharacterDrawTool(vm.SelectedCharacter);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
