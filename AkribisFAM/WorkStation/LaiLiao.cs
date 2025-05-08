using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using AkribisFAM.Manager;
using AkribisFAM.Windows;

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

        bool has_board = false;
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

        public void Wait(int delta)
        {
            WarningManager.Current.WaitLaiLiao(delta);
        }

        public bool BoradIn()
        {
            if (GlobalManager.Current.IO_test1 == true && has_board == false)
            {
                GlobalManager.Current.IO_test1 = false;
                has_board = true;
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
            has_board = false;
            GlobalManager.Current.IO_test2 = true;
        }

        public bool Step1()
        {
            Console.WriteLine("LaiLiao.Current.Step1()");

            if(!BoradIn()) 
                return false;

            //触发 UI 动画
            OnTriggerStep1?.Invoke();

            //用thread.sleep模拟实际生成动作
            Thread.Sleep(1000);

            delta = GlobalManager.Current.current_Lailiao_step1_state == true ? 0 : 999999;

            Wait(delta);

            //触发 UI 动画
            OnStopStep1?.Invoke();

            GlobalManager.Current.current_Lailiao_step = 1;

            return true;
        }

        public bool Step2()
        {
            Console.WriteLine("LaiLiao.Current.Step2()");

            //触发 UI 动画
            OnTriggerStep2?.Invoke();

            //用thread.sleep模拟实际生成动作
            Thread.Sleep(1000);

            GlobalManager.Current.current_Lailiao_step = 2;
            delta = GlobalManager.Current.current_Lailiao_step2_state == true ? 0 : 999999;

            Wait(delta);

            //触发 UI 动画
            OnStopStep2?.Invoke();

            return true;
        }

        public bool Step3()
        {
            Console.WriteLine("LaiLiao.Current.Step3()");

            //触发 UI 动画
            OnTriggerStep3?.Invoke();

            //用thread.sleep模拟实际生成动作
            Thread.Sleep(1000);

            delta = GlobalManager.Current.current_Lailiao_step3_state == true ? 0 : 999999;

            Wait(delta);

            //触发 UI 动画
            OnStopStep3?.Invoke();

            Boardout();

            GlobalManager.Current.current_Lailiao_step = 3;

            return true;
        }

        public override void AutoRun()
        {

            try
            {
                while (true)
                {

                    step1: if (!Step1()) continue;

                    step2: Step2();

                    step3: Step3();

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
