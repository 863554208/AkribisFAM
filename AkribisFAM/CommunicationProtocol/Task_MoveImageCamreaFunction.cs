using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.CommunicationProtocol
{
    #region Group流程移动图片
    public class MoveImage
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照发送头部指令
            public class SendGroupCommandTop
            {
                public string Group; // 组合图片模块头
            }
            //定义拍照位置
            public class SendGroupCamreaposition
            {
                public string Folders_Number;//文件夹数量
                public string Folders_SNOK; // 文件夹名称
                public string ImageNum;//图片数量
                public string PhotoSN1;//取料拍照SN
                public string PhotoSN2;//吸嘴拍照SN
                public string PhotoSN3;//定位载具拍照SN
                public string PhotoSN4;//复检拍照SN
            }
        }
        #endregion

        #region//接收的指令
        public class Acceptcommand
        {
            //定义接受Cognex头部指令
            public class AcceptGroupCommandTop
            {
                public string Group; // 组合图片模块头
            }
            //定义Group_Status数据
            public class AcceptGroupRecheckAppend
            {
                public string Group_Status;//分组完成状态
                public string Image_Filepath;//图片文件路径   
            }

            //移动图片准备状态
            public class GroupCamreaready
            {
                public string CamreaReadyFlag;//移动图片准备完成状态,"OK"代表完成
            }
        }
        #endregion
    }
    #endregion
    class Task_MoveImageCamreaFunction
    {
        public enum MoveImageCamreaProcessCommand
        {
            GROUP,//GROUP
            Down//预留
        }

        private static string InstructionHeader;//指令头

        public static bool TriggMoveImageCamreaSendData(MoveImageCamreaProcessCommand moveImageCamreaProcessCommand, List<object> list_positions) //移动图片与相机交互自动触发流程
        {
            try
            {
                switch ((int)moveImageCamreaProcessCommand)
                {
                    case (int)MoveImageCamreaProcessCommand.GROUP://GROUP触发指令
                        {
                            //GROUP触发指令头
                            //Group3,1,542PAMC311100183_TestSN20250418152024 + 2_2_OK,4,
                            //Pick_0_20250418152023_1,
                            //LocateNozzle_1_20250418152027,
                            //TLTTestSN20250418152024 + 2,
                            //TFCTestSN20250418152024 + 2

                            MoveImage.Pushcommand.SendGroupCommandTop sendGroupCommandTop1= new MoveImage.Pushcommand.SendGroupCommandTop();
                            sendGroupCommandTop1.Group = "Group3";
                            InstructionHeader = $"{sendGroupCommandTop1.Group},";

                            //文件夹数量+文件夹名称+ 图片数量+ 取料拍照SN+ 吸嘴拍照SN+定位载具拍照SN+ 复检拍照SN
                            List<MoveImage.Pushcommand.SendGroupCamreaposition> sendGroupCamreapositions = new List<MoveImage.Pushcommand.SendGroupCamreaposition>();
                            MoveImage.Pushcommand.SendGroupCamreaposition sendGroupCamreaposition1 = new MoveImage.Pushcommand.SendGroupCamreaposition();

                            //sendGroupCamreaposition1.Folders_Number = "1";
                            //sendGroupCamreaposition1.Folders_SNOK= "542PAMC311100183_TestSN20250418152024 + 2_2_OK";
                            //sendGroupCamreaposition1.ImageNum = "4";
                            //sendGroupCamreaposition1.PhotoSN1 = "Pick_0_20250418152023_1";
                            //sendGroupCamreaposition1.PhotoSN2 = "LocateNozzle_1_20250418152027";
                            //sendGroupCamreaposition1.PhotoSN3 = "TLTTestSN20250418152024 + 2";
                            //sendGroupCamreaposition1.PhotoSN4 = "TFCTestSN20250418152024 + 2";
                            //sendGroupCamreapositions.Add(sendGroupCamreaposition1);

                            //组合字符串
                            string sendcommandData = StrClass1.BuildPacket(sendGroupCommandTop1, list_positions.Cast<object>().ToList());

                            //发送字符串到Socket
                            bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                            RecordLog("触发移动图片: " + sendcommandData);
                            if (!sendcommand_status)
                            {
                                return false;
                            }
                        }
                        break;
                    case (int)MoveImageCamreaProcessCommand.Down://预留指令
                        {
                            //MoveImage.Pushcommand.SendGroupCommandTop sendGroupCommandTop1 = new MoveImage.Pushcommand.SendGroupCommandTop();
                            //sendGroupCommandTop1.Group = "Group3";
                            //InstructionHeader = $"{sendGroupCommandTop1.Group},";

                            ////文件夹数量+文件夹名称+ 图片数量+ 取料拍照SN+ 吸嘴拍照SN+定位载具拍照SN+ 复检拍照SN
                            //List<MoveImage.Pushcommand.SendGroupCamreaposition> sendGroupCamreapositions = new List<MoveImage.Pushcommand.SendGroupCamreaposition>();
                            //MoveImage.Pushcommand.SendGroupCamreaposition sendGroupCamreaposition1 = new MoveImage.Pushcommand.SendGroupCamreaposition();

                            //sendGroupCamreaposition1.Folders_Number = "1";
                            //sendGroupCamreaposition1.Folders_SNOK = "542PAMC311100183_TestSN20250418152024 + 2_2_OK";
                            //sendGroupCamreaposition1.ImageNum = "4";
                            //sendGroupCamreaposition1.PhotoSN1 = "Pick_0_20250418152023_1";
                            //sendGroupCamreaposition1.PhotoSN2 = "LocateNozzle_1_20250418152027";
                            //sendGroupCamreaposition1.PhotoSN3 = "TLTTestSN20250418152024 + 2";
                            //sendGroupCamreaposition1.PhotoSN4 = "TFCTestSN20250418152024 + 2";
                            //sendGroupCamreapositions.Add(sendGroupCamreaposition1);

                            ////组合字符串
                            //string sendcommandData = StrClass1.BuildPacket(sendGroupCommandTop1, sendGroupCamreapositions.Cast<object>().ToList());

                            ////发送字符串到Socket
                            //bool sendcommand_status = VisionpositionPushcommand(sendcommandData);
                            //RecordLog("触发移动图片: " + sendcommandData);
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

        public static string TriggMoveImageCamreaready()//读准备就绪
        {
            string VisionAcceptData = null;
            if (!VisionpositionAcceptcommand(out VisionAcceptData))
            {
                return null;
            }
            //GROUP触发指令头
            MoveImage.Pushcommand.SendGroupCommandTop sendGroupCommandTop1 = new MoveImage.Pushcommand.SendGroupCommandTop();
            Type camdowntype = typeof(MoveImage.Acceptcommand.GroupCamreaready);
            List<object> list_position = new List<object>();
            //解析字符串
            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, sendGroupCommandTop1, list_position, camdowntype);
            if (!Analysis_status)
            {
                return null;
            }
            //需要输出list_position
            return ((MoveImage.Acceptcommand.GroupCamreaready)list_position[0]).CamreaReadyFlag;
        }

        public static bool TriggMoveImageCamreaAcceptData(MoveImageCamreaProcessCommand moveImageCamreaProcessCommand, out List<object> list_position)//移动图片与相机交互接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionAcceptcommand(out VisionAcceptData);
                RecordLog("移动图片收到: " + VisionAcceptData);
                list_position = null;

                if (!VisionAcceptData_status)
                {
                    return false;
                }
                switch ((int)moveImageCamreaProcessCommand)
                {
                    case (int)MoveImageCamreaProcessCommand.GROUP://GROUP接收指令
                        {
                            //GROUP接收指令
                            MoveImage.Acceptcommand.AcceptGroupCommandTop accepGroupCommandTop1 = new MoveImage.Acceptcommand.AcceptGroupCommandTop();
                            Type camdowntype = typeof(MoveImage.Acceptcommand.AcceptGroupRecheckAppend);
                            //List<object> list_position = new List<object>();
                            list_position = new List<object>();
                            //解析字符串
                            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, accepGroupCommandTop1, list_position, camdowntype);
                            if (!Analysis_status)
                            {
                                return false;
                            }
                            //需要输出list_position
                        }
                        break;
                    case (int)MoveImageCamreaProcessCommand.Down://预留指令
                        {
                            ////GROUP接收指令
                            //MoveImage.Acceptcommand.AcceptGroupCommandTop accepGroupCommandTop1 = new MoveImage.Acceptcommand.AcceptGroupCommandTop();
                            //Type camdowntype = typeof(MoveImage.Acceptcommand.AcceptGroupRecheckAppend);
                            ////List<object> list_position = new List<object>();
                            //list_position = new List<object>();
                            ////解析字符串
                            //bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, accepGroupCommandTop1, list_position, camdowntype);
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

        public static void TriggDownCamreaStrClear()//清除客户端最后一条字符串
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
