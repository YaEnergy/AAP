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
        private readonly Task mainTask;
        public Task MainTask
        {
            get => mainTask;
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

        private readonly Stopwatch stopwatch = new();
        public TimeSpan Time
        {
            get => stopwatch.Elapsed;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public BackgroundTask(string name, Task task)
        {
            this.name = name;
            mainTask = task;

            stopwatch.Start();
        }
    }
}
