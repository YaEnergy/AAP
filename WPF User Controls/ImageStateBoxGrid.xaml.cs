using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AAP
{
    /// <summary>
    /// Interaction logic for ImageStateBoxGrid.xaml
    /// </summary>
    public partial class ImageStateBoxGrid : System.Windows.Controls.UserControl
    {
        private bool allowOneEnabledOnly = false;
        public bool AllowOneEnabledOnly
        { 
            get => allowOneEnabledOnly; 
            set
            {
                allowOneEnabledOnly = value;
            }
        }

        public List<ImageStateBox> EnabledBoxes
        {
            get
            {
                List<ImageStateBox> boxes = new();

                for (int i = 0; i < imageStateBoxes.Count; i++)
                    if (imageStateBoxes[i].State)
                        boxes.Add(imageStateBoxes[i]);

                return boxes;
            }
        }

        public List<ImageStateBox> Boxes
        {
            get
            {
                List<ImageStateBox> boxes = new();

                for (int i = 0; i < imageStateBoxes.Count; i++)
                    boxes.Add(imageStateBoxes[i]);

                return boxes; //So the original list cannot be modified
            }
        }

        private readonly List<ImageStateBox> imageStateBoxes = new();

        public System.Windows.Size GridItemSize { get; set; } = new(60, 60);

        public ImageStateBoxGrid(List<ImageStateBox>? boxes = null)
        {
            InitializeComponent();

            if (boxes != null)
                for (int i = 0; i < boxes.Count; i++)
                    AddImageStateBox(boxes[i]);

            UpdateImageStateBoxListLayout();
        }

        public void AddImageStateBox(ImageStateBox imageStateBox)
        {
            imageStateBox.OnStateChanged += OnImageStateBoxStateChanged;

            imageStateBoxes.Add(imageStateBox);
        }

        public bool RemoveImageStateBox(ImageStateBox imageStateBox)
        {
            if (imageStateBoxes.Contains(imageStateBox))
                return false;

            imageStateBox.OnStateChanged -= OnImageStateBoxStateChanged;

            return imageStateBoxes.Remove(imageStateBox);
        }

        public void ClearImageStateBoxes()
        {
            foreach(ImageStateBox imageStateBox in imageStateBoxes)
                imageStateBox.OnStateChanged -= OnImageStateBoxStateChanged;

            imageStateBoxes.Clear();
        }

        public void UpdateImageStateBoxListLayout()
        {
            //Set Box Sizes to GridItemSize
            //Set Box Positions

            throw new NotImplementedException();
        }

        private void OnImageStateBoxStateChanged(ImageStateBox sender, bool state)
        {
            if (AllowOneEnabledOnly && state)
                foreach (ImageStateBox imageStateBox in imageStateBoxes)
                    if (imageStateBox != sender)
                        imageStateBox.State = false;
        }
    }
}
