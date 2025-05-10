using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.Manager;
using LiveCharts.SeriesAlgorithms;
using YamlDotNet.Core;
using HslCommunication;

namespace AkribisFAM.WorkStation
{
    internal class Reject : WorkStationBase
    {

        private static Reject _instance;
        public override string Name => nameof(Reject);

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;

        int delta = 0;
        public bool has_board = false;

        public static Reject Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new Reject();
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
            if (GlobalManager.Current.IO_test4 == true && !has_board)
            {
                GlobalManager.Current.IO_test4 = false;
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

        public void CheckState()
        {
            GlobalManager.Current.Reject_state[GlobalManager.Current.current_Lailiao_step] = 0;
            GlobalManager.Current.Lailiao_CheckState();
            WarningManager.Current.WaitLaiLiao();
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

        public override void AutoRun()
        {

            try
            {
                while (true)
                {
                    step1:
                        bool ret = Step1();
                        if (GlobalManager.Current.Reject_exit) break;
                        if (!ret) continue;

                    step2:
                        Step2();
                        if (GlobalManager.Current.Reject_exit) break;

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
