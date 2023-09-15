using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class ImageASCIIArtExportOptionsWindowViewModel : INotifyPropertyChanged
    {
        private string textSizeText = "";
        public string TextSizeText
        {
            get => textSizeText;
            set 
            {
                if (textSizeText == value)
                    return;

                if (int.TryParse(value, out int size) && size > 0 && size <= 256)
                    textSizeText = value;
                else
                    MessageBox.Show("Text size must be between 1 & 256 and a natural number!", "Image ASCII Art Export Options", MessageBoxButton.OK, MessageBoxImage.Error);

                PropertyChanged?.Invoke(this, new(nameof(TextSizeText)));
            }
        }

        private SolidColorBrush artColorBrush = Brushes.Black;
        public SolidColorBrush ArtColorBrush
        {
            get => artColorBrush;
            set
            {
                if (artColorBrush == value) 
                    return;

                artColorBrush = value;

                PropertyChanged?.Invoke(this, new(nameof(ArtColorBrush)));
            }
        }

        private SolidColorBrush backgroundColorBrush = Brushes.White;
        public SolidColorBrush BackgroundColorBrush
        {
            get => backgroundColorBrush;
            set
            {
                if (backgroundColorBrush == value)
                    return;
                
                backgroundColorBrush = value;

                PropertyChanged?.Invoke(this, new(nameof(BackgroundColorBrush)));
            }
        }

        private string closeButtonContent = "Apply changes";
        public string CloseButtonContent
        {
            get => closeButtonContent;
            set
            {
                if (closeButtonContent == value)
                    return;

                closeButtonContent = value;

                PropertyChanged?.Invoke(this, new(nameof(CloseButtonContent)));
            }
        }

        private ICommand? closeButtonCommand;
        public ICommand? CloseButtonCommand
        {
            get => closeButtonCommand;
            set
            {
                if (closeButtonCommand == value)
                    return;

                closeButtonCommand = value;

                PropertyChanged?.Invoke(this, new(nameof(CloseButtonCommand)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageASCIIArtExportOptionsWindowViewModel()
        {
            
        }

    }
}
