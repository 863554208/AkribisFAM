using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AkribisFAM.CommunicationProtocol.AssUpCamrea.Pushcommand;



namespace AkribisFAM.CommunicationProtocol
{
    #region 组装定位上相机
    public class AssUpCamrea
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照位置
            public class SendTLTCamreaposition
            {
                public string SN; // SN序列号
                public string NozzleID;//吸嘴编号
                public string MaterialTypeN1;//原始物料名称(Foam)
                public string AcupointNumber;//穴位编号
                public string TargetMaterialName1;//目标物料名称(Foam->Module) 
                public string Photo_X1; // X 坐标
                public string Photo_Y1; // Y 坐标
                public string Photo_R1; // R 值
            }

            //定义拍照发送追加指令(获取贴装坐标)
            public class SendGTCommandAppend
            {
                public string NozzlelD1; //吸嘴编号
                public string RawMaterialName1; // 原始物料名称(Foam)
                public string CaveID1; //穴位编号
                public string TargetMaterialName1;//目标物料名称(Module)
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {
            
            //定义吸嘴贴装位置的目标坐标
            public class AcceptTLTRunnerPosition
            {
                public string Errcode;//错误代码，1为成功
                public string PartX1;//载具上物料特征坐标X1
                public string PartY1;//载具上物料特征坐标Y1
                public string PartR1;//载具上物料特征坐标R1
                public string Data2;//视觉收集数据
                public string TargetX; // 贴装目标坐标X
                public string TargetY; // 贴装目标坐标Y
                public string TargetR; // 贴装目标坐标R
            }

            //定义Cognex接收追加指令(获取贴装坐标)
            public class AcceptGTCommandAppend
            {
                public string Subareas_Errcode;//子穴错误代码，1为成功

                public string Unload_X; //放料坐标X
                public string Unload_Y; //放料坐标Y
                public string Unload_R; //放料坐标R
            }

            //相机准备状态
            public class TLTCamreaready
            {
                public string CamreaReadyFlag;//相机准备完成状态,"OK"代表完成
            }
        }
        #endregion
    }
    #endregion

    public class Task_AssUpCameraFunction
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
        public enum AssUpCameraProcessCommand
        {
            TLT,//定位载具
            GT//获取取料坐标
        }

        private static string InstructionHeader;//指令头

        public static bool TriggAssUpCamreaTLTSendData(AssUpCameraProcessCommand assUpCameraProcessCommand, List<SendTLTCamreaposition> list_positions) //组装定位拍照与相机交互TLT自动触发流程
        {
            try
            {
                InstructionHeader = $"TLT,CmdTLT_100,12,";
                //组合字符串
                string sendcommandData = $"{InstructionHeader}{StrClass1.BuildPacket(list_positions.Cast<object>().ToList())}";
                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发流道定位: " + sendcommandData);
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


        public static bool TriggAssUpCamreaGTSendData(AssUpCameraProcessCommand assUpCameraProcessCommand, List<SendGTCommandAppend> list_positions) //组装定位拍照与相机交互GT自动触发流程
        {
            try
            {
                InstructionHeader = $"GT,1,";
                //组合字符串
                string sendcommandData = $"{InstructionHeader}{StrClass1.BuildPacket(list_positions.Cast<object>().ToList())}";
                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发吸嘴贴装: " + sendcommandData);
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


        public static string TriggAssUpCamreaready()//读准备就绪
        {
            string VisionAcceptData = null;
            if (!VisionpositionAcceptcommand(out VisionAcceptData))
            {
                return null;
            }
            Type camdowntype = typeof(AssUpCamrea.Acceptcommand.TLTCamreaready);
            List<object> list_position = new List<object>();
            //解析字符串
            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, list_position, camdowntype);
            if (!Analysis_status)
            {
                return null;
            }
            //需要输出list_position
            return ((AssUpCamrea.Acceptcommand.TLTCamreaready)list_position[0]).CamreaReadyFlag;
        }

        public static List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition> TriggAssUpCamreaTLTAcceptData(AssUpCameraProcessCommand assUpCameraProcessCommand)//组装定位拍照与相机交互TLT接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("收到贴装坐标: " + VisionAcceptData);
                if (!VisionAcceptData_status)
                {
                    return null;
                }

                
                Type camdowntype = typeof(AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition);
                List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition> list_positions = new List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition>();
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
                    list_positions.Add((AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }


        public static List<AssUpCamrea.Acceptcommand.AcceptGTCommandAppend> TriggAssUpCamreaGTAcceptData(AssUpCameraProcessCommand assUpCameraProcessCommand)//组装定位拍照与相机交互GT接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("收到贴装坐标: " + VisionAcceptData);
                if (!VisionAcceptData_status)
                {
                    return null;
                }

                //GT接收指令头
                Type camdowntype = typeof(AssUpCamrea.Acceptcommand.AcceptGTCommandAppend);
                List<AssUpCamrea.Acceptcommand.AcceptGTCommandAppend> list_positions = new List<AssUpCamrea.Acceptcommand.AcceptGTCommandAppend>();
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
                    list_positions.Add((AssUpCamrea.Acceptcommand.AcceptGTCommandAppend)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }


        public static void TriggAssUpCamreaStrClear()//清除客户端最后一条字符串
        {
            TCPNetworkManage.ClearLastMessage(ClientNames.camera1_Feed);
        }

        private static void RecordLog(string message)//记录日志
        {
            // Logger.WriteLog(message);
        }

        private static bool VisionpositionAcceptcommand(out string VisionAcceptCommand)//从网络Socket读取字符串
        {
            VisionAcceptCommand = null;
            int timeoutMs = 5000;//5秒之后超时
            int pollIntervalMs = 50;//50毫秒线程延时
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                VisionAcceptCommand = TCPNetworkManage.GetLastMessage(ClientNames.camera1_Feed);
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
            ReceiveMessage(VisionAcceptCommand);
            //VisionAcceptCommand = "TLM,Cmd_100,2,1,1,2,1,132_133_130_126_999.999,1,133_135_132_128_999.999,1,2,2,1,139_141_136_128_999.999,1,131_133_129_127_999.999";
            return true;//需要添加代码修改(网络Socket读取字符串)
        }

        private static bool VisionpositionPushcommand(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.camera1_Feed, VisionSendCommand + "\r\n");
            SendMessage(VisionSendCommand);
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}
