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
using AAP.BackgroundTasks;
using AAP.UI.ViewModels;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for BackgroundTaskWindow.xaml
    /// </summary>
    public partial class BackgroundTaskWindow : Window
    {
        public BackgroundTask DisplayBackgroundTask { get; set; }
        public bool CloseOnFinish { get; set; } = true;

        private BackgroundTaskViewModel viewModel { get; set; }

        public BackgroundTaskWindow(BackgroundTask backgroundTask, bool closeOnFinish = true)
        {
            InitializeComponent();

            CloseOnFinish = closeOnFinish;
            Title = backgroundTask.Name;

            viewModel = (BackgroundTaskViewModel)FindResource("ViewModel");

            DisplayBackgroundTask = backgroundTask;

            viewModel.BackgroundTask = backgroundTask;

            //backgroundTask.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            Closing += OnClosing;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            /*if (DisplayBackgroundTask.Worker.IsBusy)
            {
                if (!DisplayBackgroundTask.Worker.WorkerSupportsCancellation)
                    e.Cancel = true;
                else
                {
                    e.Cancel = true;
                    CloseOnFinish = true;
                    DisplayBackgroundTask.CancelAsync();
                }
            }*/
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (CloseOnFinish)
                Close();
            else
                Focus();
        }
    }
}
