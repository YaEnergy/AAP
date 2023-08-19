using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public abstract ToolType Type { get; }
        protected Point StartArtPos = new(0, 0);
        protected Point CurrentArtPos = new(0, 0);
        protected Point EndArtPos = new(0, 0);

        public Tool()
        {

        }

        protected virtual void UseStart(Point startArtPos)
        {
            return;
        }

        protected virtual void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            return;
        }

        protected virtual void UseEnd(Point startArtPos, Point endArtPos)
        {
            return;
        }

        public void ActivateStart(Point artMatrixPosition) //Location has the x and y of the character on the canvas clicked
        {
            StartArtPos = artMatrixPosition;
            CurrentArtPos = artMatrixPosition;
            EndArtPos = artMatrixPosition;

            UseStart(StartArtPos);

            OnActivateStart?.Invoke(this, StartArtPos);
        }

        public void ActivateUpdate(Point artMatrixPosition)
        {
            CurrentArtPos = artMatrixPosition;

            UseUpdate(StartArtPos, CurrentArtPos);

            OnActivateUpdate?.Invoke(this, CurrentArtPos);
        }

        public void ActivateEnd()
        {
            EndArtPos = CurrentArtPos;

            UseEnd(StartArtPos, EndArtPos);

            OnActivateEnd?.Invoke(this, EndArtPos);
        }
    }
}
