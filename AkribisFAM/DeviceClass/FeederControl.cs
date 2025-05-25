using AkribisFAM.CommunicationProtocol;
using System.Windows.Media;

namespace AkribisFAM.DeviceClass
{
    public class FeederControl
    {
        private int feederNumber;

        public int FeederNumber
        {
            get { return feederNumber; }
            set { feederNumber = value; }
        }

        public FeederControl(int num)
        {
            if (num == 1 || num == 2)
            {
                FeederNumber = num;
            }
        }
        public bool IsInitialized // manual states: 完成为低电平，未完成为高电平； healthy bit
        {
            get
            {
                IO_INFunction_Table input = FeederNumber == 1 ? IO_INFunction_Table.IN4_0Initialized_feeder1 : IO_INFunction_Table.IN4_4Initialized_feeder2;
                return IOManager.Instance.INIO_status[(int)input] == 1;
            }

        }
        public bool IsAlarm //有报警产生低电平，无报警产生高电平, low: alarm, high - ok
        {
            get
            {
                IO_INFunction_Table input = FeederNumber == 1 ? IO_INFunction_Table.IN4_1Alarm_feeder1 : IO_INFunction_Table.IN4_5Alarm_feeder2;
                return IOManager.Instance.INIO_status[(int)input] == 1;
            }
        }
        public bool IsFoamsIn
        {
            get
            {
                IO_INFunction_Table input = FeederNumber == 1 ? IO_INFunction_Table.IN4_2Platform_has_label_feeder1 : IO_INFunction_Table.IN4_6Platform_has_label_feeder2;
                return IOManager.Instance.INIO_status[(int)input] == 1;
            }
        }

        public bool IsLock
        {
            get
            {
                IO_INFunction_Table input1 = FeederNumber == 1 ? IO_INFunction_Table.IN4_8Feeder1_limit_cylinder_extend_InPos : IO_INFunction_Table.IN4_10Feeder2_limit_cylinder_extend_InPos;
                IO_INFunction_Table input2 = FeederNumber == 1 ? IO_INFunction_Table.IN4_9Feeder1_limit_cylinder_retract_InPos : IO_INFunction_Table.IN4_11Feeder2_limit_cylinder_retract_InPos;
                return IOManager.Instance.INIO_status[(int)input1] == 1 && IOManager.Instance.INIO_status[(int)input2] == 0;
            }
        }

        public bool IsUnlock
        {
            get
            {
                IO_INFunction_Table input1 = FeederNumber == 1 ? IO_INFunction_Table.IN4_8Feeder1_limit_cylinder_extend_InPos : IO_INFunction_Table.IN4_10Feeder2_limit_cylinder_extend_InPos;
                IO_INFunction_Table input2 = FeederNumber == 1 ? IO_INFunction_Table.IN4_9Feeder1_limit_cylinder_retract_InPos : IO_INFunction_Table.IN4_11Feeder2_limit_cylinder_retract_InPos;
                return IOManager.Instance.INIO_status[(int)input1] == 0 && IOManager.Instance.INIO_status[(int)input2] == 1;
            }
        }

        public bool IsDrawerInPos
        {
            get
            {
                IO_INFunction_Table input1 = FeederNumber == 1 ? IO_INFunction_Table.IN4_12Feeder1_drawer_InPos : IO_INFunction_Table.IN4_13Feeder2_drawer_InPos;
                return IOManager.Instance.INIO_status[(int)input1] == 1;
            }
        }
        public bool IsVac1PressureOk
        {
            get
            {
                IO_INFunction_Table input1 = FeederNumber == 1 ? IO_INFunction_Table.IN5_0Feeder_vacuum1_Pressure_feedback : IO_INFunction_Table.IN5_2Feeder_vacuum3_Pressure_feedback;
                return IOManager.Instance.INIO_status[(int)input1] == 1;
            }
        }
        public bool IsVac2PressureOk
        {
            get
            {
                IO_INFunction_Table input1 = FeederNumber == 1 ? IO_INFunction_Table.IN5_1Feeder_vacuum2_Pressure_feedback : IO_INFunction_Table.IN5_3Feeder_vacuum4_Pressure_feedback;
                return IOManager.Instance.INIO_status[(int)input1] == 1;
            }
        }


        public enum ErrorType
        {
            NotInit,
            Alarm,
            IndexFailPartExist,

        }
        public bool VacOn(bool checkVacSuccess = false)
        {

            IO_OutFunction_Table vac1_supply = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_8Feeder_vacuum1_Supply : IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply;
            IO_OutFunction_Table vac1_release = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_9Feeder_vacuum1_Release : IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release;
            IO_OutFunction_Table vac2_supply = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_10Feeder_vacuum2_Supply : IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply;
            IO_OutFunction_Table vac2_release = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_11Feeder_vacuum2_Release : IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release; ;

            if (!IOManager.Instance.IO_ControlStatus(vac1_supply, 1))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(vac1_release, 0))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(vac2_supply, 1))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(vac2_release, 0))
            {
                return false;
            }

            if (checkVacSuccess)
            {
                return (!IsVac1PressureOk && !IsVac2PressureOk);
            }

            return true;
        }

        public bool VacOff(bool checkVacSuccess = false)
        {
            IO_OutFunction_Table vac1_supply = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_8Feeder_vacuum1_Supply : IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply;
            IO_OutFunction_Table vac1_release = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_9Feeder_vacuum1_Release : IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release;
            IO_OutFunction_Table vac2_supply = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_10Feeder_vacuum2_Supply : IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply;
            IO_OutFunction_Table vac2_release = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_11Feeder_vacuum2_Release : IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release; ;



            if (!IOManager.Instance.IO_ControlStatus(vac1_supply, 0))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(vac1_release, 1))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(vac2_supply, 0))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(vac2_release, 1))
            {
                return false;
            }

            if (checkVacSuccess)
            {
                return IsVac1PressureOk && IsVac2PressureOk;
            }
            return true;
        }
        public bool Lock()
        {
            IO_OutFunction_Table extend = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_0Feeder1_limit_cylinder_extend : IO_OutFunction_Table.OUT5_2Feeder2_limit_cylinder_extend;
            IO_OutFunction_Table retract = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_1Feeder1_limit_cylinder_retract : IO_OutFunction_Table.OUT5_3Feeder2_limit_cylinder_retract;


            if (!IsDrawerInPos)
            {
                return false;
            }


            if (!IOManager.Instance.IO_ControlStatus(extend, 1))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(retract, 0))
            {
                return false;
            }
            if (!IsLock)
            {
                return false;
            }
            return true;
        }

        public bool Unlock()
        {
            IO_OutFunction_Table extend = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_0Feeder1_limit_cylinder_extend : IO_OutFunction_Table.OUT5_2Feeder2_limit_cylinder_extend;
            IO_OutFunction_Table retract = FeederNumber == 1 ? IO_OutFunction_Table.OUT5_1Feeder1_limit_cylinder_retract : IO_OutFunction_Table.OUT5_3Feeder2_limit_cylinder_retract;

            if (!IsDrawerInPos)
            {
                return false;
            }


            if (!IOManager.Instance.IO_ControlStatus(extend, 0))
            {
                return false;
            }

            if (!IOManager.Instance.IO_ControlStatus(retract, 1))
            {
                return false;
            }
            if (!IsUnlock)
            {
                return false;
            }
            return true;
        }

        public bool Index()
        {
            //Check feeder init status
            if (IsInitialized)
            {
                return false;
            }
            //Check feeder alarm status
            if (IsAlarm)
            {
                return false;
            }
            //Check feeder foam present
            if (IsFoamsIn)
            {
                return false;
            }

            if (!IsLock)
            {
                return false;
            }
            if (!IsVac1PressureOk)
            {
                return false;
            }
            if (!IsVac2PressureOk)
            {
                return false;
            }

            //Index and feed in material
            if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_9Run_feeder1, 1))
            {
                return false;
            }

            //Check if part has been insert successfully
            if (!IsFoamsIn)
            {
                return false;
            }

            return true;

        }
        public bool ClearError()
        {

            if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_10initialize_feeder1, 1))
            {
                return false;
            }
            if (IsAlarm)
            {
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_8Stop_feeder1, 1))
            {
                return false;
            }
            return true;

        }

    }


}
