using AkribisFAM.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.DeviceClass
{
    public class BuzzerControl
    {
        public bool BeepStatus { get; set; }
        public bool EnableBeep { get; set; } = true;

        public BuzzerControl()
        {
        }
        public bool On()
        {
            return IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 1);
        }
        public bool Off()
        {
            BeepStatus = false;
            return IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 0);
        }

        public bool Warn()
        {
            bool res = IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 1);
            Thread.Sleep(1000);
            res = res && IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 0);
            return res;
        }

        public void BeepOn()
        {
            if (!BeepStatus && EnableBeep)
            {
                BeepStatus = true;
                EnableBeep = false;
                Task.Run(() =>
                {
                    while (BeepStatus)
                    {
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 1);
                        Thread.Sleep(500);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_5Buzzer, 0);
                        Thread.Sleep(500);
                    }
                });

            }
        }
        public void BeepOff()
        {
            BeepStatus = false;

        }
    }
}
