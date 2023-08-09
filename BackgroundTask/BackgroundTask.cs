using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.BackgroundTasks
{
    public class BackgroundTask : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;

                name = value;
                PropertyChanged?.Invoke(this, new(nameof(Name)));
            }
        }

        private readonly BackgroundWorker worker;
        public BackgroundWorker Worker
        {
            get => worker;
        }

        private int progressPercentage;
        public int ProgressPercentage
        {
            get => progressPercentage;
            set
            {
                if (progressPercentage == value)
                    return;

                progressPercentage = value;
                PropertyChanged?.Invoke(this, new(nameof(ProgressPercentage)));
            }
        }

        private bool isDeterminate;
        public bool IsDeterminate
        {
            get => isDeterminate;
            set
            {
                if (isDeterminate == value)
                    return;

                isDeterminate = value;
                PropertyChanged?.Invoke(this, new(nameof(IsDeterminate)));
            }
        }

        private string objective = "";
        public string Objective
        {
            get => objective;
            set
            {
                if (objective == value)
                    return;

                objective = value;
                PropertyChanged?.Invoke(this, new(nameof(Objective)));
            }
        }

        private readonly Stopwatch stopwatch = new();
        public TimeSpan Time
        {
            get => stopwatch.Elapsed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public BackgroundTask(string name, BackgroundWorker worker)
        {
            this.name = name;
            this.worker = worker;

            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            if (worker.IsBusy)
                stopwatch.Start();
            else
                worker.DoWork += Worker_DoWork;
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            stopwatch.Start();
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;

            if (e.UserState is string taskStateString)
                Objective = $"{taskStateString} ({e.ProgressPercentage}%)";
            else if (e.UserState is BackgroundTaskUpdateArgs backgroundTaskUpdateArgs)
            {
                IsDeterminate = backgroundTaskUpdateArgs.IsDeterminate;
                Objective = $"{backgroundTaskUpdateArgs.CurrentObjective}";

                if (!backgroundTaskUpdateArgs.IsDeterminate)
                    Objective += $" ({e.ProgressPercentage}%)";
            }
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();

            if (e.Cancelled)
                Objective = "Cancelled";
            else if (e.Error != null)
                Objective = $"Error encountered!";
            else
                Objective = $"Finished";
        }

        public void CancelAsync()
        {
            if (!Worker.WorkerSupportsCancellation)
            {
                ConsoleLogger.Warn("Background Task Worker does not support cancellation!");
                return;
            }

            Worker.CancelAsync();
        }
    }
}
