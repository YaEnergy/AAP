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
using AAP.UI.ViewModels;
using Newtonsoft.Json.Linq;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for ImageASCIIArtExportOptionsWindow.xaml
    /// </summary>
    public partial class ImageASCIIArtExportOptionsWindow : Window
    {
        private ImageASCIIArtExportOptions exportOptions = new();
        public ImageASCIIArtExportOptions ExportOptions
        {
            get => exportOptions;
            set
            {
                if (exportOptions == value)
                    return;

                exportOptions = value;

                WindowViewModel.TextSizeText = value.TextSize.ToString();
            }
        }

        public ImageASCIIArtExportOptionsWindow()
        {
            InitializeComponent();

            Title = "Export ASCII Art Image Options";
            WindowViewModel.CloseButtonContent = "Export";

            WindowViewModel.TextSizeText = ExportOptions.TextSize.ToString();

            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyToOptions());
        }

        public void ApplyToOptions()
        {
            if (!int.TryParse(WindowViewModel.TextSizeText, out int textSize))
            {
                MessageBox.Show("Invalid export text size! (Must be greater than 0 & less than or equal to 256)", "Image ASCII Art Export Options", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (textSize <= 0 || textSize > 256)
            {
                MessageBox.Show("Invalid export text size! (Must be greater than 0 & less than or equal to 256)", "Image ASCII Art Export Options", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ExportOptions.TextSize = textSize;
            
            DialogResult = true;
            Close();
        }
    }
}
