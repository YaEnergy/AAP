using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            App.OnCurrentArtChanged += OnCurrentArtChanged;
            App.OnCurrentFilePathChanged += OnCurrentFilePathChanged;
            App.OnCurrentToolChanged += OnCurrentToolChanged;
            App.OnSelectionChanged += OnSelectionChanged;

            artCanvas.Tool = App.CurrentTool;

            UpdateTitle();
        }

        private void UpdateTitle()
            => Title = $"{App.ProgramTitle} - {( App.CurrentFilePath == null ? "*.*" : new FileInfo(App.CurrentFilePath).Name)} - {(App.CurrentArt != null ? App.CurrentArt.Width.ToString() : "*")}x{(App.CurrentArt != null ? App.CurrentArt.Height.ToString() : "*")}";

        #region App Events

        private void OnCurrentArtChanged(ASCIIArt? art, ASCIIArtDraw? artDraw)
        {
            artCanvas.DisplayArt = art; 
            artCanvas.DisplayArtDraw = artDraw;

            UpdateTitle();
        }

        private void OnCurrentFilePathChanged(string? filePath)
        {
            UpdateTitle();
        }

        private void OnCurrentToolChanged(Tool tool)
        {
            artCanvas.Tool = tool;
        }

        private void OnSelectionChanged(Rect selected)
        {
            artCanvas.SelectArtMatrixRect(selected);
        }

        #endregion

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Open ASCII Art File",
                Filter = "ASCII Art Files (*.aaf)|*.aaf|Text Files (*.txt)|*.txt",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                Exception? ex = App.OpenFile(new(openFileDialog.FileName));

                if (ex != null)
                    System.Windows.MessageBox.Show($"An error has occurred while opening art file ({openFileDialog.FileName})! Exception: {ex.Message}", "Open File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
