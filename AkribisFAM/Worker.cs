using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM
{
    public class Worker
    {
        System.Threading.Thread _workerThread = null;
        ManualResetEvent _stopSignal = new ManualResetEvent(false);
        System.Threading.ThreadStart _action;
        bool _isBackground;


        public bool ShoudStop
        {
            get
            {
                return _stopSignal.WaitOne(0, false);
            }
        }

        public bool WaitStopSignal(TimeSpan timeout)
        {
            return _stopSignal.WaitOne(timeout, false);
        }

        public bool WaitStopSignal(int millisecondsTimeout)
        {
            return _stopSignal.WaitOne(millisecondsTimeout, false);
        }

        public Worker(System.Threading.ThreadStart action)
            : this(action, true)
        { }

        public Worker(System.Threading.ThreadStart action, bool isBackground)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
            _isBackground = isBackground;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            _stopSignal.Reset();
            _workerThread = new System.Threading.Thread(_action);
            _workerThread.IsBackground = _isBackground;
            _workerThread.Start();

            Trace.WriteLine(" Worker.cs  Starts  _workerThread.IsAlive :" + _workerThread.IsAlive.ToString());
        }

        public void Stop()
        {
            Trace.WriteLine(" Worker.cs  IsRunning:");

            if (!IsRunning)
                return;

            RequestStop();
        }

        public void RequestStop()
        {
            if (!IsRunning)
                return;
            _stopSignal.Set();
        }

        public bool IsRunning
        {
            get
            {

                if (_workerThread == null)
                    return false;

                return _workerThread.IsAlive;
            }
        }

    }
}
