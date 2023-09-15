using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class ASCIIArtWindowViewModel : INotifyPropertyChanged
    {
        private string widthText = "32";
        public string WidthText
        {
            get => widthText;
            set 
            {
                if (widthText == value)
                    return;

                if (int.TryParse(value, out int width) && width > 0)
                    widthText = value;
                else
                    MessageBox.Show("Invalid width! (Must be greater than or equal to 1 and a natural number!)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);

                PropertyChanged?.Invoke(this, new(nameof(WidthText)));
            }
        }

        private string heightText = "16";
        public string HeightText
        {
            get => heightText;
            set
            {
                if (heightText == value)
                    return;

                if (int.TryParse(value, out int height) && height > 0)
                    heightText = value;
                else
                    MessageBox.Show("Invalid height! (Must be greater than or equal to 1 and a natural number!)", "ASCII Art", MessageBoxButton.OK, MessageBoxImage.Error);

                PropertyChanged?.Invoke(this, new(nameof(HeightText)));
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

        public ASCIIArtWindowViewModel()
        {

        }

    }
}
