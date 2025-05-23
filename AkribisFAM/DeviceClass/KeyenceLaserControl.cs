using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AkribisFAM.CommunicationProtocol.KEYENCEDistance.Acceptcommand;
using static AkribisFAM.CommunicationProtocol.KEYENCEDistance.Pushcommand;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.DeviceClass
{
    public class KeyenceLaserControl
    {
        List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend> AcceptKDistanceAppend = new List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend>();
        List<KEYENCEDistance.Pushcommand.SendKDistanceAppend> sendKDistanceAppend = new List<KEYENCEDistance.Pushcommand.SendKDistanceAppend>();


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
            Task_KEYENCEDistance.SendMSData(Task_KEYENCEDistance.KEYENCEDistanceProcessCommand.MS, req);

            //得到测量结果
            AcceptKDistanceAppend = Task_KEYENCEDistance.AcceptMSData(Task_KEYENCEDistance.KEYENCEDistanceProcessCommand.MS);
            var res = AcceptKDistanceAppend[0].MeasurData;
            Logger.WriteLog("激光测距结果:" + res);
            return true;
        }
    }
}
