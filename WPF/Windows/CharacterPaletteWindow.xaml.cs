using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for CharacterPaletteWindow.xaml
    /// </summary>
    public partial class CharacterPaletteWindow : Window
    {
        private CharacterPalette palette = new();
        public CharacterPalette Palette
        {
            get => palette;
            set
            {
                if (palette == value)
                    return;

                palette = value;

                WindowViewModel.PaletteName = palette.Name;

                string newPaletteString = "";
                foreach (char character in palette.Characters)
                    newPaletteString += character;

                WindowViewModel.PaletteString = newPaletteString;
            }
        }

        public CharacterPaletteWindow()
        {
            InitializeComponent();

            Title = "Create New Palette";
            WindowViewModel.CloseButtonContent = "Create palette";

            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyToPalette());
        }

        public CharacterPaletteWindow(CharacterPalette palette)
        {
            InitializeComponent();

            Title = "Edit Palette";
            Palette = palette;
            WindowViewModel.CloseButtonContent = "Apply changes to palette";

            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyToPalette());
        }

        public void ApplyToPalette()
        {
            if (WindowViewModel.PaletteName == string.Empty)
            {
                MessageBox.Show("Invalid Palette Name!", "Invalid Palette", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Palette.Name = WindowViewModel.PaletteName;

            ObservableCollection<char> characters = new();
            foreach (char character in WindowViewModel.PaletteString.ToCharArray())
            {
                if (CharacterPalette.InvalidCharacters.Contains(character))
                {
                    MessageBox.Show("Invalid character in palette: " + character);
                    return;
                }

                characters.Add(character);
            }

            Palette.Characters = characters;

            DialogResult = true;

            Close();
        }
    }
}
