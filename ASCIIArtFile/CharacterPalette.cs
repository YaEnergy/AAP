using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class CharacterPalette : INotifyPropertyChanged
    {
        public static readonly char[] InvalidCharacters = new char[] { ASCIIArt.EMPTYCHARACTER };

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;

                name = value;
                PropertyChanged?.Invoke(this, new(nameof(Name)));
            }
        }

        private ObservableCollection<char> characters = new();
        public ObservableCollection<char> Characters
        {
            get => characters;
            set
            {
                if (characters == value)
                    return;

                characters = value;
                PropertyChanged?.Invoke(this, new(nameof(Characters)));
            }
        }

        private bool isPresetPalette = false;
        public bool IsPresetPalette
        {
            get => isPresetPalette;
            set
            {
                if (isPresetPalette == value)
                    return;

                isPresetPalette = value;

                PropertyChanged?.Invoke(this, new(nameof(IsPresetPalette)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CharacterPalette()
        {

        }

        [JsonConstructor]
        public CharacterPalette(string name, ObservableCollection<char> characters)
        {
            this.name = name;
            this.characters = characters;

            Name = name;
            Characters = characters;
        }


        public override string ToString()
            => Name;
    }
}
