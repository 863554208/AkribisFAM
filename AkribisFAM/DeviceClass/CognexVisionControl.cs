using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using static AkribisFAM.CommunicationProtocol.Task_PrecisionDownCamreaFunction;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.DeviceClass
{


    public class CognexVisionControl
    {
        List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> snapFeederPath = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();
        List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> ccd2SnapPath = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> palletePath = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendGTCommandAppend> fetchMatrial = new List<AssUpCamrea.Pushcommand.SendGTCommandAppend>();
        public List<string> msg = new List<string>();

        List<SinglePoint> snapPalleteList = new List<SinglePoint>();
        List<SinglePoint> RealPalletePointsList = new List<SinglePoint>();
        List<SinglePoint> feedar1pointList = new List<SinglePoint>();
        public enum FeederNum
        {
            Feeder1 = 1,
            Feeder2 = 2,
        }
        public CognexVisionControl() { }

        public bool Vision1OnTheFlyFoamTrigger(FeederNum feeder)
        {
            List<SinglePoint> points = new List<SinglePoint>();
            switch (feeder)
            {
                case FeederNum.Feeder1:
                    points = GlobalManager.Current.feedar1Points;
                    break;
                case FeederNum.Feeder2:
                    points = GlobalManager.Current.feedar2Points;
                    break;
                default:
                    return false;
            }

            if (!MoveFoamStandbyPos(feeder))
            {
                return false;
            }

            snapFeederPath.Clear();
            feedar1pointList.Clear();
            int index = 0;
            int count = 0;
            double start_pos_X = points[0].X;
            double start_pos_Y = points[0].Y;
            double end_pos_X = points[1].X;
            double end_pos_Y = points[1].Y;
            for (int i = 0; i < 4; i++)
            {
                feedar1pointList.Add(new SinglePoint()
                {
                    X = start_pos_X + 16 * i,
                    Y = start_pos_Y,
                    Z = 0,
                    R = 0,
                });
            }
            foreach (var Point in feedar1pointList)
            {
                FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                {
                    SN1 = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    RawMaterialName1 = "Foam",
                    FOV = (count + 1).ToString(),
                    Photo_X1 = Point.X.ToString(),
                    Photo_Y1 = Point.Y.ToString(),
                    Photo_R1 = "0"
                };
                count++;
                snapFeederPath.Add(sendTLMCamreaposition1);
            }

            //给Cognex发拍照信息
            if (!Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath))
            {
                Logger.WriteLog("Failed to send TLM");
                return false;
            }
            int retryCount = 0;
            while (Task_FeedupCameraFunction.TriggFeedUpCamreaready() != "OK")
            {
                Thread.Sleep(300);
                retryCount++;

                if (retryCount > 10) return false;
            }
            AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, points[0].X, 16, points[0].X + 16 * 3, 1);
            //AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, 105, 16, 105 + 16 * 3, 1);
            Thread.Sleep(300);
            //AkrAction.Current.EventEnable(AxisName.FSX);
            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);

            //移动到拍照结束点
            if (!MoveFoamEndingPos(feeder))
            {
                return false;
            }

            AkrAction.Current.EventDisable(AxisName.FSX);
            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);

            ////接受Cognex的信息
            List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> msg_received = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
            msg_received = Task_FeedupCameraFunction.TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand.TLM);



            Logger.WriteLog("feedar飞拍接收到的消息为:" + msg_received[0].Errcode1);
            return true;
        }

        public bool Vision2OnTheFlyTrigger()
        {
            ccd2SnapPath.Clear();
            int count = 0;
            var start_x = GlobalManager.Current.lowerCCDPoints[0].X;
            var start_y = GlobalManager.Current.lowerCCDPoints[0].Y;
            for (int i = 0; i < 4; i++)
            {

                PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition SendTLNCamreaposition = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition()
                {
                    SN = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    NozzleID = (count + 1).ToString(),
                    RawMaterialName = "Foam",
                    CaveID = "0",
                    TargetMaterialName1 = "Foam->Moudel",
                    Photo_X1 = (start_x - i * 16).ToString(),
                    Photo_Y1 = (start_y).ToString(),
                    Photo_R1 = "90.0",

                };
                ccd2SnapPath.Add(SendTLNCamreaposition);
                count++;
            }


            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);
            Thread.Sleep(300);


            //移动到拍照起始点urrent.MoveNoWait(AxisName.PICK2_T, 90, (int)AxisSpeed.PICK2_T);
            //if (!(App.assemblyGantryControl.TRotate(AssemblyGantryControl.Picker.Picker1, 90) &&
            //    App.assemblyGantryControl.TRotate(AssemblyGantryControl.Picker.Picker2, 90) &&
            //    App.assemblyGantryControl.TRotate(AssemblyGantryControl.Picker.Picker3, 90) &&
            //        App.assemblyGantryControl.TRotate(AssemblyGantryControl.Picker.Picker4, 90)))
            //{

            //    return false;
            //}


            Logger.WriteLog("CCD2准备移动到拍照位置");

            if (!MoveVision2StandbyPos())
            {
                return false;
            }
            if (!App.assemblyGantryControl.ZCamPos(AssemblyGantryControl.Picker.Picker1))
            {
                return false;
            }
            AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, GlobalManager.Current.lowerCCDPoints[0].X, -16, GlobalManager.Current.lowerCCDPoints[0].X - 16 * 3, 2);
            Thread.Sleep(200);

            Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNSendData(PrecisionDownCamreaProcessCommand.TLN, ccd2SnapPath);
            Thread.Sleep(100);
            //给Cognex发拍照信息
            Logger.WriteLog("CCD2 开始接受COGNEX的OK信息");
            while (Task_PrecisionDownCamreaFunction.TriggDownCamreaready() != "OK")
            {
                string res = "接收到的信息是:" + Task_PrecisionDownCamreaFunction.TriggDownCamreaready();
                Logger.WriteLog(res);
                Thread.Sleep(300);
            }
            Logger.WriteLog("CCD2 接受完成COGNEX的OK信息");
            Thread.Sleep(30);
            AkrAction.Current.EventEnable(AxisName.FSX);
            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response6);
            Thread.Sleep(200);

            Logger.WriteLog("开始CCD2运动1");
            //移动到拍照结束点
            if (AkrAction.Current.Move(AxisName.FSX, (GlobalManager.Current.lowerCCDPoints[0].X - 16 * 4), (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0)
            {
                return false;
            }

            if (!MoveVision2EndingPos())
            {
                return false;
            }

            Thread.Sleep(200);
            Logger.WriteLog("结束CCD2运动1");
            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response3);
            //接受Cognex信息
            //List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> AcceptTLNDownPosition = new List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition>();
            //AcceptTLNDownPosition = Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNAcceptData(PrecisionDownCamreaProcessCommand.TLN);

            return (AkrAction.Current.MoveNoWait(AxisName.PICK1_Z, 0, (int)AxisSpeed.PICK1_Z) == 0 &&
            AkrAction.Current.Move(AxisName.PICK2_Z, 0, (int)AxisSpeed.PICK2_Z) == 0);
        }
        public bool MoveVision2StandbyPos()
        {
            List<SinglePoint> points = new List<SinglePoint>();
            points = GlobalManager.Current.lowerCCDPoints;

            if (!App.assemblyGantryControl.ZUpAll())
                return false;

            //移动到拍照起始点
            return (AkrAction.Current.Move(AxisName.FSX, points[0].X + 16, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) == 0 &&
            AkrAction.Current.Move(AxisName.FSY, points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) == 0);


            //AkrAction.Current.Move(AxisName.FSX, GlobalManager.Current.lowerCCDPoints[0].X + 16, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            //AkrAction.Current.Move(AxisName.FSY, GlobalManager.Current.lowerCCDPoints[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);
        }
        public bool MoveVision2EndingPos()
        {
            List<SinglePoint> points = new List<SinglePoint>();
            points = GlobalManager.Current.lowerCCDPoints;

            if (!App.assemblyGantryControl.ZUpAll())
                return false;

            //AkrAction.Current.Move(AxisName.FSX, (GlobalManager.Current.lowerCCDPoints[0].X - 16 * 4), (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            return (AkrAction.Current.Move(AxisName.FSX, (GlobalManager.Current.lowerCCDPoints[0].X - 16 * 4), (int)AxisSpeed.FSX, (int)AxisAcc.FSX) == 0 &&
            AkrAction.Current.Move(AxisName.FSY, points[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) == 0);

        }

        public bool MoveVision3EndingPos()
        {
            List<SinglePoint> points = new List<SinglePoint>();
            points = GlobalManager.Current.lowerCCDPoints;

            if (!App.assemblyGantryControl.ZUpAll())
                return false;

            //AkrAction.Current.Move(AxisName.FSX, (GlobalManager.Current.lowerCCDPoints[0].X - 16 * 4), (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            return (AkrAction.Current.MoveNoWait(AxisName.FSX, (GlobalManager.Current.lowerCCDPoints[0].X - 16 * 4), (int)AxisSpeed.FSX, (int)AxisAcc.FSX) == 0 &&
            AkrAction.Current.Move(AxisName.FSY, points[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) == 0);

        }


        public bool Vision1OnTheFlyPalletTrigger(int TotalRow, int TotalColumn)
        {
            if (!App.assemblyGantryControl.ZUpAll())
                return false;

            CalculateSnapPosition(TotalRow, TotalColumn);
            CalculateFlySnapPath(TotalRow, TotalColumn);
            PopulateSendString(TotalRow, TotalColumn);
            return VisionTriggerMove();
        }

        public void CalculateSnapPosition(int TotalRow, int TotalColumn)
        {
            double start_pos_X = GlobalManager.Current.snapPalletePoints[0].X;
            double start_pos_Y = GlobalManager.Current.snapPalletePoints[0].Y;
            int totalRow = TotalRow;
            int totalColumn = TotalColumn;
            int gap_X = GlobalManager.Current.PalleteGap_X;
            int gap_Y = GlobalManager.Current.PalleteGap_Y;
            //double end_pos_X = (totalColumn - 1) * gap_X;
            //double end_pos_Y = (totalRow - 1) * gap_Y;
            double left_end_X = start_pos_X - (totalColumn - 1) * gap_X;

            bool reverse2 = true;
            RealPalletePointsList.Clear();
            for (int row = 0; row < totalRow; row++)
            {
                //double current_start_pos_Y = start_pos_Y - row * gap_Y;
                //double current_end_pos_Y = current_start_pos_Y;
                //double current_start_pos_X = start_pos_X;
                if (reverse2)
                {
                    for (int column = 0; column < totalColumn; column++)
                    {

                        RealPalletePointsList.Add(new SinglePoint()
                        {
                            X = start_pos_X - column * gap_X,
                            Y = start_pos_Y - row * gap_Y,
                            Z = 0,
                            R = 0
                        });

                    }
                    reverse2 = false;
                }
                else
                {
                    for (int column = 0; column < totalColumn; column++)
                    {
                        RealPalletePointsList.Add(new SinglePoint()
                        {
                            X = left_end_X + column * gap_X,
                            Y = start_pos_Y - row * gap_Y,
                            Z = 0,
                            R = 0
                        });

                    }
                    reverse2 = true;
                }
            }

        }
        public void CalculateFlySnapPath(int TotalRow, int TotalColumn)
        {

            double start_pos_X = GlobalManager.Current.snapPalletePoints[0].X;
            double start_pos_Y = GlobalManager.Current.snapPalletePoints[0].Y;
            int totalRow = TotalRow;
            int totalColumn = TotalColumn;
            int gap_X = GlobalManager.Current.PalleteGap_X;
            int gap_Y = GlobalManager.Current.PalleteGap_Y;
            double end_pos_X = (totalColumn - 1) * gap_X;
            double end_pos_Y = (totalRow - 1) * gap_Y;

            snapPalleteList.Clear();

            for (int row = 0; row < totalRow; row++)
            {
                double current_start_pos_Y = start_pos_Y - row * gap_Y; // 当前行的起点Y坐标
                double current_end_pos_Y = current_start_pos_Y; // 当前行的终点Y坐标（在同一行）]
                double current_start_pos_X = start_pos_X;
                double current_end_pos_X = start_pos_X - (totalColumn - 1) * gap_X;
                snapPalleteList.Add(new SinglePoint()
                {
                    X = current_start_pos_X,
                    Y = current_start_pos_Y,
                    Z = 0,
                    R = 0
                });
                snapPalleteList.Add(new SinglePoint()
                {
                    X = current_end_pos_X,
                    Y = current_end_pos_Y,
                    Z = 0,
                    R = 0
                });
            }
        }
        public void PopulateSendString(int row, int column)
        {
            for (int count2 = 0; count2 < row * column; count2++)
            {

                AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCameraposition = new AssUpCamrea.Pushcommand.SendTLTCamreaposition()
                {
                    SN = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    NozzleID = "0",
                    MaterialTypeN1 = "Foam",
                    AcupointNumber = $"{count2 + 1}",
                    TargetMaterialName1 = "Foam->Moudel",
                    Photo_X1 = RealPalletePointsList[count2].X.ToString(),
                    Photo_Y1 = RealPalletePointsList[count2].Y.ToString(),
                    Photo_R1 = "0"
                };
                palletePath.Add(sendTLTCameraposition);
            }
        }

        private bool VisionTriggerMove()
        {
            bool reverse = true;
            int count = 0;
            bool has_sent = false;
            while (count < snapPalleteList.Count)
            {
                if (!reverse)
                {
                    Logger.WriteLog("料盘飞拍开始");

                    AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);
                    Thread.Sleep(300);
                    //0,1 => 3,3 +,5
                    if (AkrAction.Current.Move(AxisName.FSX, snapPalleteList[count + 1].X - GlobalManager.Current.PalleteGap_X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0 ||
                    (AkrAction.Current.Move(AxisName.FSY, snapPalleteList[count + 1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX)) != 0)
                    {
                        return false;
                    }
                    if (!has_sent)
                    {
                        Task_AssUpCameraFunction.TriggAssUpCamreaTLTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.TLT, palletePath);
                        //Thread.Sleep(300);
                        //GetPlacePosition(1, 1);
                        has_sent = true;
                    }

                    AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, snapPalleteList[count + 1].X, GlobalManager.Current.PalleteGap_X, snapPalleteList[count].X, 1);
                    Thread.Sleep(300);
                    AkrAction.Current.EventEnable(AxisName.FSX);
                    AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);
                    Thread.Sleep(300);

                    if (AkrAction.Current.Move(AxisName.FSX, snapPalleteList[count].X + GlobalManager.Current.PalleteGap_X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0)
                    {
                        return false;
                    }

                    AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response3);

                    count += 2;

                    reverse = true;
                    Thread.Sleep(100);
                }
                else
                {
                    Logger.WriteLog("料盘飞拍开始");

                    AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);
                    Thread.Sleep(300);

                    if (AkrAction.Current.Move(AxisName.FSX, snapPalleteList[count].X + GlobalManager.Current.PalleteGap_X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0 ||
                    AkrAction.Current.Move(AxisName.FSY, snapPalleteList[count].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSX) != 0)
                    {
                        return false;
                    }

                    if (!has_sent)
                    {
                        Task_AssUpCameraFunction.TriggAssUpCamreaTLTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.TLT, palletePath);
                        //Thread.Sleep(300);
                        //GetPlacePosition(1, 1);
                        has_sent = true;
                    }

                    AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, snapPalleteList[count].X, -GlobalManager.Current.PalleteGap_X, snapPalleteList[count + 1].X, 1);

                    AkrAction.Current.EventEnable(AxisName.FSX);
                    AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);
                    Thread.Sleep(300);

                    if (AkrAction.Current.Move(AxisName.FSX, snapPalleteList[count + 1].X - GlobalManager.Current.PalleteGap_X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0)
                    {
                        return false;
                    }

                    AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response3);
                    count += 2;
                    reverse = false;
                }


            }

            return true;
        }
        public bool MoveFoamStandbyPos(FeederNum feeder)
        {
            List<SinglePoint> points = new List<SinglePoint>();
            switch (feeder)
            {
                case FeederNum.Feeder1:
                    points = GlobalManager.Current.feedar1Points;
                    break;
                case FeederNum.Feeder2:
                    points = GlobalManager.Current.feedar2Points;
                    break;
                default:
                    return false;
            }

            if (!App.assemblyGantryControl.ZUpAll())
                return false;

            //移动到拍照起始点
            return AkrAction.Current.Move(AxisName.FSX, points[0].X - 16, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) == 0 &&
              AkrAction.Current.Move(AxisName.FSY, points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) == 0;

        }


        public bool MoveFoamEndingPos(FeederNum feeder)
        {
            List<SinglePoint> points = new List<SinglePoint>();
            switch (feeder)
            {
                case FeederNum.Feeder1:
                    points = GlobalManager.Current.feedar1Points;
                    break;
                case FeederNum.Feeder2:
                    points = GlobalManager.Current.feedar2Points;
                    break;
                default:
                    return false;
            }
            if (!App.assemblyGantryControl.ZUpAll())
                return false;

            //移动到拍照结束点
            return (AkrAction.Current.Move(AxisName.FSX, points[0].X + 16 * 4, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) == 0 &&
            AkrAction.Current.Move(AxisName.FSY, points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) == 0);
        }
    }
}
