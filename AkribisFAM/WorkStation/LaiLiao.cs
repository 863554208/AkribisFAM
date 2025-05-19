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
using static AkribisFAM.CommunicationProtocol.KEYENCEDistance.Acceptcommand;

namespace AkribisFAM.WorkStation
{
    internal class LaiLiao : WorkStationBase
    {

        public enum LailiaoStep
        {
            Step1,
            Step2,
            Step3,
            Complete
        }
        private static LaiLiao _instance;
        public override string Name => nameof(LaiLiao);

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;

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

        public override void ReturnZero()
        {
            throw new NotImplementedException();
        }


        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }

        public override bool Ready()
        {
            return true;
        }

        public bool ReadIO(IO_INFunction_Table index)
        {
            return IOManager.Instance.INIO_status[(int)index];

        }

        public void SetIO(IO_OutFunction_Table index , int value)
        {
            IOManager.Instance.IO_ControlStatus( index , value);
        }

        public int ScanBarcode()
        {
            GlobalManager.Current.IsByPass = false;
            //触发扫码枪扫码

            GlobalManager.Current.IsByPass = true;
            return 0;
        }

        public int LaserHeight()
        {
            //激光测距
            foreach (var Point in GlobalManager.Current.laserPoints)
            {
                //移动
                AkrAction.Current.MoveNoWait(AxisName.LSX, (int)Point.X * 200 ,(int)AxisSpeed.LSX );
                AkrAction.Current.Move(AxisName.LSY, (int)Point.Y * 200, (int)AxisSpeed.LSY );

                //触发测距
                //sendKDistanceAppend.Clear();
                //KEYENCEDistance.Pushcommand.SendKDistanceAppend temp = new KEYENCEDistance.Pushcommand.SendKDistanceAppend()
                //{
                //    TestNumber = "1",
                //    address = "0",
                //};
                //sendKDistanceAppend.Add(temp);
                //string temp = "1,0";
                Task_KEYENCEDistance.SendMSData(Task_KEYENCEDistance.KEYENCEDistanceProcessCommand.MS, "1,0");

                //得到测量结果
                AcceptKDistanceAppend = Task_KEYENCEDistance.AcceptMSData(Task_KEYENCEDistance.KEYENCEDistanceProcessCommand.MS);
                var res = AcceptKDistanceAppend[0].MeasurData;
                GlobalManager.Current.currentLasered++;

                Thread.Sleep(50);
            }

            return 0;
        }

        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveConveyor(vel);
        }

        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
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

        public void WaitConveyor(int type)
        {
            DateTime time = DateTime.Now;
            bool ret = false;
            switch (type)
            {
                case 2: 
                    while (ScanBarcode() == 1);
                    break;

                case 3:
                    while(LaserHeight() ==1); 
                    break;
            }
        }

        public void ResumeConveyor()
        {
            if (GlobalManager.Current.station2_IsBoardInLowSpeed || GlobalManager.Current.station3_IsBoardInLowSpeed || GlobalManager.Current.station4_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(100);
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

            if (ReadIO(IO_INFunction_Table.IN7_0BOARD_AVAILABLE) && board_count == 0)
            {
                StateManager.Current.TotalInput++;
                Set("station1_IsBoardInHighSpeed", true);

                //将要板信号清空
                SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);

                //传送带高速移动
                MoveConveyor((int)AxisSpeed.BL1);

                //等待减速光电1
                if(!WaitIO(999999, IO_INFunction_Table.IN1_0Slowdown_Sign1 ,true)) throw new Exception();

                //阻挡气缸1上气
                SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, 1);
                SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, 0);

                //标志位转换
                Set("station1_IsBoardInHighSpeed", false);
                Set("station1_IsBoardInLowSpeed", true);

                //传送带低速运动
                MoveConveyor(100);

                //等待料盘挡停到位信号1
                if (!WaitIO(999999, IO_INFunction_Table.IN1_4Stop_Sign1, true)) throw new Exception();

                //停止皮带移动，直到该工位顶升完成，才能继续移动皮带
                Set("station1_IsBoardInLowSpeed", false);
                Set("station1_IsBoardIn", false);
                Set("station1_IsLifting", true);
                
                StopConveyor();

                //执行测距位顶升气缸顶升                

                SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 0);
                SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 0);
                
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

        public void Boardout()
        {
            Set("station1_IsBoardOut", true);

            //模拟给下一个工位发进板信号
            GlobalManager.Current.IO_test2 = true;
            if (GlobalManager.Current.IsByPass)
            {
                GlobalManager.Current.SendByPassToStation2 = true;
            }

            //如果后续工站正在执行出站，就不要让该工位的气缸放气和下降
            while (GlobalManager.Current.station2_IsBoardOut || GlobalManager.Current.station3_IsBoardOut || GlobalManager.Current.station4_IsBoardOut)
            {
                Thread.Sleep(100);
            }

            //执行气缸放气，下降
            StopConveyor();
            SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, 0);
            SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, 1);

            ResumeConveyor();

            Set("station1_IsBoardOut", false);
            //SetIO(IO.LaiLiao_BoardOut ,true);
            board_count -= 1;
            
        }

        public void CheckState()
        {
            GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
            GlobalManager.Current.Lailiao_CheckState();
            WarningManager.Current.WaitLaiLiao();
        }

        public bool Step1()
        {            
            Debug.WriteLine("LaiLiao.Current.Step1()");

            //进板
            if (!BoradIn())
                return false;

            GlobalManager.Current.currentLasered = 0;

            Thread.Sleep(500);

            CheckState();

            GlobalManager.Current.current_Lailiao_step = 1;

            return true;
        }

        public bool Step2()
        {
            Console.WriteLine("LaiLiao.Current.Step2()");

            GlobalManager.Current.current_Lailiao_step = 2;

            //扫码
            WaitConveyor(GlobalManager.Current.current_Lailiao_step);

            CheckState();

            return true;
        }

        public bool Step3()
        {
            Console.WriteLine("LaiLiao.Current.Step3()");

            GlobalManager.Current.current_Lailiao_step = 3;

            //激光测距
            WaitConveyor(GlobalManager.Current.current_Lailiao_step);

            CheckState();

            return true;
        }


        public override void AutoRun()
        {

            try
            {
                while (true)
                {

                    step1: bool ret = Step1();
                        if (GlobalManager.Current.Lailiao_exit) break;
                        if (!ret) continue;

                    step2: Step2();
                        if (GlobalManager.Current.Lailiao_exit) break;
                        if (GlobalManager.Current.IsByPass) goto step4;

                    step3: Step3();
                        if (GlobalManager.Current.Lailiao_exit) break;
                        //if (GlobalManager.Current.currentLasered < GlobalManager.Current.TotalLaserCount)
                    //出板
                    step4:
                        Boardout();

                    #region 老代码
                    //if (GlobalManager.Current.lailiao_ChuFaJinBan)
                    //{
                    //    //TODO 执行进板
                    //    GlobalManager.Current.lailiao_ChuFaJinBan = false;


                    //    WorkState = 1;
                    //    has_board = true;
                    //    Console.WriteLine("检测到进板信号，已进板");
                    //    GlobalManager.Current.lailiao_JinBanWanCheng = true;
                    //}

                    //// 处理板
                    //if (has_board && WorkState == 1)
                    //{
                    //    try
                    //    {
                    //        //执行完才能改变workstatiion
                    //        WorkState = 2;

                    //        //TODO 扫码枪扫码
                    //        System.Threading.Thread.Sleep(1000);
                    //        OnJinBanExecuted?.Invoke();
                    //        GlobalManager.Current.lailiao_SaoMa = true;
                    //        Console.WriteLine("扫码枪扫码已完成");

                    //        bool asd = false;
                    //        //TODO 上传条码，等待HIVE返回该板是否组装的指令
                    //        if (asd)
                    //        {
                    //            GlobalManager.Current.hive_Result = false;
                    //        }
                    //        else
                    //        {
                    //            //TODO 基恩士激光测距
                    //            System.Threading.Thread.Sleep(1000);
                    //            GlobalManager.Current.lailiao_JiGuangCeJu = true;
                    //            OnLaserExecuted.Invoke();
                    //            Console.WriteLine("激光测距已完成");
                    //        }

                    //        WorkState = 3; // 更新状态为出板
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        has_error = true; // 标记为出错
                    //        Console.WriteLine($"处理过程中发生异常: {ex.Message}");
                    //    }
                    //}

                    //// 出板
                    //if (WorkState == 3 || has_error)
                    //{
                    //    if (has_error)
                    //    {
                    //        AutorunManager.Current.isRunning = false;
                    //    }
                    //    System.Threading.Thread.Sleep(1000);
                    //    OnMovePalleteExecuted.Invoke();
                    //    WorkState = 0;
                    //    has_board = false;
                    //    Console.WriteLine("来料工站所有工序完成，流至下一工站");
                    //    GlobalManager.Current.IO_test2 = true;
                    //}

                    #endregion

                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }
    }
}
