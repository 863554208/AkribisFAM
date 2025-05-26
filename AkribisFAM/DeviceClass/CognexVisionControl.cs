using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.DeviceClass
{


    public class CognexVisionControl
    {
        List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> snapFeederPath = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();
        List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> ccd2SnapPath = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> palletePath = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendGTCommandAppend> fetchMatrial = new List<AssUpCamrea.Pushcommand.SendGTCommandAppend>();
        public enum FeederNum
        {
            Feeder1,
            Feeder2,
        }
        public CognexVisionControl() { }

        public bool FoamOnTheFlyTrigger(FeederNum feeder)
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


            snapFeederPath.Clear();
            int index = 0;
            foreach (var Point in points)
            {
                FeedUpCamrea.Pushcommand.SendTLMCamreaposition sendTLMCamreaposition1 = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                {
                    SN1 = "ASDASD",
                    RawMaterialName1 = "FOAM",
                    FOV = index.ToString(),
                    Photo_X1 = Point.X.ToString(),
                    Photo_Y1 = Point.Y.ToString(),
                    Photo_R1 = "0"
                };
                snapFeederPath.Add(sendTLMCamreaposition1);
            }
            //给Cognex发拍照信息
            if (!Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, snapFeederPath))
            {
                Logger.WriteLog("Failed to send TLM");
                return false;
            }

            //Z Safe

            //移动到拍照起始点
            AkrAction.Current.MoveNoWait(AxisName.FSX, points[0].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

            AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, points[0].X, 50, GlobalManager.Current.feedar1Points[1].X, 1);

            //移动到拍照结束点
            AkrAction.Current.MoveNoWait(AxisName.FSX, points[1].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, points[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

            ////接受Cognex的信息
            List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> msg_received = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
            msg_received = Task_FeedupCameraFunction.TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand.TLM);

            Logger.WriteLog("feedar飞拍接收到的消息为:" + msg_received[0].Errcode1);
            return true;
        }
        public bool MoveVision2StandbyPos()
        {
            List<SinglePoint> points = new List<SinglePoint>(); 
            points = GlobalManager.Current.lowerCCDPoints;
           

            //移动到拍照起始点
            AkrAction.Current.MoveNoWait(AxisName.FSX, points[0].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

            return true;
        }
        public bool MoveVision2EndingPos()
        {
            List<SinglePoint> points = new List<SinglePoint>(); 
            points = GlobalManager.Current.lowerCCDPoints;


            AkrAction.Current.MoveNoWait(AxisName.FSX, points[1].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, points[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

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

            //移动到拍照起始点
            AkrAction.Current.MoveNoWait(AxisName.FSX, points[0].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, points[0].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

            return true;
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

            //移动到拍照结束点
            AkrAction.Current.MoveNoWait(AxisName.FSX, points[1].X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX);
            AkrAction.Current.Move(AxisName.FSY, points[1].Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY);

            return true;
        }
    }
}
