using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AkribisFAM.Manager;
using static AkribisFAM.GlobalManager;
using AkribisFAM.CommunicationProtocol;
using static AkribisFAM.CommunicationProtocol.Task_RecheckCamreaFunction;
using AkribisFAM.Util;
using AkribisFAM.Windows;
using static AkribisFAM.Manager.MaterialManager;

namespace AkribisFAM.WorkStation
{
    internal class FuJian : WorkStationBase
    {
        private static int _movestep = 0;
        private static int _filmRemoveMovestep = 0;
        private static DateTime startTime = DateTime.Now;
        private static int _inspectMovestep = 0;
        private bool _isProcessOngoing = false;
        private static FuJian _instance;
        private static List<SinglePointExt> _movePoints = new List<SinglePointExt>();
        public override string Name => nameof(FuJian);

        private ErrorCode errorCode;
        public int board_count = 0;


        public static FuJian Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new FuJian();
                    }
                }
                return _instance;
            }
        }


        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }


        public bool ReadIO(IO_INFunction_Table index)
        {
            if (IOManager.Instance.INIO_status[(int)index] == 0)
            {
                return false;
            }
            else if (IOManager.Instance.INIO_status[(int)index] == 1)
            {
                return true;
            }
            else
            {
                ErrorManager.Current.Insert(ErrorCode.IOErr, $"Failed to read {index.ToString()}");
                return false;
            }
        }

        public void SetIO(IO_OutFunction_Table index, int value)
        {
            IOManager.Instance.IO_ControlStatus(index, value);
        }


        private int[] signalval = new int[10];
        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
            DateTime time = DateTime.Now;
            bool ret = false;
            errorCode = ErrorCode.WaitIO;
            int cnt = 0;
            for (int i = 0; i < signalval.Length; i++)
            {
                signalval[i] = 0;
            }
            while ((DateTime.Now - time).TotalMilliseconds < delta)
            {
                int validx = 0;
                if (cnt < 10)
                {
                    validx = cnt;
                }
                else
                {
                    validx = cnt % 10;
                }
                if (ReadIO(index) == value)
                {
                    signalval[validx] = 1;
                }
                else
                {
                    signalval[validx] = 0;
                }
                cnt++;
                if (signalval.Sum() >= 8)
                {
                    ret = true;
                    break;
                }
                Thread.Sleep(1);
            }

            return ret;
        }



        public int CheckState(bool state)
        {
            if (GlobalManager.Current.FuJian_exit) return 1;
            if (state)
            {
                GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
            }
            else
            {
                //报错
                GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 1;
                AutorunManager.Current.ToPause = true;
                ErrorManager.Current.Insert(errorCode);
            }
            GlobalManager.Current.FuJian_CheckState();
            WarningManager.Current.WaiFuJian();
            return 0;
        }

        public bool Tearing()
        {
            bool ret, ret1;
            int actionret;
            int cnt;
            //撕膜
            actionret = AkrAction.Current.MoveRecheckZ(GlobalManager.Current.SafeZPos.Z);
            errorCode = ErrorCode.TimeOut;
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            for (int i = 0; i < GlobalManager.Current.tearingPoints.Count; ++i)
            {
                //移动到穴位
                Logger.WriteLog("开始执行撕膜X");
                actionret = AkrAction.Current.MoveRecheckXY(GlobalManager.Current.tearingPoints[i].X, GlobalManager.Current.tearingPoints[i].Y);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                Logger.WriteLog("BBBBBBBBBBBBBB");
                //夹爪气缸打开
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0);
                //移动z轴下降
                Logger.WriteLog("AAAAAAAAAAAAA");
                actionret = AkrAction.Current.MoveRecheckZ(GlobalManager.Current.tearingPoints[i].Z);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                //检测到位信号
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, true);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, false);
                Logger.WriteLog("CCCCCCCCCCCC");
                if (CheckState(ret && ret1) == 1)
                {
                    return false;
                }
                //夹爪气缸夹取
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1);
                //检测到位信号
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, false);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, true);
                Logger.WriteLog("DDDDDDDDDDDDDD");
                if (CheckState(ret && ret1) == 1)
                {
                    return false;
                }
                Thread.Sleep(100);
                //移动撕膜
                errorCode = ErrorCode.TimeOut;
                //if (Math.Abs(GlobalManager.Current.TearX) > 0.001)
                //{
                //    AkrAction.Current.MoveRelNoWait(AxisName.PRX, GlobalManager.Current.TearX, GlobalManager.Current.TearXvel);
                //}
                //if (Math.Abs(GlobalManager.Current.TearY) > 0.001)
                //{
                //    AkrAction.Current.MoveRelNoWait(AxisName.PRY, GlobalManager.Current.TearY, GlobalManager.Current.TearYvel);
                //}
                //AkrAction.Current.MoveRelNoWait(AxisName.PRZ, GlobalManager.Current.TearZ, GlobalManager.Current.TearZvel);
                Logger.WriteLog("EEEEEEEEEEEEEE");
                //Z轴上升
                //TODO 
                int agmIndex = (int)AxisName.PRZ / 8;
                int axisRefNum = (int)AxisName.PRZ % 8;
                cnt = 0;
                while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4 && cnt < 100)
                {
                    Thread.Sleep(50);
                    cnt++;
                }
                if (CheckState(cnt >= 100) == 1)
                {
                    return false;
                }
                Logger.WriteLog("ASDDDDDDDDDDDDDE");
                errorCode = ErrorCode.TimeOut;
                actionret = AkrAction.Current.MoveRecheckZ(GlobalManager.Current.SafeZPos.Z);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                Logger.WriteLog("QQQQQQQQQQQQQQQQQQ");
                //移动到蓝膜收集处
                actionret = AkrAction.Current.MoveRecheckXY(GlobalManager.Current.RecheckRecylePos.X, GlobalManager.Current.RecheckRecylePos.Y);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                //必须XY到位后再移动Z轴
                actionret = AkrAction.Current.MoveRecheckZ(GlobalManager.Current.RecheckRecylePos.Z);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                Logger.WriteLog("EEEEEEEEEEEEEEEEEEEESADSAD");
                //夹爪气缸打开
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0);
                //检测到位信号
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, true);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, false);
                if (CheckState(ret && ret1) == 1)
                {
                    return false;
                }
                //蓝膜收集吸气
                SetIO(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT4_3Machine_Reset, 0);
                Thread.Sleep(500);
                SetIO(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT4_3Machine_Reset, 0);
                //夹爪气缸缩回
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1);
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, false);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, true);
                if (CheckState(ret && ret1) == 1)
                {
                    return false;
                }
                errorCode = ErrorCode.TimeOut;
                actionret = AkrAction.Current.MoveRecheckZ(GlobalManager.Current.SafeZPos.Z);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
            }
            return true;
        }


        public bool Recheck()
        {
            //复检
            int modulestatecnt = 0;
            int actionret;
            errorCode = ErrorCode.TimeOut;
            actionret = AkrAction.Current.MoveRecheckZ(GlobalManager.Current.SafeZPos.Z);
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            for (int i = 0; i < GlobalManager.Current.TotalRow; ++i)
            {
                for (int j = 0; j < GlobalManager.Current.TotalColumn; ++j)
                {
                    int k = 0;
                    if (i % 2 == 0)
                    {
                        k = j + i * GlobalManager.Current.TotalColumn;
                    }
                    else
                    {
                        k = GlobalManager.Current.TotalColumn - 1 - j + i * GlobalManager.Current.TotalColumn;
                    }
                    k = GlobalManager.Current.TotalRow * GlobalManager.Current.TotalColumn - 1 - k;
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 0);
                    //移动到穴位
                    actionret = AkrAction.Current.MoveFoamXY(GlobalManager.Current.recheckPoints[k].X,
                        GlobalManager.Current.recheckPoints[k].Y);
                    if (CheckState(actionret == 0) == 1)
                    {
                        return false;
                    }
                    //康耐视复检
                    string command = "SN" + "sqcode" + $"+{k}," + $"{k}," + "Foam+Moudel," + "0.000,0.000,0.000";
                    TriggRecheckCamreaTFCSendData(RecheckCamreaProcessCommand.TFC, command);

                    Logger.WriteLog("CCD3 开始接受COGNEX的OK信息");
                    int cnt = 0;
                    while (Task_RecheckCamreaFunction.TriggRecheckCamreaready() != "OK" && cnt < 10)
                    {
                        string res = "接收到的信息是:" + Task_RecheckCamreaFunction.TriggRecheckCamreaready();
                        Logger.WriteLog(res);
                        Thread.Sleep(300);
                        cnt++;
                    }
                    Logger.WriteLog("CCD3 接受到COGNEX的OK信息");
                    if (CheckState(cnt < 10) == 1)
                    {
                        return false;
                    }
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 1);
                    //获取康耐视数据
                    string Errcode = TriggRecheckCamreaTFCAcceptData(RecheckCamreaProcessCommand.TFC)[0].Errcode;
                    int cogres;
                    bool ret = int.TryParse(Errcode, out cogres);
                    Logger.WriteLog("CCD3 获取康耐视数据" + Errcode);
                    errorCode = ErrorCode.TimeOut;
                    if (CheckState(ret) == 1)
                    {
                        return false;
                    }
                    if (cogres != 1)
                    {
                        //康耐视报错
                        CheckState(false);
                    }
                    string Datan = TriggRecheckCamreaTFCAcceptData(RecheckCamreaProcessCommand.TFC)[0].Datan;

                    modulestatecnt = modulestatecnt + 1;
                    Logger.WriteLog($"单点结束{i}");
                }
            }

            if (modulestatecnt < GlobalManager.Current.recheckPoints.Count)
            {
                GlobalManager.Current.isNGPallete = true;
            }
            return true;
        }

        public int UploadMES()
        {

            Task_CreateMesSocket.UploadMessage();
            return 0;
        }
        /// <summary>
        /// Use this to get the list of teach points for laser measurement
        /// </summary>
        /// <param name="type">Recipe Tray Type enum</param>
        /// <param name="listOfPoints">List of SinglePoint Ext including the index, x, y, z, r</param>
        /// <returns>True: Get teach points successfully , False : Failed to get teach points</returns>
        public bool GetTeachPointList(TrayType type, out List<SinglePointExt> listOfPoints)
        {
            listOfPoints = new List<SinglePointExt>();

            //Get teach points from recipe file
            var stationsPoints = App.recipeManager.Get_RecipeStationPoints(type);
            if (stationsPoints == null)
            {
                return false;
            }

            //Read teach points named "Laser Points"
            var teachpoints = stationsPoints.FuJianPointList.FirstOrDefault(x => x.name != null && x.name.Equals("Tearing Points"));
            if (teachpoints == null)
            {
                return false;
            }


            //Extract X,Y,Z,R data
            listOfPoints = teachpoints.childList.Select((x, index) => new SinglePointExt
            {
                X = x.childPos[0],
                Y = x.childPos[1],
                Z = x.childPos[2],
                R = x.childPos[3],
                TeachPointIndex = index + 1
            }).ToList();

            return true;
        }
        public bool BoardOut()
        {
            int actionret;
            GlobalManager.Current.flag_RecheckStationHaveTray = 1;
            GlobalManager.Current.flag_TrayProcessCompletedNumber++;
            errorCode = ErrorCode.TimeOut;
            actionret = AkrAction.Current.MoveRecheckZ(GlobalManager.Current.SafeZPos.Z);
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            actionret = AkrAction.Current.MoveLaserXY(GlobalManager.Current.StartPoint.X, GlobalManager.Current.StartPoint.Y);
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            return true;
        }
        private bool _isTrayReadyToProcess = false;
        private int _currentPeelerIndex = 0;
        private int _currentVisionIndex = 0;
        public void SetTrayReadyToProcess()
        {
            _isTrayReadyToProcess = true;
        }
        public bool IsProcessOngoing()
        {
            return _isProcessOngoing;
        }
        private void ProcessingDone()
        {
            _isProcessOngoing = false;
            _isTrayReadyToProcess = false;
        }
        private void StartProcessing()
        {
            _isProcessOngoing = true;
        }
        public override bool AutoRun()
        {
            // WAIT FOR TRAY IN POSITION
            if (_movestep == 0)
            {
                if (_isTrayReadyToProcess)
                {
                    StartProcessing();
                    _movestep = 1;
                }
            }

            // GET TEACH POINTS
            if (_movestep == 1)
            {
                if (GetTeachPointList(TrayType.PAM_230_144_3X4, out _movePoints))
                {
                    _currentPeelerIndex = 0; // Reset index for move points
                    Logger.WriteLog("retest teach points loaded successfully.");
                    _movestep = 2;
                }
                else
                {
                    Logger.WriteLog("Failed to load retest teach points.");
                    return ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"GetTeachPointList(TrayType.PAM_230_144_3X4, out _movePoints)"); // Exit the process
                }
            }

            // FILM REMOVAL SEQUENCE
            if (_movestep == 2)
            {
                var filmRemovalSeqRes = FilmRemovalSequence();
                if (filmRemovalSeqRes == 1)
                {
                    _currentVisionIndex = 0;
                    _movestep = 3;
                }
                else if (filmRemovalSeqRes == -1)
                {
                    //return ErrorManager.Current.Insert(ErrorCode.motionErr, $"FilmRemovalSequence()");
                    return false;
                }
            }

            // GET VISION TEACH POINTS
            if (_movestep == 3)
            {
                if (GetTeachPointList(TrayType.PAM_230_144_3X4, out _movePoints))
                {
                    _currentVisionIndex = 0; // Reset index for move points
                    _inspectMovestep = 0;
                    Logger.WriteLog("retest teach points loaded successfully.");
                    _movestep = 4;
                }
                else
                {
                    Logger.WriteLog("Failed to load retest teach points.");
                    return ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"(GetTeachPointList({TrayType.PAM_230_144_3X4}, out {_movePoints}))");
                }
            }

            // INSPECT SEQUENCE
            if (_movestep == 4)
            {
                var inspectRes = InspectSequence();
                if (inspectRes == 1)
                {
                    _movestep = 5;
                }
                else if (inspectRes == -1)
                {
                    // TODO: ERROR HANDLING
                    return false;
                }
            }

            // SEQUENCE COMPLETE
            if (_movestep == 5)
            {
                ProcessingDone();

                // Temporary, should be conveyor checking the stations status
                Conveyor.Current.ProcessingDone(Conveyor.ConveyorStation.Recheck, true);

                _movestep = 0;
            }



            return true;
        }
        private int FilmRemovalSequence()
        {
            var zPos = 12.0; // TODO: remove temporary hardcode z position
            //DateTime startTime = DateTime.Now;
            int vacuumDelay = 1000;

            // MOVE TO POSITION
            if (_filmRemoveMovestep == 0)
            {

                if (_currentPeelerIndex >= _movePoints.Count)
                {
                    return 1;
                }

                var movePt = _movePoints[_currentPeelerIndex];
                if (AkrAction.Current.MoveRecheckXY(movePt.X, movePt.Y, false) != 0)
                {
                    // Error moving to position
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveRecheckXY({movePt.X},{movePt.Y}, false)"); // Exit the process
                    return -1;
                }
                startTime = DateTime.Now;
                _filmRemoveMovestep = 1; // Move to next step
            }

            // WAIT XY REACH POSITION  AND OPEN GRIP IF NOT OPEN YET
            if (_filmRemoveMovestep == 1)
            {
                var movePt = _movePoints[_currentPeelerIndex];
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (AkrAction.Current.IsMoveRecheckXYDone(movePt.X, movePt.Y)) // if motion stopped/reached position
                    {
                        App.filmRemoveGantryControl.ClawOpen();
                        _filmRemoveMovestep = 2;
                    }
                }
                else
                {

                    ErrorManager.Current.Insert(ErrorCode.IncomingTrayTimeOut, $"IsMoveRecheckXYDone({movePt.X}, {movePt.Y})");
                    return -1;
                }
            }

            // Z DOWN
            if (_filmRemoveMovestep == 2)
            {
                if (AkrAction.Current.MoveRecheckZ(zPos, false) != 0)
                {
                    // Error moving to position
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveRecheckZ({zPos}, false)");
                    return -1;
                }
                startTime = DateTime.Now;
                _filmRemoveMovestep = 3; // Move to next step
            }

            // WAIT Z TO REACH POSITION
            if (_filmRemoveMovestep == 3)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (AkrAction.Current.IsMoveRecheckZDone(zPos)) // if motion stopped/reached position
                    {
                        _filmRemoveMovestep = 7;
                    }

                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.IncomingTrayTimeOut, $"IsMoveRecheckZDone({zPos}");
                    return -1;
                }
            }

            // MOVE X DIRECTION
            // 5

            // WAIT
            // 6

            // GRIP CLOSE
            if (_filmRemoveMovestep == 7)
            {
                if (!App.filmRemoveGantryControl.ClawClose())
                {
                    ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"ClawClose()");
                    return -1;
                }
                _filmRemoveMovestep = 8; // Move to next step
                startTime = DateTime.Now;
            }

            // WAIT
            if (_filmRemoveMovestep == 8)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (App.filmRemoveGantryControl.IsClawClose())
                    {
                        _filmRemoveMovestep = 13; // Move to next step
                    }
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.ClawReedSwitchTimeOut, $"IsClawClose()");
                    return -1;
                }
            }

            // ZUP A BIT
            // 9

            // WAIT
            // 10

            // MOVE Y DIRECTION A BIT
            // 11

            // WAIT
            // 12

            // ZUP FULLY
            if (_filmRemoveMovestep == 13)
            {
                if (AkrAction.Current.MoveRecheckZ(0, false) != 0)
                {
                    // Error moving to position
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveRecheckZ({0}, false)");
                    return -1;
                }
                _filmRemoveMovestep = 14; // Move to next step
                startTime = DateTime.Now;
            }

            // WAIT
            if (_filmRemoveMovestep == 14)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (AkrAction.Current.IsMoveRecheckZDone(0)) // if motion stopped/reached position
                    {
                        _filmRemoveMovestep = 15;
                    }
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.ClawReedSwitchTimeOut, $"IsMoveRecheckZDone(0)");
                    return -1;
                }

            }

            // MOVE TO BIN
            if (_filmRemoveMovestep == 15)
            {
                var movePt = GlobalManager.Current.RecheckRecylePos;
                if (AkrAction.Current.MoveRecheckXY(movePt.X, movePt.Y, false) != 0)
                {
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveRecheckXY({movePt.X}, {movePt.Y}, false)");
                    return -1;
                }

                _filmRemoveMovestep = 16;
                startTime = DateTime.Now;
            }

            // WAIT THEN ON VAC
            if (_filmRemoveMovestep == 16)
            {
                var movePt = GlobalManager.Current.RecheckRecylePos;
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (AkrAction.Current.IsMoveRecheckXYDone(movePt.X, movePt.Y)) // if motion stopped/reached position
                    {

                        App.filmRemoveGantryControl.VacOn();
                        _filmRemoveMovestep = 17;
                    }

                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.motionTimeoutErr, $"IsMoveRecheckXYDone({movePt.X}, {movePt.Y})");
                    return -1;
                }
            }

            // Z DOWN
            if (_filmRemoveMovestep == 17)
            {
                if (AkrAction.Current.MoveRecheckZ(zPos, false) != 0)
                {
                    // Error moving to position
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveRecheckXY({zPos}, false)");
                    return -1;
                }
                _filmRemoveMovestep = 18; // Move to next step
                startTime = DateTime.Now;
            }



            // WAIT
            if (_filmRemoveMovestep == 18)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (AkrAction.Current.IsMoveRecheckZDone(zPos)) // if motion stopped/reached position
                    {
                        _filmRemoveMovestep = 19;
                    }
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.motionTimeoutErr, $"IsMoveRecheckZDone({zPos})");
                    return -1;
                }
            }

            // GRIP OPEN
            if (_filmRemoveMovestep == 19)
            {
                if (!App.filmRemoveGantryControl.ClawOpen())
                {
                    ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"ClawOpen()");
                    return -1;
                }
                startTime = DateTime.Now;
                _filmRemoveMovestep = 20; // Move to next step
            }

            // WAIT GRIPPER OPEN
            if (_filmRemoveMovestep == 20)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds <= 10000)
                {
                    if (App.filmRemoveGantryControl.IsClawOpen())
                    {
                        startTime = DateTime.Now; // Reset start time for delay
                        _filmRemoveMovestep = 21; // Move to next step
                    }
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.motionTimeoutErr, $"IsMoveRecheckZDone({zPos})");
                    return -1;
                }
            }

            // WAIT DELAY TO LET FILM RELEASE
            if (_filmRemoveMovestep == 21)
            {
                //if ((DateTime.Now - startTime).TotalMilliseconds >= vacuumDelay)
                //{
                App.filmRemoveGantryControl.VacOff();
                _filmRemoveMovestep = 22; // Move to next step
                                          //}
            }

            // ZUP FULLY
            if (_filmRemoveMovestep == 22)
            {
                if (AkrAction.Current.MoveRecheckZ(zPos, false) != 0)
                {
                    // Error moving to position
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveRecheckZ({zPos}, false)");
                    return -1;
                }
                _filmRemoveMovestep = 23; // Move to next step
                startTime = DateTime.Now;
            }

            // WAIT
            if (_filmRemoveMovestep == 23)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (AkrAction.Current.IsMoveRecheckZDone(zPos)) // if motion stopped/reached position
                    {
                        _filmRemoveMovestep = 24;

                    }
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.motionTimeoutErr, $"IsMoveRecheckZDone(0)");
                    return -1;
                }
            }

            // GRIP CLOSE
            if (_filmRemoveMovestep == 24)
            {
                App.filmRemoveGantryControl.ClawClose();
                startTime = DateTime.Now;
                _filmRemoveMovestep = 25; // Move to next step
            }

            // WAIT
            if (_filmRemoveMovestep == 25)
            {
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (App.filmRemoveGantryControl.IsClawClose())
                    {
                        _currentPeelerIndex++;
                        _filmRemoveMovestep = 0; // Move to next step
                    }
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"IsClawClose()");
                    return -1;
                }

            }
            return 0;
        }

        private int InspectSequence()
        {
            // MOVE TO POSITION
            if (_filmRemoveMovestep == 0)
            {

                if (_currentVisionIndex >= _movePoints.Count)
                {
                    return 1;
                }

                var movePt = _movePoints[_currentVisionIndex];
                if (!App.filmRemoveGantryControl.MoveToVisionPos(movePt.X, movePt.Y, true))
                {
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"!App.filmRemoveGantryControl.MoveToVisionPos({movePt.X}, {movePt.Y}, true)");
                    return -1;
                }
                startTime = DateTime.Now;
                _inspectMovestep = 2; // Move to next step
            }

            // WAIT TO REACH POSITION
            if (_inspectMovestep == 1)
            {
                var movePt = _movePoints[_currentVisionIndex];
                if ((DateTime.Now - startTime).TotalMilliseconds <= 5000)
                {
                    if (AkrAction.Current.IsMoveRecheckXYDone(movePt.X + App.filmRemoveGantryControl.XOffset, movePt.Y + (-App.filmRemoveGantryControl.YOffset))) // if motion stopped/reached position
                    {
                        _inspectMovestep = 2;
                    }
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.IncomingTrayTimeOut, $"IsMoveRecheckXYDone({movePt.X + App.filmRemoveGantryControl.XOffset}, {movePt.Y + (-App.filmRemoveGantryControl.YOffset)})");
                    return -1;
                }
            }

            // INSPECT
            if (_inspectMovestep == 2)
            {
                if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 0))
                {
                    ErrorManager.Current.Insert(ErrorCode.IOErr, $"!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 0)");
                    return -1;
                }
                System.Threading.Thread.Sleep(100);
                if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 1))
                {
                    ErrorManager.Current.Insert(ErrorCode.IOErr, $"!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 1)");
                    return -1;
                }
                System.Threading.Thread.Sleep(100);
                if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 0))
                {
                    ErrorManager.Current.Insert(ErrorCode.IOErr, $"!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 0)");
                    return -1;
                }
                _currentVisionIndex++;
                _inspectMovestep = 0;
            }

            // CHECK RESULT

            return 0;
        }

        public override void Paused()
        {
            return;
        }


        public override void Initialize()
        {
            return;
        }

        public override void ResetAfterPause()
        {
            startTime = DateTime.Now;
        }
    }
}
