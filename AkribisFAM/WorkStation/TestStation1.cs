using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using AAMotion;
using AkribisFAM.Manager;
using static AAComm.Extensions.AACommFwInfo;

namespace AkribisFAM.WorkStation
{
    public class TestStation1 :WorkStationBase
    {
        private static TestStation1 _instance;
        public override string Name => nameof(TestStation1);

        public static TestStation1 Current
        {
            get
            {
                if (_instance == null)
                {
                        if (_instance == null)
                        {
                            _instance = new TestStation1();
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

            int WorkState = 11;
            try
            {
                TaskContextManager.SetContext(new TaskContext
                {
                    StationName = Name,
                    StartTime = DateTime.Now,
                    TaskId = "AutoRun_Task_1"  // 给任务一个唯一标识
                });


                // 开始一个工序流程
                bool processCompleted = false;
                bool start = true;

                //bool test = true;

                while (!processCompleted)
                {
                    //if (test)
                    //{
                    //    test = false;
                        
                    //}

                    switch (WorkState)
                    {
                        case 11: // 上料
                            if (GlobalManager.Current.IsBInTarget || start)
                            {
                                if (start)
                                {
                                    start = false;
                                    Console.WriteLine("A轴开始执行的一次");
                                }
                                LoadMaterial();
                                WorkState = 20; // 切到贴装
                            }
                            break;

                        case 20: // 贴装
                            if (GlobalManager.Current.IsBInTarget)
                            {
                                //throw new NotImplementedException();

                                AttachPart();
                                WorkState = 11; // 切到检测
                            }
                            break;


                        default:
                            Console.WriteLine($"Unknown WorkState: {WorkState}. Reset to 11.");
                            WorkState = 11;
                            processCompleted = true; // 出错也退出
                            break;
                    }
                    System.Threading.Thread.Sleep(300);
                }
            }
            catch (Exception ex) 
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }

        private void LoadMaterial()
        {
            GlobalManager.Current.IsBInTarget = false;

            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).ClearBuffer();
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).LinearAbsoluteXY(50000, -200000, 100000, 20000);
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Begin();

            //AAMotionAPI.LinearAbsoluteXY(GlobalManager.Current._Agm800.controller, 50000, -200000, 100000, 20000);

            //GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Begin();

            while (GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).InTargetStat != 4)
            {
                if (GlobalManager.Current.IsPause)
                {
                    GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Pause();
                    Thread.Sleep(10);
                }
                else
                {
                    GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Resume();
                    Thread.Sleep(10);
                }
                //Console.WriteLine("当前轴A运动状态1 " + GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat);
                System.Threading.Thread.Sleep(10);
            }

            GlobalManager.Current.IsAInTarget = true;
            //Console.WriteLine("loadMaterial 1");
        }

        private void AttachPart()
        {
            GlobalManager.Current.IsBInTarget = false;

            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).ClearBuffer();
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).LinearAbsoluteXY(-200000, 50000, 100000, 20000);
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Begin();

            while (GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).InTargetStat != 4)
            {
                if (GlobalManager.Current.IsPause)
                {
                    GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Pause();
                    Thread.Sleep(10);
                }
                else
                {
                    GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Resume();
                    Thread.Sleep(10);
                }
                //Console.WriteLine("当前轴A运动状态2 " + GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat);
                System.Threading.Thread.Sleep(10);
            }

            GlobalManager.Current.IsAInTarget = true;
            //Console.WriteLine("attachpart 1");
        }



    }
}
