using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.CommunicationProtocol
{
    #region 飞达取料上相机
    public class FeedUpCamrea
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照位置(定位飞达)
            public class SendTLMCamreaposition
            {
                public string SN1; // 物料的SN
                public string RawMaterialName1; // 原始物料名称  
                public string FOV; // 视野编号
                public string Photo_X1; // X 坐标
                public string Photo_Y1; // Y 坐标
                public string Photo_R1; // R 值
            }
            //定义拍照发送追加指令(获取取料坐标)
            public class SendGMCommandAppend
            {
                public string NozzlelD1; //吸嘴编号
                public string RawMaterialName1; // 原始物料名称
                public string FOV1; // 视野编号
                public string SubRegionID1;//子区域编号
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {
            //定义飞达取料位置的坐标(定位飞达)
            public class AcceptTLMFeedPosition
            {
                public string Errcode1;//错误代码，1为成功
                public string FOV1; // 视野编号
                public string Subareas_count;//子区域个数
                public string Subareas_Errcode11;//1号穴位错误代码，1为成功
                public string Data11;//11穴位数据收集,NA无数据收集
                public string Subareas_Errcode22;//2号穴位错误代码，1为成功
                public string Data22;//22穴位数据收集,NA无数据收集
            }
            //定义Cognex接收追加指令(获取取料坐标)
            public class AcceptGMCommandAppend
            {
                public string Subareas_Errcode;//子穴错误代码，1为成功
                public string NozzlelD1; //吸嘴编号
                public string Pick_X; //取料坐标X
                public string Pick_Y; //取料坐标Y
                public string Pick_R; //取料坐标R
            }
            //相机准备状态
            public class TLMCamreaready
            {
                public string CamreaReadyFlag;//相机准备完成状态,"OK"代表完成
            }
        }
        #endregion
    }
    #endregion

    class Task_FeedupCameraFunction
    {
        public enum FeedupCameraProcessCommand
        {
            TLM,//定位飞达
            GM//获取取料坐标
        }

        private static string InstructionHeader;//指令头

        public static bool TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand feedupCameraProcessCommand, List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> list_positions) //飞达拍照与相机交互TLM自动触发流程
        {
            try
            {
                InstructionHeader = $"TLM,Cmd_100,2,";// 模块头+指令编号+拍照次数  
                //组合字符串
                string sendcommandData = $"{InstructionHeader}{StrClass1.BuildPacket(list_positions.Cast<object>().ToList())}";

                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发飞达定位: " + sendcommandData);
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


        public static bool TriggFeedUpCamreaGMSendData(FeedupCameraProcessCommand feedupCameraProcessCommand, List<FeedUpCamrea.Pushcommand.SendGMCommandAppend> list_positions) //飞达拍照与相机交互GM自动触发流程
        {
            try
            {
                InstructionHeader = $"GM,1,";// 模块头+计算取料坐标个数 
                //组合字符串
                string sendcommandData = $"{InstructionHeader}{StrClass1.BuildPacket(list_positions.Cast<object>().ToList())}";

                //发送字符串到Socket
                bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                RecordLog("触发飞达取料: " + sendcommandData);
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

        public static List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand feedupCameraProcessCommand)//飞达拍照与相机交互TLM接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("收到取料坐标: " + VisionAcceptData);
                if (!VisionAcceptData_status)
                {
                    return null;
                }

                Type camdowntype = typeof(FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition);
                List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> list_positions = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
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
                    list_positions.Add((FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {

                ex.ToString();
                return null;
            }
        }


        public static List<FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend> TriggFeedUpCamreaGMAcceptData(FeedupCameraProcessCommand feedupCameraProcessCommand)//飞达拍照与相机交互GM接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("收到取料坐标: " + VisionAcceptData);
                if (!VisionAcceptData_status)
                {
                    return null;
                }

                Type camdowntype = typeof(FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend);
                List<FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend> list_positions = new List<FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend>();

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
                    list_positions.Add((FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend)list[i]);
                }
                return list_positions;
            }
            catch (Exception ex)
            {

                ex.ToString();
                return null;
            }
        }


        public static string TriggFeedUpCamreaready()
        {
            string VisionAcceptData = null;
            if (!VisionpositionAcceptcommand(out VisionAcceptData))
            {
                return null;
            }
           
            Type camdowntype = typeof(FeedUpCamrea.Acceptcommand.TLMCamreaready);
            List<object> list_position = new List<object>();
            //解析字符串
            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData,list_position, camdowntype);
            if (!Analysis_status)
            {
                return null;
            }
            //需要输出list_position
            return ((FeedUpCamrea.Acceptcommand.TLMCamreaready)list_position[0]).CamreaReadyFlag;
        }


        public static void TriggFeedUpCamreaStrClear()//清除客户端最后一条字符串
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
            int timeoutMs = 1000;//1秒之后超时
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

            //VisionAcceptCommand = "TLM,Cmd_100,2,1,1,2,1,132_133_130_126_999.999,1,133_135_132_128_999.999,1,2,2,1,139_141_136_128_999.999,1,131_133_129_127_999.999";
            return true;//需要添加代码修改(网络Socket读取字符串)
        }

        private static bool VisionpositionPushcommand(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.camera1_Feed, VisionSendCommand + "\r\n");
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}
