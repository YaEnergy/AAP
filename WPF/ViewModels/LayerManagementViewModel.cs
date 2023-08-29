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

        private ObservableCollection<ArtLayer> layers = new();
        public ObservableCollection<ArtLayer> Layers
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
        }

        private void LayerNameChanged(ArtLayer layer, string name)
        {
            SelectedLayerName = name;
        }

        private void LayerVisibilityChanged(ArtLayer layer, bool visible)
            => SelectedLayerVisibility = visible;

        private void ArtLayerAdded(int index, ArtLayer layer)
        {
            SelectedLayer = SelectedLayerID != -1 ? Layers[Math.Clamp(SelectedLayerID, 0, Layers.Count - 1)] : null;
        }

        private void ArtLayerRemoved(int index, ArtLayer layer)
        {
            SelectedLayer = SelectedLayerID != -1 ? Layers[Math.Clamp(SelectedLayerID, 0, Layers.Count - 1)] : null;
        }
    }
}
