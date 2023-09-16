using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AAP.Files;
using AAP.UI.ViewModels;
using Newtonsoft.Json.Linq;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for ImageASCIIArtImportOptionsWindow.xaml
    /// </summary>
    public partial class ImageASCIIArtImportOptionsWindow : Window
    {
        private ImageASCIIArtDecodeOptions importOptions = new();
        public ImageASCIIArtDecodeOptions ImportOptions
        {
            get => importOptions;
            set
            {
                if (importOptions == value)
                    return;

                importOptions = value;

                WindowViewModel.InvertBrightness = importOptions.ImageArtLayerConverter.Invert;
            }
        }

        public ImageASCIIArtImportOptionsWindow()
        {
            InitializeComponent();

            Title = "Import ASCII Art Image Options";
            WindowViewModel.CloseButtonContent = "Import";

            WindowViewModel.InvertBrightness = importOptions.ImageArtLayerConverter.Invert;

            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyToOptions());
        }

        public void ApplyToOptions()
        {
            ImportOptions.ImageArtLayerConverter.Invert = WindowViewModel.InvertBrightness;

            DialogResult = true;
            Close();
        }
    }
}
