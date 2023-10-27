﻿using AAP.FileObjects;
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

                if (string.IsNullOrEmpty(value))
                {
                    PropertyChanged?.Invoke(this, new(nameof(SelectedLayerName)));
                    return;
                }

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

            PropertyChanged?.Invoke(this, new(nameof(LayerOptionsContent)));
            PropertyChanged?.Invoke(this, new(nameof(LayerNameContent)));
            PropertyChanged?.Invoke(this, new(nameof(LayerVisibilityContent)));

            PropertyChanged?.Invoke(this, new(nameof(AddLayerTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(MoveLayerUpTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(MoveLayerDownTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(DuplicateLayerTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(MergeLayerDownTooltip)));
            PropertyChanged?.Invoke(this, new(nameof(RemoveLayerTooltip)));
        }
        #endregion

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
