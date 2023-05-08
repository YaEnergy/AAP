using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public abstract class Tool
    {
        protected Point MouseStartPoint = new(0, 0);

        public virtual void CanvasClick_Start(Point location) //Location has the x and y of the character on the canvas clicked
            => MouseStartPoint = location;

        public virtual void CanvasClick_Update(Point location)
            => throw new NotImplementedException();

        public virtual void CanvasClick_End(Point location)
            => throw new NotImplementedException();
    }
}
