using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Timelines
{
    /// <summary>
    /// Rolls ITimelineObjects backwards and forwards to saved time points without creating an entirely new object.
    /// TimelineObject must include a method to DEEP CLONE the object.
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

        public delegate void TimeTravelEvent(ObjectTimeline sender);
        public event TimeTravelEvent? Rolledback;

        public event TimeTravelEvent? Rolledforward;

        public delegate void NewTimePointEvent(ObjectTimeline sender);
        public event NewTimePointEvent? TimePointCreated;

        private int timePoint = 0;
        private readonly ICloneable?[] timeline;
        private readonly ITimelineObject timelineObject;

        /// <summary>
        /// TimelineObject must include a method to DEEP CLONE the object.
        /// </summary>
        public ObjectTimeline(ITimelineObject timelineObject, int sizeLimit)
        {
            this.timelineObject = timelineObject;
            this.timeline = new ICloneable?[sizeLimit];
            
            timeline[timePoint] = (ICloneable)timelineObject.Clone();
            ConsoleLogger.Log("Created timepoint 0");
            TimePointCreated?.Invoke(this);
        }

        /// <summary>
        /// Saves the object at a new time point to rollback and rollforward to later.
        /// </summary>
        public void NewTimePoint()
        {
            if (timePoint == SizeLimit - 1)
            {
                //Shift all elements in the timeline down by one, getting rid of element index 0 and having index SizeLimit - 1 as null
                for (int i = 0; i < SizeLimit - 1; i++)
                    timeline[i] = timeline[i + 1];

                timeline[SizeLimit - 1] = null;
            }
            else
                timePoint++;

            ICloneable deepCopyCloneable = (ICloneable)timelineObject.Clone();
            timeline[timePoint] = deepCopyCloneable;

            //Destroy now incorrect future, if there is one
            if (timePoint != Size - 1)
                for (int i = Math.Clamp(timePoint + 1, 0, SizeLimit - 1); i < SizeLimit; i++)
                    timeline[i] = null;

            ConsoleLogger.Log("Created new time point " +  timePoint);
            TimePointCreated?.Invoke(this);
        }

        /// <summary>
        /// Rolls the timeline object back to the previous time point, if possible.
        /// </summary>
        public void Rollback()
        {
            if (!CanRollback)
                return;

            timePoint--;

            ICloneable? timeObjectCloneable = timeline[timePoint];
            if (timeObjectCloneable != null)
            {
                object clone = timeObjectCloneable.Clone();
                timelineObject.CopyPropertiesOf(clone);
            }
            else
                throw new NullReferenceException($"Timepoint {timePoint} in timeline {this} is null!");

            ConsoleLogger.Log("Rolled back to time point " + timePoint);
            Rolledback?.Invoke(this);
        }

        /// <summary>
        /// Rolls the timeline object back to the next time point, if possible.
        /// </summary>
        public void Rollforward()
        {
            if (!CanRollforward)
                return;

            timePoint++;

            ICloneable? timeObjectCloneable = timeline[timePoint];
            if (timeObjectCloneable != null)
            {
                object clone = timeObjectCloneable.Clone();
                timelineObject.CopyPropertiesOf(clone);
            }
            else
                throw new NullReferenceException($"Timepoint {timePoint} in timeline {this} is null!");

            ConsoleLogger.Log("Rolled forward to time point " + timePoint);
            Rolledforward?.Invoke(this);
        }

        /// <summary>
        /// Destroys the past of the timeline object, with the current time point set to index 0.
        /// </summary>
        public void DestroyPast()
        {
            int pastAmount = timePoint;
            for (int i = 0; i < timePoint; i++)
                timeline[i] = null;

            for(int i = timePoint; i < SizeLimit; i++)
                timeline[i - pastAmount] = timeline[i];

            timePoint = 0;
        }
    }
}
