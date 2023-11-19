using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AAP.BackgroundTasks;
using AAP.FileObjects;
using AAP.Files;
using AAP.Timelines;
using AAP.UI.Controls;
using AAP.UI.Windows;

namespace AAP.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private BackgroundTaskToken? currentBackgroundTaskToken = null;
        public BackgroundTaskToken? CurrentBackgroundTaskToken
        {
            get => currentBackgroundTaskToken;
            set
            {
                if (currentBackgroundTaskToken == value)
                    return;

                currentBackgroundTaskToken = value;
                IsBackgroundTaskVisible = value != null;

                PropertyChanged?.Invoke(this, new(nameof(CurrentBackgroundTaskToken)));
            }
        }

        private bool isBackgroundTaskVisible = false;
        public bool IsBackgroundTaskVisible
        {
            get => isBackgroundTaskVisible;
            set
            {
                if (isBackgroundTaskVisible == value)
                    return;

                isBackgroundTaskVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsBackgroundTaskVisible)));
            }
        }

        private bool isDarkModeOn = App.Settings.DarkMode;
        public bool IsDarkModeOn
        {
            get => isDarkModeOn;
            set
            {
                if (isDarkModeOn == value)
                    return;

                isDarkModeOn = value;

                PropertyChanged?.Invoke(this, new(nameof(IsDarkModeOn)));
            }
        }

        private bool isToolboxVisible = true;
        public bool IsToolboxVisible
        {
            get => isToolboxVisible;
            set
            {
                if (isToolboxVisible == value)
                    return;

                isToolboxVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsToolboxVisible)));
            }
        }

        private bool isLayerManagementVisible = true;
        public bool IsLayerManagementVisible
        {
            get => isLayerManagementVisible;
            set
            {
                if (isLayerManagementVisible == value)
                    return;

                isLayerManagementVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsLayerManagementVisible)));
            }
        }

        public ICommand OpenAboutCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            OpenAboutCommand = new ActionCommand((parameter) => OpenAboutWindow());
            OpenSettingsCommand = new ActionCommand((parameter) => OpenSettingsWindow());
            ExitCommand = new ActionCommand((parameter) => Application.Current.Shutdown());

            App.Settings.PropertyChanged += Settings_PropertyChanged;
            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content

        private string aboutMenuContent = App.Language.GetString("About");
        public string AboutMenuContent => aboutMenuContent;

        private string settingsMenuContent = App.Language.GetString("Settings");
        public string SettingsMenuContent => settingsMenuContent;

        private string exitContent = App.Language.GetString("Exit");
        public string ExitContent => exitContent;

        private string viewMenuContent = App.Language.GetString("ViewMenu");
        public string ViewMenuContent => viewMenuContent;

        private string darkModeContent = App.Language.GetString("DarkMode");
        public string DarkModeContent => darkModeContent;

        private string toolboxContent = App.Language.GetString("Toolbox");
        public string ToolboxContent => toolboxContent;

        private string canvasContent = App.Language.GetString("Canvas");
        public string CanvasContent => canvasContent;

        private string layerManagementContent = App.Language.GetString("LayerManagement");
        public string LayerManagementContent => layerManagementContent;

        private string visibilityCheckboxContent = App.Language.GetString("Visible");
        public string VisibilityCheckboxContent => visibilityCheckboxContent;

        private void OnLanguageChanged(Language language)
        {
            aboutMenuContent = App.Language.GetString("About");
            settingsMenuContent = App.Language.GetString("Settings");
            exitContent = App.Language.GetString("Exit");
            viewMenuContent = App.Language.GetString("ViewMenu");
            darkModeContent = App.Language.GetString("DarkMode");
            toolboxContent = App.Language.GetString("Toolbox");
            canvasContent = App.Language.GetString("Canvas");
            layerManagementContent = App.Language.GetString("LayerManagement");
            visibilityCheckboxContent = App.Language.GetString("Visible");

            PropertyChanged?.Invoke(this, new(nameof(AboutMenuContent)));
            PropertyChanged?.Invoke(this, new(nameof(SettingsMenuContent)));
            PropertyChanged?.Invoke(this, new(nameof(ExitContent)));
            PropertyChanged?.Invoke(this, new(nameof(ViewMenuContent)));
            PropertyChanged?.Invoke(this, new(nameof(DarkModeContent)));
            PropertyChanged?.Invoke(this, new(nameof(ToolboxContent)));
            PropertyChanged?.Invoke(this, new(nameof(CanvasContent)));
            PropertyChanged?.Invoke(this, new(nameof(LayerManagementContent)));
            PropertyChanged?.Invoke(this, new(nameof(VisibilityCheckboxContent)));
        }
        #endregion

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not AppSettings settings)
                return;

            switch(e.PropertyName)
            {
                case nameof(settings.DarkMode):
                    IsDarkModeOn = settings.DarkMode;
                    break;
                default:
                    break;
            }
        }

        public void OpenAboutWindow()
        {
            AboutWindow window = new();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        public void OpenSettingsWindow()
        {
            SettingsWindow settingsWindow = new();
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }
    }
}
