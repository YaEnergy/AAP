using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace AAP
{
    /// <summary>
    /// Interaction logic for ImageStateBox.xaml
    /// </summary>
    public partial class ImageStateBox : System.Windows.Controls.UserControl
    {
        private ImageStateBoxViewModel viewModel;

        private ImageSource? boxImageSource = null;
        public ImageSource? BoxImageSource
        {
            get => boxImageSource;
            set
            {
                if (value == boxImageSource)
                    return;

                boxImageSource = value;
                viewModel.BoxImageSource = boxImageSource;
            }
        }

        private bool state = false;
        public bool State
        {
            get => state;
            set
            {
                if (value == state)
                    return;

                state = value;
                viewModel.BoxBrush = State ? EnabledStateBrushHighlighted : DisabledStateBrushHighlighted;
                OnStateChanged?.Invoke(this, state);
            }
        }

        private bool highlighted = false;
        public bool Highlighted
        {
            get => highlighted;
            set
            {
                if (value == highlighted)
                    return;

                highlighted = value;

                if (value)
                    viewModel.BoxBrush = State ? EnabledStateBrushHighlighted : DisabledStateBrushHighlighted;
                else
                    viewModel.BoxBrush = State ? EnabledStateBrush : DisabledStateBrush;
            }
        }

        public bool AllowManualDisable { get; set; } = true;

        public System.Windows.Media.Brush DisabledStateBrush { get; } = System.Windows.Media.Brushes.White;
        public System.Windows.Media.Brush DisabledStateBrushHighlighted { get; } = System.Windows.Media.Brushes.LightGray;
        public System.Windows.Media.Brush EnabledStateBrush { get; } = System.Windows.Media.Brushes.DarkGreen;
        public System.Windows.Media.Brush EnabledStateBrushHighlighted { get; } = System.Windows.Media.Brushes.DarkOliveGreen;

        public delegate void StateChangedEvent(ImageStateBox sender, bool state);
        public event StateChangedEvent? OnStateChanged;

        public ImageStateBox(bool startingState = false, bool allowManualDisable = true, ImageSource? imageSource = null)
        {
            InitializeComponent();

            viewModel = (ImageStateBoxViewModel)FindResource("ViewModel");

            state = startingState;
            highlighted = false;
            boxImageSource = imageSource;
            AllowManualDisable = allowManualDisable;

            if (highlighted)
                viewModel.BoxBrush = State ? EnabledStateBrushHighlighted : DisabledStateBrushHighlighted;
            else
                viewModel.BoxBrush = State ? EnabledStateBrush : DisabledStateBrush;
        }

        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
            => Highlighted = true;

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
            => Highlighted = false;

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AllowManualDisable && State)
                return;

            State = !State;
        }
    }
}
