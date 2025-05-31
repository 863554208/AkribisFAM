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
using Newtonsoft.Json;
using static AkribisFAM.WorkStation.Reject;
using System.IO;
using AkribisFAM.Helper;
using AkribisFAM.Util;

namespace AkribisFAM.WorkStation
{
    internal class FuJian : WorkStationBase
    {

        private static FuJian _instance;
        public override string Name => nameof(FuJian);

        private ErrorCode errorCode;
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
                GlobalManager.Current.IsPause = true;
                ErrorManager.Current.Insert(errorCode);
            }
            GlobalManager.Current.FuJian_CheckState();
            WarningManager.Current.WaiFuJian();
            return 0;
        }

        public bool Tearing()
        {
            bool ret, ret1;
            int actionret;
            int cnt;
            //撕膜
            actionret = AkrAction.Current.Move(AxisName.PRZ, GlobalManager.Current.SafeZPos.Z, (int)AxisSpeed.PRZ, (int)AxisAcc.PRZ);
            errorCode = ErrorCode.TimeOut;
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            for (int i = 0; i < GlobalManager.Current.tearingPoints.Count; ++i)
            {
                //移动到穴位
                Logger.WriteLog("开始执行撕膜X");
                actionret = AkrAction.Current.Move(AxisName.PRX, GlobalManager.Current.tearingPoints[i].X, (int)AxisSpeed.PRX, (int)AxisAcc.PRX);//mm * 10000
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                Logger.WriteLog("开始执行撕膜Y");
                actionret = AkrAction.Current.Move(AxisName.PRY, GlobalManager.Current.tearingPoints[i].Y, (int)AxisSpeed.PRY, (int)AxisAcc.PRY);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                Logger.WriteLog("BBBBBBBBBBBBBB");
                //夹爪气缸打开
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0);
                //移动z轴下降
                Logger.WriteLog("AAAAAAAAAAAAA");
                actionret = AkrAction.Current.Move(AxisName.PRZ, GlobalManager.Current.tearingPoints[i].Z, (int)AxisSpeed.PRZ, (int)AxisAcc.PRZ);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                //检测到位信号
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, true);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, false);
                Logger.WriteLog("CCCCCCCCCCCC");
                if (CheckState(ret&&ret1) == 1)
                {
                    return false;
                }
                //夹爪气缸夹取
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1);
                //检测到位信号
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, false);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, true);
                Logger.WriteLog("DDDDDDDDDDDDDD");
                if (CheckState(ret && ret1) == 1)
                {
                    return false;
                }
                Thread.Sleep(100);
                //移动撕膜
                errorCode = ErrorCode.TimeOut;
                if (Math.Abs(GlobalManager.Current.TearX) > 0.001)
                {
                    AkrAction.Current.MoveRelNoWait(AxisName.PRX, GlobalManager.Current.TearX, GlobalManager.Current.TearXvel);
                }
                if (Math.Abs(GlobalManager.Current.TearY) > 0.001) {
                    AkrAction.Current.MoveRelNoWait(AxisName.PRY, GlobalManager.Current.TearY, GlobalManager.Current.TearYvel);
                }
                AkrAction.Current.MoveRelNoWait(AxisName.PRZ, GlobalManager.Current.TearZ, GlobalManager.Current.TearZvel);
                Logger.WriteLog("EEEEEEEEEEEEEE");
                //Z轴上升
                //TODO 
                int agmIndex = (int)AxisName.PRZ / 8;
                int axisRefNum = (int)AxisName.PRZ % 8;
                cnt = 0;
                while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4 && cnt < 100)
                {
                    Thread.Sleep(50);
                    cnt++;
                }
                if (CheckState(cnt>=100) == 1)
                {
                    return false;
                }
                Logger.WriteLog("ASDDDDDDDDDDDDDE");
                errorCode = ErrorCode.TimeOut;
                actionret = AkrAction.Current.Move(AxisName.PRZ, GlobalManager.Current.SafeZPos.Z, (int)AxisSpeed.PRZ, (int)AxisAcc.PRZ);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                Logger.WriteLog("QQQQQQQQQQQQQQQQQQ");
                //移动到蓝膜收集处
                actionret = AkrAction.Current.Move(AxisName.PRX, GlobalManager.Current.RecheckRecylePos.X, (int)AxisSpeed.PRX, (int)AxisAcc.PRX);//mm * 10000
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                actionret = AkrAction.Current.Move(AxisName.PRY, GlobalManager.Current.RecheckRecylePos.Y, (int)AxisSpeed.PRY, (int)AxisAcc.PRY);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                //必须XY到位后再移动Z轴
                actionret = AkrAction.Current.Move(AxisName.PRZ, GlobalManager.Current.RecheckRecylePos.Z, (int)AxisSpeed.PRZ, (int)AxisAcc.PRZ);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
                Logger.WriteLog("EEEEEEEEEEEEEEEEEEEESADSAD");
                //夹爪气缸打开
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0);
                //检测到位信号
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, true);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, false);
                if (CheckState(ret && ret1) == 1)
                {
                    return false;
                }
                //蓝膜收集吸气
                SetIO(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 1);
                SetIO(IO_OutFunction_Table.OUT4_3Peeling_Recheck_vacuum1_Release, 0);
                Thread.Sleep(500);
                SetIO(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 0);
                SetIO(IO_OutFunction_Table.OUT4_3Peeling_Recheck_vacuum1_Release, 0);
                //夹爪气缸缩回
                SetIO(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0);
                SetIO(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1);
                ret = WaitIO(999, IO_INFunction_Table.IN3_9Claw_extend_in_position, false);
                ret1 = WaitIO(999, IO_INFunction_Table.IN3_10Claw_retract_in_position, true);
                if (CheckState(ret && ret1) == 1)
                {
                    return false;
                }
                errorCode = ErrorCode.TimeOut;
                actionret = AkrAction.Current.Move(AxisName.PRZ, GlobalManager.Current.SafeZPos.Z, (int)AxisSpeed.PRZ, (int)AxisAcc.PRZ);
                if (CheckState(actionret == 0) == 1)
                {
                    return false;
                }
            }
            return true;
        }

        
        public bool Recheck()
        {
            //复检
            int modulestatecnt = 0;
            int actionret;
            errorCode = ErrorCode.TimeOut;
            actionret = AkrAction.Current.Move(AxisName.PRZ, GlobalManager.Current.SafeZPos.Z, (int)AxisSpeed.PRZ, (int)AxisAcc.PRZ);
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            for (int i = 0; i < GlobalManager.Current.TotalRow; ++i)
            {
                for (int j = 0; j < GlobalManager.Current.TotalColumn; ++j)
                {
                    int k = 0;
                    if (i % 2 == 0)
                    {
                        k = j + i * GlobalManager.Current.TotalColumn;
                    }
                    else {
                        k = GlobalManager.Current.TotalColumn - 1 - j + i * GlobalManager.Current.TotalColumn; 
                    }
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Reserve, 0);
                    //移动到穴位
                    actionret = AkrAction.Current.Move(AxisName.PRX, GlobalManager.Current.recheckPoints[k].X, (int)AxisSpeed.PRX, (int)AxisAcc.PRX);//mm * 10000
                    if (CheckState(actionret == 0) == 1)
                    {
                        return false;
                    }
                    actionret = AkrAction.Current.Move(AxisName.PRY, GlobalManager.Current.recheckPoints[k].Y, (int)AxisSpeed.PRX, (int)AxisAcc.PRX);
                    if (CheckState(actionret == 0) == 1)
                    {
                        return false;
                    }
                    //康耐视复检
                    string command = "SN" + "sqcode" + $"+{k}," + $"{k}," + "Foam+Moudel," + "0.000,0.000,0.000";
                    TriggRecheckCamreaTFCSendData(RecheckCamreaProcessCommand.TFC, command);

                    Logger.WriteLog("CCD3 开始接受COGNEX的OK信息");
                    int cnt = 0;
                    while (Task_RecheckCamreaFunction.TriggRecheckCamreaready() != "OK" && cnt < 10)
                    {
                        string res = "接收到的信息是:" + Task_RecheckCamreaFunction.TriggRecheckCamreaready();
                        Logger.WriteLog(res);
                        Thread.Sleep(300);
                        cnt++;
                    }
                    Logger.WriteLog("CCD3 接受到COGNEX的OK信息");
                    if (CheckState(cnt < 10) == 1)
                    {
                        return false;
                    }
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Reserve, 1);
                    //获取康耐视数据
                    string Errcode = TriggRecheckCamreaTFCAcceptData(RecheckCamreaProcessCommand.TFC)[0].Errcode;
                    int cogres;
                    bool ret = int.TryParse(Errcode, out cogres);
                    Logger.WriteLog("CCD3 获取康耐视数据" + Errcode);
                    errorCode = ErrorCode.TimeOut;
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

                    modulestatecnt = modulestatecnt + 1;
                    Logger.WriteLog($"单点结束{i}");
                }
            }

            if (modulestatecnt < GlobalManager.Current.recheckPoints.Count)
            {
                GlobalManager.Current.isNGPallete = true;
            }
            return true;
        }

        public bool BoardOut()
        {
            int actionret;
            GlobalManager.Current.flag_RecheckStationHaveTray = 1 ;
            GlobalManager.Current.flag_TrayProcessCompletedNumber++;
            errorCode = ErrorCode.TimeOut;
            actionret = AkrAction.Current.Move(AxisName.PRZ, GlobalManager.Current.SafeZPos.Z, (int)AxisSpeed.PRZ, (int)AxisAcc.PRZ);
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            actionret = AkrAction.Current.Move(AxisName.PRX, GlobalManager.Current.StartPoint.X, (int)AxisSpeed.PRX, (int)AxisAcc.PRX);//mm * 10000
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            actionret = AkrAction.Current.Move(AxisName.PRY, GlobalManager.Current.StartPoint.Y, (int)AxisSpeed.PRY, (int)AxisAcc.PRY);
            if (CheckState(actionret == 0) == 1)
            {
                return false;
            }
            return true;
        }

        public override void AutoRun(CancellationToken token)
        {
            GlobalManager.Current.flag_RecheckTrayArrived = 0;
            try
            {
                while (true)
                {

                step1:
                    GlobalManager.Current.flag_RecheckTrayArrived = 0;
                    GlobalManager.Current.current_FuJian_step = 1;
                    if (GlobalManager.Current.FuJian_exit) break;

                step2:
                    GlobalManager.Current.current_FuJian_step = 2;
                    Tearing();
                    if (GlobalManager.Current.FuJian_exit)break;

                step3:
                    GlobalManager.Current.current_FuJian_step = 3;
                    Recheck();
                    if (GlobalManager.Current.FuJian_exit) break;
                step4:
                    GlobalManager.Current.current_FuJian_step = 4;
                    BoardOut();
                    if (GlobalManager.Current.FuJian_exit) break;
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
