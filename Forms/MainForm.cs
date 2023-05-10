using System.Windows.Forms;

namespace AAP
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void NewFileToolStripMenuItem_Click(object sender, EventArgs e)
            => new NewFileDialog().ShowDialog();

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open ASCII Art File";
            openFileDialog.Filter = "Art files (*.aaf)|*.aaf";
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath;
            openFileDialog.ValidateNames = true;
            openFileDialog.FileOk += (sender, args) => MainProgram.OpenFilePath(openFileDialog.FileName);

            openFileDialog.ShowDialog();
        }

        private void SaveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArtFile == null)
                return;

            if (MainProgram.CurrentFilePath == null || MainProgram.CurrentFilePath == "")
            {
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.Title = "Save ASCII Art File";
                saveFileDialog.Filter = "Art file (*.aaf)|*.aaf";
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.CheckPathExists = true;
                saveFileDialog.InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath;
                saveFileDialog.ValidateNames = true;
                saveFileDialog.FileOk += (sender, args) => MainProgram.SaveFileToPathAsync(MainProgram.CurrentArtFile, saveFileDialog.FileName);

                saveFileDialog.ShowDialog();

                return;
            }

            MainProgram.SaveFileToPathAsync(MainProgram.CurrentArtFile, MainProgram.CurrentFilePath);
        }

        private void SaveAsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArtFile == null)
                return;

            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Title = "Save ASCII Art File";
            saveFileDialog.Filter = "Art file (*.aaf)|*.aaf";
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath;
            saveFileDialog.ValidateNames = true;
            saveFileDialog.FileOk += (sender, args) => MainProgram.SaveFileToPathAsync(MainProgram.CurrentArtFile, saveFileDialog.FileName);

            saveFileDialog.ShowDialog();

            return;
        }
    }
}