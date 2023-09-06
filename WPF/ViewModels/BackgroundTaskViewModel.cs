using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using AAP.BackgroundTasks;
using AAP.UI.Controls;

namespace AAP.UI.ViewModels
{
    public class BackgroundTaskViewModel : INotifyPropertyChanged
    {
        private BackgroundTaskToken? backgroundTaskToken = null;
        public BackgroundTaskToken? BackgroundTaskToken
        {
            get => backgroundTaskToken;
            set
            {
                if (backgroundTaskToken == value)
                    return;

                backgroundTaskToken = value;
                PropertyChanged?.Invoke(this, new(nameof(BackgroundTaskToken)));
            }
        }

        public DispatcherTimer UpdateTimeTimer { get; } = new();

        private string taskElapsedTimeString = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
        public string TaskElapsedTimeString
        {
            get => taskElapsedTimeString;
            set
            {
                taskElapsedTimeString = value;
                PropertyChanged?.Invoke(this, new(nameof(TaskElapsedTimeString)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public BackgroundTaskViewModel() 
        {
            UpdateTimeTimer.Interval = new(0, 0, 1);
            UpdateTimeTimer.Tick += UpdateTimeTimer_Tick;
        }

        private void UpdateTimeTimer_Tick(object? sender, EventArgs e)
        {
            if (BackgroundTaskToken == null)
                return;

            TaskElapsedTimeString = BackgroundTaskToken.Time.ToString(@"hh\:mm\:ss");
        }
    }
}
