using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Threading;

namespace MHApi.Threading
{
    /// <summary>
    /// Instantiation of WorkerT that allows the thread job
    /// to execute code on the parents thread via a dispatcher object
    /// </summary>
    public class Worker : WorkerT<Dispatcher>
    {
        public Worker(ThreadJob job, bool autostart, int joinTimeout) : base(job, Dispatcher.CurrentDispatcher, autostart, joinTimeout)
        { }

        public Worker(ThreadJob job) : this(job, true, 500)
        { }

        public Worker(ThreadJob job, bool autostart) : this(job, autostart, 500)
        { }
    }
}
