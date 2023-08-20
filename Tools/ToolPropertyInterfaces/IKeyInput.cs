using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP
{
    internal interface IKeyInput
    {
        public abstract void OnPressedKey(Key key, ModifierKeys modifierKeys);
    }
}
