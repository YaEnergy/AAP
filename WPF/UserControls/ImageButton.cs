using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AAP.UI.Controls
{
    public class ImageButton : FrameworkElement
    {
        protected readonly VisualCollection _children;

        private readonly DrawingVisual backgroundVisual = new();
        private readonly DrawingVisual imageVisual = new();

        private ImageSource? imageSource = null;
        public ImageSource? ImageSource
        {
            get => imageSource;
            set
            {
                if (imageSource == value)
                    return;

                imageSource = value;
                DrawImage();
            }
        }

        protected override int VisualChildrenCount => _children.Count;

        private bool highlighted = false;
        public bool Highlighted
        {
            get => highlighted;
            private set
            {
                if (highlighted == value)
                    return;

                highlighted = value;

                if (Clicked == true)
                    BackgroundBrush = ClickedBackground;
                else
                    BackgroundBrush = Highlighted ? HighlightedBackground : Background;
            }
        }

        private bool clicked = false;
        public bool Clicked
        {
            get => clicked;
            private set
            {
                if (clicked == value)
                    return;

                clicked = value;

                if (Clicked == true)
                    BackgroundBrush = ClickedBackground;
                else
                    BackgroundBrush = Highlighted ? HighlightedBackground : Background;
            }
        }

        private Brush backgroundBrush;
        public Brush BackgroundBrush
        {
            get => backgroundBrush;
            private set
            {
                if (value == backgroundBrush)
                    return;

                backgroundBrush = value;
                DrawBackground();
            }
        }

        public Brush Background { get; set; } = Brushes.White;
        public Brush HighlightedBackground { get; set; } = Brushes.LightGray;
        public Brush ClickedBackground { get; set; } = Brushes.DarkGray;

        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            name: "Command",
            propertyType: typeof(ICommand),
            ownerType: typeof(ImageButton),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public ICommand? Command 
        { 
            get => (ICommand?)GetValue(CommandProperty); 
            set => SetValue(CommandProperty, value); 
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        private void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            dc.DrawRectangle(BackgroundBrush, null, new(0, 0, ActualWidth, ActualHeight));
        }

        private void DrawImage()
        {
            using DrawingContext dc = imageVisual.RenderOpen();

            if (imageSource != null)
                dc.DrawImage(imageSource, new(0, 0, ActualWidth, ActualHeight));
        }

        public ImageButton()
        {
            _children = new(this)
            {
                backgroundVisual,
                imageVisual
            };

            if (Clicked == true)
                backgroundBrush = ClickedBackground;
            else
                backgroundBrush = Highlighted ? HighlightedBackground : Background;

            DrawBackground();
            DrawImage();

            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;

            SizeChanged += (sender, e) => DrawBackground();
            SizeChanged += (sender, e) => DrawImage();
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            Highlighted = true;
            Clicked = false;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            Highlighted = false;
            Clicked = false;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(this);
            Clicked = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => Clicked = false;
    }
}
