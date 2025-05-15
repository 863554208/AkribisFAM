using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;


namespace AkribisFAM.CommunicationProtocol
{
    #region 精定位下相机
    public class PrecisionDownCamrea
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照位置
            public class SendTLNCamreaposition
            {
                public string SN; // SN序列号
                public string NozzleID;//吸嘴编号
                public string RawMaterialName; // 吸嘴上的物料  
                public string CaveID;//贴装位未拍照发0
                public string TargetMaterialName1;//目标物料名称(Foam->Module)
                public string Photo_X1; // X 坐标
                public string Photo_Y1; // Y 坐标
                public string Photo_R1; // R 值
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {
            //定义物料精定位的坐标
            public class AcceptTLNDownPosition
            {
                public string Errcode;//错误代码，1为成功
                public string PartX1;//吸嘴上物料特征坐标X1
                public string PartY1;//吸嘴上物料特征坐标Y1
                public string PartR1;//吸嘴上物料特征坐标R1
                public string Data1;//视觉收集数据
                public string X1; // 贴装目标坐标X
                public string Y1; // 贴装目标坐标Y
                public string R1; // 贴装目标坐标R
            }
            //相机准备状态
            public class TLNCamreaready
            {
                public string CamreaReadyFlag;//相机准备完成状态,"OK"代表完成
            }
        }
        #endregion
    }
    #endregion

    class Task_PrecisionDownCamreaFunction
    {
        public enum PrecisionDownCamreaProcessCommand
        {
            TLN,//定位吸嘴
            Down//预留
        }

        private static string InstructionHeader;//指令头


        public static bool TriggDownCamreaTLNSendData(PrecisionDownCamreaProcessCommand precisionDownCamreaProcessCommand, List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> list_positions) //下相机拍照与相机交互TLN自动触发流程
        {
            try
            {
                InstructionHeader = $"TLN,Cmd_100,4,";

                ////SN1+物料名称+视野编号+X+Y+R
                //List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> sendTLNCamreapositions = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();
                //PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition sendTLNCamreaposition1 = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition();

                //sendTLNCamreaposition1.SN = "LocateNozzle_1_20250418152027";
                //sendTLNCamreaposition1.NozzleID= "1";//吸嘴编号
                //sendTLNCamreaposition1.RawMaterialName = "Foam";
                //sendTLNCamreaposition1.CaveID = "0";
                //sendTLNCamreaposition1.TargetMaterialName1 = "Foam->Moudel";
                //sendTLNCamreaposition1.Photo_X1 = "256.890";
                //sendTLNCamreaposition1.Photo_Y1 = "345.445";
                //sendTLNCamreaposition1.Photo_R1 = "67.456";
                //sendTLNCamreapositions.Add(sendTLNCamreaposition1);

                ////SN2+物料名称+视野编号+X+Y+R
                //PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition sendTLNCamreaposition2 = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition();

                //sendTLNCamreaposition2.SN = "LocateNozzle_2_20250418152027";
                //sendTLNCamreaposition2.NozzleID = "2";//吸嘴编号
                //sendTLNCamreaposition2.RawMaterialName = "Foam";
                //sendTLNCamreaposition2.CaveID = "0";
                //sendTLNCamreaposition2.TargetMaterialName1 = "Foam->Moudel";
                //sendTLNCamreaposition2.Photo_X1 = "256.890";
                //sendTLNCamreaposition2.Photo_Y1 = "345.445";
                //sendTLNCamreaposition2.Photo_R1 = "67.456";
                //sendTLNCamreapositions.Add(sendTLNCamreaposition2);

                ////SN3+物料名称+视野编号+X+Y+R
                //PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition sendTLNCamreaposition3 = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition();

                //sendTLNCamreaposition3.SN = "LocateNozzle_3_20250418152027";
                //sendTLNCamreaposition3.NozzleID = "3";//吸嘴编号
                //sendTLNCamreaposition3.RawMaterialName = "Foam";
                //sendTLNCamreaposition3.CaveID = "0";
                //sendTLNCamreaposition3.TargetMaterialName1 = "Foam->Moudel";
                //sendTLNCamreaposition3.Photo_X1 = "256.890";
                //sendTLNCamreaposition3.Photo_Y1 = "345.445";
                //sendTLNCamreaposition3.Photo_R1 = "67.456";
                //sendTLNCamreapositions.Add(sendTLNCamreaposition3);

                ////SN4+物料名称+视野编号+X+Y+R
                //PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition sendTLNCamreaposition4 = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition();

                //sendTLNCamreaposition4.SN = "LocateNozzle_4_20250418152027";
                //sendTLNCamreaposition4.NozzleID = "4";//吸嘴编号
                //sendTLNCamreaposition4.RawMaterialName = "Foam";
                //sendTLNCamreaposition4.CaveID = "0";
                //sendTLNCamreaposition4.TargetMaterialName1 = "Foam->Moudel";
                //sendTLNCamreaposition4.Photo_X1 = "256.890";
                //sendTLNCamreaposition4.Photo_Y1 = "345.445";
                //sendTLNCamreaposition4.Photo_R1 = "67.456";
                //sendTLNCamreapositions.Add(sendTLNCamreaposition4);

                //组合字符串
                string sendcommandData = $"{InstructionHeader}{StrClass1.BuildPacket(list_positions.Cast<object>().ToList())}";

                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发下相机精定位定位: " + sendcommandData);
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

        public static string TriggDownCamreaready()//读准备就绪
        {
            string VisionAcceptData = null;
            if (!VisionpositionAcceptcommand(out VisionAcceptData))
            {
                return null;
            }

            Type camdowntype = typeof(PrecisionDownCamrea.Acceptcommand.TLNCamreaready);
            List<object> list_position = new List<object>();
            //解析字符串
            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, list_position, camdowntype);
            if (!Analysis_status)
            {
                return null;
            }
            //需要输出list_position
            return ((PrecisionDownCamrea.Acceptcommand.TLNCamreaready)list_position[0]).CamreaReadyFlag;
        }

        public static List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> TriggDownCamreaTLNAcceptData(PrecisionDownCamreaProcessCommand precisionDownCamreaProcessCommand)//下相机拍照与相机交互TLN接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("下相机收到取料坐标: " + VisionAcceptData);
                if (!VisionAcceptData_status)
                {
                    return null;
                }
                Type camdowntype = typeof(PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition);
                List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> list_positions = new List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition>();
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
                    list_positions.Add((PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }

        public static void TriggDownCamreaStrClear()//清除客户端最后一条字符串
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