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

        private BackgroundTask? currentBackgroundTask = null;
        public BackgroundTask? CurrentBackgroundTask
        {
            get => currentBackgroundTask;
            set
            {
                if (currentBackgroundTask == value)
                    return;

                currentBackgroundTask = value;

                PropertyChanged?.Invoke(this, new(nameof(CurrentBackgroundTask)));
            }
        }

        public ICommand NewPaletteCommand { get; private set; }
        public ICommand EditPaletteCommand { get; private set; }
        public ICommand RemovePaletteCommand { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CharacterPaletteSelectionViewModel()
        {
            NewPaletteCommand = new ActionCommand((parameter) => NewPaletteAsync());
            EditPaletteCommand = new ActionCommand((parameter) => EditPaletteAsync());
            RemovePaletteCommand = new ActionCommand((parameter) => RemovePaletteAsync());
        }

        #region Background Task Work
        private BackgroundTask? NewPaletteAsync()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to create a new palette.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            CharacterPaletteWindow characterPaletteWindow = new();
            characterPaletteWindow.Owner = Application.Current.MainWindow;
            bool? createdPalette = characterPaletteWindow.ShowDialog();

            if (createdPalette == true)
            {
                BackgroundTask? bgTask = App.ExportPaletteAsync(characterPaletteWindow.Palette);

                if (bgTask == null)
                    return bgTask;

                bgTask.Worker.RunWorkerCompleted += BackgroundNewComplete;
                CurrentBackgroundTask = bgTask;

                return bgTask;
            }

            return null;
        }

        private BackgroundTask? EditPaletteAsync()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to edit a palette.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (SelectedPalette == null)
                return null;

            if (SelectedPalette.IsPresetPalette)
            {
                MessageBox.Show("Can not edit preset palettes.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string originalPaletteName = SelectedPalette.Name;

            CharacterPaletteWindow characterPaletteWindow = new(SelectedPalette);
            characterPaletteWindow.Owner = Application.Current.MainWindow;

            bool? appliedChanges = characterPaletteWindow.ShowDialog();
            if (appliedChanges == true)
            {
                BackgroundTask? bgTask = App.EditPaletteFileAsync(originalPaletteName, characterPaletteWindow.Palette);

                if (bgTask == null)
                    return bgTask;

                bgTask.Worker.RunWorkerCompleted += BackgroundEditComplete;
                CurrentBackgroundTask = bgTask;

                return bgTask;
            }

            return null;
        }

        private BackgroundTask? RemovePaletteAsync()
        {
            if (CurrentBackgroundTask != null)
            {
                MessageBox.Show("Current background task must be cancelled in order to remove a palette.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (SelectedPalette == null)
                return null;

            if (SelectedPalette.IsPresetPalette)
            {
                MessageBox.Show("Can not remove preset palettes.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            MessageBoxResult questionResult = MessageBox.Show("Are you sure you want to remove palette " + SelectedPalette.Name + "? This can not be undone.", "Palettes", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (questionResult != MessageBoxResult.Yes)
                return null;

            BackgroundTask? bgTask = App.RemovePaletteAsync(SelectedPalette);

            if (bgTask == null)
                return bgTask;

            bgTask.Worker.RunWorkerCompleted += BackgroundRemoveComplete;
            CurrentBackgroundTask = bgTask;

            return bgTask;
        }
        #endregion
        #region Background Task Complete
        void BackgroundNewComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (CurrentBackgroundTask != null)
                if (bgWorker == CurrentBackgroundTask.Worker)
                {
                    CurrentBackgroundTask = null;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled creating palette!", "Palettes", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while creating palette!\nException: " + args.Error.Message, "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (args.Result is CharacterPalette palette)
            {
                Palettes.Add(palette);
                SelectedPalette = palette;
            }
            else
                MessageBox.Show("Palette Creation was successful, but result is not Character Palette. Palette can not be used.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        void BackgroundEditComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (CurrentBackgroundTask != null)
                if (bgWorker == CurrentBackgroundTask.Worker)
                {
                    CurrentBackgroundTask = null;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled editing palette!", "Palettes", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while editing palette!\nException: " + args.Error.Message, "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (args.Result is CharacterPalette palette)
                SelectedPalette = palette;
            else
                MessageBox.Show("Palette Edit was successful, but result is not Character Palette.", "Palettes", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        void BackgroundRemoveComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (sender is not BackgroundWorker bgWorker)
                return;

            if (CurrentBackgroundTask != null)
                if (bgWorker == CurrentBackgroundTask.Worker)
                {
                    CurrentBackgroundTask = null;
                }

            if (args.Cancelled)
                MessageBox.Show("Cancelled removing palette!", "Palettes", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (args.Error != null)
                MessageBox.Show("An error has occurred while removing palette!\nException: " + args.Error.Message, "Palettes", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                SelectedPalette = null;
        }
        #endregion
    }
}
