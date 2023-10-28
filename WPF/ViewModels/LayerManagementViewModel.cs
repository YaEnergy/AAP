using AAP.FileObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP.UI.ViewModels
{
    public class LayerManagementViewModel : INotifyPropertyChanged
    {
        private ASCIIArtFile? artFile = null;
        public ASCIIArtFile? ArtFile
        {
            get => artFile;
            set
            {
                if (artFile == value)
                    return;

                if (artFile != null)
                {
                    artFile.Art.OnArtLayerAdded -= ArtLayerAdded;
                    artFile.Art.OnArtLayerRemoved -= ArtLayerRemoved;
                }

                artFile = value;

                if (artFile != null)
                {
                    artFile.Art.OnArtLayerAdded += ArtLayerAdded;
                    artFile.Art.OnArtLayerRemoved += ArtLayerRemoved;
                }

                SelectedLayerID = -1;

                PropertyChanged?.Invoke(this, new(nameof(ArtFile)));
            }
        }
        
        private int selectedLayerID = -1;
        public int SelectedLayerID
        {
            get => selectedLayerID;
            set
            {
                SelectedLayer = value != -1 && ArtFile != null ? ArtFile.Art.ArtLayers[value] : null;

                if (selectedLayerID == value)
                    return;

                selectedLayerID = value;

                App.CurrentLayerID = selectedLayerID;

                PropertyChanged?.Invoke(this, new(nameof(SelectedLayerID)));
            }
        }

        private ArtLayer? selectedLayer = null;
        public ArtLayer? SelectedLayer
        {
            get => selectedLayer;
            private set
            {
                if (selectedLayer == value)
                    return;

                if (selectedLayer != null)
                {
                    selectedLayer.NameChanged -= LayerNameChanged;
                    selectedLayer.VisibilityChanged -= LayerVisibilityChanged;
                    selectedLayer.DataChanged -= LayerDataChanged;
                    selectedLayer.OffsetChanged -= LayerOffsetChanged;
                }

                selectedLayer = value;

                HasSelectedLayer = value != null;
                if (value != null)
                {
                    SelectedLayerName = value.Name;
                    SelectedLayerVisibility = value.Visible;
                    LayerOffsetX = value.OffsetX;
                    LayerOffsetY = value.OffsetY;
                    LayerSizeX = value.Width;
                    LayerSizeY = value.Height;

                    value.NameChanged += LayerNameChanged;
                    value.VisibilityChanged += LayerVisibilityChanged;
                    value.DataChanged += LayerDataChanged;
                    value.OffsetChanged += LayerOffsetChanged;
                }

                PropertyChanged?.Invoke(this, new(nameof(SelectedLayer)));
            }
        }

        private string selectedLayerName = "";
        public string SelectedLayerName
        {
            get => selectedLayerName;
            set
            {
                if (SelectedLayer == null)
                    return;

                if (selectedLayerName == value)
                    return;

                if (string.IsNullOrEmpty(value))
                {
                    PropertyChanged?.Invoke(this, new(nameof(SelectedLayerName)));
                    return;
                }

                selectedLayerName = value;

                if (selectedLayer != null && selectedLayer.Name != selectedLayerName)
                {
                    selectedLayer.Name = selectedLayerName;
                    ArtFile?.ArtTimeline.NewTimePoint();
                }

                PropertyChanged?.Invoke(this, new(nameof(SelectedLayerName)));
            }
        }

        private bool selectedLayerVisibility = true;
        public bool SelectedLayerVisibility
        {
            get => selectedLayerVisibility;
            set
            {
                if (SelectedLayer == null)
                    return;

                if (selectedLayerVisibility == value)
                    return;

                selectedLayerVisibility = value;

                if (selectedLayer != null && selectedLayer.Visible != selectedLayerVisibility)
                {
                    selectedLayer.Visible = selectedLayerVisibility;
                    ArtFile?.ArtTimeline.NewTimePoint();
                    ArtFile?.Art.Update();
                }

                PropertyChanged?.Invoke(this, new(nameof(SelectedLayerVisibility)));
            }
        }

        private bool hasSelectedLayer = false;
        public bool HasSelectedLayer
        {
            get => hasSelectedLayer;
            private set
            {
                if (hasSelectedLayer == value)
                    return;

                hasSelectedLayer = value;

                PropertyChanged?.Invoke(this, new(nameof(HasSelectedLayer)));
            }
        }

        private string layerOffsetXText = "-1";
        public string LayerOffsetXText
        {
            get => layerOffsetXText;
            set
            {
                if (layerOffsetXText == value)
                    return;

                if (!int.TryParse(value, out int x))
                {
                    PropertyChanged?.Invoke(this, new(nameof(LayerOffsetXText)));
                    return;
                }

                layerOffsetXText = x.ToString();
                LayerOffsetX = x;

                PropertyChanged?.Invoke(this, new(nameof(LayerOffsetXText)));
            }
        }

        private int layerOffsetX = -1;
        public int LayerOffsetX
        {
            get => layerOffsetX;
            set
            {
                if (layerOffsetX == value)
                    return;

                layerOffsetX = value;
                LayerOffsetXText = value.ToString();

                if (selectedLayer != null && selectedLayer.OffsetX != layerOffsetX)
                {
                    selectedLayer.OffsetX = layerOffsetX;
                    ArtFile?.ArtTimeline.NewTimePoint();
                    ArtFile?.Art.Update();
                }

                PropertyChanged?.Invoke(this, new(nameof(LayerOffsetX)));
            }
        }

        private string layerOffsetYText = "-1";
        public string LayerOffsetYText
        {
            get => layerOffsetYText;
            set
            {
                if (layerOffsetYText == value)
                    return;

                if (!int.TryParse(value, out int y))
                {
                    PropertyChanged?.Invoke(this, new(nameof(LayerOffsetYText)));
                    return;
                }

                layerOffsetYText = y.ToString();
                LayerOffsetY = y;

                PropertyChanged?.Invoke(this, new(nameof(LayerOffsetYText)));
            }
        }

        private int layerOffsetY = -1;
        public int LayerOffsetY
        {
            get => layerOffsetY;
            set
            {
                if (layerOffsetY == value)
                    return;

                layerOffsetY = value;
                LayerOffsetYText = value.ToString();

                if (selectedLayer != null && selectedLayer.OffsetY != layerOffsetY)
                {
                    selectedLayer.OffsetY = layerOffsetY;
                    ArtFile?.ArtTimeline.NewTimePoint();
                    ArtFile?.Art.Update();
                }

                PropertyChanged?.Invoke(this, new(nameof(LayerOffsetY)));
            }
        }

        private int layerSizeX = -1;
        public int LayerSizeX
        {
            get => layerSizeX;
            private set
            {
                if (layerSizeX == value)
                    return;

                layerSizeX = value;
                PropertyChanged?.Invoke(this, new(nameof(LayerSizeX)));
            }
        }

        private int layerSizeY = -1;
        public int LayerSizeY
        {
            get => layerSizeY;
            private set
            {
                if (layerSizeY == value)
                    return;

                layerSizeY = value;
                PropertyChanged?.Invoke(this, new(nameof(LayerSizeY)));
            }
        }

        public ICommand CreateNewLayerCommand { get; private set; }
        public ICommand MoveLayerUpCommand { get; private set; }
        public ICommand MoveLayerDownCommand { get; private set; }
        public ICommand DuplicateLayerCommand { get; private set; }
        public ICommand MergeLayerCommand { get; private set; }
        public ICommand RemoveLayerCommand { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public LayerManagementViewModel()
        {
            CreateNewLayerCommand = new ActionCommand((parameter) => App.AddArtLayer());
            MoveLayerUpCommand = new ActionCommand((parameter) => App.MoveCurrentArtLayer(-1));
            MoveLayerDownCommand = new ActionCommand((parameter) => App.MoveCurrentArtLayer(1));
            DuplicateLayerCommand = new ActionCommand((parameter) => App.DuplicateCurrentArtLayer());
            MergeLayerCommand = new ActionCommand((parameter) => App.MergeCurrentArtLayerDown());
            RemoveLayerCommand = new ActionCommand((parameter) => App.RemoveCurrentArtLayer());

            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content

        private string layerOptionsContent = App.Language.GetString("LayerOptions");
        public string LayerOptionsContent => layerOptionsContent;

        private string layerNameContent = App.Language.GetString("Name");
        public string LayerNameContent => layerNameContent;

        private string layerVisibilityContent = App.Language.GetString("Visible");
        public string LayerVisibilityContent => layerVisibilityContent;

        private string addLayerTooltip = App.Language.GetString("LayerManagement_Tooltip_AddLayer");
        public string AddLayerTooltip => addLayerTooltip;

        private string moveLayerUpTooltip = App.Language.GetString("LayerManagement_Tooltip_MoveLayerUp");
        public string MoveLayerUpTooltip => moveLayerUpTooltip;

        private string moveLayerDownTooltip = App.Language.GetString("LayerManagement_Tooltip_MoveLayerDown");
        public string MoveLayerDownTooltip => moveLayerDownTooltip;

        private string duplicateLayerTooltip = App.Language.GetString("LayerManagement_Tooltip_DuplicateLayer");
        public string DuplicateLayerTooltip => duplicateLayerTooltip;

        private string mergeLayerDownTooltip = App.Language.GetString("LayerManagement_Tooltip_MergeLayerDown");
        public string MergeLayerDownTooltip => mergeLayerDownTooltip;

        private string removeLayerTooltip = App.Language.GetString("LayerManagement_Tooltip_RemoveLayer");
        public string RemoveLayerTooltip => removeLayerTooltip;

        private string layerOffsetContent = App.Language.GetString("LayerOptions_Offset");
        public string LayerOffsetContent => layerOffsetContent;

        private string layerSizeContent = App.Language.GetString("Size");
        public string LayerSizeContent => layerSizeContent;

        private void OnLanguageChanged(Language language)
        {
            layerOptionsContent = language.GetString("LayerOptions");
            layerNameContent = language.GetString("Name");
            layerVisibilityContent = language.GetString("Visible");

            addLayerTooltip = language.GetString("LayerManagement_Tooltip_AddLayer");
            moveLayerUpTooltip = language.GetString("LayerManagement_Tooltip_MoveLayerUp");
            moveLayerDownTooltip = language.GetString("LayerManagement_Tooltip_MoveLayerDown");
            duplicateLayerTooltip = language.GetString("LayerManagement_Tooltip_DuplicateLayer");
            mergeLayerDownTooltip = language.GetString("LayerManagement_Tooltip_MergeLayerDown");
            removeLayerTooltip = language.GetString("LayerManagement_Tooltip_RemoveLayer");

            layerOffsetContent = language.GetString("LayerOptions_Offset");
            layerSizeContent = language.GetString("Size");

            PropertyChanged?.Invoke(this, new(nameof(LayerOptionsContent)));
            PropertyChanged?.Invoke(this, new(nameof(LayerNameContent)));
            PropertyChanged?.Invoke(this, new(nameof(LayerVisibilityContent)));

            PropertyChanged?.Invoke(this, new(nameof(AddLayerTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(MoveLayerUpTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(MoveLayerDownTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(DuplicateLayerTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(MergeLayerDownTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(RemoveLayerTooltip)));

            PropertyChanged?.Invoke(this, new(nameof(LayerOffsetContent)));
            PropertyChanged?.Invoke(this, new(nameof(LayerSizeContent)));
        }
        #endregion

        private void LayerNameChanged(ArtLayer layer, string name)
            => SelectedLayerName = name;

        private void LayerVisibilityChanged(ArtLayer layer, bool visible)
            => SelectedLayerVisibility = visible;

        private void LayerOffsetChanged(ArtLayer layer, Point oldOffset, Point newOffset)
        {
            LayerOffsetX = (int)newOffset.X;
            LayerOffsetY = (int)newOffset.Y;
        }

        private void LayerDataChanged(ArtLayer layer, char?[][] oldData, char?[][] newData)
        {
            LayerSizeX = layer.Width;
            LayerSizeY = layer.Height;
        }

        private void ArtLayerAdded(int index, ArtLayer layer)
        {
            if (ArtFile == null)
                throw new NullReferenceException(nameof(ArtFile) + " is null!");

            SelectedLayer = SelectedLayerID != -1 ? ArtFile.Art.ArtLayers[Math.Clamp(SelectedLayerID, 0, ArtFile.Art.ArtLayers.Count - 1)] : null;
        }

        private void ArtLayerRemoved(int index, ArtLayer layer)
        {
            if (ArtFile == null)
                throw new NullReferenceException(nameof(ArtFile) + " is null!");

            SelectedLayer = SelectedLayerID != -1 ? ArtFile.Art.ArtLayers[Math.Clamp(SelectedLayerID, 0, ArtFile.Art.ArtLayers.Count - 1)] : null;
        }
    }
}
