using System.Threading;

namespace AkribisFAM.WorkStation
{
    internal class TEST1 : WorkStationBase
    {

        private static TEST1 _instance;
        public static TEST1 Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new TEST1();
                    }
                }
                return _instance;
            }
        }

        public override string Name => nameof(TEST1);

        public override bool AutoRun()
        {
            return false;
        }

        public override void Initialize()
        {
            return;
        }

        public override void Paused()
        {
            return;
        }

        public override void ResetAfterPause()
        {
            return;
        }
    }
}
