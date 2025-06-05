using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Util;
using AkribisFAM.Windows;

namespace AkribisFAM
{
    public class AutorunManager
    {
        private static AutorunManager _current;
        public bool isRunning;
        public bool hasReseted;
        Thread ThreadLaser;
        Thread ThreadFoamAssembly;
        Thread ThreadRecheck;
        Thread ThreadConveyor;
        Thread ThreadFeeder;
        Thread ThreadTest1;
        Thread ThreadTest2;
        public static CancellationTokenSource CancelToken = new CancellationTokenSource(); // Exit thread
        public bool IsPause { get; set; } = false;
        public bool IsReset { get; set; } = false;
        public bool IsError { get; set; } = false;


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
            //_loopWorker = new Worker(() => AutoRunMain());

            Initialize();
            hasReseted = false;
        }

        public bool Initialize()
        {
            try
            {

                if (ThreadConveyor == null /*|| !ThreadConveyor.IsAlive*/)
                {
                    ThreadConveyor = new Thread(() => RunAutoStation(Conveyor.Current, CancelToken.Token)) { Name = "Conveyor", Priority = ThreadPriority.Highest };
                    ThreadConveyor.IsBackground = true;
                    //ThreadConveyor.Start();

                }
                if (ThreadLaser == null /*|| !ThreadLaser.IsAlive*/)
                {

                    ThreadLaser = new Thread(() => RunAutoStation(LaiLiao.Current, CancelToken.Token)) { Name = "Laser", Priority = ThreadPriority.Highest };
                    ThreadLaser.IsBackground = true;
                    //ThreadConveyor.Start();
                }
                if (ThreadFoamAssembly == null /*|| !ThreadFoamAssembly.IsAlive*/)
                {
                    ThreadFoamAssembly = new Thread(() => RunAutoStation(ZuZhuang.Current, CancelToken.Token)) { Name = "Foam Assembly", Priority = ThreadPriority.Highest };
                    ThreadFoamAssembly.IsBackground = true;
                    //ThreadFoamAssembly.Start();

                }

                if (ThreadRecheck == null/* || !ThreadRecheck.IsAlive*/)
                {

                    ThreadRecheck = new Thread(() => RunAutoStation(FuJian.Current, CancelToken.Token)) { Name = "Recheck", Priority = ThreadPriority.Highest };
                    ThreadRecheck.IsBackground = true;
                    //ThreadRecheck.Start();
                }

                if (ThreadFeeder == null /*|| !ThreadFeeder.IsAlive*/)
                {
                    ThreadFeeder = new Thread(() => RunAutoStation(Feeder.Current, CancelToken.Token)) { Name = "Feeder", Priority = ThreadPriority.Highest };
                    ThreadFeeder.IsBackground = true;
                    //ThreadFeeder.Start();

                }

               
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool CheckTaskReady()
        {
            //Task<bool>[] TaskArray = new Task<bool>[1];

            //TaskArray[0] = Task.Run(() => { return TestStation1.Current.Ready(); });

            //Task.WaitAll(TaskArray);

            return true;
        }

        public bool ReadIO(IO_INFunction_Table index)
        {
            if (IOManager.Instance.INIO_status[(int)index] == 0)
            {
                return true;
            }
            else if (IOManager.Instance.INIO_status[(int)index] == 1)
            {
                return false;
            }
            else
            {
                ErrorManager.Current.Insert(ErrorCode.IOErr, $"Failed to read {index.ToString()}");
                return false;
            }
        }

        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
            DateTime time = DateTime.Now;
            bool ret = false;
            while ((DateTime.Now - time).TotalMilliseconds < delta)
            {
                if (ReadIO(index) == value)
                {
                    ret = true;
                    break;
                }
                Thread.Sleep(1);
            }

            return ret;
        }

        public void Clear()
        {

            GlobalManager.Current.laserPoints.Clear();
            GlobalManager.Current.feedar1Points.Clear();
            GlobalManager.Current.feedar2Points.Clear();
            GlobalManager.Current.pickFoam1Points.Clear();
            GlobalManager.Current.pickFoam2Points.Clear();
            GlobalManager.Current.lowerCCDPoints.Clear();
            GlobalManager.Current.dropBadFoamPoints.Clear();
            GlobalManager.Current.snapPalletePoints.Clear();
            GlobalManager.Current.placeFoamPoints.Clear();
            GlobalManager.Current.recheckPoints.Clear();
            GlobalManager.Current.tearingPoints.Clear();

            //GlobalManager.Current.BarcodeQueue.Clear();
        }
        public void StartAutoRunThreads()
        {
            if (!CheckTaskReady())
            {
                Console.WriteLine("Not Ready");
                return;
            }
            //if (!hasReseted)
            //{
            //    MessageBox.Show("Please Reset!");
            //    return;
            //}
            if (!Initialize())
            {
                return;
            }
            try
            {
                Trace.WriteLine("Autorun Process");
                Logger.WriteLog("Autorun Process Start");
                ParameterConfig.LoadPoints();

                if (ThreadConveyor != null && !ThreadConveyor.IsAlive)
                {
                    Conveyor.Current.ThreadState = WorkStationBase.ThreadStatus.Init;
                    ThreadConveyor.Start();
                }
                else
                {
                    Conveyor.Current.ThreadState = WorkStationBase.ThreadStatus.Resuming;
                }
                if (ThreadLaser != null && !ThreadLaser.IsAlive)
                {
                    LaiLiao.Current.ThreadState = WorkStationBase.ThreadStatus.Init;
                    ThreadLaser.Start();
                }
                else
                {
                    LaiLiao.Current.ThreadState = WorkStationBase.ThreadStatus.Resuming;
                }
                //if (ThreadFoamAssembly != null && !ThreadFoamAssembly.IsAlive)
                //{
                //    ZuZhuang.Current.ThreadState = WorkStationBase.ThreadStatus.Init;
                //    ThreadFoamAssembly.Start();
                //}
                //else
                //{
                //    ZuZhuang.Current.ThreadState = WorkStationBase.ThreadStatus.Resuming;
                //}
                //if (ThreadRecheck != null && !ThreadRecheck.IsAlive)
                //{
                //    FuJian.Current.ThreadState = WorkStationBase.ThreadStatus.Init;
                //    ThreadRecheck.Start();
                //}
                //else
                //{
                //    FuJian.Current.ThreadState = WorkStationBase.ThreadStatus.Resuming;
                //}

                //if (ThreadFeeder != null && !ThreadRecheck.IsAlive)
                //{
                //    Feeder.Current.ThreadState = WorkStationBase.ThreadStatus.Init;
                //    ThreadFeeder.Start();
                //}
                //else
                //{
                //    Feeder.Current.ThreadState = WorkStationBase.ThreadStatus.Resuming;
                //}


                //if (ThreadTest1 != null && !ThreadTest1.IsAlive)
                //{
                //    TEST1.Current.ThreadState = WorkStationBase.ThreadStatus.Init;
                //    ThreadTest1.Start();
                //}
                //else
                //{
                //    TEST1.Current.ThreadState = WorkStationBase.ThreadStatus.Resuming;
                //}


                //if (ThreadTest2 != null && !ThreadTest2.IsAlive)
                //{
                //    TEST2.Current.ThreadState = WorkStationBase.ThreadStatus.Init;
                //    ThreadTest2.Start();
                //}
                //else
                //{
                //    TEST2.Current.ThreadState = WorkStationBase.ThreadStatus.Resuming;
                //}


            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("AutoRun is Cancelled");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error Process");

            }
            finally
            {
                Trace.WriteLine("Final Process");
            }
            Logger.WriteLog("Autorun Process End");
        }

        private void RunAutoStation(WorkStationBase station, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {

                switch (station.ThreadState)
                {
                    case WorkStationBase.ThreadStatus.Init:
                        station.Initialize();
                        station.ThreadState = WorkStationBase.ThreadStatus.Running;
                        break;
                    case WorkStationBase.ThreadStatus.Running:
                        {
                            if (IsPause)
                            {
                                station.ThreadState = WorkStationBase.ThreadStatus.Pausing;
                                break;
                            }
                            else
                            {

                                try
                                {
                                    if (!station.AutoRun())
                                    {
                                        station.ThreadState = WorkStationBase.ThreadStatus.Pausing;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    station.ThreadState = WorkStationBase.ThreadStatus.Pausing;
                                }
                            }
                        }
                        break;
                    case WorkStationBase.ThreadStatus.Pausing:
                        IsPause = true;
                        station.Paused();
                        station.ThreadState = WorkStationBase.ThreadStatus.Paused;
                        break;
                    case WorkStationBase.ThreadStatus.Paused:
                        break;
                    case WorkStationBase.ThreadStatus.Resuming:
                        station.ResetAfterPause();
                        station.ThreadState = WorkStationBase.ThreadStatus.Running;
                        break;
                    case WorkStationBase.ThreadStatus.Stop:
                        break;
                    default:
                        break;

                }

                Thread.Sleep(0);
            }

            AutorunManager.Current.isRunning = false;
        }

        // 退出AutoRun
        public void StopAutoRun()
        {
            CancelToken?.Cancel();
            GlobalManager.Current.Lailiao_exit = true;
            GlobalManager.Current.Zuzhuang_exit = true;
            GlobalManager.Current.FuJian_exit = true;
            GlobalManager.Current.Reject_exit = true;
            AutorunManager.Current.isRunning = false;
            hasReseted = false;
            GlobalManager.Current.IO_test1 = false;
            GlobalManager.Current.IO_test2 = false;
            GlobalManager.Current.IO_test3 = false;
        }

        public void CylinderDown()
        {
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_124_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_134_lift_cylinder_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, 1);


            //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0);
            //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1);


            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_8Reserve, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_9Reserve, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_10Reserve, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_11Reserve, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_12Reserve, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_13Reserve, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_14Reserve, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_15Reserve, 0);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_8Feeder_vacuum1_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_9Feeder_vacuum1_Release, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_10Feeder_vacuum2_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_11Feeder_vacuum2_Release, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_12Feeder_vacuum3_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_13Feeder_vacuum3_Release, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_14Feeder_vacuum4_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_15Feeder_vacuum4_Release, 0);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_3light1, 1);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_4light2, 1);

            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_3Peeling_Recheck_vacuum1_Release, 0);

            //判断所有气缸缩回
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos, 1);
            GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos, 1);
            //GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_10Claw_retract_in_position, 1);
        }
        public bool Reset()
        {
            ErrorManager.Current.Clear();
            return true;
        }

        //public bool Reset()
        //{

        //    //20250519 测试 【史彦洋】 追加 Start
        //    //CylinderDown();
        //    //Conveyor.Current.AllWorkStopCylinderAct(1, 0);
        //    //return true;

        //    AkrAction.Current.MoveAllConveyor();
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 0);
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, 0);
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_0Tri_color_light_red, 0);

        //    //飞达复位
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_10initialize_feeder1, 1);

        //    //需要这两个信号都是0，代表电机可以复位，安全门也可以复位
        //    if (!WaitIO(3000, IO_INFunction_Table.IN5_14SSR1_OK_emergency_stop, false) && !WaitIO(3000, IO_INFunction_Table.IN5_15SSR2_OK_LOCK, false))
        //    {
        //        return false;
        //    }


        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 1);
        //    Thread.Sleep(300);
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 0);

        //    //AkrAction.Current.axisAllZEnableMotor(true);
        //    //Thread.Sleep(200);


        //    //先对Z轴hardstop回零
        //    //AkrAction.Current.axisAllZHome_HardStop();
        //    //if (AkrAction.Current.WaitAllHomingZFinished() != 0) return false;


        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 1);
        //    Thread.Sleep(300);
        //    //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 0);
        //    //复位气缸和吸嘴IO
        //    CylinderDown();

        //    //轴使能
        //    AkrAction.Current.EnableAllMotors(true);

        //    AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response4);
        //    Thread.Sleep(300);
        //    //轴回原点

        //    //AkrAction.Current.axisAllHome("D:\\akribisfam_config\\HomeFile");
        //    //AkrAction.Current.axisAllTHome("D:\\akribisfam_config\\HomeFileT");

        //    //if (AkrAction.Current.CheckAllAxisHomeCompleted(out bool allEnabled) != (int)AkrAction.ACTTION_ERR.NONE) return false;

        //    //把旋转轴的当前位置作为0位置
        //    //AkrAction.Current.SetZeroAll();


        //    //if (LaiLiao.Current.board_count != 0 || ZuZhuang.Current.board_count != 0 || FuJian.Current.board_count != 0 || Reject.Current.board_count != 0)
        //    //{
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, 1);
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, 0);

        //    //    Thread.Sleep(3000);
        //    //}

        //    //传送带停止
        //    //AkrAction.Current.StopConveyor();


        //    //激光测距复位(tcp)
        //    //Task_KEYENCEDistance.SendResetData();
        //    //var a = Task_KEYENCEDistance.AcceptMSData()[0];

        //    //相机复位(tcp)
        //    //sendSetStatCamreapositionList.Clear();
        //    //SendSetStatCamreaposition command = new SendSetStatCamreaposition
        //    //{
        //    //    AE_Station = "123",
        //    //    ProjectName ="project",
        //    //};
        //    //sendSetStatCamreapositionList.Add(command);
        //    //Task_ResetCamreaFunction.TriggResetCamreaSendData(Task_ResetCamreaFunction.ResetCamreaProcessCommand.SetStation , sendSetStatCamreapositionList);

        //    //程序状态为置0
        //    GlobalManager.Current.current_Lailiao_step = 0;
        //    GlobalManager.Current.current_Zuzhuang_step = 0;
        //    GlobalManager.Current.current_FuJian_step = 0;
        //    GlobalManager.Current.current_Reject_step = 0;
        //    LaiLiao.Current.board_count = 0;
        //    ZuZhuang.Current.board_count = 0;
        //    FuJian.Current.board_count = 0;
        //    Reject.Current.board_count = 0;

        //    GlobalManager.Current.Lailiao_exit = false;
        //    GlobalManager.Current.Zuzhuang_exit = false;
        //    GlobalManager.Current.FuJian_exit = false;
        //    GlobalManager.Current.Reject_exit = false;

        //    //把所有阻挡气缸伸出
        //    Conveyor.Current.AllWorkStopCylinderAct(1, 0);

        //    //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_1Tri_color_light_yellow, 0);
        //    //Thread.Sleep(500);
        //    //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, 1);
        //    //Thread.Sleep(500);
        //    //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, 0);
        //    //Thread.Sleep(500);
        //    //IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_2Tri_color_light_green, 1);

        //    //AkrAction.Current.axisAllZHome("D:\\akribisfam_config\\HomeFileZ");
        //    //if (AkrAction.Current.WaitAllHomingZFinished() != 0) return false;

        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 1);
        //    Thread.Sleep(500);
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 0);
        //    //让飞达送料
        //    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_9Run_feeder1, 1);

        //    IsPause = false;
        //    return true;
        //}


    }
}
