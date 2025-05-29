using System;
using System.Collections.Generic;
using System.Threading;

namespace AkribisFAM.CommunicationProtocol
{
    #region//基恩士测距
    public class KEYENCEDistance
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义基恩士测距追加
            public class SendKDistanceAppend
            {
                public string TestNumber; // 测量编号
                public string address;//测量地址
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {

            //定义接收到的数据
            public class AcceptKDistanceAppend
            {

                public string MeasurData;//测量数据  
            }
            
        }
        #endregion
    }
    #endregion


   public class Task_KEYENCEDistance
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
        public enum KEYENCEDistanceProcessCommand
        {
            MS,//定位载具
            Down//获取取料坐标
        }

        private static string InstructionHeader;//指令头

        public static bool SendMSData() //来料与基恩士测距交互MS自动触发流程
        {
            try
            {
                InstructionHeader = $"MS,";
                 //MS,0,1\n
                //组合字符串
                //string sendcommandData = $"{InstructionHeader}{sendmsdata}";
                string sendcommand = "MS,0,1";
                string endSymbol = "\r";
                string all = sendcommand + endSymbol;
                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand2(all);
                RecordLog("激光测距: " + sendcommand);
                if (!sendcommand_status)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                //bool sendcommand_status = this.VisionpositionfeedPushcommand("信息报错:"+ex.ToString());
                return false;
            }
        }

        public static bool SendResetData() //来料与基恩士测距交互MS自动触发流程
        {
            try
            {
                //MS,0,1\n
                //组合字符串
                //string sendcommandData = $"{InstructionHeader}{sendmsdata}";
                string sendcommand = "RA";
                string endSymbol = "\r";
                string all = sendcommand + endSymbol;
                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand2(all);
                RecordLog("复位结果: " + sendcommand);
                if (!sendcommand_status)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ex.ToString();
                //bool sendcommand_status = this.VisionpositionfeedPushcommand("信息报错:"+ex.ToString());
                return false;
            }
        }


        public static List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend> AcceptMSData()//来料与基恩士测距交互MS自动接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("收到测高数据: " + VisionAcceptData);
                ReceiveMessage(VisionAcceptData);
                if (!VisionAcceptData_status)
                {
                    return null;
                }

                Type camdowntype = typeof(KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend);
                List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend> list_positions = new List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend>();
                List<object> list = new List<object>();
                //解析字符串
                bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, list, camdowntype);
                if (!Analysis_status)
                {
                    return null;
                }
                if (list == null || list.Count == 0)
                {
                    return null;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    list_positions.Add((KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }

        public static void TriggMSStrClear()//清除客户端最后一条字符串
        {
            TCPNetworkManage.ClearLastMessage(ClientNames.lazer);
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
                VisionAcceptCommand = TCPNetworkManage.GetLastMessage(ClientNames.lazer);
                if (!string.IsNullOrEmpty(VisionAcceptCommand))
                {
                    break;//1秒之内读到数据跳出循环
                }
                Thread.Sleep(pollIntervalMs); // 避免死循环
            }


            if (VisionAcceptCommand == null)
            {
                return false;
            }
            return true;//需要添加代码修改(网络Socket读取字符串)
        }

        private static bool VisionpositionPushcommand(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.lazer, VisionSendCommand + "\n");
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }

        private static bool VisionpositionPushcommand2(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.lazer, VisionSendCommand);

            SendMessage(VisionSendCommand);
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}

