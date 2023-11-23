using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using AAP.BackgroundTasks;
using AAP.Files;
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
        private bool willCloseSoon = false;
        
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

            ArtFileViewModel.OpenArtFiles = App.OpenArtFiles;
            ArtFileViewModel.CurrentTool = App.CurrentTool;
            ToolOptionsViewModel.CharacterPaletteSelectionViewModel = CharacterPaletteSelectionViewModel;

            CharacterPaletteSelectionViewModel.Palettes = App.CharacterPalettes;

            App.Settings.PropertyChanged += SettingsPropertyChanged;

            ArtCanvasViewModel.CanvasTypeface = new(App.Settings.CanvasTypefaceSource);
            ArtCanvasViewModel.ShowToolPreviews = App.Settings.ToolPreviews;

            OnCurrentArtFileChanged(App.CurrentArtFile);

            UpdateTitle();

            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Draw, DrawToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Eraser, EraserToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Select, SelectToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Move, MoveToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Bucket, BucketToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Text, TextToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Line, LineToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Rectangle, RectangleToolStateBox);
            ToolSelectionViewModel.ToolStateBoxes.Add(ToolType.Ellipse, EllipseToolStateBox);

            Closing += OnClosing;

            App.OnLanguageChanged += (language) => UpdateTitle();

            #region Shortcut Commands

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, new((sender, e) => ArtFileViewModel.NewFile())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new(async (sender, e) => await ArtFileViewModel.OpenFileAsync())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new(async (sender, e) => { if (App.CurrentArtFile != null) await ArtFileViewModel.SaveFileAsync(App.CurrentArtFile); })));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.SaveAsShortcut, new(async (sender, e) => { if (App.CurrentArtFile != null) await ArtFileViewModel.SaveAsFileAsync(App.CurrentArtFile); })));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.ImportFileShortcut, new(async (sender, e) => { if (App.CurrentArtFile != null) await ArtFileViewModel.ImportFileAsync(); })));

            CommandBindings.Add(new CommandBinding(FileShortcutCommands.ExportAsShortcut, new(async (sender, e) => { if (App.CurrentArtFile != null) await ArtFileViewModel.ExportFileAsync(App.CurrentArtFile); })));
            CommandBindings.Add(new CommandBinding(FileShortcutCommands.CopyToClipboardShortcut, new((sender, e) => ArtFileViewModel.CopyCurrentArtToClipboard())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new((sender, e) => Application.Current.Shutdown())));

            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.EnlargeTextSizeShortcut, new((sender, e) => ArtCanvasViewModel.EnlargeTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ShrinkTextSizeShortcut, new((sender, e) => ArtCanvasViewModel.ShrinkTextSize())));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.ResetTextSizeShortcut, new((sender, e) => ArtCanvasViewModel.ResetTextSize())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, new((sender, e) => App.CurrentArtFile?.ArtTimeline.Rollback())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, new((sender, e) => App.CurrentArtFile?.ArtTimeline.Rollforward())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, new((sender, e) => App.CutSelectedArt())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new((sender, e) => App.CopySelectedArtToClipboard())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, new((sender, e) => App.PasteLayerFromClipboard())));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, new((sender, e) => App.DeleteSelection())));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, new((sender, e) => App.SelectCanvas())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.SelectLayerShortcut, new((sender, e) => App.SelectLayer())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CancelSelectionShortcut, new((sender, e) => App.CancelArtSelection())));

            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CropArtShortcut, new((sender, e) => App.CropArtFileToSelected())));
            CommandBindings.Add(new CommandBinding(EditShortcutCommands.CropLayerShortcut, new((sender, e) => App.CropCurrentArtLayerToSelected())));

            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.DrawToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Draw))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.EraserToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Eraser))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.SelectToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Select))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.MoveToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Move))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.LineToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Line))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.BucketToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Bucket))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.TextToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Text))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.RectangleToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Rectangle))));
            CommandBindings.Add(new CommandBinding(CanvasShortcutCommands.EllipseToolShortCut, new((sender, e) => App.SelectToolType(ToolType.Ellipse))));
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
            if (willCloseSoon)
            {
                e.Cancel = true;
                return;
            }    

            foreach (ASCIIArtFile artFile in App.OpenArtFiles)
                if (artFile.UnsavedChanges)
                {
                    MessageBoxResult result = MessageBox.Show(App.Language.GetString("Application_CloseWithoutSavingWarningMessage"), App.ProgramTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }

                    break;
                }

            BackgroundTaskToken? taskToken = MainWindowViewModel.CurrentBackgroundTaskToken;

            if (taskToken != null)
            {
                if (taskToken.MainTask != null)
                {
                    if (!taskToken.MainTask.IsCompleted)
                    {
                        MessageBox.Show(this, App.Language.GetString("Application_AutoCloseMessage"), App.ProgramTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                        if (MainWindowViewModel.CurrentBackgroundTaskToken != null) //If task hasn't finished after message box
                        {
                            willCloseSoon = true;
                            e.Cancel = true;
                            taskToken.Completed += (token, ex) =>
                            {
                                willCloseSoon = false;

                                if (ex == null)
                                    Close();
                                else
                                    MessageBox.Show(string.Format(App.Language.GetString("Application_CloseBackgroundTaskErrorMessage"), ex.Message), App.ProgramTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                            };
                        }
                        else if (taskToken.Exception != null)
                        {
                            e.Cancel = true;
                            MessageBox.Show(string.Format(App.Language.GetString("Application_CloseBackgroundTaskErrorMessage"), taskToken.Exception.Message), App.ProgramTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                    }
                }
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

            if (sender is not AppSettings settings)
                return;

            switch (e.PropertyName)
            {
                case nameof(settings.CanvasTypefaceSource):
                    ArtCanvasViewModel.CanvasTypeface = new(settings.CanvasTypefaceSource);
                    break;
                case nameof(settings.ToolPreviews):
                    ArtCanvasViewModel.ShowToolPreviews = settings.ToolPreviews;
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
                    App.Settings.DarkMode = vm.IsDarkModeOn;
                    App.SaveSettings();
                    break;
                case nameof(vm.CurrentBackgroundTaskToken):
                    ArtFileViewModel.CurrentBackgroundTaskToken = vm.CurrentBackgroundTaskToken;
                    CharacterPaletteSelectionViewModel.CurrentBackgroundTaskToken = vm.CurrentBackgroundTaskToken;
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
                    LayerManagementViewModel.ArtFile = vm.CurrentArtFile;
                    break;
                case nameof(vm.CurrentTool):
                    ArtCanvasViewModel.CurrentTool = vm.CurrentTool;
                    ToolOptionsViewModel.Tool = vm.CurrentTool;
                    break;
                case nameof(vm.CanUseTool):
                    ArtCanvasViewModel.CanUseTool = vm.CanUseTool;
                    break;
                case nameof(vm.CurrentBackgroundTaskToken):
                    MainWindowViewModel.CurrentBackgroundTaskToken = vm.CurrentBackgroundTaskToken;
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
                case nameof(vm.CurrentBackgroundTaskToken):
                    MainWindowViewModel.CurrentBackgroundTaskToken = vm.CurrentBackgroundTaskToken;
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
