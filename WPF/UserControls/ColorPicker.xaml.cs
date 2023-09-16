using AAP.UI.ViewModels;
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

namespace AAP.UI.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public static readonly DependencyProperty PickedColorProperty =
        DependencyProperty.Register(
            name: "PickedColor",
            propertyType: typeof(SolidColorBrush),
            ownerType: typeof(ColorPicker),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.White, OnColorPropertyChangedCallBack));

        public SolidColorBrush PickedColor
        {
            get => (SolidColorBrush)GetValue(PickedColorProperty);
            set
            {
                if (PickedColor == value)
                    return;
                
                SetValue(PickedColorProperty, value);
            }
        }

        public delegate void ColorChangedEvent(ColorPicker colorPicker, SolidColorBrush color);
        public event ColorChangedEvent? ColorChanged;

        public ColorPicker()
        {
            InitializeComponent();

            ControlViewModel.PropertyChanged += OnControlViewModelPropertyChanged;
        }

        private static void OnColorPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ColorPicker colorPicker)
                return;

            colorPicker.ControlViewModel.PickedColor = colorPicker.PickedColor.Color;
            colorPicker.ColorChanged?.Invoke(colorPicker, colorPicker.PickedColor);
        }

        private void OnControlViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ColorPickerViewModel colorPickerViewModel)
                return;

            if (e.PropertyName == nameof(colorPickerViewModel.ColorBrush))
                PickedColor = colorPickerViewModel.ColorBrush;
        }
    }
}
