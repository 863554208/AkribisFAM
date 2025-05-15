using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.CommunicationProtocol
{   
    #region 基恩士测距
    public class KEDistance
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照位置
            public class SendMSAppend
            {
                public string Controlnumber; //测量控制编号
                public string MeasureAddress;//测量地址
                
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {

            //定义测量高度数据
            public class AcceptMSAppend
            {
                
                public string HeightData;//高度数据

            }
        }
        #endregion
    }
#endregion

    class Task_KEDistance
    {
        public enum KEDistanceProcessCommand
        {
            MS,//测量高度数据指令头
            Down//预留
        }

        private static string InstructionHeader;//指令头

        public static bool MSSendData(KEDistanceProcessCommand kEDistanceProcessCommand,List<KEDistance.Pushcommand.SendMSAppend> list_positions) //测量高度与基恩士交互MS自动触发流程
        {
            try
            {
                InstructionHeader = $"MS,";
                //组合字符串
                string sendcommandData = $"{InstructionHeader}{StrClass1.BuildPacket(list_positions.Cast<object>().ToList())}";

                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发测量高度: " + sendcommandData);
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

        public static List<KEDistance.Acceptcommand.AcceptMSAppend> MSAcceptData(KEDistanceProcessCommand kEDistanceProcessCommand)//复检相机拍照与相机交互MS接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("测量高度数据收到: " + VisionAcceptData);

                if (!VisionAcceptData_status)
                {
                    return null;
                }

                Type camdowntype = typeof(RecheckCamrea.Acceptcommand.AcceptTFCRecheckAppend);
                List<RecheckCamrea.Acceptcommand.AcceptTFCRecheckAppend> list_positions = new List<RecheckCamrea.Acceptcommand.AcceptTFCRecheckAppend>();

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
                    list_positions.Add((RecheckCamrea.Acceptcommand.AcceptTFCRecheckAppend)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {

                ex.ToString();
                return null;
            }
        }

        public static void TriggRecheckCamreaStrClear()//清除客户端最后一条字符串
        {
            TCPNetworkManage.ClearLastMessage(ClientNames.camera3);
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
                VisionAcceptCommand = TCPNetworkManage.GetLastMessage(ClientNames.camera3);
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

            //VisionAcceptCommand = "TLM,Cmd_100,2,1,1,2,1,132_133_130_126_999.999,1,133_135_132_128_999.999,1,2,2,1,139_141_136_128_999.999,1,131_133_129_127_999.999";
            return true;//需要添加代码修改(网络Socket读取字符串)
        }

        private static bool VisionpositionPushcommand(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.camera3, VisionSendCommand + "\r\n");
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}
