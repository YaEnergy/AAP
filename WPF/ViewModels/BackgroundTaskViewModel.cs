using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class BackgroundTaskViewModel : INotifyPropertyChanged
    {
        private string taskName = "Background Task";
        public string TaskName
        {
            get => taskName;
            set
            {
                taskName = value;
                PropertyChanged?.Invoke(this, new(nameof(TaskName)));
            }
        }

        private string taskState = "";
        public string TaskState
        {
            get => taskState;
            set
            {
                taskState = value;
                PropertyChanged?.Invoke(this, new(nameof(TaskState)));
            }
        }

        private int taskProgress = 0;
        public int TaskProgress
        {
            get => taskProgress;
            set
            {
                taskProgress = value;
                PropertyChanged?.Invoke(this, new(nameof(TaskProgress)));
            }
        }

        private bool isDeterminate = false;
        public bool IsDeterminate
        {
            get => isDeterminate;
            set
            {
                isDeterminate = value;
                PropertyChanged?.Invoke(this, new(nameof(IsDeterminate)));
            }
        }

        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer = new();

        private readonly Stopwatch taskStopwatch = new();

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

        public void StartStopwatch()
        {
            taskStopwatch.Start();
            dispatcherTimer.Start();
        }

        public void StopStopwatch()
        {
            taskStopwatch.Stop();
            dispatcherTimer.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public BackgroundTaskViewModel() 
        {
            dispatcherTimer.Interval = new(0, 0, 1);
            dispatcherTimer.Tick += (sender, e) => TaskElapsedTimeString = taskStopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        }

    }
}
