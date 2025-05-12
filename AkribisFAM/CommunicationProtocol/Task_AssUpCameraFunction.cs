using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace AkribisFAM.CommunicationProtocol
{
    #region 组装定位上相机
    public class AssUpCamrea
    {
        #region//发送的指令
        public class Pushcommand
        {
            //定义拍照发送头部指令
            public class SendTLTCommandTop
            {
                public string TLT; // 模块头
                public string CmdID;  // 指令编号
                public string CamreaCount; // 拍照次数  
            }
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

            //定义拍照发送头部指令(获取贴装坐标)
            public class SendGTCommandTop
            {
                public string GT; // 模块头
                public string PickNumber;  // 计算取料坐标个数

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
            //定义接受Cognex头部指令
            public class AcceptTLTCommandTop
            {
                public string TLT; // 模块头
                public string CmdID;  // 指令编号
                public string CamreaCount; // 拍照次数
            }
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

            //定义Cognex头部接收指令(获取贴装坐标)
            public class AcceptGTCommandTop
            {
                public string GT; // 模块头
                public string PickNumber;  // 计算放料坐标个数

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

    class Task_AssUpCameraFunction
    {
        public enum AssUpCameraProcessCommand
        {
            TLT,//定位载具
            GT//获取取料坐标
        }

        private static string InstructionHeader;//指令头

        public static bool TriggAssUpCamreaSendData(AssUpCameraProcessCommand assUpCameraProcessCommand) //组装定位拍照与相机交互自动触发流程
        {
            try
            {
                switch ((int)assUpCameraProcessCommand)
                {
                    case (int)AssUpCameraProcessCommand.TLT://TLT触发指令
                        {
                            //TLT触发指令头
                            AssUpCamrea.Pushcommand.SendTLTCommandTop sendTLTCommandTop1 = new AssUpCamrea.Pushcommand.SendTLTCommandTop();
                            sendTLTCommandTop1.TLT = "TLT";
                            sendTLTCommandTop1.CmdID = "CmdTLT_100";
                            sendTLTCommandTop1.CamreaCount = "10";
                            InstructionHeader = $"{sendTLTCommandTop1.TLT},{sendTLTCommandTop1.CmdID},{sendTLTCommandTop1.CamreaCount},";

                            //SN1+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> sendTLTCamreapositions = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition>();
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition1 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition1.SN = "TLTTestSN20250418152256+1";
                            sendTLTCamreaposition1.NozzleID = "0";
                            sendTLTCamreaposition1.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition1.AcupointNumber = "1";
                            sendTLTCamreaposition1.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition1.Photo_X1 = "256.890";
                            sendTLTCamreaposition1.Photo_Y1 = "345.445";
                            sendTLTCamreaposition1.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition1);

                            //SN2+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition2 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition2.SN = "TLTTestSN20250418152256+2";
                            sendTLTCamreaposition2.NozzleID = "0";
                            sendTLTCamreaposition2.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition2.AcupointNumber = "2";
                            sendTLTCamreaposition2.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition2.Photo_X1 = "256.890";
                            sendTLTCamreaposition2.Photo_Y1 = "345.445";
                            sendTLTCamreaposition2.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition2);

                            //SN3+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition3 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition3.SN = "TLTTestSN20250418152256+3";
                            sendTLTCamreaposition3.NozzleID = "0";
                            sendTLTCamreaposition3.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition3.AcupointNumber = "3";
                            sendTLTCamreaposition3.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition3.Photo_X1 = "256.890";
                            sendTLTCamreaposition3.Photo_Y1 = "345.445";
                            sendTLTCamreaposition3.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition3);

                            //SN4+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition4 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition4.SN = "TLTTestSN20250418152256+4";
                            sendTLTCamreaposition4.NozzleID = "0";
                            sendTLTCamreaposition4.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition4.AcupointNumber = "4";
                            sendTLTCamreaposition4.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition4.Photo_X1 = "256.890";
                            sendTLTCamreaposition4.Photo_Y1 = "345.445";
                            sendTLTCamreaposition4.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition4);

                            //SN5+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition5 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition5.SN = "TLTTestSN20250418152256+5";
                            sendTLTCamreaposition5.NozzleID = "0";
                            sendTLTCamreaposition5.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition5.AcupointNumber = "5";
                            sendTLTCamreaposition5.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition5.Photo_X1 = "256.890";
                            sendTLTCamreaposition5.Photo_Y1 = "345.445";
                            sendTLTCamreaposition5.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition5);

                            //SN6+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition6 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition6.SN = "TLTTestSN20250418152256+6";
                            sendTLTCamreaposition6.NozzleID = "0";
                            sendTLTCamreaposition6.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition6.AcupointNumber = "6";
                            sendTLTCamreaposition6.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition6.Photo_X1 = "256.890";
                            sendTLTCamreaposition6.Photo_Y1 = "345.445";
                            sendTLTCamreaposition6.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition6);

                            //SN7+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition7 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition7.SN = "TLTTestSN20250418152256+7";
                            sendTLTCamreaposition7.NozzleID = "0";
                            sendTLTCamreaposition7.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition7.AcupointNumber = "7";
                            sendTLTCamreaposition7.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition7.Photo_X1 = "256.890";
                            sendTLTCamreaposition7.Photo_Y1 = "345.445";
                            sendTLTCamreaposition7.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition7);

                            //SN8+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition8 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition8.SN = "TLTTestSN20250418152256+8";
                            sendTLTCamreaposition8.NozzleID = "0";
                            sendTLTCamreaposition8.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition8.AcupointNumber = "8";
                            sendTLTCamreaposition8.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition8.Photo_X1 = "256.890";
                            sendTLTCamreaposition8.Photo_Y1 = "345.445";
                            sendTLTCamreaposition8.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition8);

                            //SN9+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition9 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition9.SN = "TLTTestSN20250418152256+9";
                            sendTLTCamreaposition9.NozzleID = "0";
                            sendTLTCamreaposition9.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition9.AcupointNumber = "9";
                            sendTLTCamreaposition9.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition9.Photo_X1 = "256.890";
                            sendTLTCamreaposition9.Photo_Y1 = "345.445";
                            sendTLTCamreaposition9.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition9);

                            //SN10+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition10 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition10.SN = "TLTTestSN20250418152256+10";
                            sendTLTCamreaposition10.NozzleID = "0";
                            sendTLTCamreaposition10.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition10.AcupointNumber = "10";
                            sendTLTCamreaposition10.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition10.Photo_X1 = "256.890";
                            sendTLTCamreaposition10.Photo_Y1 = "345.445";
                            sendTLTCamreaposition10.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition10);

                            //SN11+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition11 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition11.SN = "TLTTestSN20250418152256+11";
                            sendTLTCamreaposition11.NozzleID = "0";
                            sendTLTCamreaposition11.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition11.AcupointNumber = "11";
                            sendTLTCamreaposition11.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition11.Photo_X1 = "256.890";
                            sendTLTCamreaposition11.Photo_Y1 = "345.445";
                            sendTLTCamreaposition11.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition11);

                            //SN12+吸嘴编号+原始物料名称+穴位编号+目标物料名称+X+Y+R
                            AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCamreaposition12 = new AssUpCamrea.Pushcommand.SendTLTCamreaposition();
                            sendTLTCamreaposition12.SN = "TLTTestSN20250418152256+12";
                            sendTLTCamreaposition12.NozzleID = "0";
                            sendTLTCamreaposition12.MaterialTypeN1 = "Foam";
                            sendTLTCamreaposition12.AcupointNumber = "12";
                            sendTLTCamreaposition12.TargetMaterialName1 = "Foam->Moudel";
                            sendTLTCamreaposition12.Photo_X1 = "256.890";
                            sendTLTCamreaposition12.Photo_Y1 = "345.445";
                            sendTLTCamreaposition12.Photo_R1 = "67.456";
                            sendTLTCamreapositions.Add(sendTLTCamreaposition12);


                            // TLT,1,10,
                            //TLTTestSN20250418152256 + 1,0,Foam,1,Foam->Moudel,293.25,322.466,0,
                            //TLTTestSN20250418152256 + 2,0,Foam,2,Foam->Moudel,250.25,322.466,0,
                            //TLTTestSN20250418152256 + 3,0,Foam,3,Foam->Moudel,207.25,322.466,0,
                            //TLTTestSN20250418152256 + 4,0,Foam,4,Foam->Moudel,164.25,322.466,0,
                            //TLTTestSN20250418152256 + 5,0,Foam,5,Foam->Moudel,121.25,322.466,0,
                            //TLTTestSN20250418152256 + 6,0,Foam,6,Foam->Moudel,121.25,379.466,0,
                            //TLTTestSN20250418152256 + 7,0,Foam,7,Foam->Moudel,164.25,379.466,0,
                            //TLTTestSN20250418152256 + 8,0,Foam,8,Foam->Moudel,207.25,379.466,0,
                            //TLTTestSN20250418152256 + 9,0,Foam,9,Foam->Moudel,250.25,379.466,0,
                            //TLTTestSN20250418152256 + 10,0,Foam,10,Foam->Moudel,293.25,379.466,0

                            //组合字符串
                            string sendcommandData = StrClass1.BuildPacket(sendTLTCommandTop1, sendTLTCamreapositions.Cast<object>().ToList());
                           
                            //发送字符串到Socket
                            bool sendcommand_status = VisionpositionfeedPushcommand(sendcommandData);
                            RecordLog("触发流道定位: " + sendcommandData);
                            if (!sendcommand_status)
                            {
                                return false;
                            }
                        }
                        break;
                    case (int)AssUpCameraProcessCommand.GT://GT触发指令
                        {
                            //GT触发指令
                            AssUpCamrea.Pushcommand.SendGTCommandTop sendGTCommandTop1 = new AssUpCamrea.Pushcommand.SendGTCommandTop();
                            sendGTCommandTop1.GT = "GT";
                            sendGTCommandTop1.PickNumber = "1";
                            InstructionHeader = $"{sendGTCommandTop1.GT},{sendGTCommandTop1.PickNumber},";

                            //吸嘴编号+物料名称+穴位编号+目标物料名称
                            //GT,1,4,Foam,10,Foam->Moudel
                            List<AssUpCamrea.Pushcommand.SendGTCommandAppend> sendGTCommandAppends = new List<AssUpCamrea.Pushcommand.SendGTCommandAppend>();
                            AssUpCamrea.Pushcommand.SendGTCommandAppend sendGTCommandAppend1 = new AssUpCamrea.Pushcommand.SendGTCommandAppend();
                            sendGTCommandAppend1.NozzlelD1 = "4";
                            sendGTCommandAppend1.RawMaterialName1 = "Foam";
                            sendGTCommandAppend1.CaveID1 = "10";
                            sendGTCommandAppend1.TargetMaterialName1 = "Foam->Moudel";
                            sendGTCommandAppends.Add(sendGTCommandAppend1);
                            //组合字符串
                            string sendcommandData = StrClass1.BuildPacket(sendGTCommandTop1, sendGTCommandAppends.Cast<object>().ToList());
                           
                            //发送字符串到Socket
                            bool sendcommand_status = VisionpositionfeedPushcommand(sendcommandData);
                            RecordLog("触发吸嘴贴装: " + sendcommandData);
                            if (!sendcommand_status)
                            {
                                return false;
                            }
                        }
                        break;
                    default:
                        {
                            //发送字符串到Socket
                            bool sendcommand_status = VisionpositionfeedPushcommand("触发指令有误");
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

        public static string TriggAssUpCamreaready()//读准备就绪
        {
            string VisionAcceptData = null;
            if (!VisionpositionfeedAcceptcommand(out VisionAcceptData))
            {
                return null;
            }
            //TLNT接收指令头
            AssUpCamrea.Pushcommand.SendTLTCommandTop sendTLTCommandTop1 = new AssUpCamrea.Pushcommand.SendTLTCommandTop();
            Type camdowntype = typeof(AssUpCamrea.Acceptcommand.TLTCamreaready);
            List<object> list_position = new List<object>();
            //解析字符串
            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, sendTLTCommandTop1, list_position, camdowntype);
            if (!Analysis_status)
            {
                return null;
            }
            //需要输出list_position
            return ((AssUpCamrea.Acceptcommand.TLTCamreaready)list_position[0]).CamreaReadyFlag;
        }

        public static bool TriggAssUpCamreaAcceptData(AssUpCameraProcessCommand assUpCameraProcessCommand, out List<object> list_position)//组装定位拍照与相机交互接收流程
        {
            try
            {
                string VisionAcceptData = "";
                bool VisionAcceptData_status = VisionpositionfeedAcceptcommand(out VisionAcceptData);
                RecordLog("收到贴装坐标: " + VisionAcceptData);
                list_position = null;


                if (!VisionAcceptData_status)
                {
                    return false;
                }
                switch ((int)assUpCameraProcessCommand)
                {
                    case (int)AssUpCameraProcessCommand.TLT://TLT接收指令
                        {
                            //TLT接收指令头
                            AssUpCamrea.Acceptcommand.AcceptTLTCommandTop accepTLTCommandTop1 = new AssUpCamrea.Acceptcommand.AcceptTLTCommandTop();
                            Type camdowntype = typeof(AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition);
                            //List<object> list_position = new List<object>();
                            list_position = new List<object>();
                            //解析字符串
                            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, accepTLTCommandTop1, list_position, camdowntype);
                            if (!Analysis_status)
                            {
                                return false;
                            }
                            //需要输出list_position
                        }
                        break;
                    case (int)AssUpCameraProcessCommand.GT://GT接收指令
                        {
                            //GT接收指令头
                            AssUpCamrea.Acceptcommand.AcceptGTCommandTop accepGTCommandTop1 = new AssUpCamrea.Acceptcommand.AcceptGTCommandTop();
                            Type camdowntype = typeof(AssUpCamrea.Acceptcommand.AcceptGTCommandAppend);
                            //List<object> list_position = new List<object>();
                            list_position = new List<object>();
                            //解析字符串
                            bool Analysis_status = StrClass1.TryParsePacket(InstructionHeader, VisionAcceptData, accepGTCommandTop1, list_position, camdowntype);
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

        public static void TriggAssUpCamreaStrClear()//清除客户端最后一条字符串
        {
            TCPNetworkManage.ClearLastMessage(ClientNames.camera1_Runner);
        }

        private static void RecordLog(string message)//记录日志
        {
           // Logger.WriteLog(message);
        }

        private static bool VisionpositionfeedAcceptcommand(out string VisionAcceptCommand)//从网络Socket读取字符串
        {
            VisionAcceptCommand = null;
            int timeoutMs = 5000;//5秒之后超时
            int pollIntervalMs = 50;//50毫秒线程延时
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                VisionAcceptCommand = TCPNetworkManage.GetLastMessage(ClientNames.camera1_Runner);
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

        private static bool VisionpositionfeedPushcommand(string VisionSendCommand)//(发送字符串到网络Socket)
        {
            TCPNetworkManage.InputLoop(ClientNames.camera1_Runner, VisionSendCommand);
            return true;//需要添加代码修改(发送字符串到网络Socket)
        }
    }
}
