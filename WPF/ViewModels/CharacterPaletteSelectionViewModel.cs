using AAP.BackgroundTasks;
using AAP.Files;
using AAP.UI.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace AAP.UI.ViewModels
{
    public class CharacterPaletteSelectionViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CharacterPalette> palettes = new();
        public ObservableCollection<CharacterPalette> Palettes
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

        private BackgroundTaskToken? currentBackgroundTaskToken = null;
        public BackgroundTaskToken? CurrentBackgroundTaskToken
        {
            get => currentBackgroundTaskToken;
            set
            {
                if (currentBackgroundTaskToken == value)
                    return;

                currentBackgroundTaskToken = value;

                PropertyChanged?.Invoke(this, new(nameof(CurrentBackgroundTaskToken)));
            }
        }

        public ICommand NewPaletteCommand { get; private set; }
        public ICommand EditPaletteCommand { get; private set; }
        public ICommand RemovePaletteCommand { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CharacterPaletteSelectionViewModel()
        {
            NewPaletteCommand = new ActionCommand(async (parameter) => await NewPaletteAsync());
            EditPaletteCommand = new ActionCommand(async (parameter) => await EditPaletteAsync());
            RemovePaletteCommand = new ActionCommand(async (parameter) => await RemovePaletteAsync());
        }

        public bool? ShowCharacterPaletteDialog(CharacterPalette palette, string closeMessage)
        {
            string dialogTitle = App.Language.GetString("Palette");
            string namePropertyName = App.Language.GetString("Name");
            string charactersPropertyName = App.Language.GetString("Palette_Characters");

            string invalidPropertyNameErrorMessage = App.Language.GetString("Error_DefaultInvalidPropertyMessage");

            string invalidCharacterInCharactersErrorMessage = App.Language.GetString("Error_Palette_InvalidCharacterMessage");
            string invalidPaletteNameErrorMessage = App.Language.GetString("Error_Palette_InvalidNameMessage");
            string invalidFileNameErrorMessage = App.Language.GetString("Error_Palette_InvalidFileNameMessage");
            string paletteExistsErrorMessage = App.Language.GetString("Error_Palette_AlreadyExistsMessage");

            bool successful = false;

            while (!successful)
            {
                PropertiesWindow paletteWindow = new(dialogTitle, closeMessage);
                paletteWindow.AddStringProperty(namePropertyName, palette.Name);

                string startPaletteCharacterString = "";
                foreach (char character in palette.Characters)
                    startPaletteCharacterString += character;

                paletteWindow.AddStringProperty(charactersPropertyName, startPaletteCharacterString);

                bool? createdPalette = paletteWindow.ShowDialog();

                if (createdPalette != true)
                    return createdPalette;

                if (paletteWindow.GetProperty(namePropertyName) is not string paletteName || string.IsNullOrWhiteSpace(paletteName))
                {
                    MessageBox.Show(invalidPaletteNameErrorMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

                foreach (char fileNameChar in paletteName.ToCharArray())
                    if (invalidFileNameChars.Contains(fileNameChar))
                    {
                        string invalidFileNameCharsString = "";
                        foreach (char invalidFileNameChar in invalidFileNameChars)
                            invalidFileNameCharsString += invalidFileNameChar.ToString();

                        MessageBox.Show(string.Format(invalidFileNameErrorMessage, invalidFileNameCharsString), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                foreach (CharacterPalette characterPalette in App.CharacterPalettes)
                    if (characterPalette.Name == paletteName && characterPalette != palette)
                    {
                        MessageBox.Show(string.Format(paletteExistsErrorMessage, paletteName), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                if (paletteWindow.GetProperty(charactersPropertyName) is not string paletteCharactersString)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, charactersPropertyName), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                successful = true;

                ObservableCollection<char> characters = new();
                foreach (char character in paletteCharactersString.ToCharArray())
                    if (CharacterPalette.InvalidCharacters.Contains(character))
                        MessageBox.Show(string.Format(invalidCharacterInCharactersErrorMessage, character), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    else if (!characters.Contains(character))
                        characters.Add(character);

                palette.Name = paletteName;
                palette.Characters = characters;
            }

            return successful;
        }

        #region Background Task Work
        private async Task NewPaletteAsync()
        {
            string dialogTitle = App.Language.GetString("Palette");

            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ObservableCollection<char> paletteCharacters = new();
            foreach (char character in $@"|\/*-_".ToCharArray())
                paletteCharacters.Add(character);

            CharacterPalette palette = new(App.Language.GetString("Default_Palettes_New"), paletteCharacters);
            bool? createdPalette = ShowCharacterPaletteDialog(palette, App.Language.GetString("Create"));

            if (createdPalette != true)
                return;

            try
            {
                BackgroundTaskToken bgTask = new(string.Format(App.Language.GetString("Palette_CreateBusy"), palette.Name));
                Task task = App.ExportPaletteFileAsync(palette);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                await task;

                Palettes.Add(palette);
                SelectedPalette = palette;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show(string.Format(App.Language.GetString("Palette_CreateFailedMessage"), ex.Message), dialogTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;
        }

        private async Task EditPaletteAsync()
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to edit a palette.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedPalette == null)
                return;

            CharacterPalette palette = SelectedPalette;

            if (palette.IsPresetPalette)
            {
                MessageBox.Show("Can not edit preset palettes.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string originalPaletteName = palette.Name;

            bool? appliedChanges = ShowCharacterPaletteDialog(palette, "Edit");

            if (appliedChanges != true)
                return;

            try
            {
                BackgroundTaskToken bgTask = new($"Editing palette {palette.Name} file...");
                Task task = App.EditPaletteFileAsync(originalPaletteName, palette);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                await task;

                SelectedPalette = palette;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to edit palette file! Exception message: {ex.Message}", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;
        }

        private async Task RemovePaletteAsync()
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to remove a palette.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CharacterPalette? palette = SelectedPalette;

            if (palette == null)
                return;

            if (palette.IsPresetPalette)
            {
                MessageBox.Show("Can not remove preset palettes.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult questionResult = MessageBox.Show("Are you sure you want to remove palette " + palette.Name + "? This can not be undone.", "Palettes", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (questionResult != MessageBoxResult.Yes)
                return;

            try
            {
                BackgroundTaskToken bgTask = new($"Removing palette {palette.Name} file...");
                Task task = App.RemovePaletteFileAsync(palette);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                await task;

                SelectedPalette = null;
                Palettes.Remove(palette); 
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to remove palette file! Exception message: {ex.Message}", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;
        }
        #endregion
    }
}
