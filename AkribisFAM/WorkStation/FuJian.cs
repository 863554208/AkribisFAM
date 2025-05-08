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

        public override void AutoRun()
        {
            int delta = 0;
            bool has_board = false;
            GlobalManager.Current.IO_test3 = false;
            try
            {
                while (true)
                {
                    goto step1;

                    step1:
                        if (GlobalManager.Current.IO_test3 == true) 
                        {
                            GlobalManager.Current.IO_test3 = false;
                        }
                        else
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        Console.WriteLine("Fujian step1");
                        OnTriggerStep1?.Invoke();

                        System.Threading.Thread.Sleep(1000);

                        GlobalManager.Current.current_FuJian_step = 1;
                        GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
                        GlobalManager.Current.FuJian_CheckState();
                        WarningManager.Current.WaiFuJian();
                        OnStopStep1?.Invoke();

                    step2:
                        Console.WriteLine("step2");
                        OnTriggerStep2?.Invoke();
                        System.Threading.Thread.Sleep(2000);

                        GlobalManager.Current.current_FuJian_step = 2;
                        GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
                        GlobalManager.Current.FuJian_CheckState();
                        WarningManager.Current.WaiFuJian();

                        OnStopStep2?.Invoke();

                    step3:
                        Console.WriteLine("step3");
                        OnTriggerStep3?.Invoke();
                        System.Threading.Thread.Sleep(1000);

                        GlobalManager.Current.current_FuJian_step = 3;
                        GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
                        GlobalManager.Current.FuJian_CheckState();
                        WarningManager.Current.WaiFuJian();
                        OnStopStep3?.Invoke();
                        

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
