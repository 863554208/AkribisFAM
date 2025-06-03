using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Windows;
using static AkribisFAM.GlobalManager;
using AkribisFAM.Util;
using System.Windows;
using System.Net.Sockets;
namespace AkribisFAM.WorkStation
{
    internal class LaiLiao : WorkStationBase
    {
        private static int _movestep = 0;
        private static int _laserMoveStep = 0;
        private static DateTime startTime = DateTime.Now;
        private bool _isProcessOngoing = false;
        private int _BarcodeScanRetryCount = 0;
        private int _BarcodeScanRetryMax = 5;
        private List<List<SinglePointExt>> _laserPoints = null; // List of laser points to measure
        //private List<Point> _laserMoveList = new List<Point>(); // List of points to move the laser to
        private List<LaserPointData> _laserPointData = new List<LaserPointData>();

        public enum LailiaoStep
        {
            Step1,
            Step2,
            Step3,
            Complete
        }
        private static LaiLiao _instance;
        public override string Name => nameof(LaiLiao);

        public int board_count = 0;
        int delta = 0;

        List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend> AcceptKDistanceAppend = new List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend>();
        List<KEYENCEDistance.Pushcommand.SendKDistanceAppend> sendKDistanceAppend = new List<KEYENCEDistance.Pushcommand.SendKDistanceAppend>();

        public static LaiLiao Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new LaiLiao();
                    }
                }
                return _instance;
            }
        }


        public override void Initialize()
        {
            startTime = DateTime.Now;
            return;
        }

        public static void Get(string propertyName)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.GetValue(GlobalManager.Current);
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

        public int CheckState(int state)
        {
            if (GlobalManager.Current.Lailiao_exit) return 0;
            if (state == 0)
            {
                GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
            }
            else
            {
                GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 1;
                ShowWarningMessage(state);
            }
            GlobalManager.Current.Lailiao_CheckState();
            WarningManager.Current.WaitLaiLiao();
            return 0;
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

        private int AddToLaserList(double height, int count)
        {
            try
            {
                int row = count / GlobalManager.Current.laser_point_length;
                int col = count % GlobalManager.Current.laser_point_length;
                GlobalManager.Current.laser_data[row][col] = height;
                return (int)ErrorCode.NoError;
            }
            catch (Exception ex)
            {
                return (int)ErrorCode.Laser_Failed;
            }
        }

        private int TriggerLaser(int count)
        {
            try
            {
                Thread.Sleep(GlobalManager.Current.LaserHeightDelay);

                if (!Task_KEYENCEDistance.SendMSData()) return (int)ErrorCode.Laser_Failed;
                //得到测量结果
                AcceptKDistanceAppend = Task_KEYENCEDistance.AcceptMSData();

                var res = AcceptKDistanceAppend[0].MeasurData;

                Logger.WriteLog("激光测距结果:" + res);

                double height = AkribisFAM.Util.Parser.TryParseTwoValues("=" + res);

                return AddToLaserList(height, count);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("激光测距报错 : " + ex.ToString());
                return (int)ErrorCode.Laser_Failed;
            }
        }

        private int WaitFor_X_AxesArrival()
        {
            return MoveView.WaitAxisArrived(new object[] { AxisName.LSX });
        }

        private int WaitFor_Y_AxesArrival()
        {
            return MoveView.WaitAxisArrived(new object[] { AxisName.LSY });
        }

        public int ScanBarcode()
        {

            Task_Scanner.TriggScannerSendData();
            var (barcode, error) = Task_Scanner.TriggScannerAcceptData();

            if (error == ErrorCode.BarocdeScan_Failed)
            {
                return (int)ErrorCode.BarocdeScan_Failed;
            }

            Logger.WriteLog($"Readout scanner : {barcode} ");
            GlobalManager.Current.BarcodeQueue.Enqueue(barcode ?? "NULL");

            int byPassMsg_maxTryCount = 3;
            int byPassMsg_Count = 0;
            //global switch for using mes system
            if (GlobalManager.Current.IsUseMES)
            {
                if (Task_CreateMesSocket.CreateNewSocket() == 0)
                {
                    Logger.WriteLog("Start Sending Barcode to Bali ......");
                    TcpClient firstClient = GlobalManager.Current.tcpQueue.Peek();
                    if (barcode != "NULL")
                    {
                        while (true)
                        {
                            if (byPassMsg_Count > byPassMsg_maxTryCount)
                            {
                                Logger.WriteLog("Failed To Receive byPassMsg");
                                //Stop the machine
                            }
                            string req = Task_CreateMesSocket.Compose(barcode, "station_id");
                            int res = Task_CreateMesSocket.Write(firstClient, req);
                            Thread.Sleep(500);
                            string byPassMsg = Task_CreateMesSocket.Read(firstClient);
                            if (byPassMsg != null)
                            {
                                if (byPassMsg.Contains("OK"))
                                {
                                    GlobalManager.Current.IsByPass = false;
                                    break;
                                }
                                else
                                {
                                    GlobalManager.Current.IsByPass = true;
                                    break;
                                }

                            }

                            byPassMsg_Count++;
                        }

                    }
                }
                else
                {
                    //TODO : Stop the machine or bypass
                }

            }
            else
            {
                GlobalManager.Current.IsByPass = false;
            }

            return (int)ErrorCode.NoError;
        }

        public int LaserHeight()
        {

            int count = 0;
            foreach (var point in GlobalManager.Current.laserPoints)
            {
                if (count % 4 == 0)
                {
                    //var arr1 = new object[] { AxisName.LSX, (int)point.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX };
                    //var arr2 = new object[] { AxisName.LSY, (int)point.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY };


                    int x_move = AkrAction.Current.MoveLaserXY(point.X, point.Y);

                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);
                    count++;
                }
                if (count % 4 == 1)
                {
                    int x_move = AkrAction.Current.MoveLaserXY(point.X + GlobalManager.Current.laserpoint1_shift_X,
                        (int)point.Y + GlobalManager.Current.laserpoint1_shift_Y);

                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);

                    count++;
                }
                if (count % 4 == 2)
                {
                    int x_move = AkrAction.Current.MoveLaserXY((int)point.X + GlobalManager.Current.laserpoint2_shift_X,
                        (int)point.Y + GlobalManager.Current.laserpoint2_shift_Y);

                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);

                    count++;
                }
                if (count % 4 == 3)
                {
                    int x_move = AkrAction.Current.MoveLaserXY(point.X + GlobalManager.Current.laserpoint3_shift_X, point.Y + GlobalManager.Current.laserpoint3_shift_Y);


                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);

                    count++;
                }

                Thread.Sleep(100);
            }

            return 0;
        }

        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveAllConveyor();
        }
        /// <summary>
        /// Use this to get the list of teach points for fujian tearing process and refernce for vision capture
        /// </summary>
        /// <param name="type">Recipe Tray Type enum</param>
        /// <param name="listOfPoints">List of SinglePoint Ext including the index, x, y, z, r</param>
        /// <returns>True: Get teach points successfully , False : Failed to get teach points</returns>

        public bool GetTeachPointList(TrayType type, out List<List<SinglePointExt>> listOfPoints)
        {
            listOfPoints = new List<List<SinglePointExt>>();
            List<SinglePointExt> lsp = new List<SinglePointExt>();

            //Get teach points from recipe file
            var stationsPoints = App.recipeManager.Get_RecipeStationPoints(type);
            if (stationsPoints == null)
                return false;

            //Read teach points named "Laser Points"
            var laser = stationsPoints.LaiLiaoPointList.FirstOrDefault(x => x.name != null && x.name.Equals("Laser Points"));
            if (laser == null)
            {
                return false;
            }

            //Extract X,Y,Z,R data
            lsp = laser.childList.Select((x, index) => new SinglePointExt
            {
                X = x.childPos[0],
                Y = x.childPos[1],
                Z = x.childPos[2],
                R = x.childPos[3],
                TeachPointIndex = index + 1
            }).ToList();


            var points = new List<SinglePointExt>(lsp);
            var individualTeachPoint = new List<SinglePointExt>();
            listOfPoints.Clear();

            //Compile list of points for measuring teach points.
            foreach (var pt in points)
            {
                individualTeachPoint = new List<SinglePointExt>();
                individualTeachPoint.Add(new SinglePointExt()
                {
                    X = pt.X + 0,
                    Y = pt.Y + 0,
                });
                individualTeachPoint.Add(new SinglePointExt()
                {
                    X = pt.X + GlobalManager.Current.laserpoint1_shift_X,
                    Y = pt.Y + GlobalManager.Current.laserpoint1_shift_Y,
                });
                individualTeachPoint.Add(new SinglePointExt()
                {
                    X = pt.X + GlobalManager.Current.laserpoint2_shift_X,
                    Y = pt.Y + GlobalManager.Current.laserpoint2_shift_Y,
                });
                individualTeachPoint.Add(new SinglePointExt()
                {
                    X = pt.X + GlobalManager.Current.laserpoint3_shift_X,
                    Y = pt.Y + GlobalManager.Current.laserpoint3_shift_Y,
                });
                listOfPoints.Add(individualTeachPoint);
            }
            return true;

        }
        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
        }

        private int[] signalval = new int[10];
        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
            DateTime time = DateTime.Now;
            bool ret = false;
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

        public int WaitConveyor(int type)
        {
            switch (type)
            {
                case 2:
                    return ScanBarcode();

                case 3:
                    return LaserHeight();

                default:
                    return (int)ErrorCode.ProcessErr;
            }
        }

        public void ResumeConveyor()
        {
            if (GlobalManager.Current.station2_IsBoardInLowSpeed || GlobalManager.Current.station3_IsBoardInLowSpeed || GlobalManager.Current.station4_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(10);

            }
            else if (GlobalManager.Current.station2_IsBoardInHighSpeed || GlobalManager.Current.station3_IsBoardInHighSpeed || GlobalManager.Current.station4_IsBoardInHighSpeed)
            {
                MoveConveyor((int)AxisSpeed.BL1);
            }
        }

        public bool BoradIn()
        {
            //给上游发要板信号
            SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);

            if ((ReadIO(IO_INFunction_Table.IN7_0BOARD_AVAILABLE) && board_count == 0) || (GlobalManager.Current.IO_test1 && board_count == 0))
            {
                StateManager.Current.TotalInput++;
                Set("station1_IsBoardInHighSpeed", true);


                //将要板信号清空
                SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);

                //传送带高速移动
                MoveConveyor((int)AxisSpeed.BL1);

                //等待减速光电1
                if (!WaitIO(999999, IO_INFunction_Table.IN1_0Slowdown_Sign1, false)) throw new Exception();

                //阻挡气缸1上气
                SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, 1);
                SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, 0);

                //标志位转换
                Set("station1_IsBoardInHighSpeed", false);
                Set("station1_IsBoardInLowSpeed", true);

                //传送带低速运动
                MoveConveyor(10);

                //等待料盘挡停到位信号1
                if (!WaitIO(999999, IO_INFunction_Table.IN1_4Stop_Sign1, true)) throw new Exception();

                //停止皮带移动，直到该工位顶升完成，才能继续移动皮带
                Set("station1_IsBoardInLowSpeed", false);
                Set("station1_IsLifting", true);

                StopConveyor();

                //执行测距位顶升气缸顶升                

                SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 0);
                SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 0);

                Set("station1_IsLifting", false);
                Set("station1_IsBoardIn", false);

                ResumeConveyor();

                board_count += 1;

                return true;
            }
            else
            {
                Thread.Sleep(100);
                return false;
            }
        }

        public void Boardout()
        {
            Logger.WriteLog(" 测距工站执行加一");
            GlobalManager.Current.flag_TrayProcessCompletedNumber++;

            #region 使用新的传送带控制逻辑
            //Set("station1_IsBoardOut", true);

            //while (ZuZhuang.Current.board_count != 0)
            //{
            //    Thread.Sleep(300);
            //}

            ////模拟给下一个工位发进板信号
            //if (GlobalManager.Current.IsByPass)
            //{
            //    GlobalManager.Current.SendByPassToStation2 = true;
            //}
            //GlobalManager.Current.IO_test2 = true;


            ////如果后续工站正在执行出站，就不要让该工位的气缸放气和下降
            ////while (GlobalManager.Current.station2_IsBoardOut || GlobalManager.Current.station3_IsBoardOut || GlobalManager.Current.station4_IsBoardOut)
            ////{
            ////    Thread.Sleep(100);
            ////}       



            ////执行气缸放气，下降
            //StopConveyor();
            //SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, 1);
            //SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);
            //SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 1);

            //if (!WaitIO(99999,IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos, true))
            //{
            //    throw new Exception();
            //}
            //ResumeConveyor();
            //if (!WaitIO(9999, IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1, true))
            //{
            //    throw new Exception();
            //}
            ////时间预测
            //if (!WaitIO(9999, IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1, false))
            //{
            //    throw new Exception();
            //}
            ////checkState();
            ////GlobalManager.Current.IO_test1 = true;
            //Set("station1_IsBoardOut", false);
            //board_count -= 1;

            #endregion

        }
        public void checkState()
        {
            //TODO 检查状态
            if (!WaitIO(9999, IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1, false))
            {
                throw new Exception();
            }
        }

        public void CheckState()
        {
            GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
            GlobalManager.Current.Lailiao_CheckState();
            WarningManager.Current.WaitLaiLiao();
        }

        public bool Step1()
        {
            //Debug.WriteLine("LaiLiao.Current.Step1()");

            //进板
            //if (!BoradIn())
            //    return false;
            Logger.WriteLog("测距工站等待进板");

            while (GlobalManager.Current.flag_RangeFindingTrayArrived != 1)
            {
                Thread.Sleep(300);
            }

            Logger.WriteLog("测距工站进板完成");
            GlobalManager.Current.flag_RangeFindingTrayArrived = 0;
            GlobalManager.Current.currentLasered = 0;

            Thread.Sleep(300);

            GlobalManager.Current.current_Lailiao_step = 1;
            Logger.WriteLog("测距工站进板Checkstate开始");
            CheckState();
            Logger.WriteLog("测距工站进板Checkstate完成");

            return true;
        }

        public int Step2()
        {
            Console.WriteLine("LaiLiao.Current.Step2()");

            GlobalManager.Current.current_Lailiao_step = 2;

            //扫码
            Logger.WriteLog("测距工站扫码开始");
            int ret = WaitConveyor(GlobalManager.Current.current_Lailiao_step);
            Logger.WriteLog("测距工站扫码结束");
            Logger.WriteLog("测距工站扫码Checkstate开始");
            CheckState();
            Logger.WriteLog("测距工站扫码Checkstate结束");
            return ret;
        }

        public int Step3()
        {
            Console.WriteLine("LaiLiao.Current.Step3()");

            GlobalManager.Current.current_Lailiao_step = 3;

            //激光测距
            Logger.WriteLog("测距工站测距开始");
            int ret = WaitConveyor(GlobalManager.Current.current_Lailiao_step);
            Logger.WriteLog("测距工站测距结束");
            Logger.WriteLog("测距工站测距Checkstate开始");
            CheckState();
            Logger.WriteLog("测距工站测距Checkstate结束");
            return ret;
        }

        private void ShowErrorMessage(int error)
        {
            string errorName = Enum.IsDefined(typeof(ErrorCode), error)
                                ? Enum.GetName(typeof(ErrorCode), error)
                                : "未知错误";

            // 弹出错误提示框
            System.Windows.MessageBox.Show(
                $"测距工位发生致命错误：{errorName}\n 错误代码: {error}\n 即将退出该工站的运行流程",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void ShowWarningMessage(int error)
        {
            string errorName = Enum.IsDefined(typeof(ErrorCode), error)
                                ? Enum.GetName(typeof(ErrorCode), error)
                                : "未知错误";

            // 弹出错误提示框
            System.Windows.MessageBox.Show(
                $"测距工位发生报警：{errorName}\n 报警代码: {error}\n 请检查后按Resume后恢复运行",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        private bool _isTrayReadyToProcess = false;
        private int _currentLaserPointIndex = 0;

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

            // SCAN BARCODE
            if (_movestep == 1)
            {
                if (App.scanner.ScanBarcode(out string result) == 0)
                {
                    _movestep = 2;
                }
                else
                {
                    _BarcodeScanRetryCount++;
                    Logger.WriteLog($"Barcode scan failed. Retry count {_BarcodeScanRetryCount}");

                    if (_BarcodeScanRetryCount >= _BarcodeScanRetryMax)
                    {
                        // TODO: Handle maximum retries exceeded
                        return ErrorManager.Current.Insert(ErrorCode.BarocdeScan_Failed, "ScanBarcode");
                    }
                }
            }

            // SEND REQUEST BARCODE DATA
            if (_movestep == 2)
            {
                if (true) // TODO: ADD SERVER REQUEST HERE
                {
                    _movestep = 3;
                }
                else
                {

                }
            }



            //// GET THE TEACH POINTS
            //if (_movestep == 0)
            //{
            //    if (!GetTeachPointList(TrayType.PAM_230_144_3X4, out _laserPoints))
            //    {
            //        ShowErrorMessage((int)ErrorCode.Laser_Failed);
            //        return -1; // Failed to get teach points
            //    }
            //    _laserMoveStep = 1;
            //}

            // GET LASER TEACH POINTS
            if (_movestep == 3)
            {
                if (GetTeachPointList(TrayType.PAM_230_144_3X4, out _laserPoints))
                {
                    _laserPointData.Clear(); // Clear previous laser point data
                    foreach (var pointList in _laserPoints)
                    {
                        foreach (var point in pointList)
                        {
                            _laserPointData.Add(new LaserPointData
                            {
                                Point = point
                            });
                        }
                    }
                    _currentLaserPointIndex = 0; // Reset index for laser points
                    Logger.WriteLog("Laser teach points loaded successfully.");
                    _movestep = 4;
                }
                else
                {
                    Logger.WriteLog("Failed to load laser teach points.");
                    return ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"GetTeachPointList(TrayType.PAM_230_144_3X4, out _laserPoints)");
                }
            }

            // RUN LASER MEASUREMENT SEQUENCE
            if (_movestep == 4)
            {
                var laserSeqResult = LaserMeasureSequence(); // Returns 2 on sequence complete, -1 on error
                if (laserSeqResult == 1)
                {
                    _movestep = 5;
                }
                else if (laserSeqResult == -1)
                {
                    // Error occurred during laser measurement
                    return false;
                }
            }


            // PROCESSING DONE
            if (_movestep == 5)
            {
                ProcessingDone();

                // Temporary, should be conveyor checking the stations status
                Conveyor.Current.ProcessingDone(Conveyor.ConveyorStation.Laser, true);

                _movestep = 0; // Reset for next tray
            }
            return true;
        }

        /// <summary>
        /// Laser measurement sequence logic.
        /// Returns 0 on no error, Returns -1 on error. Returns 1 when the sequence is complete.
        /// </summary>
        /// <returns></returns>
        private int LaserMeasureSequence()
        {
            // MOVE TO POSITION
            if (_laserMoveStep == 0)
            {
                if (_currentLaserPointIndex >= _laserPointData.Count)
                {
                    return 1;
                }
                var movePt = _laserPointData[_currentLaserPointIndex].Point;
                // Move to the laser point position
                if (AkrAction.Current.MoveLaserXY(movePt.X, movePt.Y, false) != 0)
                {
                    // Error moving to position
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"AkrAction.Current.MoveLaserXY({movePt.X}, {movePt.Y}, false)");
                    return -1;
                }
                _laserMoveStep = 1; // Move to next step
            }

            // WAIT FOR POSITION ARRIVAL
            if (_laserMoveStep == 1)
            {
                var movePt = _laserPointData[_currentLaserPointIndex].Point;
                if (AkrAction.Current.IsMoveLaserXYDone(movePt.X, movePt.Y)) // if motion stopped/reached position
                {
                    _currentLaserPointIndex++;
                    _laserMoveStep = 0;
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.motionTimeoutErr, $"IsMoveLaserXYDone({movePt.X},{movePt.Y})");
                    return -1;
                }
            }

            // DO LASER MEASURE
            if (_laserMoveStep == 2)
            {
                if (!App.laser.Measure(out double res))
                {
                    ErrorManager.Current.Insert(ErrorCode.LaserErr, $"App.laser.Measure(out double res)");
                    return -1;
                }
                else
                {
                    _laserMoveStep = 0;
                }
            }
            return 0;
        }

        public override void Paused()
        {
            return;
        }

        public override void ResetAfterPause()
        {
            startTime = DateTime.Now;
        }

        private class LaserPointData
        {
            public SinglePointExt Point { get; set; } // The point data
            public int TeachPointIndex { get; set; }
            public double? Measurement { get; set; } = null; // Measurement result from laser

        }
    }
}
