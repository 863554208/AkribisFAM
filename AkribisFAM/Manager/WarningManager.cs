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

        public void WaitZuZhuang(int time)
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause) 
            {
                Console.WriteLine("执行暂停");
                time = 999999;
            }

            while (true)
            {
                if (!GlobalManager.Current.IsPause)
                {
                    break;
                }                
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = time - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);
            }

        }

        public void WaitLaiLiao(int time)
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause)
            {
                Console.WriteLine("执行暂停");
                time = 999999;
            }

            while (true)
            {
                if (!GlobalManager.Current.IsPause)
                {
                    break;
                }
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = time - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    break;
                }

                int sleepTime = (int)Math.Min(remaining, 50);
                Thread.Sleep(sleepTime);
            }

        }

        public void WaiFuJian(int time)
        {
            DateTime startTime = DateTime.Now;

            if (GlobalManager.Current.IsPause)
            {
                Console.WriteLine("执行暂停");
                time = 999999;
            }

            while (true)
            {
                if (!GlobalManager.Current.IsPause)
                {
                    break;
                }
                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = time - elapsed.TotalMilliseconds;

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
