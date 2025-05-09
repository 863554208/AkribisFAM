using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.Manager;
using LiveCharts.SeriesAlgorithms;
using YamlDotNet.Core;

namespace AkribisFAM.WorkStation
{
    internal class FuJian : WorkStationBase
    {

        private static FuJian _instance;
        public override string Name => nameof(FuJian);

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;

        int delta = 0;
        public bool has_board = false;

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

        public bool BoardIn()
        {
            if (GlobalManager.Current.IO_test3 == true && !has_board)
            {
                GlobalManager.Current.IO_test3 = false;
                has_board = true;
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
            has_board = false;
        }

        public bool Step1()
        {
            if (!BoardIn()) return false;

            Console.WriteLine("Fujian step1");
            //触发 UI 动画
            OnTriggerStep1?.Invoke();
            //用thread.sleep模拟实际生成动作
            System.Threading.Thread.Sleep(1000);

            GlobalManager.Current.current_FuJian_step = 1;
            GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
            GlobalManager.Current.FuJian_CheckState();
            WarningManager.Current.WaiFuJian();
            //触发 UI 动画
            OnStopStep1?.Invoke();
            //ErrorManager.Current.Insert(ErrorCode.AGM800Disconnect);
            return true;
        }

        public bool Step2()
        {
            Console.WriteLine("step2");

            //触发 UI 动画
            OnTriggerStep2?.Invoke();
            
            //用thread.sleep模拟实际生成动作
            System.Threading.Thread.Sleep(1000);

            GlobalManager.Current.current_FuJian_step = 2;
            GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
            GlobalManager.Current.FuJian_CheckState();
            WarningManager.Current.WaiFuJian();
            //触发 UI 动画
            OnStopStep2?.Invoke();


            return true;
        }
        public bool Step3()
        {
            Console.WriteLine("step3");
            //触发 UI 动画
            OnTriggerStep3?.Invoke();
            //用thread.sleep模拟实际生成动作
            System.Threading.Thread.Sleep(1000);

            GlobalManager.Current.current_FuJian_step = 3;
            GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
            GlobalManager.Current.FuJian_CheckState();
            WarningManager.Current.WaiFuJian();
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
                step1:
                    bool ret = Step1();
                    if (GlobalManager.Current.FuJian_exit)
                    {
                        break;
                    }
                    if (!ret) continue;
                step2:
                    Step2();
                    if (GlobalManager.Current.FuJian_exit)
                    {
                        break;
                    }
                step3:
                    Step3();
                    if (GlobalManager.Current.FuJian_exit)
                    {
                        break;
                    }
                    BoardOut();
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
