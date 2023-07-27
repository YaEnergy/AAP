using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class NewASCIIArtDialogViewModel : INotifyPropertyChanged
    {
        private string widthText = "";
        public string WidthText
        {
            get => widthText;
            set 
            {
                widthText = value;

                PropertyChanged?.Invoke(this, new(nameof(WidthText)));
            }
        }

        private string heightText = "";
        public string HeightText
        {
            get => heightText;
            set
            {
                heightText = value;

                PropertyChanged?.Invoke(this, new(nameof(HeightText)));
            }
        }

        private bool canCreate = true;
        public bool CanCreate
        {
            get => canCreate;
            set
            {
                canCreate = value;

                PropertyChanged?.Invoke(this, new(nameof(CanCreate)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public NewASCIIArtDialogViewModel()
        {

        }

    }
}
