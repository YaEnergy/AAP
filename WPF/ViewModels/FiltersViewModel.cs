using AAP.FileObjects;
using AAP.Filters;
using AAP.UI.Windows;
using System.ComponentModel;
using System.Windows.Input;

namespace AAP.UI.ViewModels
{
    public class FiltersViewModel : INotifyPropertyChanged
    {
        private ASCIIArtFile? currentArtFile = null;
        public ASCIIArtFile? CurrentArtFile
        {
            get => currentArtFile;
            set
            {
                if (currentArtFile == value)
                    return;

                currentArtFile = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentArtFile)));
            }
        }

        private bool canUseTool;
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
        public ICommand MirrorFilterCommand { get; set; }
        public ICommand OutlineFilterCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FiltersViewModel()
        {
            MirrorFilterCommand = new ActionCommand((parameter) => ApplyMirrorFilter());
            OutlineFilterCommand = new ActionCommand((parameter) => ApplyOutlineFilter());

            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content

        private string filtersMenuContent = App.Language.GetString("FiltersMenu");
        public string FiltersMenuContent => filtersMenuContent;

        private string mirrorFilterContent = App.Language.GetString("Filters_Mirror");
        public string MirrorFilterContent => mirrorFilterContent;

        private string outlineFilterContent = App.Language.GetString("Filters_Outline");
        public string OutlineFilterContent => outlineFilterContent;

        private string applyFilterContent = App.Language.GetString("Filter_Apply");
        public string ApplyFilterContent => applyFilterContent;

        private void OnLanguageChanged(Language language)
        {
            filtersMenuContent = language.GetString("FiltersMenu");
            mirrorFilterContent = language.GetString("Filters_Mirror");
            outlineFilterContent = language.GetString("Filters_Outline");

            applyFilterContent = language.GetString("Filter_Apply");

            PropertyChanged?.Invoke(this, new(nameof(FiltersMenuContent)));
            PropertyChanged?.Invoke(this, new(nameof(MirrorFilterContent)));
            PropertyChanged?.Invoke(this, new(nameof(OutlineFilterContent)));

            PropertyChanged?.Invoke(this, new(nameof(ApplyFilterContent)));
        }
        #endregion

        public void ApplyMirrorFilter()
        {
            if (App.CurrentArtFile == null)
                return;

            string affectPropertyContent = App.Language.GetString("Filters_Main_Affect");
            string affectPropertyTooltip = App.Language.GetString("Filters_Main_Affect_Tooltip");

            string mirrorAxisPropertyContent = App.Language.GetString("Filters_Main_Axis");
            string mirrorAxisPropertyTooltip = App.Language.GetString("Filters_Mirror_Axis_Tooltip");

            string affectSelectionOption = App.Language.GetString("Filters_Main_Affect_Selection");
            string affectLayerOption = App.Language.GetString("Filters_Main_Affect_Layer");
            string affectCanvasOption = App.Language.GetString("Filters_Main_Affect_Canvas");

            string invalidPropertyNameErrorMessage = App.Language.GetString("Error_DefaultInvalidPropertyMessage");
            string selectionOutsideErrorMessage = App.Language.GetString("Filters_Main_SelectionOutsideMessage");

            List<string> affectOptions = new();

            if (App.SelectedArt != Rect.Empty)
                affectOptions.Add(affectSelectionOption);

            if (App.CurrentLayerID != -1)
                affectOptions.Add(affectLayerOption);

            if (App.CurrentArtFile.Art.ArtLayers.Count != 0)
                affectOptions.Add(affectCanvasOption);

            if (affectOptions.Count == 0)
            {
                MessageBox.Show(App.Language.GetString("Filters_Main_UnavailableMessage"), MirrorFilterContent, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Filter? filter = null;

            while (filter == null)
            {
                PropertiesWindow artWindow = new(MirrorFilterContent, ApplyFilterContent);

                // Property additions
                artWindow.AddProperty(affectPropertyContent, affectPropertyTooltip, artWindow.CreateComboBoxListProperty("Affect", affectOptions, 0));

                artWindow.AddProperty(mirrorAxisPropertyContent, mirrorAxisPropertyTooltip, artWindow.CreateComboBoxEnumProperty("MirrorAxis", Axis2D.X));

                artWindow.AddLabel(App.Language.GetString("Filters_Mirror_Description"));

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return;

                // Property errors

                if (artWindow.GetProperty("Affect") is not string affectString)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, affectPropertyContent), MirrorFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("MirrorAxis") is not Axis2D mirrorAxis)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, mirrorAxisPropertyContent), MirrorFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                // Create Filter
                if (affectString == affectSelectionOption)
                {
                    ASCIIArt art = App.CurrentArtFile.Art;
                    ArtLayer layer = art.ArtLayers[App.CurrentLayerID];

                    int left = Math.Max((int)App.SelectedArt.Left - layer.OffsetX, 0);
                    int top = Math.Max((int)App.SelectedArt.Top - layer.OffsetY, 0);
                    int right = Math.Min((int)App.SelectedArt.Right - layer.OffsetX, layer.Width);
                    int bottom = Math.Min((int)App.SelectedArt.Bottom - layer.OffsetY, layer.Height);

                    //If affect rect is outside layer, do nothing.
                    if (left >= layer.Width || top >= layer.Height || right < 0 || bottom < 0)
                    {
                        MessageBox.Show(selectionOutsideErrorMessage, MirrorFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (right - left <= 0 || bottom - top <= 0)
                    {
                        MessageBox.Show(selectionOutsideErrorMessage, MirrorFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Rect affectRect = new(left, top, right - left, bottom - top);
                    App.SelectedArt = new(Math.Max(App.SelectedArt.Left, layer.OffsetX), Math.Max(App.SelectedArt.Top, layer.OffsetY), Math.Min(App.SelectedArt.Right, layer.OffsetX + layer.Width) - Math.Max(App.SelectedArt.Left, layer.OffsetX), Math.Min(App.SelectedArt.Bottom, layer.OffsetY + layer.Height) - Math.Max(App.SelectedArt.Top, layer.OffsetY));

                    filter = new MirrorFilter(layer, affectRect, mirrorAxis);
                }
                else if (affectString == affectLayerOption)
                    filter = new MirrorFilter(App.CurrentArtFile.Art.ArtLayers[App.CurrentLayerID], mirrorAxis);
                else
                    filter = new MirrorFilter(App.CurrentArtFile.Art, mirrorAxis);
            }

            if (filter == null)
                return;

            filter.Apply();
            App.CurrentArtFile.ArtTimeline.NewTimePoint();
            App.CurrentArtFile.Art.Update();
        }

        public void ApplyOutlineFilter()
        {
            if (App.CurrentArtFile == null)
                return;

            string affectPropertyContent = App.Language.GetString("Filters_Main_Affect");
            string affectPropertyTooltip = App.Language.GetString("Filters_Main_Affect_Tooltip");

            string characterPropertyContent = App.Language.GetString("Filters_Outline_Character");
            string characterPropertyTooltip = App.Language.GetString("Filters_Outline_Character_Tooltip");

            string affectSelectionOption = App.Language.GetString("Filters_Main_Affect_Selection");
            string affectLayerOption = App.Language.GetString("Filters_Main_Affect_Layer");

            string invalidPropertyNameErrorMessage = App.Language.GetString("Error_DefaultInvalidPropertyMessage");
            string selectionOutsideErrorMessage = App.Language.GetString("Filters_Main_SelectionOutsideMessage");

            List<string> affectOptions = new();

            if (App.SelectedArt != Rect.Empty)
                affectOptions.Add(affectSelectionOption);

            if (App.CurrentLayerID != -1)
                affectOptions.Add(affectLayerOption);

            if (affectOptions.Count == 0)
            {
                MessageBox.Show(App.Language.GetString("Filters_Main_UnavailableMessage"), OutlineFilterContent, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Filter? filter = null;

            while (filter == null)
            {
                PropertiesWindow artWindow = new(OutlineFilterContent, ApplyFilterContent);

                // Property additions
                artWindow.AddProperty(affectPropertyContent, affectPropertyTooltip, artWindow.CreateComboBoxListProperty("Affect", affectOptions, 0));

                artWindow.AddProperty(characterPropertyContent, characterPropertyTooltip, artWindow.CreateInputCharProperty("Character", '/'));

                artWindow.AddLabel(App.Language.GetString("Filters_Outline_Description"));

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return;

                // Property errors

                if (artWindow.GetProperty("Affect") is not string affectString)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, affectPropertyContent), OutlineFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (artWindow.GetProperty("Character") is not char character)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, characterPropertyContent), OutlineFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                // Create Filter
                if (affectString == affectSelectionOption)
                {
                    ASCIIArt art = App.CurrentArtFile.Art;
                    ArtLayer layer = art.ArtLayers[App.CurrentLayerID];

                    int left = Math.Max((int)App.SelectedArt.Left - layer.OffsetX, 0);
                    int top = Math.Max((int)App.SelectedArt.Top - layer.OffsetY, 0);
                    int right = Math.Min((int)App.SelectedArt.Right - layer.OffsetX, layer.Width);
                    int bottom = Math.Min((int)App.SelectedArt.Bottom - layer.OffsetY, layer.Height);

                    //If affect rect is outside layer, do nothing.
                    if (left >= layer.Width || top >= layer.Height || right < 0 || bottom < 0)
                    {
                        MessageBox.Show(selectionOutsideErrorMessage, OutlineFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (right - left <= 0 || bottom - top <= 0)
                    {
                        MessageBox.Show(selectionOutsideErrorMessage, OutlineFilterContent, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Rect affectRect = new(left, top, right - left, bottom - top);
                    App.SelectedArt = new(Math.Max(App.SelectedArt.Left, layer.OffsetX), Math.Max(App.SelectedArt.Top, layer.OffsetY), Math.Min(App.SelectedArt.Right, layer.OffsetX + layer.Width) - Math.Max(App.SelectedArt.Left, layer.OffsetX), Math.Min(App.SelectedArt.Bottom, layer.OffsetY + layer.Height) - Math.Max(App.SelectedArt.Top, layer.OffsetY));

                    filter = new OutlineFilter(layer, affectRect, character);
                }
                else if (affectString == affectLayerOption)
                    filter = new OutlineFilter(App.CurrentArtFile.Art.ArtLayers[App.CurrentLayerID], character);
            }

            if (filter == null)
                return;

            filter.Apply();
            App.CurrentArtFile.ArtTimeline.NewTimePoint();
            App.CurrentArtFile.Art.Update();
        }
    }
}
