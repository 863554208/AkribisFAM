using System;
using System.Threading;
using AkribisFAM.Manager;

namespace AkribisFAM.CommunicationProtocol
{
    public class Task_Scanner
    {
        public delegate void OnCameraMessageSentEventHandler(object sender, string message);

        public static event OnCameraMessageSentEventHandler OnMessageSent;

        public static void SendMessage(string msg)
        {
            OnMessageSent.Invoke(null, msg);
        }
        public delegate void OnCameraMessageReceiveEventHandler(object sender, string message);

        public static event OnCameraMessageReceiveEventHandler OnMessageReceive;

        public static void ReceiveMessage(string msg)
        {
            OnMessageReceive.Invoke(null, msg);
        }
        public enum ScannerProcessCommand
        {
            Trigger,//触发扫码枪
            Down//预留
        }
        private static string InstructionHeader;//指令头

        public static bool TriggScannerSendData() //扫码与扫码枪交互Trigger自动触发流程
        {
            try
            {
                InstructionHeader = $"Trigger";//Trigger\r\n
                //组合字符串
                string sendcommandData = $"{InstructionHeader}";

                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发扫码: " + sendcommandData);
                if (!sendcommand_status)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
        }


        public static (string Result, ErrorCode Error) TriggScannerAcceptData()//扫码返回SN
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("扫码收到: " + VisionAcceptData);

                if (!VisionAcceptData_status)
                {
                    return (null, ErrorCode.BarocdeScan_NoBarcode);
                }
                return (VisionAcceptData, ErrorCode.NoError);
            }
            catch (Exception ex)
            {
                return (null, ErrorCode.BarocdeScan_Failed);
            }
        }

        public static void TriggScannerStrClear()//清除客户端最后一条字符串
        {
            TCPNetworkManage.ClearLastMessage(ClientNames.scanner);
        }

        private static void RecordLog(string message)//记录日志
        {
            // Logger.WriteLog(message);
        }

        private static bool VisionpositionAcceptcommand(out string VisionAcceptCommand)//从网络Socket读取字符串
        {
            VisionAcceptCommand = null;
            int timeoutMs = 1000;//1秒之后超时
            int pollIntervalMs = 50;//50毫秒线程延时
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                VisionAcceptCommand = TCPNetworkManage.GetLastMessage(ClientNames.scanner);
                if (!string.IsNullOrEmpty(VisionAcceptCommand))
                {
                    break;//1秒之内读到数据跳出循环
                }
                Thread.Sleep(pollIntervalMs); // 避免死循环
            }

            if (VisionAcceptCommand == null || VisionAcceptCommand == "")
            {
                return false;
            }

            if (VisionAcceptCommand.Contains("\r\n"))
            {
                VisionAcceptCommand = VisionAcceptCommand.Replace("\r\n", "");
            }
            ReceiveMessage(VisionAcceptCommand);
            return true;//需要添加代码修改(网络Socket读取字符串)
        }

        private static bool VisionpositionPushcommand(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.scanner, VisionSendCommand + "\r\n");
            SendMessage(VisionSendCommand);
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}
