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

        public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            name: "ImageSource",
            propertyType: typeof(ImageSource),
            ownerType: typeof(ImageStateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, OnImageSourcePropertyChangedCallBack));

        public ImageSource? ImageSource
        {
            get => (ImageSource?)GetValue(ImageSourceProperty);
            set
            {
                if (ImageSource == value)
                    return;

                SetValue(ImageSourceProperty, value);
            }
        }

        private void DrawImage()
        {
            using DrawingContext dc = imageVisual.RenderOpen();

            if (ImageSource != null)
                dc.DrawImage(ImageSource, new(0, 0, ActualWidth, ActualHeight));
        }

        public ImageStateBox() : base()
        {
            _children.Add(imageVisual);

            DrawImage();

            SizeChanged += (sender, e) => DrawImage();
        }

        private static void OnImageSourcePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ImageStateBox box)
                return;

            box.DrawImage();
        }
    }
}
