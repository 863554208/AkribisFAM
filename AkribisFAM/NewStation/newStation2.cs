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
using System;

namespace AkribisFAM.NewStation
{
    public class newStation2 :WorkStation.WorkStationBase
    {
        private static newStation2 _instance;

        public override string Name => nameof(newStation2);

        public static newStation2 Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new newStation2();
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
    }
}
