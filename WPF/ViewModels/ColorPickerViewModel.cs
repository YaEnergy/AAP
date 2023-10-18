using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AAP.UI.ViewModels
{
    public class ColorPickerViewModel : INotifyPropertyChanged
    {
        private static string ColorPickerName => App.Language.GetString("ColorPicker");
        private static string InvalidHexColorMessage => App.Language.GetString("Error_InvalidHexColorMessage");
        private static string InvalidColorMessage => App.Language.GetString("Error_InvalidColorMessage");

        private Color pickedColor = Colors.White;
        public Color PickedColor
        {
            get => pickedColor;
            set
            {
                if (pickedColor == value)
                    return;

                pickedColor = value;

                ColorBrush = new SolidColorBrush(PickedColor);
                redText = value.R.ToString();
                PropertyChanged?.Invoke(this, new(nameof(RedText)));
                greenText = value.G.ToString();
                PropertyChanged?.Invoke(this, new(nameof(GreenText)));
                blueText = value.B.ToString();
                PropertyChanged?.Invoke(this, new(nameof(BlueText)));
                hexText = "#" + ToHex(value.R, 2) + ToHex(value.G, 2) + ToHex(value.B, 2);
                PropertyChanged?.Invoke(this, new(nameof(HexText)));

                PropertyChanged?.Invoke(this, new(nameof(PickedColor)));
            }
        }

        private SolidColorBrush colorBrush;
        public SolidColorBrush ColorBrush
        {
            get => colorBrush;
            set
            {
                if (colorBrush == value)
                    return;

                colorBrush = value;

                PropertyChanged?.Invoke(this, new(nameof(ColorBrush)));
            }
        }

        private string redText;
        public string RedText
        {
            get => redText;
            set
            {
                if (redText == value) 
                    return;
                
                try
                {
                    PickedColor = Color.FromRgb(byte.Parse(value), byte.Parse(GreenText), byte.Parse(BlueText));

                    redText = value;
                }
                catch (Exception)
                {
                    MessageBox.Show(InvalidColorMessage, ColorPickerName, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                PropertyChanged?.Invoke(this, new(nameof(RedText)));
            }
        }

        private string greenText;
        public string GreenText
        {
            get => greenText;
            set
            {
                if (greenText == value)
                    return;

                try
                {
                    PickedColor = Color.FromRgb(byte.Parse(RedText), byte.Parse(value), byte.Parse(BlueText));

                    greenText = value;
                }
                catch (Exception)
                {
                    MessageBox.Show(InvalidColorMessage, ColorPickerName, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                PropertyChanged?.Invoke(this, new(nameof(GreenText)));
            }
        }

        private string blueText;
        public string BlueText
        {
            get => blueText;
            set
            {
                if (blueText == value)
                    return;

                try
                {
                    PickedColor = Color.FromRgb(byte.Parse(RedText), byte.Parse(GreenText), byte.Parse(value));

                    blueText = value;
                }
                catch (Exception)
                {
                    MessageBox.Show(InvalidColorMessage, ColorPickerName, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                PropertyChanged?.Invoke(this, new(nameof(BlueText)));
            }
        }

        private string hexText = "";
        public string HexText
        {
            get => hexText;
            set
            {
                if (hexText == value)
                    return;

                string newHexText = value;
            
                if (!newHexText.StartsWith('#'))
                    newHexText = "#" + newHexText;

                try
                {
                    Color color = (Color)ColorConverter.ConvertFromString(newHexText);
                    PickedColor = color;

                    hexText = newHexText;
                }
                catch (Exception)
                {
                    MessageBox.Show(InvalidHexColorMessage, ColorPickerName, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                PropertyChanged?.Invoke(this, new(nameof(HexText)));
            }
        }

        public static string ToHex(int num, int minLength = 0)
        {
            Stack<int> remains = new();

            remains.Push(num % 16);
            num /= 16;

            while (num > 0)
            {
                remains.Push(num % 16);
                num /= 16;
            }

            string hex = "";
            for (int  i = 0; i < Math.Max(minLength, remains.Count); i++)
            {
                if (remains.Count > 0)
                {
                    int remain = remains.Pop();
                    if (remain < 10)
                        hex += remain.ToString();
                    else
                        switch (remain)
                        {
                            case 10:
                                hex += "A";
                                break;
                            case 11:
                                hex += "B";
                                break;
                            case 12:
                                hex += "C";
                                break;
                            case 13:
                                hex += "D";
                                break;
                            case 14:
                                hex += "E";
                                break;
                            case 15:
                                hex += "F";
                                break;
                        }
                }
                else
                    hex += "0";
            }

            return hex;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ColorPickerViewModel()
        {
            colorBrush = new SolidColorBrush(PickedColor);
            redText = PickedColor.R.ToString();
            greenText = PickedColor.G.ToString();
            blueText = PickedColor.B.ToString();
            hexText = "#" + ToHex(PickedColor.R, 2) + ToHex(PickedColor.G, 2) + ToHex(PickedColor.B, 2);
            PropertyChanged?.Invoke(this, new(nameof(HexText)));
        }
    }
}
