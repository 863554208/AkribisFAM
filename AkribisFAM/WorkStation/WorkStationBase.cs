using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.WorkStation
{
    public abstract class WorkStationBase
    {
        public abstract string Name { get; }
        public abstract void Initialize();
        public abstract bool AutoRun();
        public abstract void Paused();
        public ThreadStatus ThreadState = ThreadStatus.Init;
        public enum ThreadStatus
        {
            Init,
            Pausing,
            Paused,
            Running,
            Stop,
        }

    }
}
