using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.CommunicationProtocol
{
    #region 吸嘴训练
    public class TTNCamrea
    {
        #region//发送的指令
        public class Pushcommand
        {

            //定义拍照位置
            public class SendTTNCamreaposition
            {
                public string SN1; // TTN_吸嘴编号_时间戳
                public string NozzleID1;//吸嘴编号
                public string RawMaterialName1;//原始物料名称(Foam)
                public string Photo_X1; // 拍照时的运动坐标X
                public string Photo_Y1; // 拍照时的运动坐标Y
                public string Photo_R1; // 拍照时的运动坐标R，发0,0,0即可
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {

            //定义复检数据
            public class AcceptTTNCamreaAppend
            {
                public string Errcode1;//错误代码，1为成功
                public string PartX1;//吸嘴特征坐标X1
                public string PartY1;//吸嘴特征坐标Y1
                public string PartA1;//吸嘴特征坐标A1
                public string Data1;//视觉收集数据，分号区分
            }
            //相机准备状态
            public class TTNCamreaready
            {
                public string CamreaReadyFlag;//相机准备完成状态,"OK"代表完成
            }
        }
        #endregion
    }
    #endregion

    class Task_TTNCamreaFunction
    {
        public enum TTNProcessCommand
        {
            TTN,//吸嘴训练
            Down//预留
        }

        private static string InstructionHeader;//指令头

        public static bool TriggTTNCamreaSendData(TTNProcessCommand tTNProcessCommand, string SendData) //吸嘴训练拍照与相机交互TTN自动触发流程
        {
            try
            {
                InstructionHeader = $"TTN,1,1,";
                //组合字符串
                string sendcommandData = $"{InstructionHeader}{SendData}";
                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发吸嘴训练: " + sendcommandData);
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

        public static string TriggTTNCamreaready()//读准备就绪
        {
            string VisionAcceptData = null;
            if (!VisionpositionAcceptcommand(out VisionAcceptData))
            {
                return null;
            }
            //TTN接收指令头

            Type camdowntype = typeof(TTNCamrea.Acceptcommand.TTNCamreaready);
            List<object> list_position = new List<object>();
            //解析字符串
            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, list_position, camdowntype);
            if (!Analysis_status)
            {
                return null;
            }
            //需要输出list_position
            return ((TTNCamrea.Acceptcommand.TTNCamreaready)list_position[0]).CamreaReadyFlag;
        }

        public static List<TTNCamrea.Acceptcommand.AcceptTTNCamreaAppend> TriggTTNCamreaAcceptData(TTNProcessCommand tTNProcessCommand)//吸嘴训练拍照与相机交互TTN接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("吸嘴训练收到: " + VisionAcceptData);

                if (!VisionAcceptData_status)
                {
                    return null;
                }


                //TTN接收指令头

                Type camdowntype = typeof(TTNCamrea.Acceptcommand.AcceptTTNCamreaAppend);
                List<TTNCamrea.Acceptcommand.AcceptTTNCamreaAppend> list_positions = new List<TTNCamrea.Acceptcommand.AcceptTTNCamreaAppend>();

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
                    list_positions.Add((TTNCamrea.Acceptcommand.AcceptTTNCamreaAppend)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {

                ex.ToString();
                return null;
            }
        }

        public static void TriggTTNCamreaStrClear()//清除客户端最后一条字符串
        {
            TCPNetworkManage.ClearLastMessage(ClientNames.camera2);
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
                VisionAcceptCommand = TCPNetworkManage.GetLastMessage(ClientNames.camera2);
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
            VisionAcceptCommand = VisionAcceptCommand.Replace("\r\n", "");
            //VisionAcceptCommand = "TLM,Cmd_100,2,1,1,2,1,132_133_130_126_999.999,1,133_135_132_128_999.999,1,2,2,1,139_141_136_128_999.999,1,131_133_129_127_999.999";
            return true;//需要添加代码修改(网络Socket读取字符串)
        }

        private static bool VisionpositionPushcommand(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.camera2, VisionSendCommand + "\r\n");
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}
