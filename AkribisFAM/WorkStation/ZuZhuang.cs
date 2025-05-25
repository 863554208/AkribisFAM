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

namespace AkribisFAM.WorkStation
{
    internal class ZuZhuang : WorkStationBase
    {
        private static ZuZhuang _instance;
        public override string Name => nameof(ZuZhuang);

        int delta = 0;
        public int board_count = 0;

        List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> snapFeederPath = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();
        List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> ccd2SnapPath = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> palletePath = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> ();
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

        public void CheckState()
        {
            GlobalManager.Current.Zuzhuang_state[GlobalManager.Current.current_Zuzhuang_step] = 0;
            GlobalManager.Current.ZuZhuang_CheckState();
            WarningManager.Current.WaitZuZhuang();
        }

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }
        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
            DateTime time = DateTime.Now;
            bool ret = false;
            while ((DateTime.Now - time).TotalMilliseconds < delta)
            {
                if (ReadIO(index) == value)
                {
                    ret = true;
                    break;
                }
                Thread.Sleep(50);
            }

            return ret;
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
        public bool BoradIn()
        {
            if (true)
            {
                //将要板信号清空
                Set("IO_test2", false);
                Set("station2_IsBoardInHighSpeed", true);

                //传送带高速移动
                MoveConveyor((int)AxisSpeed.BL1);

                //等待减速光电2 , false为感应到
                if (!WaitIO(999999, IO_INFunction_Table.IN1_1Slowdown_Sign2, false)) throw new Exception();

                //阻挡气缸2上气
                SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 1);
                SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 0);

                //标志位转换
                Set("station2_IsBoardInHighSpeed", false);
                Set("station2_IsBoardInLowSpeed", true);

                //传送带减速
                MoveConveyor(10);

                //等待料盘挡停到位信号1
                if (!WaitIO(999999, IO_INFunction_Table.IN1_5Stop_Sign2, true)) throw new Exception();

                //停止皮带移动，直到该工位顶升完成，才能继续移动皮带
                Set("station2_IsBoardInLowSpeed", false);
                Set("station2_IsLifting", true);

                StopConveyor();

                //执行测距位顶升气缸顶升                

                SetIO(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 0);
                SetIO(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 0);

                if (!WaitIO(999999, IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos, true)) throw new Exception();

                Set("station1_IsLifting", false);
                Set("station2_IsBoardIn", false);
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

        public void WaitConveyor(int type)
        {
            DateTime time = DateTime.Now;
            switch (type)
            {
                case 2:
                    while (SnapFeedar() == 1) ;
                    break;

                case 3:
                    while (PickFoam() == 1) ;
                    break;

                case 4:
                    while (LowerCCD() == 1) ;
                    break;

                case 5:
                    while (DropBadFoam() == 1) ;
                    break;

                case 6:
                    while (SnapPallete() == 1) ;
                    break;

                case 7:
                    while (PlaceFoam() == 1) ;
                    break;
            }
        }

        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveConveyor(vel);
        }

        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
        }

        public bool ReadIO(IO_INFunction_Table index)
        {
            if (IOManager.Instance.INIO_status[(int)index] == 0)
            {
                return true;
            }
            else if (IOManager.Instance.INIO_status[(int)index] == 1)
            {
                return false;
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

        public int SnapFeedar()
        {
            //feedar信号
            //while (!ReadIO(IO_INFunction_Table.IN4_2Platform_has_label_feeder1) && !ReadIO(IO_INFunction_Table.IN4_6Platform_has_label_feeder2))
            //{
            //    Thread.Sleep(100);
            //}
            //优先选择feedar1 ,再选择feedar2

            //给Cognex发信息
            if (GlobalManager.Current.UseFeedar1)
            {
                snapFeederPath.Clear();
                int index = 0;
                foreach (var Point in GlobalManager.Current.feedar1Points)
                {
                    FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                    {
                        SN1 = "ASDASD",
                        RawMaterialName1 = "FOAM",
                        FOV = index.ToString(),
                        Photo_X1 = Point.X.ToString(),
                        Photo_Y1 = Point.Y.ToString(),
                        Photo_R1 = "0"
                    };
                    snapFeederPath.Add(sendTLMCamreaposition1);
                }
                //给Cognex发拍照信息
                Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath);

                //移动到拍照起始点
                AkrAction.Current.MoveNoWait(AxisName.FSX, GlobalManager.Current.feedar1Points[0].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.feedar1Points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

                AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.feedar1Points[0].X, 50, GlobalManager.Current.feedar1Points[1].X, 1);

                //移动到拍照结束点
                AkrAction.Current.MoveNoWait(AxisName.FSX, GlobalManager.Current.feedar1Points[1].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.feedar1Points[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

                ////接受Cognex的信息
                List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> msg_received = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
                msg_received = Task_FeedupCameraFunction.TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand.TLM);

                Logger.WriteLog("feedar飞拍接收到的消息为:" + msg_received[0].Errcode1);
            }
            else if (GlobalManager.Current.UseFeedar2)
            {
                snapFeederPath.Clear();
                int index = 0;
                foreach (var Point in GlobalManager.Current.feedar2Points)
                {
                    FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                    {
                        SN1 = "ASDASD",
                        RawMaterialName1 = "FOAM",
                        FOV = index.ToString(),
                        Photo_X1 = Point.X.ToString(),
                        Photo_Y1 = Point.Y.ToString(),
                        Photo_R1 = "0"
                    };
                    snapFeederPath.Add(sendTLMCamreaposition1);
                }
                //给Cognex发拍照信息
                Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath);

                //移动到拍照起始点
                AkrAction.Current.MoveNoWait(AxisName.FSX, GlobalManager.Current.feedar2Points[0].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.feedar2Points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

                AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.feedar2Points[0].X, 50, GlobalManager.Current.feedar2Points[1].X, 1);

                //移动到拍照结束点
                AkrAction.Current.MoveNoWait(AxisName.FSX, GlobalManager.Current.feedar2Points[1].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.feedar2Points[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

                ////接受Cognex的信息
                List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> msg_received = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
                msg_received = Task_FeedupCameraFunction.TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand.TLM);

                Logger.WriteLog("feedar飞拍接收到的消息为:" + msg_received[0].Errcode1);
            }

            //根据congex返回的结果判断坐标，以及是否有


            GlobalManager.Current.BadFoamCount = 0;
            return 0;
        }

        private int[] FeederState = new int[4];//飞拍位置数量=4
        private int[] NozzleState = new int[4];//吸嘴数量=4
        private int[] NozzlePickNum = new int[4];//吸嘴数量=4
        public List<SinglePoint> PickFeederPoints = new List<SinglePoint>(4);

        public int PickFoam1234(List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> msg_received) {
            for(int i = 0; i < 4; ++i)
            {
                if (msg_received[i].Errcode1 == "1" && msg_received[i].Subareas_Errcode11 == "1")
                {
                    FeederState[i] = 1;
                }
                else {
                    FeederState[i] = 0;
                }
            }
            for(int i = 0; i < 4; ++i)
            {
                if (NozzleState[i] == 1) { 
                    for(int j = 0; j < 4; ++j)
                    {
                        if(FeederState[j] == 1)
                        {
                            NozzlePickNum[i] = j;
                        }
                        else
                        {
                            NozzlePickNum[i] = -1;
                        }
                    }
                }
                NozzlePickNum[i] = -1;
            }

            for(int i = 0; i < 4; ++i)
            {
                if (NozzlePickNum[i] != -1) {
                    try {
                        string command = "GM,1," + $"{i + 1}" + ",Foam," + $"{NozzlePickNum[i] + 1}," + "1";
                        Task_FeedupCameraFunction.PushcommandFunction(command);
                        FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend GMout = Task_FeedupCameraFunction.TriggFeedUpCamreaGMAcceptData(FeedupCameraProcessCommand.GM)[0];
                        if (GMout.Subareas_Errcode == "1")
                        {
                            PickFeederPoints[i].X = double.Parse(GMout.Pick_X);
                            PickFeederPoints[i].Y = double.Parse(GMout.Pick_Y);
                            PickFeederPoints[i].R = double.Parse(GMout.Pick_R);
                        }
                    }
                    catch
                    {
                        return -1;
                    }
                }
            }
            return 0;
        }

        public SinglePoint GetPickPosition(int Nozzlenum, int Fovnum) {
            SinglePoint singlePoint = new SinglePoint();
            string command = "GM,1," + $"{Nozzlenum}" + ",Foam," + $"{Fovnum}," + "1";
            Task_FeedupCameraFunction.PushcommandFunction(command);
            FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend GMout = Task_FeedupCameraFunction.TriggFeedUpCamreaGMAcceptData(FeedupCameraProcessCommand.GM)[0];
            if (GMout.Subareas_Errcode == "1")
            {
                singlePoint.X = double.Parse(GMout.Pick_X);
                singlePoint.Y = double.Parse(GMout.Pick_Y);
                singlePoint.R = double.Parse(GMout.Pick_R);
            }
            return singlePoint;
        }

        public int PickFoam()
        {
            //移动到取料位
            AkrAction.Current.Move(AxisName.FSX, GlobalManager.Current.pickFoamPoints[0].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.pickFoamPoints[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX);

            if (GlobalManager.Current.UsePicker1)
            {
                SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply,0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                Thread.Sleep(20);

                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                Thread.Sleep(20);

                AkrAction.Current.MoveNoWait(AxisName.PICK1_Z, 26, (int)AxisSpeed.PICK1_Z);
                
                //SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                
                GlobalManager.Current.current_FOAM_Count++;
            }

            if (GlobalManager.Current.UsePicker2)
            {
                AkrAction.Current.MoveNoWait(AxisName.PICK2_Z, 26, (int?)(int)AxisSpeed.PICK2_Z);
                SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 1);
                Thread.Sleep(20);
                //SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);

                GlobalManager.Current.current_FOAM_Count++;
            }

            if (GlobalManager.Current.UsePicker3)
            {
                AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 26, (int?)(int)AxisSpeed.PICK3_Z);
                SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply,1);
                Thread.Sleep(20);
                //SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);

                GlobalManager.Current.current_FOAM_Count++;
            }

            if (GlobalManager.Current.UsePicker4)
            {
                AkrAction.Current.MoveNoWait(AxisName.PICK4_Z, 26, (int?)(int)AxisSpeed.PICK4_Z);
                SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 1);
                Thread.Sleep(20);
                //SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);

                GlobalManager.Current.current_FOAM_Count++;
            }
            //让飞达送料
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_9Run_feeder1, 1);

            Thread.Sleep(200);
            AkrAction.Current.MoveNoWait(AxisName.PICK1_Z, 10, (int)AxisSpeed.PICK1_Z);
            AkrAction.Current.MoveNoWait(AxisName.PICK2_Z, 10, (int)AxisSpeed.PICK2_Z);
            AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 10, (int)AxisSpeed.PICK3_Z);
            AkrAction.Current.Move(AxisName.PICK4_Z, 10, (int)AxisSpeed.PICK4_Z);

            return 0;
        }

        public int LowerCCD()
        {

            //ccd2SnapPath.Clear();
            //foreach (var Point in GlobalManager.Current.feedarPoints)
            //{
            //    PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition SendTLNCamreaposition = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition()
            //    {
            //        SN = "asd",
            //        NozzleID = "1",
            //        RawMaterialName = "1",
            //        CaveID = "0",
            //        TargetMaterialName1 = "1",
            //        Photo_X1 = Point.X.ToString(),
            //        Photo_Y1 = Point.Y.ToString(),
            //        Photo_R1 = Point.Y.ToString(),

            //    };
            //    ccd2SnapPath.Add(SendTLNCamreaposition);
            //}

            ////给Cognex发拍照信息
            //Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNSendData(PrecisionDownCamreaProcessCommand.TLN, ccd2SnapPath);

            //移动到拍照起始点
            AkrAction.Current.MoveNoWait(AxisName.FSX, GlobalManager.Current.lowerCCDPoints[0].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.lowerCCDPoints[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

            AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.lowerCCDPoints[0].X, 20, GlobalManager.Current.lowerCCDPoints[1].X, 1);

            //移动到拍照结束点
            AkrAction.Current.MoveNoWait(AxisName.FSX, GlobalManager.Current.lowerCCDPoints[1].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.lowerCCDPoints[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

            //接受Cognex信息
            //List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> AcceptTLNDownPosition = new List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition>();
            //AcceptTLNDownPosition = Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNAcceptData(PrecisionDownCamreaProcessCommand.TLN);

            return 0;
        }

        public int DropBadFoam()
        {
            if (GlobalManager.Current.picker1State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 290, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 300, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_8solenoid_valve1_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_9solenoid_valve1_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
                GlobalManager.Current.BadFoamCount--;
                GlobalManager.Current.TotalBadFoam++;
            }
            if (GlobalManager.Current.picker2State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 280, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 390, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_10solenoid_valve2_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_11solenoid_valve2_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
                GlobalManager.Current.BadFoamCount--;
                GlobalManager.Current.TotalBadFoam++;
            }
            if (GlobalManager.Current.picker3State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 270, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 390, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_12solenoid_valve3_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_13solenoid_valve3_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
                GlobalManager.Current.BadFoamCount--;
                GlobalManager.Current.TotalBadFoam++;
            }
            if (GlobalManager.Current.picker4State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 260, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 390, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_14solenoid_valve4_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_15solenoid_valve4_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
                GlobalManager.Current.BadFoamCount--;
                GlobalManager.Current.TotalBadFoam++;
            }
            return 0;
        }

        public int SnapPallete()
        {
            palletePath.Clear();
            //foreach(var Point in GlobalManager.Current.snapPalletePoints)
            //{

            //    AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition = new AssUpCamrea.Pushcommand.SendTLTCamreaposition()
            //    {
            //        SN = "ASDASD",
            //        NozzleID = "1",
            //        MaterialTypeN1 = "0",
            //        AcupointNumber = count.ToString(),
            //        TargetMaterialName1 = "0",
            //        Photo_X1 = Point.X.ToString(),
            //        Photo_Y1 = Point.Y.ToString(),
            //        Photo_R1 = "0"
            //    };
            //    palletePath.Add(sendTLTCamreaposition);
            //    count++;
            //}

            //Task_AssUpCameraFunction.TriggAssUpCamreaTLTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.TLT, palletePath);
            int count = 0;
            foreach (var Point in GlobalManager.Current.snapPalletePoints)
            {
                Logger.WriteLog("料盘飞拍开始");

                AkrAction.Current.Move(AxisName.FSX, GlobalManager.Current.snapPalletePoints[count].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.snapPalletePoints[count].Y, (int)AxisSpeed.FSY ,(int)AxisAcc.FSX);

                AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.snapPalletePoints[count].X, 50, GlobalManager.Current.snapPalletePoints[count+1].X, 1);

                count++;

                AkrAction.Current.Move(AxisName.FSX, GlobalManager.Current.snapPalletePoints[count].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.snapPalletePoints[count].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX);

                count++;

                Thread.Sleep(100);

            }
            //等待Cognex返回的结果
            GlobalManager.Current.palleteSnaped = true;
            return 0;
        }

        public int PlaceFoam() 
        { 

            GlobalManager.Current.picker1State = true;
            GlobalManager.Current.picker2State = true;
            GlobalManager.Current.picker3State = true;
            GlobalManager.Current.picker4State = true;

            var caveId = (GlobalManager.Current.current_Assembled + 1);

            if (GlobalManager.Current.picker1State == true)
            {
                fetchMatrial.Clear();
                AssUpCamrea.Pushcommand.SendGTCommandAppend sendGTCommandAppend = new AssUpCamrea.Pushcommand.SendGTCommandAppend()
                {
                    NozzlelD1 ="1",
                    RawMaterialName1 = "123",
                    CaveID1 = caveId.ToString(),
                    TargetMaterialName1="123"
                };
                fetchMatrial.Add(sendGTCommandAppend);
                Task_AssUpCameraFunction.TriggAssUpCamreaGTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.GT, fetchMatrial);

                var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X;
                var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;
                AkrAction.Current.Move(AxisName.FSX ,temp_x, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, temp_y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX);

                AkrAction.Current.Move(AxisName.PICK1_Z, 20, (int)AxisSpeed.PICK1_Z);

                SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                Thread.Sleep(20);
                AkrAction.Current.Move(AxisName.PICK1_Z,0, (int)AxisSpeed.PICK1_Z);

                caveId++;
                GlobalManager.Current.current_Assembled++;
                GlobalManager.Current.current_FOAM_Count--;
                
            }
            if (GlobalManager.Current.picker2State == true)
            {
                fetchMatrial.Clear();
                AssUpCamrea.Pushcommand.SendGTCommandAppend sendGTCommandAppend = new AssUpCamrea.Pushcommand.SendGTCommandAppend()
                {
                    NozzlelD1 = "2",
                    RawMaterialName1 = "123",
                    CaveID1 = caveId.ToString(),
                    TargetMaterialName1 = "123"
                };
                fetchMatrial.Add(sendGTCommandAppend);
                Task_AssUpCameraFunction.TriggAssUpCamreaGTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.GT, fetchMatrial);

                //移动到CaveId对应的点
                var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X-20;
                var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;
                AkrAction.Current.Move(AxisName.FSX, temp_x, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, temp_y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX);

                AkrAction.Current.Move(AxisName.PICK2_Z, 20, (int)AxisSpeed.PICK2_Z);
                AkrAction.Current.Move(AxisName.PICK2_T, 30, (int)AxisSpeed.PICK2_T);
                SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
                Thread.Sleep(20);
                AkrAction.Current.Move(AxisName.PICK2_Z, 0, (int)AxisSpeed.PICK2_Z);

                caveId++;
                GlobalManager.Current.current_Assembled++;
                GlobalManager.Current.current_FOAM_Count--;

            }            
            if (GlobalManager.Current.picker3State == true)
            {
                fetchMatrial.Clear();
                AssUpCamrea.Pushcommand.SendGTCommandAppend sendGTCommandAppend = new AssUpCamrea.Pushcommand.SendGTCommandAppend()
                {
                    NozzlelD1 = "3",
                    RawMaterialName1 = "123",
                    CaveID1 = caveId.ToString(),
                    TargetMaterialName1 = "123"
                };
                fetchMatrial.Add(sendGTCommandAppend);
                Task_AssUpCameraFunction.TriggAssUpCamreaGTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.GT, fetchMatrial);

                //移动到CaveId对应的点
                var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X - 40;
                var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;
                AkrAction.Current.Move(AxisName.FSX, temp_x, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, temp_y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX);

                AkrAction.Current.Move(AxisName.PICK3_Z, 20, (int)AxisSpeed.PICK3_Z);
                SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
                Thread.Sleep(20);
                AkrAction.Current.Move(AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z);

         

                caveId++;
                GlobalManager.Current.current_Assembled++;
                GlobalManager.Current.current_FOAM_Count--;

            }
            if (GlobalManager.Current.picker4State == true)
            {
                fetchMatrial.Clear();
                AssUpCamrea.Pushcommand.SendGTCommandAppend sendGTCommandAppend = new AssUpCamrea.Pushcommand.SendGTCommandAppend()
                {
                    NozzlelD1 = "4",
                    RawMaterialName1 = "123",
                    CaveID1 = caveId.ToString(),
                    TargetMaterialName1 = "123"
                };
                fetchMatrial.Add(sendGTCommandAppend);
                Task_AssUpCameraFunction.TriggAssUpCamreaGTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.GT, fetchMatrial);

                //移动到CaveId对应的点
                var temp_x = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].X - 60;
                var temp_y = (int)GlobalManager.Current.placeFoamPoints[caveId - 1].Y;
                AkrAction.Current.Move(AxisName.FSX, temp_x, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
                AkrAction.Current.Move(AxisName.FSY, temp_y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX);

                AkrAction.Current.Move(AxisName.PICK4_Z, 20, (int)AxisSpeed.PICK4_Z);
                SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
                Thread.Sleep(20);
                AkrAction.Current.Move(AxisName.PICK4_Z, 0, (int)AxisSpeed.PICK4_Z);

                caveId++;
                GlobalManager.Current.current_Assembled++;
                GlobalManager.Current.current_FOAM_Count--;

            }

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

            CheckState();

            return true;
        }

        public bool Step2()
        {
            Debug.WriteLine("ZuZhuang.Current.Step2()");

            GlobalManager.Current.current_Zuzhuang_step = 2;

            //到feedar上拍照
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public bool Step3()
        {
            Debug.WriteLine("ZuZhuang.Current.Step3()");

            GlobalManager.Current.current_Zuzhuang_step = 3;

            //吸嘴取料
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public bool Step4()
        {
            Console.WriteLine("ZuZhuang.Current.Step4()");

            GlobalManager.Current.current_Zuzhuang_step = 4;

            //CCD2精定位
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            CheckState();


            return true;
        }

        public bool Step5()
        {
            Console.WriteLine("ZuZhuang.Current.Step5()");

            GlobalManager.Current.current_Zuzhuang_step = 5;

            //拍Pallete料盘
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public bool Step6()
        {
            Console.WriteLine("ZuZhuang.Current.Step6()");

            GlobalManager.Current.current_Zuzhuang_step = 6;

            //拍Pallete料盘
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            Logger.WriteLog("waitstep6 start");
            CheckState();
            Logger.WriteLog("waitstep6 end");
            return true;
        }

        public bool Step7()
        {
            Console.WriteLine("ZuZhuang.Current.Step7()");

            GlobalManager.Current.current_Zuzhuang_step = 7;

            //拍Pallete料盘
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public async override void AutoRun(CancellationToken token)
        {
            board_count  = 0;
            GlobalManager.Current.current_Assembled = 0;
            GlobalManager.Current.total_Assemble_Count = 4;
            try
            {

                while (true)
                {

                    step1:
                        //if (!GlobalManager.Current.IO_test2 || board_count != 0)
                        //{
                        //    Thread.Sleep(100);
                        //    continue;
                        //}

                         Step1();

                        //var task1 = Task.Run(() => Step1());
                        
                        if (GlobalManager.Current.SendByPassToStation2) goto step9;
                        if (GlobalManager.Current.Zuzhuang_exit) break;
                        //如果吸嘴上有料，直接跳去CCD2精定位
                        if (GlobalManager.Current.current_FOAM_Count > 0) goto step4;

                    step2:
                        //飞达上拍料;
                        Step2();
                        if (GlobalManager.Current.Zuzhuang_exit) break;


                    step3:
                        //吸嘴取料
                        Step3();
                        if (GlobalManager.Current.Zuzhuang_exit) break;

                    step4:
                    //CCD2精定位
                        Logger.WriteLog("精定位开始");
                        Step4();
                        Logger.WriteLog("精定位完成");
                        if (GlobalManager.Current.Zuzhuang_exit) break;
                        if (GlobalManager.Current.BadFoamCount > 0)
                        {
                            goto step5;
                        }
                        else
                        {
                            goto step6;
                        }

                    step5:
                        //如果有坏料，放到坏料盒里
                        Step5();
                        if (GlobalManager.Current.Zuzhuang_exit) break;

                    step6:
                        if (GlobalManager.Current.palleteSnaped) goto step7;
                        Logger.WriteLog("开始等待料盘到位");
                        while (GlobalManager.Current.flag_assembleTrayArrived != 1)
                        {
                            Thread.Sleep(300);
                        }
                        Logger.WriteLog("等到料盘到位");
                        GlobalManager.Current.flag_assembleTrayArrived = 0;

                        Logger.WriteLog("组装工位开始飞拍");
                        //拍料盘                        
                        Step6();
                            if (GlobalManager.Current.Zuzhuang_exit) break;

                    step7:
                        Logger.WriteLog("开始放料");
                        //放料
                        Step7();
                        if (GlobalManager.Current.Zuzhuang_exit) break;
                        //当前组装的料小于穴位数时，要一直取料
                        if (GlobalManager.Current.current_Assembled < GlobalManager.Current.total_Assemble_Count) goto step2;

                    step8:
                        BoardOut();
                        GlobalManager.Current.SendByPassToStation3 = false;
                        continue;

                    step9:
                        GlobalManager.Current.SendByPassToStation3 = true;
                        BoardOut();

                }

                #region 老代码
                //if (GlobalManager.Current.IO_test2 && !has_board)
                //{
                //    WorkState = 1;
                //    has_board = true;
                //    GlobalManager.Current.IO_test2 = false;
                //    Console.WriteLine("贴膜工位板已进");
                //}

                //// 处理板
                //if (has_board && WorkState == 1)
                //{
                //    try
                //    {
                //        WorkState = 2;
                //        GlobalManager.Current.total_Assemble_Count = 12;
                //        GlobalManager.Current.current_Assembled = 0;
                //        GlobalManager.Current.current_FOAM_Count = 0;
                //        while (GlobalManager.Current.current_Assembled < GlobalManager.Current.total_Assemble_Count)
                //        {
                //            if (GlobalManager.Current.current_FOAM_Count == 0)
                //            {
                //                //TODO 相机拍飞达上的料
                //                OnZuZhuangExecuted_2?.Invoke();
                //                while (!GlobalManager.Current.CCD1InPosition)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //                OnZuZhuangExecuted_1?.Invoke();
                //                while (!GlobalManager.Current.Feedar1Captured)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //                Console.WriteLine("已拍飞达上的料");
                //                //TODO 吸嘴吸取飞达上的4个料

                //                //现在吸嘴上实际吸了4个料
                //                GlobalManager.Current.current_FOAM_Count += 4;
                //            }

                //            //TODO 相机到CCD2拍照精定位

                //            OnZuZhuangExecuted_3?.Invoke();
                //            while (!GlobalManager.Current.CCD2Captured)
                //            {
                //                Thread.Sleep(300);
                //            }
                //            Console.WriteLine("已拍CCD2上的料");

                //            if (!GlobalManager.Current.has_XueWeiXinXi)
                //            {
                //                //TODO 对料盘拍照获取穴位信息

                //                OnZuZhuangExecuted_4?.Invoke();
                //                while (!GlobalManager.Current.MoveToLiaopan)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //                OnZuZhuangExecuted_5?.Invoke();
                //                while (!GlobalManager.Current.GrabLiaoPan)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //            }

                //            //TODO 组装

                //            //目前料盘上组装了多少料
                //            GlobalManager.Current.current_Assembled += 4;

                //            //吸嘴上现在有多少foam（减去实际贴上去的料的数量） ： 如果没有foam，下一片板子走正常流程 ；如果有foam , 不再拍feeder上的料的图片
                //            GlobalManager.Current.current_FOAM_Count -= 4;

                //            Thread.Sleep(300);

                //        }

                //        WorkState = 3; // 更新状态为出板
                //    }
                //    catch (Exception ex)
                //    {
                //        has_error = true; // 标记为出错
                //    }
                //}

                //// 出板
                //if (WorkState == 3 || has_error)
                //{
                //    if (has_error)
                //    {
                //        AutorunManager.Current.isRunning = false;
                //    }

                //    WorkState = 0;
                //    has_board = false;
                //    Console.WriteLine("组装工位板已出");
                //}
                //System.Threading.Thread.Sleep(100);
                #endregion
            }
            catch (Exception ex)
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }

    }
}
