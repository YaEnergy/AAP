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
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newFileToolStripMenuItem = new ToolStripMenuItem();
            openFileToolStripMenuItem = new ToolStripMenuItem();
            saveFileToolStripMenuItem = new ToolStripMenuItem();
            saveAsFileToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            filtersToolStripMenuItem = new ToolStripMenuItem();
            windowsToolStripMenuItem = new ToolStripMenuItem();
            leftDock = new Panel();
            toolOptionsPanel = new Panel();
            toolPanel = new Panel();
            rightDock = new Panel();
            menuStrip.SuspendLayout();
            leftDock.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, filtersToolStripMenuItem, windowsToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1262, 28);
            menuStrip.TabIndex = 2;
            menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newFileToolStripMenuItem, openFileToolStripMenuItem, saveFileToolStripMenuItem, saveAsFileToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // newFileToolStripMenuItem
            // 
            newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
            newFileToolStripMenuItem.Size = new Size(143, 26);
            newFileToolStripMenuItem.Text = "New";
            // 
            // openFileToolStripMenuItem
            // 
            openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            openFileToolStripMenuItem.Size = new Size(143, 26);
            openFileToolStripMenuItem.Text = "Open";
            // 
            // saveFileToolStripMenuItem
            // 
            saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            saveFileToolStripMenuItem.Size = new Size(143, 26);
            saveFileToolStripMenuItem.Text = "Save";
            // 
            // saveAsFileToolStripMenuItem
            // 
            saveAsFileToolStripMenuItem.Name = "saveAsFileToolStripMenuItem";
            saveAsFileToolStripMenuItem.Size = new Size(143, 26);
            saveAsFileToolStripMenuItem.Text = "Save As";
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(49, 24);
            editToolStripMenuItem.Text = "Edit";
            // 
            // filtersToolStripMenuItem
            // 
            filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
            filtersToolStripMenuItem.Size = new Size(62, 24);
            filtersToolStripMenuItem.Text = "Filters";
            // 
            // windowsToolStripMenuItem
            // 
            windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            windowsToolStripMenuItem.Size = new Size(84, 24);
            windowsToolStripMenuItem.Text = "Windows";
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
            leftDock.Size = new Size(250, 645);
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
            toolPanel.Dock = DockStyle.Top;
            toolPanel.Location = new Point(0, 0);
            toolPanel.Name = "toolPanel";
            toolPanel.Size = new Size(250, 300);
            toolPanel.TabIndex = 4;
            // 
            // rightDock
            // 
            rightDock.BackColor = SystemColors.ActiveBorder;
            rightDock.Dock = DockStyle.Right;
            rightDock.Location = new Point(1012, 28);
            rightDock.Name = "rightDock";
            rightDock.Size = new Size(250, 645);
            rightDock.TabIndex = 4;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonFace;
            ClientSize = new Size(1262, 673);
            Controls.Add(rightDock);
            Controls.Add(leftDock);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            Text = "ASCII Art Program";
            WindowState = FormWindowState.Maximized;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            leftDock.ResumeLayout(false);
            leftDock.PerformLayout();
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
        private ToolStripMenuItem windowsToolStripMenuItem;
        private ToolStripMenuItem saveFileToolStripMenuItem;
        private ToolStripMenuItem saveAsFileToolStripMenuItem;
        private Panel leftDock;
        private Panel toolOptionsPanel;
        private Panel toolPanel;
        private Panel rightDock;
    }
}