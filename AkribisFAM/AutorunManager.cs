using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using AAMotion;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;

namespace AkribisFAM
{
    public class AutorunManager
    {
        private static AutorunManager _current;
        public bool isRunning;

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

        public async void AutoRunMain()
        {
            if (!CheckTaskReady())
            {
                Console.WriteLine("Not Ready");
                return;
            }

            isRunning = true;

            try
            {
                Trace.WriteLine("Autorun Process");

                try
                {
                        
                    List<Task> tasks = new List<Task>();

                    //tasks.Add(Task.Run(() => RunAutoStation(TestStation1.Current)));
                    //tasks.Add(Task.Run(() => RunAutoStation(TestStation2.Current)));

                    tasks.Add(Task.Run(() => RunAutoStation(LaiLiao.Current)));
                    tasks.Add(Task.Run(() => RunAutoStation(ZuZhuang.Current)));
                    tasks.Add(Task.Run(() => RunAutoStation(FuJian.Current)));
                    //tasks.Add(Task.Run(() => RunAutoStation(TestStation3)));

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex) 
                { 
                
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

        private void RunAutoStation(WorkStationBase station)
        {
            try
            {

                while (isRunning)
                {
                    if (!IsSafe())
                    {
                        continue;
                    }

                    station.AutoRun(); 

                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                ErrorReportManager.Report(ex);
            }
        }

        // 退出AutoRun
        public void StopAutoRun()
        {
            isRunning = false;
        }

    }
}
