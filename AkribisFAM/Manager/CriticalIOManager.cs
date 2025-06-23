using AkribisFAM.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.Manager
{
    public class CriticalIOManager
    {
        public bool IsMainAirOn => IOManager.Instance.ReadIO(IO_INFunction_Table.IN4_15Compressed_Air_Pressure);
        public bool IsEmergencyStopOk => IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_13emergency_stop);
        public bool IsSSR1Ok => IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_14SSR1_OK_emergency_stop);
        public bool IsSSR2Ok => IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_15SSR2_OK_LOCK);
        public bool IsFeeder => IOManager.Instance.ReadIO(IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback);

        

        public bool ResetMachine()
        {
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_3Machine_Reset, 0);

            Thread.Sleep(500);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_3Machine_Reset, 1);
            Thread.Sleep(500);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_3Machine_Reset, 0);

            return true;
        }
    }
}
