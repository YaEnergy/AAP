using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP.UI
{
    /// <summary>
    ///     Implements the ICommand interface.
    /// </summary>
    public class ActionCommand : ICommand
    {
        private readonly Action<object?> _action;
        public event EventHandler? CanExecuteChanged;

        public ActionCommand(Action<object?> executeAction)
        {
            _action = executeAction;
        }

        public bool CanExecute(object? parameter = null)
        {
            return true;
        }

        public void Execute(object? parameter = null)
        {
            _action(parameter);
            Console.WriteLine("Executed ActionCommand!");
        }
    }
}
