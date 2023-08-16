using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public abstract class Tool
    {
        public delegate void ActivateEvent(Tool tool, Point artPosition);
        public event ActivateEvent? OnActivateStart;

        public event ActivateEvent? OnActivateUpdate;

        public event ActivateEvent? OnActivateEnd;

        public abstract ToolType Type { get; protected set; }
        protected Point StartPoint = new(0, 0);
        protected Point CurrentPoint = new(0, 0);
        protected Point EndPoint = new(0, 0);

        public virtual void ActivateStart(Point artMatrixPosition) //Location has the x and y of the character on the canvas clicked
        {
            StartPoint = artMatrixPosition;
            OnActivateStart?.Invoke(this, StartPoint);
        }

        public virtual void ActivateUpdate(Point artMatrixPosition)
        {
            CurrentPoint = artMatrixPosition;
            OnActivateUpdate?.Invoke(this, CurrentPoint);
        }

        public virtual void ActivateEnd()
        {
            EndPoint = CurrentPoint;
            OnActivateEnd?.Invoke(this, EndPoint);
        }
    }
}
