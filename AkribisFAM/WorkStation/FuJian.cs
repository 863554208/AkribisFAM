using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.Manager;
using LiveCharts.SeriesAlgorithms;
using YamlDotNet.Core;
using HslCommunication;
using static AkribisFAM.GlobalManager;
using AkribisFAM.CommunicationProtocol;
using System.Reflection;
using YamlDotNet.Core.Tokens;
using System.Xml.Linq;
using System.Data.SQLite;
using static System.Windows.Forms.AxHost;
using System.Windows;
using static AkribisFAM.CommunicationProtocol.Task_RecheckCamreaFunction;
using System.Diagnostics;

namespace AkribisFAM.WorkStation
{
    internal class FuJian : WorkStationBase
    {

        private static FuJian _instance;
        public override string Name => nameof(FuJian);

        int delta = 0;
        public int board_count = 0;


        public static FuJian Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new FuJian();
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
        public void WaitConveyor(int type)
        {
            //DateTime time = DateTime.Now;
            //bool ret = false;
            //switch (type)
            //{
            //    case 2: 
            //        while (ScanBarcode() == 1);
            //        break;

            //    case 4:
            //        while(LaserHeight() ==1); 
            //        break;
            //}
        }

        public void ResumeConveyor()
        {
            if (GlobalManager.Current.station2_IsBoardInLowSpeed || GlobalManager.Current.station1_IsBoardInLowSpeed || GlobalManager.Current.station4_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(100);
            }
            else if (GlobalManager.Current.station2_IsBoardInHighSpeed || GlobalManager.Current.station1_IsBoardInHighSpeed || GlobalManager.Current.station4_IsBoardInHighSpeed)
            {
                MoveConveyor((int)AxisSpeed.BL1);
            }
        }
		

        public int CheckState(bool state)
        {
            if (GlobalManager.Current.FuJian_exit) return 1;
            if (state)
            {
                GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
            }
            else {
                //报错
                GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 1;
            }
            GlobalManager.Current.FuJian_CheckState();
            WarningManager.Current.WaiFuJian();
            return 0;
        }

        public void WaitConveyor(int delta, int type)
        {
            //DateTime time = DateTime.Now;

            //switch (type)
            //{
            //    case 2:
            //        while (RemoveFilm() == 1) ;
            //        break;
            //    case 3:
            //        while (CCD3ReCheck() == 1) ;
            //        break;

            //}
        }

        const int modulenum = 26;//1-12 撕膜 13收集 14-25复检 26z撕膜后z轴位置
        public struct FuJianPoint
        {
            public double x;
            public double y;
            public double z;
        }

        public List<FuJianPoint> Pointlist = new List<FuJianPoint>(modulenum);

        public bool BoardIn()
        {
            bool ret;
            //进入后改回false
            GlobalManager.Current.IOTable[(int)IO.FuJian_BoardIn] = false;
            Set("station3_IsBoardInHighSpeed", true);
            Set("IO_test4", false);
            //传送带高速移动
            MoveConveyor((int)AxisSpeed.BL3);
            MoveConveyor((int)AxisSpeed.BR3);
            //等待减速IO
            ret = WaitIO(9999, IO_INFunction_Table.IN1_2Slowdown_Sign3, true);
            if (CheckState(ret) == 1) {
                return false;
            }
            Set("station3_IsBoardInHighSpeed", false);
            //挡板气缸上气
            SetIO(IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend, 1);
            SetIO(IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract, 0);
            Set("station3_IsBoardInLowSpeed", true);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //传送带减速
            MoveConveyor(100);
            //等待停止IO
            ret =  WaitIO(9999, IO_INFunction_Table.IN1_6Stop_Sign3, true);

            if (CheckState(ret) == 1)
            {
                return false;
            }
            Set("station3_IsBoardInLowSpeed", false);
            Set("station3_IsBoardIn", false);
            Set("station3_IsLifting", true);
            //停止传送带
            StopConveyor();
            //顶起气缸上气
            SetIO(IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend, 1);
            SetIO(IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract, 0);
            SetIO(IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend, 1);
            SetIO(IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract, 0);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            //禁止来料
            Set("station3_IsLifting", false);
            ResumeConveyor();
            board_count += 1;

            return true;
        }

        public bool Tearing()
        {
            bool ret;
            //撕膜
            for(int i = 0; i < modulenum; ++i)
            {
                //移动到穴位
                AkrAction.Current.Move(AxisName.PRX, (int)Pointlist[i].x);//mm * 10000
                AkrAction.Current.Move(AxisName.PRY, (int)Pointlist[i].y);
                if (CheckState(true) == 1)
                {
                    return false;
                }
                //移动z轴下降
                AkrAction.Current.Move(AxisName.PRZ, (int)Pointlist[i].z);
                if (CheckState(true) == 1)
                {
                    return false;
                }
                //夹爪气缸打开
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0);
                //检测到位信号
                ret = WaitIO(9999, IO_INFunction_Table.IN3_9Claw_extend_in_position, true);
                if (CheckState(ret) == 1)
                {
                    return false;
                }
                //夹爪气缸夹取
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1);
                //检测到位信号
                ret = WaitIO(9999, IO_INFunction_Table.IN3_10Claw_retract_in_position, true);
                if (CheckState(ret) == 1)
                {
                    return false;
                }
                //移动撕膜
                AkrAction.Current.MoveRel(AxisName.PRY, 2000, 100000);
                AkrAction.Current.MoveRel(AxisName.PRZ, 2000, 100000);
                //Z轴上升
                AkrAction.Current.Move(AxisName.PRZ, (int)Pointlist[26].z);
                if (CheckState(true) == 1)
                {
                    return false;
                }
                //移动到蓝膜收集处
                AkrAction.Current.Move(AxisName.PRX, (int)Pointlist[12].x);//mm * 10000
                AkrAction.Current.Move(AxisName.PRY, (int)Pointlist[12].y);
                AkrAction.Current.Move(AxisName.PRZ, (int)Pointlist[12].z);
                if (CheckState(true) == 1)
                {
                    return false;
                }
                //夹爪气缸打开
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0);
                //检测到位信号
                ret = WaitIO(9999, IO_INFunction_Table.IN3_9Claw_extend_in_position, true);
                if (CheckState(ret) == 1)
                {
                    return false;
                }
                //蓝膜收集吸气
                SetIO(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT4_3Peeling_Recheck_vacuum1_Release, 0);
                Thread.Sleep(1000);
                SetIO(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT4_3Peeling_Recheck_vacuum1_Release, 1);
                if (CheckState(true) == 1)
                {
                    return false;
                }
            }
            return true;
        }

        public int[] modulestate = new int[modulenum];
        public int boardstate;
        public bool Recheck()
        {
            //复检
            int cnt = 0;
            for (int i = modulenum + 1; i < modulenum*2+1; ++i)
            {
                //移动到穴位
                AkrAction.Current.SetSingleEvent(AxisName.PRZ, (int)Pointlist[i].z, 1);
                AkrAction.Current.Move(AxisName.PRX, (int)Pointlist[i].x);//mm * 10000
                AkrAction.Current.Move(AxisName.PRY, (int)Pointlist[i].y);
                AkrAction.Current.Move(AxisName.PRZ, (int)Pointlist[i].z);
                if (CheckState(true) == 1)
                {
                    return false;
                }
                //康耐视复检
                string command = "SN" + "sqcode" + $"+{i - modulenum}," + $"{i - modulenum}," + "Foam+Moudel," + "0.000,0.000,0.000";
                TriggRecheckCamreaTFCSendData(RecheckCamreaProcessCommand.TFC, command);

                //获取康耐视数据
                string Errcode = TriggRecheckCamreaTFCAcceptData(RecheckCamreaProcessCommand.TFC)[0].Errcode;
                int cogres;
                bool ret = int.TryParse(Errcode, out cogres);
                if (CheckState(ret) == 1)
                {
                    return false;
                }
                if (cogres != 1)
                {
                    //康耐视报错
                    CheckState(false);
                }
                string Datan = TriggRecheckCamreaTFCAcceptData(RecheckCamreaProcessCommand.TFC)[0].Datan;
                
                cnt = cnt + modulestate[i - (modulenum + 1)];
            }
            if (cnt < modulenum)
            {
                GlobalManager.Current.isNGPallete = true;
            }
            return true;
        }

        public bool BoardOut()
        {
            bool ret;
            //顶起气缸下降
            SetIO(IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract, 1);
            SetIO(IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract, 1);
            if (CheckState(true) == 1)
            {
                return false;
            }
            //等待顶起气缸下降信号
            ret = WaitIO(9999, IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos, true);
            ret = WaitIO(9999, IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos, true);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            //挡板气缸下降
            SetIO(IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend, 0);
            SetIO(IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract, 1);
            //等待挡板下降到位信号
            ret = WaitIO(9999, IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos, true);
            if (CheckState(ret) == 1)
            {
                return false;
            }
            Set("station3_IsBoardOut", true);
            board_count--;
            GlobalManager.Current.IO_test4 = true;
            return true;
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
                    if (!GlobalManager.Current.IOTable[(int)IO.FuJian_BoardIn] || board_count != 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    GlobalManager.Current.current_FuJian_step = 1;
                    BoardIn();
                    if (GlobalManager.Current.FuJian_exit) break;

                step2:
                    GlobalManager.Current.current_FuJian_step = 2;
                    Tearing();
                    if (GlobalManager.Current.FuJian_exit)break;

                step3:
                    GlobalManager.Current.current_FuJian_step = 3;
                    Recheck();
                    if (GlobalManager.Current.FuJian_exit) break;

                BoardOut();
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
