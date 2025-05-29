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

            return AkrAction.Current.Move(AxisName.PRZ, 0, (int)AxisSpeed.PRZ) == 0;
        }
        public bool ZUp()
        {

            return AkrAction.Current.Move(AxisName.PRZ, 0, (int)AxisSpeed.PRZ) == 0;
        }
        public bool ZSafe()
        {

            return AkrAction.Current.Move(AxisName.PRZ, 0, (int)AxisSpeed.PRZ) == 0;
        }
        public bool ClawClose()
        {
            bool res = false;
            if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0) ||
                    !IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1))
            {
                return false;
            }


            return GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_9Claw_extend_in_position, 1) &&
                GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_10Claw_retract_in_position, 0);
        }
        public bool ClawOpen()
        {
            bool res = false;
            if (!IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1) ||
                    !IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0))
            {
                return false;
            }


            return GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_9Claw_extend_in_position, 0) &&
                GlobalManager.Current.WaitIO(IO_INFunction_Table.IN3_10Claw_retract_in_position, 1);
        }
        public bool MovePos(double x, double y)
        {
            if (!ZUp())
            {
                return false;
            }

            if (AkrAction.Current.Move(AxisName.PRX, x, (int)AxisSpeed.PRX, (int)AxisAcc.PRX) != 0 ||
            AkrAction.Current.Move(AxisName.PRY, y, (int)AxisSpeed.PRY, (int)AxisAcc.PRY) != 0)
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


            if (AkrAction.Current.Move(AxisName.PRX, teachpointX + xOffset, (int)AxisSpeed.PRX, (int)AxisAcc.PRX) != 0 ||
            AkrAction.Current.Move(AxisName.PRY, teachpointY + yOffset, (int)AxisSpeed.PRY, (int)AxisAcc.PRY) != 0)
            {

                return false;
            }
            return true;
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
            var point = GlobalManager.Current.RecheckRecylePos;
            return MovePos(point.X, point.Y);
        }
        public bool Toss()
        {
            if(!MoveToBinPos())
            {
                return false;
            }

            if (!ZDown())
            {
                return false;
            }


            if (!ClawOpen())
            {
                return false;
            }
            if (!ZUp())
            {
                return false;
            }
            return true;

        }
    }
}
