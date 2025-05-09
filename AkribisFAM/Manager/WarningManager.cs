using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.WorkStation;
using static AkribisFAM.GlobalManager;

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

        public int WaitIO(int[] IOarr, int size)
        {
            int timeout = 30000; //30s
            DateTime startTime = DateTime.Now;

            int cnt = 0;

            while (true)
            {
                cnt++;
                if (cnt == 300) {
                    GlobalManager.Current.lailiaoIO[(int)Input.LaiLiao_JianSu] = 1;
                }
                int judge = 0;
                for (int i = 0; i < size; ++i)
                {
                    judge += GlobalManager.Current.lailiaoIO[IOarr[i]];
                }
                if(judge == size)
                {
                    return 0;
                }

                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = timeout - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    return 1;
                }

                Thread.Sleep(10);
            }

        }

        public int WaitMessage(string sendmessage)
        {
            int timeout = 30000; //30s
            DateTime startTime = DateTime.Now;

            int cnt = 0;
            while (true)
            {
                //if(sendMessage(sendmessage) == 1)
                //{
                //    return 0;
                //}
                cnt++;
                if (cnt == 300)
                {
                    return 0;
                }

                TimeSpan elapsed = DateTime.Now - startTime;
                double remaining = timeout - elapsed.TotalMilliseconds;

                if (remaining <= 0)
                {
                    return 1;
                }

                Thread.Sleep(10);
            }
        }
    }
}
