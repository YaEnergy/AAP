using AAP.UI.Controls;
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
    /// Interaction logic for ObjectPropertiesWindow.xaml
    /// </summary>
    public partial class ObjectPropertiesWindow : Window
    {
        private object inspectObject;
        public object InspectObject
        {
            get => inspectObject;
            set
            {
                if (inspectObject == value)
                    return;

                inspectObject = value;

                PropertyList.Children.Clear();

                Type objectType = value.GetType();
                if (!objectType.IsClass)
                    throw new ArgumentException("classObject must be a class!");

                foreach (PropertyInfo propertyInfo in objectType.GetProperties())
                    if (propertyInfo.CanWrite)
                        AddEditableProperty(propertyInfo);
            }
        }

        private readonly Dictionary<PropertyInfo, object?> properties = new();

        public ObjectPropertiesWindow(object classObject)
        {
            InitializeComponent();

            Type objectType = classObject.GetType();
            if (!objectType.IsClass)
                throw new ArgumentException("classObject must be a class!");

            inspectObject = classObject;

            foreach (PropertyInfo propertyInfo in objectType.GetProperties())
                if (propertyInfo.CanWrite)
                    AddEditableProperty(propertyInfo);
        }

        public void AddEditableProperty(PropertyInfo propertyInfo)
        {     
            if (propertyInfo.PropertyType == typeof(string))
                PropertyList.Children.Add(CreateStringPropertyUIElement(propertyInfo));
            else if (propertyInfo.PropertyType == typeof(int))
                PropertyList.Children.Add(CreateIntPropertyUIElement(propertyInfo));
            else if (propertyInfo.PropertyType == typeof(double))
                PropertyList.Children.Add(CreateDoublePropertyUIElement(propertyInfo));
            else if (propertyInfo.PropertyType == typeof(Color))
                PropertyList.Children.Add(CreateColorPropertyUIElement(propertyInfo));

            properties.Add(propertyInfo, propertyInfo.GetValue(InspectObject));
        }

        #region PropertyUIElement type creations
        private UIElement CreateStringPropertyUIElement(PropertyInfo propertyInfo)
        {
            WrapPanel propertyPanel = new();

            Label label = new();
            label.Content = propertyInfo.Name;

            TextBox textBox = new();
            textBox.Width = 128;
            textBox.Height = 22;

            textBox.Text = (string?)propertyInfo.GetValue(InspectObject) ?? string.Empty;
            textBox.LostFocus += (sender, e) => SetProperty(propertyInfo, textBox.Text);

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(textBox);

            return propertyPanel;
        }

        private UIElement CreateIntPropertyUIElement(PropertyInfo propertyInfo)
        {
            WrapPanel propertyPanel = new();

            Label label = new();
            label.Content = propertyInfo.Name;

            TextBox textBox = new();
            textBox.Width = 64;
            textBox.Height = 22;

            int? startValue = (int?)propertyInfo.GetValue(InspectObject);

            string textVal = startValue.ToString() ?? string.Empty;
            if (startValue != null)
                textBox.Text = startValue.ToString();

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (int.TryParse(textBoxSender.Text, out int value))
                {
                    SetProperty(propertyInfo, value);
                    textVal = textBoxSender.Text;
                }
                else
                    textBoxSender.Text = textVal;
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(textBox);

            return propertyPanel;
        }

        private UIElement CreateDoublePropertyUIElement(PropertyInfo propertyInfo)
        {
            WrapPanel propertyPanel = new();

            Label label = new();
            label.Content = propertyInfo.Name;

            TextBox textBox = new();
            textBox.Width = 64;
            textBox.Height = 22;

            double? startValue = (double?)propertyInfo.GetValue(InspectObject);

            string textVal = startValue.ToString() ?? string.Empty;
            if (startValue != null)
                textBox.Text = startValue.ToString();

            textBox.LostFocus += (sender, e) =>
            {
                if (sender is not TextBox textBoxSender)
                    return;

                if (double.TryParse(textBoxSender.Text, out double value))
                {
                    SetProperty(propertyInfo, value);
                    textVal = textBoxSender.Text;
                }
                else
                    textBoxSender.Text = textVal;
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(textBox);

            return propertyPanel;
        }

        private UIElement CreateColorPropertyUIElement(PropertyInfo propertyInfo)
        {
            WrapPanel propertyPanel = new();

            Label label = new();
            label.Content = propertyInfo.Name;

            ColorPicker colorPicker = new();

            Color? value = (Color?)propertyInfo.GetValue(InspectObject);

            if (value != null)
            {
                SolidColorBrush brush = new(value.Value);
                brush.Freeze();

                colorPicker.PickedColor = brush;
            }

            colorPicker.ColorChanged += (sender, color) =>
            {
                if (sender is not ColorPicker colorPickerSender)
                    return;

                SetProperty(propertyInfo, color);
            };

            propertyPanel.Children.Add(label);
            propertyPanel.Children.Add(colorPicker);

            return propertyPanel;
        }
        #endregion

        public void SetProperty(PropertyInfo propertyInfo, object? value)
        {
            if (!properties.ContainsKey(propertyInfo))
                throw new InvalidOperationException($"Property Info {propertyInfo.Name} doesn't exist in properties!");

            properties[propertyInfo] = value;
        }

        public void ApplyProperties()
        {
            foreach(KeyValuePair<PropertyInfo, object?> kvp in properties)
            {
                if (!kvp.Key.CanWrite)
                    throw new InvalidOperationException("Can't write to this Property!");

                kvp.Key.SetValue(InspectObject, kvp.Value);
            }
        }
    }
}
