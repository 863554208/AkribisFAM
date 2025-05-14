using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using AAMotion;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using AkribisFAM.CommunicationProtocol;

namespace AkribisFAM
{
    public class AutorunManager
    {
        private static AutorunManager _current;
        public bool isRunning;
        public bool hasReseted;
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
            hasReseted = false;
        }

        Worker _loopWorker;


        public static bool CheckTaskReady()
        {
            //Task<bool>[] TaskArray = new Task<bool>[1];

            //TaskArray[0] = Task.Run(() => { return TestStation1.Current.Ready(); });
            
            //Task.WaitAll(TaskArray);

            return true;
        }

        public async void AutoRunMain()
        {
            if (!CheckTaskReady())
            {
                Console.WriteLine("Not Ready");
                return;
            }
            if (!hasReseted)
            {
                MessageBox.Show("Please Reset!"); ;
                return;
            }
            isRunning = true;

            try
            {
                Debug.WriteLine("Autorun Process");

                try
                {
                        
                    List<Task> tasks = new List<Task>();

                    //tasks.Add(Task.Run(() => RunAutoStation(TestStation1.Current)));
                    //tasks.Add(Task.Run(() => RunAutoStation(TestStation2.Current)));

                    tasks.Add(Task.Run(() => RunAutoStation(LaiLiao.Current)));
                    tasks.Add(Task.Run(() => RunAutoStation(ZuZhuang.Current)));
                    tasks.Add(Task.Run(() => RunAutoStation(FuJian.Current)));
                    tasks.Add(Task.Run(() => RunAutoStation(Reject.Current)));
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
            GlobalManager.Current.Lailiao_exit = true;
            GlobalManager.Current.Zuzhuang_exit = true;
            GlobalManager.Current.FuJian_exit = true;
            isRunning = false;
            hasReseted = false;
            GlobalManager.Current.IO_test1 = false;
            GlobalManager.Current.IO_test2 = false;
            GlobalManager.Current.IO_test3 = false;
        }

        public void CylinderDown()
        {
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);
        }

        public bool Reset()
        {
            //先把气缸下降
            CylinderDown();

            //轴使能
            AkrAction.Current.axisAllEnable(true);

            //轴回原点
            AkrAction.Current.axisAllHome("D:\\akribisfam_config\\HomeFile");

            //传送带启动

            //传送带停止

            //飞达复位

            //激光测距复位

            //相机复位

            GlobalManager.Current.current_Lailiao_step = 0;
            GlobalManager.Current.current_Zuzhuang_step = 0;
            GlobalManager.Current.current_FuJian_step = 0;
            LaiLiao.Current.board_count = 0;

            GlobalManager.Current.Lailiao_exit = false;
            GlobalManager.Current.Zuzhuang_exit = false;
            GlobalManager.Current.FuJian_exit = false;

            return true;
        }
    }
}
