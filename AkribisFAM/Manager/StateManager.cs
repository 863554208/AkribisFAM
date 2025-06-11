using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Helper;
using AkribisFAM.Models;
using AkribisFAM.WorkStation;
using YamlDotNet.Core.Tokens;

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
            IDLE = 4
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
        public int  RunningHourCnt;
        public int currentUPH;
        public int currentNG;

        public DateTime RunningEnd;
        public DateTime StoppedEnd;
        public DateTime MaintenanceEnd;
        public DateTime IdleEnd;

        public DateTime RunningStart;     
        public DateTime StoppedStart;    
        public DateTime MaintenanceStart; 
        public DateTime IdleStart;

        public TimeSpan RunningTime;
        public TimeSpan StoppedTime;
        public TimeSpan MaintenanceTime;
        public TimeSpan IdleTime;

        public TimeSpan totalRunningTime = TimeSpan.FromSeconds(0);
        public TimeSpan totalStoppedTime = TimeSpan.FromSeconds(0);
        public TimeSpan totalMaintenanceTime = TimeSpan.FromSeconds(0);
        public TimeSpan totalIdleTime = TimeSpan.FromSeconds(0);

        public int TotalInput;
        public int TotalOutputOK;
        public int TotalOutputNG;

        public double availability;
        public double perform;
        public double quality;
        public int UPH;
        public int Yield;
        public int PlannedUPH = 1200;
        public double PlannedProductionTime = 0;

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
                                totalRunningTime = totalRunningTime + delta1;
                                InsertOEEDataBase();
                                tRunningEnd = RunningEnd;
                            }
                            if (StoppedEnd > StoppedStart && tStoppedEnd != StoppedEnd)
                            {
                                //log or database
                                TimeSpan delta2 = StoppedEnd - StoppedStart;
                                totalStoppedTime = totalStoppedTime + delta2;
                                InsertOEEDataBase();
                                tStoppedEnd = StoppedEnd;
                            }
                            if (MaintenanceEnd > MaintenanceStart && tMaintenanceEnd != MaintenanceEnd)
                            {
                                //log or database
                                TimeSpan delta3 = MaintenanceEnd - MaintenanceStart;
                                totalMaintenanceTime = totalMaintenanceTime + delta3;
                                InsertOEEDataBase();
                                tMaintenanceEnd = MaintenanceEnd;
                            }
                            if (IdleEnd > IdleStart && tIdleEnd != IdleEnd)
                            {
                                //log or database
                                TimeSpan delta4 = IdleEnd - IdleStart;
                                totalIdleTime = totalIdleTime + delta4;
                                InsertOEEDataBase();
                                tIdleEnd = IdleEnd;

                            }
                            Thread.Sleep(200);
                        }
                    }
                ));
        }
        public void StateLightThread()
        {
            Task.Run(new Action(() =>
            {
                while (true)
                {
                    if (State == StateCode.IDLE)
                    {
                        if (ErrorManager.Current.ErrorCnt == 0) {
                            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, 1);
                            Thread.Sleep(500);
                            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, 0);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 1);
                            Thread.Sleep(500);
                            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 0);
                            Thread.Sleep(500);
                        }
                    }
                    else if (State == StateCode.RUNNING)
                    {
                        if (ErrorManager.Current.ErrorCnt == 0)
                        {
                            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, 1);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 1);
                            Thread.Sleep(500);
                            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 0);
                            Thread.Sleep(500);
                        }
                    }
                    else if (State == StateCode.MAINTENANCE)
                    {
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 1);
                        Thread.Sleep(500);
                    }
                    else if (State == StateCode.STOPPED)
                    {
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_0Tri_color_light_red, 1);
                        //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 1);
                        Thread.Sleep(500);
                    }
                }
            }
            ));
        }

        public void InsertOEEDataBase() {
            OeeRecord oee = new OeeRecord();
            oee.AlarmsCount = ErrorManager.Current.ErrorCnt;
            oee.Availability = availability;
            oee.DownTimeHr = StoppedTime.TotalHours + MaintenanceTime.TotalHours;
            oee.UpTimeHr = RunningTime.TotalHours + IdleTime.TotalHours;
            oee.ProductiveTimeHr = RunningTime.TotalHours;
            oee.Performance = perform;
            oee.TotalTimeHr = StoppedTime.TotalHours + IdleTime.TotalHours + MaintenanceTime.TotalHours + RunningTime.TotalHours;
            oee.Quality = quality;
            oee.PlannedUPH = PlannedUPH;
            oee.PlannedProductionTime = PlannedProductionTime;
            App.DbManager.AddOeeRecord(oee);
        }
    }
}
