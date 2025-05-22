using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using AAMotion;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Windows;
using static AAComm.Extensions.AACommFwInfo;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using System.CodeDom;
using AkribisFAM.WorkStation;

namespace AkribisFAM.NewStation 
{
    public class newStation1 : WorkStation.WorkStationBase
    {

        private static newStation1 _instance;

        public override string Name => nameof(LaiLiao);

        public static newStation1 Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new newStation1();
                    }
                }
                return _instance;
            }
        }

        public override void ReturnZero()
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override bool Ready()
        {
            return true;
        }


        public override void AutoRun(CancellationToken token)
        {
            while (true)
            {
                step1:
                    int a = 1;

                step2:

                step3:
                    int b = 1;

            }
        }
    }
}
