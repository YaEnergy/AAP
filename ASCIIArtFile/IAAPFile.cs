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
        public abstract void Import(T file, BackgroundWorker? bgWorker = null);

        public abstract void Export(T file, BackgroundWorker? bgWorker = null);

    }
}
