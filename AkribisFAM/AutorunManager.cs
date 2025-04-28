using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using AkribisFAM.WorkStation;

namespace AkribisFAM
{
    public class AutorunManager
    {
        private static AutorunManager _current;

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
            _loopWorker = new Worker(AutoRun);
        }

        Worker _loopWorker;


        public static bool CheckTaskReady()
        {
            Task<bool>[] TaskArray = new Task<bool>[1];

            TaskArray[0] = Task.Run(() => { return TestStation1.Current.Ready(); });
            
            Task.WaitAll(TaskArray);

            return true;
        }

        public void AutoRun()
        {
            if (!CheckTaskReady())
            {
                Console.WriteLine("Not Ready");
                return;
            }


            try
            {
                while (!_loopWorker.WaitStopSignal(300))
                {
                    if (!IsSafe())
                    {
                        continue;
                    }

                    Trace.WriteLine("Autorun Process");

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


    }
}
