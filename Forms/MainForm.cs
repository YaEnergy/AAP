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
        }

        public void SetCanvasTextSize(int textSize)
        {
            CanvasTextSize = textSize;

            Font font = new Font("Consolas", textSize, FontStyle.Regular);

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

            Font font = new Font("Consolas", textSize, FontStyle.Regular);
            List<Point> coordsToRemove = new List<Point>();

            foreach (KeyValuePair<Point, Label> pair in CharLabels)
            {
                if (pair.Key.X >= width || pair.Key.Y >= height)
                {
                    coordsToRemove.Add(pair.Key);
                    continue;
                }

                pair.Value.Text = " ";
                pair.Value.Font = font;
                pair.Value.Size = new Size(CanvasTextSize + 2, CanvasTextSize * 2);
                pair.Value.Location = new Point(pair.Key.X * (CanvasTextSize + 2), pair.Key.Y * CanvasTextSize * 2);
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
                    Point coords = new Point(x, y);

                    if (CharLabels.ContainsKey(coords))
                        continue;

                    Label label = new Label();
                    label.SuspendLayout();

                    label.Text = " ";
                    label.Font = font;
                    label.Size = new Size(CanvasTextSize + 2, CanvasTextSize * 2);
                    label.Location = new Point(x * (CanvasTextSize + 2), y * CanvasTextSize * 2);

                    CharLabels.Add(new Point(x, y), label);
                    Canvas.Controls.Add(label);

                    label.ResumeLayout(true);
                }
        }

        public void DisplayArtFile(ASCIIArtFile artFile)
        {
            Canvas.Hide();
            NoFileOpenLabel.Hide();

            SetCanvasSize(artFile.Width, artFile.Height, CanvasTextSize);

            for(int i = 0; i < artFile.ArtLayers.Count; i++)
                for (int x = 0; x < artFile.Width; x++)
                    for (int y = 0; y < artFile.Height; y++)
                    {
                        Point coords = new(x, y);

                        if (artFile.ArtLayers[i].Data[x][y] == null)
                            continue;

                        CharLabels[coords].Text = artFile.ArtLayers[i].Data[x][y].ToString();
                    }

            Canvas.Show();
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