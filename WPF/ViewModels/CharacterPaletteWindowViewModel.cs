using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP.UI.ViewModels
{
    public class CharacterPaletteWindowViewModel : INotifyPropertyChanged
    {
        private string paletteName = "New Palette";
        public string PaletteName
        {
            get => paletteName;
            set
            {
                if (paletteName == value)
                    return;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();

                    foreach (char fileNameChar in value.ToCharArray())
                        if (invalidFileNameChars.Contains(fileNameChar))
                        {
                            string invalidFileNameCharsString = "";
                            foreach (char invalidFileNameChar in invalidFileNameChars)
                                invalidFileNameCharsString += invalidFileNameChar.ToString();

                            MessageBox.Show($"Palette Name can not contain any of these characters: {invalidFileNameCharsString}", "Invalid Palette", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                    paletteName = value;
                }
                else
                    MessageBox.Show("Palette Name can not be null or just white spaces!", "Character Palette", MessageBoxButton.OK, MessageBoxImage.Error);

                PropertyChanged?.Invoke(this, new(nameof(PaletteName)));
            }
        }

        private string paletteString = $@"|\/*-_";
        public string PaletteString
        {
            get => paletteString;
            set
            {
                if (paletteString == value)
                    return;

                paletteString = value;

                PropertyChanged?.Invoke(this, new(nameof(PaletteString)));
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

        public CharacterPaletteWindowViewModel()
        {
            
        }
    }
}
