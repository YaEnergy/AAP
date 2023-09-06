using AAP.BackgroundTasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    internal interface IAAPFile<T>
    {
        public T FileObject { get; protected set; }
        public string FilePath { get; protected set; }

        /// <summary>
        /// Sets properties of FileObject to the properties of the object imported from FilePath
        /// </summary>
        /// <param name="bgWorker"></param>
        public abstract void Import();

        /// <summary>
        /// Sets properties of FileObject to the properties of the object imported from FilePath
        /// </summary>
        public abstract Task ImportAsync(BackgroundTaskToken? taskToken = null);

        /// <summary>
        /// Writes FileObject to FilePath
        /// </summary>
        /// <param name="bgWorker"></param>
        /// <returns>True if export was a success, false if cancelled.</returns>
        public abstract void Export();

        /// <summary>
        /// Sets properties of FileObject to the properties of the object imported from FilePath
        /// </summary>
        public abstract Task ExportAsync(BackgroundTaskToken? taskToken = null);

    }
}
