using AAP.BackgroundTasks;
using AAP.FileObjects;
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

            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content

        private string paletteContent = App.Language.GetString("Palette");
        public string PaletteContent => paletteContent;

        private string addContent = App.Language.GetString("Palette_New");
        public string AddContent => addContent;

        private string editContent = App.Language.GetString("Palette_Edit");
        public string EditContent => editContent;

        private string removeContent = App.Language.GetString("Palette_Remove");
        public string RemoveContent => removeContent;

        private void OnLanguageChanged(Language language)
        {
            paletteContent = language.GetString("Palette");
            addContent = language.GetString("Palette_New");
            editContent = language.GetString("Palette_Edit");
            removeContent = language.GetString("Palette_Remove");

            PropertyChanged?.Invoke(this, new(nameof(PaletteContent)));
            PropertyChanged?.Invoke(this, new(nameof(AddContent)));
            PropertyChanged?.Invoke(this, new(nameof(EditContent)));
            PropertyChanged?.Invoke(this, new(nameof(RemoveContent)));
        }

        #endregion

        public bool? ShowCharacterPaletteDialog(CharacterPalette palette, string closeMessage)
        {
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
                PropertiesWindow paletteWindow = new(PaletteContent, closeMessage);
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
                    MessageBox.Show(invalidPaletteNameErrorMessage, PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
                bool invalidFileName = false;
                string foundInvalidFileNameCharacters = "";

                foreach (char fileNameChar in paletteName.ToCharArray())
                    if (invalidFileNameChars.Contains(fileNameChar))
                    {
                        foundInvalidFileNameCharacters += fileNameChar + " ";

                        invalidFileName = true;
                    }

                if (invalidFileName)
                {
                    MessageBox.Show(string.Format(invalidFileNameErrorMessage, foundInvalidFileNameCharacters), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                foreach (CharacterPalette characterPalette in App.CharacterPalettes)
                    if (characterPalette.Name == paletteName && characterPalette != palette)
                    {
                        MessageBox.Show(string.Format(paletteExistsErrorMessage, paletteName), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                if (paletteWindow.GetProperty(charactersPropertyName) is not string paletteCharactersString)
                {
                    MessageBox.Show(string.Format(invalidPropertyNameErrorMessage, charactersPropertyName), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                successful = true;

                ObservableCollection<char> characters = new();
                foreach (char character in paletteCharactersString.ToCharArray())
                    if (CharacterPalette.InvalidCharacters.Contains(character))
                        MessageBox.Show(string.Format(invalidCharacterInCharactersErrorMessage, character), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(string.Format(App.Language.GetString("Palette_CreateFailedMessage"), ex.Message), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);

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
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedPalette == null)
                return;

            CharacterPalette palette = SelectedPalette;

            if (palette.IsPresetPalette)
            {
                MessageBox.Show(App.Language.GetString("Palette_EditPresetPaletteMessage"), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string originalPaletteName = palette.Name;

            bool? appliedChanges = ShowCharacterPaletteDialog(palette, App.Language.GetString("Edit"));

            if (appliedChanges != true)
                return;

            try
            {
                BackgroundTaskToken bgTask = new(string.Format(App.Language.GetString("Palette_EditBusy"), palette.Name));
                Task task = App.EditPaletteFileAsync(originalPaletteName, palette);
                bgTask.MainTask = task;

                CurrentBackgroundTaskToken = bgTask;

                await task;

                SelectedPalette = palette;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show(string.Format(App.Language.GetString("Palette_EditFailedMessage"), palette.Name, ex.Message), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);

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
                MessageBox.Show(App.Language.GetString("BackgroundTaskBusyMessage"), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CharacterPalette? palette = SelectedPalette;

            if (palette == null)
                return;

            if (palette.IsPresetPalette)
            {
                MessageBox.Show(App.Language.GetString("Palette_RemovePresetPaletteMessage"), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult questionResult = MessageBox.Show(string.Format(App.Language.GetString("Palette_RemoveMessage"), palette.Name), PaletteContent, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (questionResult != MessageBoxResult.Yes)
                return;

            try
            {
                BackgroundTaskToken bgTask = new(string.Format(App.Language.GetString("Palette_RemoveMessage"), palette.Name));
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
                MessageBox.Show(string.Format(App.Language.GetString("Palette_RemoveFailedMessage"), palette.Name, ex.Message), PaletteContent, MessageBoxButton.OK, MessageBoxImage.Error);

                if (CurrentBackgroundTaskToken != null)
                    CurrentBackgroundTaskToken.Exception = ex;
            }

            CurrentBackgroundTaskToken?.Complete();
            CurrentBackgroundTaskToken = null;
        }
        #endregion
    }
}
