using System;
using System.Collections.Generic;
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

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for NewFileDialogWindow.xaml
    /// </summary>
    public partial class NewASCIIArtDialogWindow : Window
    {
        private NewASCIIArtDialogViewModel viewModel { get; set; }
        public ASCIIArt? CreatedArt { get; set; }

        public NewASCIIArtDialogWindow()
        {
            InitializeComponent();

            viewModel = (NewASCIIArtDialogViewModel)FindResource("NewFileDialogViewModel");
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(viewModel.WidthText, out int width))
            {
                MessageBox.Show("Invalid art width! (Must be greater than 0)", "New ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (width <= 0)
            {
                MessageBox.Show("Invalid art width! (Must be greater than 0)", "New ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(viewModel.HeightText, out int height))
            {
                MessageBox.Show("Invalid art height! (Must be greater than 0)", "New ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (height <= 0)
            {
                MessageBox.Show("Invalid art height! (Must be greater than 0)", "New ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (width * height > App.MaxArtArea)
            {
                MessageBox.Show($"The art area of one layer is too large! Max: {App.MaxArtArea} characters ({width * height} characters)", "New ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CreatedArt = new();
            CreatedArt.SetSize(width, height);
            CreatedArt.AddLayer(new("Background", width, height));

            Close();
        }
    }
}
