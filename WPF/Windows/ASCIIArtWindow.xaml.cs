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

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for NewFileDialogWindow.xaml
    /// </summary>
    public partial class ASCIIArtWindow : Window
    {
        private ASCIIArt art = new();
        public ASCIIArt Art
        {
            get => art;
            set
            {
                if (art == value)
                    return;

                art = value;

                WindowViewModel.WidthText = art.Width.ToString();
                WindowViewModel.HeightText = art.Height.ToString();
            }
        }

        public ASCIIArtWindow()
        {
            InitializeComponent();

            Title = "Create New ASCII Art";
            WindowViewModel.CloseButtonContent = "Create ASCII Art";

            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyToArt());
        }

        public ASCIIArtWindow(ASCIIArt art)
        {
            InitializeComponent();

            Title = "Edit ASCII Art";
            Art = art;
            WindowViewModel.CloseButtonContent = "Apply changes to ASCII Art";

            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyToArt());
        }

        public void ApplyToArt()
        {
            if (!int.TryParse(WindowViewModel.WidthText, out int width))
            {
                MessageBox.Show("Invalid art width! (Must be greater than 0)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (width <= 0)
            {
                MessageBox.Show("Invalid art width! (Must be greater than 0)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(WindowViewModel.HeightText, out int height))
            {
                MessageBox.Show("Invalid art height! (Must be greater than 0)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (height <= 0)
            {
                MessageBox.Show("Invalid art height! (Must be greater than 0)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (width * height > App.MaxArtArea)
            {
                MessageBox.Show($"Canvas is too large! Max: {App.MaxArtArea} characters ({width * height} characters)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int fullArtArea = Art.GetTotalArtArea();

            if (fullArtArea < width * height)
                fullArtArea = width * height;

            if (fullArtArea >= App.WarningLargeArtArea)
            {
                string message = $"The art you're trying to create/edit has an total art area of {fullArtArea} characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters.\nThis might take a long time to load and save, and can be performance heavy.\nAre you sure you want to continue?";

                if (fullArtArea >= App.WarningIncrediblyLargeArtArea)
                    message = $"The art you're to trying to create/edit has a total art area of {fullArtArea} characters. This is above the recommended area limit of {App.WarningLargeArtArea} characters and above the less recommended area limit of {App.WarningIncrediblyLargeArtArea} characters.\nThis might take a VERY long time to load and save, and can be INCREDIBLY performance heavy.\nAre you SURE you want to continue?";

                MessageBoxResult result = MessageBox.Show(message, "ASCII Art", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    Close();
                    return;
                }
            }

            Art.SetSize(width, height);

            DialogResult = true;
            Close();
        }
    }
}
