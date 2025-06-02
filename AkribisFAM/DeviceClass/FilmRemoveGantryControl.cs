using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Windows;
using AkribisFAM.WorkStation;
using LiveCharts.Wpf;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YamlDotNet.Core.Tokens;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.DeviceClass
{
    public class FilmRemoveGantryControl
    {
        private double xOffset;

        public double XOffset
        {
            get { return xOffset; }
            set { xOffset = value; }
        }

        private double yOffset;

        public double YOffset
        {
            get { return yOffset; }
            set { yOffset = value; }
        }

        public FilmRemoveGantryControl() { }

        public bool ZDown()
        {

            return AkrAction.Current.MoveRecheckZ(12) == (int)AkrAction.ACTTION_ERR.NONE;
        }
        public bool ZUp()
        {

            return AkrAction.Current.MoveRecheckZ(0) == (int)AkrAction.ACTTION_ERR.NONE;
        }
        public bool ZSafe()
        {

            return AkrAction.Current.MoveRecheckZ(0) == (int)AkrAction.ACTTION_ERR.NONE;
        }
        public bool ClawClose()
        {
            if (!(IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0) &&
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1)))
                return false;

            return true;
        }
        public bool IsClawClose()
        {
            return GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_9Claw_extend_in_position, 1) &&
               GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_10Claw_retract_in_position, 0);
        }
        public bool ClawOpen()
        {
            if (!(IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1) &&
              IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0)))
                return false;

            return true;
        }
        public bool IsClawOpen()
        {
            return GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_9Claw_extend_in_position, 0) &&
               GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_10Claw_retract_in_position, 1);
        }

        public bool MovePos(double x, double y)
        {
            if (!ZUp())
            {
                return false;
            }
            if (!ClawClose())
            {
                return false;
            }
            if (AkrAction.Current.MoveRecheckXY(x,y) != (int)AkrAction.ACTTION_ERR.NONE)
            {

                return false;
            }
            return true;
        }

        public bool MoveToVisionPos(double teachpointX, double teachpointY)
        {
            if (!ZUp())
            {
                return false;
            }
            if (!ClawClose())
            {
                return false;
            }

            if (AkrAction.Current.MoveRecheckXY(teachpointX + (xOffset) + 0, teachpointY + (-yOffset) + 0) != (int)AkrAction.ACTTION_ERR.NONE)
            {

                return false;
            }
            return true;
        }
        public bool VacOn()
        {

            return (IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 1) &&
                 IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_3Peeling_Recheck_vacuum1_Release, 0));
        }
        public bool VacOff()
        {
            return (IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 0) &&
                 IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_3Peeling_Recheck_vacuum1_Release, 0));
        }
        public bool RemoveFilm(double teachpointX, double teachpointY)
        {
            if (!ZUp())
            {
                return false;
            }
            if (!MovePos(teachpointX, teachpointY))
            {
                return false;
            }
            if (!ClawOpen())
            {
                return false;
            }
            if (!ZDown())
            {
                return false;
            }
            if (!ClawClose())
            {
                return false;
            }

            //offset depend on product
            if (!ZSafe())
            {
                return false;
            }

            return true;
        }

        public bool MoveToBinPos()
        {
            if (!ZUp())
                return false;

            var point = GlobalManager.Current.RecheckRecylePos;
            return MovePos(point.X, point.Y);
        }
        public bool Toss()
        {
            if (!MoveToBinPos())
            {
                return false;
            }

            //if (!ZDown())
            //{
            //    return false;
            //}

            if (!VacOn())
            {
                return false;
            }
            if (!ClawOpen())
            {
                return false;
            }
            if (!VacOff())
            {
                return false;
            }

            if (!ZUp())
            {
                return false;
            }

            if (!ClawClose())
            {
                return false;
            }

            return true;

        }
    }
}
