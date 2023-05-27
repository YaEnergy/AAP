using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AAP
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private readonly static int DefaultCanvasTextSize = 12;
        private readonly static int DefaultHighlightRectangleThickness = 4;
        private readonly static SolidBrush CanvasArtBrush = new(Color.Black);
        private readonly static Point CanvasArtOffset = new(8, 8);

        private SizeF trueCanvasSize = new(0f, 0f);
        private Font canvasArtFont;

        private int canvasTextSize = 12;
        private int highlightRectangleThickness = 4;
        public int CanvasTextSize { get => canvasTextSize; private set { canvasTextSize = Math.Clamp(value, 4, 128); Canvas.Refresh(); Console.WriteLine("Canvas Text Size: " + canvasTextSize); zoomToolStripMenuItem.Text = $"Zoom: {Math.Truncate((float)canvasTextSize / DefaultCanvasTextSize * 100)}%"; } }
        public int HighlightRectangleThickness
        {
            get => highlightRectangleThickness;
            private set
            {
                int val = value;

                if (val < highlightRectangleThickness)
                {
                    Canvas.Invalidate(new Rectangle(highlightRectangle.Location.X - highlightRectangleThickness / 2, highlightRectangle.Location.Y - highlightRectangleThickness / 2, highlightRectangle.Size.Width + highlightRectangleThickness, highlightRectangle.Size.Height + highlightRectangleThickness));
                    Canvas.Invalidate(new Rectangle(oldHighlightRectangle.Location.X - highlightRectangleThickness / 2, oldHighlightRectangle.Location.Y - highlightRectangleThickness / 2, oldHighlightRectangle.Size.Width + highlightRectangleThickness, oldHighlightRectangle.Size.Height + highlightRectangleThickness));
                }

                highlightRectangleThickness = Math.Clamp(value, 1, 12);

                if (val > highlightRectangleThickness)
                {
                    Canvas.Invalidate(new Rectangle(highlightRectangle.Location.X - highlightRectangleThickness / 2, highlightRectangle.Location.Y - highlightRectangleThickness / 2, highlightRectangle.Size.Width + highlightRectangleThickness, highlightRectangle.Size.Height + highlightRectangleThickness));
                    Canvas.Invalidate(new Rectangle(oldHighlightRectangle.Location.X - highlightRectangleThickness / 2, oldHighlightRectangle.Location.Y - highlightRectangleThickness / 2, oldHighlightRectangle.Size.Width + highlightRectangleThickness, oldHighlightRectangle.Size.Height + highlightRectangleThickness));
                }

                Canvas.Update();

                highlightThicknessNumToolStripMenuItem.Text = $"Highlight Thickness: {Math.Truncate((float)highlightRectangleThickness / DefaultHighlightRectangleThickness * 100)}%";
            }
        }

        private Point canvasMousePos = new(0, 0);

        private Rectangle oldHighlightRectangle = new();
        private Rectangle highlightRectangle = new();
        public Rectangle HighlightRectangle
        {
            get => highlightRectangle;
            set
            {
                if (value == highlightRectangle)
                    return;

                oldHighlightRectangle = highlightRectangle;
                highlightRectangle = value;

                Canvas.Invalidate(new Rectangle(highlightRectangle.Location.X - highlightRectangleThickness / 2, highlightRectangle.Location.Y - highlightRectangleThickness / 2, highlightRectangle.Size.Width + highlightRectangleThickness, highlightRectangle.Size.Height + highlightRectangleThickness));
                Canvas.Invalidate(new Rectangle(oldHighlightRectangle.Location.X - highlightRectangleThickness / 2, oldHighlightRectangle.Location.Y - highlightRectangleThickness / 2, oldHighlightRectangle.Size.Width + highlightRectangleThickness, oldHighlightRectangle.Size.Height + highlightRectangleThickness));
                Canvas.Update();
            }
        }

        private Rectangle oldSelectionRectangle = new();
        private Rectangle selectionRectangle = new();
        public Rectangle SelectionRectangle
        {
            get => selectionRectangle;
            set
            {
                if (value == selectionRectangle)
                    return;

                oldSelectionRectangle = selectionRectangle;
                selectionRectangle = value;

                Canvas.Invalidate(new Rectangle(selectionRectangle.Location.X - highlightRectangleThickness / 2, selectionRectangle.Location.Y - highlightRectangleThickness / 2, selectionRectangle.Size.Width + highlightRectangleThickness, selectionRectangle.Size.Height + highlightRectangleThickness));
                Canvas.Invalidate(new Rectangle(oldSelectionRectangle.Location.X - highlightRectangleThickness / 2, oldSelectionRectangle.Location.Y - highlightRectangleThickness / 2, oldSelectionRectangle.Size.Width + highlightRectangleThickness, oldSelectionRectangle.Size.Height + highlightRectangleThickness));
                Canvas.Update();
            }
        }

        public MainForm()
        {
            InitializeComponent();

            UpdateTitle();
            OnCurrentArtChanged(MainProgram.CurrentArt);
            OnCurrentToolTypeChanged(MainProgram.CurrentToolType);

            MainProgram.OnCurrentFilePathChanged += (file) => UpdateTitle();
            MainProgram.OnCurrentArtChanged += OnCurrentArtChanged;
            MainProgram.OnCurrentToolTypeChanged += OnCurrentToolTypeChanged;
            MainProgram.OnSelectionChanged += OnSelectionChanged;

            canvasArtFont = new("Consolas", CanvasTextSize, GraphicsUnit.Point);

            zoomToolStripMenuItem.Text = $"Zoom: {Math.Truncate((float)canvasTextSize / DefaultCanvasTextSize * 100)}%";
            highlightThicknessNumToolStripMenuItem.Text = $"Highlight Thickness: {Math.Truncate((float)highlightRectangleThickness / DefaultHighlightRectangleThickness * 100)}%";

            Canvas.MouseDown += (sender, args) => ToolActivateStart(sender, args);
            Canvas.MouseMove += (sender, args) =>
            {
                Point? newMouseArtMatrixPosition = GetArtMatrixPoint(args.Location);
                Point? oldMouseArtMatrixPosition = GetArtMatrixPoint(canvasMousePos);

                if (oldMouseArtMatrixPosition == null || newMouseArtMatrixPosition == null)
                {
                    HighlightRectangle = Rectangle.Empty;
                    return;
                }

                //Is On Canvas?
                if (newMouseArtMatrixPosition.Value.X >= MainProgram.CurrentArt?.Width || newMouseArtMatrixPosition.Value.Y >= MainProgram.CurrentArt?.Height || newMouseArtMatrixPosition.Value.X < 0 || newMouseArtMatrixPosition.Value.Y < 0)
                {
                    HighlightRectangle = Rectangle.Empty;
                    return;
                }

                if (oldMouseArtMatrixPosition.Value == newMouseArtMatrixPosition.Value)
                    return;

                Rectangle? newCanvasCharacterRectangle = GetCanvasCharacterRectangle(newMouseArtMatrixPosition.Value);

                if (newCanvasCharacterRectangle == null)
                    return;

                HighlightRectangle = newCanvasCharacterRectangle.Value;
            };

            Canvas.MouseLeave += (sender, args) => HighlightRectangle = Rectangle.Empty;
        }

        #region Tool Functions
        private void ToolActivateStart(object? sender, MouseEventArgs e)
        {
            Canvas.MouseMove += ToolActivateUpdate;
            Canvas.MouseUp += ToolActivateEnd;

            Point? artMatrixPosition = GetArtMatrixPoint(e.Location);

            if (!artMatrixPosition.HasValue)
                return;

            MainProgram.CurrentTool.ActivateStart(artMatrixPosition.Value);

            Console.WriteLine("Tool activate start!");
        }

        private void ToolActivateUpdate(object? sender, MouseEventArgs e)
        {
            Point? artMatrixPosition = GetArtMatrixPoint(e.Location);

            if (!artMatrixPosition.HasValue)
                return;

            MainProgram.CurrentTool.ActivateUpdate(artMatrixPosition.Value);
        }

        private void ToolActivateEnd(object? sender, MouseEventArgs e)
        {
            Canvas.MouseMove -= ToolActivateUpdate;
            Canvas.MouseUp -= ToolActivateEnd;

            Console.WriteLine("Tool activate end!");
        }
        #endregion

        #region Main Program Events
        private void UpdateTitle()
        {
            string filePath = string.IsNullOrEmpty(MainProgram.CurrentFilePath) ? "*.*" : new FileInfo(MainProgram.CurrentFilePath).Name;
            string size = "?x?";

            if (MainProgram.CurrentArt != null)
                size = MainProgram.CurrentArt.Width + "x" + MainProgram.CurrentArt.Height;

            Text = $"{MainProgram.ProgramTitle} - {filePath} ({size})";
#if DEBUG
            Text += " - DEBUG BUILD";
#endif
        }
        private void OnCurrentArtChanged(ASCIIArt? art)
        {
            Canvas.Refresh();
            UpdateTitle();

            if (art != null)
                art.OnArtChanged += OnArtChanged;

            bool artFileExists = art != null;

            saveFileToolStripMenuItem.Enabled = artFileExists;
            saveAsFileToolStripMenuItem.Enabled = artFileExists;
            exportToolStripMenuItem.Enabled = artFileExists;
            asFileToolStripMenuItem.Enabled = artFileExists;
            copyArtToClipboardToolStripMenuItem.Enabled = artFileExists;
        }

        private void OnCurrentToolTypeChanged(ToolType type)
        {
            Color selectedToolColor = Color.DarkGray;
            Color unselectedToolColor = Color.White;

            drawToolButton.BackColor = type == ToolType.Draw ? selectedToolColor : unselectedToolColor;
            eraserToolButton.BackColor = type == ToolType.Eraser ? selectedToolColor : unselectedToolColor;
            selectToolButton.BackColor = type == ToolType.Select ? selectedToolColor : unselectedToolColor;
            moveToolButton.BackColor = type == ToolType.Move ? selectedToolColor : unselectedToolColor;
            textToolButton.BackColor = type == ToolType.Text ? selectedToolColor : unselectedToolColor;
        }

        private void OnSelectionChanged(Rectangle selection)
        {
            Rectangle? startRectangle = GetCanvasCharacterRectangle(selection.Location);
            Rectangle? endRectangle = GetCanvasCharacterRectangle(selection.Location + selection.Size);

            if (startRectangle == null)
                return;

            if (endRectangle == null)
                return;

            int startX = startRectangle.Value.X < endRectangle.Value.X ? startRectangle.Value.X : endRectangle.Value.X;
            int startY = startRectangle.Value.Y < endRectangle.Value.Y ? startRectangle.Value.Y : endRectangle.Value.Y;
            int sizeX = startRectangle.Value.X < endRectangle.Value.X ? endRectangle.Value.X - startRectangle.Value.X + endRectangle.Value.Width / 2 : startRectangle.Value.X - endRectangle.Value.X;
            int sizeY = startRectangle.Value.Y < endRectangle.Value.Y ? endRectangle.Value.Y - startRectangle.Value.Y : startRectangle.Value.Y - endRectangle.Value.Y;

            SelectionRectangle = new(new(startX, startY), new(sizeX, sizeY));
        }
        #endregion

        #region Canvas

        public Point? GetArtMatrixPoint(Point canvasPosition)
        {
            if (MainProgram.CurrentArt == null)
                return null;

            SizeF nonOffsetCanvasSize = trueCanvasSize - new SizeF(CanvasArtOffset.X * 2, CanvasArtOffset.Y * 2);

            PointF artMatrixFloatPos = new((canvasPosition.X + CanvasArtOffset.X) / (nonOffsetCanvasSize.Width / MainProgram.CurrentArt.Width) - 1f, (canvasPosition.Y + CanvasArtOffset.Y + (canvasArtFont.Height / 2)) / canvasArtFont.Height - 1f);

            Point artMatrixPos = Point.Truncate(artMatrixFloatPos);

            return artMatrixPos;
        }

        public Rectangle? GetCanvasCharacterRectangle(Point artMatrixPosition)
        {
            if (MainProgram.CurrentArt == null)
                return null;

            SizeF nonOffsetCanvasSize = trueCanvasSize - new SizeF(CanvasArtOffset.X * 2, CanvasArtOffset.Y * 2);

            PointF canvasFloatPos = new(artMatrixPosition.X * (nonOffsetCanvasSize.Width / MainProgram.CurrentArt.Width) + 1, artMatrixPosition.Y * canvasArtFont.Height + CanvasArtOffset.Y);

            Point canvasPos = Point.Truncate(canvasFloatPos);

            return new(canvasPos, TextRenderer.MeasureText(ASCIIArt.EMPTYCHARACTER.ToString(), canvasArtFont));
        }

        private void Canvas_Paint(object sender, PaintEventArgs args)
        {
            if (sender is not Panel canvas)
                throw new Exception("Sender is not a panel!");

            canvasArtFont.Dispose();
            canvasArtFont = new("Consolas", CanvasTextSize, GraphicsUnit.Point);

            if (MainProgram.CurrentArt == null)
            {
                string noFileOpenText = "No File Open!";
                canvas.Size = new(CanvasTextSize * noFileOpenText.Length, canvasArtFont.Height);

                args.Graphics.DrawString("No File Open!", canvasArtFont, CanvasArtBrush, new Point(0, 0));
                return;
            }

            args.Graphics.DrawRectangle(new(Canvas.BackColor, highlightRectangleThickness), oldHighlightRectangle);
            args.Graphics.DrawRectangle(new(Canvas.BackColor, highlightRectangleThickness), oldSelectionRectangle);

            args.Graphics.DrawRectangle(new(Color.Orange, highlightRectangleThickness), selectionRectangle);
            args.Graphics.DrawRectangle(new(Color.Blue, highlightRectangleThickness), highlightRectangle);

            string artString = MainProgram.CurrentArt.GetArtString();

            string[] lines = artString.Split('\n');

            SizeF size = args.Graphics.MeasureString(artString, canvasArtFont);

            trueCanvasSize = new SizeF(size.Width + CanvasArtOffset.X * 2, size.Height + CanvasArtOffset.Y * 2 + canvasArtFont.Height * 2);
            canvas.Size = trueCanvasSize.ToSize();

            for (int y = 0; y < lines.Length; y++)
            {
                PointF position = new(CanvasArtOffset.X / 2f, canvasArtFont.Height * y + CanvasArtOffset.Y);

                if (!args.Graphics.IsVisible(position))
                    continue;

                args.Graphics.DrawString(lines[y], canvasArtFont, CanvasArtBrush, position);
            }
        }

        private void OnArtChanged(int layerIndex, Point artMatrixPosition, char character)
        {
            Rectangle? canvasCharacterRect = GetCanvasCharacterRectangle(artMatrixPosition);

            if (!canvasCharacterRect.HasValue)
                return;

            Canvas.Invalidate(canvasCharacterRect.Value);

            UpdateTitle();
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

        #region ToolStripMenuItem Functions
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
            openFileDialog.FileOk += OnFileOk;

            void OnFileOk(object? sender, CancelEventArgs args)
            {
                Exception? ex = MainProgram.OpenFile(new(openFileDialog.FileName));

                if (ex != null)
                    MessageBox.Show($"An error has occurred while opening art file ({openFileDialog.FileName})! Exception: {ex.Message}", "Open File", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            openFileDialog.ShowDialog();
        }

        private void SaveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArt == null)
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
            if (MainProgram.CurrentArt == null)
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
            if (MainProgram.CurrentArt == null)
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

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
            => CanvasTextSize += 4;

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
            => CanvasTextSize -= 4;

        private void resetZoomToolStripMenuItem_Click(object sender, EventArgs e)
            => CanvasTextSize = DefaultCanvasTextSize;

        private void increaseThicknessToolStripMenuItem_Click(object sender, EventArgs e)
            => HighlightRectangleThickness += 1;

        private void decreaseThicknessToolStripMenuItem_Click(object sender, EventArgs e)
            => HighlightRectangleThickness -= 1;

        private void resetThicknessToolStripMenuItem_Click(object sender, EventArgs e)
            => HighlightRectangleThickness = DefaultHighlightRectangleThickness;

        private void cropArtToSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArt == null)
                return;

            if (MainProgram.Selected == Rectangle.Empty)
                return;

            MainProgram.CropArtFileToSelected();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgram.CurrentArt == null)
                return;

            MainProgram.Selected = new(0, 0, MainProgram.CurrentArt.Width, MainProgram.CurrentArt.Height);
        }

        #endregion
        #region Tool Buttons
        private void drawToolButton_Click(object sender, EventArgs e)
            => MainProgram.CurrentToolType = ToolType.Draw;

        private void eraserToolButton_Click(object sender, EventArgs e)
            => MainProgram.CurrentToolType = ToolType.Eraser;

        private void selectToolButton_Click(object sender, EventArgs e)
            => MainProgram.CurrentToolType = ToolType.Select;

        private void moveToolButton_Click(object sender, EventArgs e)
            => MainProgram.CurrentToolType = ToolType.Move;

        private void textToolButton_Click(object sender, EventArgs e)
            => MainProgram.CurrentToolType = ToolType.Text;

        #endregion
    }
}