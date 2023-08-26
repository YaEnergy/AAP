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
                MessageBox.Show("Invalid Palette Name! Palette name can not be empty.", "Invalid Palette", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();

            foreach (char fileNameChar in WindowViewModel.PaletteName.ToCharArray())
                if (invalidFileNameChars.Contains(fileNameChar))
                {
                    string invalidFileNameCharsString = "";
                    foreach (char invalidFileNameChar in invalidFileNameChars)
                        invalidFileNameCharsString += invalidFileNameChar.ToString();

                    MessageBox.Show($"Invalid Palette Name! Palette name can not contain any of these characters: {invalidFileNameCharsString}", "Invalid Palette", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            foreach (CharacterPalette characterPalette in App.CharacterPalettes)
                if (characterPalette.Name == WindowViewModel.PaletteName && characterPalette != Palette)
                {
                    MessageBox.Show("Palette " + WindowViewModel.PaletteName + " already exists!", "Invalid Palette", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (!characters.Contains(character))
                    characters.Add(character);
            }

            Palette.Characters = characters;

            DialogResult = true;

            Close();
        }
    }
}
