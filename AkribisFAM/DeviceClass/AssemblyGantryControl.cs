using AkribisFAM.CommunicationProtocol;
using AkribisFAM.WorkStation;
using System.Reflection;
using System.Threading;
using YamlDotNet.Core.Tokens;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.DeviceClass
{
    public class AssemblyGantryControl
    {
        public enum Picker
        {
            Picker1,
            Picker2,
            Picker3,
            Picker4,
        }


        public AssemblyGantryControl() { }

        public bool ZUp(Picker picker)
        {
            switch (picker)
            {
                case Picker.Picker1:
                    break;
                case Picker.Picker2:
                    break;
                case Picker.Picker3:
                    break;
                case Picker.Picker4:
                    break;
                default:
                    return false;
            }

            return true;

        }
        public bool VacOn(Picker picker)
        {
            IO_OutFunction_Table vacSupply;
            IO_OutFunction_Table vacRelease;
            switch (picker)
            {
                case Picker.Picker1:
                    vacSupply = IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release;
                    break;
                case Picker.Picker2:
                    vacSupply = IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release;
                    break;
                case Picker.Picker3:

                    vacSupply = IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release;
                    break;
                case Picker.Picker4:
                    vacSupply = IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release;
                    break;
                default:
                    return false;
            }
            IOManager.Instance.IO_ControlStatus(vacSupply, 1);
            IOManager.Instance.IO_ControlStatus(vacRelease, 0);
            return true;
        }

        public bool Purge(Picker picker)
        {
            IO_OutFunction_Table vacSupply;
            IO_OutFunction_Table vacRelease;
            switch (picker)
            {
                case Picker.Picker1:
                    vacSupply = IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release;
                    break;
                case Picker.Picker2:
                    vacSupply = IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release;
                    break;
                case Picker.Picker3:
                    vacSupply = IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release;
                    break;
                case Picker.Picker4:
                    vacSupply = IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release;
                    break;
                default:
                    return false;
            }
            IOManager.Instance.IO_ControlStatus(vacSupply, 0);
            IOManager.Instance.IO_ControlStatus(vacRelease, 1);
            return true;
        }

        public bool Off(Picker picker)
        {
            IO_OutFunction_Table vacSupply;
            IO_OutFunction_Table vacRelease;
            switch (picker)
            {
                case Picker.Picker1:
                    vacSupply = IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release;
                    break;
                case Picker.Picker2:
                    vacSupply = IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release;
                    break;
                case Picker.Picker3:

                    vacSupply = IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release;
                    break;
                case Picker.Picker4:
                    vacSupply = IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply;
                    vacRelease = IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release;
                    break;
                default:
                    return false;
            }
            IOManager.Instance.IO_ControlStatus(vacSupply, 0);
            IOManager.Instance.IO_ControlStatus(vacRelease, 0);
            return true;
        }
        public bool ZDown(Picker picker)
        {
            AxisName axis;
            AxisSpeed speed;
            switch (picker)
            {
                case Picker.Picker1:
                    axis = AxisName.PICK1_Z;
                    speed = AxisSpeed.PICK1_Z;
                    break;
                case Picker.Picker2:

                    axis = AxisName.PICK2_Z;
                    speed = AxisSpeed.PICK2_Z;
                    break;
                case Picker.Picker3:

                    axis = AxisName.PICK3_Z;
                    speed = AxisSpeed.PICK3_Z;
                    break;
                case Picker.Picker4:

                    axis = AxisName.PICK4_Z;
                    speed = AxisSpeed.PICK4_Z;
                    break;
                default:
                    return false;
            }

            AkrAction.Current.Move(axis, 0, (int)speed);
            return true;

        }
    }
}
