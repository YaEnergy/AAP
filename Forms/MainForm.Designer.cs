﻿namespace AAP
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
            toolPanel = new Panel();
            eraserToolButton = new PictureBox();
            drawToolButton = new PictureBox();
            rightDock = new Panel();
            fillDock = new Panel();
            Canvas = new Panel();
            bottomDock = new Panel();
            TaskInfoLabel = new Label();
            menuStrip.SuspendLayout();
            leftDock.SuspendLayout();
            toolPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)eraserToolButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)drawToolButton).BeginInit();
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
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(49, 24);
            editToolStripMenuItem.Text = "Edit";
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
            canvasToolStripMenuItem.Size = new Size(224, 26);
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
            toolOptionsPanel.Dock = DockStyle.Top;
            toolOptionsPanel.Location = new Point(0, 300);
            toolOptionsPanel.MinimumSize = new Size(0, 200);
            toolOptionsPanel.Name = "toolOptionsPanel";
            toolOptionsPanel.Size = new Size(250, 200);
            toolOptionsPanel.TabIndex = 5;
            // 
            // toolPanel
            // 
            toolPanel.BackColor = SystemColors.ActiveCaption;
            toolPanel.Controls.Add(eraserToolButton);
            toolPanel.Controls.Add(drawToolButton);
            toolPanel.Dock = DockStyle.Top;
            toolPanel.Location = new Point(0, 0);
            toolPanel.Name = "toolPanel";
            toolPanel.Size = new Size(250, 300);
            toolPanel.TabIndex = 4;
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
            // 
            // drawToolButton
            // 
            drawToolButton.BackColor = SystemColors.Control;
            drawToolButton.Image = Properties.Resources.DrawToolIcon;
            drawToolButton.Location = new Point(4, 4);
            drawToolButton.Margin = new Padding(4);
            drawToolButton.Name = "drawToolButton";
            drawToolButton.Padding = new Padding(4);
            drawToolButton.Size = new Size(64, 64);
            drawToolButton.SizeMode = PictureBoxSizeMode.Zoom;
            drawToolButton.TabIndex = 0;
            drawToolButton.TabStop = false;
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
            toolPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)eraserToolButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)drawToolButton).EndInit();
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
        private Panel toolPanel;
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
    }
}