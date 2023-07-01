using System;
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

namespace AAP
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

            artCanvas.Tool = App.CurrentTool;

            UpdateTitle();

            #region Shortcut Commands

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, new((sender, e) => NewFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new((sender, e) => OpenFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new((sender, e) => SaveFileAction())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, new((sender, e) => SaveAsFileAction())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new((sender, e) => System.Windows.Application.Current.Shutdown())));

            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.EnlargeTextSizeShortcut, new((sender, e) => artCanvasViewModel.EnlargeTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ShrinkTextSizeShortcut, new((sender, e) => artCanvasViewModel.ShrinkTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ResetTextSizeShortcut, new((sender, e) => artCanvasViewModel.ResetTextSize())));

            #endregion
        }

        private void UpdateTitle()
            => Title = $"{App.ProgramTitle} - {( App.CurrentFilePath == null ? "*.*" : new FileInfo(App.CurrentFilePath).Name)} - {(App.CurrentArt != null ? App.CurrentArt.Width.ToString() : "*")}x{(App.CurrentArt != null ? App.CurrentArt.Height.ToString() : "*")}";

        #region App Events

        private void OnCurrentArtChanged(ASCIIArt? art, ASCIIArtDraw? artDraw)
        {
            if (art != null)
                if (art.Width * art.Height >= App.WarningLargeArtArea)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show($"The ASCII Art you're trying to open has an art area of {art.Width * art.Height} characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters.\nThis might take a long time to load and save.\nAre you sure you want to continue?", "ASCII Art Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.No)
                    {
                        App.NewFile(null);
                        return;
                    }
                }
                else if (art.ArtLayers.Count >= App.WarningManyArtLayers)
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show($"The ASCII Art you're trying to open has {art.ArtLayers.Count} art layers. This is above the recommended art layer limit of {App.WarningManyArtLayers} characters.\nThis might take a long time to load and save.\nAre you sure you want to continue?", "ASCII Art Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        App.NewFile(null);
                        return;
                    }
                }

            artCanvasViewModel.CurrentArt = art; 
            artCanvasViewModel.CurrentArtDraw = artDraw;

            UpdateTitle();
        }

        private void OnCurrentFilePathChanged(string? filePath)
        {
            UpdateTitle();
        }

        private void OnCurrentToolChanged(Tool tool)
        {
            artCanvasViewModel.CurrentTool = tool;
        }

        private void OnSelectionChanged(Rect selected)
        {
            artCanvasViewModel.Selected = selected;
        }
        #endregion

        #region Background Run Worker Complete Functions
        void BackgroundSaveComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            currentBackgroundWorker = null;

            if (args.Cancelled)
            {
                Console.WriteLine("Save File: Art file save cancelled!");
                System.Windows.MessageBox.Show("Cancelled saving art file!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (args.Error != null)
            {
                Console.WriteLine("Save File: An error has occurred while saving art file! Exception: " + args.Error.Message);
                System.Windows.MessageBox.Show("An error has occurred while saving art file!\nException: " + args.Error.Message, "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Save Art File did not return file info!");

                System.Windows.MessageBox.Show("Saved art file to " + fileInfo.FullName + "!", "Save File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void BackgroundExportComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            currentBackgroundWorker = null;

            if (args.Cancelled)
            {
                Console.WriteLine("Export File: Art file export cancelled!");
                System.Windows.MessageBox.Show("Cancelled exporting art file!", "Export File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (args.Error != null)
            {
                Console.WriteLine("Export File: An error has occurred while exporting art file! Exception: " + args.Error.Message);
                System.Windows.MessageBox.Show("An error has occurred while exporting art file!\nException: " + args.Error.Message, "Export File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Export Art File did not return file info!");

                System.Windows.MessageBox.Show("Exported art file to " + fileInfo.FullName + "!", "Export File", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start("explorer.exe", fileInfo.DirectoryName ?? MainProgram.DefaultArtFilesDirectoryPath);
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
                    System.Windows.MessageBox.Show($"An error has occurred while opening art file ({openFileDialog.FileName})! Exception: {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
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

            BackgroundTaskWindow backgroundTaskWindow = new(currentBackgroundWorker, $"Saving to {new FileInfo(savePath).Name}");
            backgroundTaskWindow.Show();
            backgroundTaskWindow.Owner = this;

            currentBackgroundWorker.RunWorkerCompleted += BackgroundSaveComplete;
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
            => System.Windows.Application.Current.Shutdown();

        #endregion
    }
}
