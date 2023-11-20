using AAP.BackgroundTasks;
using AAP.FileObjects;
using AAP.Files;
using AAP.Filters;
using AAP.Timelines;
using AAP.UI.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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

            string affectPropertyContent = App.Language.GetString("Filters_Affect");
            string affectPropertyTooltip = App.Language.GetString("Filters_Affect_Tooltip");

            string mirrorAxisPropertyContent = App.Language.GetString("Filters_Mirror_Axis");
            string mirrorAxisPropertyTooltip = App.Language.GetString("Filters_Mirror_Axis_Tooltip");

            List<string> affectOptions = new() { "Selection", "Selected Layer", "Canvas" };

            Filter? filter = null;

            while (filter == null)
            {
                PropertiesWindow artWindow = new(MirrorFilterContent, ApplyFilterContent);

                // Property additions
                artWindow.AddProperty(affectPropertyContent, affectPropertyTooltip, artWindow.CreateComboBoxIntProperty("Affect", affectOptions, 0));

                artWindow.AddProperty(mirrorAxisPropertyContent, mirrorAxisPropertyTooltip, artWindow.CreateComboBoxEnumProperty("MirrorAxis", Axis2D.X));
                
                artWindow.AddLabel(App.Language.GetString("Filter_Mirror_Description"));

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return;

                // Property errors

                if (artWindow.GetProperty("Affect") is not int affectInt)
                {
                    //Invalid affect message here, somehow
                    continue;
                }

                if (artWindow.GetProperty("MirrorAxis") is not Axis2D mirrorAxis)
                {
                    //Invalid mirror axis message here, somehow
                    continue;
                }

                // Create Filter
                switch (affectInt)
                {
                    case 0:
                        ASCIIArt art = App.CurrentArtFile.Art;
                        ArtLayer layer = art.ArtLayers[App.CurrentLayerID];

                        int left = Math.Max((int)App.SelectedArt.Left - layer.OffsetX, 0);
                        int top = Math.Max((int)App.SelectedArt.Top - layer.OffsetY, 0);
                        int right = Math.Min((int)App.SelectedArt.Right - layer.OffsetX, layer.Width);
                        int bottom = Math.Min((int)App.SelectedArt.Bottom - layer.OffsetY, layer.Height);

                        //If affect rect is outside layer, do nothing.
                        if (left >= layer.Width || top >= layer.Height || right < 0 || bottom < 0)
                            return;

                        if (right - left <= 0 || bottom - top <= 0)
                            return;

                        Rect affectRect = new(left, top, right - left, bottom - top);
                        App.SelectedArt = affectRect;

                        filter = new MirrorFilter(layer, affectRect, mirrorAxis);
                        break;
                    case 1:
                        filter = new MirrorFilter(App.CurrentArtFile.Art.ArtLayers[App.CurrentLayerID], mirrorAxis);
                        break;
                    case 2:
                        filter = new MirrorFilter(App.CurrentArtFile.Art, mirrorAxis);
                        break;
                }
            }

            if (filter == null)
                return;

            filter.Apply();
            App.CurrentArtFile.ArtTimeline.NewTimePoint();
            App.CurrentArtFile.Art.Update();
        }

        public void ApplyOutlineFilter()
        {
            Filter? filter = null;

            while (filter == null)
            {
                PropertiesWindow artWindow = new(OutlineFilterContent, ApplyFilterContent);
                
                // Property additions
                // Enum property additions (Need a method for adding combo boxes)

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return;

                // Property errors

                // Create Filter
            }

            if (filter == null)
                return;

            filter.Apply();
        }
    }
}
