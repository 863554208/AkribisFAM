using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.WorkStation
{
    public abstract class WorkStationBase
    {

        public abstract void Initialize();
        public abstract void AutoRun();
        public abstract void ReturnZero();
        public abstract bool Ready();


    }
}
