using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Filters
{
    public abstract class Filter
    {
        /// <summary>
        /// Name of this filter.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Description of this filter.
        /// </summary>
        public abstract string Description { get; }

        public abstract void Apply();
    }
}
