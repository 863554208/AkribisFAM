using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using AkribisFAM.Manager;
using AkribisFAM.Windows;
using static AkribisFAM.GlobalManager;

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

        public override bool Ready()
        {
            return true;
        }

        public bool ReadIO(IO index)
        {
            return GlobalManager.Current.laiLiaoIO[(int)index];
        }

        public void SetIO(IO index ,bool value)
        {
            GlobalManager.Current.laiLiaoIO[(int)index] = value;
        }

        public int ScanBarcode()
        {
            //扫码
            Thread.Sleep(100);
            return 0;
        }

        public int LaserHeight()
        {
            //扫码
            Thread.Sleep(100);
            return 0;
        }

        public void MoveConveyor(int vel)
        {
            //TODO 移动传送带
        }

        public void StopConveyor()
        {
            //TODO 停止传送带
        }

        public int[] ToIntegerArray(params bool[] boolValues)
        {
            return boolValues.Select(b => b ? 1 : 0).ToArray();
        }

        public void WaitConveyor(int delta, int[] IOarr , int type)
        {
            DateTime time = DateTime.Now;

            if (delta != 0 && IOarr != null)
            {
                while ((DateTime.Now - time).TotalMilliseconds < delta)
                {
                    int judge = 0;
                    foreach (int item in IOarr)
                    {
                        judge += item;
                    }

                    if (judge > 0) 
                    {
                        break;
                    }
                    Thread.Sleep(50);
                }
            }
            else
            {
                switch (type)
                {
                    case 2: 
                        while (ScanBarcode() == 1) ;
                        break;

                    case 4:
                        while(LaserHeight() ==1); 
                        break;
                }
            }
        }


        public bool BoradIn()
        {
            if (ReadIO(IO.LaiLiao_BoardIn) && board_count == 0)
            {                
                //传送带高速移动
                MoveConveyor(200);

                var IOArray = ToIntegerArray(ReadIO(IO.LaiLiao_JianSu));
                WaitConveyor(9999, IOArray, 0);

                //顶板气缸上气
                SetIO(IO.LaiLiao_QiGang ,true);

                //传送带减速
                MoveConveyor(100);

                //TODO 这边有没有告诉已经到位的IO信号？
                StopConveyor();

                //实际生产时要把这行注释掉，进板IO信号不是我们软件给
                SetIO(IO.LaiLiao_BoardIn ,false);

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
            //WaitConveyor((int)Input.LaiLiao_BoardOut);
            SetIO(IO.LaiLiao_BoardOut ,true);
            board_count -= 1;
            //模拟给下一个工位发进板信号
            GlobalManager.Current.IO_test2 = true;
        }

        public void CheckState()
        {
            GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
            GlobalManager.Current.Lailiao_CheckState();
            WarningManager.Current.WaitLaiLiao();
        }

        public bool Step1()
        {            
            Console.WriteLine("LaiLiao.Current.Step1()");

            //进板
            if (!BoradIn()) 
                return false;

            //触发 UI 动画
            OnTriggerStep1?.Invoke();

            Thread.Sleep(1000);

            CheckState();

            GlobalManager.Current.current_Lailiao_step = 1;

            //触发 UI 动画
            OnStopStep1?.Invoke();

            return true;
        }

        public bool Step2()
        {
            Console.WriteLine("LaiLiao.Current.Step2()");

            GlobalManager.Current.current_Lailiao_step = 2;

            //触发 UI 动画
            OnTriggerStep2?.Invoke();

            //扫码
            WaitConveyor( 0,null, GlobalManager.Current.current_Lailiao_step);

            //触发 UI 动画
            OnStopStep2?.Invoke();

            CheckState();

            return true;
        }

        public bool Step3()
        {
            Console.WriteLine("LaiLiao.Current.Step3()");

            GlobalManager.Current.current_Lailiao_step = 3;

            var IOArray = ToIntegerArray(ReadIO(IO.LaiLiao_DingSheng));

            //顶升
            WaitConveyor(9999, IOArray, GlobalManager.Current.current_Lailiao_step);

            CheckState();

            return true;
        }

        public bool Step4()
        {
            Console.WriteLine("LaiLiao.Current.Step4()");

            GlobalManager.Current.current_Lailiao_step = 4;

            //触发 UI 动画
            OnTriggerStep3?.Invoke();

            //激光测距
            WaitConveyor(0, null, GlobalManager.Current.current_Lailiao_step);

            CheckState();

            //触发 UI 动画
            OnStopStep3?.Invoke();

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

                    step3: Step3();
                        if (GlobalManager.Current.Lailiao_exit) break;

                    step4: Step4();
                        if (GlobalManager.Current.Lailiao_exit) break;

                    //出板
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
