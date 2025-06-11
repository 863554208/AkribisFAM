using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AkribisFAM.Models;

namespace AkribisFAM.Manager
{
    public enum ErrorCode
    {
        NoError = 0x0000,
        //hardware
        AGM800Disconnect = 0x1000,
        IODisconnect = 0x2000,
        FeederDisconnect = 0x3000,
        LaserDisconnect = 0x4000,
        CamDisconnect = 0x5000,
        AGM800Err = 0x6000,
        IOErr = 0x7000,
        FeederErr = 0x8000,
        LaserErr = 0x9000,
        HardwareErr = 0xA000,
        ProcessErr = 0xB000,
        //operation
        FeederEmpty = 0x0100,
        DoorOpened = 0x0200,
        LaserDataErr = 0x0300,
        CCD1DataErr = 0x0400,
        CCD2DataErr = 0x0500,
        CCD3DataErr = 0x0600,
        TimeOut = 0x0700,
        //process warning
        NoInPallet = 0x0001,
        FeederLow = 0x0002,
        AssemblyNGFull = 0x0003,
        RecheckNGFull = 0x0004,
        YieldLow = 0x0005,
        HasNGPallet = 0x0006,
        BarocdeScan_Failed = 0x0007,
        BarocdeScan_NoBarcode = 0x0008,
        Laser_Failed = 0x0009,
        Cognex_DisConnected = 0x000A,
        OUT3_1_PNP_Gantry_vacuum1_Release_Error = 0x000B,
        OUT3_2_PNP_Gantry_vacuum2_Release_Error = 0x000C,
        WaitMotion = 0x000D,
        WaitIO = 0x000E,
        CognexErr = 0x000F,
        Nozzle1_feedback = 0x0010,
        TeachpointErr = 0x0011,
        LogicErr = 0x0012,
        motionErr = 0x0013,
        motionTimeoutErr = 0x0014,
        PneumaticErr = 0x0015,


        //Conveyor Error
        TrayLeaveSensorErr = 0x0016,
        GateReedSwitchTimeOut = 0x0017,
        LifterReedSwitchTimeOut = 0x0018,
        TrayPresentSensorTimeOut = 0x0019,
        IncomingTrayTimeOut = 0x0020,



        RejectCoverOpened = 0x0021,
        NGOccupied = 0x0022,
        MissingNGTray = 0x0023,

        ClawReedSwitchTimeOut = 0x0024,

    }

    public class ErrorInfo
    {
        //Modify By YXW
        public string DateTime { get; set; }
        public string User { get; set; }
        public string Lot { get; set; }
        public string ErrorCode { get; set; }
        public int Level { get; set; }

        public string Info { get; set; }
        public string Description { get; set; }

        public ErrorInfo(DateTime dT, string lot, string usr, ErrorCode eC, string description = "")
        {
            DateTime = dT.ToString();
            User = usr;
            Lot = lot;
            ErrorCode = "0x" + Convert.ToString((int)eC, 16);
            if ((int)eC > 0x0FFF)
            {
                Level = 1;
            }
            else if ((int)eC > 0x00FF)
            {
                Level = 2;
            }
            else
            {
                Level = 3;
            }
            Description = description;
            Info = eC.ToString();
        }
        //End Modify
    }

    public class ErrorManager
    {
        private static ErrorManager _instance;

        public static ErrorManager Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new ErrorManager();
                    }
                }
                return _instance;
            }
        }

        public bool IsAlarm => ErrorStack.Count > 0 || ErrorInfos.Count > 0;

        private ConcurrentStack<ErrorCode> ErrorStack = new ConcurrentStack<ErrorCode>();
        private ConcurrentStack<ErrorCode> ErrorHistory = new ConcurrentStack<ErrorCode>();

        //Modify By YXW
        public ObservableCollection<ErrorInfo> ErrorInfos { get; set; } = new ObservableCollection<ErrorInfo>();
        public ObservableCollection<ErrorInfo> ErrorHistoryInfos { get; set; } = new ObservableCollection<ErrorInfo>();
        //End Modify

        private Dictionary<int, string> ErrorLevel = new Dictionary<int, string>
        {
            { 1, "Critical" },
            { 2, "High" },
            { 3, "Low" },
        };
        private AlarmRecord alarm = new AlarmRecord();
        public int ErrorCnt => ErrorStack.Count;
        public int ModbusErrCnt;
        public event Action UpdateErrorCnt;
        public static int ModbusErrCntLimit = 10;

        public bool Insert(ErrorCode err, string descriptions = "")
        {
            ErrorStack.Push(err);
            ErrorHistory.Push(err);

            //Modify By YXW
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorInfos.Add(new ErrorInfo(DateTime.Now, "1", GlobalManager.Current.username, err, descriptions));
                ErrorHistoryInfos.Add(new ErrorInfo(DateTime.Now, "1", GlobalManager.Current.username, err, descriptions));
            });
            alarm.AlarmLevel = ErrorLevel[ErrorInfos[ErrorInfos.Count - 1].Level];
            alarm.AlarmMessage = ErrorInfos[ErrorInfos.Count - 1].Info;
            alarm.AlarmCode = ErrorInfos[ErrorInfos.Count - 1].ErrorCode;
            alarm.UserID = ErrorInfos[ErrorInfos.Count - 1].User;
            alarm.LotID = ErrorInfos[ErrorInfos.Count - 1].Lot;
            alarm.TimeOccurred = DateTime.Now;
            bool ret = App.DbManager.AddAlarm(alarm);
            UpdateErrorCnt?.Invoke();
            return false;
        }
        public void Pop()
        {
            if (ErrorInfos.Count == 0) { 
                return;
            }
            ErrorCode result;
            alarm.AlarmLevel = ErrorLevel[ErrorInfos[ErrorInfos.Count - 1].Level];
            alarm.AlarmMessage = ErrorInfos[ErrorInfos.Count - 1].Info;
            alarm.AlarmCode = ErrorInfos[ErrorInfos.Count - 1].ErrorCode;
            alarm.UserID = ErrorInfos[ErrorInfos.Count - 1].User;
            alarm.LotID = ErrorInfos[ErrorInfos.Count - 1].Lot;
            alarm.TimeResolved = DateTime.Now;
            bool ret = App.DbManager.AddAlarm(alarm);
            if (ErrorStack.TryPop(out result))
            {
                Console.WriteLine("Removed element: " + result);
                UpdateErrorCnt?.Invoke();
            }
        }

        public void Clear()
        {
            ErrorStack.Clear();
            UpdateErrorCnt?.Invoke();
        }
    }

    public class ErrorReportManager
    {
        public static ConcurrentStack<Exception> ErrorStack = new ConcurrentStack<Exception>();

        public static void Report(Exception ex)
        {
            var context = TaskContextManager.GetCurrentContext();  // 获取当前上下文
            var taskInfo = context != null ? $"[Task: {context.TaskId}, Station: {context.StationName}]" : "[No Context]";

            // 打印错误信息
            Console.WriteLine($"{taskInfo} [Error] {DateTime.Now}: {ex.Message}\n{ex.StackTrace}");

            // 将异常添加到stack
            ErrorStack.Push(ex);
        }
    }
}
