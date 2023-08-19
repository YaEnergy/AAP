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
        protected override int VisualChildrenCount => _children.Count;
        protected readonly VisualCollection _children;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        private readonly DrawingVisual backgroundVisual = new();
        private readonly DrawingVisual imageVisual = new();

        #region Brushes
        public static readonly DependencyProperty UnhighlightedProperty =
       DependencyProperty.Register(
           name: "Unhighlighted",
           propertyType: typeof(Brush),
           ownerType: typeof(ImageButton),
           typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.White, OnBackgroundDrawPropertyChangedCallBack));

        public Brush Unhighlighted
        {
            get => (Brush)GetValue(UnhighlightedProperty);
            set
            {
                if (Unhighlighted == value)
                    return;

                SetValue(UnhighlightedProperty, value);
            }
        }

        public static readonly DependencyProperty HighlightedProperty =
        DependencyProperty.Register(
            name: "Highlighted",
            propertyType: typeof(Brush),
            ownerType: typeof(ImageButton),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.LightGray, OnBackgroundDrawPropertyChangedCallBack));

        public Brush Highlighted
        {
            get => (Brush)GetValue(HighlightedProperty);
            set
            {
                if (Highlighted == value)
                    return;

                SetValue(HighlightedProperty, value);
            }
        }

        public static readonly DependencyProperty PressedProperty =
        DependencyProperty.Register(
            name: "Pressed",
            propertyType: typeof(Brush),
            ownerType: typeof(ImageButton),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Gray, OnBackgroundDrawPropertyChangedCallBack));

        public Brush Pressed
        {
            get => (Brush)GetValue(PressedProperty);
            set
            {
                if (Pressed == value)
                    return;

                SetValue(PressedProperty, value);
            }
        }

        public static readonly DependencyProperty BorderProperty =
        DependencyProperty.Register(
            name: "Border",
            propertyType: typeof(Brush),
            ownerType: typeof(ImageButton),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Black, OnBackgroundDrawPropertyChangedCallBack));

        public Brush Border
        {
            get => (Brush)GetValue(BorderProperty);
            set
            {
                if (Border == value)
                    return;

                SetValue(BorderProperty, value);
            }
        }

        public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(
            name: "BorderThickness",
            propertyType: typeof(double),
            ownerType: typeof(ImageButton),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0d, OnBackgroundDrawPropertyChangedCallBack));

        public double BorderThickness
        {
            get => (double)GetValue(BorderThicknessProperty);
            set
            {
                if (BorderThickness == value)
                    return;

                SetValue(BorderThicknessProperty, value);
            }
        }
        #endregion
        public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            name: "ImageSource",
            propertyType: typeof(ImageSource),
            ownerType: typeof(ImageButton),
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

        private bool isHighlighted = false;
        public bool IsHighlighted
        {
            get => isHighlighted;
            private set
            {
                if (isHighlighted == value)
                    return;

                isHighlighted = value;

                DrawBackground();
            }
        }

        public static readonly DependencyProperty IsPressedProperty =
         DependencyProperty.Register(
             name: "IsPressed",
             propertyType: typeof(bool),
             ownerType: typeof(ImageButton),
             typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, OnIsPressedPropertyChangedCallBack));

        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            private set
            {
                if (IsPressed == value)
                    return;

                SetValue(IsPressedProperty, value);
            }
        }

        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            name: "Command",
            propertyType: typeof(ICommand),
            ownerType: typeof(ImageButton),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public ICommand? Command 
        { 
            get => (ICommand?)GetValue(CommandProperty); 
            set
            {
                if (Command == value)
                    return;

                SetValue(CommandProperty, value);
            }
        }

        private void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            Brush fillBrush;
            if (IsPressed)
                fillBrush = Pressed;
            else
                fillBrush = IsHighlighted ? Highlighted : Unhighlighted;

            dc.DrawRectangle(fillBrush, new(Border, BorderThickness), new(0, 0, ActualWidth, ActualHeight));
        }

        private void DrawImage()
        {
            using DrawingContext dc = imageVisual.RenderOpen();

            if (ImageSource != null)
                dc.DrawImage(ImageSource, new(0, 0, ActualWidth, ActualHeight));
        }

        public ImageButton()
        {
            _children = new(this)
            {
                backgroundVisual,
                imageVisual
            };

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
            IsHighlighted = true;
            IsPressed = false;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsHighlighted = false;
            IsPressed = false;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(this);
            IsPressed = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => IsPressed = false;

        private static void OnImageSourcePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ImageButton button)
                return;

            button.DrawImage();
        }

        private static void OnBackgroundDrawPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ImageButton button)
                return;

            button.DrawBackground();
        }

        private static void OnIsPressedPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ImageButton button)
                return;

            button.DrawBackground();
        }
    }
}
