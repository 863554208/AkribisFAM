using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AAMotion;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using System.Diagnostics;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.CommunicationProtocol.Task_PrecisionDownCamreaFunction;
using System.Windows.Controls;
using AkribisFAM.Util;
using static AkribisFAM.CommunicationProtocol.Task_AssUpCameraFunction;
using System.Windows;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace AkribisFAM.WorkStation
{
    internal class ZuZhuang : WorkStationBase
    {
        DeviceClass.CognexVisionControl.FeederNum _activeFeederNum = DeviceClass.CognexVisionControl.FeederNum.Feeder1;
        public SinglePoint[] PickPositions = new SinglePoint[4];
        public SinglePoint[] PlacePositions = new SinglePoint[4];

        private static ZuZhuang _instance;
        private static int _movestep = 0;
        private static int _pickCaptureMovestep = 0;
        private static int _pickPartMovestep = 0;
        private static int _placeInspectMovestep = 0;
        private static int _placePartMovestep = 0;
        private static int _trayPlaceMovestep = 0;
        private static int _currentTrayPlaceIndex = 0;
        private static int _currentPickerIndex = 0;
        private bool _isProcessOngoing = false;
        private bool _trayInspectDone = false;
        private int _rowCount = 3;
        private int _colCount = 4;
        private List<TraySlot> _traySlots = new List<TraySlot>();
        private Recipe _currentRecipe = App.recipeManager.GetRecipe(TrayType.PAM_230_144_3X4);
        private static int _selectedPicker;
        private List<Picker> _pickers = new List<Picker>()
        {
            new Picker() { PickerIndex = 1, IsDisabled = false },
            new Picker() { PickerIndex = 2, IsDisabled = false },
            new Picker() { PickerIndex = 3, IsDisabled = true },
            new Picker() { PickerIndex = 4, IsDisabled = true },
        };
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
                ErrorManager.Current.Insert(ErrorCode.IOErr);
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
            //        TargetMaterialName1 = "Foam->Moudel",
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
            //        TargetMaterialName1 = "Foam->Moudel",
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
                Z = GlobalManager.Current.pickerZPickPoints[pickerNum - 1].Z
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
        private bool GetPickPositionDryRun(int Nozzlenum,  int fovNum ,int feedernum, out SinglePoint point)
        {
            var points = feedernum == 1 ? GlobalManager.Current.pickFoam1Points : GlobalManager.Current.pickFoam2Points;
            Thread.Sleep(1);
            point = new SinglePoint()
            {
                X = points[0].X + (fovNum - Nozzlenum) * App.assemblyGantryControl.XOffset,
                Y = points[0].Y,
                R = points[0].R,
                Z = points[0].Z,
            };
            return true;

        }
        /// <summary>
        /// Use this to get the list of teach points for pick process. MUST PERFORM ON THE FLY CAPTURE FIRST
        /// </summary>
        /// <param name="Nozzlenum">Pick selected to pick foam</param>
        /// <param name="Fovnum">Target foam number, number of image selected</param>
        /// <param name="point">Absolute X,Y,R point to move</param>
        /// <returns>True: Get teach points successfully , False : Failed to get teach points</returns>
        public bool GetPickPosition(int Nozzlenum, int Fovnum, out SinglePoint point)
        {
            point = new SinglePoint();
            int feeder = 1;

            if (Nozzlenum < 1 || Nozzlenum > 4 || Fovnum < 1 || Fovnum > 4)
            {
                return false;
            }
            if (GlobalManager.Current.CurrentMode == RunMode.DryrunMode)
            {
                if (!GetPickPositionDryRun(Nozzlenum, Fovnum, feeder, out point))
                {
                    PickPositions[Nozzlenum] = point;
                    return false;
                }
                else
                {
                    PickPositions[Nozzlenum] = point;
                    return true;
                }

            }
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
            PickPositions[Nozzlenum] = point;
            return true;
        }
        private bool GetPlacePositionDryRun(int Nozzlenum, int Fovnum, out SinglePoint point)
        {

            Thread.Sleep(1);
            point = new SinglePoint()
            {
                X = GlobalManager.Current.placeFoamPoints[Fovnum-1].X - (Nozzlenum - 1) * App.assemblyGantryControl.XOffset,
                Y = GlobalManager.Current.placeFoamPoints[Fovnum-1].Y,
                R = GlobalManager.Current.placeFoamPoints[Fovnum-1].R,
                Z = GlobalManager.Current.placeFoamPoints[Fovnum - 1].Z,
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
            if (GlobalManager.Current.CurrentMode == RunMode.DryrunMode)
            {
                if (!GetPlacePositionDryRun(Nozzlenum, Fovnum, out point))
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }

            string command = "GT,1," + $"{Nozzlenum}" + ",Foam," + $"{Fovnum}," + "Foam->Moudel";
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
            PlacePositions[Nozzlenum] = point;
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

        public async override void AutoRun(CancellationToken token)
        {

            //App.vision1.MoveFoamStandbyPos();
            //App.vision1.Vision1OnTheFlyFoamTrigger();
            //App.assemblyGantryControl.PickFoam() // which picker to which foam
            //App.assemblyGantryControl.PickAllFoam() // which picker to which foam
            //App.vision1.MoveVision2StandbyPos();
            //App.vision1.Vision2OnTheFlyTrigger();
            //App.vision1.Vision1OnTheFlyPalletTrigger();
            //App.assemblyGantryControl.PlaceFoam() // which picker to which foam

            // MOVE TO PICK POSITION AT CURRENT ACTIVE FEEDER
            if (_movestep == 0)
            {
                // Get active feeder number
                _activeFeederNum = Feeder.Current.GetActiveFeederNumber() == 1 ? 
                    DeviceClass.CognexVisionControl.FeederNum.Feeder1 : 
                    DeviceClass.CognexVisionControl.FeederNum.Feeder2;

                //if (Feeder.Current.CanPick() && Feeder.Current.IsFeederReady())
                //{
                    App.vision1.MoveFoamStandbyPos(_activeFeederNum);
                //}
                // timeout if no feeder is ready at certain duration

                _movestep = 1;
            }

            // WAIT MOTOR TO REACH POSITION
            if (_movestep == 1)
            {
                var points = App.vision1.GetFoamStandbyPos(_activeFeederNum);
                if (AkrAction.Current.IsMoveFoamXYDone(points[0].X - 16, points[0].Y))
                {
                    _movestep = 2;
                }
                else
                {
                    // Handle error or retry logic here
                    return; // MOVE FAILED
                }
                // additional check for timeout or error handling if needed
            }

            // ON THE FLY CAPTURE PICK POSITION
            if (_movestep == 2)
            {
                if (App.vision1.Vision1OnTheFlyFoamTrigger(_activeFeederNum))
                {
                    _movestep = 3;
                }
                else
                {
                    // Handle error or retry logic here
                    return; // ON THE FLY CAPTURE FAILED   
                }

                    //var PickSeqResult = PickCaptureSequence();
                    //if (PickSeqResult == 1)
                    //{
                    //    _movestep = 3;
                    //} else if (PickSeqResult == -1)
                    //{

                    //    return; // PICK CAPTURE FAILED
                    //}
            }
            
            // PICK PART SEQUENCE
            if (_movestep == 3)
            {
                if (App.assemblyGantryControl.PickAllFoam())
                {
                    _movestep = 4;
                }
                else
                {
                    // Handle error or retry logic here
                    _movestep = 4;
                    //return; // PICK FAILED
                }
                //var PickResult = PickPartSequence();
                //if (PickResult == 1)
                //{
                //    _movestep = 4;
                //}
                //else if (PickResult == -1)
                //{
                //    _movestep = 2; // RETRY PICK CAPTURE SEQUENCE
                //    return; // PICK FAILED
                //}
            }

            // MOVE TO ON THE FLY CAPTURE POSITION
            if (_movestep == 4)
            {
                if (App.vision1.MoveVision2StandbyPos()) // MOVE TO OTF CAPTURE POSITION
                {
                    _movestep = 6; // skip 5 checking motor position temporarily
                }
                else
                {
                    return; // MOVE FAILED
                }
            }

            // (SKIP TEMPORARILY)
            if (_movestep == 5)
            {
                if (true) 
                {
                    if (true) 
                    {
                        _movestep = 6;
                    }
                    else
                    {
                        _movestep = 4; // RETRY MOVE TO OTF CAPTURE POSITION
                        return; // MOTOR FAILED TO REACH POSITION
                    }
                }
            }

            // START ON THE FLY CAPTURE SEQUENCE
            if (_movestep == 6)
            {
                if (App.vision1.Vision2OnTheFlyTrigger()) // CALL OTF CAPTURE SEQUENCE
                {
                    _movestep = 7;
                }
                else
                {
                    return; // CAPTURE FAILED
                }
            }

            // CHECK IF TRAY AVAILABILITY
            if (_movestep == 7)
            {
                if (IsProcessOngoing())
                {
                    _movestep = 9; // SKIP TRAY WAITING
                } else
                {
                    _movestep = 8; // WAIT FOR TRAY IN POSITION
                }
            }

            // WAIT FOR TRAY IN POSITION
            if (_movestep == 8)
            {
                if (_isTrayReadyToProcess)
                {
                    StartProcessing();
                    _traySlots = new List<TraySlot>();
                    for (int i = 0; i < _currentRecipe.PartRow * _currentRecipe.PartColumn; i++)
                    {
                        _traySlots.Add(new TraySlot());
                    }
                    _trayInspectDone = false;
                    _currentTrayPlaceIndex = 0;
                    _movestep = 9;
                }
            }

            // ON THE FLY PALLET TRIGGER
            if (_movestep == 9)
            {
                if (!_trayInspectDone) // If not yet inspected, trigger on the fly vision first
                {
                    if (App.vision1.Vision1OnTheFlyPalletTrigger(_currentRecipe.PartRow, _currentRecipe.PartColumn))
                    {
                        _currentPickerIndex = 0;
                        _trayPlaceMovestep = 0;
                        _trayInspectDone = true;
                        _movestep = 10;
                    }
                    else
                    {
                        // todo error handling
                        return;
                    }
                }
                else
                {
                    _currentPickerIndex = 0;
                    _trayPlaceMovestep = 0;
                    _movestep = 10;
                }
           
            }

            // TRAY PLACE SEQUENCE
            if (_movestep == 10)
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
                    // Placement error
                    //return;
                }
            }
        }

        private int PickCaptureSequence()
        {
            if (_pickCaptureMovestep == 0)
            {
                _pickCaptureMovestep = 1;
            }

            return 0;
        }

        private int PickPartSequence()
        {

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
                if (_currentTrayPlaceIndex >= _traySlots.Count)
                {
                    return 2; // ALL TRAY SLOTS HAVE BEEN PROCESSED
                }
                if (_pickers[_currentPickerIndex].IsDisabled == false)
                {
                    _selectedPicker = _pickers[_currentPickerIndex].PickerIndex;
                    _trayPlaceMovestep = 1;
                }
                else
                {
                    _currentPickerIndex++;
                }
            }

            // SELECT PICKER AND MOVE TO PLACE
            if (_trayPlaceMovestep == 1)
            {
                if (App.assemblyGantryControl.PlaceFoam((DeviceClass.AssemblyGantryControl.Picker)_selectedPicker, _currentTrayPlaceIndex + 1))
                {
                    _traySlots[_currentTrayPlaceIndex].IsOccupied = true; // Mark the tray slot as occupied
                    _currentTrayPlaceIndex++;
                    _currentPickerIndex++;
                    _trayPlaceMovestep = 0;
                    if (_currentTrayPlaceIndex >= _traySlots.Count)
                    {
                        return 2; // ALL TRAY SLOTS HAVE BEEN PROCESSED
                    }
                } else
                {
                    //// TODO REMOVE AFTER DRY RUN
                    //_traySlots[_currentTrayPlaceIndex].IsOccupied = true; // Mark the tray slot as occupied
                    //_currentTrayPlaceIndex++;
                    //_currentPickerIndex++;
                    //_trayPlaceMovestep = 0;
                    //if (_currentTrayPlaceIndex >= _traySlots.Count)
                    //{
                    //    return 2; // ALL TRAY SLOTS HAVE BEEN PROCESSED
                    //}

                    return -1; // PLACE FAILED
                }
            }
            return 0;
        }

        private class TraySlot
        {
            public int SlotIndex { get; set; }
            //public SinglePoint Position { get; set; }
            public bool IsOccupied { get; set; } = false;
        }

        private class Picker
        {
            public int PickerIndex { get; set; }
            public bool IsDisabled { get; set; } = false;
        }

    }
}
