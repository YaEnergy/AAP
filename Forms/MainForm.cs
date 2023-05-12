using System.Windows.Forms;

namespace AAP
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        public Dictionary<Point, Label> CharLabels { get; private set; } = new();
        public int CanvasTextSize { get; private set; } = 12;
        public MainForm()
        {
            InitializeComponent();

            OnCurrentFilePathChanged(null);

            MainProgram.OnCurrentFilePathChanged += OnCurrentFilePathChanged;
        }

        private void OnCurrentFilePathChanged(string? filePath)
        {
            Text = $"{MainProgram.ProgramTitle} - {(string.IsNullOrEmpty(filePath) ? "*.*" : new FileInfo(filePath).Name)}";
        }

        public void SetCanvasTextSize(int textSize)
        {
            CanvasTextSize = textSize;

            Font font = new("Consolas", textSize, FontStyle.Regular);

            foreach (KeyValuePair<Point, Label> pair in CharLabels)
            {
                pair.Value.Font = font;
                pair.Value.Size = new Size(CanvasTextSize + 1, CanvasTextSize * 2);
                pair.Value.Location = new Point(pair.Key.X * (CanvasTextSize + 2), pair.Key.Y * CanvasTextSize * 2);
            }
        }
        public void SetCanvasSize(int width, int height, int textSize)
        {
            CanvasTextSize = textSize;

            Font font = new("Consolas", textSize, FontStyle.Regular);
            List<Point> coordsToRemove = new();

            foreach (KeyValuePair<Point, Label> pair in CharLabels)
            {
                if (pair.Key.X >= width || pair.Key.Y >= height)
                {
                    coordsToRemove.Add(pair.Key);
                    continue;
                }

                pair.Value.Text = " ";
                pair.Value.Font = font;
                pair.Value.Size = new(CanvasTextSize + 2, CanvasTextSize * 2);
                pair.Value.Location = new(pair.Key.X * (CanvasTextSize + 2), pair.Key.Y * CanvasTextSize * 2);
            }

            foreach (Point coordToRemove in coordsToRemove)
            {
                CharLabels[coordToRemove].Dispose();
                Canvas.Controls.Remove(CharLabels[coordToRemove]);
                CharLabels.Remove(coordToRemove);
            }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Point coords = new(x, y);

                    if (CharLabels.ContainsKey(coords))
                        continue;

                    Label label = new();
                    label.SuspendLayout();

                    label.Text = " ";
                    label.Font = font;
                    label.Size = new(CanvasTextSize + 2, CanvasTextSize * 2);
                    label.Location = new(x * (CanvasTextSize + 2), y * CanvasTextSize * 2);

                    CharLabels.Add(new(x, y), label);
                    Canvas.Controls.Add(label);

                    label.ResumeLayout(true);
                }
        }

        public void DisplayArtFile(ASCIIArtFile artFile)
        {
            Canvas.Hide();
            NoFileOpenLabel.Hide();

            TaskInfoLabel.Text = "Setting up canvas...";
            SetCanvasSize(artFile.Width, artFile.Height, CanvasTextSize);

            TaskInfoLabel.Text = "Displaying art on canvas...";
            for (int i = 0; i < artFile.ArtLayers.Count; i++)
                for (int x = 0; x < artFile.Width; x++)
                    for (int y = 0; y < artFile.Height; y++)
                    {
                        Point coords = new(x, y);

                        if (artFile.ArtLayers[i].Data[x][y] == null)
                            continue;

                        CharLabels[coords].Text = artFile.ArtLayers[i].Data[x][y].ToString();
                    }

            Canvas.Show();
            TaskInfoLabel.Text = "";
        }

        private void NewFileToolStripMenuItem_Click(object sender, EventArgs e)
            => new NewFileDialog().ShowDialog();

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Open ASCII Art File",
                Filter = "ASCII Art Files (*.aaf)|*.aaf|Text Files (*.txt)|*.txt",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };
            openFileDialog.FileOk += (sender, args) => MainProgram.OpenFile(new(openFileDialog.FileName));

            openFileDialog.ShowDialog();
        }

        private void SaveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArtFile == null)
                return;

            if (MainProgram.CurrentFilePath == null || MainProgram.CurrentFilePath == "")
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Title = "Save ASCII Art File",
                    Filter = "Art file (*.aaf)|*.aaf",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath,
                    ValidateNames = true
                };
                saveFileDialog.FileOk += (sender, args) => MainProgram.SaveArtFileToPathAsync(MainProgram.CurrentArtFile, saveFileDialog.FileName);

                saveFileDialog.ShowDialog();

                return;
            }

            MainProgram.SaveArtFileToPathAsync(MainProgram.CurrentArtFile, MainProgram.CurrentFilePath);
        }

        private void SaveAsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArtFile == null)
                return;

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Save ASCII Art File",
                Filter = "Art file (*.aaf)|*.aaf",
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };
            saveFileDialog.FileOk += (sender, args) => MainProgram.SaveArtFileToPathAsync(MainProgram.CurrentArtFile, saveFileDialog.FileName);

            saveFileDialog.ShowDialog();

            return;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
            => Application.Exit();

        private void CopyArtToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TaskInfoLabel.Text = "Copying art to clipboard...";

            try
            {
                MainProgram.CurrentArtFile?.CopyToClipboard();
                TaskInfoLabel.Text = "Copied art to clipboard!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Copy Art To Clipboard: Failed to copy art to clipboard! {ex.Message}");
                MessageBox.Show("Failed to copy art to clipboard!", "Copy Art To Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArtFile == null)
                return;

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Export ASCII Art File As",
                Filter = "Text file (*.txt)|*.txt",
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };
            saveFileDialog.FileOk += (sender, args) => MainProgram.ExportArtFileToPathAsync(MainProgram.CurrentArtFile, saveFileDialog.FileName);

            saveFileDialog.ShowDialog();

            return;
        }
    }
}