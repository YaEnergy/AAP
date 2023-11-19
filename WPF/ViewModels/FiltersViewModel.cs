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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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
            Filter? filter = null;

            while (filter == null)
            {
                PropertiesWindow artWindow = new(MirrorFilterContent, ApplyFilterContent);
                
                // Property additions
                // Enum property additions (Need a method for adding combo boxes)

                bool? result = artWindow.ShowDialog();

                if (result != true)
                    return;

                // Property errors

                Axis2D mirrorAxis = Axis2D.X;

                // Create Filter
            }

            if (filter == null)
                return;

            filter.Apply();
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
