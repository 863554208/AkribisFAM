//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using AAMotion;
//using AkribisFAM.Manager;
//namespace AkribisFAM.WorkStation
//{
//    internal class TestStation2 :WorkStationBase
//    {
//        private static TestStation2 _instance;
//        public override string Name => nameof(TestStation2);
//        public static TestStation2 Current
//        {
//            get
//            {
//                if (_instance == null)
//                {
//                    if (_instance == null)
//                    {
//                        _instance = new TestStation2();
//                    }
//                }
//                return _instance;
//            }
//        }



//        public override void ReturnZero()
//        {
//            throw new NotImplementedException();
//        }


//        public override void Initialize()
//        {
//            throw new NotImplementedException();
//        }

//        public override bool Ready()
//        {
//            return true;
//        }

//        public override void AutoRun()
//        {

//            int WorkState = 11;
//            try
//            {
//                TaskContextManager.SetContext(new TaskContext
//                {
//                    StationName = Name,
//                    StartTime = DateTime.Now,
//                    TaskId = "AutoRun_Task_2"  // 给任务一个唯一标识
//                });


//                // 开始一个工序流程
//                bool processCompleted = false;

//                while (!processCompleted)
//                {
//                    switch (WorkState)
//                    {
//                        case 11: // 上料
//                            if (GlobalManager.Current.IsAInTarget)
//                            {
//                                LoadMaterial();
//                                WorkState = 20; // 切到贴装
//                            }
//                            break;

//                        case 20: // 贴装
//                            if (GlobalManager.Current.IsAInTarget)
//                            {
//                                AttachPart();
//                                WorkState = 11; // 切到检测
//                            }
//                            break;


//                        default:
//                            Console.WriteLine($"Unknown WorkState: {WorkState}. Reset to 11.");
//                            WorkState = 11;
//                            processCompleted = true; // 出错也退出
//                            break;
//                    }
//                    System.Threading.Thread.Sleep(300);
//                }
//            }
//            catch (Exception ex) { }


//        }

//        private void LoadMaterial()
//        {
//            GlobalManager.Current.IsAInTarget = false;

//            GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).ClearBuffer();
//            GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, 50000, -300000, null, null, 100000, 20000);
//            GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Begin();

//            while (GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.C).InTargetStat != 4 && GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.D).InTargetStat != 4)
//            {
//                if (GlobalManager.Current.IsPause)
//                {
//                    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Pause();
//                    Thread.Sleep(10);
//                }
//                else
//                {
//                    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Resume();
//                    Thread.Sleep(10);
//                }
//                //Console.WriteLine("当前轴B运动状态1 " + GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat);
//                System.Threading.Thread.Sleep(10);
//            }

//            GlobalManager.Current.IsBInTarget = true;
//            //Console.WriteLine("LoadMaterial 2");
//        }

//        private void AttachPart()
//        {
//            GlobalManager.Current.IsAInTarget = false;

//            GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).ClearBuffer();
//            GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, -50000, 50000, null, null, 100000, 20000);
//            GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Begin();

//            while (GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.C).InTargetStat != 4 && GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.D).InTargetStat != 4)
//            {
//                if (GlobalManager.Current.IsPause)
//                {
//                    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Pause();
//                    Thread.Sleep(10);
//                }
//                else
//                {
//                    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Resume();
//                    Thread.Sleep(10);
//                }
//                //Console.WriteLine("当前轴B运动状态2 " + GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat);
//                System.Threading.Thread.Sleep(10);
//            }
//            GlobalManager.Current.IsBInTarget = true;
//            //Console.WriteLine("AttachPart 2");
//        }

//    }
//}
