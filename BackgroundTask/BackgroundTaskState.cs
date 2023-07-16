using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public readonly struct BackgroundTaskState
    {
        public readonly string CurrentObjective;
        public readonly bool IsDeterminate;

        public BackgroundTaskState(string currentObjective = "", bool isDeterminate = false) 
        {
            CurrentObjective = currentObjective;
            IsDeterminate = isDeterminate;
        }
    }
}
