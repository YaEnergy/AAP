using AAP.BackgroundTasks;
using AAP.UI.Windows;
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

        #region Background Task Work
        private async Task NewPaletteAsync()
        {
            if (CurrentBackgroundTaskToken != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to create a new palette.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CharacterPaletteWindow characterPaletteWindow = new();
            characterPaletteWindow.Owner = Application.Current.MainWindow;
            bool? createdPalette = characterPaletteWindow.ShowDialog();

            if (createdPalette != true)
                return;

            try
            {
                Task task = App.ExportPaletteFileAsync(characterPaletteWindow.Palette);

                BackgroundTaskToken bgTask = new($"Creating palette {characterPaletteWindow.Palette.Name} file...", task);
                CurrentBackgroundTaskToken = bgTask;

                await task;

                Palettes.Add(characterPaletteWindow.Palette);
                SelectedPalette = characterPaletteWindow.Palette;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to create palette file! Exception message: {ex.Message}", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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

            if (SelectedPalette.IsPresetPalette)
            {
                MessageBox.Show("Can not edit preset palettes.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string originalPaletteName = SelectedPalette.Name;

            CharacterPaletteWindow characterPaletteWindow = new(SelectedPalette);
            characterPaletteWindow.Owner = Application.Current.MainWindow;

            bool? appliedChanges = characterPaletteWindow.ShowDialog();

            if (appliedChanges != true)
                return;

            try
            {
                Task task = App.EditPaletteFileAsync(originalPaletteName, characterPaletteWindow.Palette);

                BackgroundTaskToken bgTask = new($"Editing palette {characterPaletteWindow.Palette.Name} file...", task);
                CurrentBackgroundTaskToken = bgTask;

                await task;

                SelectedPalette = characterPaletteWindow.Palette;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to edit palette file! Exception message: {ex.Message}", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
                Task task = App.RemovePaletteFileAsync(palette);

                BackgroundTaskToken bgTask = new($"Removing palette {palette.Name} file...", task);
                CurrentBackgroundTaskToken = bgTask;

                await task;

                SelectedPalette = null;
                Palettes.Remove(palette); 
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error(ex);
                MessageBox.Show($"Failed to remove palette file! Exception message: {ex.Message}", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CurrentBackgroundTaskToken = null;
        }
        #endregion
    }
}
