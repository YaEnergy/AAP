using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP
{
    /// <summary>
    ///     Implements the ICommand interface with a sender object and Type T.
    /// </summary>
    public class EventArgsCommand<T> : ICommand
    {
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
            Console.WriteLine("Executed EventArgsCommand!");
        }

        public void Execute(object? sender, T? args)
        {
            _action(sender, args);
            Console.WriteLine("Executed EventArgsCommand!");
        }
    }
}
