using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public delegate void PreviewChangedEvent(object? preview);

    internal interface IPreviewable<T>
    {
        public T Preview { get; set; }

        public event PreviewChangedEvent? OnPreviewChanged;
    }
}
