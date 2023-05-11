using System.Windows.Forms;

namespace AAP
{
    partial class AAPLoadingForm : Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AAPLoadingForm));
            loadingLabel = new Label();
            progressBar = new ProgressBar();
            splashIcon = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)splashIcon).BeginInit();
            SuspendLayout();
            // 
            // loadingLabel
            // 
            loadingLabel.Anchor = AnchorStyles.Bottom;
            loadingLabel.Location = new Point(12, 178);
            loadingLabel.Name = "loadingLabel";
            loadingLabel.Size = new Size(458, 31);
            loadingLabel.TabIndex = 0;
            loadingLabel.Text = "aaa";
            loadingLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom;
            progressBar.Location = new Point(12, 212);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(458, 29);
            progressBar.TabIndex = 1;
            // 
            // splashIcon
            // 
            splashIcon.Location = new Point(12, 12);
            splashIcon.Name = "splashIcon";
            splashIcon.Size = new Size(458, 163);
            splashIcon.TabIndex = 2;
            splashIcon.TabStop = false;
            // 
            // AAPLoadingForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(482, 253);
            Controls.Add(splashIcon);
            Controls.Add(progressBar);
            Controls.Add(loadingLabel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new Size(500, 300);
            MinimizeBox = false;
            MinimumSize = new Size(500, 300);
            Name = "AAPLoadingForm";
            Opacity = 0.5D;
            Text = "ASCII Art Program";
            TopMost = true;
            ((System.ComponentModel.ISupportInitialize)splashIcon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label loadingLabel;
        private ProgressBar progressBar;
        private PictureBox splashIcon;
    }
}