using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Timelines
{
    public interface ITimelineObject : ICloneable
    {
        public abstract void CopyPropertiesOf(object obj);
    }
}
