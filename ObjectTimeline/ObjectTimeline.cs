﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Timelines
{
    /// <summary>
    /// Rolls ITimelineObjects backwards and forwards to saved time points without creating an entirely new object.
    /// </summary>
    public class ObjectTimeline
    {
        public int SizeLimit
        {
            get => timeline.Length;
        }

        public int Size
        {
            get => timeline.Count(x => x != null);
        }

        public bool CanRollback
        {
            get => timePoint != 0;
        }

        public bool CanRollforward
        {
            get => timePoint != Size - 1;
        }

        private int timePoint = 0;
        private readonly object?[] timeline;
        private readonly ITimelineObject timelineObject;
        
        public ObjectTimeline(ITimelineObject timelineObject, int sizeLimit)
        {
            this.timelineObject = timelineObject;
            this.timeline = new object?[sizeLimit];
            
            timeline[timePoint] = timelineObject.Clone();
        }

        /// <summary>
        /// Saves the object at a new time point to rollback and rollforward to later.
        /// </summary>
        public void NewTimePoint()
        {
            //Destroy now incorrect future
            if(Size - 1 > timePoint)
                for (int i = Size; i < SizeLimit; i++)
                    timeline[i] = null;
            
            if (timePoint == SizeLimit - 1)
            {
                //Shift all elements in the timeline down by one, getting rid of element index 0 and having index SizeLimit - 1 as null
                for (int i = 0; i < SizeLimit; i++)
                    if(i != SizeLimit - 1)
                        timeline[i] = timeline[i + 1];
                    else
                        timeline[i] = null;
            }
            else
                timePoint++;
            
            object deepCopy = timelineObject.Clone();
            timeline[timePoint] = deepCopy;
        }

        /// <summary>
        /// Rolls the timeline object back to the previous time point, if possible.
        /// </summary>
        public void Rollback()
        {
            if (!CanRollback)
                return;

            timePoint--;

            object? timeObject = timeline[timePoint];
            if (timeObject != null)
                timelineObject.CopyPropertiesOf(timeObject);
        }

        /// <summary>
        /// Rolls the timeline object back to the next time point, if possible.
        /// </summary>
        public void Rollforward()
        {
            if (!CanRollforward)
                return;

            timePoint++;

            object? timeObject = timeline[timePoint];
            if (timeObject != null)
                timelineObject.CopyPropertiesOf(timeObject);
        }
    }
}
