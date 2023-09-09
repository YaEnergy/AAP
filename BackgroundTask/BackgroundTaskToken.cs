using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.BackgroundTasks
{
    public delegate void BackgroundTaskProgressChangedEvent(BackgroundTaskToken token, int progress, BackgroundTaskProgressArgs? args);
    public delegate void BackgroundTaskCompletedEvent(BackgroundTaskToken token, Exception? ex);

    public class BackgroundTaskToken : INotifyPropertyChanged
    {
        private Task? mainTask;
        public Task? MainTask
        {
            get => mainTask;
            set
            {
                if (mainTask == value)
                    return;

                mainTask = value;

                PropertyChanged?.Invoke(this, new(nameof(MainTask)));
            }
        }

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

        private bool isDeterminate = true;
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

        private bool cancelationPending = false;
        public bool CancelationPending
        {
            get => cancelationPending;
            protected set
            {
                if (cancelationPending == value)
                    return;

                cancelationPending = value;
                PropertyChanged?.Invoke(this, new(nameof(CancelationPending)));
            }
        }

        private Exception? exception = null;
        public Exception? Exception
        {
            get => exception;
            set
            {
                if (exception == value)
                    return;

                exception = value;
                PropertyChanged?.Invoke(this, new(nameof(Exception)));
            }
        }

        private readonly Stopwatch stopwatch = new();
        public TimeSpan Time
        {
            get => stopwatch.Elapsed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event BackgroundTaskProgressChangedEvent? ProgressChanged;
        public event BackgroundTaskCompletedEvent? Completed;

        public BackgroundTaskToken(string name)
        {
            this.name = name;

            stopwatch.Start();
        }

        public void ReportProgress(int progressPercentage, BackgroundTaskProgressArgs? updateArgs)
        {
            ProgressPercentage = progressPercentage;

            if (updateArgs != null)
            {
                Objective = updateArgs.Value.CurrentObjective;
                IsDeterminate = updateArgs.Value.IsDeterminate;
            }

            ProgressChanged?.Invoke(this, progressPercentage, updateArgs);
        }

        public void Complete(Exception? ex = null)
        {
            Exception = ex;
            Completed?.Invoke(this, ex);
        }
    }
}
