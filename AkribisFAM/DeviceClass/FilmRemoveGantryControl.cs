using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;

namespace AkribisFAM.DeviceClass
{
    public class FilmRemoveGantryControl
    {
        private int _step = 0;
        public bool CanPlaceRetry => _step < 5;
        public ErrorCode ProcessErrorCode;
        public string ProcessErrorMessage;
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

        public bool MoveToVisionPos(double teachpointX, double teachpointY, bool waitMotionDone = true)
        {
            if (!ZUp())
            {
                return false;
            }
            if (!ClawClose())
            {
                return false;
            }

            if (AkrAction.Current.MoveRecheckXY(teachpointX + (xOffset) + 0, teachpointY + (-yOffset) + 0, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {

                return false;
            }
            return true;
        }
        public bool VacOn()
        {

            return (IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 1));
        }
        public bool VacOff()
        {
            return (IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_2Peeling_Recheck_vacuum1_Supply, 0));
        }
        public bool RemoveFilm(double teachpointX, double teachpointY)
        {
            _step = 0;
            if (_step == 0)
            {
                if (!ZUp())
                {
                    ProcessErrorMessage = $"Peeler failed to move to safe position";
                    ProcessErrorCode = ErrorCode.motionErr;
                    return false;
                }
                _step = 1;
            }
            if (_step == 1)
            {
                if (!MovePos(teachpointX, teachpointY))
                {
                    ProcessErrorMessage = $"Failed to move to x:{teachpointX}, y:{teachpointY}";
                    ProcessErrorCode = ErrorCode.motionErr;
                    return false;
                }
                _step = 2;
            }
            if (_step == 2)
            {
                if (!ClawOpen())
                {
                    ProcessErrorMessage = $"Failed to open claw";
                    ProcessErrorCode = ErrorCode.PneumaticErr;
                    return false;
                }
                _step = 3;
            }
            if (_step == 3)
            {
                if (!ZDown())
                {
                    ProcessErrorMessage = $"Failed to Z down";
                    ProcessErrorCode = ErrorCode.motionErr;
                    _step = -1;
                }
                _step = 4;
            }
            if (_step == 4)
            {
                if (!ClawClose())
                {
                    ProcessErrorMessage = $"Failed to close claw";
                    ProcessErrorCode = ErrorCode.PneumaticErr;
                    _step = -1;
                }
                _step = 5;
            }
            //offset depend on product
            if (_step == 5)
            {
                if (!ZSafe())
                {
                    ProcessErrorMessage = $"Failed to move Z to safe position";
                    ProcessErrorCode = ErrorCode.PneumaticErr;
                    _step = -1;
                }
                _step = 6;
            }

            if (_step == -1)
            {
                ZUp();
                return false;
            }

            _step = 0;
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
            _step = 0;
            if (_step == 0)
            {
                if (!ZUp())
                {
                    ProcessErrorMessage = $"Peeler failed to move to safe position";
                    ProcessErrorCode = ErrorCode.motionErr;
                    return false;
                }
                _step = 1;
            }
            if (_step == 1)
            {
                if (!MoveToBinPos())
                {
                    ProcessErrorMessage = $"Peeler failed to move to bin position";
                    ProcessErrorCode = ErrorCode.motionErr;
                    return false;
                }
                _step = 2;
            }
            if (_step == 2)
            {
                //if (!ZDown())
                //{
                //    ProcessErrorMessage = $"Peeler failed to move to Z down at bin position";
                //    ProcessErrorCode = ErrorCode.motionErr;
                //    return false;
                //}
                _step = 3;
            }

            if (_step == 3)
            {
                if (!VacOn())
                {
                    ProcessErrorMessage = $"Fail to turn on vacuum";
                    ProcessErrorCode = ErrorCode.IOErr;
                    return false;
                }
                _step = 4;
            }
            //if (!ZDown())
            //{
            //    return false;
            //}
            if (_step == 4)
            {
                if (!ClawOpen())
                {
                    ProcessErrorMessage = $"Failed to open claw";
                    ProcessErrorCode = ErrorCode.PneumaticErr;
                    return false;
                }
                _step = 5;
            }
            if (_step == 5)
            {
                if (!VacOff())
                {
                    ProcessErrorMessage = $"Failed to turn off";
                    ProcessErrorCode = ErrorCode.IOErr;
                    return false;
                }
                _step = 6;
            }
            if (_step == 1)
            {
                if (!ZUp())
                {
                    ProcessErrorMessage = $"Peeler failed to move to safe position";
                    ProcessErrorCode = ErrorCode.motionErr;
                    return false;
                }
                _step = 2;
            }
            if (_step == 1)
            {
                if (!ClawClose())
                {
                    ProcessErrorMessage = $"Failed to close claw";
                    ProcessErrorCode = ErrorCode.PneumaticErr;
                    return false;
                }
                _step = 2;
            }
            return true;

        }
    }
}
