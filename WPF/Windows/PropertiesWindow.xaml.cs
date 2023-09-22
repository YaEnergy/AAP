﻿using AAP.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        private readonly Dictionary<string, object?> properties = new();

        public PropertiesWindow(string title, string closeMessage)
        {
            InitializeComponent();

            Title = title;
            WindowViewModel.CloseButtonContent = closeMessage;
            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyClose());
        }

        public void AddCategory(string name)
        {
            Label label = new();
            label.Content = name;
            label.FontSize = 16;

            PropertyList.Children.Add(label);
        }

        public void AddStringProperty(string name, string startingValue)
        {
            PropertyList.Children.Add(CreateStringPropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        public void AddBoolProperty(string name, bool startingValue)
        {
            PropertyList.Children.Add(CreateBoolPropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        public void AddIntProperty(string name, int startingValue)
        {
            PropertyList.Children.Add(CreateIntPropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        public void AddFloatProperty(string name, float startingValue)
        {
            PropertyList.Children.Add(CreateFloatPropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        public void AddDoubleProperty(string name, double startingValue)
        {
            PropertyList.Children.Add(CreateDoublePropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        public void AddSliderDoubleProperty(string name, double min, double max, double startingValue, double step = 1)
        {
            PropertyList.Children.Add(CreateSliderPropertyUIElement(name, min, max, startingValue, step));

            properties.Add(name, startingValue);
        }

        public void AddColorProperty(string name, Color startingValue)
        {
            PropertyList.Children.Add(CreateColorPropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        public void AddSizeDoubleProperty(string name, Size startingValue)
        {
            PropertyList.Children.Add(CreateSizeDoublePropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        public void AddSizeIntProperty(string name, Size startingValue)
        {
            PropertyList.Children.Add(CreateSizeIntPropertyUIElement(name, startingValue));

            properties.Add(name, startingValue);
        }

        #region PropertyUIElement type creations
        private UIElement CreateStringPropertyUIElement(string name, string value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            TextBox textBox = new();
            textBox.Text = value;
            textBox.Margin = new(5, 0, 5, 0);
            textBox.Width = 128;
            textBox.Height = 22;

            textBox.LostFocus += (sender, e) => SetProperty(name, textBox.Text);

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(textBox);

            return propertyPanel;
        }

        private UIElement CreateBoolPropertyUIElement(string name, bool value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            CheckBox checkBox = new();
            checkBox.Padding = new(5);
            checkBox.Content = name;
            checkBox.IsChecked = value;
            checkBox.SetBinding(CheckBox.StyleProperty, "CheckBoxStyle");
            checkBox.SetBinding(CheckBox.ForegroundProperty, "CheckBoxForeground");

            checkBox.Checked += (sender, e) => SetProperty(name, checkBox.IsChecked);

            propertyPanel.Children.Add(checkBox);

            return propertyPanel;
        }

        private UIElement CreateIntPropertyUIElement(string name, int value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            TextBox textBox = new();
            textBox.Margin = new(5, 0, 5, 0);
            textBox.Width = 64;
            textBox.Height = 22;

            textBox.Text = value.ToString();

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (int.TryParse(textBoxSender.Text, out int newValue))
                {
                    value = newValue;
                    SetProperty(name, newValue);
                }

                textBoxSender.Text = value.ToString();
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(textBox);

            return propertyPanel;
        }

        private UIElement CreateFloatPropertyUIElement(string name, float value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            TextBox textBox = new();
            textBox.Margin = new(5, 0, 5, 0);
            textBox.Width = 64;
            textBox.Height = 22;

            textBox.Text = value.ToString();

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (float.TryParse(textBoxSender.Text, out float newValue))
                {
                    value = newValue;
                    SetProperty(name, newValue);
                }

                textBoxSender.Text = value.ToString();
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(textBox);

            return propertyPanel;
        }

        private UIElement CreateDoublePropertyUIElement(string name, double value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            TextBox textBox = new();
            textBox.Margin = new(5, 0, 5, 0);
            textBox.Width = 64;
            textBox.Height = 22;

            textBox.Text = value.ToString();

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (double.TryParse(textBoxSender.Text, out double newValue))
                {
                    value = newValue;
                    SetProperty(name, newValue);
                }

                textBoxSender.Text = value.ToString();
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(textBox);

            return propertyPanel;
        }

        private UIElement CreateSliderPropertyUIElement(string name, double min, double max, double value, double step = 1)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            Slider slider = new();
            slider.Width = 128;
            slider.Height = 22;

            slider.Value = value;
            slider.Minimum = min;
            slider.Maximum = max;
            slider.IsSnapToTickEnabled = true;
            slider.TickFrequency = step;
            slider.VerticalContentAlignment = VerticalAlignment.Stretch;

            Label valueLabel = new();
            valueLabel.Content = value;

            slider.ValueChanged += (sender, e) => valueLabel.Content = e.NewValue;
            slider.ValueChanged += (sender, e) => SetProperty(name, value);

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(slider);
            propertyPanel.Children.Add(valueLabel);

            return propertyPanel;
        }

        private UIElement CreateColorPropertyUIElement(string name, Color value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            ColorPicker colorPicker = new();

            SolidColorBrush brush = new(value);
            brush.Freeze();

            colorPicker.PickedColor = brush;

            colorPicker.ColorChanged += (sender, color) =>
            {
                if (sender is not ColorPicker colorPickerSender)
                    return;

                SetProperty(name, color.Color);
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(colorPicker);

            return propertyPanel;
        }

        private UIElement CreateSizeDoublePropertyUIElement(string name, Size value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            Label xLabel = new();
            xLabel.Content = "x";

            TextBox widthBox = new();
            widthBox.Margin = new(5, 0, 5, 0);
            widthBox.Width = 64;
            widthBox.Height = 22;
            widthBox.Text = value.Width.ToString();

            TextBox heightBox = new();
            heightBox.Margin = new(5, 0, 5, 0);
            heightBox.Width = 64;
            heightBox.Height = 22;
            heightBox.Text = value.Height.ToString();

            widthBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (double.TryParse(textBoxSender.Text, out double newWidth) && newWidth >= 0)
                {
                    value = new(newWidth, value.Height);
                    SetProperty(name, value);
                }
                else
                    textBoxSender.Text = value.Width.ToString();
            };

            heightBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (double.TryParse(textBoxSender.Text, out double newHeight) && newHeight >= 0)
                {
                    value = new(value.Width, newHeight);
                    SetProperty(name, value);
                }
                else
                    textBoxSender.Text = value.Height.ToString();
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(widthBox);
            propertyPanel.Children.Add(xLabel);
            propertyPanel.Children.Add(heightBox);
            
            return propertyPanel;
        }

        private UIElement CreateSizeIntPropertyUIElement(string name, Size value)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5);

            Label label = new();
            label.Content = name;

            Label xLabel = new();
            xLabel.Content = "x";

            TextBox widthBox = new();
            widthBox.Margin = new(5, 0, 5, 0);
            widthBox.Width = 64;
            widthBox.Height = 22;
            widthBox.Text = value.Width.ToString();

            TextBox heightBox = new();
            heightBox.Margin = new(5, 0, 5, 0);
            heightBox.Width = 64;
            heightBox.Height = 22;
            heightBox.Text = value.Height.ToString();

            widthBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (int.TryParse(textBoxSender.Text, out int newWidth) && newWidth >= 0)
                {
                    value = new(newWidth, value.Height);
                    SetProperty(name, value);
                }
                else
                    textBoxSender.Text = value.Width.ToString();
            };

            heightBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (int.TryParse(textBoxSender.Text, out int newHeight) && newHeight >= 0)
                {
                    value = new(value.Width, newHeight);
                    SetProperty(name, value);
                }
                else
                    textBoxSender.Text = value.Height.ToString();
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(widthBox);
            propertyPanel.Children.Add(xLabel);
            propertyPanel.Children.Add(heightBox);

            return propertyPanel;
        }
        #endregion

        public object? GetProperty(string name)
        {
            if (!properties.ContainsKey(name))
                throw new InvalidOperationException($"{name} is not a property");

            return properties[name];
        }

        public void SetProperty(string name, object? value)
        {
            if (!properties.ContainsKey(name))
                throw new InvalidOperationException($"{name} is not a property");

            properties[name] = value;
        }

        private void ApplyClose()
        {
            DialogResult = true;
            return;
        }
    }
}