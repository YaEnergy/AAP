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
        private BackgroundTask? backgroundTask = null;
        public BackgroundTask? BackgroundTask
        {
            get => backgroundTask;
            set
            {
                if (backgroundTask == value)
                    return;

                if (backgroundTask != null)
                {
                    if (backgroundTask.Worker.IsBusy)
                        UpdateTimeTimer.Stop();

                    backgroundTask.Worker.DoWork -= Worker_DoWork;
                }

                if (value != null)
                {
                    if (value.Worker.IsBusy)
                        UpdateTimeTimer.Start();
                    else
                        value.Worker.DoWork += Worker_DoWork;

                    value.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                }

                backgroundTask = value;
                PropertyChanged?.Invoke(this, new(nameof(BackgroundTask)));
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
            if (BackgroundTask == null)
                return;

            TaskElapsedTimeString = BackgroundTask.Time.ToString(@"hh\:mm\:ss");
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            UpdateTimeTimer.Start();
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            UpdateTimeTimer.Stop();
        }
    }
}
