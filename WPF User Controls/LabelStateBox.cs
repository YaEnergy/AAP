using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AAP
{
    public class LabelStateBox : StateBox
    {
        private readonly DrawingVisual labelVisual = new();

        private string text = "";
        public string Text
        {
            get => text;
            set
            {
                if (value == text)
                    return;

                text = value;
                DrawLabel();
            }
        }

        private Typeface typeface = new("Consolas");
        public Typeface Typeface
        {
            get => typeface;
            set
            {
                if (value == typeface)
                    return;

                typeface = value;
                DrawLabel();
            }
        }

        private double textSize = 48;
        public double TextSize
        {
            get => textSize;
            set
            {
                if (value == textSize)
                    return;

                textSize = value;
                DrawLabel();
            }
        }

        private void DrawLabel()
        {
            FormattedText formattedText = new(Text, System.Globalization.CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight, Typeface, TextSize, System.Windows.Media.Brushes.Black, 1);
            using (DrawingContext dc = labelVisual.RenderOpen())
                dc.DrawText(formattedText, new(ActualWidth / 2 - formattedText.Width / 2, ActualHeight/2-formattedText.Height/2));
        }

        public LabelStateBox() : base()
        {
            _children.Add(labelVisual);

            DrawLabel();

            SizeChanged += (sender, e) => DrawLabel();
        }
    }
}
