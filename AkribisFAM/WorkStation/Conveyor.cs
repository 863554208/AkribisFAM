using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using AAMotion;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using LiveCharts.SeriesAlgorithms;
using YamlDotNet.Core;
using HslCommunication;
using static AkribisFAM.GlobalManager;
using static AAComm.Extensions.AACommFwInfo;
using AkribisFAM.Windows;
using System.Windows;
using static AkribisFAM.CommunicationProtocol.Task_PrecisionDownCamreaFunction;
using System.Diagnostics.Eventing.Reader;
using static AkribisFAM.CommunicationProtocol.Task_RecheckCamreaFunction;
using static AkribisFAM.CommunicationProtocol.Task_TTNCamreaFunction;
using Newtonsoft.Json;
using System.IO;
using AkribisFAM.Util;

namespace AkribisFAM.WorkStation
{
    internal class Conveyor : WorkStationBase
    {
        private static Conveyor _instance;
        public override string Name => nameof(Conveyor);

        private ErrorCode errorCode;

        public static Conveyor Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new Conveyor();
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

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }

        public override bool Ready()
        {
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
                ErrorManager.Current.Insert(ErrorCode.IOErr);
                return false;
            }
        }

        public void SetIO(IO_OutFunction_Table index, int value)
        {
            IOManager.Instance.IO_ControlStatus(index, value);
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
                Thread.Sleep(10);
            }

            return ret;
        }

        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveConveyor(vel);
        }
        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
        }
        public void AllWorkLiftCylinderRetract()
        {
            AllWorkStopCylinderAct(0, 1);

            SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract, 1);



            WaitIO(99999999, IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos, true);



        }
        public void AllWorkStopCylinderAct(int extendValue, int retractValue)
        {
            SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, extendValue);
            SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, retractValue);
            SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, extendValue);
            SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, retractValue);
            SetIO(IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend, extendValue);
            SetIO(IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract, retractValue);
            //SetIO(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, extendValue);
            //SetIO(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, retractValue);


            if (extendValue == 1)
            {
                WaitIO(499, IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos, true);
            }
            if (retractValue == 1)
            {
                WaitIO(499, IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos, true);
            }


        }

        public void LiftUpRelatedTray(IO_OutFunction_Table IOName1,IO_OutFunction_Table IOName2,IO_OutFunction_Table IOName3,IO_OutFunction_Table IOName4,IO_INFunction_Table IOName5,IO_INFunction_Table IOName6)
        {
            SetIO(IOName1, 1);
            SetIO(IOName2, 0);

            SetIO(IOName3, 1);
            SetIO(IOName4, 0);

            WaitIO(99999, IOName5, true);
            WaitIO(99999, IOName6, true);

        }
        public override void AutoRun(CancellationToken token)
        {
            while (true)
            {
            STEP_Init:
                int data_AllStationTrayNumber = 0;                       //不包含NG工位

                GlobalManager.Current.flag_RangeFindingTrayArrived = 0;  //测距工位
                GlobalManager.Current.flag_assembleTrayArrived = 0;      //贴装工位
                GlobalManager.Current.flag_RecheckTrayArrived = 0;       //复检工位

                GlobalManager.Current.flag_TrayProcessCompletedNumber = 0;  //每个工位完成料盘处理后对此变量自增1
                GlobalManager.Current.flag_TrayArrivedNumber = 0;

                GlobalManager.Current.flag_RecheckStationHaveTray = 0;
                GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 0;

                GlobalManager.Current.flag_Bypass = 0;



            STEP_JudgeAllStationTrayNumberIsZero:
                Logger.WriteLog("皮带任务_判断设备内有无料盘");
                if (data_AllStationTrayNumber == 0)
                {
                    goto STEP_WaitUpstreamEquipmentHaveTray;
                }


            //如果设备内已经有料盘，执行下面的逻辑
            STEP_WaitingAllTrayProcessCompleted:
                Logger.WriteLog("皮带任务_等待所有工位料盘产品处理完成");
                Logger.WriteLog("flag_TrayProcessCompletedNumber : " + GlobalManager.Current.flag_TrayProcessCompletedNumber.ToString());

                while (data_AllStationTrayNumber != GlobalManager.Current.flag_TrayProcessCompletedNumber) { System.Threading.Thread.Sleep(30); }


            STEP_JudgeIsBypass:
                Logger.WriteLog("皮带任务_判断是否为bypass");
                if (GlobalManager.Current.flag_Bypass == 1)
                {
                    goto STEP_BypassStart;
                }

                GlobalManager.Current.flag_TrayProcessCompletedNumber = 0;


            STEP_JudeRecheckStationHaveTray:
                Logger.WriteLog("皮带任务_判断复检工位是否有料盘");
                if (GlobalManager.Current.flag_RecheckStationHaveTray == 0)
                {
                    goto STEP_JudeUpstreamDeviceHaveTray;
                }

                Logger.WriteLog("皮带任务_等待NG工位允许料盘进入");
                while (GlobalManager.Current.flag_NGStationAllowTrayEnter != 1) { System.Threading.Thread.Sleep(30); }

            STEP_SetRecheckStationRequestOutflowTray:
                Logger.WriteLog("皮带任务_设置复检工位请求流出料盘");
                GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 1;
                GlobalManager.Current.flag_RecheckStationHaveTray = 0;

                Logger.WriteLog("皮带任务_所有工位料盘数量自减1(不包含NG工位)");
                data_AllStationTrayNumber -= 1;


            STEP_JudeUpstreamDeviceHaveTray:
                Logger.WriteLog("皮带任务_判断上游设备是否有料盘");
                //if (!ReadIO(IO_INFunction_Table.IN7_0BOARD_AVAILABLE))
                //{
                //    goto STEP_AllWorkTrayGoDown;
                //}
                if (!GlobalManager.Current.IO_test1)
                {
                    goto STEP_AllWorkTrayGoDown;
                }
                GlobalManager.Current.IO_test1 = false;

            STEP_JudeUpstreamDeviceTrayisFailed:

                bool isFliledTray = WaitIO(999, IO_INFunction_Table.IN7_1FAILED_BOARD_AVAILABLE_OPTIONAL, true);
                if (isFliledTray)
                {
                    GlobalManager.Current.flag_Bypass = 1;
                    Logger.WriteLog("皮带任务_上游设备输送failed料盘后等待NG工位允许进板");
                    while (GlobalManager.Current.flag_NGStationAllowTrayEnter != 1) { System.Threading.Thread.Sleep(30); }

                    Logger.WriteLog("皮带任务_isFliledTray后允许上游设备送料盘");
                    SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);
                    System.Threading.Thread.Sleep(1000);
                    SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);
                    //ToBypassStep
                }



            STEP_SetDeviceAllowEnterTray:

                Logger.WriteLog("皮带任务_所有工位顶升气缸下降(不包含NG工位,先控制阻挡气缸下降再向上游设备要板)");
                AllWorkLiftCylinderRetract();

                Logger.WriteLog("皮带任务_再次允许上游设备送料盘");
                SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);

                Logger.WriteLog("皮带任务_所有工位料盘数量再次自增1(不包含NG工位)");
                data_AllStationTrayNumber += 1;


            STEP_AllWorkTrayGoDown:
                Logger.WriteLog("皮带任务_所有工位顶升气缸下降(不包含NG工位)");
                AllWorkLiftCylinderRetract();

            STEP_WaitTrayThroughStopCylinder:
                Logger.WriteLog("皮带任务_皮带再次高速运行");
                MoveConveyor((int)AxisSpeed.BL1);

                bool IN1_10 = ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1);
                bool IN1_11 = ReadIO(IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2);
                bool IN6_6 = ReadIO(IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3);

                Logger.WriteLog("皮带任务_等待任一料盘触发流出阻挡气缸光电信号");
                System.Threading.Thread.Sleep(3000);
                //while (IN1_10 == false && IN1_11 == false && IN6_6 == false)   //任一光电被触发后，就退出循环
                //{
                //    IN1_10 = ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1);
                //    IN1_11 = ReadIO(IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2);
                //    IN6_6 = ReadIO(IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3);

                //    System.Threading.Thread.Sleep(300);

                //}

                Logger.WriteLog("皮带任务_等待所有料盘流出阻挡气缸光电信号");
                //如果上游设备延迟给料太久，此次会有bug，不影响整体测试。后续再想方案处理TODO
                //while (IN1_10 == true || IN1_11 == true || IN6_6 == true)     //所有料盘流出此光电后，退出循环
                //{
                //    IN1_10 = ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1);
                //    IN1_11 = ReadIO(IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2);
                //    IN6_6 = ReadIO(IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3);

                //    System.Threading.Thread.Sleep(50);

                //}

                Logger.WriteLog("皮带任务_再次禁止上游设备送料盘");
                SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);

            STEP_WaitStopCylinderExtend:
                Logger.WriteLog("皮带任务_所有阻挡气缸伸出");
                AllWorkStopCylinderAct(1, 0);  //阻挡气缸伸出

                Logger.WriteLog("皮带任务_等待任一减速光电信号");

                bool IN1_0 = ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1);
                bool IN1_1 = ReadIO(IO_INFunction_Table.IN1_1Slowdown_Sign2);
                bool IN1_2 = ReadIO(IO_INFunction_Table.IN1_2Slowdown_Sign3);
                while (IN1_0 == true && IN1_1 == true && IN1_2 == true)
                {
                    IN1_0 = ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1);
                    IN1_1 = ReadIO(IO_INFunction_Table.IN1_1Slowdown_Sign2);
                    IN1_2 = ReadIO(IO_INFunction_Table.IN1_2Slowdown_Sign3);
                    System.Threading.Thread.Sleep(20);
                }
            STEP_BeltSlowDown:
                Logger.WriteLog("皮带任务_皮带减速");
                MoveConveyor(20);

            STEP_WaitAnyTrayArrived:

                Logger.WriteLog("皮带任务_等待任一料盘到阻挡位");
                bool IN1_4 = ReadIO(IO_INFunction_Table.IN1_4Stop_Sign1);
                bool IN1_5 = ReadIO(IO_INFunction_Table.IN1_5Stop_Sign2);
                bool IN1_6 = ReadIO(IO_INFunction_Table.IN1_6Stop_Sign3);
                while (IN1_4 == false && IN1_5 == false && IN1_6 == false)
                {
                    IN1_4 = ReadIO(IO_INFunction_Table.IN1_4Stop_Sign1);
                    IN1_5 = ReadIO(IO_INFunction_Table.IN1_5Stop_Sign2);
                    IN1_6 = ReadIO(IO_INFunction_Table.IN1_6Stop_Sign3);

                    System.Threading.Thread.Sleep(10);
                }
                Logger.WriteLog("皮带任务_皮带停止");
                StopConveyor();
                System.Threading.Thread.Sleep(200);

            STEP_LiftUpRelatedTray:
                //优先判断贴装位料盘
                Logger.WriteLog("皮带任务_判断贴装位料盘是否到位");
                if (IN1_5 == true)
                {
                    Logger.WriteLog("皮带任务_贴装位料盘顶起");
                    LiftUpRelatedTray(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend,
                                      IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract,
                                      IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend,
                                      IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract,
                                      IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos,
                                      IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos);

                    Logger.WriteLog("皮带任务_设置贴装位料盘就位");
                    GlobalManager.Current.flag_assembleTrayArrived = 1;
                }

                Logger.WriteLog("皮带任务_判断测距位料盘是否到位");
                if (IN1_4 == true)
                {
                    Logger.WriteLog("皮带任务_测距位料盘顶起");
                    LiftUpRelatedTray(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend,
                                      IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract,
                                      IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend,
                                      IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract,
                                      IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos,
                                      IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos);

                    Logger.WriteLog("皮带任务_设置测距位料盘就位");
                    GlobalManager.Current.flag_RangeFindingTrayArrived = 1;
                }

                Logger.WriteLog("皮带任务_判断复检位料盘是否到位");
                if (IN1_6 == true)
                {
                    Logger.WriteLog("皮带任务_复检位料盘顶起");
                    LiftUpRelatedTray(IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend,
                                      IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract,
                                      IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend,
                                      IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract,
                                      IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos,
                                      IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos);
                    Logger.WriteLog("皮带任务_设置复检位料盘就位");
                    GlobalManager.Current.flag_RecheckTrayArrived = 1;
                    GlobalManager.Current.flag_RecheckStationHaveTray = 1;
                }

            STEP_WaitAllTrayIsArrived:
                Logger.WriteLog("皮带任务_料盘就位数量自增1");
                GlobalManager.Current.flag_TrayArrivedNumber += 1;
                Logger.WriteLog("皮带任务_判断料盘是否全部就位");
                if (GlobalManager.Current.flag_TrayArrivedNumber != data_AllStationTrayNumber)
                {
                    goto STEP_BeltSlowDown;
                }

            STEP_WaitStopCylinderRetract:
                Logger.WriteLog("皮带任务_料盘就位数量和料盘处理完成数量清零");
                GlobalManager.Current.flag_TrayArrivedNumber = 0;

                Logger.WriteLog("皮带任务_所有料盘顶升气缸下降(不包含NG工位)");
                AllWorkStopCylinderAct(0, 1);
                goto STEP_JudgeAllStationTrayNumberIsZero;





            //处理bypass料盘
            STEP_BypassStart:
                GlobalManager.Current.flag_Bypass = 0;

                SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
                SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);
                SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
                SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 1);

                WaitIO(1999, IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos, true);
                WaitIO(1999, IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos, true);

                GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 1;

            STEP_BypassBeltStart:
                MoveConveyor((int)AxisSpeed.BL1);

            STEP_BypassWaitSlowDownSig4:
                WaitIO(99999, IO_INFunction_Table.IN1_7Stop_Sign4, true);
                WaitIO(99999, IO_INFunction_Table.IN1_7Stop_Sign4, false);
                StopConveyor();
                System.Threading.Thread.Sleep(200);
                goto STEP_JudgeAllStationTrayNumberIsZero;


            //处理进入设备内的第一个料盘
            STEP_WaitUpstreamEquipmentHaveTray:
                Logger.WriteLog("皮带任务_等待上游设备有料盘");
                while (!GlobalManager.Current.IO_test1)
                {
                    Thread.Sleep(300);
                }
                GlobalManager.Current.IO_test1 = false;
                //WaitIO(99999999, IO_INFunction_Table.IN7_0BOARD_AVAILABLE, true);

                Logger.WriteLog("皮带任务_允许上游设备送料盘");
                SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);

                Logger.WriteLog("皮带任务_皮带高速运行");
                MoveConveyor((int)AxisSpeed.BL1);

            STEP_WaitSlowDownSig1:
                Logger.WriteLog("皮带任务_等待测距位减速光电信号");
                WaitIO(99999, IO_INFunction_Table.IN1_0Slowdown_Sign1, false);  //有信号时输入模块信号为0

                Logger.WriteLog("皮带任务_禁止上游设备送料盘");
                SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);

                Logger.WriteLog("皮带任务_皮带低速运行");
                MoveConveyor(20);

            STEP_WaitStopSig1:
                Logger.WriteLog("皮带任务_等待料盘到达测距位挡停气缸信号");
                WaitIO(99999, IO_INFunction_Table.IN1_4Stop_Sign1, true);
                Logger.WriteLog("皮带任务_皮带停止");
                StopConveyor();
                System.Threading.Thread.Sleep(200);

            STEP_LiftCylinderExtend1:
                Logger.WriteLog("皮带任务_顶起测距位料盘");
                SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 0);
                SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 0);

                Logger.WriteLog("皮带任务_等待测距位料盘被顶起");
                WaitIO(99999999, IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos, true);
                WaitIO(99999999, IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos, true);

                Logger.WriteLog("皮带任务_设置测距位料盘就位");
                GlobalManager.Current.flag_RangeFindingTrayArrived = 1;

                Logger.WriteLog("皮带任务_所有工位料盘数量自增1(不包含NG工位)");
                data_AllStationTrayNumber += 1;
                goto STEP_JudgeAllStationTrayNumberIsZero;


            }




        }


    }
}
