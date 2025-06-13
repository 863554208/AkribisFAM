using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.WorkStation
{
    public abstract class WorkStationBase : ViewModelBase
    {
        public abstract string Name { get; }
        public abstract void Initialize();
        public abstract bool AutoRun();
        public abstract void Paused();
        public abstract void ResetAfterPause();
        public ThreadStatus ThreadState = ThreadStatus.Init;
        public enum ThreadStatus
        {
            Init,
            Pausing,
            Paused,
            Resuming,
            Running,
            Stop,
        }
        public static int _movestep = 0;
        public static int _subMovestep = 0;
        public static int _recoveryMovestep = 0;
        public static int _subRecoveryMovestep = 0;

    }
}
