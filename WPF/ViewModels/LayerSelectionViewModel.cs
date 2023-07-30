using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.UI.ViewModels
{
    public class LayerSelectionViewModel : INotifyPropertyChanged
    {
        private ASCIIArt? art = null;
        public ASCIIArt? Art
        {
            get => art;
            set
            {
                if (art == value)
                    return;

                art = value;
                Layers = art != null ? art.ArtLayers : new();
                PropertyChanged?.Invoke(this, new(nameof(Art)));
            }
        }

        private List<ArtLayer> layers = new();
        public List<ArtLayer> Layers
        {
            get => layers;
            private set
            {
                if (layers == value)
                    return;

                layers = value;
                selectedLayerID = -1;
                PropertyChanged?.Invoke(this, new(nameof(Layers)));
            }
        }
        
        private int selectedLayerID = -1;
        public int SelectedLayerID
        {
            get => selectedLayerID;
            set
            {
                if (selectedLayerID == value)
                    return;

                selectedLayerID = value;
                SelectedLayer = value != -1 ? Layers[value] : null;
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
                }

                selectedLayer = value;

                HasSelectedLayer = value != null;
                if (value != null)
                {
                    SelectedLayerName = value.Name;
                    SelectedLayerVisibility = value.Visible;

                    value.NameChanged += LayerNameChanged;
                    value.VisibilityChanged += LayerVisibilityChanged;
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

                selectedLayerName = value;
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

                PropertyChanged?.Invoke(this, new(nameof(SelectedLayerVisibility)));
            }
        }

        private Visibility layerOptionsVisibility = Visibility.Hidden;
        public Visibility LayerOptionsVisibility
        {
            get => layerOptionsVisibility;
            private set
            {
                if (layerOptionsVisibility == value)
                    return;

                layerOptionsVisibility = value;
                PropertyChanged?.Invoke(this, new(nameof(LayerOptionsVisibility)));
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

                LayerOptionsVisibility = value ? Visibility.Visible : Visibility.Hidden;

                PropertyChanged?.Invoke(this, new(nameof(HasSelectedLayer)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void LayerNameChanged(ArtLayer layer, string name)
            => SelectedLayerName = name;

        private void LayerVisibilityChanged(ArtLayer layer, bool visible)
            => SelectedLayerVisibility = visible;
    }
}
