using System;
using System.Linq;
using System.Threading;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;

namespace AkribisFAM.WorkStation
{
    internal class Reject : WorkStationBase
    {

        private static Reject _instance;
        public override string Name => nameof(Reject);

        private ErrorCode errorCode;

        private static DateTime startTime = DateTime.Now;

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



        public override void Initialize()
        {
            startTime = DateTime.Now;
            return;
        }

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }

        public bool ReadIO(IO_INFunction_Table index)
        {
            if (IOManager.Instance.INIO_status[(int)index] == 0)
            {
                return false;
            }
            else if (IOManager.Instance.INIO_status[(int)index] == 1)
            {
                return true;
            }
            else
            {
                ErrorManager.Current.Insert(ErrorCode.IOErr, $"Failed to read {index.ToString()}");
                return false;
            }
        }

        public void SetIO(IO_OutFunction_Table index , int value)
        {
            IOManager.Instance.IO_ControlStatus( index , value);
        }
		
  
        public void MoveNGConveyor(int vel)
        {
            AkrAction.Current.MoveAllConveyor();
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
            errorCode = ErrorCode.WaitIO;
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
                //IsPause = true;
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

        public override bool AutoRun()
        {
           

            return true;
        
        }

        public override void Paused()
        {
            return;
        }

        public override void ResetAfterPause()
        {
            throw new NotImplementedException();
        }
    }
}
