using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AkribisFAM.Util;
using AkribisFAM.ViewModel;
using AkribisFAM.WorkStation;
using System.Windows; 

namespace AkribisFAM.Manager
{
    public enum ErrorCode { 
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

        WaitMotion,
        WaitIO,
        CognexErr,
    }

    public struct ErrorInfo
    {
        //Modify By YXW
        public string DateTime { get; set; }
        public string User { get; set; }
        public string ErrorCode { get; set; }
        public int Level { get; set; }

        public string Info { get; set; }

        public ErrorInfo(DateTime dT, string usr, ErrorCode eC)
        {
            DateTime = dT.ToString();
            User = usr;
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

        private ConcurrentStack<ErrorCode> ErrorStack = new ConcurrentStack<ErrorCode>();
        //public List<ErrorInfo> ErrorInfos = new List<ErrorInfo>();

        //Modify By YXW
        public ObservableCollection<ErrorInfo> ErrorInfos { get; set; } = new ObservableCollection<ErrorInfo>();

        //End Modify

        public int ErrorCnt = 0;
        public int ModbusErrCnt;
        public event Action UpdateErrorCnt;
        public static int ModbusErrCntLimit = 10;

        public void Insert(ErrorCode err)
        {
            ErrorStack.Push(err);
            ErrorCnt = ErrorStack.Count;

            //Modify By YXW
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                 ErrorInfos.Add(new ErrorInfo(DateTime.Now, GlobalManager.Current.username, err));
            });
            //END 
            //20250519 测试用 【史彦洋】 修改 Start
            if ((int)err > 0x00FF && StateManager.Current.State == StateManager.StateCode.RUNNING)
            {
                StateManager.Current.State = StateManager.StateCode.STOPPED;
                GlobalManager.Current.Lailiao_exit = true;
                GlobalManager.Current.Zuzhuang_exit = true;
                GlobalManager.Current.FuJian_exit = true;
                GlobalManager.Current.Reject_exit = true;
            }
            UpdateErrorCnt?.Invoke();
        }

        public void Pop()
        {
            ErrorCode result;
            if (ErrorStack.TryPop(out result))
            {
                Console.WriteLine("Removed element: " + result);
                ErrorCnt = ErrorStack.Count;
                UpdateErrorCnt?.Invoke();
            }
        }

        public void Clear()
        {
            ErrorStack.Clear();
            ErrorCnt = ErrorStack.Count;
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
