using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AAP.UI.Controls
{
    public class LabelStateBox : StateBox
    {
        private readonly DrawingVisual labelVisual = new();

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            name: "Text",
            propertyType: typeof(Brush),
            ownerType: typeof(LabelStateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Black, OnTextDrawPropertyChangedCallBack));

        public Brush Text
        {
            get => (Brush)GetValue(TextProperty);
            set
            {
                if (Text == value)
                    return;

                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty ContentFormatProperty =
        DependencyProperty.Register(
            name: "ContentFormatFormat",
            propertyType: typeof(string),
            ownerType: typeof(LabelStateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: string.Empty, OnTextDrawPropertyChangedCallBack));

        public string ContentFormat
        {
            get => (string)GetValue(ContentFormatProperty);
            set
            {
                if (Content == value)
                    return;

                SetValue(ContentFormatProperty, value);
            }
        }

        public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            name: "Content",
            propertyType: typeof(string),
            ownerType: typeof(LabelStateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: string.Empty, OnTextDrawPropertyChangedCallBack));

        public string Content
        {
            get => (string)GetValue(ContentProperty);
            set
            {
                if (Content == value)
                    return;

                SetValue(ContentProperty, value);
            }
        }

        public static readonly DependencyProperty TypefaceProperty =
        DependencyProperty.Register(
            name: "Typeface",
            propertyType: typeof(Typeface),
            ownerType: typeof(LabelStateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Typeface("Consolas"), OnTextDrawPropertyChangedCallBack));

        public Typeface Typeface
        {
            get => (Typeface)GetValue(TypefaceProperty);
            set
            {
                if (Typeface == value)
                    return;

                SetValue(TypefaceProperty, value);
            }
        }

        public static readonly DependencyProperty TextSizeProperty =
        DependencyProperty.Register(
            name: "TextSize",
            propertyType: typeof(double),
            ownerType: typeof(LabelStateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: 48d, OnTextDrawPropertyChangedCallBack));

        public double TextSize
        {
            get => (double)GetValue(TextSizeProperty);
            set
            {
                if (TextSize == value)
                    return;

                SetValue(TextSizeProperty, value);
            }
        }

        private void DrawLabel()
        {
            string formattedContent = ContentFormat == string.Empty ? Content : string.Format(ContentFormat, Content);

            FormattedText formattedText = new(formattedContent, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, TextSize, Text, 1);
           
            using DrawingContext dc = labelVisual.RenderOpen();

            dc.DrawText(formattedText, new(ActualWidth / 2 - formattedText.Width / 2, ActualHeight / 2 - formattedText.Height / 2));
        }

        public LabelStateBox() : base()
        {
            _children.Add(labelVisual);

            DrawLabel();

            SizeChanged += (sender, e) => DrawLabel();
        }

        private static void OnTextDrawPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not LabelStateBox box)
                return;

            box.DrawLabel();
        }
    }
}
