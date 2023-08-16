using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.UI.ViewModels
{
    public class CharacterPaletteSelectionViewModel : INotifyPropertyChanged
    {
        private List<CharacterPalette> palettes = new();
        public List<CharacterPalette> Palettes
        {
            get => palettes;
            set
            {
                if (value == palettes)
                    return;

                palettes = value;

                PropertyChanged?.Invoke(this, new(nameof(Palettes)));
            }
        }

        private CharacterPalette? selectedPalette = null;
        public CharacterPalette? SelectedPalette 
        { 
            get => selectedPalette; 
            set
            {
                if (value == selectedPalette)
                    return;

                selectedPalette = value;

                PropertyChanged?.Invoke(this, new(nameof(SelectedPalette)));
            }
        }

        private char? selectedCharacter = null;
        public char? SelectedCharacter
        {
            get => selectedCharacter;
            set
            {
                if (value == selectedCharacter)
                    return;

                selectedCharacter = value;

                PropertyChanged?.Invoke(this, new(nameof(SelectedCharacter)));
            }
        }

        private Visibility visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => visibility;
            set
            {
                if (value == visibility) 
                    return;

                visibility = value;

                PropertyChanged?.Invoke(this, new(nameof(Visibility)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
