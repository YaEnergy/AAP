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
                IsBackgroundTaskVisible = value != null;

                PropertyChanged?.Invoke(this, new(nameof(CurrentBackgroundTask)));
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

        public ICommand? ExitCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            ExitCommand = new ActionCommand((parameter) => Application.Current.Shutdown());
        }
    }
}
