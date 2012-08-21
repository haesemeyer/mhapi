using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace MHApi.Threading
{
    public class Worker : IDisposable
    {
        /// <summary>
        /// The autoreset event used to signal the thread to stop executing
        /// </summary>
        AutoResetEvent stop;

        /// <summary>
        /// The actual thread running our thread job
        /// </summary>
        Thread thread;

        /// <summary>
        /// The dispatcher of the background thread to allow executing UI changes on the UI thread
        /// </summary>
        Dispatcher dispatcher;

        /// <summary>
        /// The number of ms we wait for the thread-join before throwing an exception
        /// </summary>
        public int JoinTimeout { get; private set; }

        /// <summary>
        /// Indicates if the thread is started
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Delegate for the client thread procedure
        /// </summary>
        /// <param name="stop">The autoreset event used to communicate with the thread</param>
        /// <param name="dispatcher">The dispatcher of the threads owner</param>
        public delegate void ThreadJob(AutoResetEvent stop, Dispatcher dispatcher);

        /// <summary>
        /// The thread job to execute
        /// </summary>
        private ThreadJob threadJob; 


        /// <summary>
        /// Constructor with autostart
        /// </summary>
        /// <param name="job">The method to execute from the thread</param>
        public Worker(ThreadJob job)
        {
            JoinTimeout = 500;
            threadJob = job;
            init(true);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="job">The method to execute from the thread</param>
        /// <param name="autostart">Whether to automatically or manually start the thread</param>
        public Worker(ThreadJob job, bool autostart)
        {
            JoinTimeout = 500;
            threadJob = job;
            init(autostart);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="job">The method to execute from the thread</param>
        /// <param name="autostart">Whether to automatically or manually start the thread</param>
        /// <param name="joinTimeout">The time to wait for the thread join on disposal or stop</param>
        public Worker(ThreadJob job,bool autostart, int joinTimeout)
        {
            JoinTimeout = joinTimeout;
            threadJob = job;
            init(autostart);
        }

        /// <summary>
        /// Manual starting of the thread
        /// </summary>
        public void Start()
        {
            if (isDisposed)
                throw new ApplicationException("Attempted to start disposed worker thread");
            if (IsRunning)
                throw new ApplicationException("Attempted to start already running thread");
            thread.Start();
            IsRunning = true;
        }

        /// <summary>
        /// Manual stop of the thread
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                stop.Set();
                if (!thread.Join(JoinTimeout))
                {
                    throw new ApplicationException("ThreadJoinFailed");
                }
            }
            IsRunning = false;
        }

        /// <summary>
        /// Initialization routine called by the constructors
        /// </summary>
        /// <param name="autostart">Whether to automatically or manually start the thread</param>
        private void init(bool autostart)
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            stop = new AutoResetEvent(false);
            thread = new Thread(threadProc);
            thread.Name = threadJob.Method.ToString();
            thread.IsBackground = true;
            if (autostart)
            {
                thread.Start();
                IsRunning = true;
            }
        }

        private void threadProc()
        {
            threadJob.Invoke(stop,dispatcher);
            IsRunning = false;
        }



        #region IDisposable Members

        private bool isDisposed;
        
        public void Dispose()
        {
            if (isDisposed)
                return;
            if (IsRunning)
            {
                stop.Set();
                if (!thread.Join(JoinTimeout))
                {
                    throw new ApplicationException("ThreadJoinFailed");
                }
            }
            isDisposed = true;
        }

        ~Worker()
        {
            Dispose();
        }

        #endregion
    }
}
