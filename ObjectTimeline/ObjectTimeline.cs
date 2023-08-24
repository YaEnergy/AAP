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
        public bool CanRollback
        {
            get => timeline.Count > 1;
        }

        public bool CanRollforward
        {
            get => future.Count <= 0;
        }

        public delegate void TimeTravelEvent(ObjectTimeline sender);
        public event TimeTravelEvent? Rolledback;

        public event TimeTravelEvent? Rolledforward;

        public delegate void NewTimePointEvent(ObjectTimeline sender);
        public event NewTimePointEvent? TimePointCreated;

        private readonly Stack<ICloneable?> timeline = new();
        private readonly Stack<ICloneable?> future = new();

        private readonly ITimelineObject timelineObject;

        /// <summary>
        /// TimelineObject must include a method to DEEP CLONE the object.
        /// </summary>
        public ObjectTimeline(ITimelineObject timelineObject)
        {
            this.timelineObject = timelineObject;

            NewTimePoint();
        }

        /// <summary>
        /// Saves the object at a new time point to rollback and rollforward to later.
        /// </summary>
        public void NewTimePoint()
        {
            if (future.Count > 0)
                future.Clear();

            ICloneable deepCopyCloneable = (ICloneable)timelineObject.Clone();
            timeline.Push(deepCopyCloneable);

            ConsoleLogger.Log("Created new time point");
            TimePointCreated?.Invoke(this);
        }

        /// <summary>
        /// Rolls the timeline object back to the previous time point, if possible.
        /// </summary>
        public void Rollback()
        {
            if (timeline.Count <= 1)
                return;

            ICloneable? rolledbackTimeObject = timeline.Pop();
            ICloneable? newCurrentTimeObject = timeline.Peek();

            future.Push(rolledbackTimeObject);

            if (newCurrentTimeObject != null)
            {
                object clone = newCurrentTimeObject.Clone();
                timelineObject.CopyPropertiesOf(clone);
            }
            else
                throw new NullReferenceException($"Timepoint in timeline {this} is null!");

            ConsoleLogger.Log("Rolled back timepoint");
            Rolledback?.Invoke(this);
        }

        /// <summary>
        /// Rolls the timeline object back to the next time point, if possible.
        /// </summary>
        public void Rollforward()
        {
            if (future.Count <= 0)
                return;

            ICloneable? newCurrentTimeObject = future.Pop();

            timeline.Push(newCurrentTimeObject);

            if (newCurrentTimeObject != null)
            {
                object clone = newCurrentTimeObject.Clone();
                timelineObject.CopyPropertiesOf(clone);
            }
            else
                throw new NullReferenceException($"Timepoint in timeline {this} is null!");

            ConsoleLogger.Log("Rolled forward time point");
            Rolledforward?.Invoke(this);
        }
    }
}
