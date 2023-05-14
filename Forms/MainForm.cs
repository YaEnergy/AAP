using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AAP
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private readonly static SolidBrush CanvasArtBrush = new(Color.Black);
        private readonly static Point CanvasArtOffset = new(8, 8);
        private SizeF trueCanvasSize = new(0f, 0f);
        private Font canvasArtFont;
        public int CanvasTextSize { get; private set; } = 12;

        public MainForm()
        {
            InitializeComponent();

            OnCurrentFilePathChanged(null);

            MainProgram.OnCurrentFilePathChanged += OnCurrentFilePathChanged;

            canvasArtFont = new("Consolas", CanvasTextSize, GraphicsUnit.Point);

            Canvas.MouseDown += (sender, args) => ToolActivateStart(sender, args);
        }

        #region Tool Functions
        private void ToolActivateStart(object? sender, MouseEventArgs e)
        {
            Point? artMatrixPosition = GetArtMatrixPoint(e.Location);

            if (!artMatrixPosition.HasValue)
                return;

            Canvas.MouseMove += ToolActivateUpdate;
            Canvas.MouseUp += ToolActivateEnd;

            MainProgram.CurrentTool.ActivateStart(artMatrixPosition.Value);

            Console.WriteLine("Tool activate start!");
        }

        private void ToolActivateUpdate(object? sender, MouseEventArgs e)
        {
            Point? artMatrixPosition = GetArtMatrixPoint(e.Location);

            if (!artMatrixPosition.HasValue)
                return;

            MainProgram.CurrentTool.ActivateUpdate(artMatrixPosition.Value);

            Console.WriteLine("Tool activate update!");
        }

        private void ToolActivateEnd(object? sender, MouseEventArgs e)
        {
            Canvas.MouseMove -= ToolActivateUpdate;
            Canvas.MouseUp -= ToolActivateEnd;

            Console.WriteLine("Tool activate end!");
        }
        #endregion

        private void OnCurrentFilePathChanged(string? filePath)
        {
            Text = $"{MainProgram.ProgramTitle} - {(string.IsNullOrEmpty(filePath) ? "*.*" : new FileInfo(filePath).Name)} ({(MainProgram.CurrentArtFile == null ? "?" : MainProgram.CurrentArtFile.Width)}x{(MainProgram.CurrentArtFile == null ? "?" : MainProgram.CurrentArtFile.Height)})";
        }

        #region Canvas
        public void DisplayArt()
        {
            Canvas.Hide();

            TaskInfoLabel.Text = "Setting up canvas...";

            Canvas.Update();

            Canvas.Show();
            TaskInfoLabel.Text = "";
        }

        public Point? GetArtMatrixPoint(Point canvasPosition)
        {
            if (MainProgram.CurrentArtFile == null)
                return null;

            SizeF nonOffsetCanvasSize = trueCanvasSize - new SizeF(CanvasArtOffset.X * 2, CanvasArtOffset.Y * 2);
            PointF artMatrixFloatPos = new((canvasPosition.X + CanvasArtOffset.X) / (nonOffsetCanvasSize.Width / MainProgram.CurrentArtFile.Width) - 1f, (canvasPosition.Y + CanvasArtOffset.Y) / (nonOffsetCanvasSize.Height / MainProgram.CurrentArtFile.Height) - 1f);

            Point artMatrixPos = new(Convert.ToInt32(Math.Floor(artMatrixFloatPos.X)), Convert.ToInt32(Math.Floor(artMatrixFloatPos.Y)));

            Console.WriteLine(artMatrixPos);

            return artMatrixPos;
        }
        private void Canvas_Paint(object sender, PaintEventArgs args)
        {
            if (sender is not Panel canvas)
                throw new Exception("Sender is not a panel!");

            canvasArtFont = new("Consolas", CanvasTextSize, GraphicsUnit.Point);

            if (MainProgram.CurrentArtFile == null)
            {
                string noFileOpenText = "No File Open!";
                canvas.Size = new(CanvasTextSize * noFileOpenText.Length, canvasArtFont.Height);

                args.Graphics.DrawString("No File Open!", canvasArtFont, CanvasArtBrush, new Point(0, 0));
                return;
            }

            string artString = MainProgram.CurrentArtFile.GetArtString();

            string[] lines = artString.Split('\n');

            SizeF size = args.Graphics.MeasureString(artString, canvasArtFont);

            trueCanvasSize = new SizeF(size.Width + CanvasArtOffset.X * 2, size.Height + CanvasArtOffset.Y * 2);
            canvas.Size = trueCanvasSize.ToSize();

            for (int y = 0; y < lines.Length; y++)
            {
                PointF position = new(CanvasArtOffset.X / 2f, canvasArtFont.Height * y + CanvasArtOffset.Y);

                if (!args.Graphics.IsVisible(position))
                    continue;

                args.Graphics.DrawString(lines[y], canvasArtFont, CanvasArtBrush, position);
            }
        }
        #endregion
        #region Background Run Worker Complete Functions
        void BackgroundSaveComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Cancelled)
            {
                Console.WriteLine("Save File: Art file save cancelled!");
                MessageBox.Show("Cancelled saving art file!", "Save File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (args.Error != null)
            {
                Console.WriteLine("Save File: An error has occurred while saving art file! Exception: " + args.Error.Message);
                MessageBox.Show("An error has occurred while saving art file!\nException: " + args.Error.Message, "Save File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Save Art File did not return file info!");

                MessageBox.Show("Saved art file to " + fileInfo.FullName + "!", "Save File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void BackgroundExportComplete(object? sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Cancelled)
            {
                Console.WriteLine("Export File: Art file export cancelled!");
                MessageBox.Show("Cancelled exporting art file!", "Export File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (args.Error != null)
            {
                Console.WriteLine("Export File: An error has occurred while exporting art file! Exception: " + args.Error.Message);
                MessageBox.Show("An error has occurred while exporting art file!\nException: " + args.Error.Message, "Export File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (args.Result is not FileInfo fileInfo)
                    throw new Exception("Background Worker Export Art File did not return file info!");

                MessageBox.Show("Exported art file to " + fileInfo.FullName + "!", "Export File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start("explorer.exe", fileInfo.DirectoryName ?? MainProgram.DefaultArtFilesDirectoryPath);
            }
        }
        #endregion

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
                saveFileDialog.FileOk += OnFileOk;

                void OnFileOk(object? sender, CancelEventArgs args)
                {
                    BackgroundWorker? bgWorker = MainProgram.SaveArtFileToPathAsync(saveFileDialog.FileName);

                    if (bgWorker == null)
                        return;

                    bgWorker.RunWorkerCompleted += BackgroundSaveComplete;
                }

                saveFileDialog.ShowDialog();

                return;
            }

            BackgroundWorker? bgWorker = MainProgram.SaveArtFileToPathAsync(MainProgram.CurrentFilePath);

            if (bgWorker == null)
                return;

            bgWorker.RunWorkerCompleted += BackgroundSaveComplete;
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
            saveFileDialog.FileOk += OnFileOk;

            void OnFileOk(object? sender, CancelEventArgs args)
            {
                BackgroundWorker? bgWorker = MainProgram.SaveArtFileToPathAsync(saveFileDialog.FileName);

                if (bgWorker == null)
                    return;

                bgWorker.RunWorkerCompleted += SaveComplete;

                void SaveComplete(object? sender, RunWorkerCompletedEventArgs args)
                {
                    if (args.Cancelled)
                    {
                        Console.WriteLine("Save File: Art file save to " + saveFileDialog.FileName + " cancelled!");
                        MessageBox.Show("Cancelled saving " + saveFileDialog.FileName + "!", "Save File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (args.Error != null)
                    {
                        Console.WriteLine("Save File: An error has occurred while saving art file to " + saveFileDialog.FileName + "! Exception: " + args.Error.Message);
                        MessageBox.Show("An error has occurred while saving art file to " + saveFileDialog.FileName + "!\nException: " + args.Error.Message, "Save File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Console.WriteLine("Save File: Art file saved to " + saveFileDialog.FileName + "!");
                        MessageBox.Show("Saved art file to " + saveFileDialog.FileName + "!", "Save File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }

            saveFileDialog.ShowDialog();

            return;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
            => Application.Exit();

        private void CopyArtToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainProgram.CopyArtFileToClipboard();

            MessageBox.Show("Copied art to clipboard!", "Copy To Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArtFile == null)
                return;

            SaveFileDialog saveFileDialog = new()
            {
                Title = "Save ASCII Art File",
                Filter = "Text Files (*.txt)|*.txt",
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = MainProgram.DefaultArtFilesDirectoryPath,
                ValidateNames = true
            };
            saveFileDialog.FileOk += OnFileOk;

            void OnFileOk(object? sender, CancelEventArgs args)
            {
                BackgroundWorker? bgWorker = MainProgram.ExportArtFileToPathAsync(saveFileDialog.FileName);

                if (bgWorker == null)
                    return;

                bgWorker.RunWorkerCompleted += BackgroundExportComplete;
            }

            saveFileDialog.ShowDialog();

            return;
        }
    }
}