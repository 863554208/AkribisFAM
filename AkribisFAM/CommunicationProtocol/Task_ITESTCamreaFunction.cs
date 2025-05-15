using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AkribisFAM.CommunicationProtocol.ITESTCamrea.Acceptcommand;


namespace AkribisFAM.CommunicationProtocol
{
    #region 相机ITEST功能
    public class ITESTCamrea
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照位置
            public class SendRecordCamreaposition
            {
                public string ImagePath; //ITEST图像存储路径
                public string Num; //存储数量
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {

            //定义ITEST status数据
            public class AcceptRecordRecheckAppend
            {
                public string status; // 存储成功
                public string ImagePath; // 存储路径
            }
        }
        #endregion
    }
    #endregion

    class Task_ITESTCamreaFunction
    {

        public enum ITESTCamreaProcessCommand
        {
            RecordImage,//ITEST功能需要,不开启不需要发送
            Down//预留
        }

        private static string InstructionHeader;//指令头

        public static bool TriggITESTCamreaSendData(ITESTCamreaProcessCommand iTESTCamreaProcessCommand, List<AcceptRecordRecheckAppend> list_positions) //机台复位时与相机交互自动触发流程
        {
            try
            {
                //RecordImage,F:\itestimage,1
                InstructionHeader = $"RecordImage,";
                ////ITEST图像存储路径+存储数量
                //List<ITESTCamrea.Pushcommand.SendRecordCamreaposition> sendRecordCamreapositions = new List<ITESTCamrea.Pushcommand.SendRecordCamreaposition>();
                //ITESTCamrea.Pushcommand.SendRecordCamreaposition sendRecordCamreaposition1= new ITESTCamrea.Pushcommand.SendRecordCamreaposition();

                //sendRecordCamreaposition1.ImagePath = @"F:\\itestimage";
                //sendRecordCamreaposition1.Num = "1";
                //sendRecordCamreapositions.Add(sendRecordCamreaposition1);

                //组合字符串
                string sendcommandData = $"{InstructionHeader}{StrClass1.BuildPacket(list_positions.Cast<object>().ToList())}";

                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发ITEST功能时发送: " + sendcommandData);
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

        public static List<ITESTCamrea.Acceptcommand.AcceptRecordRecheckAppend> TriggResetCamreaAcceptData(ITESTCamreaProcessCommand iTESTCamreaProcessCommand)//机台复位时与相机交互接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("机台ITEST功能时收到: " + VisionAcceptData);
                if (!VisionAcceptData_status)
                {
                    return null;
                }

                Type camdowntype = typeof(ITESTCamrea.Acceptcommand.AcceptRecordRecheckAppend);
                List<ITESTCamrea.Acceptcommand.AcceptRecordRecheckAppend> list_positions = new List<ITESTCamrea.Acceptcommand.AcceptRecordRecheckAppend>();
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
                    list_positions.Add((ITESTCamrea.Acceptcommand.AcceptRecordRecheckAppend)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {

                ex.ToString();
                return null;
            }
        }

        public static void TriggResetCamreaStrClear()//清除客户端最后一条字符串
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
