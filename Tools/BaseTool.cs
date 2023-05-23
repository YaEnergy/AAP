using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public abstract class Tool
    {
        protected Point StartPoint = new(0, 0);
        protected Point CurrentPoint = new(0, 0);
        protected Point EndPoint = new(0, 0);

        public virtual void ActivateStart(Point artMatrixPosition) //Location has the x and y of the character on the canvas clicked
            => StartPoint = artMatrixPosition;

        public virtual void ActivateUpdate(Point artMatrixPosition)
            => CurrentPoint = artMatrixPosition;

        public virtual void ActivateEnd()
            => EndPoint = CurrentPoint;
    }
}
