using AAP.UI.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace AAP.UI.ViewModels
{
    public class PropertiesWindowViewModel : INotifyPropertyChanged
    {
        private Style? checkBoxStyle = null;
        public Style? CheckBoxStyle
        {
            get => checkBoxStyle;
            set
            {
                if (checkBoxStyle == value)
                    return;

                checkBoxStyle = value;

                PropertyChanged?.Invoke(this, new(nameof(CheckBoxStyle)));
            }
        }

        private Brush? checkBoxForeground = null;
        public Brush? CheckBoxForeground
        {
            get => checkBoxForeground;
            set
            {
                if (checkBoxForeground == value)
                    return;

                checkBoxForeground = value;

                PropertyChanged?.Invoke(this, new(nameof(CheckBoxForeground)));
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

        public PropertiesWindowViewModel()
        {
            
        }
    }
}
