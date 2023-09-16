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
    public class ImageASCIIArtImportOptionsWindowViewModel : INotifyPropertyChanged
    {
        private bool invertBrightness = false;
        public bool InvertBrightness
        {
            get => invertBrightness;
            set 
            {
                if (invertBrightness == value)
                    return;

                invertBrightness = value;

                PropertyChanged?.Invoke(this, new(nameof(InvertBrightness)));
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

        public ImageASCIIArtImportOptionsWindowViewModel()
        {
            
        }

    }
}
