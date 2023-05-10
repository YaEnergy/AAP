namespace AAP
{
    partial class NewFileDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            CreateButton = new Button();
            OptionsPanel = new FlowLayoutPanel();
            SizePanel = new Panel();
            xLabel = new Label();
            HeightTextBox = new TextBox();
            WidthTextBox = new TextBox();
            sizeLabel = new Label();
            AddBackgroundLayerCheckBox = new CheckBox();
            optionLabel = new Label();
            OptionsPanel.SuspendLayout();
            SizePanel.SuspendLayout();
            SuspendLayout();
            // 
            // CreateButton
            // 
            CreateButton.Location = new Point(12, 112);
            CreateButton.Name = "CreateButton";
            CreateButton.Size = new Size(258, 29);
            CreateButton.TabIndex = 0;
            CreateButton.Text = "Create";
            CreateButton.UseVisualStyleBackColor = true;
            CreateButton.Click += CreateButton_Click;
            // 
            // OptionsPanel
            // 
            OptionsPanel.AutoScroll = true;
            OptionsPanel.Controls.Add(SizePanel);
            OptionsPanel.Controls.Add(AddBackgroundLayerCheckBox);
            OptionsPanel.Location = new Point(12, 34);
            OptionsPanel.Name = "OptionsPanel";
            OptionsPanel.Size = new Size(258, 72);
            OptionsPanel.TabIndex = 1;
            // 
            // SizePanel
            // 
            SizePanel.BackColor = SystemColors.Control;
            SizePanel.Controls.Add(xLabel);
            SizePanel.Controls.Add(HeightTextBox);
            SizePanel.Controls.Add(WidthTextBox);
            SizePanel.Controls.Add(sizeLabel);
            SizePanel.Location = new Point(3, 3);
            SizePanel.Name = "SizePanel";
            SizePanel.Size = new Size(250, 27);
            SizePanel.TabIndex = 0;
            // 
            // xLabel
            // 
            xLabel.AutoSize = true;
            xLabel.Location = new Point(141, 3);
            xLabel.Name = "xLabel";
            xLabel.Size = new Size(16, 20);
            xLabel.TabIndex = 3;
            xLabel.Text = "x";
            // 
            // HeightTextBox
            // 
            HeightTextBox.Location = new Point(163, 0);
            HeightTextBox.MaxLength = 3;
            HeightTextBox.Name = "HeightTextBox";
            HeightTextBox.PlaceholderText = "height";
            HeightTextBox.Size = new Size(84, 27);
            HeightTextBox.TabIndex = 2;
            // 
            // WidthTextBox
            // 
            WidthTextBox.Location = new Point(48, 0);
            WidthTextBox.MaxLength = 3;
            WidthTextBox.Name = "WidthTextBox";
            WidthTextBox.PlaceholderText = "width";
            WidthTextBox.Size = new Size(84, 27);
            WidthTextBox.TabIndex = 1;
            // 
            // sizeLabel
            // 
            sizeLabel.AutoSize = true;
            sizeLabel.Location = new Point(3, 3);
            sizeLabel.Name = "sizeLabel";
            sizeLabel.Size = new Size(39, 20);
            sizeLabel.TabIndex = 0;
            sizeLabel.Text = "Size:";
            // 
            // AddBackgroundLayerCheckBox
            // 
            AddBackgroundLayerCheckBox.AutoSize = true;
            AddBackgroundLayerCheckBox.CheckAlign = ContentAlignment.MiddleRight;
            AddBackgroundLayerCheckBox.Checked = true;
            AddBackgroundLayerCheckBox.CheckState = CheckState.Checked;
            AddBackgroundLayerCheckBox.Location = new Point(3, 36);
            AddBackgroundLayerCheckBox.Name = "AddBackgroundLayerCheckBox";
            AddBackgroundLayerCheckBox.Size = new Size(178, 24);
            AddBackgroundLayerCheckBox.TabIndex = 1;
            AddBackgroundLayerCheckBox.Text = "Add background layer";
            AddBackgroundLayerCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionLabel
            // 
            optionLabel.AutoSize = true;
            optionLabel.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            optionLabel.Location = new Point(12, 6);
            optionLabel.Name = "optionLabel";
            optionLabel.Size = new Size(78, 25);
            optionLabel.TabIndex = 2;
            optionLabel.Text = "Options";
            // 
            // NewFileDialog
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(282, 153);
            Controls.Add(optionLabel);
            Controls.Add(OptionsPanel);
            Controls.Add(CreateButton);
            MaximumSize = new Size(300, 200);
            MinimumSize = new Size(300, 200);
            Name = "NewFileDialog";
            Text = "New ASCII Art File";
            OptionsPanel.ResumeLayout(false);
            OptionsPanel.PerformLayout();
            SizePanel.ResumeLayout(false);
            SizePanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public FlowLayoutPanel OptionsPanel;
        private Label optionLabel;
        private Panel SizePanel;
        private Label xLabel;
        private Label sizeLabel;
        private CheckBox AddBackgroundLayerCheckBox;
        private Button CreateButton;
        private TextBox HeightTextBox;
        private TextBox WidthTextBox;
    }
}