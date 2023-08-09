using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.BackgroundTasks
{
    public readonly struct BackgroundTaskUpdateArgs
    {
        public readonly string CurrentObjective;
        public readonly bool IsDeterminate;

        public BackgroundTaskUpdateArgs(string currentObjective = "", bool isDeterminate = false) 
        {
            CurrentObjective = currentObjective;
            IsDeterminate = isDeterminate;
        }
    }
}
