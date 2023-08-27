using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using AAP.BackgroundTasks;
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
            App.OnAvailableCharacterPalettesChanged += (palettes) => CharacterPaletteSelectionViewModel.Palettes = palettes;

            MainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;
            ArtFileViewModel.PropertyChanged += ArtFileViewModelPropertyChanged;
            ArtCanvasViewModel.PropertyChanged += ArtCanvasViewModelPropertyChanged;
            CharacterPaletteSelectionViewModel.PropertyChanged += CharacterPaletteSelectionViewModelPropertyChanged;
            LayerManagementViewModel.PropertyChanged += LayerSelectionViewModelPropertyChanged;

            ArtFileViewModel.CurrentTool = App.CurrentTool;
            ToolOptionsViewModel.CharacterPaletteSelectionViewModel = CharacterPaletteSelectionViewModel;

            CharacterPaletteSelectionViewModel.Palettes = App.CharacterPalettes;

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
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new((sender, e) => ArtFileViewModel.OpenFile())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new((sender, e) => ArtFileViewModel.SaveFile())));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.SaveAsShortcut, new((sender, e) => ArtFileViewModel.SaveAsFile())));

            CommandBindings.Add(new CommandBinding(FileShortcutCommands.ExportAsShortcut, new((sender, e) => ArtFileViewModel.ExportFile())));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.CopyToClipboardShortcut, new((sender, e) => ArtFileViewModel.CopyArtToClipboard())));

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

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (App.CurrentArtFile == null)
                return;

            if (ArtFileViewModel.CurrentBackgroundTask != null)
            {
                e.Cancel = true;
                
                ArtFileViewModel.CurrentBackgroundTask.Worker.RunWorkerCompleted += (sender, e) => Close();

                BackgroundTaskWindow backgroundTaskWindow = new(new("Waiting for remaining task to finish...", ArtFileViewModel.CurrentBackgroundTask.Worker), true);
                backgroundTaskWindow.Show();
                backgroundTaskWindow.Owner = this;
                
                return;
            }

            if (App.CurrentArtFile.UnsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show("You've made some changes that haven't been saved.\nWould you like to save?", "Save ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {

                    ArtFileViewModel.SaveFile();

                    if (ArtFileViewModel.CurrentBackgroundTask == null)
                        return;

                    ArtFileViewModel.CurrentBackgroundTask.Worker.RunWorkerCompleted += (sender, e) => Close();
                    e.Cancel = true;

                    return;
                }
            }

        }

        #region App Events
        private void OnCurrentArtFileChanged(ASCIIArtFile? artFile)
        {
            if (artFile != null)
            {
                int fullArtArea = artFile.Art.GetTotalArtArea();

                if (fullArtArea < artFile.Art.Width * artFile.Art.Height)
                    fullArtArea = artFile.Art.Width * artFile.Art.Height;

                if (fullArtArea >= App.WarningLargeArtArea)
                {
                    string message = $"The ASCII Art you're trying to open has an total art area of {fullArtArea} (Area * ArtLayers) characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters.\nThis might take a long time to load and save, and can be performance heavy.\nAre you sure you want to continue?";

                    if (fullArtArea >= App.WarningIncrediblyLargeArtArea)
                        message = $"The ASCII Art you're trying to open has an total art area of {fullArtArea} (Area * ArtLayers) characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters and above the less recommended area limit of {App.WarningIncrediblyLargeArtArea} characters.\nThis might take a VERY long time to load and save, and can be INCREDIBLY performance heavy.\nAre you SURE you want to continue?";

                    MessageBoxResult result = MessageBox.Show(message, "ASCII Art Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.No)
                    {
                        App.SetArtAsNewFile(null);
                        return;
                    }
                }
            }

            //Remove old listeners
            if (ArtFileViewModel.CurrentArtFile != null)
            {
                ArtFileViewModel.CurrentArtFile.OnUnsavedChangesChanged -= OnArtFileViewModelArtUnsavedChangesChanged;
                ArtFileViewModel.CurrentArtFile.Art.OnSizeChanged -= OnArtFileViewModelArtSizeChanged;
                ArtFileViewModel.CurrentArtFile.OnSavePathChanged -= OnSavePathChanged;
            }

            ArtFileViewModel.CurrentArtFile = artFile;
            
            ArtCanvasViewModel.CurrentArtDraw = artFile?.ArtDraw;

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
        private void MainWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (sender is not MainWindowViewModel vm)
                return;

            switch (e.PropertyName)
            {
                case nameof(vm.IsDarkModeOn):
                    App.AppTheme = vm.IsDarkModeOn ? Theme.Dark : Theme.Light;
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
                    ArtCanvasViewModel.CurrentArt = vm.CurrentArtFile?.Art;
                    ArtCanvasViewModel.CurrentArtDraw = vm.CurrentArtFile?.ArtDraw;
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
