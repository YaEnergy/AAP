using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AAP
{
    public partial class NewFileDialog : Form
    {
        private readonly static string InvalidFileMessageBoxTitle = "Invalid File Options";
        public NewFileDialog()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            int sizeWidth;
            int sizeHeight;

            if (!int.TryParse(WidthTextBox.Text, out sizeWidth))
            {
                MessageBox.Show("Art size width is invalid! Please type a positive integer.", InvalidFileMessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(HeightTextBox.Text, out sizeHeight))
            {
                MessageBox.Show("Art size height is invalid! Please type a positive integer.", InvalidFileMessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (sizeWidth <= 0)
            {
                MessageBox.Show("Art size width is invalid! Please type a positive integer.", InvalidFileMessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (sizeHeight <= 0)
            {
                MessageBox.Show("Art size height is invalid! Please type a positive integer.", InvalidFileMessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (sizeWidth * sizeHeight > 9800)
            {
                MessageBox.Show($"Art Area is too large! Max: {MainProgram.MaxArtArea} characters ({sizeWidth * sizeHeight} characters)", InvalidFileMessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ASCIIArtFile artFile = new(sizeWidth, sizeHeight, MainProgram.Version, MainProgram.Version);

            if (AddBackgroundLayerCheckBox.Checked)
                artFile.AddBackgroundLayer();

            MainProgram.NewFile(artFile);

            Close();
        }
    }
}
