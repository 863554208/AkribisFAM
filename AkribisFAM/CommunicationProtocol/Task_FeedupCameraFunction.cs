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
            //定义拍照发送头部指令(定位飞达)
            public class SendTLMCommandTop
            {
                public string TLM; // 模块头
                public string CmdID;  // 指令编号
                public string CamreaCount; // 拍照次数  
            }
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

            //定义拍照发送头部指令(获取取料坐标)
            public class SendGMCommandTop
            {
                public string GM; // 模块头
                public string PickNumber;  // 计算取料坐标个数

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
            //定义接受Cognex头部指令(定位飞达)
            public class AcceptTLMCommandTop
            {
                public string TLM; // 模块头
                public string CmdID;  // 指令编号
                public string CamreaCount; // 拍照次数
            }
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

            //定义Cognex头部接收指令(获取取料坐标)
            public class AcceptGMCommandTop
            {
                public string GM; // 模块头
                public string PickNumber;  // 计算取料坐标个数

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

        //public static bool TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand feedupCameraProcessCommand,List<object> list_positions) //飞达拍照与相机交互自动触发流程
        //{
        //    try
        //    {
        //        switch ((int)feedupCameraProcessCommand)
        //        {
        //            case (int)FeedupCameraProcessCommand.TLM://TLM触发指令
        //                {
        //                    //TLM触发指令头
        //                    //FeedUpCamrea.Pushcommand.SendTLMCommandTop sendTLMCommandTop1 = new FeedUpCamrea.Pushcommand.SendTLMCommandTop();
        //                    //sendTLMCommandTop1.TLM = "TLM";
        //                    //sendTLMCommandTop1.CmdID = "Cmd_100";
        //                    //sendTLMCommandTop1.CamreaCount = "2";
        //                    //InstructionHeader = $"{sendTLMCommandTop1.TLM},{sendTLMCommandTop1.CmdID},{sendTLMCommandTop1.CamreaCount},";

        //                    FeedUpCamrea.Pushcommand.SendTLMCommandTop sendTLMCommandTop1 = new FeedUpCamrea.Pushcommand.SendTLMCommandTop();
        //                    FeedUpCamrea.Pushcommand.SendTLMCommandTop.TLM = "TLM";
        //                    FeedUpCamrea.Pushcommand.SendTLMCommandTop.CmdID = "Cmd_100";
        //                    FeedUpCamrea.Pushcommand.SendTLMCommandTop.CamreaCount = "2";
        //                    InstructionHeader = $"{FeedUpCamrea.Pushcommand.SendTLMCommandTop.TLM},{FeedUpCamrea.Pushcommand.SendTLMCommandTop.CmdID},{FeedUpCamrea.Pushcommand.SendTLMCommandTop.CamreaCount},";

        //                    ////SN1+物料名称+视野编号+X+Y+R
        //                    //List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> sendTLMCamreapositions = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();
        //                    //FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition();
        //                    //sendTLMCamreaposition1.SN1 = "Pick_0_20250418152023_1";
        //                    //sendTLMCamreaposition1.RawMaterialName1 = "Foam";
        //                    //sendTLMCamreaposition1.FOV = "1";
        //                    //sendTLMCamreaposition1.Photo_X1 = "256.890";
        //                    //sendTLMCamreaposition1.Photo_Y1 = "345.445";
        //                    //sendTLMCamreaposition1.Photo_R1 = "67.456";
        //                    //sendTLMCamreapositions.Add(sendTLMCamreaposition1);
        //                    ////SN2+物料名称+视野编号+X+Y+R
        //                    //FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition2 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition();
        //                    //sendTLMCamreaposition2.SN1 = "Pick_0_20250418152023_2";
        //                    //sendTLMCamreaposition2.RawMaterialName1 = "Foam2";
        //                    //sendTLMCamreaposition2.FOV = "2";
        //                    //sendTLMCamreaposition2.Photo_X1 = "256.8902";
        //                    //sendTLMCamreaposition2.Photo_Y1 = "345.4452";
        //                    //sendTLMCamreaposition2.Photo_R1 = "67.4562";
        //                    //sendTLMCamreapositions.Add(sendTLMCamreaposition2);
        //                    //组合字符串
        //                   // string sendcommandData = StrClass1.BuildPacket(sendTLMCommandTop1, sendTLMCamreaposition2.Cast<object>().ToList());

        //                    //组合字符串
        //                    string sendcommandData = StrClass1.BuildPacket(sendTLMCommandTop1, list_positions.Cast<object>().ToList());

        //                    //发送字符串到Socket
        //                    bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
        //                    RecordLog("触发飞达定位: " + sendcommandData);
        //                    if (!sendcommand_status)
        //                    {
        //                        return false;
        //                    }
        //                }
        //                break;
        //            case (int)FeedupCameraProcessCommand.GM://GM触发指令
        //                {
        //                    //GM触发指令头
        //                    FeedUpCamrea.Pushcommand.SendGMCommandTop sendGMCommandTop1 = new FeedUpCamrea.Pushcommand.SendGMCommandTop();
        //                    sendGMCommandTop1.GM = "GM";
        //                    sendGMCommandTop1.PickNumber = "1";
        //                    InstructionHeader = $"{sendGMCommandTop1.GM},{sendGMCommandTop1.PickNumber},";
        //                    ////吸嘴编号+物料名称+视野编号+子区域编号
        //                    //List<FeedUpCamrea.Pushcommand.SendGMCommandAppend> sendGMCommandAppends = new List<FeedUpCamrea.Pushcommand.SendGMCommandAppend>();
        //                    //FeedUpCamrea.Pushcommand.SendGMCommandAppend sendGMCommandAppend = new FeedUpCamrea.Pushcommand.SendGMCommandAppend();
        //                    //sendGMCommandAppend.NozzlelD1 = "4";
        //                    //sendGMCommandAppend.RawMaterialName1 = "Foam";
        //                    //sendGMCommandAppend.FOV1 = "1";
        //                    //sendGMCommandAppend.SubRegionID1 = "3";
        //                    //sendGMCommandAppends.Add(sendGMCommandAppend);
        //                    //组合字符串
        //                    // string sendcommandData = StrClass1.BuildPacket(sendTLMCommandTop1, sendGMCommandAppend.Cast<object>().ToList());


        //                    //组合字符串
        //                    string sendcommandData = StrClass1.BuildPacket(sendGMCommandTop1, list_positions.Cast<object>().ToList());

        //                    //发送字符串到Socket
        //                    bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
        //                    RecordLog("触发飞达取料: " + sendcommandData);
        //                    if (!sendcommand_status)
        //                    {
        //                        return false;
        //                    }
        //                }
        //                break;
        //            default:
        //                {
        //                    //发送字符串到Socket
        //                    bool sendcommand_status = VisionpositionPushcommand("触发指令有误");
        //                    if (!sendcommand_status)
        //                    {
        //                        return false;
        //                    }
        //                }
        //                break;
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //        //bool sendcommand_status = this.VisionpositionfeedPushcommand("信息报错:"+ex.ToString());
        //        return false;
        //    }
        //}

        public static bool TriggFeedUpCamreaGMSendData(FeedupCameraProcessCommand feedupCameraProcessCommand, List<FeedUpCamrea.Pushcommand.SendGMCommandAppend> list_positions) //飞达拍照与相机交互GM自动触发流程
        {
            try
            {
                //GM触发指令头
                FeedUpCamrea.Pushcommand.SendGMCommandTop sendGMCommandTop1 = new FeedUpCamrea.Pushcommand.SendGMCommandTop();
                sendGMCommandTop1.GM = "GM";
                sendGMCommandTop1.PickNumber = "1";
                InstructionHeader = $"{sendGMCommandTop1.GM},{sendGMCommandTop1.PickNumber},";

                //组合字符串
                string sendcommandData = StrClass1.BuildPacket(sendGMCommandTop1, list_positions.Cast<object>().ToList());

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

        public static bool TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand feedupCameraProcessCommand, List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> list_positions) //飞达拍照与相机交互TLM自动触发流程
        {
            try
            {
                //TLM触发指令头
                FeedUpCamrea.Pushcommand.SendTLMCommandTop sendTLMCommandTop1 = new FeedUpCamrea.Pushcommand.SendTLMCommandTop();
                sendTLMCommandTop1.TLM = "TLM";
                sendTLMCommandTop1.CmdID = "Cmd_100";
                sendTLMCommandTop1.CamreaCount = "2";
                InstructionHeader = $"{sendTLMCommandTop1.TLM},{sendTLMCommandTop1.CmdID},{sendTLMCommandTop1.CamreaCount},";

                //组合字符串
                string sendcommandData = StrClass1.BuildPacket(sendTLMCommandTop1, list_positions.Cast<object>().ToList());

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

        public static string TriggFeedUpCamreaready()
        {
            string VisionAcceptData = null;
            if (!VisionpositionAcceptcommand(out VisionAcceptData))
            {
                return null;
            }
            //TLM接收指令头
            FeedUpCamrea.Acceptcommand.AcceptTLMCommandTop accepTLMCommandTop1 = new FeedUpCamrea.Acceptcommand.AcceptTLMCommandTop();
            Type camdowntype = typeof(FeedUpCamrea.Acceptcommand.TLMCamreaready);
            List<object> list_position = new List<object>();
            //解析字符串
            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, accepTLMCommandTop1, list_position, camdowntype);
            if (!Analysis_status)
            {
                return null;
            }
            //需要输出list_position
            return ((FeedUpCamrea.Acceptcommand.TLMCamreaready)list_position[0]).CamreaReadyFlag;
        }

        public static bool TriggFeedUpCamreaAcceptData(FeedupCameraProcessCommand feedupCameraProcessCommand, out List<object> list_position)//飞达拍照与相机交互接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("收到取料坐标: " + VisionAcceptData);
                list_position = null;


                if (!VisionAcceptData_status)
                {
                    return false;
                }
                switch ((int)feedupCameraProcessCommand)
                {
                    case (int)FeedupCameraProcessCommand.TLM://TLM接收指令
                        {
                            //TLM接收指令头
                            FeedUpCamrea.Acceptcommand.AcceptTLMCommandTop accepTLMCommandTop1 = new FeedUpCamrea.Acceptcommand.AcceptTLMCommandTop();
                            Type camdowntype = typeof(FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition);
                            //List<object> list_position = new List<object>();
                            list_position = new List<object>();
                            //解析字符串
                            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, accepTLMCommandTop1, list_position, camdowntype);
                            if (!Analysis_status)
                            {
                                return false;
                            }
                            //需要输出list_position
                        }
                        break;
                    case (int)FeedupCameraProcessCommand.GM://GM接收指令
                        {
                            //GM接收指令头
                            FeedUpCamrea.Acceptcommand.AcceptGMCommandTop accepGMCommandTop1 = new FeedUpCamrea.Acceptcommand.AcceptGMCommandTop();
                            Type camdowntype = typeof(FeedUpCamrea.Acceptcommand.AcceptGMCommandAppend);
                            //List<object> list_position = new List<object>();
                            list_position = new List<object>();
                            //解析字符串
                            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, accepGMCommandTop1, list_position, camdowntype);
                            if (!Analysis_status)
                            {
                                return false;
                            }
                            //需要输出list_position
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
