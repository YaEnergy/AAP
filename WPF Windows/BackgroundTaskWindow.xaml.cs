using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            viewModel.TaskProgress = e.ProgressPercentage;

            viewModel.TaskState = e.UserState is string taskStateString ? $"{taskStateString} ({e.ProgressPercentage}%)" : $"{e.ProgressPercentage}%";
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                viewModel.TaskState = "Cancelled";
            else if (e.Error != null)
                viewModel.TaskState = "Error occurred!";
            else
                viewModel.TaskState = "Finished";

            if (CloseOnFinish)
                Close();
            else
                Focus();
        }
    }
}
