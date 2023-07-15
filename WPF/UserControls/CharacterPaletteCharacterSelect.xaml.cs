using AAP.UI.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AAP.UI.Controls
{
    /// <summary>
    /// Interaction logic for CharacterPaletteCharacterSelect.xaml
    /// </summary>
    public partial class CharacterPaletteCharacterSelect : UserControl
    {
        private Size gridItemSize = new(60, 60);
        public Size GridItemSize
        {
            get => gridItemSize;
            set
            {
                if (value == gridItemSize)
                    return;

                gridItemSize = value;
                UpdateDisplay();
            }
        }

        public static readonly DependencyProperty PaletteProperty =
        DependencyProperty.Register(
            name: "Palette",
            propertyType: typeof(CharacterPalette),
            ownerType: typeof(CharacterPaletteCharacterSelect),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, OnPalettePropertyChangedCallBack));

        public CharacterPalette? Palette
        {
            get => (CharacterPalette?)GetValue(PaletteProperty);
            set
            {
                if (value == Palette) 
                    return;

                SetValue(PaletteProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(
            name: "SelectedCharacter",
            propertyType: typeof(char?),
            ownerType: typeof(CharacterPaletteCharacterSelect),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, OnSelectedCharacterPropertyChangedCallBack));

        public char? SelectedCharacter 
        {
            get => (char?)GetValue(SelectedCharacterProperty);
            set
            {
                if (value == SelectedCharacter)
                    return;

                SetValue(SelectedCharacterProperty, value);
            }
        }

        public delegate void SelectedCharacterEvent(char? selectedCharacter);
        public event SelectedCharacterEvent? SelectedCharacterChanged;

        protected Dictionary<char, StateBox> stateBoxes { get; } = new();

        public CharacterPaletteCharacterSelect()
        {
            InitializeComponent();

            Loaded += (sender, e) => UpdateDisplay();
        }

        private static void OnPalettePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CharacterPaletteCharacterSelect? characterPaletteCharacterSelect = sender as CharacterPaletteCharacterSelect;

            if (characterPaletteCharacterSelect != null)
                characterPaletteCharacterSelect.UpdateDisplay();
        }

        private static void OnSelectedCharacterPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CharacterPaletteCharacterSelect? characterPaletteCharacterSelect = sender as CharacterPaletteCharacterSelect;

            if (characterPaletteCharacterSelect != null)
            {
                foreach (char character in characterPaletteCharacterSelect.stateBoxes.Keys)
                    characterPaletteCharacterSelect.stateBoxes[character].ForceSetState(character == characterPaletteCharacterSelect.SelectedCharacter);

                characterPaletteCharacterSelect.SelectedCharacterChanged?.Invoke((char?)e.NewValue);
            }
        }

        public void UpdateDisplay()
        {
            CharacterSelectGrid.Children.Clear();
            CharacterSelectGrid.RowDefinitions.Clear();
            CharacterSelectGrid.ColumnDefinitions.Clear();

            stateBoxes.Clear();

            if (Palette == null)
                return;

            int column = 0;
            int row = 0;

            foreach (char character in Palette.Characters)
            {
                char stateBoxCharacter = character;

                if (stateBoxes.ContainsKey(stateBoxCharacter))
                    continue;

                if (CharacterSelectGrid.RowDefinitions.Count <= row)
                    CharacterSelectGrid.RowDefinitions.Add(new() { Height = new(GridItemSize.Height, GridUnitType.Pixel) });

                if (CharacterSelectGrid.ColumnDefinitions.Count <= column)
                    CharacterSelectGrid.ColumnDefinitions.Add(new() { Width = new(GridItemSize.Width, GridUnitType.Pixel) });

                LabelStateBox labelStateBox = new()
                {
                    Text = stateBoxCharacter.ToString(),
                    TextSize = 32,
                    Margin = new(2, 2, 2, 2),
                    AllowManualDisable = false
                };

                void OnStateBoxStateChanged(StateBox sender, bool state)
                {
                    if (!state)
                        return;

                    SelectedCharacter = stateBoxCharacter;

                    foreach (StateBox stateBox in stateBoxes.Values)
                        if (stateBox != sender)
                            stateBox.State = false;
                }

                void OnUnloaded(object sender, RoutedEventArgs e)
                    => labelStateBox.OnStateChanged -= OnStateBoxStateChanged;


                if (SelectedCharacter == stateBoxCharacter)
                    labelStateBox.ForceSetState(true);

                labelStateBox.OnStateChanged += OnStateBoxStateChanged;
                labelStateBox.Unloaded += OnUnloaded;

                Grid.SetRow(labelStateBox, row);
                Grid.SetColumn(labelStateBox, column);
                
                stateBoxes.Add(stateBoxCharacter, labelStateBox);
                CharacterSelectGrid.Children.Add(labelStateBox);

                column++;

                if (Width < (column + 1) * GridItemSize.Width - SystemParameters.VerticalScrollBarWidth)
                {
                    column = 0;
                    row++;
                }
            }
        }
    }
}
