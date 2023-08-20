using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public delegate void ReceivedTextInputEvent(object? sender, string text);

    internal interface ITextInput
    {
        public event ReceivedTextInputEvent? ReceivedTextInput;

        public abstract void OnTextInput(string text);
    }
}
