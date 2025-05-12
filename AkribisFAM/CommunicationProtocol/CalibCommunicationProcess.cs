using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.CommunicationProtocol
{
    public enum DownCamreaAloneCalibProcess
    {
        start,
        Ongoing,
        calibend,
        processend
    }

    public enum CombineCalibProcess
    {
        Combinestart,
        Combinepick,
        CombineRelationpick,
        Combineongoing,
        Combineput,
        CombineRelationput,
        Combinecalibend,
        Combineprocessend,
    }

    public enum MoveCameraCalibPositionNumber//移动相机标定指令头
    {
        [Description("CarriercalibPlate")]//载具平台
        C4,
        [Description("FeedPosition")]//出料平台
        C5
    }

    public enum DownCamreaNozzleCalibNumber//下相机标定指令头
    {
        [Description("1#Nozzle")]
        C2,
        [Description("2#Nozzle")]
        C7,
        [Description("3#Nozzle")]
        C8,
        [Description("4#Nozzle")]
        C9
    }

    class CalibCommunicationProcess
    {
        private static string InstructionHeader;//指令头 

        public static void SendCombineCalibration(CombineCalibProcess combineCalibProcess)//多相机联合标定通讯
        {
            switch (combineCalibProcess)
            {
                case CombineCalibProcess.Combinestart:
                    {
                        CalibPushcommand($"SC,1,1,2,11,3,1\r\n");
                    }
                    break;
                case CombineCalibProcess.Combinepick:
                    {

                        string CombinepickRobot = "234,789,90";
                        CalibPushcommand($"C1,{CombinepickRobot}\r\n");
                    }
                    break;
                case CombineCalibProcess.CombineRelationpick:
                    {
                        string CombineRelationpickPhotoXY = ReadaxisPositionXYR();
                        CalibPushcommand($"SET,1,4,{CombineRelationpickPhotoXY.Split(',')[0]},{CombineRelationpickPhotoXY.Split(',')[1]},0\r\n");
                    }
                    break;
                case CombineCalibProcess.Combineongoing:
                    {
                        SendAloneCalib(DownCamreaAloneCalibProcess.Ongoing, DownCamreaNozzleCalibNumber.C2);
                    }
                    break;
                case CombineCalibProcess.Combineput:
                    {
                        string CombineputRobot = "234,789,95";
                        CalibPushcommand($"C3,{CombineputRobot}\r\n");
                    }
                    break;
                case CombineCalibProcess.CombineRelationput:
                    {
                        string CombineRelationputPhotoXY = ReadaxisPositionXYR();
                        CalibPushcommand($"SET,3,5,{CombineRelationputPhotoXY.Split(',')[0]},{CombineRelationputPhotoXY.Split(',')[1]},0\r\n");
                    }
                    break;
                case CombineCalibProcess.Combinecalibend:
                    {
                        CalibPushcommand($"EC,1\r\n");
                    }
                    break;
                case CombineCalibProcess.Combineprocessend:
                    {
                        CalibPushcommand($"UN,1");
                    }
                    break;
                default:
                    {
                        //CalibPushcommand($"UN,1");
                    }
                    break;
            }
        }

        public static void SendAloneCalib(DownCamreaAloneCalibProcess downCamreaCalibStatus, DownCamreaNozzleCalibNumber downCamreaNozzleCalibNumber)//下相机单独11点标定通讯
        {
            switch (downCamreaCalibStatus)
            {
                case DownCamreaAloneCalibProcess.start:
                    {
                        CalibPushcommand($"SC,{downCamreaNozzleCalibNumber.ToString().Substring(1)},11\r\n");
                    }
                    break;
                case DownCamreaAloneCalibProcess.Ongoing:
                    {
                        CalibPushcommand($"{downCamreaNozzleCalibNumber.ToString()},{ReadaxisPositionXYR()}\r\n");
                    }
                    break;
                case DownCamreaAloneCalibProcess.calibend:
                    {
                        CalibPushcommand($"EC,1\r\n");
                    }
                    break;
                case DownCamreaAloneCalibProcess.processend:
                    {
                        CalibPushcommand($"UN,1");
                    }
                    break;
                default:
                    {
                        //DownCamreaCalibPushcommand($"UN,1");
                    }
                    break;
            }
        }

        public static void SendMoveCameraCalib(DownCamreaAloneCalibProcess downCamreaAloneCalibProcess, MoveCameraCalibPositionNumber moveCameraCalibPositionNumber)//移动相机单独九点标定
        {
            switch (downCamreaAloneCalibProcess)
            {
                case DownCamreaAloneCalibProcess.start:
                    {
                        CalibPushcommand($"SC,{moveCameraCalibPositionNumber.ToString().Substring(1)},9");
                    }
                    break;
                case DownCamreaAloneCalibProcess.Ongoing:
                    {
                        string CombineRelationpickPhotoXY = ReadaxisPositionXYR();
                        CalibPushcommand($"{moveCameraCalibPositionNumber.ToString()},{CombineRelationpickPhotoXY.Split(',')[0]},{CombineRelationpickPhotoXY.Split(',')[1]},0");
                    }
                    break;
                case DownCamreaAloneCalibProcess.calibend:
                    {
                        CalibPushcommand($"EC,1\r\n");
                    }
                    break;
                case DownCamreaAloneCalibProcess.processend:
                    {
                        CalibPushcommand($"UN,1");
                    }
                    break;
                default:
                    {
                        //CalibPushcommand($"UN,1");
                    }

                    break;
            }
        }

        public static bool CalibStatus()//接收每次CalibProcess是否成功
        {
            string VisionAcceptCommand = null;
            if (!CalibReadcommand(out VisionAcceptCommand))
            {
                return false;
            }
            if (VisionAcceptCommand.Split(',')[0] != InstructionHeader)
            {
                return false;
            }
            if (VisionAcceptCommand.Split(',')[1] == "1")
            {
                return true;
            }
            return false;
        }

        private static string ReadaxisPositionXYR()//读轴的实时位置,只需要x，y，r
        {
            return "345,567,89";
        }

        public static void TriggAssUpCamreaStrClear()//清除客户端最后一条字符串
        {
            TCPNetworkManage.ClearLastMessage(ClientNames.camera2);
        }

        private static void CalibPushcommand(string VisionSendCommand)//写socket
        {
            InstructionHeader = VisionSendCommand.Split(',')[0];
            TCPNetworkManage.InputLoop(ClientNames.camera2, VisionSendCommand);
        }

        private static bool CalibReadcommand(out string VisionAcceptCommand)//从网络Socket读取字符串
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

            if (VisionAcceptCommand == null || VisionAcceptCommand == "")
            {
                return false;
            }
            if (VisionAcceptCommand.Contains("\r\n"))
            {
                VisionAcceptCommand = VisionAcceptCommand.Replace("\r\n", "");
            }
            return true;//需要添加代码修改(网络Socket读取字符串)
        }
    }
}









