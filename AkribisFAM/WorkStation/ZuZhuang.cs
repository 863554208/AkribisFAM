
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using static AkribisFAM.GlobalManager;
using AkribisFAM.Util;
using System.Windows;
using System.Linq;
using AkribisFAM.DeviceClass;
using static AkribisFAM.Manager.MaterialManager;
using static AkribisFAM.DeviceClass.CognexVisionControl;
using System.Drawing;

namespace AkribisFAM.WorkStation
{
    internal class ZuZhuang : WorkStationBase
    {
        CognexVisionControl.FeederNum _activeFeederNum = CognexVisionControl.FeederNum.Feeder1;
        public class SinglePointRequest
        {
            public SinglePoint point { get; set; } = new SinglePoint();
            public bool IsRequested { get; set; }

            public void Reset()
            {
                point = new SinglePoint();
                IsRequested = false;
            }
        }
        public SinglePointRequest[] PickPositions = new SinglePointRequest[4];
        public SinglePointRequest[] PlacePositions = new SinglePointRequest[4];
        private static DateTime startTime = DateTime.Now;
        private static ZuZhuang _instance;

        private static int _pickCaptureMovestep = 0;
        private static int _pickPartMovestep = 0;
        private static int _placeInspectMovestep = 0;
        private static int _placePartMovestep = 0;
        private static int _trayPlaceMovestep = 0;
        private static int _trayCaptureMovestep = 0;
        private static int _currentTrayPlaceIndex = 0;
        private static int _currentPickerIndex = 0;
        private static int _currentPickerDone = 0;
        private static int _currentOnTheFlyRowIndex = 0;
        private static int _currentFeederIndex = 0;
        private bool _isProcessOngoing = false;
        private bool _trayInspectDone = false;
        private int _remainder = 2;
        private int _rowCount = 3;
        private int _colCount = 4;
        private int _vision1FeederRetryCount = 0;
        private int _vision1FeederMaxRetry = 2;
        private int _vision1TrayRetryCount = 0;
        private int _vision1TrayMaxRetry = 2;
        private int _vision2FeederRetryCount = 0;
        private int _vision2FeederMaxRetry = 2;
        private Recipe _currentRecipe = App.recipeManager.GetRecipe(TrayType.PAM_230_144_3X4);

        private static int _selectedPicker;
        private static int _selectedTrayIndex;
        private static int _selectedFeederIndex;
        private static bool[] _feederHasPartIndex = new bool[4];
        private List<Picker> _pickers = new List<Picker>()
        {
            new Picker() { PickerIndex = 1, IsEnabled = true },
            new Picker() { PickerIndex = 2, IsEnabled = true },
            new Picker() { PickerIndex = 3, IsEnabled = false },
            new Picker() { PickerIndex = 4, IsEnabled = false },
        };
        private List<FeederIndex> _feederIndex = new List<FeederIndex>()
        {
            new FeederIndex() { PartIndex = 1, hasPart = false },
            new FeederIndex() { PartIndex = 2, hasPart = false },
            new FeederIndex() { PartIndex = 3, hasPart = false },
            new FeederIndex() { PartIndex = 4, hasPart = false },
        };
        private List<SinglePoint> _trayOTFProductPosition = new List<SinglePoint>();
        private List<VisionTravelPath> _trayOTFTravelPaths = new List<VisionTravelPath>();
        private List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> _TLTcommands = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition>();
        private int _TrayOTFRowDone = 0;
        public bool SetPickerEnable(int pickerIndex, bool enable)
        {
            if (pickerIndex < 1 || pickerIndex > 4)
            {
                return false;
            }
            var picker = _pickers.First(x => x.PickerIndex == pickerIndex);
            if (picker == null)
            {
                return false;
            }
            picker.IsEnabled = enable;
            return true;
        }
        // get first index picker TODO 
        public int GetNextPickerEnableIndex()
        {
            var picker = _pickers.FirstOrDefault(x => x.IsEnabled);

            if (picker == null)
                return 0;
            else
                return picker.PickerIndex;
        }
        public override string Name => nameof(ZuZhuang);

        int delta = 0;
        public int board_count = 0;

        List<SinglePoint> snapPalleteList = new List<SinglePoint>();
        List<SinglePoint> RealPalletePointsList = new List<SinglePoint>();
        List<SinglePoint> feedar1pointList = new List<SinglePoint>();
        List<SinglePoint> feedar2pointList = new List<SinglePoint>();
        List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> snapFeederPath = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();
        List<FeedUpCamrea.Pushcommand.SendGMCommandAppend> snapFeederGMPath = new List<FeedUpCamrea.Pushcommand.SendGMCommandAppend>();
        List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> ccd2SnapPath = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> palletePath = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendGTCommandAppend> fetchMatrial = new List<AssUpCamrea.Pushcommand.SendGTCommandAppend>();
        public static ZuZhuang Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new ZuZhuang();
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

        public int CheckState(int state)
        {
            if (GlobalManager.Current.Zuzhuang_exit) return 0;
            if (state == 0)
            {
                GlobalManager.Current.Zuzhuang_state[GlobalManager.Current.current_Zuzhuang_step] = 0;
            }
            else
            {
                ShowErrorMessage(state);
                GlobalManager.Current.Zuzhuang_state[GlobalManager.Current.current_Zuzhuang_step] = 1;
            }
            GlobalManager.Current.ZuZhuang_CheckState();
            WarningManager.Current.WaitZuZhuang();
            return 0;
        }

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }

        public int WaitIO(int delta, IO_INFunction_Table index, bool value, ErrorCode errorCode)
        {
            DateTime time = DateTime.Now;
            int ret = (int)ErrorCode.NoError;
            while ((DateTime.Now - time).TotalMilliseconds < delta)
            {
                if (ReadIO(index) == value)
                {
                    ret = (int)ErrorCode.NoError;
                    break;
                }
                Thread.Sleep(10);
            }

            return (int)errorCode;
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
                $"测距工位发生报警：{errorName}\n 报警代码: {error}\n 请处理后按RESUME恢复运行",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public void ResumeConveyor()
        {
            //20250520 
            if (GlobalManager.Current.station1_IsBoardInLowSpeed || GlobalManager.Current.station3_IsBoardInLowSpeed || GlobalManager.Current.station4_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(10);
            }
            else if (GlobalManager.Current.station1_IsBoardInHighSpeed || GlobalManager.Current.station3_IsBoardInHighSpeed || GlobalManager.Current.station4_IsBoardInHighSpeed)
            {
                MoveConveyor((int)AxisSpeed.BL1);
            }
        }
        //public bool BoradIn()
        //{
        //    if (true)
        //    {
        //        //将要板信号清空
        //        Set("IO_test2", false);
        //        Set("station2_IsBoardInHighSpeed", true);

        //        //传送带高速移动
        //        MoveConveyor((int)AxisSpeed.BL1);

        //        //等待减速光电2 , false为感应到
        //        if (!WaitIO(999999, IO_INFunction_Table.IN1_1Slowdown_Sign2, false)) throw new Exception();

        //        //阻挡气缸2上气
        //        SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 1);
        //        SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 0);

        //        //标志位转换
        //        Set("station2_IsBoardInHighSpeed", false);
        //        Set("station2_IsBoardInLowSpeed", true);

        //        //传送带减速
        //        MoveConveyor(10);

        //        //等待料盘挡停到位信号1
        //        if (!WaitIO(999999, IO_INFunction_Table.IN1_5Stop_Sign2, true)) throw new Exception();

        //        //停止皮带移动，直到该工位顶升完成，才能继续移动皮带
        //        Set("station2_IsBoardInLowSpeed", false);
        //        Set("station2_IsLifting", true);

        //        StopConveyor();

        //        //执行测距位顶升气缸顶升                

        //        SetIO(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 1);
        //        SetIO(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 0);
        //        SetIO(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 1);
        //        SetIO(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 0);

        //        if (!WaitIO(999999, IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos, true)) throw new Exception();

        //        Set("station1_IsLifting", false);
        //        Set("station2_IsBoardIn", false);
        //        ResumeConveyor();

        //        board_count += 1;
        //        return true;
        //    }
        //    else
        //    {
        //        Thread.Sleep(100);
        //        return false;
        //    }
        //}
        public void BoardOut()
        {
            Logger.WriteLog("组装工站执行完成");
            //AkrAction.Current.MoveNoWait(AxisName.FSX, (double)3.0, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            //AkrAction.Current.Move(AxisName.FSY, (double)3.0, (int)AxisSpeed.FSY, (int)AxisAcc.FSX);
            GlobalManager.Current.flag_TrayProcessCompletedNumber++;
            GlobalManager.Current.palleteSnaped = false;
            GlobalManager.Current.current_Assembled = 0;
            #region 使用新的传送带控制逻辑
            //Set("station2_IsBoardOut", true);

            //while (FuJian.Current.board_count != 0)
            //{
            //    Thread.Sleep(300);
            //}

            ////模拟给下一个工位发进板信号
            //if (GlobalManager.Current.SendByPassToStation2)
            //{
            //    GlobalManager.Current.SendByPassToStation3 = true;
            //}
            //GlobalManager.Current.IO_test3 = true;

            ////如果后续工站正在执行出站，就不要让该工位的气缸放气和下降
            ////while (GlobalManager.Current.station3_IsBoardOut || GlobalManager.Current.station4_IsBoardOut)
            ////{
            ////    Thread.Sleep(100);
            ////}

            ////如果有后续工站在工作，不能下降


            //StopConveyor();
            //SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 1);

            //Thread.Sleep(100);
            //SetIO(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 1);
            //SetIO(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 1);

            //if (!WaitIO(99999, IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos, true))
            //{
            //    throw new Exception();
            //}
            //ResumeConveyor();
            //if (!WaitIO(9999, IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2, true))
            //{
            //    throw new Exception();
            //}
            //if (!WaitIO(9999, IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2, false))
            //{
            //    throw new Exception();
            //}

            //出板时将穴位信息清空


            //Set("station2_IsBoardOut", false);
            //board_count--;


            #endregion
        }

        public int WaitConveyor(int type)
        {
            switch (type)
            {
                case 2:
                    return SnapFeedar();

                case 3:
                    return PickFoam();

                case 4:
                    return LowerCCD();

                case 5:
                    return DropBadFoam();

                case 6:
                    return SnapPallete();

                case 7:
                    return PlaceFoam();

                default:
                    return (int)ErrorCode.ProcessErr;
            }
        }

        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveAllConveyor();
        }

        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
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

        private int WaitFor_X_AxesArrival()
        {
            return MoveView.WaitAxisArrived(new object[] { AxisName.FSX });
        }

        private int WaitFor_Y_AxesArrival()
        {
            return MoveView.WaitAxisArrived(new object[] { AxisName.FSY });
        }

        public int SnapFeedar()
        {
            //feedar信号
            //while (!ReadIO(IO_INFunction_Table.IN4_1Platform_has_label_feeder1) && !ReadIO(IO_INFunction_Table.IN4_5Platform_has_label_feeder2))
            //{
            //    Thread.Sleep(100);
            //}
            //优先选择feedar1 ,再选择feedar2

            //给Cognex发信息
            if (GlobalManager.Current.UseFeedar1)
            {
                snapFeederPath.Clear();
                feedar1pointList.Clear();
                int index = 0;
                int count = 0;
                double start_pos_X = GlobalManager.Current.feedar1Points[0].X;
                double start_pos_Y = GlobalManager.Current.feedar1Points[0].Y;
                double end_pos_X = GlobalManager.Current.feedar1Points[1].X;
                double end_pos_Y = GlobalManager.Current.feedar1Points[1].Y;
                for (int i = 0; i < 4; i++)
                {
                    feedar1pointList.Add(new SinglePoint()
                    {
                        X = start_pos_X + 16 * i,
                        Y = start_pos_Y,
                        Z = 0,
                        R = 0,
                    });
                }
                foreach (var Point in feedar1pointList)
                {
                    FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                    {
                        SN1 = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        RawMaterialName1 = "Foam",
                        FOV = (count + 1).ToString(),
                        Photo_X1 = Point.X.ToString(),
                        Photo_Y1 = Point.Y.ToString(),
                        Photo_R1 = "0"
                    };
                    count++;
                    snapFeederPath.Add(sendTLMCamreaposition1);
                }

                //移动到拍照起始点

                //int moveToStart = (AkrAction.Current.Move( GlobalManager.Current.feedar1Points[0].X - 16, GlobalManager.Current.feedar1Points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY, (int)AxisAcc.FSX)==0) ? 0 : (int)ErrorCode.AGM800Err;

                //if (moveToStart > 0x1000) return -1;
                //CheckState(moveToStart);

                if (!Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath))

                    Logger.WriteLog("发送好数据");
                int retryCount = 0;
                while (Task_FeedupCameraFunction.TriggFeedUpCamreaready() != "OK")
                {
                    Thread.Sleep(300);
                    retryCount++;

                    if (retryCount > 10) return (int)ErrorCode.Cognex_DisConnected;
                }

                AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.feedar1Points[0].X, 16, GlobalManager.Current.feedar1Points[0].X + 16 * 3, 1);
                Thread.Sleep(300);

                AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);

                Logger.WriteLog("开始CCD2运动");

                //移动到拍照结束点
                int moveToEnd = AkrAction.Current.MoveFoamXY(GlobalManager.Current.feedar1Points[0].X + 16 * 4, GlobalManager.Current.feedar1Points[0].Y);

                if (moveToEnd > 0x1000) return -1;
                CheckState(moveToEnd);

                Logger.WriteLog("结束点的X为" + GlobalManager.Current.feedar1Points[1].X);
                Logger.WriteLog("结束CCD2运动");

                Thread.Sleep(300);
                AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response4);
                Thread.Sleep(300);

                ////接受Cognex的信息
                List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> msg_received = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
                msg_received = Task_FeedupCameraFunction.TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand.TLM);

                Logger.WriteLog("feedar飞拍接收到的消息为:" + msg_received[0].Errcode1);

            }
            else if (GlobalManager.Current.UseFeedar2)
            {
                snapFeederPath.Clear();
                feedar2pointList.Clear();
                int index = 0;
                int count = 0;
                double start_pos_X = GlobalManager.Current.feedar2Points[0].X;
                double start_pos_Y = GlobalManager.Current.feedar2Points[0].Y;
                double end_pos_X = GlobalManager.Current.feedar2Points[1].X;
                double end_pos_Y = GlobalManager.Current.feedar2Points[1].Y;
                for (int i = 0; i < 4; i++)
                {
                    feedar2pointList.Add(new SinglePoint()
                    {
                        X = start_pos_X + 16 * i,
                        Y = start_pos_Y,
                        Z = 0,
                        R = 0,
                    });
                }
                foreach (var Point in feedar2pointList)
                {
                    FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                    {
                        SN1 = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        RawMaterialName1 = "Foam",
                        FOV = (count + 1).ToString(),
                        Photo_X1 = Point.X.ToString(),
                        Photo_Y1 = Point.Y.ToString(),
                        Photo_R1 = "0"
                    };
                    snapFeederPath.Add(sendTLMCamreaposition1);
                }
                //给Cognex发拍照信息
                Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath);

                //移动到拍照起始点
                int moveToStart = (AkrAction.Current.MoveLaserXY(GlobalManager.Current.feedar2Points[0].X - 16, GlobalManager.Current.feedar2Points[0].Y));


                if (moveToStart > 0x1000) return -1;
                CheckState(moveToStart);

                Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath);

                Logger.WriteLog("发送好数据");
                int retryCount = 0;
                while (Task_FeedupCameraFunction.TriggFeedUpCamreaready() != "OK")
                {
                    Thread.Sleep(300);
                    retryCount++;

                    if (retryCount > 10) return (int)ErrorCode.Cognex_DisConnected;
                }

                AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.feedar2Points[0].X, 16, GlobalManager.Current.feedar2Points[1].X + 16 * 3, 1);
                Thread.Sleep(300);

                AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);
                //移动到拍照结束点
                int moveToEnd = AkrAction.Current.MoveFoamXY(GlobalManager.Current.feedar2Points[0].X + 16 * 4, GlobalManager.Current.feedar2Points[0].Y);

                if (moveToEnd > 0x1000) return -1;
                CheckState(moveToEnd);

                Thread.Sleep(300);
                AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response4);
                Thread.Sleep(300);

                ////接受Cognex的信息
                List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> msg_received = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
                msg_received = Task_FeedupCameraFunction.TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand.TLM);

                Logger.WriteLog("feedar飞拍接收到的消息为:" + msg_received[0].Errcode1);
            }

            //int Zup = (AkrAction.Current.MoveFoamZ1(AxisName.PICK1_Z, 0, (int)AxisSpeed.PICK1_Z, (int)AxisAcc.PICK1_Z, (int)AxisAcc.PICK1_Z)==0 && 
            //          AkrAction.Current.MoveFoamZ1(AxisName.PICK2_Z, 0, (int)AxisSpeed.PICK2_Z, (int)AxisAcc.PICK2_Z, (int)AxisAcc.PICK2_Z)==0 &&
            //          AkrAction.Current.MoveFoamZ1(AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z, (int)AxisAcc.PICK3_Z, (int)AxisAcc.PICK3_Z) == 0)? 0 :(int)ErrorCode.AGM800Err;
            //var moveZ1 = new object[] { AxisName.PICK1_Z, (int)AxisAcc.PICK1_Z, (int)AxisAcc.PICK1_Z };
            //var moveZ2 = new object[] { AxisName.PICK2_Z, 0, (int)AxisAcc.PICK2_Z, (int)AxisAcc.PICK2_Z };
            //var moveZ3 = new object[] { AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z, (int)AxisAcc.PICK3_Z, (int)AxisAcc.PICK3_Z };
            //var moveZ4 = new object[] { AxisName.PICK4_Z, 0, (int)AxisSpeed.PICK4_Z, (int)AxisAcc.PICK4_Z, (int)AxisAcc.PICK4_Z };

            //if (Zup > 0x1000) return -1;
            //CheckState(Zup);

            //根据congex返回的结果判断坐标，以及是否有bad foam
            GlobalManager.Current.BadFoamCount = 0;

            return 0;
        }

        public int PickFoam()
        {

            //GlobalManager.Current.UsePicker1 = true;
            //GlobalManager.Current.UsePicker2 = true;
            //GlobalManager.Current.UsePicker3 = false;
            //GlobalManager.Current.UsePicker4 = false;


            ////移动到取料位
            ////int Zup = (AkrAction.Current.Move(AxisName.PICK1_Z, 0, (int)AxisSpeed.PICK1_Z, (int)AxisAcc.PICK1_Z, (int)AxisAcc.PICK1_Z) == 0 &&
            ////            AkrAction.Current.Move(AxisName.PICK2_Z, 0, (int)AxisSpeed.PICK2_Z, (int)AxisAcc.PICK2_Z, (int)AxisAcc.PICK2_Z) == 0 &&
            ////            AkrAction.Current.Move(AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z, (int)AxisAcc.PICK3_Z, (int)AxisAcc.PICK3_Z) == 0) ? 0 : (int)ErrorCode.AGM800Err;

            ////if (Zup > 0x1000) return -1;
            ////CheckState(Zup);

            //if (GlobalManager.Current.UsePicker1)
            //{

            //    if (!GetPickPosition(1, 1, out SinglePoint point))
            //    {
            //        return -1;
            //    }

            //    //int moveToStart = (AkrAction.Current.Move(AxisName.FSX, res1.X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX, (int)AxisAcc.FSX) == 0 &&
            //    //                   AkrAction.Current.Move(AxisName.FSY, res1.Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY, (int)AxisAcc.FSX) == 0) ? 0 : (int)ErrorCode.AGM800Err;

            //    //if (moveToStart > 0x1000) return -1;
            //    //CheckState(moveToStart);

            //    SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
            //    //int nozzle1_move1 = WaitIO(300, IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback, false, ErrorCode.Nozzle1_feedback);
            //    //if(nozzle1_move1 != 0) return (int)ErrorCode.Nozzle1_feedback;
            //    //CheckState(nozzle1_move1);



            //    SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);

            //    //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
            //    //    Thread.Sleep(20);
            //    //    AkrAction.Current.Move(AxisName.PICK1_T, 0, (int)AxisSpeed.PICK1_T);
            //    //    AkrAction.Current.MoveFoamZ1(21.5);

            //    //    //int ret = WaitIO(3000, IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback, true, ErrorCode.OUT3_1_PNP_Gantry_vacuum1_Release_Error);
            //    //    //if ((int)ret > 0x1000) return ret;
            //    //    //CheckState(ret);

            //    //int nozzle1_move2 = WaitIO(300, IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback, true, ErrorCode.Nozzle1_feedback);
            //    //if (nozzle1_move2 != 0) return (int)ErrorCode.Nozzle1_feedback;
            //    //CheckState(nozzle1_move2);

            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);

            //    //    AkrAction.Current.MoveFoamZ1(11.5);
            //    //    //SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
            //    //int nozzle1_move2 = WaitIO(300, IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback, true, ErrorCode.Nozzle1_feedback);
            //    //if (nozzle1_move2 != 0) return (int)ErrorCode.Nozzle1_feedback;
            //    //CheckState(nozzle1_move2);

            //    Thread.Sleep(20);

            //    int rotate1 = AkrAction.Current.MoveFoamZ1(res1.R);
            //    if (rotate1 != 0) return -1;
            //    CheckState(rotate1);

            //    int zdown = AkrAction.Current.MoveFoamZ1(19.5);
            //    if (zdown != 0) return -1;
            //    CheckState(zdown);

            //    //    //AkrAction.Current.Move(AxisName.PICK2_T, 0, (int)AxisSpeed.PICK2_T);
            //    //    AkrAction.Current.MoveFoamZ2(0);
            //    //    AkrAction.Current.MoveFoamZ2(20.5);
            //    //int ret = WaitIO(3000, IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback, true, ErrorCode.OUT3_1_PNP_Gantry_vacuum1_Release_Error);
            //    //if ((int)ret > 0x1000) return ret;
            //    //CheckState(ret);

            //    Thread.Sleep(50);

            //    //    AkrAction.Current.MoveFoamZ2(11.5);

            //    int zup = AkrAction.Current.MoveFoamZ1(11.5);
            //    if (zup != 0) return -1;
            //    CheckState(zup);
            //    //SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);

            //    GlobalManager.Current.current_FOAM_Count++;
            //}

            //if (GlobalManager.Current.UsePicker2)
            //{
            //    if (!ZuZhuang.Current.GetPickPosition(2, 2, out SinglePoint point))
            //    {
            //        return -1;
            //    }

            //    int moveToStart = (AkrAction.Current.MoveFoamXY(point.X, point.Y));

            //    if (moveToStart > 0x1000) return -1;
            //    CheckState(moveToStart);

            //    SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 1);
            //    Thread.Sleep(20);
            //    //SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);

            //    int rotate1 = AkrAction.Current.MoveFoamT2(point.R);
            //    if (rotate1 != 0) return -1;
            //    CheckState(rotate1);

            //    int zdown = AkrAction.Current.MoveFoamZ2(20.5);
            //    if (zdown != 0) return -1;
            //    CheckState(zdown);

            //    //int ret = WaitIO(3000, IO_INFunction_Table.IN3_13PNP_Gantry_vacuum2_Pressure_feedback, true, ErrorCode.OUT3_2_PNP_Gantry_vacuum2_Release_Error);
            //    //if ((int)ret > 0x1000) return ret;
            //    //CheckState(ret);

            //    //int zup = AkrAction.Current.MoveFoamZ2(AxisName.PICK2_Z, 11.5, (int)AxisSpeed.PICK2_Z);
            //    //if (zup != 0) return -1;
            //    //CheckState(zup);

            //    ////让飞达送料
            //    //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_9Run_feeder1, 1);
            //    //Logger.WriteLog("将Z轴移上去结束");
            //    ////AkrAction.Current.MoveNoWait(AxisName.PICK2_Z, 0);
            //    ////AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z);
            //    ////AkrAction.Current.MoveFoamZ4(0, (int)AxisSpeed.PICK4_Z);
            //    GlobalManager.Current.current_FOAM_Count++;
            //}

            //if (GlobalManager.Current.UsePicker3)
            //{

            //    AkrAction.Current.MoveFoamZ3(10);
            //    SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 1);
            //    Thread.Sleep(20);
            //    //SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);

            //    GlobalManager.Current.current_FOAM_Count++;
            //}

            //if (GlobalManager.Current.UsePicker4)
            //{

            //    AkrAction.Current.MoveFoamZ4(10);
            //    //SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
            //    //Thread.Sleep(20);
            //    //SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
            //    //Thread.Sleep(20);
            //    //SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 1);
            //    //Thread.Sleep(20);
            //    //SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);

            //    //GlobalManager.Current.current_FOAM_Count++;
            //}


            //Logger.WriteLog("取料结束");
            //Thread.Sleep(500);


            ////让飞达送料
            //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_9Run_feeder1, 1);
            //Logger.WriteLog("将Z轴移上去结束");
            ////AkrAction.Current.MoveNoWait(AxisName.PICK2_Z, 0, (int)AxisSpeed.PICK2_Z);
            ////AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z);
            ////AkrAction.Current.Move(AxisName.PICK4_Z, 0, (int)AxisSpeed.PICK4_Z);

            return 0;
        }

        public int LowerCCD()
        {

            //ccd2SnapPath.Clear();
            //int count = 0;
            //var start_x = GlobalManager.Current.lowerCCDPoints[0].X;
            //var start_y = GlobalManager.Current.lowerCCDPoints[0].Y;
            //for (int i = 0; i < 4; i++)
            //{

            //    PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition SendTLNCamreaposition = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition()
            //    {
            //        SN = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //        NozzleID = (count + 1).ToString(),
            //        RawMaterialName = "Foam",
            //        CaveID = "0",
            //        TargetMaterialName1 = "Foam->Module",
            //        Photo_X1 = (start_x - i * 16).ToString(),
            //        Photo_Y1 = (start_y).ToString(),
            //        Photo_R1 = "90.0",

            //    };
            //    ccd2SnapPath.Add(SendTLNCamreaposition);
            //    count++;
            //}


            //AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);
            //Thread.Sleep(300);

            ////如果是IsLowerCCD,状态，XY移动时不能判断Z轴高度
            //GlobalManager.Current.isLowerCCD = true;

            ////移动到拍照起始点


            //AkrAction.Current.MoveFoamT2(90);
            //AkrAction.Current.MoveFoamT1(90);


            //Logger.WriteLog("CCD2准备移动到拍照位置");

            //int moveToStart = (AkrAction.Current.MoveFoamXY(GlobalManager.Current.lowerCCDPoints[0].X + 16, GlobalManager.Current.lowerCCDPoints[0].Y));
            //if (moveToStart > 0x1000) return -1;
            //CheckState(moveToStart);

            //AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.lowerCCDPoints[0].X, -16, GlobalManager.Current.lowerCCDPoints[0].X - 16 * 3, 2);
            //Thread.Sleep(200);

            //Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNSendData(PrecisionDownCamreaProcessCommand.TLN, ccd2SnapPath);
            //Thread.Sleep(100);

            ////给Cognex发拍照信息
            //Logger.WriteLog("CCD2 开始接受COGNEX的OK信息");

            //while (Task_PrecisionDownCamreaFunction.TriggDownCamreaready() != "OK")
            //{
            //    string res = "接收到的信息是:" + Task_PrecisionDownCamreaFunction.TriggDownCamreaready();
            //    Logger.WriteLog(res);
            //    Thread.Sleep(300);
            //}

            //Logger.WriteLog("CCD2 接受完成COGNEX的OK信息");
            //Thread.Sleep(30);

            //AkrAction.Current.EventEnable(AxisName.FSX);
            //AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response6);
            //Thread.Sleep(200);

            ////移动到拍照结束点
            //int moveToEnd = AkrAction.Current.MoveFoamXY((GlobalManager.Current.lowerCCDPoints[0].X - 16 * 4), GlobalManager.Current.lowerCCDPoints[0].Y);
            //if (moveToEnd != 0) return -1;

            //Thread.Sleep(200);

            //AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response3);

            ////接受Cognex信息
            ////List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> AcceptTLNDownPosition = new List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition>();
            ////AcceptTLNDownPosition = Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNAcceptData(PrecisionDownCamreaProcessCommand.TLN);

            ////如果是IsLowerCCD,状态，XY移动时不能判断Z轴高度
            //GlobalManager.Current.isLowerCCD = false;

            //int zUpAll = (AkrAction.Current.MoveFoamZ1(0) == 0 &&
            //                AkrAction.Current.MoveFoamZ2(0) == 0) ? 0 : (int)ErrorCode.AGM800Err;
            ////AkrAction.Current.MoveNoWait(AxisName.PICK1_Z);
            //////AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z);
            //////AkrAction.Current.MoveNoWait(AxisName.PICK4_Z, 0, (int)AxisSpeed.PICK4_Z);
            ////AkrAction.Current.MoveFoamZ2(0);

            //if (zUpAll != 0) return zUpAll;
            //CheckState(zUpAll);
            ////AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z);
            ////AkrAction.Current.MoveNoWait(AxisName.PICK4_Z, 0, (int)AxisSpeed.PICK4_Z);

            return 0;
        }

        public int DropBadFoam()
        {
            //if (GlobalManager.Current.picker1State == false)
            //{
            //    AkrAction.Current.Move(AxisName.FSX, 290, (int)AxisSpeed.FSX);
            //    AkrAction.Current.Move(AxisName.FSY, 300, (int)AxisSpeed.FSY);

            //    SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
            //    SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 1);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_8solenoid_valve1_A, 1);
            //    SetIO(IO_OutFunction_Table.OUT3_9solenoid_valve1_B, 0);
            //    Thread.Sleep(20);
            //    GlobalManager.Current.current_FOAM_Count--;
            //    GlobalManager.Current.BadFoamCount--;
            //    GlobalManager.Current.TotalBadFoam++;
            //}
            //if (GlobalManager.Current.picker2State == false)
            //{
            //    AkrAction.Current.Move(AxisName.FSX, 280, (int)AxisSpeed.FSX);
            //    AkrAction.Current.Move(AxisName.FSY, 390, (int)AxisSpeed.FSY);

            //    SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
            //    SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 1);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_10solenoid_valve2_A, 1);
            //    SetIO(IO_OutFunction_Table.OUT3_11solenoid_valve2_B, 0);
            //    Thread.Sleep(20);
            //    GlobalManager.Current.current_FOAM_Count--;
            //    GlobalManager.Current.BadFoamCount--;
            //    GlobalManager.Current.TotalBadFoam++;
            //}
            //if (GlobalManager.Current.picker3State == false)
            //{
            //    AkrAction.Current.Move(AxisName.FSX, 270, (int)AxisSpeed.FSX);
            //    AkrAction.Current.Move(AxisName.FSY, 390, (int)AxisSpeed.FSY);

            //    SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
            //    SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 1);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_12solenoid_valve3_A, 1);
            //    SetIO(IO_OutFunction_Table.OUT3_13solenoid_valve3_B, 0);
            //    Thread.Sleep(20);
            //    GlobalManager.Current.current_FOAM_Count--;
            //    GlobalManager.Current.BadFoamCount--;
            //    GlobalManager.Current.TotalBadFoam++;
            //}
            //if (GlobalManager.Current.picker4State == false)
            //{
            //    AkrAction.Current.Move(AxisName.FSX, 260, (int)AxisSpeed.FSX);
            //    AkrAction.Current.Move(AxisName.FSY, 390, (int)AxisSpeed.FSY);

            //    SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
            //    SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 1);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_14solenoid_valve4_A, 1);
            //    SetIO(IO_OutFunction_Table.OUT3_15solenoid_valve4_B, 0);
            //    Thread.Sleep(20);
            //    GlobalManager.Current.current_FOAM_Count--;
            //    GlobalManager.Current.BadFoamCount--;
            //    GlobalManager.Current.TotalBadFoam++;
            //}
            return 0;
        }

        public void CalculateFlySnapPath()
        {
            double start_pos_X = GlobalManager.Current.snapPalletePoints[0].X;
            double start_pos_Y = GlobalManager.Current.snapPalletePoints[0].Y;
            int totalRow = GlobalManager.Current.TotalRow;
            int totalColumn = GlobalManager.Current.TotalColumn;
            int gap_X = GlobalManager.Current.PalleteGap_X;
            int gap_Y = GlobalManager.Current.PalleteGap_Y;
            double end_pos_X = (totalColumn - 1) * gap_X;
            double end_pos_Y = (totalRow - 1) * gap_Y;

            snapPalleteList.Clear();

            for (int row = 0; row < totalRow; row++)
            {
                double current_start_pos_Y = start_pos_Y - row * gap_Y; // 当前行的起点Y坐标
                double current_end_pos_Y = current_start_pos_Y; // 当前行的终点Y坐标（在同一行）]
                double current_start_pos_X = start_pos_X;
                double current_end_pos_X = start_pos_X - (totalColumn - 1) * gap_X;
                snapPalleteList.Add(new SinglePoint()
                {
                    X = current_start_pos_X,
                    Y = current_start_pos_Y,
                    Z = 0,
                    R = 0
                });
                snapPalleteList.Add(new SinglePoint()
                {
                    X = current_end_pos_X,
                    Y = current_end_pos_Y,
                    Z = 0,
                    R = 0
                });
            }
        }


        public int SnapPallete()
        {
            //AkrAction.Current.MoveFoamZ1(-3);
            //AkrAction.Current.MoveFoamZ2(-3);
            //Thread.Sleep(1000);
            ////int zUpAll = (AkrAction.Current.MoveNoWait(AxisName.PICK1_Z, 0, (int)AxisSpeed.PICK1_Z) == 0 &&
            ////                AkrAction.Current.Move(AxisName.PICK2_Z, 0, (int)AxisSpeed.PICK2_Z) == 0) ? 0 : (int)ErrorCode.AGM800Err;

            ////if (zUpAll != 0) return zUpAll;
            ////CheckState(zUpAll);

            //palletePath.Clear();

            ////计算实际的拍照位置
            //double start_pos_X = GlobalManager.Current.snapPalletePoints[0].X;
            //double start_pos_Y = GlobalManager.Current.snapPalletePoints[0].Y;
            //int totalRow = GlobalManager.Current.TotalRow;
            //int totalColumn = GlobalManager.Current.TotalColumn;
            //int gap_X = GlobalManager.Current.PalleteGap_X;
            //int gap_Y = GlobalManager.Current.PalleteGap_Y;
            //double end_pos_X = (totalColumn - 1) * gap_X;
            //double end_pos_Y = (totalRow - 1) * gap_Y;
            //double left_end_X = start_pos_X - (totalColumn - 1) * gap_X;

            //bool reverse2 = true;
            //RealPalletePointsList.Clear();
            //for (int row = 0; row < totalRow; row++)
            //{
            //    double current_start_pos_Y = start_pos_Y - row * gap_Y;
            //    double current_end_pos_Y = current_start_pos_Y;
            //    double current_start_pos_X = start_pos_X;
            //    if (reverse2)
            //    {
            //        for (int column = 0; column < totalColumn; column++)
            //        {

            //            RealPalletePointsList.Add(new SinglePoint()
            //            {
            //                X = start_pos_X - column * gap_X,
            //                Y = start_pos_Y - row * gap_Y,
            //                Z = 0,
            //                R = 0
            //            });

            //        }
            //        reverse2 = false;
            //    }
            //    else
            //    {
            //        for (int column = 0; column < totalColumn; column++)
            //        {
            //            RealPalletePointsList.Add(new SinglePoint()
            //            {
            //                X = left_end_X + column * gap_X,
            //                Y = start_pos_Y - row * gap_Y,
            //                Z = 0,
            //                R = 0
            //            });

            //        }
            //        reverse2 = true;
            //    }
            //}

            //var a = RealPalletePointsList;

            ////计算实际的拍照位置

            //for (int count2 = 0; count2 < 12; count2++)
            //{

            //    AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition = new AssUpCamrea.Pushcommand.SendTLTCamreaposition()
            //    {
            //        SN = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //        NozzleID = "0",
            //        MaterialTypeN1 = "Foam",
            //        AcupointNumber = $"{count2 + 1}",
            //        TargetMaterialName1 = "Foam->Module",
            //        Photo_X1 = RealPalletePointsList[count2].X.ToString(),
            //        Photo_Y1 = RealPalletePointsList[count2].Y.ToString(),
            //        Photo_R1 = "0"
            //    };
            //    palletePath.Add(sendTLTCamreaposition);
            //}

            //CalculateFlySnapPath();
            //bool reverse = true;
            //int count = 0;
            //bool has_sent = false;

            //Thread.Sleep(300);

            //while (count < snapPalleteList.Count)
            //{
            //    if (!reverse)
            //    {
            //        Logger.WriteLog("料盘飞拍开始");

            //        AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);
            //        Thread.Sleep(300);
            //        var point = snapPalleteList[count + 1];
            //        AkrAction.Current.MoveFoamXY(point.X - GlobalManager.Current.PalleteGap_X, point.Y);
            //        if (!has_sent)
            //        {
            //            Task_AssUpCameraFunction.TriggAssUpCamreaTLTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.TLT, palletePath);
            //            Thread.Sleep(100);
            //            has_sent = true;
            //        }

            //        AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, snapPalleteList[count + 1].X, GlobalManager.Current.PalleteGap_X, snapPalleteList[count].X, 1);
            //        AkrAction.Current.EventEnable(AxisName.FSX);
            //        AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);
            //        Thread.Sleep(50);

            //        AkrAction.Current.MoveFoamXY(snapPalleteList[count].X + GlobalManager.Current.PalleteGap_X, snapPalleteList[count].Y);

            //        AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response3);

            //        count += 2;

            //        reverse = true;
            //        Thread.Sleep(50);
            //    }
            //    else
            //    {
            //        Logger.WriteLog("料盘飞拍开始");

            //        AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);
            //        Thread.Sleep(50);

            //        var point = snapPalleteList[count];
            //        AkrAction.Current.MoveFoamXY(point.X + GlobalManager.Current.PalleteGap_X, point.Y);

            //        if (!has_sent)
            //        {
            //            Task_AssUpCameraFunction.TriggAssUpCamreaTLTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.TLT, palletePath);
            //            has_sent = true;
            //        }

            //        AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, snapPalleteList[count].X, -GlobalManager.Current.PalleteGap_X, snapPalleteList[count + 1].X, 1);

            //        AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);
            //        Thread.Sleep(50);

            //        AkrAction.Current.MoveFoamXY(snapPalleteList[count + 1].X - GlobalManager.Current.PalleteGap_X, snapPalleteList[count + 1].Y);

            //        AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response3);
            //        count += 2;
            //        reverse = false;
            //    }


            //}
            ////等待Cognex返回的结果
            //GlobalManager.Current.palleteSnaped = true;
            return 0;
        }

        public int PlaceFoam()
        {

            //GlobalManager.Current.picker1State = true;
            //GlobalManager.Current.picker2State = true;
            //GlobalManager.Current.picker3State = false;
            //GlobalManager.Current.picker4State = false;

            //var caveId = (GlobalManager.Current.current_Assembled + 1);

            //if (GlobalManager.Current.picker1State == true)
            //{

            //    if (!GetPlacePosition(1, caveId, out SinglePoint point))
            //    {
            //        return -1;
            //    }

            //    var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X;
            //    var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;
            //    AkrAction.Current.MoveFoamXY(point.X, point.Y);
            //    Logger.WriteLog("开始将PICK1_Z轴移动到21.5的位置");
            //    AkrAction.Current.MoveFoamZ1(21.5);
            //    Logger.WriteLog("开始发送力控信号");
            //    AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[101]=1000", out string response44);
            //    Logger.WriteLog("开始发送力控信号");
            //    Thread.Sleep(100);
            //    AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[102]=5000", out string response123);
            //    Logger.WriteLog("力控信号111");
            //    Thread.Sleep(50);
            //    AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[800]=2", out string response4);
            //    Logger.WriteLog("力控信号222");
            //    while (true)
            //    {
            //        //AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[103]", out string response);
            //        //if (response.Equals("1"))
            //        //{
            //        //    break;
            //        //}

            //        AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[800]", out string response5);
            //        if (response5.Equals("0"))
            //        {
            //            break;
            //        }
            //        Thread.Sleep(500);
            //    }
            //    Thread.Sleep(1000);
            //    Logger.WriteLog("收到力控完毕信号");


            //    SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
            //    Thread.Sleep(200);

            //    AkrAction.Current.MoveFoamZ1(0);

            //    caveId++;
            //    GlobalManager.Current.current_Assembled++;
            //    GlobalManager.Current.current_FOAM_Count--;

            //}
            //if (GlobalManager.Current.picker2State == true)
            //{
            //    if (!GetPlacePosition(2, caveId, out SinglePoint point))
            //    {
            //        return -1;
            //    }

            //    //var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X-16;
            //    //var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;

            //    ////移动到CaveId对应的点
            //    var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X - 16;
            //    var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;

            //    AkrAction.Current.MoveLaserXY(temp_x, temp_y);


            //    AkrAction.Current.MoveFoamZ2(20.5);

            //    SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
            //    Thread.Sleep(200);
            //    AkrAction.Current.MoveFoamZ2(0);

            //    caveId++;
            //    GlobalManager.Current.current_Assembled++;
            //    GlobalManager.Current.current_FOAM_Count--;

            //}
            //if (GlobalManager.Current.picker3State == true)
            //{
            //    //fetchMatrial.Clear();

            //    //var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X - 32;
            //    //var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;

            //    //AkrAction.Current.MoveFoamXY(temp_x, temp_y);

            //    AkrAction.Current.MoveFoamZ3(0);
            //    SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
            //    Thread.Sleep(200);
            //    //SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 1);
            //    //Thread.Sleep(20);
            //    //SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
            //    //Thread.Sleep(20);
            //    AkrAction.Current.MoveFoamZ3(-5);

            //    caveId++;
            //    GlobalManager.Current.current_Assembled++;
            //    GlobalManager.Current.current_FOAM_Count--;

            //}
            //if (GlobalManager.Current.picker4State == true)
            //{
            //    //fetchMatrial.Clear();

            //    //var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X - 48;
            //    //var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;


            //    //AkrAction.Current.MoveFoamXY(temp_x, temp_y);


            //    AkrAction.Current.MoveFoamZ4(0);
            //    SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 1);
            //    Thread.Sleep(20);
            //    SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
            //    Thread.Sleep(20);
            //    AkrAction.Current.MoveFoamZ4(-5);

            //    caveId++;
            //    GlobalManager.Current.current_Assembled++;
            //    GlobalManager.Current.current_FOAM_Count--;

            //}

            return 0;
        }

        public bool Step1()
        {
            //测试用
            Logger.WriteLog("等待组装工位");

            //进板
            //if (!BoradIn())
            //    return false;

            GlobalManager.Current.current_Zuzhuang_step = 1;

            //将当前穴位信息清空
            GlobalManager.Current.palleteSnaped = false;

            //CheckState();

            return true;
        }

        public int Step2()
        {
            Debug.WriteLine("ZuZhuang.Current.Step2()");

            GlobalManager.Current.current_Zuzhuang_step = 2;

            //到feedar上拍照
            return WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            //CheckState();

        }

        public int Step3()
        {
            Debug.WriteLine("ZuZhuang.Current.Step3()");

            GlobalManager.Current.current_Zuzhuang_step = 3;

            //吸嘴取料
            return WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            //CheckState();

        }

        public int Step4()
        {
            Console.WriteLine("ZuZhuang.Current.Step4()");

            GlobalManager.Current.current_Zuzhuang_step = 4;

            //CCD2精定位
            return WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

        }

        public int Step5()
        {
            Console.WriteLine("ZuZhuang.Current.Step5()");

            GlobalManager.Current.current_Zuzhuang_step = 5;

            //拍Pallete料盘
            return WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

        }

        public int Step6()
        {
            Console.WriteLine("ZuZhuang.Current.Step6()");

            GlobalManager.Current.current_Zuzhuang_step = 6;

            //拍Pallete料盘
            return WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

        }

        public int Step7()
        {
            Console.WriteLine("ZuZhuang.Current.Step7()");

            GlobalManager.Current.current_Zuzhuang_step = 7;

            //拍Pallete料盘
            return WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

        }

        public SinglePoint GetZPickPosition(int pickerNum)
        {
            if (GlobalManager.Current.pickerZSafePoints.Count < 1)
            {
                return new SinglePoint();
            }
            return new SinglePoint()
            {
                Z = GlobalManager.Current.pickerZPickPoints[pickerNum - 1].Z // Change this teachpoint to two levels
            };
        }
        public SinglePoint GetZCam2Position(int pickerNum)
        {
            if (GlobalManager.Current.pickerZSafePoints.Count < 1)
            {
                return new SinglePoint();
            }
            return new SinglePoint()
            {
                Z = GlobalManager.Current.pickerZCam2Points[pickerNum - 1].Z
            };
        }
        public SinglePoint GetZSafePosition(int pickerNum)
        {
            if (GlobalManager.Current.pickerZSafePoints.Count < 1)
            {
                return new SinglePoint();
            }
            return new SinglePoint()
            {
                Z = GlobalManager.Current.pickerZSafePoints[pickerNum - 1].Z
            };
        }
        public SinglePoint GetLoadCellPosition(int pickerNum)
        {
            if (GlobalManager.Current.pickerLoadCellPoints.Count < 1)
            {
                return new SinglePoint();
            }
            return new SinglePoint()
            {
                X = GlobalManager.Current.pickerLoadCellPoints[pickerNum - 1].X,
                Y = GlobalManager.Current.pickerLoadCellPoints[pickerNum - 1].Y,
                R = GlobalManager.Current.pickerLoadCellPoints[pickerNum - 1].R,
                Z = GlobalManager.Current.pickerLoadCellPoints[pickerNum - 1].Z
            };
        }
        public bool GetRejectPosition(out SinglePoint point)
        {
            point = new SinglePoint();
            if (GlobalManager.Current.rejectPoints.Count < 1)
            {
                return false;
            }
            point = new SinglePoint()
            {
                X = GlobalManager.Current.rejectPoints[0].X,
                Y = GlobalManager.Current.rejectPoints[0].Y,
                R = GlobalManager.Current.rejectPoints[0].R,
                Z = GlobalManager.Current.rejectPoints[0].Z
            };
            return true;
        }

        /// <summary>
        /// Use this to get the list of teach points for pick process. MUST PERFORM ON THE FLY CAPTURE FIRST
        /// </summary>
        /// <param name="Nozzlenum">Pick selected to pick foam</param>
        /// <param name="Fovnum">Target foam number, number of image selected</param>
        /// <param name="feeder">Selected feeder number</param>
        /// <param name="point">Absolute X,Y,R point to move</param>
        /// <returns>True: Get teach points successfully , False : Failed to get teach points</returns>
        public bool GetPickPosition(int Nozzlenum, int Fovnum, out SinglePoint point)
        {
            point = new SinglePoint();

            if (Nozzlenum < 1 || Nozzlenum > 4 || Fovnum < 1 || Fovnum > 4)
            {
                return false;
            }

            if (PickPositions[Nozzlenum].IsRequested)
            {
                point = PickPositions[Nozzlenum].point;
            }
            else
            {
                string command = "GM,1," + $"{Nozzlenum}" + ",Foam," + $"{Fovnum}," + "1";
                Task_FeedupCameraFunction.PushcommandFunction(command);
                FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend GMout = Task_FeedupCameraFunction.TriggFeedUpCamreaGMAcceptData(FeedupCameraProcessCommand.GM)[0];
                if (GMout.Subareas_Errcode == "1")
                {
                    point.X = double.Parse(GMout.Pick_X);
                    point.Y = double.Parse(GMout.Pick_Y);
                    point.R = double.Parse(GMout.Pick_R);
                }
                else
                {
                    return false;
                }
                PickPositions[Nozzlenum].point = point;
                PickPositions[Nozzlenum].IsRequested = true;
            }

            return true;
        }
        /// <summary>
        /// Use this to get the list of teach points for pick process. MUST PERFORM ON THE FLY CAPTURE FIRST
        /// </summary>
        /// <param name="Nozzlenum">Pick selected to pick foam</param>
        /// <param name="Fovnum">Target foam number, number of image selected</param>
        /// <param name="feeder">Selected feeder number</param>
        /// <param name="point">Absolute X,Y,R point to move</param>
        /// <returns>True: Get teach points successfully , False : Failed to get teach points</returns>
        public bool GetStandbyPickPosition(int Nozzlenum, int Fovnum, int Feedernum, out SinglePoint point)
        {
            point = new SinglePoint();

            if (Nozzlenum < 1 || Nozzlenum > 4 || Fovnum < 1 || Fovnum > 4 || Feedernum < 1 || Feedernum > 2)
            {
                return false;
            }
            var points = Feedernum == 1 ? GlobalManager.Current.pickFoam1Points : GlobalManager.Current.pickFoam2Points;
            point = new SinglePoint()
            {
                X = points[0].X + (Fovnum - Nozzlenum) * App.assemblyGantryControl.XOffset,
                Y = points[0].Y,
                R = points[0].R,
                Z = points[0].Z,

            };

            return true;
        }
        public bool GetStandbyPlacePosition(int Nozzlenum, int Fovnum, out SinglePoint point)
        {
            point = new SinglePoint();
            if (Nozzlenum < 1 || Nozzlenum > 4)
            {
                return false;
            }
            point = new SinglePoint()
            {
                X = GlobalManager.Current.placeFoamPoints[Fovnum - 1].X - (Nozzlenum - 1) * App.assemblyGantryControl.XOffset,
                Y = GlobalManager.Current.placeFoamPoints[Fovnum - 1].Y,
                R = GlobalManager.Current.placeFoamPoints[Fovnum - 1].R,
                Z = GlobalManager.Current.placeFoamPoints[Fovnum - 1].Z,
            };
            return true;

        }
        public bool GetRejectPosition(int Nozzlenum, out SinglePoint point)
        {

            point = new SinglePoint()
            {
                X = GlobalManager.Current.rejectFoamPoints[Nozzlenum - 1].X - (Nozzlenum - 1) * App.assemblyGantryControl.XOffset,
                Y = GlobalManager.Current.rejectFoamPoints[Nozzlenum - 1].Y,
                R = GlobalManager.Current.rejectFoamPoints[Nozzlenum - 1].R,
                Z = GlobalManager.Current.rejectFoamPoints[Nozzlenum - 1].Z,
            };
            return true;
        }
        /// <summary>
        /// Use this to get the list of teach points for pick process. MUST PERFORM ON THE FLY CAPTURE FIRST
        /// </summary>
        /// <param name="Nozzlenum">Pick selected to pick foam</param>
        /// <param name="Fovnum">Target foam number, number of image selected</param>
        /// <param name="singlePoint">Absolute X,Y,R point to move</param>
        /// <returns>True: Get teach points successfully , False : Failed to get teach points</returns>

        public bool GetPlacePosition(int Nozzlenum, int Fovnum, out SinglePoint point)
        {
            point = new SinglePoint();

            if (Nozzlenum < 1 || Nozzlenum > 4 || (Fovnum < 1 || Fovnum > 20))
            {
                return false;
            }
            //if (GlobalManager.Current.CurrentMode == ProcessMode.Dryrun)
            //{
            //    if (!GetPlacePositionDryRun(Nozzlenum, Fovnum, out point))
            //    {
            //        return false;
            //    }
            //    else
            //    {
            //        return true;
            //    }

            //}
            if (PlacePositions[Nozzlenum].IsRequested)
            {
                point = PlacePositions[Nozzlenum].point;
            }
            else
            {
                string command = "GT,1," + $"{Nozzlenum}" + ",Foam," + $"{Fovnum}," + "Foam->Module";
                Task_FeedupCameraFunction.PushcommandFunction(command);
                var GMout = Task_FeedupCameraFunction.TriggFeedUpCamreaGTAcceptData()[0];
                if (GMout.Subareas_Errcode == "1")
                {
                    point.X = double.Parse(GMout.Unload_X);
                    point.Y = double.Parse(GMout.Unload_Y);
                    point.R = double.Parse(GMout.Unload_R);
                    point.Z = 0.0;
                }
                else
                {
                    return false;
                }
                PlacePositions[Nozzlenum].point = point;
            }
            return true;
        }

        private bool _isTrayReadyToProcess = false;
        private bool _isProcessingDone = false;
        public void SetTrayReadyToProcess()
        {
            _isTrayReadyToProcess = true;
        }
        public bool IsProcessOngoing()
        {
            return _isProcessOngoing;
        }
        public void SetRecipe(Recipe recipe)
        {
            _currentRecipe = recipe;
            _rowCount = _currentRecipe.PartRow;
            _colCount = _currentRecipe.PartColumn;
        }
        private void ProcessingDone()
        {
            _isProcessOngoing = false;
            _isTrayReadyToProcess = false;
            _isProcessingDone = true;
        }
        private void StartProcessing()
        {
            _isProcessingDone = false;
            _isProcessOngoing = true;
        }
        private void SetFeederIndex()
        {
            foreach (var index in _feederIndex)
            {
                index.hasPart = true;
            }
        }
        private bool IsActiveFeederChanged()
        {
            // Check if the active feeder has changed
            var currentActiveFeeder = Feeder.Current.GetActiveFeederNumber();
            if ((CognexVisionControl.FeederNum)currentActiveFeeder != _activeFeederNum)
            {
                Logger.WriteLog($"Active feeder changed from {_activeFeederNum} to {currentActiveFeeder}");
                return true;
            }
            return false;
        }
        private bool SetFoamTracker(CognexVisionControl.FeederNum feeder, List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> results)
        {
            ProductTracker tracker;
            foreach (var result in results)
            {
                var status = result.Errcode1 == "1";
                var fovNum = Int32.TryParse(result.FOV1, out int foamNum);

                switch (feeder)
                {
                    case CognexVisionControl.FeederNum.Feeder1:
                        tracker = App.productTracker.Feeder1Foams;
                        break;
                    case CognexVisionControl.FeederNum.Feeder2:
                        tracker = App.productTracker.Feeder2Foams;
                        break;
                    default:
                        return false;
                }
                tracker.PartArray[foamNum - 1].present = true;
                tracker.PartArray[foamNum - 1].failed = status ? false : true;
                if (!status)
                {
                    tracker.PartArray[foamNum - 1].FailReason = FailReason.FoamOnTheFlyFail;
                }
            }
            return true;
        }
        private bool SetPickerTrackerAfterOnTheFly(List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> results)
        {
            ProductTracker tracker = App.productTracker.GantryPickerFoams;
            for (int i = 0; i < results.Count; i++)
            {
                var status = results[i].Errcode == "1";
                tracker.PartArray[i].present = true;
                tracker.PartArray[i].failed = status ? false : true;
                if (!status)
                {
                    tracker.PartArray[i].FailReason = FailReason.BottomOnTheFLyFail;
                }
            }

            return true;
        }
        public bool IsGantryPickerFailed => App.productTracker.GantryPickerFoams.PartArray.Any(x => x.present && !x.failed);

        private bool SetFeederFailed(CognexVisionControl.FeederNum feeder, int foamNumber)
        {
            ProductTracker trackerFeeder;

            switch (feeder)
            {
                case CognexVisionControl.FeederNum.Feeder1:
                    trackerFeeder = App.productTracker.Feeder1Foams;
                    break;
                case CognexVisionControl.FeederNum.Feeder2:
                    trackerFeeder = App.productTracker.Feeder2Foams;
                    break;
                default:
                    return false;
            }

            trackerFeeder.PartArray[foamNumber].SetFail(FailReason.FailToPick);
            return true;
        }
        public override bool AutoRun()
        {
            var feeder1XDirection = App.paramLocal.LiveParam.Feeder1OnTheFlyXDirection;
            var feeder2XDirection = App.paramLocal.LiveParam.Feeder1OnTheFlyXDirection;
            var XDirection = _activeFeederNum == CognexVisionControl.FeederNum.Feeder1 ? feeder1XDirection : feeder2XDirection;
            var bottomVisionXDirection = App.paramLocal.LiveParam.TrayOnTheFlyXDirection;
            var bottomVisionYDirection = App.paramLocal.LiveParam.TrayOnTheFlyYDirection;
            // MOVE TO PICK POSITION AT CURRENT ACTIVE FEEDER
            if (_movestep == 0)
            {
                if (IsTimeOut())
                {
                    return ErrorManager.Current.Insert(ErrorCode.FeederEmpty, $"Feeder is out");
                }
                // Get active feeder number
                _activeFeederNum = Feeder.Current.GetActiveFeederNumber() == 1 ?
                CognexVisionControl.FeederNum.Feeder1 :
                CognexVisionControl.FeederNum.Feeder2;

                if (Feeder.Current.CanPick() && Feeder.Current.IsFeederReady())
                {
                    _movestep = 1;
                }

            }

            // Move the All picker back to zero degree
            if (_movestep == 1)
            {
                if (!App.assemblyGantryControl.TZeroAll(false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.assemblyGantryControl.TZeroAll(false)");
                }


                _movestep = 2;
            }

            // Move to vision standby position
            if (_movestep == 2)
            {
                if (!App.visionControl.MoveToFoamVisionStandbyPos(_activeFeederNum, XDirection, waitMotionDone: false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveToFoamVisionStandbyPos(_activeFeederNum, foamDirection, waitMotionDone: false)");
                }

                _movestep = 3;
                ResetTimeout();
            }

            // WAIT MOTOR TO REACH on the fly first POSITION
            if (_movestep == 3)
            {
                if (IsTimeOut())
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionTimeoutErr, $"GetFoamStandbyPos({_activeFeederNum})");
                }
                if (!App.visionControl.GetFoamStandbyPos(_activeFeederNum, XDirection, out SinglePoint point))
                {
                    return ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"GetFoamStandbyPos(_activeFeederNum, foamDirection, out SinglePoint point)");
                }

                if (AkrAction.Current.IsMoveFoamXYDone(point.X, point.Y))
                {
                    _movestep = 2;
                }

            }

            // ON THE FLY CAPTURE PICK POSITION
            if (_movestep == 2)
            {
                if (!IsActiveFeederChanged()) // Check if active feeder has changed
                {
                    //App.visionControl.VisionOnTheFlyFoam(_activeFeederNum);//Bypassed by Raymond
                    _movestep = 4;
                }
                else
                {
                    _movestep = 0; // Move to the new active feeder position
                }
            }

            // READY ON THE FLY CAPTURE
            if (_movestep == 4)
            {
                if (!App.visionControl.VisionOnTheFlyFoam(_activeFeederNum, out List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> results, waitResult: false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.OnTheFlyFoamFailed, "Foam assembly on the fly failed");
                }

                _currentPickerIndex = 0;
                _movestep = 5; // Proceed to pick part sequence
            }

            // WAIT MOTOR TO PRE TEACH PICK POSITION FOR STANDBY with the first available picker
            if (_subMovestep == 5)
            {
                var selectedPickerIndex = GetNextPickerEnableIndex();
                if (!App.assemblyGantryControl.MoveStandbyPickPos((AssemblyGantryControl.Picker)selectedPickerIndex, 1, (int)_activeFeederNum, false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.assemblyGantryControl.MoveStandbyPickPos((AssemblyGantryControl.Picker){_selectedPicker}, {_selectedFeederIndex}, false))");
                }

                _movestep = 6;
            }

            // Get vision result from on the fly foam
            if (_subMovestep == 6)
            {
                if (!App.visionControl.GetTLMResult(out var TLMResults))
                {
                    return ErrorManager.Current.Insert(ErrorCode.OnTheFlyFoamFailed, $"Vision return is not ok,App.visionControl.IsAllFeederVisionOK()");
                }

                SetFoamTracker(_activeFeederNum, TLMResults);
                _movestep = 7; // skip 5 checking motor position temporarily
            }

            // WAIT MOTOR TO PRE TEACH PICK POSITION FOR STANDBY
            if (_subMovestep == 7)
            {
                if (!App.visionControl.IsAllFeederVisionOK())
                {
                    return ErrorManager.Current.Insert(ErrorCode.OnTheFlyFoamFailed, $"Vision return is not ok.{App.visionControl.GetFailedFoamResult()}");
                }
                _remainder = 0;
                _movestep = 8;
            }


            // PICK SEQUENCE
            if (_movestep == 8)
            {
                var pickResult = PickPartSequence(_activeFeederNum); // Perform pick parts sequence
                if (pickResult == 1) // ALL PICKERS HAVE BEEN PROCESSED
                {
                    _movestep = 9;
                }
                else if (pickResult < 0)
                {
                    return false; // PICKING FAILED
                }
            }

            // MOVE TO ON THE FLY CAPTURE POSITION
            if (_movestep == 9)
            {
                if (!App.visionControl.MoveToBottomVisionStandbyPos(bottomVisionXDirection, false)) // MOVE TO OTF CAPTURE POSITION
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveToBottomVisionStandbyPos(bottomVisionDirection, true)");
                }

                _movestep = 10;
            }

            // Trigger again TRotate to 90 degree
            if (_movestep == 10)
            {
                if (!App.assemblyGantryControl.TRotateAll(90, false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveToBottomVisionStandbyPos(bottomVisionDirection, true)");
                }

                ResetTimeout();
                _movestep = 11;
            }

            // Wait All picker rotate to 90 degree done
            if (_movestep == 11)
            {
                if (IsTimeOut())
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.visionControl.MoveToBottomVisionStandbyPos(bottomVisionDirection, true)");
                }
                if (App.assemblyGantryControl.IsTAllRotateDone(90))
                {
                    _movestep = 12;
                }
            }
            // Wait All picker rotate to 90 degree done
            if (_movestep == 12)
            {
                if (!App.assemblyGantryControl.ZCamPosAll(false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"ZCamPosAll(false)");
                }

                ResetTimeout();
                _movestep = 13;
            }

            // Wait All picker rotate to 90 degree done
            if (_movestep == 13)
            {
                if (IsTimeOut())
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.visionControl.MoveToBottomVisionStandbyPos(bottomVisionDirection, true)");
                }
                if (App.assemblyGantryControl.GetZCamPos(out var point))
                {
                    return ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"GetZCamPos(out var point)");
                }

                if (App.assemblyGantryControl.IsZAllDone(point.Z))
                {
                    _movestep = 14;
                    ResetTimeout();
                }
            }

            // Check picker XY reach bottom vision on the fly standby vision 
            if (_movestep == 14)
            {
                if (IsTimeOut())
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.visionControl.MoveToBottomVisionStandbyPos(bottomVisionDirection, true)");
                }

                if (!App.visionControl.GetBottomVisionStandbyPos(bottomVisionXDirection, out SinglePoint point))
                {
                    return ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"GetBottomVisionStandbyPos(bottomVisionDirection, out SinglePoint point)");

                }
                if (AkrAction.Current.IsMoveFoamXYDone(point.X, point.Y))
                {
                    _movestep = 15;
                }

            }

            // START ON THE FLY CAPTURE SEQUENCE
            if (_movestep == 15)
            {
                if (!App.visionControl.Vision2OnTheFlyTrigger(bottomVisionXDirection, out var TLNResults)) // CALL OTF CAPTURE SEQUENCE
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.visionControl.MoveToBottomVisionStandbyPos(bottomVisionDirection, true)");
                }

                SetPickerTrackerAfterOnTheFly(TLNResults);
                _movestep = IsGantryPickerFailed ? 16 : 19;
            }

            // MOVE TO REJECT POSITION
            if (_movestep == 16)
            {
                if (!App.assemblyGantryControl.MoveRejectPos(false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.visionControl.MoveToBottomVisionStandbyPos(bottomVisionDirection, true)");
                }

                _movestep = 17;
                ResetTimeout();
            }
            // Check XY reach reject position
            if (_movestep == 17)
            {
                if (IsTimeOut())
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionTimeoutErr, $"App.assemblyGantryControl.MoveRejectPos(false)");
                }
                if (!GetRejectPosition(out var point))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"GetRejectPosition(out var point)");
                }

                if (AkrAction.Current.IsMoveFoamXYDone(point.X, point.Y))
                {
                    _movestep = 18;
                }
            }

            // Reject failed foam according to tracker
            if (_movestep == 18)
            {
                if (!App.assemblyGantryControl.RejectFailedFoam()) // CALL OTF CAPTURE SEQUENCE
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"RejectFailedFoam");
                }

                _movestep = 19;
            }

            // Go To pallet on the fly position
            if (_movestep == 19)
            {
                if (!App.visionControl.MoveToPalletVisionStartingPos(App.lotManager.CurrLot.Recipe, false))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveToPalletVisionStartingPos(App.lotManager.CurrLot.Recipe, false)");
                }

                _movestep = 20;
                ResetTimeout();
            }
            // Go To pallet on the fly position
            if (_movestep == 20)
            {
                if (!IsTimeOut())
                {
                    return ErrorManager.Current.Insert(ErrorCode.TimeOut, $"MoveToPalletVisionStartingPos(App.lotManager.CurrLot.Recipe, false)");
                }
                if (!App.visionControl.GetPalletStandbyPosition(App.lotManager.CurrLot.Recipe, out SinglePoint point))
                {
                    return ErrorManager.Current.Insert(ErrorCode.motionErr, $"GetPalletStandbyPosition");
                }
                if (AkrAction.Current.IsMoveFoamXYDone(point.X, point.Y))
                {
                    _movestep = 21;
                }
            }

            // CHECK IF TRAY AVAILABILITY
            if (_movestep == 21)
            {
                if (IsProcessOngoing())
                {
                    _movestep = 23; // SKIP TRAY WAITING
                }
                else
                {
                    _movestep = 22; // WAIT FOR TRAY IN POSITION
                }
            }

            // WAIT FOR TRAY IN POSITION
            if (_movestep == 22)
            {
                if (_isTrayReadyToProcess)
                {
                    StartProcessing();
                    _trayInspectDone = false;
                    _currentTrayPlaceIndex = 0;
                    _movestep = 23;
                }
            }

            // ON THE FLY PALLET TRIGGER
            if (_movestep == 23)
            {
                // If not yet inspected, trigger on the fly vision first
                if (!_trayInspectDone)
                {
                    _trayOTFTravelPaths.Clear();
                    _TLTcommands.Clear();
                    _trayOTFProductPosition.Clear();
                    _TrayOTFRowDone = 0;
                    _trayCaptureMovestep = 0;
                    _movestep = 24;
                }
                else
                {
                    _movestep = 25;
                }
            }

            if (_movestep == 24)
            {
                var onTheFlyResult = TrayOnTheFlySequence();
                if (onTheFlyResult == 1) // ALL PICKERS HAVE BEEN PROCESSED
                {
                    _movestep = 25;

                    _currentPickerIndex = 0;
                    _trayPlaceMovestep = 0;
                    _trayInspectDone = true;
                }
                else if (onTheFlyResult < 0)
                {
                    return ErrorManager.Current.Insert(ErrorCode.OnTheFlyTrayFailed);
                }
            }

            // TRAY PLACE SEQUENCE
            if (_movestep == 25)
            {
                var placeResult = TrayPlaceSequence();
                if (placeResult == 1) // ALL PICKERS HAVE BEEN PROCESSED
                {
                    _movestep = 0;
                }
                else if (placeResult == 2) // ALL TRAY SLOTS HAVE BEEN PROCESSED
                {
                    // DONE PROCESS
                    ProcessingDone();
                    Conveyor.Current.ProcessingDone(Conveyor.ConveyorStation.Foam, true);
                    _movestep = 0; // RESET MOVESTEP FOR NEXT CYCLE
                }
                else if (placeResult < 0)
                {
                    return false; // PLACING FAILED
                }
            }
            return true;
        }

        private int PickCaptureSequence()
        {
            if (_pickCaptureMovestep == 0)
            {
                _pickCaptureMovestep = 1;
            }

            return 0;
        }

        private int PickPartSequence(CognexVisionControl.FeederNum feeder)
        {
            // SELECT PICKER AND FEEDER INDEX THEN MOVE TO POSITION
            ProductTracker tracker = feeder == CognexVisionControl.FeederNum.Feeder1 ?
                App.productTracker.Feeder1Foams :
                App.productTracker.Feeder2Foams;
            if (_subMovestep == 0)
            {
                // CHECK IF PICKER INDEX HAS REACHED MAXIMUM
                if (_currentPickerDone >= _pickers.Count(x => x.IsEnabled))
                {
                    _remainder = 1;
                    return 1; // ALL PICKERS HAVE BEEN PROCESSED
                }
                if (_currentPickerIndex >= _pickers.Count(x => x.IsEnabled))
                {
                    _currentPickerIndex = 0;
                }

                //if (_currentFeederIndex >= _feederIndex.Count)
                if (_currentFeederIndex >= tracker.PartArray.Count(x => x.CanPick()))
                {
                    // temporary assume all parts placed
                    //SetFeederIndex(); // Set all feeder index to true
                    return 1; // ALL FEEDERS HAVE BEEN PROCESSED
                }

                if (_pickers[_currentPickerIndex].IsEnabled && _currentPickerIndex % 2 == _remainder)
                {
                    if (App.productTracker.FeederCanBePick(feeder, _currentFeederIndex) &&
                        App.productTracker.PickerCanDoPick(_currentPickerIndex))
                    {
                        _selectedPicker = _pickers[_currentPickerIndex].PickerIndex;
                        _selectedFeederIndex = _feederIndex[_currentFeederIndex].PartIndex;
                        // MOVE TO PICK POSITION
                        _subMovestep = 1;
                    }
                    else
                    {
                        // Feeder index does not have part, skip to next picker
                        _currentFeederIndex++;
                    }
                }
                else
                {
                    _currentPickerIndex++;
                }
            }



            // WAIT MOTOR TO REACH POSITION
            if (_subMovestep == 1)
            {
                bool result = false;
                if (GlobalManager.Current.CurrentMode == ProcessMode.Dryrun)
                {
                    result = App.assemblyGantryControl.PickFoamDryRun((AssemblyGantryControl.Picker)_selectedPicker, _selectedFeederIndex, (int)feeder, waitTRotate90: false);
                }
                else
                {
                    result = App.assemblyGantryControl.PickFoam((AssemblyGantryControl.Picker)_selectedPicker, _selectedFeederIndex, waitTRotate90: false);
                }

                if (!result)
                {
                    if (App.assemblyGantryControl.CanPickRetry)
                    {
                        ErrorManager.Current.Insert(App.assemblyGantryControl.ProcessErrorCode, App.assemblyGantryControl.ProcessErrorMessage);
                        ErrorManager.Current.Insert(ErrorCode.FoamPickFailed, "Failed to pick, reset and retry");
                        return -1;
                    }
                    else
                    {
                        _currentPickerDone++;
                        _currentPickerIndex++;
                        _currentFeederIndex++;
                        App.productTracker.PickerPickFail(_activeFeederNum, _selectedFeederIndex);
                        ErrorManager.Current.Insert(App.assemblyGantryControl.ProcessErrorCode, App.assemblyGantryControl.ProcessErrorMessage);
                        ErrorManager.Current.Insert(ErrorCode.FoamPickFailed, "Failed to pick, please remove");
                        return -1;
                    }
                }

                _currentPickerIndex++;
                _currentFeederIndex++;
                _currentPickerDone++;
                App.productTracker.PickerPicked(_activeFeederNum, _selectedFeederIndex, _selectedPicker);
                _subMovestep = 0;
            }
            // Move Z down to Bottom Vision Position on the fly position
            if (_subMovestep == 100)
            {
                //if (App.assemblyGantryControl.ZCamPosAll(false))
                if (true)
                {
                    _subMovestep = 0;
                }
                else
                {
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"App.assemblyGantryControl.ZCamPosAll(false)");
                    return -1; // MOVE FAILED
                }
            }
            // ZDOWN TO PICK POSITION
            if (_subMovestep == 2)
            {
                if (true)
                {
                    _subMovestep = 3;
                }
                else
                {
                    return -1; // MOVE FAILED
                }
            }

            // CHECK IF MOTOR REACHED POSITION
            if (_subMovestep == 3)
            {
                if (true) // Replace with actual motor position check
                {
                    _subMovestep = 4; // Proceed to pick part sequence
                }
                else
                {
                    return -1; // MOTOR FAILED TO REACH POSITION
                }
            }

            // VACUUM ON AND WAIT FOR AWHILE
            if (_subMovestep == 4)
            {
                if (true) // Replace with actual vacuum on command
                {
                    Thread.Sleep(500);// wait for vacuum to stabalize. (TEMP) Proper way is to loop
                    _subMovestep = 5; // Proceed to check if part is picked
                }
                else
                {
                    return -1; // VACUUM ON FAILED
                }
            }

            // ZUP TO SAFE POSITION
            if (_subMovestep == 5)
            {
                if (true) // Replace with actual Z up command
                {
                    _subMovestep = 0; // Proceed to check if part is picked
                }
                else
                {
                    return -1; // Z UP FAILED
                }
            }

            return 0;
        }

        private int TrayOnTheFlySequence()
        {
            // GET AVAILABLE POINTS
            if (_trayCaptureMovestep == 0)
            {
                if (!App.visionControl.GetPalletXYPoints(_currentRecipe, out List<SinglePoint> points, out List<VisionTravelPath> travelPaths))
                {
                    ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"GetPalletXYPoints(_currentRecipe, out List<SinglePoint> points, out List<VisionTravelPath> travelPaths)");
                    return -1;
                }

                if (!App.visionControl.XYPointToTLTCommand(points, out List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> commands))
                {
                    ErrorManager.Current.Insert(ErrorCode.TeachpointErr, $"XYPointToTLTCommand(points, out List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> commands)");
                    return -1;
                }

                _trayOTFProductPosition = points;
                _trayOTFTravelPaths = travelPaths;
                _TLTcommands = commands;
                _trayCaptureMovestep = 1;
            }


            if (_trayCaptureMovestep == 1)
            {
                if (!App.visionControl.SetAgitoXOnTheFlyModeOff())
                {
                    ErrorManager.Current.Insert(ErrorCode.AGM800Err, $"SetAgitoXOnTheFlyModeOff()");
                    return -1;
                }
                _trayCaptureMovestep = 2;
            }


            if (_trayCaptureMovestep == 2)
            {
                if (_currentOnTheFlyRowIndex >= _trayOTFTravelPaths.Count)
                {
                    _currentOnTheFlyRowIndex = 0;
                    _trayCaptureMovestep = 0;
                    return 1; // ALL PICKERS HAVE BEEN PROCESSED
                }
                _trayCaptureMovestep = 3;
            }

            // MOVE TO Starting point
            if (_trayCaptureMovestep == 3)
            {
                var startingPoint = _trayOTFTravelPaths[_currentOnTheFlyRowIndex].StartingPoint;
                if (AkrAction.Current.MoveFoamXY(startingPoint.X, startingPoint.Y, waitMotionDone: false) != (int)AkrAction.ACTTION_ERR.NONE)
                {
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveFoamXY(startingPoint.X, startingPoint.Y, waitMotionDone: false");
                    return -1;
                }
                _trayCaptureMovestep = 4;
                ResetTimeout();
            }

            if (_trayCaptureMovestep == 4)
            {
                if (IsTimeOut())
                {
                    ErrorManager.Current.Insert(ErrorCode.TimeOut, $"MoveFoamXY(startingPoint.X, startingPoint.Y, waitMotionDone: false");
                    return -1;
                }

                var startingPoint = _trayOTFTravelPaths[_currentOnTheFlyRowIndex].StartingPoint;
                if (AkrAction.Current.IsMoveFoamXYDone(startingPoint.X, startingPoint.Y))
                {
                    _trayCaptureMovestep = 5;
                }
            }

            if (_trayCaptureMovestep == 5)
            {
                if (_currentOnTheFlyRowIndex == 0 && !App.visionControl.SendTLTCommands(_TLTcommands))
                {
                    ErrorManager.Current.Insert(ErrorCode.CognexErr, $"SendTLTCommands(_TLTcommands)");
                    return -1;
                }

                _trayCaptureMovestep = 6;
            }

            if (_trayCaptureMovestep == 6)
            {
                if (!App.visionControl.SetAgitoXOnTheFlyModeOn(_trayOTFProductPosition[_currentOnTheFlyRowIndex * _currentRecipe.PartColumn].X,
                    _trayOTFProductPosition[_currentOnTheFlyRowIndex * _currentRecipe.PartColumn + _currentRecipe.PartColumn - 1].X,
                    _currentRecipe.XPitch, 1))
                {
                    ErrorManager.Current.Insert(ErrorCode.AGM800Err, $"SetAgitoXOnTheFlyModeOn()");
                    return -1;
                }

                _trayCaptureMovestep = 7;
            }
            if (_trayCaptureMovestep == 7)
            {
                var endingPoint = _trayOTFTravelPaths[_currentOnTheFlyRowIndex].EndingPoint;
                if (AkrAction.Current.MoveFoamXY(endingPoint.X, endingPoint.Y, waitMotionDone: false) != (int)AkrAction.ACTTION_ERR.NONE)
                {
                    ErrorManager.Current.Insert(ErrorCode.motionErr, $"MoveFoamXY(endingPoint.X, endingPoint.Y, waitMotionDone: false");
                    return -1;
                }

                _trayCaptureMovestep = 8;
                ResetTimeout();
            }

            if (_trayCaptureMovestep == 8)
            {
                if (IsTimeOut())
                {
                    ErrorManager.Current.Insert(ErrorCode.TimeOut, $"MoveFoamXY(endingPoint.X, endingPoint.Y, waitMotionDone: false");
                    return -1;
                }

                var endingPoint = _trayOTFTravelPaths[_currentOnTheFlyRowIndex].EndingPoint;
                if (AkrAction.Current.IsMoveFoamXYDone(endingPoint.X, endingPoint.Y))
                {
                    _trayCaptureMovestep = 1;
                    _currentOnTheFlyRowIndex++;
                }
            }

            return 0;
        }


        private int TrayPlaceSequence()
        {
            // GET AVAILABLE PICKER
            if (_trayPlaceMovestep == 0)
            {
                if (_currentPickerIndex >= _pickers.Count)
                {
                    _currentPickerIndex = 0;
                    return 1; // ALL PICKERS HAVE BEEN PROCESSED
                }
                if (App.productTracker.IsAllAvailablePartPlaceDone)
                {
                    return 2; // ALL TRAY SLOTS HAVE BEEN PROCESSED
                }
                if (_pickers[_currentPickerIndex].IsEnabled && App.productTracker.PickerCanDoPlace(_currentPickerIndex))
                {
                    if (App.productTracker.TrayCanBePlace(_currentTrayPlaceIndex))
                    {
                        _selectedPicker = _currentPickerIndex + 1;
                        _selectedTrayIndex = _currentTrayPlaceIndex;
                        _trayPlaceMovestep = 1;
                    }
                    else
                    {
                        _currentTrayPlaceIndex++;
                    }
                }
                else
                {
                    _currentPickerIndex++;
                }
            }

            // SELECT PICKER AND MOVE TO PLACE
            if (_trayPlaceMovestep == 1)
            {
                bool result = false;
                if (GlobalManager.Current.CurrentMode == ProcessMode.Dryrun)
                {
                    result = App.assemblyGantryControl.PlaceFoamDryrun((AssemblyGantryControl.Picker)_selectedPicker, _selectedTrayIndex + 1);
                }
                else
                {
                    result = App.assemblyGantryControl.PlaceFoam((AssemblyGantryControl.Picker)_selectedPicker, _selectedTrayIndex + 1);
                }

                if (!result)
                {
                    if (App.assemblyGantryControl.CanPlaceRetry)
                    {
                        ErrorManager.Current.Insert(App.assemblyGantryControl.ProcessErrorCode, App.assemblyGantryControl.ProcessErrorMessage);
                        ErrorManager.Current.Insert(ErrorCode.FoamPlaceFailed, $"Failed to place from picker {_selectedPicker} to tray index {_currentTrayPlaceIndex + 1}, reset and retry");
                        return -1; // PLACE FAILED
                    }
                    else
                    {
                        App.productTracker.PickerPlaceFail((AssemblyGantryControl.Picker)_selectedPicker, _selectedTrayIndex);
                        ErrorManager.Current.Insert(App.assemblyGantryControl.ProcessErrorCode, App.assemblyGantryControl.ProcessErrorMessage);
                        ErrorManager.Current.Insert(ErrorCode.FoamPlaceFailed, $"Failed to place from picker {_selectedPicker} to tray index {_currentTrayPlaceIndex + 1}, please remove");
                        return -1; // PLACE FAILED
                    }

                }
                _currentTrayPlaceIndex++;
                _currentPickerIndex++;
                _trayPlaceMovestep = 0;

                App.productTracker.PickerPlaced((AssemblyGantryControl.Picker)_selectedPicker, _selectedTrayIndex);

                if (App.productTracker.IsAllAvailablePartPlaceDone)
                {
                    return 2; // ALL TRAY SLOTS HAVE BEEN PROCESSED
                }
            }
            return 0;
        }


        private class Picker
        {
            public int PickerIndex { get; set; }
            public bool IsEnabled { get; set; } = true;
        }

        private class FeederIndex
        {
            public int PartIndex { get; set; }
            public bool hasPart { get; set; } = false;
        }

        public override void Paused()
        {
            return;
        }

        public override void ResetAfterPause()
        {
            startTime = DateTime.Now;
        }
    }
}
