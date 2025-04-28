using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using AkribisFAM.WorkStation;

namespace AkribisFAM
{
    public class AutorunManager
    {
        private static AutorunManager _current;
        private bool isRunning;

        public static AutorunManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AutorunManager();
                }
                return _current;
            }
        }

        public AutorunManager()
        {
            _loopWorker = new Worker(() => AutoRunMain());
        }

        Worker _loopWorker;


        public static bool CheckTaskReady()
        {
            Task<bool>[] TaskArray = new Task<bool>[1];

            TaskArray[0] = Task.Run(() => { return TestStation1.Current.Ready(); });
            
            Task.WaitAll(TaskArray);

            return true;
        }

        public void AutoRunMain()
        {
            if (!CheckTaskReady())
            {
                Console.WriteLine("Not Ready");
                return;
            }

            isRunning = true;

            try
            {
                while (!_loopWorker.WaitStopSignal(300))
                {
                    if (!IsSafe())
                    {
                        continue;
                    }

                    if (!isRunning) 
                    {
                        Console.WriteLine("退出自动运行");
                        break;
                    }


                    Trace.WriteLine("Autorun Process");

                    TestStation1.Current.AutoRun();

                }

            }
            catch (Exception ex) 
            {
                Trace.WriteLine("Error Process");

            }
            finally
            {
                Trace.WriteLine("Final Process");
            }

        }

        private bool IsSafe()
        {
            return true;
        }

        // 退出AutoRun
        public void StopAutoRun()
        {
            isRunning = false;  // 设为 false，停止 AutoRunMain 循环
        }

    }
}
