using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AAP.UI.Controls;
using AAP.Properties;
using System.IO;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AAP.UI.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public string[] TypefaceSources { get; } = new string[] { "Consolas", "Courier New", "Cascadia Code", "Cascadia Mono", "Noto Mono", "Lucida Console" };

        private bool darkMode = App.Settings.DarkMode;
        public bool DarkMode
        {
            get => darkMode;
            set
            {
                if (darkMode == value)
                    return;

                darkMode = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(DarkMode)));
            }
        }

        private string canvasTypefaceSource = App.Settings.CanvasTypefaceSource;
        public string CanvasTypefaceSource
        {
            get => canvasTypefaceSource;
            set
            {
                if (canvasTypefaceSource == value)
                    return;

                canvasTypefaceSource = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(CanvasTypefaceSource)));
            }
        }

        private bool autosaveFiles = App.Settings.AutosaveFiles;
        public bool AutosaveFiles
        {
            get => autosaveFiles;
            set
            {
                if (autosaveFiles == value)
                    return;

                autosaveFiles = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveFiles)));
            }
        }

        private string autosaveIntervalMinutesText = ((int)App.Settings.AutosaveInterval.TotalMinutes).ToString();
        public string AutosaveIntervalMinutesText
        {
            get => autosaveIntervalMinutesText;
            set
            {
                if (autosaveIntervalMinutesText == value)
                    return;

                if (int.TryParse(value, out int minutes) && minutes > 0 && minutes <= 120)
                {
                    autosaveIntervalMinutesText = value;
                    AutosaveIntervalMinutes = minutes;
                    ChangesMade = true;
                }
                else
                    MessageBox.Show("Invalid Autosave Save Interval! Time interval must be larger than 0 and less than 120 minutes.", "Settings", MessageBoxButton.OK, MessageBoxImage.Error);

                PropertyChanged?.Invoke(this, new(nameof(AutosaveIntervalMinutesText)));
            }
        }

        private int autosaveIntervalMinutes = (int)App.Settings.AutosaveInterval.TotalMinutes;
        public int AutosaveIntervalMinutes
        {
            get => autosaveIntervalMinutes;
            set
            {
                if (autosaveIntervalMinutes == value)
                    return;

                autosaveIntervalMinutes = value;
                ChangesMade = true;

                PropertyChanged?.Invoke(this, new(nameof(AutosaveIntervalMinutes)));
            }
        }

        private bool changesMade = false;
        public bool ChangesMade
        {
            get => changesMade;
            set
            {
                if (changesMade == value)
                    return;

                changesMade = value;

                PropertyChanged?.Invoke(this, new(nameof(ChangesMade)));
            }
        }

        public ICommand ApplyCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand OpenAutosavesCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsWindowViewModel()
        {
            ApplyCommand = new ActionCommand((parameter) => Apply());
            ResetCommand = new ActionCommand((parameter) => Reset());
            OpenAutosavesCommand = new ActionCommand((parameter) => Process.Start("explorer.exe", App.AutoSaveDirectoryPath));
        }

        public void Apply()
        {
            App.Settings.DarkMode = DarkMode;
            App.Settings.CanvasTypefaceSource = CanvasTypefaceSource;
            App.Settings.AutosaveFiles = AutosaveFiles;
            App.Settings.AutosaveInterval = new(0, AutosaveIntervalMinutes, 0);

            ChangesMade = false;
            App.SaveSettings();
        }

        public void Reset()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to reset all settings?", "Settings", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                App.Settings.Reset();

                DarkMode = App.Settings.DarkMode;
                CanvasTypefaceSource = App.Settings.CanvasTypefaceSource;
                AutosaveFiles = App.Settings.AutosaveFiles;
                AutosaveIntervalMinutes = (int)App.Settings.AutosaveInterval.TotalMinutes;

                Apply();
            }
        }
    }
}
