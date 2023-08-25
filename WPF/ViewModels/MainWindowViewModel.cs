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
using AAP.Timelines;
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private BackgroundTask? currentBackgroundTask = null;
        public BackgroundTask? CurrentBackgroundTask
        {
            get => currentBackgroundTask;
            set
            {
                if (currentBackgroundTask == value)
                    return;

                currentBackgroundTask = value;
                BackgroundTaskVisibility = CurrentBackgroundTask == null ? Visibility.Collapsed : Visibility.Visible;

                PropertyChanged?.Invoke(this, new(nameof(CurrentBackgroundTask)));
            }
        }

        private Visibility backgroundTaskVisibility = Visibility.Collapsed;
        public Visibility BackgroundTaskVisibility
        {
            get => backgroundTaskVisibility;
            set
            {
                if (backgroundTaskVisibility == value)
                    return;

                backgroundTaskVisibility = value;

                PropertyChanged?.Invoke(this, new(nameof(BackgroundTaskVisibility)));
            }
        }

        private bool isDarkModeOn = false;
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

                ToolboxVisibility = value ? Visibility.Visible : Visibility.Collapsed;

                PropertyChanged?.Invoke(this, new(nameof(IsToolboxVisible)));
            }
        }

        private Visibility toolboxVisibility = Visibility.Visible;
        public Visibility ToolboxVisibility
        {
            get => toolboxVisibility;
            private set
            {
                if (toolboxVisibility == value)
                    return;

                toolboxVisibility = value;
                PropertyChanged?.Invoke(this, new(nameof(ToolboxVisibility)));
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

                LayerManagementVisibility = value ? Visibility.Visible : Visibility.Collapsed;

                PropertyChanged?.Invoke(this, new(nameof(IsLayerManagementVisible)));
            }
        }

        private Visibility layerManagementVisibility = Visibility.Visible;
        public Visibility LayerManagementVisibility
        {
            get => layerManagementVisibility;
            private set
            {
                if (layerManagementVisibility == value)
                    return;

                layerManagementVisibility = value;
                PropertyChanged?.Invoke(this, new(nameof(LayerManagementVisibility)));
            }
        }

        public ICommand? ExitCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            ExitCommand = new ActionCommand((parameter) => Application.Current.Shutdown());
        }
    }
}
