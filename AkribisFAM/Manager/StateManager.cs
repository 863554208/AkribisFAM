using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.WorkStation;

namespace AkribisFAM.Manager
{
    public class StateManager
    {
        private static StateManager _instance;

        public static StateManager Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new StateManager();
                    }
                }
                return _instance;
            }
        }

        public enum StateCode
        {
            Init = 0,
            RUNNING = 1,
            STOPPED = 2,
            MAINTENANCE = 3,
            IDLE = 4,
        }

        public Dictionary<StateCode, string> StateDict = new Dictionary<StateCode, string>
        {
            { StateCode.RUNNING, "RUNNING" },
            { StateCode.STOPPED, "STOPPED" },
            { StateCode.MAINTENANCE, "MAINTENANCE" },
            { StateCode.IDLE, "IDLE" }
        };

        public int Guarding;
        public StateCode State;

        public DateTime RunningEnd;
        public DateTime StoppedEnd;
        public DateTime MaintenanceEnd;
        public DateTime IdleEnd;

        public DateTime RunningStart;     
        public DateTime StoppedStart;    
        public DateTime MaintenanceStart; 
        public DateTime IdleStart;

        public void DetectRemainBoard() {
            while (true)
            {
                int cnt = 0;
                cnt += LaiLiao.Current.board_count;
                cnt += ZuZhuang.Current.board_count;
                cnt += FuJian.Current.board_count;
                cnt += Reject.Current.board_count;
                if (cnt == 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        public void DetectTimeDeltaThread()
        {
            RunningEnd = DateTime.Now;
            StoppedEnd = DateTime.Now;
            MaintenanceEnd = DateTime.Now;
            IdleEnd = DateTime.Now;

            RunningStart = DateTime.Now;
            StoppedStart = DateTime.Now;
            MaintenanceStart = DateTime.Now;
            IdleStart = DateTime.Now;

            DateTime tRunningEnd = RunningEnd;
            DateTime tStoppedEnd = StoppedEnd;
            DateTime tMaintenanceEnd = MaintenanceEnd;
            DateTime tIdleEnd = IdleEnd;

            Task.Run(new Action(() =>
                    {
                        while (true)
                        {
                            if (RunningEnd > RunningStart && tRunningEnd != RunningEnd)
                            {
                                //log or database
                                TimeSpan delta1 = RunningEnd - RunningStart;
                                tRunningEnd = RunningEnd;
                            }
                            if (StoppedEnd > StoppedStart && tStoppedEnd != StoppedEnd)
                            {
                                //log or database
                                TimeSpan delta2 = StoppedEnd - StoppedStart;
                                tStoppedEnd = StoppedEnd;
                            }
                            if (MaintenanceEnd > MaintenanceStart && tMaintenanceEnd != MaintenanceEnd)
                            {
                                //log or database
                                TimeSpan delta3 = MaintenanceEnd - MaintenanceStart;
                                tMaintenanceEnd = MaintenanceEnd;
                            }
                            if (IdleEnd > IdleStart && tIdleEnd != IdleEnd)
                            {
                                //log or database
                                TimeSpan delta4 = IdleEnd - IdleStart;
                                tIdleEnd = IdleEnd;

                            }
                            Thread.Sleep(200);
                        }
                    }
                ));
        }
    }
}
