using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AkribisFAM.Util;
using AkribisFAM.ViewModel;
using AkribisFAM.WorkStation;

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
        //operation
        FeederEmpty = 0x0100,
        DoorOpened = 0x0200,
        LaserDataErr = 0x0300,
        CCD1DataErr = 0x0400,
        CCD2DataErr = 0x0500,
        CCD3DataErr = 0x0600,
        //process warning
        NoInPallet = 0x0001,
        FeederLow = 0x0002,
        AssemblyNGFull = 0x0003,
        RecheckNGFull = 0x0004,
        YieldLow = 0x0005,
        HasNGPallet = 0x0006
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

        public int ErrorCnt = 0;
        public event Action UpdateErrorCnt;

        public void Insert(ErrorCode err)
        {
            ErrorStack.Push(err);
            ErrorCnt = ErrorStack.Count;
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
