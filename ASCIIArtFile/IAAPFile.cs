using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    internal interface IAAPFile<T>
    {
        public T FileObject { get; protected set; }
        public string FilePath { get; protected set; }

        public abstract void Import(BackgroundWorker? bgWorker = null);

        public abstract void Export(BackgroundWorker? bgWorker = null);

    }
}
