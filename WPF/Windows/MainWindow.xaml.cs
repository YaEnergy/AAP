using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using AAP.BackgroundTasks;
using AAP.Properties;
using AAP.Timelines;
using AAP.UI.Themes;
using AAP.UI.ViewModels;
using Microsoft.Win32;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            App.OnCurrentArtFileChanged += OnCurrentArtFileChanged;
            App.OnCurrentToolChanged += OnCurrentToolChanged;
            App.OnSelectedArtChanged += OnSelectedArtChanged;
            App.OnCurrentLayerIDChanged += CurrentLayerIDChanged;

            MainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;
            ArtFileViewModel.PropertyChanged += ArtFileViewModelPropertyChanged;
            ArtCanvasViewModel.PropertyChanged += ArtCanvasViewModelPropertyChanged;
            CharacterPaletteSelectionViewModel.PropertyChanged += CharacterPaletteSelectionViewModelPropertyChanged;
            LayerManagementViewModel.PropertyChanged += LayerSelectionViewModelPropertyChanged;

            ArtFileViewModel.OpenArtFiles = App.OpenArtFiles;
            ArtFileViewModel.CurrentTool = App.CurrentTool;
            ToolOptionsViewModel.CharacterPaletteSelectionViewModel = CharacterPaletteSelectionViewModel;

            CharacterPaletteSelectionViewModel.Palettes = App.CharacterPalettes;

            Settings.Default.PropertyChanged += SettingsPropertyChanged;

            ArtCanvasViewModel.CanvasTypeface = new System.Windows.Media.Typeface(Settings.Default.CanvasTypefaceSource);

            OnCurrentArtFileChanged(App.CurrentArtFile);

            UpdateTitle();

            ToolSelectionViewModel.ToolStateBoxes.Add(DrawToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(EraserToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(SelectToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(MoveToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(BucketToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(TextToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(LineToolStateBox);

            Closing += OnClosing;

            #region Shortcut Commands

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, new((sender, e) => ArtFileViewModel.NewFile())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new(async (sender, e) => await ArtFileViewModel.OpenFileAsync())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new(async (sender, e) => { if (App.CurrentArtFile != null) await ArtFileViewModel.SaveFileAsync(App.CurrentArtFile); })));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.SaveAsShortcut, new(async (sender, e) => { if (App.CurrentArtFile != null) await ArtFileViewModel.SaveAsFileAsync(App.CurrentArtFile); })));

            CommandBindings.Add(new CommandBinding(FileShortcutCommands.ExportAsShortcut, new(async (sender, e) => { if (App.CurrentArtFile != null) await ArtFileViewModel.ExportFileAsync(App.CurrentArtFile); })));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.CopyToClipboardShortcut, new((sender, e) => ArtFileViewModel.CopyCurrentArtToClipboard())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new((sender, e) => Application.Current.Shutdown())));

            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.EnlargeTextSizeShortcut, new((sender, e) => ArtCanvasViewModel.EnlargeTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ShrinkTextSizeShortcut, new((sender, e) => ArtCanvasViewModel.ShrinkTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ResetTextSizeShortcut, new((sender, e) => ArtCanvasViewModel.ResetTextSize())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, new((sender, e) => App.CurrentArtTimeline?.Rollback())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, new((sender, e) => App.CurrentArtTimeline?.Rollforward())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, new((sender, e) => App.CutSelectedArt())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new((sender, e) => App.CopySelectedArtToClipboard())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, new((sender, e) => App.PasteLayerFromClipboard())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, new((sender, e) => App.FillSelectedWith(null))));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, new((sender, e) => App.SelectArt())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.SelectLayerShortcut, new((sender, e) => App.SelectLayer())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CancelSelectionShortcut, new((sender, e) => App.CancelArtSelection())));

            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CropArtShortcut, new((sender, e) => App.CropArtFileToSelected())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CropLayerShortcut, new((sender, e) => App.CropCurrentArtLayerToSelected())));

            CommandBindings.Add(new CommandBinding(DrawShortcutCommands.FillSelectionShortcut, new((sender, e) => ArtFileViewModel.FillSelection())));
            #endregion
        }
        
        private void UpdateTitle()
        {            
            string fullTitle = App.CurrentArtFile == null ? $"{App.ProgramTitle}" : $"{App.ProgramTitle} - {App.CurrentArtFile}";

            if (fullTitle != Title)
                Title = fullTitle;
        }

        private async void OnClosing(object? sender, CancelEventArgs e)
        {
            foreach (ASCIIArtFile artFile in App.OpenArtFiles)
                if (artFile.UnsavedChanges)
                {
                    MessageBoxResult result = MessageBox.Show("You've made some changes that haven't been saved.\nAre you sure you want to exit? (All changes will be discarded)", "ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        e.Cancel = true;

                    break;
                }

            if (ArtFileViewModel.CurrentBackgroundTask != null)
            {
                BackgroundTaskWindow backgroundTaskWindow = new(ArtFileViewModel.CurrentBackgroundTask, true);
                backgroundTaskWindow.Show();
                backgroundTaskWindow.Owner = this;

                await ArtFileViewModel.CurrentBackgroundTask.MainTask;
            }
        }

        #region App Events
        private void OnCurrentArtFileChanged(ASCIIArtFile? artFile)
        {
            //Remove old listeners
            if (ArtFileViewModel.CurrentArtFile != null)
            {
                ArtFileViewModel.CurrentArtFile.OnUnsavedChangesChanged -= OnArtFileViewModelArtUnsavedChangesChanged;
                ArtFileViewModel.CurrentArtFile.Art.OnSizeChanged -= OnArtFileViewModelArtSizeChanged;
                ArtFileViewModel.CurrentArtFile.OnSavePathChanged -= OnSavePathChanged;
            }

            ArtFileViewModel.CurrentArtFile = artFile;

            //Add new listeners
            if (artFile != null)
            {
                artFile.OnUnsavedChangesChanged += OnArtFileViewModelArtUnsavedChangesChanged;
                artFile.Art.OnSizeChanged += OnArtFileViewModelArtSizeChanged;
                artFile.OnSavePathChanged += OnSavePathChanged;
            }

            UpdateTitle();
        }

        private void OnCurrentToolChanged(Tool? tool)
        {
            ArtFileViewModel.CurrentTool = tool;
        }

        private void OnSelectedArtChanged(Rect selected)
        {
            ArtFileViewModel.HasSelected = selected != Rect.Empty;
            ArtCanvasViewModel.Selected = selected;
        }

        private void CurrentLayerIDChanged(int layerID)
        {
            LayerManagementViewModel.SelectedLayerID = layerID;
            ArtCanvasViewModel.SelectedLayerID = layerID; 
        }

        #endregion

        #region Art Canvas View Model Art Events

        private void OnArtFileViewModelArtUnsavedChangesChanged(bool unsavedChanges)
            => UpdateTitle();

        private void OnArtFileViewModelArtSizeChanged(int width, int height)
            => UpdateTitle();

        private void OnSavePathChanged(string? savePath)
            => UpdateTitle();

        #endregion

        #region ViewModel Property Changed Events
        private void SettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not Settings settings)
                return;

            switch (e.PropertyName)
            {
                case nameof(settings.CanvasTypefaceSource):
                    ArtCanvasViewModel.CanvasTypeface = new(settings.CanvasTypefaceSource);
                    break;
                default:
                    break;
            }
        }

        private void MainWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not MainWindowViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case nameof(vm.IsDarkModeOn):
                    App.DarkMode = vm.IsDarkModeOn;
                    Settings.Default.Save();
                    break;
                case nameof(vm.CurrentBackgroundTask):
                    ArtFileViewModel.CurrentBackgroundTask = vm.CurrentBackgroundTask;
                    CharacterPaletteSelectionViewModel.CurrentBackgroundTask = vm.CurrentBackgroundTask;
                    break;
                default:
                    break;
            }
        }

        private void ArtFileViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not ArtFileViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case nameof(vm.CurrentArtFile):
                    App.CurrentArtFile = vm.CurrentArtFile;
                    ArtCanvasViewModel.CurrentArt = vm.CurrentArtFile?.Art;
                    LayerManagementViewModel.Art = vm.CurrentArtFile?.Art;
                    break;
                case nameof(vm.CurrentTool):
                    ArtCanvasViewModel.CurrentTool = vm.CurrentTool;
                    ToolOptionsViewModel.Tool = vm.CurrentTool;
                    break;
                case nameof(vm.CanUseTool):
                    ArtCanvasViewModel.CanUseTool = vm.CanUseTool;
                    break;
                case nameof(vm.CurrentBackgroundTask):
                    MainWindowViewModel.CurrentBackgroundTask = vm.CurrentBackgroundTask;
                    break;
                default:
                    break;
            }
        }

        private void ArtCanvasViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not ArtCanvasViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case nameof(vm.CanUseTool):
                    ArtFileViewModel.CanUseTool = vm.CanUseTool;
                    break;
                default:
                    break;
            }
        }

        private void CharacterPaletteSelectionViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not CharacterPaletteSelectionViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case nameof(vm.SelectedCharacter):
                    App.SelectCharacterTool(vm.SelectedCharacter);
                    break;
                case nameof(vm.CurrentBackgroundTask):
                    MainWindowViewModel.CurrentBackgroundTask = vm.CurrentBackgroundTask;
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
                case nameof(vm.SelectedLayerID):
                    App.CurrentLayerID = vm.SelectedLayerID;
                    break;
                case nameof(vm.SelectedLayerName):
                    if (vm.SelectedLayer == null)
                        break;

                    if (vm.SelectedLayer.Name == vm.SelectedLayerName)
                        break;

                    App.SetArtLayerName(vm.SelectedLayer, vm.SelectedLayerName);
                    break;
                case nameof(vm.SelectedLayerVisibility):
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
