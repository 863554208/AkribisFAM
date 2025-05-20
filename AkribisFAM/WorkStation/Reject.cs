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

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;

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
            return IOManager.Instance.INIO_status[(int)index];

        }

        public void SetIO(IO_OutFunction_Table index , int value)
        {
            IOManager.Instance.IO_ControlStatus( index , value);
        }
		
        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveConveyor(vel);
        }

        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
        }


        public int DropNGPallete()
        {
            return 0;
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
                Thread.Sleep(50);
            }

            return ret;
        }
		
        public void WaitConveyor(int delta, IO[] IOarr, int type)
        {
            //DateTime time = DateTime.Now;

            //if (delta != 0 && IOarr != null)
            //{
            //    while ((DateTime.Now - time).TotalMilliseconds < delta)
            //    {
            //        int judge = 0;
            //        foreach (var item in IOarr)
            //        {
            //            var res = ReadIO(item) ? 1 : 0;
            //            judge += res;
            //        }

            //        if (judge > 0)
            //        {
            //            break;
            //        }
            //        Thread.Sleep(50);
            //    }
            //}
            //else
            //{
            //    switch (type)
            //    {
            //        case 2:
            //            while (DropNGPallete() == 1) ;
            //            break;

            //    }
            //}
        }

        public void ResumeConveyor()
        {
            if (GlobalManager.Current.station2_IsBoardInLowSpeed || GlobalManager.Current.station3_IsBoardInLowSpeed || GlobalManager.Current.station1_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(100);
            }
            else if (GlobalManager.Current.station2_IsBoardInHighSpeed || GlobalManager.Current.station3_IsBoardInHighSpeed || GlobalManager.Current.station1_IsBoardInHighSpeed)
            {
                MoveConveyor((int)AxisSpeed.BL1);
            }
        }

        public bool hasNGboard;
        public bool BoardIn()
        {
            bool ret;
            //进入后改回false
            GlobalManager.Current.IOTable[(int)IO.Reject_BoardIn] = false;
            Set("station4_IsBoardInHighSpeed", true);
            //Set("IO_test4", false);
            //传送带高速移动
            MoveConveyor((int)AxisSpeed.BL4);
            MoveConveyor((int)AxisSpeed.BR4);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //等待减速IO
            ret = WaitIO(9999, IO_INFunction_Table.IN1_3Slowdown_Sign4, true);
            Set("station4_IsBoardInHighSpeed", false);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            //挡板气缸上气
            SetIO(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, 1);
            SetIO(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, 0);
            Set("station4_IsBoardInLowSpeed", true);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //传送带减速
            MoveConveyor(100);
            //等待停止IO
            ret = WaitIO(9999, IO_INFunction_Table.IN1_7Stop_Sign4, true);
            Set("station4_IsBoardInLowSpeed", false);
            Set("station4_IsBoardIn", false);
            Set("station4_IsLifting", true);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            board_count += 1;
            //停止传送带
            StopConveyor();
            if (CheckState(ret) == 1)
            {
                return false;
            }
            Set("station4_IsBoardOut", true);
            return true;
        }

        private bool ActionNG() {
            bool ret;
            //顶起气缸上气
            SetIO(IO_OutFunction_Table.OUT1_124_lift_cylinder_extend, 1);
            SetIO(IO_OutFunction_Table.OUT1_134_lift_cylinder_retract, 0);
            Set("station4_IsLifting", false);
            ResumeConveyor();
            if (CheckState(true) == 1)
            {
                return false;
            }
            //NG位感应IO
            ret = WaitIO(9999, IO_INFunction_Table.IN6_0NG_plate_1_in_position, true);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            //顶起气缸下降
            SetIO(IO_OutFunction_Table.OUT1_124_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_134_lift_cylinder_retract, 1);
            if (CheckState(true) == 1)
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
            ret = WaitIO(9999, IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos, true);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            board_count -= 1;
            hasNGboard = true;

            Task<bool> task = new Task<bool>(() =>
            {
                return DetectNG();
            });
            task.Start();
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
            ret = WaitIO(9999, IO_INFunction_Table.IN7_2MACHINE_READY_TO_RECEIVE, true);
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
            ret = WaitIO(9999, IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos, true);
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
            bool ret = WaitIO(999999, IO_INFunction_Table.IN6_0NG_plate_1_in_position, false);
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

            try
            {
                while (true)
                {
                    //20250519 测试 【史彦洋】 追加 Start
                    Console.WriteLine("zuzhuang ceshi 1");
                    Thread.Sleep(1000);
                    continue;

                    step1:
                        if (!GlobalManager.Current.IOTable[(int)IO.Reject_BoardIn] || board_count != 0) {
                            Thread.Sleep(100);
                            continue;
                        }
                        GlobalManager.Current.current_Reject_step = 1;
                        BoardIn();
                        if (GlobalManager.Current.Reject_exit) break;

                    step2:
                        GlobalManager.Current.current_Reject_step = 2;
                        Step2();
                        if (GlobalManager.Current.Reject_exit) break;
                            
                }
            }
            catch (Exception ex)
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }

        [JsonObject]
        public class TrainPoint
        {
            [JsonProperty("X")]
            public int x { get; set; }
            [JsonProperty("Y")]
            public int y { get; set; }
            [JsonProperty("Z")]
            public int z { get; set; }
            [JsonProperty("R")]
            public int r { get; set; }
        }

        //1-4结束位置， 5起始位置， 6-9取料位置
        public List<TrainPoint> TrainPointlist = new List<TrainPoint>(9);

        public bool TrainNozzle(int pickernum)
        {
            bool ret;
            //移动取料
            AkrAction.Current.Move(AxisName.FSX, TrainPointlist[pickernum + 5].x, (int)AxisSpeed.FSX);
            AkrAction.Current.Move(AxisName.FSY, TrainPointlist[pickernum + 5].y, (int)AxisSpeed.FSY);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //picker 取料
            AkrAction.Current.Move(AxisName.PICK1_Z, 10000, (int)AxisSpeed.PICK1_Z);
            if (pickernum == 0)
            {
                SetIO(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                ret = WaitIO(999999, IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback, true);
            }
            else if (pickernum == 1)
            {
                SetIO(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
                ret = WaitIO(999999, IO_INFunction_Table.IN3_13PNP_Gantry_vacuum2_Pressure_feedback, true);
            }
            else if (pickernum == 2) {
                SetIO(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
                ret = WaitIO(999999, IO_INFunction_Table.IN3_14PNP_Gantry_vacuum3_Pressure_feedback, true);
            }
            else if (pickernum == 3)
            {
                SetIO(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
                ret = WaitIO(999999, IO_INFunction_Table.IN3_15PNP_Gantry_vacuum4_Pressure_feedback, true);
            }
            else
            {
                ret = false;
            }
            if (CheckState(ret) == 1)
            {
                return false;
            }
            AkrAction.Current.Move(AxisName.PICK1_Z, 20000, (int)AxisSpeed.PICK1_Z);
            //移动到飞拍起始位置
            AkrAction.Current.Move(AxisName.FSX, TrainPointlist[4].x, (int)AxisSpeed.FSX);
            AkrAction.Current.Move(AxisName.FSY, TrainPointlist[4].y, (int)AxisSpeed.FSY);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //给Cognex发拍照信息
            string command = "SN" + "123456," + $"+{pickernum}," + "Foam," + $"{TrainPointlist[pickernum].x},{TrainPointlist[pickernum].y},{TrainPointlist[pickernum].r}";
            TriggTTNCamreaSendData(TTNProcessCommand.TTN, command);
            while (true) {
                if (TriggTTNCamreaready() == "OK") {
                    break;
                }
            }

            //飞拍移动到结束位置
            AkrAction.Current.SetSingleEvent(AxisName.FSX, TrainPointlist[pickernum].x, 1);
            AkrAction.Current.MoveNoWait(AxisName.FSX, TrainPointlist[pickernum].x, (int)AxisSpeed.FSX);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //接受Cognex结果
            string Errcode = TriggTTNCamreaAcceptData(TTNProcessCommand.TTN)[0].Errcode1;
            if (Errcode != "1") {
                return false;
            }

            return true;
        }

        public void TrainNozzles(int nozzlenum)
        {
            try
            {
                string folder = Directory.GetCurrentDirectory(); //获取应用程序的当前工作目录。 
                string path = folder + "\\NozzleCalib1.json";
                string content = File.ReadAllText(path);
                TrainPointlist = JsonConvert.DeserializeObject<List<TrainPoint>>(content);
                if (TrainPointlist == null)
                {
                    return;
                }
            }
            catch {
                //配置读取失败
                return;
            }
            Task<bool> task = new Task<bool>(() =>
            {
                bool ret = TrainNozzle(nozzlenum);
                if (CheckState(ret) == 1)
                {
                    return false;
                }
                return true;
            });

            task.Start();

        }
    }
}
