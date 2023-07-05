using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AAP
{
    public class ImageStateBoxViewModel : INotifyPropertyChanged
    {
        private System.Windows.Media.Brush boxBrush = System.Windows.Media.Brushes.White;
        public System.Windows.Media.Brush BoxBrush
        {
            get => boxBrush;
            set
            {
                if (value == boxBrush)
                    return;

                boxBrush = value;
                PropertyChanged?.Invoke(this, new(nameof(BoxBrush)));
            }
        }

        private ImageSource? boxImageSource = null;
        public ImageSource? BoxImageSource
        {
            get => boxImageSource;
            set
            {
                if (value == boxImageSource)
                    return;

                boxImageSource = value;
                PropertyChanged?.Invoke(this, new(nameof(BoxImageSource)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageStateBoxViewModel() 
        { 

        }

    }
}
