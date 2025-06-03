using System.Threading;

namespace AkribisFAM.WorkStation
{
    internal class TEST2 : WorkStationBase
    {

        private static TEST2 _instance;
        public static TEST2 Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new TEST2();
                    }
                }
                return _instance;
            }
        }

        public override string Name => nameof(TEST2);

        public override bool AutoRun()
        {
            return true;
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
