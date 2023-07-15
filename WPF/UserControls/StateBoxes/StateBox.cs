﻿using Newtonsoft.Json.Linq;
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

        private bool state = false;
        public bool State
        {
            get => state;
            set
            {
                if (value == state)
                    return;

                state = value;

                if (Highlighted)
                    BoxBrush = State ? EnabledStateBrushHighlighted : DisabledStateBrushHighlighted;
                else
                    BoxBrush = State ? EnabledStateBrush : DisabledStateBrush;

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
                    BoxBrush = State ? EnabledStateBrushHighlighted : DisabledStateBrushHighlighted;
                else
                    BoxBrush = State ? EnabledStateBrush : DisabledStateBrush;
            }
        }

        private Brush boxBrush = Brushes.White;
        public Brush BoxBrush
        {
            get => boxBrush;
            set
            {
                if (value == boxBrush)
                    return;

                boxBrush = value;
                DrawBackground();
            }
        }

        public bool AllowManualDisable { get; set; } = true;

        public Brush DisabledStateBrush { get; } = Brushes.White;
        public Brush DisabledStateBrushHighlighted { get; } = Brushes.LightGray;
        public Brush EnabledStateBrush { get; } = Brushes.DarkGreen;
        public Brush EnabledStateBrushHighlighted { get; } = Brushes.DarkOliveGreen;

        public delegate void StateChangedEvent(StateBox sender, bool state);
        public event StateChangedEvent? OnStateChanged;

        public static readonly DependencyProperty StateCommandProperty =
        DependencyProperty.Register(
            name: "StateCommand",
            propertyType: typeof(EventArgsCommand<bool>),
            ownerType: typeof(StateBox),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public EventArgsCommand<bool>? StateCommand 
        { 
            get => (EventArgsCommand<bool>?)GetValue(StateCommandProperty); 
            set => SetValue(StateCommandProperty, value); 
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        /// <summary>
        /// Sets the state of the StateBox without invoking the OnStateChanged event.
        /// </summary>
        /// <param name="state"></param>
        public void ForceSetState(bool state)
        {
            if (this.state == state)
                return;

            this.state = state;

            if (Highlighted)
                BoxBrush = State ? EnabledStateBrushHighlighted : DisabledStateBrushHighlighted;
            else
                BoxBrush = State ? EnabledStateBrush : DisabledStateBrush;
        }

        private void StateChanged(StateBox sender, bool state)
            => StateCommand?.Execute(sender, state);

        private void DrawBackground()
        {
            using DrawingContext dc = backgroundVisual.RenderOpen();

            dc.DrawRectangle(boxBrush, null, new(0, 0, ActualWidth, ActualHeight));
        }

        public StateBox()
        {
            DisabledStateBrush.Freeze();
            DisabledStateBrushHighlighted.Freeze();
            EnabledStateBrush.Freeze();
            EnabledStateBrushHighlighted.Freeze();

            _children = new(this)
            {
                backgroundVisual
            };

            if (highlighted)
                BoxBrush = State ? EnabledStateBrushHighlighted : DisabledStateBrushHighlighted;
            else
                BoxBrush = State ? EnabledStateBrush : DisabledStateBrush;

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
    }
}
