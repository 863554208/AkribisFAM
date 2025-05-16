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

namespace AkribisFAM.WorkStation
{
    internal class ZuZhuang : WorkStationBase
    {
        private static ZuZhuang _instance;
        public override string Name => nameof(ZuZhuang);

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;
        public event Action OnTriggerStep4;
        public event Action OnStopStep4;

        int delta = 0;
        public int board_count = 0;

        List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> snapFeederPath = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();
        List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> ccd2SnapPath = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> palletePath = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> ();
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
            if (GlobalManager.Current.station1_IsBoardInLowSpeed || GlobalManager.Current.station3_IsBoardInLowSpeed || GlobalManager.Current.station4_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(100);
            }
            else if (GlobalManager.Current.station1_IsBoardInHighSpeed || GlobalManager.Current.station3_IsBoardInHighSpeed || GlobalManager.Current.station4_IsBoardInHighSpeed)
            {
                MoveConveyor((int)AxisSpeed.BL1);
            }
        }


        public bool BoradIn()
        {
            //20250516 进板改为异步进板 【史彦洋】 修改 Start
            //if (GlobalManager.Current.IO_test2 && board_count==0)
            if (true)
            //20250516 进板改为异步进板 【史彦洋】 修改 End
            {

                //将要板信号清空
                Set("IO_test2", false);
                Set("station2_IsBoardInHighSpeed", true);

                //传送带高速移动
                MoveConveyor((int)AxisSpeed.BL1);

                Set("station2_IsBoardInHighSpeed", false);

                //等待减速光电2
                if (!WaitIO(999999, IO_INFunction_Table.IN1_1Slowdown_Sign2, true)) throw new Exception();

                //阻挡气缸2上气
                SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 1);
                SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 0);

                //标志位转换
                Set("station2_IsBoardInHighSpeed", false);
                Set("station2_IsBoardInLowSpeed", true);

                //传送带减速
                MoveConveyor(100);

                //等待料盘挡停到位信号1
                if (!WaitIO(999999, IO_INFunction_Table.IN1_5Stop_Sign2, true)) throw new Exception();

                //停止皮带移动，直到该工位顶升完成，才能继续移动皮带
                Set("station2_IsBoardInLowSpeed", false);
                Set("station2_IsBoardIn", false);
                Set("station2_IsLifting", true);

                StopConveyor();

                //执行测距位顶升气缸顶升                

                SetIO(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 0);
                SetIO(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 0);

                Set("station1_IsLifting", false);

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
            Set("station2_IsBoardOut", true);

            //模拟给下一个工位发进板信号
            GlobalManager.Current.IO_test3 = true;

            //如果后续工站正在执行出站，就不要让该工位的气缸放气和下降
            while (GlobalManager.Current.station3_IsBoardOut || GlobalManager.Current.station4_IsBoardOut)
            {
                Thread.Sleep(100);
            }
            StopConveyor();
            SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 0);
            SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 1);

            //出板时将穴位信息清空
            GlobalManager.Current.palleteSnaped = false;

            Set("station1_IsBoardOut", false);
            board_count--;

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
            return IOManager.Instance.INIO_status[(int)index];

        }

        public void SetIO(IO_OutFunction_Table index, int value)
        {
            IOManager.Instance.IO_ControlStatus(index, value);
        }

        public int SnapFeedar()
        {
            while (!ReadIO(IO_INFunction_Table.IN4_1Platform_has_label_feeder1) && !ReadIO(IO_INFunction_Table.IN4_5Platform_has_label_feeder2))
            {
                Thread.Sleep(100);
            }
            //优先选择feedar1 ,再选择feedar2
            snapFeederPath.Clear();
            foreach (var Point in GlobalManager.Current.feedarPoints)
            {
                FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                {
                    SN1 = "ASDASD",
                    RawMaterialName1 = "FOAM",
                    FOV = "1",
                    Photo_X1 = Point.X.ToString(),
                    Photo_Y1 = Point.Y.ToString(),
                    Photo_R1 = "0"
                };
                snapFeederPath.Add(sendTLMCamreaposition1);
            }
            //给Cognex发拍照信息
            Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath);

            foreach (var Point in GlobalManager.Current.feedarPoints)
            {
                AkrAction.Current.SetSingleEvent(AxisName.FSX, (int)AxisSpeed.FSX,1);
                AkrAction.Current.MoveNoWait(AxisName.FSX, (int)Point.X, (int)AxisSpeed.FSX);
                AkrAction.Current.MoveNoWait(AxisName.FSY, (int)Point.Y, (int)AxisSpeed.FSY);

            }
            
            //根据congex返回的结果判断坐标，以及是否有
            GlobalManager.Current.BadFoamCount = 0;
            return 0;
        }

        public int PickFoam()
        {
            //要把这个替换成实际抓手取料的位置，只用移动一次
            AkrAction.Current.Move(AxisName.FSX, 10000, (int)AxisSpeed.FSX);
            AkrAction.Current.Move(AxisName.FSY, 10000, (int)AxisSpeed.FSY);

            if (GlobalManager.Current.UsePicker1)
            {

                AkrAction.Current.MoveNoWait(AxisName.PICK1_Z, 10000, (int)AxisSpeed.PICK1_Z);
                SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                AkrAction.Current.MoveNoWait(AxisName.PICK1_Z, 20000, (int)AxisSpeed.PICK1_Z);
                GlobalManager.Current.current_FOAM_Count++;
            }

            if (GlobalManager.Current.UsePicker2)
            {
                AkrAction.Current.MoveNoWait(AxisName.PICK2_Z, 10000, (int?)(int)AxisSpeed.PICK2_Z);
                SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
                AkrAction.Current.MoveNoWait(AxisName.PICK2_Z, 20000, (int?)(int)AxisSpeed.PICK2_Z);
                GlobalManager.Current.current_FOAM_Count++;
            }

            if (GlobalManager.Current.UsePicker3)
            {
                AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 10000, (int?)(int)AxisSpeed.PICK3_Z);
                SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
                AkrAction.Current.MoveNoWait(AxisName.PICK3_Z, 20000, (int?)(int)AxisSpeed.PICK3_Z);
                GlobalManager.Current.current_FOAM_Count++;
            }

            if (GlobalManager.Current.UsePicker4)
            {
                AkrAction.Current.MoveNoWait(AxisName.PICK4_Z, 10000, (int?)(int)AxisSpeed.PICK4_Z);
                SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
                AkrAction.Current.MoveNoWait(AxisName.PICK4_Z, 20000, (int?)(int)AxisSpeed.PICK4_Z);
                GlobalManager.Current.current_FOAM_Count++;
            }

            return 0;
        }

        public int LowerCCD()
        {
            
            ccd2SnapPath.Clear();
            foreach (var Point in GlobalManager.Current.feedarPoints)
            {
                PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition SendTLNCamreaposition = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition()
                {
                    SN = "asd",
                    NozzleID = "1",
                    RawMaterialName = "1",
                    CaveID = "0",
                    TargetMaterialName1 = "1",
                    Photo_X1 = Point.X.ToString(),
                    Photo_Y1 = Point.Y.ToString(),
                    Photo_R1 = Point.Y.ToString(),

                };
                ccd2SnapPath.Add(SendTLNCamreaposition);
            }

            //给Cognex发拍照信息
            Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNSendData(PrecisionDownCamreaProcessCommand.TLN, ccd2SnapPath);

            AkrAction.Current.Move(AxisName.FSX, 10000, (int)AxisSpeed.FSX);
            AkrAction.Current.Move(AxisName.FSY, 10000, (int)AxisSpeed.FSY);

            foreach (var Point in GlobalManager.Current.feedarPoints)
            {
                AkrAction.Current.SetSingleEvent(AxisName.FSX, (int)AxisSpeed.FSX, 1);
                AkrAction.Current.MoveNoWait(AxisName.FSX, (int)Point.X, (int)AxisSpeed.FSX);
                AkrAction.Current.MoveNoWait(AxisName.FSY, (int)Point.Y, (int)AxisSpeed.FSY);

            }

            return 0;
        }

        public int DropBadFoam()
        {
            if (GlobalManager.Current.picker1State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 30000, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 30000, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_8solenoid_valve1_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_9solenoid_valve1_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
            }
            if (GlobalManager.Current.picker2State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 40000, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 30000, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_10solenoid_valve2_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_11solenoid_valve2_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
            }
            if (GlobalManager.Current.picker3State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 50000, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 30000, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_12solenoid_valve3_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_13solenoid_valve3_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
            }
            if (GlobalManager.Current.picker4State == false)
            {
                AkrAction.Current.Move(AxisName.FSX, 60000, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, 30000, (int)AxisSpeed.FSY);

                SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 1);
                Thread.Sleep(20);
                SetIO(IO_OutFunction_Table.OUT3_14solenoid_valve4_A, 1);
                SetIO(IO_OutFunction_Table.OUT3_15solenoid_valve4_B, 0);
                Thread.Sleep(20);
                GlobalManager.Current.current_FOAM_Count--;
            }
            return 0;
        }

        public int SnapPallete()
        {
            palletePath.Clear();
            int count = 0;
            foreach(var Point in GlobalManager.Current.palletePoints)
            {
                
                AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition = new AssUpCamrea.Pushcommand.SendTLTCamreaposition()
                {
                    SN = "ASDASD",
                    NozzleID = "1",
                    MaterialTypeN1 = "0",
                    AcupointNumber = count.ToString(),
                    TargetMaterialName1 = "0",
                    Photo_X1 = Point.X.ToString(),
                    Photo_Y1 = Point.Y.ToString(),
                    Photo_R1 = "0"
                };
                count++;
            }

            foreach (var Point in GlobalManager.Current.feedarPoints)
            {
                AkrAction.Current.SetSingleEvent(AxisName.FSX, (int)AxisSpeed.FSX, 1);
                AkrAction.Current.MoveNoWait(AxisName.FSX, (int)Point.X, (int)AxisSpeed.FSX);
                AkrAction.Current.MoveNoWait(AxisName.FSY, (int)Point.Y, (int)AxisSpeed.FSY);

            }



            GlobalManager.Current.palleteSnaped = true;
            return 0;
        }

        public int PlaceFoam()
        {
            //这里要改成实际吸取了多少料
            GlobalManager.Current.current_FOAM_Count -= 4;

            GlobalManager.Current.current_Assembled += 4;

            return 0;
        }


        public bool Step1()
        {
            //测试用
            Debug.WriteLine("ZuZhuang.Current.Step1()");
            
            if (!BoradIn())
                return false;

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
            Debug.WriteLine("ZuZhuang.Current.Step3-1()");

            GlobalManager.Current.current_Zuzhuang_step = 3;

            //吸嘴取料
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            Debug.WriteLine("ZuZhuang.Current.Step3-2()");

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
            Console.WriteLine("ZuZhuang.Current.Step5()");

            GlobalManager.Current.current_Zuzhuang_step = 6;

            //拍Pallete料盘
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public bool Step7()
        {
            Console.WriteLine("ZuZhuang.Current.Step5()");

            GlobalManager.Current.current_Zuzhuang_step = 7;

            //拍Pallete料盘
            //WaitConveyor(0, null, GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public void Step1Test()
        {
            Thread.Sleep(5000);
            Debug.WriteLine("step1");
        }

        public void Step2Test()
        {
            Thread.Sleep(1000);
            Debug.WriteLine("step2");
        }
        public void Step3Test()
        {
            Thread.Sleep(1000);
            Debug.WriteLine("step3");
        }
        public void Step4Test()
        {
            Thread.Sleep(1000);
            Debug.WriteLine("step4");
        }
        public void Step5Test()
        {
            Thread.Sleep(1000);
            Debug.WriteLine("step5");
        }

        public async void test()
        {
            //有前机要板信号
            bool has_pre_info = true;
            bool is_First = false;
            int current_foam = 0;
            int current_assembled = 0;
            int total = 20;
            while (true)
            {
                Step1:
                    if (!has_pre_info) continue;                   
                    var task1 = Task.Run(() => Step1Test());
                    if (current_foam > 0) goto Step4;

                Step2:
                    Step2Test();

                Step3:
                    Step3Test();

                Step4:
                    Step4Test();

                Step5:
                    await task1;
                    Step5Test();
                    current_assembled += 4;
                    if (current_assembled < 20)
                    {
                        goto Step2;
                    }
            }

        }

        public async override void AutoRun()
        {
            try
            {

                while (true)
                {
                    step1:
                        if (!GlobalManager.Current.IO_test2 || board_count != 0)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        var task1 = Task.Run(() => Step1());
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
                        Step4();
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
                        await task1;
                        //拍料盘
                        if (!GlobalManager.Current.palleteSnaped) goto step7;
                        Step6();
                        if (GlobalManager.Current.Zuzhuang_exit) break;

                    step7:
                        //放料
                        Step7();
                        if (GlobalManager.Current.Zuzhuang_exit) break;
                        //当前组装的料小于穴位数时，要一直取料
                        if (GlobalManager.Current.current_Assembled < GlobalManager.Current.total_Assemble_Count)
                        {
                            goto step2;
                        }
                        else
                        {
                            BoardOut();
                        }
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
