﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP.UI
{
    /// <summary>
    ///     Implements the ICommand interface with a sender object and Type T.
    /// </summary>
    public class EventArgsCommand<T> : ICommand, INotifyPropertyChanged
    {
        private bool executable = true;
        public bool Executable
        {
            get => executable;
            set
            {
                if (executable == value)
                    return;

                executable = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                PropertyChanged?.Invoke(this, new(nameof(Executable)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Action<object?, T?> _action;
        public event EventHandler? CanExecuteChanged;

        public EventArgsCommand(Action<object?, T?> executeAction)
        {
            _action = executeAction;
        }

        public bool CanExecute(object? parameter = null)
        {
            return true;
        }

        public void Execute(object? sender)
        {
            _action(sender, default);
            ConsoleLogger.Log("Executed EventArgsCommand!");
        }

        public void Execute(object? sender, T? args)
        {
            _action(sender, args);
            ConsoleLogger.Log("Executed EventArgsCommand!");
        }
    }
}
