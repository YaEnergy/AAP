using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AAP.UI.Controls
{
    public class ImageStateBox : StateBox
    {
        private readonly DrawingVisual imageVisual = new();

        private ImageSource? boxImageSource = null;
        public ImageSource? BoxImageSource
        {
            get => boxImageSource;
            set
            {
                if (value == boxImageSource)
                    return;

                boxImageSource = value;
                DrawImage();
            }
        }

        private void DrawImage()
        {
            using DrawingContext dc = imageVisual.RenderOpen();

            if (boxImageSource != null)
                dc.DrawImage(boxImageSource, new(0, 0, ActualWidth, ActualHeight));
        }

        public ImageStateBox() : base()
        {
            _children.Add(imageVisual);

            DrawImage();

            SizeChanged += (sender, e) => DrawImage();
        }
    }
}
