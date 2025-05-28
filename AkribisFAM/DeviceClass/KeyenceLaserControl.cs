using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Util;
using System.Collections.Generic;

namespace AkribisFAM.DeviceClass
{
    public class KeyenceLaserControl
    {
        List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend> AcceptKDistanceAppend = new List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend>();
        List<KEYENCEDistance.Pushcommand.SendKDistanceAppend> sendKDistanceAppend = new List<KEYENCEDistance.Pushcommand.SendKDistanceAppend>();

        public delegate void OnCameraMessageSentEventHandler(object sender, string message);

        public event OnCameraMessageSentEventHandler OnMessageSent;

        public void SendMessage(string msg)
        {
            OnMessageSent.Invoke(null, msg);
        }
        public delegate void OnCameraMessageReceiveEventHandler(object sender, string message);

        public event OnCameraMessageReceiveEventHandler OnMessageReceive;

        public void ReceiveMessage(string msg)
        {
            OnMessageReceive.Invoke(null, msg);
        }
        public bool Measure(out int result)
        {
            result = -999;
            sendKDistanceAppend.Clear();
            KEYENCEDistance.Pushcommand.SendKDistanceAppend temp = new KEYENCEDistance.Pushcommand.SendKDistanceAppend()
            {
                TestNumber = "1",
                address = "0",
            };
            string req = "1,0" + "\r";
            sendKDistanceAppend.Add(temp);
            if (!Task_KEYENCEDistance.SendMSData())
            {
                Logger.WriteLog("Failed to send MS Data");
                return false;
            }

            //得到测量结果
            AcceptKDistanceAppend = Task_KEYENCEDistance.AcceptMSData();
            if (AcceptKDistanceAppend != null)
            {
                Logger.WriteLog("Failed to receive MS response");
                return false;
            }

            var res = AcceptKDistanceAppend[0].MeasurData;
            Logger.WriteLog("激光测距结果:" + res);

            return true;
        }
    }
}
