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

namespace AAP.UI.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public string[] TypefaceSources { get; } = new string[] { "Consolas", "Courier New", "Cascadia Code", "Cascadia Mono", "Noto Mono", "Lucida Console" };

        private bool darkMode = Settings.Default.DarkMode;
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

        private string canvasTypefaceSource = Settings.Default.CanvasTypefaceSource;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsWindowViewModel()
        {
            ApplyCommand = new ActionCommand((parameter) => Apply());
            ResetCommand = new ActionCommand((parameter) => Reset());
        }

        public void Apply()
        {
            Settings.Default.DarkMode = DarkMode;
            Settings.Default.CanvasTypefaceSource = CanvasTypefaceSource;
            
            App.DarkMode = Settings.Default.DarkMode;

            ChangesMade = false;
            Settings.Default.Save();
        }

        public void Reset()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to reset all settings?", "Settings", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Settings.Default.Reset();

                DarkMode = Settings.Default.DarkMode;
                CanvasTypefaceSource = Settings.Default.CanvasTypefaceSource;

                Apply();
            }
        }
    }
}
