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
        public BackgroundTaskToken DisplayBackgroundTaskToken { get; set; }
        public bool CloseOnFinish { get; set; } = true;

        private BackgroundTaskViewModel viewModel { get; set; }

        public BackgroundTaskWindow(BackgroundTaskToken backgroundTaskToken, bool closeOnFinish = true)
        {
            InitializeComponent();

            CloseOnFinish = closeOnFinish;
            Title = backgroundTaskToken.Name;

            viewModel = (BackgroundTaskViewModel)FindResource("ViewModel");

            DisplayBackgroundTaskToken = backgroundTaskToken;

            viewModel.BackgroundTaskToken = backgroundTaskToken;

            viewModel.UpdateTimeTimer.Start();
            DisplayBackgroundTaskToken.Completed += DisplayBackgroundTaskToken_Completed;
        }

        private void DisplayBackgroundTaskToken_Completed(BackgroundTaskToken token, Exception? ex)
        {
            viewModel.UpdateTimeTimer.Stop();

            if (CloseOnFinish)
                Close();
            else
                Focus();
        }
    }
}
