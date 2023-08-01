using Newtonsoft.Json.Linq;
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

                if (art != null)
                {
                    art.OnArtLayerAdded -= ArtLayerAdded;
                    art.OnArtLayerRemoved -= ArtLayerRemoved;
                }

                art = value;

                if (art != null)
                {
                    art.OnArtLayerAdded += ArtLayerAdded;
                    art.OnArtLayerRemoved += ArtLayerRemoved;
                }

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
                SelectedLayerID = -1;
                PropertyChanged?.Invoke(this, new(nameof(Layers)));
            }
        }
        
        private int selectedLayerID = -1;
        public int SelectedLayerID
        {
            get => selectedLayerID;
            set
            {
                SelectedLayer = value != -1 ? Layers[value] : null;

                if (selectedLayerID == value)
                    return;

                selectedLayerID = value;
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

        private Visibility layerOptionsVisibility = Visibility.Collapsed;
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

                LayerOptionsVisibility = value ? Visibility.Visible : Visibility.Collapsed;

                PropertyChanged?.Invoke(this, new(nameof(HasSelectedLayer)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void LayerNameChanged(ArtLayer layer, string name)
        {
            SelectedLayerName = name;
            PropertyChanged?.Invoke(this, new(nameof(Layers)));
        }

        private void LayerVisibilityChanged(ArtLayer layer, bool visible)
            => SelectedLayerVisibility = visible;

        private void ArtLayerAdded(int index, ArtLayer layer)
        {
            SelectedLayer = SelectedLayerID != -1 ? Layers[SelectedLayerID] : null;
        }

        private void ArtLayerRemoved(int index, ArtLayer layer)
        {
            SelectedLayer = SelectedLayerID != -1 ? Layers[SelectedLayerID] : null;
        }
    }
}
