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

namespace AkribisFAM.WorkStation
{
    internal class Reject : WorkStationBase
    {

        private static Reject _instance;
        public override string Name => nameof(Reject);

        private ErrorCode errorCode;

        int delta = 0;
        public int board_count = 0;

        public static Reject Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new Reject();
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
            else {
                ErrorManager.Current.Insert(ErrorCode.IOErr);
                return false;
            }
        }

        public void SetIO(IO_OutFunction_Table index , int value)
        {
            IOManager.Instance.IO_ControlStatus( index , value);
        }
		
        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveConveyor(vel);
        }

        public void MoveNGConveyor(int vel)
        {
            AkrAction.Current.MoveNGConveyor(vel);
        }

        public void StopNGConveyor()
        {
            AkrAction.Current.StopNGConveyor();
        }

        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
        }


        public int DropNGPallete()
        {
            return 0;
        }

        private int[] signalval = new int[10];
        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
            DateTime time = DateTime.Now;
            bool ret = false;
            int cnt = 0;
            for (int i = 0; i < signalval.Length; i++)
            {
                signalval[i] = 0;
            }
            while ((DateTime.Now - time).TotalMilliseconds < delta)
            {
                int validx = 0;
                if (cnt < 10)
                {
                    validx = cnt;
                }
                else
                {
                    validx = cnt % 10;
                }
                if (ReadIO(index) == value)
                {
                    signalval[validx] = 1;
                }
                else
                {
                    signalval[validx] = 0;
                }
                cnt++;
                if (signalval.Sum() >= 8)
                {
                    ret = true;
                    break;
                }
                Thread.Sleep(1);
            }

            return ret;
        }


        public bool hasNGboard;

        private bool ActionNG() {
            bool ret, ret1;
            //顶起气缸上气
            SetIO(IO_OutFunction_Table.OUT1_124_lift_cylinder_extend, 1);
            SetIO(IO_OutFunction_Table.OUT1_134_lift_cylinder_retract, 0);
            Set("station4_IsLifting", false);
            //先等待有信号，再等待没信号
            ret = WaitIO(999, IO_INFunction_Table.IN6_0NG_plate_1_in_position, true);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            Thread.Sleep(300);
            ret = WaitIO(999, IO_INFunction_Table.IN6_0NG_plate_1_in_position, false);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            Thread.Sleep(1000);
            //顶起气缸下降
            SetIO(IO_OutFunction_Table.OUT1_124_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_134_lift_cylinder_retract, 1);
            ret = WaitIO(999, IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos, false);
            ret1 = WaitIO(999, IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos, true);
            if (CheckState(ret&&ret1) == 1)
            {
                return false;
            }
            //挡板气缸下降
            SetIO(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, 0);
            SetIO(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, 1);
            //等待挡板下降到位信号
            ret = WaitIO(999, IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos, true);
            ret1 = WaitIO(999, IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos, true);
            if (CheckState(ret && ret1) == 1)
            {
                return false;
            }
            board_count -= 1;
            hasNGboard = true;

            DetectNG();
            return true;
        }

        private bool ActionOK()
        {
            bool ret;
            //发送出料信号
            SetIO(IO_OutFunction_Table.OUT7_1BOARD_AVAILABLE, 1);

            if (CheckState(true) == 1)
            {
                return false;
            }
            //等待允许出料信号
            ret = WaitIO(99999, IO_INFunction_Table.IN7_2MACHINE_READY_TO_RECEIVE, true);   
            if (CheckState(ret) == 1)
            {
                return false;
            }
            //挡板气缸下降
            SetIO(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, 0);
            SetIO(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, 1);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //等待挡板下降到位信号
            ret = WaitIO(999, IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos, true);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            board_count -= 1;
            return true;
        }

        private bool DetectNG() {
            //打开蜂鸣器
            SetIO(IO_OutFunction_Table.OUT6_5Buzzer, 1);
            //等待取走信号
            bool ret = WaitIO(99999, IO_INFunction_Table.IN6_0NG_plate_1_in_position, false);//wait 100s
            if (CheckState(ret) == 1)
            {
                //关闭蜂鸣器
                SetIO(IO_OutFunction_Table.OUT6_5Buzzer, 0);
                return false;
            }
            if (ret)
            {
                board_count -= 1;
            }
            //关闭蜂鸣器
            SetIO(IO_OutFunction_Table.OUT6_5Buzzer, 0);
            return true;
        }

        public int CheckState(bool state)
        {
            if (GlobalManager.Current.Reject_exit) return 1;
            if (state)
            {
                GlobalManager.Current.Reject_state[GlobalManager.Current.current_Reject_step] = 0;
            }
            else
            {
                GlobalManager.Current.Reject_state[GlobalManager.Current.current_Reject_step] = 1;
                GlobalManager.Current.IsPause = true;
                ErrorManager.Current.Insert(errorCode);
            }
            GlobalManager.Current.Reject_CheckState();
            WarningManager.Current.WaiReject();
            return 0;
        }

        public bool Step2()
        {
            if (GlobalManager.Current.isNGPallete)
            {
                StateManager.Current.TotalOutputNG++;
                if (!hasNGboard)
                {
                    //NG位无料
                    return ActionNG();
                }
                else
                {
                    //NG位有料
                    CheckState(false);
                    return false;
                }
            }
            else
            {
                StateManager.Current.TotalOutputOK++;
                //OK料
                return ActionOK();
            }
        }

        public override void AutoRun(CancellationToken token)
        {
            GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 0;
            bool ret, ret1;
            try
            {
                while (true)
                {
                step1:
                    GlobalManager.Current.current_Reject_step = 1;
                    Debug.WriteLine("NG工位阻挡气缸上升");
                    SetIO(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, 1);
                    SetIO(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, 0);
                    ret = WaitIO(999, IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos, true);
                    ret1 = WaitIO(999, IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos, false);
                    if (CheckState(ret && ret1) == 1)
                    {
                        break;
                    }
                    Debug.WriteLine("NG工位允许进料盘");
                    GlobalManager.Current.flag_NGStationAllowTrayEnter = 1;

                    Debug.WriteLine("NG工位等待请求进料盘");
                    while (GlobalManager.Current.flag_RecheckStationRequestOutflowTray != 1)
                    {
                        Thread.Sleep(50);
                    }
                    GlobalManager.Current.flag_NGStationAllowTrayEnter = 0;
                    GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 0;

                    bool temp_isNG = false;
                    //先判断是否为bypass料  TODO
                    if (GlobalManager.Current.flag_Bypass == 1)
                    {
                        temp_isNG = true;
                        GlobalManager.Current.flag_Bypass = 0;
                        goto step2;
                    }

                    //获取当前料盘是否NG
                    temp_isNG = GlobalManager.Current.isNGPallete;
                    GlobalManager.Current.isNGPallete = false;

                step2:
                    GlobalManager.Current.current_Reject_step = 2;
                    Console.WriteLine("NG工位皮带高速运行");
                    MoveNGConveyor((int)AxisSpeed.BL4);
                    ret = WaitIO(9999, IO_INFunction_Table.IN1_3Slowdown_Sign4, true);
                    if (CheckState(ret) == 1)
                    {
                        break;
                    }
                    Thread.Sleep(200);
                    ret = WaitIO(9999, IO_INFunction_Table.IN1_3Slowdown_Sign4, false);
                    if (CheckState(ret) == 1)
                    {
                        break;
                    }
                    Console.WriteLine("NG工位皮带低速运行");
                    Thread.Sleep(GlobalManager.Current.NGTrayDelaytime);
                    MoveNGConveyor(30);
                    ret = WaitIO(19999, IO_INFunction_Table.IN1_7Stop_Sign4, true);
                    if (CheckState(ret) == 1)
                    {
                        break;
                    }
                    StopNGConveyor();

                step3:
                    GlobalManager.Current.current_Reject_step = 3;
                    if (temp_isNG)
                    {
                        //需要等到上次的NG料盘已经被取走。再进行顶起当前NG料盘的动作。TODO
                        //NG料盘顶起
                        SetIO(IO_OutFunction_Table.OUT1_124_lift_cylinder_extend, 1);
                        SetIO(IO_OutFunction_Table.OUT1_134_lift_cylinder_retract, 0);
                        ret = WaitIO(999, IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos, true);
                        ret1 = WaitIO(999, IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos, false);
                        if (CheckState(ret&&ret1) == 1)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                        SetIO(IO_OutFunction_Table.OUT1_124_lift_cylinder_extend, 0);
                        SetIO(IO_OutFunction_Table.OUT1_134_lift_cylinder_retract, 1);
                        ret = WaitIO(999, IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos, false);
                        ret1 = WaitIO(999, IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos, true);
                        if (CheckState(ret && ret1) == 1)
                        {
                            break;
                        }
                        //设置标志位，通知操作员取出NG料盘。需要启动另外一个线程来做提示TODO

                        goto step1;
                    }

                step4:
                    GlobalManager.Current.current_Reject_step = 4;
                    Console.WriteLine("NG工位向下游设备请求送料盘");
                    SetIO(IO_OutFunction_Table.OUT7_1BOARD_AVAILABLE, 1);
                    ret = WaitIO(99999, IO_INFunction_Table.IN7_2MACHINE_READY_TO_RECEIVE, true);
                    if (CheckState(ret) == 1)
                    {
                        break;
                    }
                    Console.WriteLine("等到下游设备允许送板信号");
                    SetIO(IO_OutFunction_Table.OUT7_1BOARD_AVAILABLE, 0);

                    Console.WriteLine("NG工位出料阻挡气缸下降");
                    SetIO(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, 0);
                    SetIO(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, 1);
                    ret = WaitIO(999, IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos, true);
                    if (CheckState(ret) == 1)
                    {
                        break;
                    }
                    MoveNGConveyor((int)AxisSpeed.BL4);

                    //确保料盘送入下游设备
                    Console.WriteLine("等待NG工位料盘流出信号被触发");
                    ret = WaitIO(9999, IO_INFunction_Table.IN6_7plate_has_left_Behind_the_stopping_cylinder4, true);
                    if (CheckState(ret) == 1)
                    {
                        break;
                    }
                    Console.WriteLine("等待NG工位料盘流出信号消失");
                    ret = WaitIO(9999, IO_INFunction_Table.IN6_7plate_has_left_Behind_the_stopping_cylinder4, false);
                    if (CheckState(ret) == 1)
                    {
                        break;
                    }
                    StopConveyor();
                    goto step1;

                }
            }
            catch (Exception ex)
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }

 
    }
}
