using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP.UI
{
    /// <summary>
    ///     Implements the ICommand interface.
    /// </summary>
    public class ActionCommand : ICommand, INotifyPropertyChanged
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

        private readonly Action<object?> _action;
        public event EventHandler? CanExecuteChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ActionCommand(Action<object?> executeAction)
        {
            _action = executeAction;
        }

        public bool CanExecute(object? parameter = null)
        {
            return Executable;
        }

        public void Execute(object? parameter = null)
        {
            _action(parameter);
            ConsoleLogger.Log("Executed ActionCommand!");
        }
    }
}
