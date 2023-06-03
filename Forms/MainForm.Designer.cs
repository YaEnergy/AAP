namespace AAP
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newFileToolStripMenuItem = new ToolStripMenuItem();
            openFileToolStripMenuItem = new ToolStripMenuItem();
            saveFileToolStripMenuItem = new ToolStripMenuItem();
            saveAsFileToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            asFileToolStripMenuItem = new ToolStripMenuItem();
            copyArtToClipboardToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            undoToolStripMenuItem = new ToolStripMenuItem();
            redoToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            cutSelectionToolStripMenuItem = new ToolStripMenuItem();
            copySelectionToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            deleteSelectionToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            selectAllToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            cropArtToSelectionToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            canvasToolStripMenuItem = new ToolStripMenuItem();
            zoomToolStripMenuItem = new ToolStripMenuItem();
            zoomInToolStripMenuItem = new ToolStripMenuItem();
            zoomOutToolStripMenuItem = new ToolStripMenuItem();
            resetZoomToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            highlightThicknessNumToolStripMenuItem = new ToolStripMenuItem();
            increaseThicknessToolStripMenuItem = new ToolStripMenuItem();
            decreaseThicknessToolStripMenuItem = new ToolStripMenuItem();
            resetThicknessToolStripMenuItem = new ToolStripMenuItem();
            filtersToolStripMenuItem = new ToolStripMenuItem();
            leftDock = new Panel();
            toolOptionsPanel = new Panel();
            characterPalettePanel = new Panel();
            characterPaletteComboBox = new ComboBox();
            characterSelectionPanel = new TableLayoutPanel();
            toolPanel = new TableLayoutPanel();
            textToolButton = new PictureBox();
            drawToolButton = new PictureBox();
            moveToolButton = new PictureBox();
            eraserToolButton = new PictureBox();
            selectToolButton = new PictureBox();
            rightDock = new Panel();
            fillDock = new Panel();
            Canvas = new Panel();
            bottomDock = new Panel();
            TaskInfoLabel = new Label();
            menuStrip.SuspendLayout();
            leftDock.SuspendLayout();
            toolOptionsPanel.SuspendLayout();
            characterPalettePanel.SuspendLayout();
            toolPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)textToolButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)drawToolButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)moveToolButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)eraserToolButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)selectToolButton).BeginInit();
            fillDock.SuspendLayout();
            bottomDock.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, filtersToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1262, 28);
            menuStrip.TabIndex = 2;
            menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newFileToolStripMenuItem, openFileToolStripMenuItem, saveFileToolStripMenuItem, saveAsFileToolStripMenuItem, exportToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // newFileToolStripMenuItem
            // 
            newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
            newFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newFileToolStripMenuItem.Size = new Size(233, 26);
            newFileToolStripMenuItem.Text = "New";
            newFileToolStripMenuItem.Click += NewFileToolStripMenuItem_Click;
            // 
            // openFileToolStripMenuItem
            // 
            openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            openFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openFileToolStripMenuItem.Size = new Size(233, 26);
            openFileToolStripMenuItem.Text = "Open";
            openFileToolStripMenuItem.Click += OpenFileToolStripMenuItem_Click;
            // 
            // saveFileToolStripMenuItem
            // 
            saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            saveFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveFileToolStripMenuItem.Size = new Size(233, 26);
            saveFileToolStripMenuItem.Text = "Save";
            saveFileToolStripMenuItem.Click += SaveFileToolStripMenuItem_Click;
            // 
            // saveAsFileToolStripMenuItem
            // 
            saveAsFileToolStripMenuItem.Name = "saveAsFileToolStripMenuItem";
            saveAsFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsFileToolStripMenuItem.Size = new Size(233, 26);
            saveAsFileToolStripMenuItem.Text = "Save As";
            saveAsFileToolStripMenuItem.Click += SaveAsFileToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { asFileToolStripMenuItem, copyArtToClipboardToolStripMenuItem });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(233, 26);
            exportToolStripMenuItem.Text = "Export";
            // 
            // asFileToolStripMenuItem
            // 
            asFileToolStripMenuItem.Name = "asFileToolStripMenuItem";
            asFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            asFileToolStripMenuItem.Size = new Size(331, 26);
            asFileToolStripMenuItem.Text = "As File";
            asFileToolStripMenuItem.Click += AsFileToolStripMenuItem_Click;
            // 
            // copyArtToClipboardToolStripMenuItem
            // 
            copyArtToClipboardToolStripMenuItem.Name = "copyArtToClipboardToolStripMenuItem";
            copyArtToClipboardToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
            copyArtToClipboardToolStripMenuItem.Size = new Size(331, 26);
            copyArtToClipboardToolStripMenuItem.Text = "Copy Art To Clipboard";
            copyArtToClipboardToolStripMenuItem.Click += CopyArtToClipboardToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(230, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitToolStripMenuItem.Size = new Size(233, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator3, cutSelectionToolStripMenuItem, copySelectionToolStripMenuItem, pasteToolStripMenuItem, deleteSelectionToolStripMenuItem, toolStripSeparator4, selectAllToolStripMenuItem, toolStripSeparator5, cropArtToSelectionToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(49, 24);
            editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            undoToolStripMenuItem.Size = new Size(262, 26);
            undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
            redoToolStripMenuItem.Size = new Size(262, 26);
            redoToolStripMenuItem.Text = "Redo";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(259, 6);
            // 
            // cutSelectionToolStripMenuItem
            // 
            cutSelectionToolStripMenuItem.Name = "cutSelectionToolStripMenuItem";
            cutSelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutSelectionToolStripMenuItem.Size = new Size(262, 26);
            cutSelectionToolStripMenuItem.Text = "Cut Selection";
            // 
            // copySelectionToolStripMenuItem
            // 
            copySelectionToolStripMenuItem.Name = "copySelectionToolStripMenuItem";
            copySelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copySelectionToolStripMenuItem.Size = new Size(262, 26);
            copySelectionToolStripMenuItem.Text = "Copy Selection";
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteToolStripMenuItem.Size = new Size(262, 26);
            pasteToolStripMenuItem.Text = "Paste as new layer";
            // 
            // deleteSelectionToolStripMenuItem
            // 
            deleteSelectionToolStripMenuItem.Name = "deleteSelectionToolStripMenuItem";
            deleteSelectionToolStripMenuItem.ShortcutKeys = Keys.Delete;
            deleteSelectionToolStripMenuItem.Size = new Size(262, 26);
            deleteSelectionToolStripMenuItem.Text = "Delete Selection";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(259, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            selectAllToolStripMenuItem.Size = new Size(262, 26);
            selectAllToolStripMenuItem.Text = "Select All";
            selectAllToolStripMenuItem.Click += selectAllToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(259, 6);
            // 
            // cropArtToSelectionToolStripMenuItem
            // 
            cropArtToSelectionToolStripMenuItem.Name = "cropArtToSelectionToolStripMenuItem";
            cropArtToSelectionToolStripMenuItem.Size = new Size(262, 26);
            cropArtToSelectionToolStripMenuItem.Text = "Crop Art To Selection";
            cropArtToSelectionToolStripMenuItem.Click += cropArtToSelectionToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { canvasToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(55, 24);
            viewToolStripMenuItem.Text = "View";
            // 
            // canvasToolStripMenuItem
            // 
            canvasToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { zoomToolStripMenuItem, zoomInToolStripMenuItem, zoomOutToolStripMenuItem, resetZoomToolStripMenuItem, toolStripSeparator2, highlightThicknessNumToolStripMenuItem, increaseThicknessToolStripMenuItem, decreaseThicknessToolStripMenuItem, resetThicknessToolStripMenuItem });
            canvasToolStripMenuItem.Name = "canvasToolStripMenuItem";
            canvasToolStripMenuItem.Size = new Size(138, 26);
            canvasToolStripMenuItem.Text = "Canvas";
            // 
            // zoomToolStripMenuItem
            // 
            zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            zoomToolStripMenuItem.Size = new Size(268, 26);
            zoomToolStripMenuItem.Text = "Zoom: num%";
            // 
            // zoomInToolStripMenuItem
            // 
            zoomInToolStripMenuItem.Name = "zoomInToolStripMenuItem";
            zoomInToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.I;
            zoomInToolStripMenuItem.Size = new Size(268, 26);
            zoomInToolStripMenuItem.Text = "Zoom in";
            zoomInToolStripMenuItem.Click += zoomInToolStripMenuItem_Click;
            // 
            // zoomOutToolStripMenuItem
            // 
            zoomOutToolStripMenuItem.Name = "zoomOutToolStripMenuItem";
            zoomOutToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.O;
            zoomOutToolStripMenuItem.Size = new Size(268, 26);
            zoomOutToolStripMenuItem.Text = "Zoom out";
            zoomOutToolStripMenuItem.Click += zoomOutToolStripMenuItem_Click;
            // 
            // resetZoomToolStripMenuItem
            // 
            resetZoomToolStripMenuItem.Name = "resetZoomToolStripMenuItem";
            resetZoomToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.P;
            resetZoomToolStripMenuItem.Size = new Size(268, 26);
            resetZoomToolStripMenuItem.Text = "Reset Zoom";
            resetZoomToolStripMenuItem.Click += resetZoomToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(265, 6);
            // 
            // highlightThicknessNumToolStripMenuItem
            // 
            highlightThicknessNumToolStripMenuItem.Name = "highlightThicknessNumToolStripMenuItem";
            highlightThicknessNumToolStripMenuItem.Size = new Size(268, 26);
            highlightThicknessNumToolStripMenuItem.Text = "Highlight Thickness: num%";
            // 
            // increaseThicknessToolStripMenuItem
            // 
            increaseThicknessToolStripMenuItem.Name = "increaseThicknessToolStripMenuItem";
            increaseThicknessToolStripMenuItem.Size = new Size(268, 26);
            increaseThicknessToolStripMenuItem.Text = "Increase thickness";
            increaseThicknessToolStripMenuItem.Click += increaseThicknessToolStripMenuItem_Click;
            // 
            // decreaseThicknessToolStripMenuItem
            // 
            decreaseThicknessToolStripMenuItem.Name = "decreaseThicknessToolStripMenuItem";
            decreaseThicknessToolStripMenuItem.Size = new Size(268, 26);
            decreaseThicknessToolStripMenuItem.Text = "Decrease thickness";
            decreaseThicknessToolStripMenuItem.Click += decreaseThicknessToolStripMenuItem_Click;
            // 
            // resetThicknessToolStripMenuItem
            // 
            resetThicknessToolStripMenuItem.Name = "resetThicknessToolStripMenuItem";
            resetThicknessToolStripMenuItem.Size = new Size(268, 26);
            resetThicknessToolStripMenuItem.Text = "Reset thickness";
            resetThicknessToolStripMenuItem.Click += resetThicknessToolStripMenuItem_Click;
            // 
            // filtersToolStripMenuItem
            // 
            filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
            filtersToolStripMenuItem.Size = new Size(62, 24);
            filtersToolStripMenuItem.Text = "Filters";
            // 
            // leftDock
            // 
            leftDock.AutoScroll = true;
            leftDock.BackColor = SystemColors.ActiveBorder;
            leftDock.Controls.Add(toolOptionsPanel);
            leftDock.Controls.Add(toolPanel);
            leftDock.Dock = DockStyle.Left;
            leftDock.Location = new Point(0, 28);
            leftDock.Name = "leftDock";
            leftDock.Size = new Size(250, 605);
            leftDock.TabIndex = 3;
            // 
            // toolOptionsPanel
            // 
            toolOptionsPanel.AutoSize = true;
            toolOptionsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            toolOptionsPanel.BackColor = SystemColors.Info;
            toolOptionsPanel.Controls.Add(characterPalettePanel);
            toolOptionsPanel.Dock = DockStyle.Top;
            toolOptionsPanel.Location = new Point(0, 144);
            toolOptionsPanel.MinimumSize = new Size(0, 300);
            toolOptionsPanel.Name = "toolOptionsPanel";
            toolOptionsPanel.Size = new Size(250, 300);
            toolOptionsPanel.TabIndex = 5;
            // 
            // characterPalettePanel
            // 
            characterPalettePanel.AutoSize = true;
            characterPalettePanel.BackColor = Color.Cyan;
            characterPalettePanel.Controls.Add(characterPaletteComboBox);
            characterPalettePanel.Controls.Add(characterSelectionPanel);
            characterPalettePanel.Dock = DockStyle.Top;
            characterPalettePanel.Location = new Point(0, 0);
            characterPalettePanel.MinimumSize = new Size(0, 125);
            characterPalettePanel.Name = "characterPalettePanel";
            characterPalettePanel.Size = new Size(250, 133);
            characterPalettePanel.TabIndex = 8;
            // 
            // characterPaletteComboBox
            // 
            characterPaletteComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            characterPaletteComboBox.FormattingEnabled = true;
            characterPaletteComboBox.Location = new Point(0, 0);
            characterPaletteComboBox.Name = "characterPaletteComboBox";
            characterPaletteComboBox.Size = new Size(250, 28);
            characterPaletteComboBox.TabIndex = 8;
            characterPaletteComboBox.SelectionChangeCommitted += characterPaletteComboBox_SelectionChangeCommitted;
            // 
            // characterSelectionPanel
            // 
            characterSelectionPanel.AutoScroll = true;
            characterSelectionPanel.AutoSize = true;
            characterSelectionPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            characterSelectionPanel.BackColor = Color.FromArgb(192, 255, 192);
            characterSelectionPanel.ColumnCount = 4;
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle());
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle());
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle());
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            characterSelectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            characterSelectionPanel.Location = new Point(0, 30);
            characterSelectionPanel.MaximumSize = new Size(250, 700);
            characterSelectionPanel.MinimumSize = new Size(250, 100);
            characterSelectionPanel.Name = "characterSelectionPanel";
            characterSelectionPanel.RowCount = 2;
            characterSelectionPanel.RowStyles.Add(new RowStyle());
            characterSelectionPanel.RowStyles.Add(new RowStyle());
            characterSelectionPanel.Size = new Size(250, 100);
            characterSelectionPanel.TabIndex = 7;
            // 
            // toolPanel
            // 
            toolPanel.AutoSize = true;
            toolPanel.BackColor = Color.FromArgb(255, 192, 255);
            toolPanel.ColumnCount = 3;
            toolPanel.ColumnStyles.Add(new ColumnStyle());
            toolPanel.ColumnStyles.Add(new ColumnStyle());
            toolPanel.ColumnStyles.Add(new ColumnStyle());
            toolPanel.Controls.Add(textToolButton, 1, 1);
            toolPanel.Controls.Add(drawToolButton, 0, 0);
            toolPanel.Controls.Add(moveToolButton, 0, 1);
            toolPanel.Controls.Add(eraserToolButton, 1, 0);
            toolPanel.Controls.Add(selectToolButton, 2, 0);
            toolPanel.Dock = DockStyle.Top;
            toolPanel.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
            toolPanel.Location = new Point(0, 0);
            toolPanel.Name = "toolPanel";
            toolPanel.RowCount = 2;
            toolPanel.RowStyles.Add(new RowStyle());
            toolPanel.RowStyles.Add(new RowStyle());
            toolPanel.Size = new Size(250, 144);
            toolPanel.TabIndex = 6;
            // 
            // textToolButton
            // 
            textToolButton.BackColor = SystemColors.Control;
            textToolButton.Image = Properties.Resources.TextToolIcon;
            textToolButton.Location = new Point(76, 76);
            textToolButton.Margin = new Padding(4);
            textToolButton.Name = "textToolButton";
            textToolButton.Padding = new Padding(4);
            textToolButton.Size = new Size(64, 64);
            textToolButton.SizeMode = PictureBoxSizeMode.Zoom;
            textToolButton.TabIndex = 4;
            textToolButton.TabStop = false;
            textToolButton.Click += textToolButton_Click;
            // 
            // drawToolButton
            // 
            drawToolButton.BackColor = SystemColors.Control;
            drawToolButton.Dock = DockStyle.Fill;
            drawToolButton.Image = Properties.Resources.DrawToolIcon;
            drawToolButton.Location = new Point(4, 4);
            drawToolButton.Margin = new Padding(4);
            drawToolButton.Name = "drawToolButton";
            drawToolButton.Padding = new Padding(4);
            drawToolButton.Size = new Size(64, 64);
            drawToolButton.SizeMode = PictureBoxSizeMode.Zoom;
            drawToolButton.TabIndex = 0;
            drawToolButton.TabStop = false;
            drawToolButton.Click += drawToolButton_Click;
            // 
            // moveToolButton
            // 
            moveToolButton.BackColor = SystemColors.Control;
            moveToolButton.Location = new Point(4, 76);
            moveToolButton.Margin = new Padding(4);
            moveToolButton.Name = "moveToolButton";
            moveToolButton.Padding = new Padding(4);
            moveToolButton.Size = new Size(64, 64);
            moveToolButton.SizeMode = PictureBoxSizeMode.Zoom;
            moveToolButton.TabIndex = 3;
            moveToolButton.TabStop = false;
            moveToolButton.Click += moveToolButton_Click;
            // 
            // eraserToolButton
            // 
            eraserToolButton.BackColor = SystemColors.Control;
            eraserToolButton.Image = Properties.Resources.EraserToolIcon;
            eraserToolButton.Location = new Point(76, 4);
            eraserToolButton.Margin = new Padding(4);
            eraserToolButton.Name = "eraserToolButton";
            eraserToolButton.Padding = new Padding(4);
            eraserToolButton.Size = new Size(64, 64);
            eraserToolButton.SizeMode = PictureBoxSizeMode.Zoom;
            eraserToolButton.TabIndex = 1;
            eraserToolButton.TabStop = false;
            eraserToolButton.Click += eraserToolButton_Click;
            // 
            // selectToolButton
            // 
            selectToolButton.BackColor = SystemColors.Control;
            selectToolButton.Location = new Point(148, 4);
            selectToolButton.Margin = new Padding(4);
            selectToolButton.Name = "selectToolButton";
            selectToolButton.Padding = new Padding(4);
            selectToolButton.Size = new Size(64, 64);
            selectToolButton.SizeMode = PictureBoxSizeMode.Zoom;
            selectToolButton.TabIndex = 2;
            selectToolButton.TabStop = false;
            selectToolButton.Click += selectToolButton_Click;
            // 
            // rightDock
            // 
            rightDock.BackColor = SystemColors.ActiveBorder;
            rightDock.Dock = DockStyle.Right;
            rightDock.Location = new Point(1012, 28);
            rightDock.Name = "rightDock";
            rightDock.Size = new Size(250, 605);
            rightDock.TabIndex = 4;
            // 
            // fillDock
            // 
            fillDock.AutoScroll = true;
            fillDock.AutoScrollMargin = new Size(20, 20);
            fillDock.BackColor = SystemColors.ControlLight;
            fillDock.Controls.Add(Canvas);
            fillDock.Dock = DockStyle.Fill;
            fillDock.Location = new Point(250, 28);
            fillDock.Name = "fillDock";
            fillDock.Size = new Size(762, 605);
            fillDock.TabIndex = 5;
            // 
            // Canvas
            // 
            Canvas.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Canvas.BackColor = SystemColors.InactiveBorder;
            Canvas.BorderStyle = BorderStyle.FixedSingle;
            Canvas.Location = new Point(3, 3);
            Canvas.Name = "Canvas";
            Canvas.Size = new Size(148, 27);
            Canvas.TabIndex = 1;
            Canvas.Paint += Canvas_Paint;
            // 
            // bottomDock
            // 
            bottomDock.BackColor = SystemColors.ButtonFace;
            bottomDock.Controls.Add(TaskInfoLabel);
            bottomDock.Dock = DockStyle.Bottom;
            bottomDock.Location = new Point(0, 633);
            bottomDock.Name = "bottomDock";
            bottomDock.Size = new Size(1262, 40);
            bottomDock.TabIndex = 2;
            // 
            // TaskInfoLabel
            // 
            TaskInfoLabel.Dock = DockStyle.Fill;
            TaskInfoLabel.Location = new Point(0, 0);
            TaskInfoLabel.Margin = new Padding(3);
            TaskInfoLabel.Name = "TaskInfoLabel";
            TaskInfoLabel.Padding = new Padding(5);
            TaskInfoLabel.Size = new Size(1262, 40);
            TaskInfoLabel.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonFace;
            ClientSize = new Size(1262, 673);
            Controls.Add(fillDock);
            Controls.Add(rightDock);
            Controls.Add(leftDock);
            Controls.Add(menuStrip);
            Controls.Add(bottomDock);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            Text = "ASCII Art Program";
            WindowState = FormWindowState.Maximized;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            leftDock.ResumeLayout(false);
            leftDock.PerformLayout();
            toolOptionsPanel.ResumeLayout(false);
            toolOptionsPanel.PerformLayout();
            characterPalettePanel.ResumeLayout(false);
            characterPalettePanel.PerformLayout();
            toolPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)textToolButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)drawToolButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)moveToolButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)eraserToolButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)selectToolButton).EndInit();
            fillDock.ResumeLayout(false);
            bottomDock.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newFileToolStripMenuItem;
        private ToolStripMenuItem openFileToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem filtersToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem saveFileToolStripMenuItem;
        private ToolStripMenuItem saveAsFileToolStripMenuItem;
        private Panel leftDock;
        private Panel toolOptionsPanel;
        private Panel rightDock;
        private Panel fillDock;
        public Panel Canvas;
        private Panel bottomDock;
        public Label TaskInfoLabel;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem asFileToolStripMenuItem;
        private ToolStripMenuItem copyArtToClipboardToolStripMenuItem;
        private PictureBox eraserToolButton;
        private PictureBox drawToolButton;
        private ToolStripMenuItem canvasToolStripMenuItem;
        private ToolStripMenuItem zoomInToolStripMenuItem;
        private ToolStripMenuItem zoomOutToolStripMenuItem;
        private ToolStripMenuItem resetZoomToolStripMenuItem;
        private ToolStripMenuItem zoomToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem highlightThicknessNumToolStripMenuItem;
        private ToolStripMenuItem increaseThicknessToolStripMenuItem;
        private ToolStripMenuItem decreaseThicknessToolStripMenuItem;
        private ToolStripMenuItem resetThicknessToolStripMenuItem;
        private PictureBox selectToolButton;
        private PictureBox textToolButton;
        private PictureBox moveToolButton;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem cutSelectionToolStripMenuItem;
        private ToolStripMenuItem copySelectionToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripMenuItem deleteSelectionToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem cropArtToSelectionToolStripMenuItem;
        private TableLayoutPanel toolPanel;
        private TableLayoutPanel characterSelectionPanel;
        private Panel characterPalettePanel;
        private ComboBox characterPaletteComboBox;
    }
}