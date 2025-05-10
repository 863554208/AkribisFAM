using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AkribisFAM.CommunicationProtocol
{
    #region 相机复位
    public class ResetCamrea
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照发送头部指令
            public class SendSetStatCommandTop
            {
                public string SetStation; //设置工站名
            }
            //定义拍照位置
            public class SendSetStatCamreaposition
            {
                public string AE_Station; // AE-PDCA站点
                public string ProjectName;//项目名称
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {
            //定义接受Cognex头部指令
            public class AcceptSetStatCommandTop
            {
                public string SetStation; //设置工站名    
            }
            //定义工站status数据
            public class AcceptSetStatRecheckAppend
            {
                public string status; // 设定成功
            }
        }
        #endregion
    }
    #endregion

    class Task_ResetCamreaFunction
    {
        public enum ResetCamreaProcessCommand
        {
            SetStation,//机台复位时发送
            Down//预留
        }

        private static string InstructionHeader;//指令头

        public static bool TriggResetCamreaSendData(ResetCamreaProcessCommand resetCamreaProcessCommand) //机台复位时与相机交互自动触发流程
        {
            try
            {
                switch ((int)resetCamreaProcessCommand)
                {
                    case (int)ResetCamreaProcessCommand.SetStation://SetStation触发指令
                        {
                            //SetStation,LXSZ_B01-4FPAM-02_4_AE-40,FAM1-BZ
                            //SetStation触发指令头
                            ResetCamrea.Pushcommand.SendSetStatCommandTop sendSetStatCommandTop1 = new ResetCamrea.Pushcommand.SendSetStatCommandTop();
                            sendSetStatCommandTop1.SetStation = "SetStation";
                            InstructionHeader = $"{sendSetStatCommandTop1.SetStation},";

                            //AE-PDCA站点+项目名称
                            List<ResetCamrea.Pushcommand.SendSetStatCamreaposition> sendSetStatCamreapositions = new List<ResetCamrea.Pushcommand.SendSetStatCamreaposition>();
                            ResetCamrea.Pushcommand.SendSetStatCamreaposition sendSetStatCamreaposition1= new ResetCamrea.Pushcommand.SendSetStatCamreaposition();
                            
                            sendSetStatCamreaposition1.AE_Station = "LXSZ_B01-4FPAM-02_4_AE-40";
                            sendSetStatCamreaposition1.ProjectName = "FAM1-BZ";
                            sendSetStatCamreapositions.Add(sendSetStatCamreaposition1);

                            //组合字符串
                            string sendcommandData = StrClass1.BuildPacket(sendSetStatCommandTop1, sendSetStatCamreapositions.Cast<object>().ToList());

                            //发送字符串到Socket
                            bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                            RecordLog("触发机台复位时发送: " + sendcommandData);
                            if (!sendcommand_status)
                            {
                                return false;
                            }
                        }
                        break;
                    case (int)ResetCamreaProcessCommand.Down://预留指令
                        {
                            ////SetStation触发指令头
                            //ResetCamrea.Pushcommand.SendSetStatCommandTop sendSetStatCommandTop1 = new ResetCamrea.Pushcommand.SendSetStatCommandTop();
                            //sendSetStatCommandTop1.SetStation = "SetStation";
                            //InstructionHeader = $"{sendSetStatCommandTop1},";

                            ////AE-PDCA站点+项目名称
                            //List<ResetCamrea.Pushcommand.SendSetStatCamreaposition> sendSetStatCamreapositions = new List<ResetCamrea.Pushcommand.SendSetStatCamreaposition>();
                            //ResetCamrea.Pushcommand.SendSetStatCamreaposition sendSetStatCamreaposition1 = new ResetCamrea.Pushcommand.SendSetStatCamreaposition();

                            //sendSetStatCamreaposition1.AE_Station = "LXSZ_B01-4FPAM-02_4_AE-40";
                            //sendSetStatCamreaposition1.ProjectName = "FAM1-BZ";
                            //sendSetStatCamreapositions.Add(sendSetStatCamreaposition1);

                            ////组合字符串
                            //string sendcommandData = StrClass1.BuildPacket(sendSetStatCommandTop1, sendSetStatCamreapositions.Cast<object>().ToList());

                            ////发送字符串到Socket
                            //bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                            //RecordLog("触发机台复位时发送: " + sendcommandData);
                            //if (!sendcommand_status)
                            //{
                            //    return false;
                            //}
                        }
                        break;
                    default:
                        {
                            //发送字符串到Socket
                            bool sendcommand_status = VisionpositionPushcommand("触发指令有误");
                            if (!sendcommand_status)
                            {
                                return false;
                            }
                        }
                        break;
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

        public static bool TriggResetCamreaAcceptData(ResetCamreaProcessCommand resetCamreaProcessCommand, out List<object> list_position)//机台复位时与相机交互接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("机台复位时收到: " + VisionAcceptData);
                list_position = null;

                if (!VisionAcceptData_status)
                {
                    return false;
                }
                switch ((int)resetCamreaProcessCommand)
                {
                    case (int)ResetCamreaProcessCommand.SetStation://SetStation接收指令
                        {
                            //SetStation接收指令头
                            ResetCamrea.Acceptcommand.AcceptSetStatCommandTop acceptSetStatCommandTop1 = new ResetCamrea.Acceptcommand.AcceptSetStatCommandTop();
                            Type camdowntype = typeof(ResetCamrea.Acceptcommand.AcceptSetStatRecheckAppend);
                            //List<object> list_position = new List<object>();
                            list_position = new List<object>();
                            //解析字符串
                            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, acceptSetStatCommandTop1, list_position, camdowntype);
                            if (!Analysis_status)
                            {
                                return false;
                            }
                            //需要输出list_position
                        }
                        break;
                    case (int)ResetCamreaProcessCommand.Down://预留指令
                        {
                            ////SetStation接收指令头
                            //ResetCamrea.Acceptcommand.AcceptSetStatCommandTop acceptSetStatCommandTop1 = new ResetCamrea.Acceptcommand.AcceptSetStatCommandTop();
                            //Type camdowntype = typeof(ResetCamrea.Acceptcommand.AcceptSetStatRecheckAppend);
                            ////List<object> list_position = new List<object>();
                            //list_position = new List<object>();
                            ////解析字符串
                            //bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, acceptSetStatCommandTop1, list_position, camdowntype);
                            //if (!Analysis_status)
                            //{
                            //    return false;
                            //}
                            ////需要输出list_position
                        }
                        break;
                    default:
                        {
                            if (true)
                            {
                                //"接受指令有误"
                                return false;
                            }
                        }
                }
                return true;
            }
            catch (Exception ex)
            {
                list_position = null;
                ex.ToString();
                return false;
            }
        }

        public static void TriggResetCamreaStrClear()//清除客户端最后一条字符串
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
            TCPNetworkManage.InputLoop(ClientNames.camera1_Feed, VisionSendCommand);
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}
