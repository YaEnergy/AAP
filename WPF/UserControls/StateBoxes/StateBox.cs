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
    public class StateBox : FrameworkElement
    {
        protected readonly VisualCollection _children;

        private readonly DrawingVisual backgroundVisual = new();

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        #region Brushes
        public static readonly DependencyProperty UnhighlightedDisabledProperty =
        DependencyProperty.Register(
            name: "UnhighlightedDisabled",
            propertyType: typeof(Brush),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.White, OnBackgroundDrawPropertyChangedCallBack));

        public Brush UnhighlightedDisabled
        {
            get => (Brush)GetValue(UnhighlightedDisabledProperty);
            set
            {
                if (UnhighlightedDisabled == value)
                    return;

                SetValue(UnhighlightedDisabledProperty, value);
            }
        }

        public static readonly DependencyProperty HighlightedDisabledProperty =
        DependencyProperty.Register(
            name: "HighlightedDisabled",
            propertyType: typeof(Brush),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Gray, OnBackgroundDrawPropertyChangedCallBack));

        public Brush HighlightedDisabled
        {
            get => (Brush)GetValue(HighlightedDisabledProperty);
            set
            {
                if (HighlightedDisabled == value)
                    return;

                SetValue(HighlightedDisabledProperty, value);
            }
        }

        public static readonly DependencyProperty UnhighlightedEnabledProperty =
        DependencyProperty.Register(
            name: "UnhighlightedEnabled",
            propertyType: typeof(Brush),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Green, OnBackgroundDrawPropertyChangedCallBack));

        public Brush UnhighlightedEnabled
        {
            get => (Brush)GetValue(UnhighlightedEnabledProperty);
            set
            {
                if (UnhighlightedEnabled == value)
                    return;

                SetValue(UnhighlightedEnabledProperty, value);
            }
        }

        public static readonly DependencyProperty HighlightedEnabledProperty =
        DependencyProperty.Register(
            name: "HighlightedEnabled",
            propertyType: typeof(Brush),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.DarkGreen, OnBackgroundDrawPropertyChangedCallBack));

        public Brush HighlightedEnabled
        {
            get => (Brush)GetValue(HighlightedEnabledProperty);
            set
            {
                if (HighlightedEnabled == value)
                    return;

                SetValue(HighlightedEnabledProperty, value);
            }
        }

        public static readonly DependencyProperty BorderProperty =
        DependencyProperty.Register(
            name: "Border",
            propertyType: typeof(Brush),
            ownerType: typeof(StateBox),
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
            ownerType: typeof(StateBox),
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

        public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(
            name: "State",
            propertyType: typeof(bool),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, OnStatePropertyChangedCallBack));

        public bool State
        {
            get => (bool)GetValue(StateProperty);
            set
            {
                if (State == value)
                    return;

                SetValue(StateProperty, value);
            }
        }

        public delegate void StateChangedEvent(StateBox sender, bool state);
        public event StateChangedEvent? OnStateChanged;

        public static readonly DependencyProperty AllowManualDisableProperty =
        DependencyProperty.Register(
            name: "AllowManualDisable",
            propertyType: typeof(bool),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: false));

        public bool AllowManualDisable
        {
            get => (bool)GetValue(AllowManualDisableProperty);
            set
            {
                if (AllowManualDisable == value)
                    return;

                SetValue(AllowManualDisableProperty, value);
            }
        }

        private bool highlighted = false;
        public bool Highlighted
        {
            get => highlighted;
            private set
            {
                if (value == highlighted)
                    return;

                highlighted = value;

                DrawBackground();
            }
        }

        public static readonly DependencyProperty StateCommandProperty =
        DependencyProperty.Register(
            name: "StateCommand",
            propertyType: typeof(EventArgsCommand<bool>),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public EventArgsCommand<bool>? StateCommand 
        { 
            get => (EventArgsCommand<bool>?)GetValue(StateCommandProperty); 
            set
            {
                if (StateCommand == value)
                    return;

                SetValue(StateCommandProperty, value);
            }
        }
        
        private void StateChanged(StateBox sender, bool state)
            => StateCommand?.Execute(sender, state);

        private void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            Brush fillBrush;
            if (Highlighted)
                fillBrush = State ? HighlightedEnabled : HighlightedDisabled;
            else
                fillBrush = State ? UnhighlightedEnabled : UnhighlightedDisabled;

            dc.DrawRectangle(fillBrush, new(Border, BorderThickness), new(0, 0, ActualWidth, ActualHeight));
        }

        public StateBox()
        {
            _children = new(this)
            {
                backgroundVisual
            };

            DrawBackground();

            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            SizeChanged += (sender, e) => DrawBackground();

            OnStateChanged += StateChanged;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
            => Highlighted = true;

        private void OnMouseLeave(object sender, MouseEventArgs e)
            => Highlighted = false;

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AllowManualDisable && State)
                return;

            State = !State;
        }

        #region Draw Property Changed Callbacks
        private static void OnBackgroundDrawPropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not StateBox box)
                return;

            box.DrawBackground();
        }

        #endregion

        private static void OnStatePropertyChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not StateBox box)
                return;

            box.DrawBackground();

            box.OnStateChanged?.Invoke(box, (bool)e.NewValue);
        }
    }
}
