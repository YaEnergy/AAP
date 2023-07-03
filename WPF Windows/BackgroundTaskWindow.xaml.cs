using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AAP
{
    /// <summary>
    /// Interaction logic for BackgroundTaskWindow.xaml
    /// </summary>
    public partial class BackgroundTaskWindow : Window
    {
        public BackgroundWorker DisplayBackgroundWorker { get; set; }
        public bool CloseOnFinish { get; set; } = true;

        private BackgroundTaskViewModel viewModel { get; set; }

        private Stopwatch stopwatch = new();

        public BackgroundTaskWindow(BackgroundWorker worker, string taskName = "Background task...", bool closeOnFinish = true)
        {
            InitializeComponent();

            CloseOnFinish = closeOnFinish;
            Title = taskName;

            viewModel = (BackgroundTaskViewModel)FindResource("ViewModel");

            DisplayBackgroundWorker = worker;

            viewModel.TaskName = taskName;
            viewModel.IsDeterminate = !worker.WorkerReportsProgress;

            if (worker.WorkerReportsProgress)
                worker.ProgressChanged += Worker_ProgressChanged;

            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            stopwatch.Start();
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            viewModel.TaskProgress = e.ProgressPercentage;

            if (e.UserState is string taskStateString)
                viewModel.TaskState = $"{taskStateString} ({e.ProgressPercentage}%) ({stopwatch.Elapsed:hh\\:mm\\:ss})";
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();

            if (e.Cancelled)
                viewModel.TaskState = $"Cancelled ({stopwatch.Elapsed:hh\\:mm\\:ss})";
            else if (e.Error != null)
                viewModel.TaskState = $"Error occurred! ({stopwatch.Elapsed:hh\\:mm\\:ss})";
            else
                viewModel.TaskState = $"Finished ({stopwatch.Elapsed:hh\\:mm\\:ss})";

            if (CloseOnFinish)
                Close();
            else
                Focus();
        }
    }
}
