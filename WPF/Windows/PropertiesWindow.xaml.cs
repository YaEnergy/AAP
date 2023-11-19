using AAP.UI.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace AAP.UI.Windows
{
    /// <summary>
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        private int LevelOffset { get; set; }

        private readonly Dictionary<string, object?> properties = new();

        public PropertiesWindow(string title, string closeMessage, int levelOffset = 10)
        {
            InitializeComponent();

            Title = title;
            LevelOffset = levelOffset;

            WindowViewModel.CloseButtonContent = closeMessage;
            WindowViewModel.CloseButtonCommand = new ActionCommand((parameter) => ApplyClose());
        }

        #region Base Properties
        public void AddProperty(object? labelContent, object? labelTooltip, UIElement propertyElement, int level = 0)
        {
            WrapPanel propertyPanel = new();
            propertyPanel.Margin = new(5 + level * LevelOffset, 5, 5, 5);

            if (labelContent != null)
            {
                Label label = new();
                label.Content = labelContent;
                label.ToolTip = labelTooltip;

                propertyPanel.Children.Add(label);
            }

            propertyPanel.Children.Add(propertyElement);

            PropertyList.Children.Add(propertyPanel);
        }

        public void AddProperty(object? labelContent, UIElement inputUIElement, int level = 0)
            => AddProperty(labelContent, null, inputUIElement, level);

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
        #endregion

        #region Decoration
        public void AddLabel(object content, double fontSize = 12, int level = 0)
        {
            Label label = new();
            label.Content = content;
            label.FontSize = fontSize;
            label.Margin = new(5 + level * LevelOffset, 0, 5, 0);

            PropertyList.Children.Add(label);
        }

        public void AddCategory(string name, int level = 0)
            => AddLabel(name, 16 - level);

        #endregion

        #region Input Property Elements
        public UIElement CreateInputStringProperty(string name, string value, Predicate<string>? predicate = null)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

            TextBox textBox = new();
            textBox.Text = value;
            textBox.Width = 128;
            textBox.Height = 22;

            textBox.LostFocus += (sender, e) =>
            {
                if (predicate != null && !predicate.Invoke(textBox.Text))
                    return;

                SetProperty(name, textBox.Text);
            };

            return textBox;
        }

        public UIElement CreateInputIntProperty(string name, int value, Predicate<int>? predicate = null)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

            TextBox textBox = new();
            textBox.Text = value.ToString();
            textBox.Width = 128;
            textBox.Height = 22;

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (int.TryParse(textBoxSender.Text, out int newValue) && (predicate == null || predicate.Invoke(newValue)))
                {
                    value = newValue;
                    SetProperty(name, newValue);
                }

                textBoxSender.Text = value.ToString();
            };

            return textBox;
        }

        public UIElement CreateInputDoubleProperty(string name, double value, Predicate<double>? predicate = null)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

            TextBox textBox = new();
            textBox.Text = value.ToString();
            textBox.Width = 128;
            textBox.Height = 22;

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (double.TryParse(textBoxSender.Text, out double newValue) && (predicate == null || predicate.Invoke(newValue)))
                {
                    value = newValue;
                    SetProperty(name, newValue);
                }

                textBoxSender.Text = value.ToString();
            };

            return textBox;
        }

        public UIElement CreateInputFloatProperty(string name, float value, Predicate<float>? predicate = null)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

            TextBox textBox = new();
            textBox.Text = value.ToString();
            textBox.Width = 128;
            textBox.Height = 22;

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (float.TryParse(textBoxSender.Text, out float newValue) && (predicate == null || predicate.Invoke(newValue)))
                {
                    value = newValue;
                    SetProperty(name, newValue);
                }

                textBoxSender.Text = value.ToString();
            };

            return textBox;
        }

        public UIElement CreateInputSizeProperty(string name, Size value, Predicate<Size>? predicate = null)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

            WrapPanel sizePanel = new();

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
                    Size newValue = new(newWidth, value.Height);

                    if (predicate == null || predicate.Invoke(newValue))
                    {
                        value.Width = newWidth;
                        SetProperty(name, value);
                    }
                }

                textBoxSender.Text = value.Width.ToString();
            };

            heightBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (double.TryParse(textBoxSender.Text, out double newHeight) && newHeight >= 0)
                {
                    Size newValue = new(value.Width, newHeight);

                    if (predicate == null || predicate.Invoke(newValue))
                    {
                        value.Height = newHeight;
                        SetProperty(name, value);
                    }
                }
                
                textBoxSender.Text = value.Height.ToString();
            };

            sizePanel.Children.Add(widthBox);
            sizePanel.Children.Add(xLabel);
            sizePanel.Children.Add(heightBox);

            return sizePanel;
        }
        #endregion

        #region Slider Property Elements
        public UIElement CreateSliderProperty(string name, double min, double max, double value, double step = 1, int precision = 0)
        {
            WrapPanel sliderPanel = new();

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

            slider.ValueChanged += (sender, e) => valueLabel.Content = e.NewValue.ToString("N" + precision.ToString());
            slider.ValueChanged += (sender, e) => SetProperty(name, e.NewValue);

            sliderPanel.Children.Add(slider);
            sliderPanel.Children.Add(valueLabel);

            return sliderPanel;
        }

        public UIElement CreateSliderProperty(string name, double min, double max, double value, double step = 1)
            => CreateSliderProperty(name, min, max, value, step, (int)Math.Abs(Math.Log10(step % 1)));
        #endregion

        #region ComboBox Property Elements
        public UIElement CreateComboBoxIntProperty(string name, List<object> items, int index = -1) 
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, index);

            //ComboBox
            ComboBox comboBox = new();

            foreach (object item in items)
                comboBox.Items.Add(item);

            comboBox.SelectedIndex = index;
            comboBox.Width = 128;
            comboBox.Height = 22;
            comboBox.IsEditable = false;

            comboBox.SelectionChanged += (sender, e) => SetProperty(name, comboBox.SelectedIndex);

            return comboBox;
        }

        public UIElement CreateComboBoxListProperty<T>(string name, List<T> items, int index = 0)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, items[index]);

            //ComboBox
            ComboBox comboBox = new();

            foreach (T item in items)
                comboBox.Items.Add(item);

            comboBox.SelectedIndex = index;
            comboBox.Width = 128;
            comboBox.Height = 22;
            comboBox.IsEditable = false;

            comboBox.SelectionChanged += (sender, e) => SetProperty(name, comboBox.SelectedItem);

            return comboBox;
        }

        public UIElement CreateComboBoxEnumProperty<T>(string name, T value) where T : Enum
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

            //ComboBox
            ComboBox comboBox = new();

            foreach (string item in Enum.GetNames(typeof(T)))
                comboBox.Items.Add(item);

            comboBox.SelectedItem = Enum.GetName(typeof(T), value);
            comboBox.Width = 128;
            comboBox.Height = 22;
            comboBox.IsEditable = false;

            comboBox.SelectionChanged += (sender, e) => 
            {
                if (comboBox.SelectedItem is not string enumValueName)
                    return;

                SetProperty(name, Enum.Parse(typeof(T), enumValueName));
            };

            return comboBox;
        }
        #endregion

        #region Other Property Elements
        public UIElement CreateBoolProperty(string name, bool value)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

            CheckBox checkBox = new();
            checkBox.Padding = new(5);
            checkBox.IsChecked = value;
            checkBox.SetBinding(StyleProperty, "CheckBoxStyle");
            checkBox.SetBinding(ForegroundProperty, "CheckBoxForeground");

            checkBox.Checked += (sender, e) => SetProperty(name, checkBox.IsChecked);

            return checkBox;
        }

        public UIElement CreateColorProperty(string name, Color value)
        {
            if (properties.ContainsKey(name))
                throw new Exception("Property " + name + " already exists!");

            properties.Add(name, value);

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

            return colorPicker;
        }
        #endregion

        private void ApplyClose()
        {
            DialogResult = true;
            return;
        }
    }
}
