using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.WorkStation;

namespace AkribisFAM.Manager
{
    public class WarningManager
    {
        private static WarningManager _instance;

        public static WarningManager Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new WarningManager();
                    }
                }
                return _instance;
            }
        }

        public void WaitZuZhuang()
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause) 
            {
                Console.WriteLine("执行暂停");
                GlobalManager.Current.Zuzhuang_delta[GlobalManager.Current.current_Zuzhuang_step] = 999999;
            }

            while (true)
            {              
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = GlobalManager.Current.Zuzhuang_delta[GlobalManager.Current.current_Zuzhuang_step] - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);


            }

        }

        public void WaitLaiLiao()
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause)
            {
                Console.WriteLine("执行暂停");
                GlobalManager.Current.Lailiao_delta[GlobalManager.Current.current_Lailiao_step] = 999999;
            }

            while (true)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = GlobalManager.Current.Lailiao_delta[GlobalManager.Current.current_Lailiao_step] - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);
            }

        }

        public void WaiFuJian()
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause)
            {
                Console.WriteLine("执行暂停");
                GlobalManager.Current.FuJian_delta[GlobalManager.Current.current_FuJian_step] = 999999;
            }

            while (true)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = GlobalManager.Current.FuJian_delta[GlobalManager.Current.current_FuJian_step] - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);
            }

        }

    }
}
