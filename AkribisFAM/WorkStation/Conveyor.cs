using System;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Threading;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Util;

namespace AkribisFAM.WorkStation
{
    public class Conveyor : WorkStationBase
    {
        private static DateTime startTime = DateTime.Now;
        private static Conveyor _instance;
        public override string Name => nameof(Conveyor);

        private ErrorCode errorCode;

        public static Conveyor Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new Conveyor();
                    }
                }
                return _instance;
            }
        }


        public override void Initialize()
        {
            startTime = DateTime.Now;
            MoveConveyorAll();
        }

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }
        public bool ReadIO(IO_INFunction_Table index)
        {
            //return AutorunManager.Current.ReadIO(index);
            if (IOManager.Instance.INIO_status[(int)index] == 0)
            {
                return true;
            }
            else if (IOManager.Instance.INIO_status[(int)index] == 1)
            {
                return false;
            }
            else
            {
                ErrorManager.Current.Insert(ErrorCode.IOErr, $"Failed to read {index.ToString()}");
                return false;
            }
        }

        public void SetIO(IO_OutFunction_Table index, int value)
        {
            IOManager.Instance.IO_ControlStatus(index, value);
        }

        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
            delta = 3000;
            DateTime time = DateTime.Now;
            bool ret = false;
            while ((DateTime.Now - time).TotalMilliseconds < delta)
            {
                if (ReadIO(index) == value)
                {
                    ret = true;
                    break;
                }
                Thread.Sleep(50);
            }

            return ret;
        }


        public int MoveConveyorAll()
        {
            return AkrAction.Current.MoveAllConveyor(); //.MoveConveyorAll(vel);
        }
        public int StopConveyor()
        {
            return AkrAction.Current.StopConveyor();
        }
        public void AllWorkLiftCylinderRetract()
        {
            AllWorkStopCylinderAct(0, 1);

            SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract, 1);

            SetIO(IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend, 0);
            SetIO(IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract, 1);



            WaitIO(99999999, IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos, true);
            WaitIO(99999999, IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos, true);



        }
        public void AllWorkStopCylinderAct(int extendValue, int retractValue)
        {
            SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, extendValue);
            SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, retractValue);
            SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, extendValue);
            SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, retractValue);
            SetIO(IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend, extendValue);
            SetIO(IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract, retractValue);
            //SetIO(IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend, extendValue);
            //SetIO(IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract, retractValue);


            if (extendValue == 1)
            {
                WaitIO(499, IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos, true);
            }
            if (retractValue == 1)
            {
                WaitIO(499, IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos, true);
                WaitIO(499, IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos, true);
            }


        }

        public void LiftUpRelatedTray(IO_OutFunction_Table IOName1, IO_OutFunction_Table IOName2, IO_OutFunction_Table IOName3, IO_OutFunction_Table IOName4, IO_INFunction_Table IOName5, IO_INFunction_Table IOName6)
        {
            SetIO(IOName1, 1);
            SetIO(IOName2, 0);

            SetIO(IOName3, 1);
            SetIO(IOName4, 0);

            WaitIO(99999, IOName5, true);
            WaitIO(99999, IOName6, true);

        }
        #region Lifter and gate control
        public bool GateUp(ConveyorStation workstationNum, bool wait = true)
        {
            IO_OutFunction_Table IOName1;
            IO_OutFunction_Table IOName2;
            IO_INFunction_Table IOName3;
            IO_INFunction_Table IOName4;

            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract;
                    IOName3 = IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos;

                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract;
                    IOName3 = IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract;
                    IOName3 = IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract;
                    IOName3 = IO_INFunction_Table.IN3_6Stopping_cylinder_4_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos;
                    break;
                default:
                    return false;
            }
            SetIO(IOName1, 1);
            SetIO(IOName2, 0);

            if (!wait) return true;

            return (WaitIO(3000, IOName3, false) &&
            WaitIO(3000, IOName4, true));

        }
        public bool GateDown(ConveyorStation workstationNum, bool wait = true)
        {
            IO_OutFunction_Table IOName1;
            IO_OutFunction_Table IOName2;
            IO_INFunction_Table IOName3;
            IO_INFunction_Table IOName4;
            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract;
                    IOName3 = IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos;

                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract;
                    IOName3 = IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract;
                    IOName3 = IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend;
                    IOName2 = IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract;
                    IOName3 = IO_INFunction_Table.IN3_6Stopping_cylinder_4_extend_InPos;
                    IOName4 = IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos;
                    break;
                default:
                    return false;
            }
            SetIO(IOName1, 0);
            SetIO(IOName2, 1);
            if (!wait) return true;

            return (WaitIO(3000, IOName3, true) &&
            WaitIO(3000, IOName4, false));

        }
        public bool LiftUpRelatedTray(ConveyorStation workstationNum, bool wait = true)
        {
            IO_OutFunction_Table IOName1;
            IO_OutFunction_Table IOName2;
            IO_OutFunction_Table IOName3;
            IO_OutFunction_Table IOName4;
            IO_INFunction_Table IOName5;
            IO_INFunction_Table IOName6;
            IO_INFunction_Table IOName7;
            IO_INFunction_Table IOName8;
            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract;


                    IOName5 = IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract;

                    IOName5 = IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract;

                    IOName5 = IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_OutFunction_Table.OUT1_124_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_134_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_124_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_134_lift_cylinder_retract;

                    IOName5 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    break;
                default:
                    return false;
            }

            SetIO(IOName1, 1);
            SetIO(IOName2, 0);
            SetIO(IOName3, 1);
            SetIO(IOName4, 0);

            if (!wait) return true;

            return (WaitIO(99999, IOName5, false) &&
                    WaitIO(99999, IOName6, false) &&
                    WaitIO(99999, IOName7, true) &&
                    WaitIO(99999, IOName8, true));
        }
        public bool LiftDownRelatedTray(ConveyorStation workstationNum, bool wait = true)
        {
            IO_OutFunction_Table IOName1;
            IO_OutFunction_Table IOName2;
            IO_OutFunction_Table IOName3;
            IO_OutFunction_Table IOName4;
            IO_INFunction_Table IOName5;
            IO_INFunction_Table IOName6;
            IO_INFunction_Table IOName7;
            IO_INFunction_Table IOName8;
            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract;


                    IOName5 = IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract;

                    IOName5 = IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract;

                    IOName5 = IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_OutFunction_Table.OUT1_124_lift_cylinder_extend;
                    IOName2 = IO_OutFunction_Table.OUT1_134_lift_cylinder_retract;
                    IOName3 = IO_OutFunction_Table.OUT1_124_lift_cylinder_extend;
                    IOName4 = IO_OutFunction_Table.OUT1_134_lift_cylinder_retract;

                    IOName5 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName6 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName7 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    IOName8 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    break;
                default:
                    return false;
            }

            SetIO(IOName1, 0);
            SetIO(IOName2, 1);
            SetIO(IOName3, 0);
            SetIO(IOName4, 1);

            if (!wait) return true;
            return (WaitIO(3000, IOName5, true) &&
                    WaitIO(3000, IOName6, true) &&
                    WaitIO(3000, IOName7, false) &&
                    WaitIO(3000, IOName8, false));

        }
        #endregion
        #region Sensor check
        public bool GateUpSensorCheck(ConveyorStation workstationNum)
        {
            IO_INFunction_Table IOName1;
            IO_INFunction_Table IOName2;
            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos;

                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_INFunction_Table.IN3_6Stopping_cylinder_4_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos;
                    break;
                default:
                    return false;
            }

            return ReadIO(IOName1) && !ReadIO(IOName2); //IO is inverted. TBC
        }
        public bool GateDownSensorCheck(ConveyorStation workstationNum)
        {
            IO_INFunction_Table IOName1;
            IO_INFunction_Table IOName2;
            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos;

                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_INFunction_Table.IN3_6Stopping_cylinder_4_extend_InPos;
                    IOName2 = IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos;
                    break;
                default:
                    return false;
            }

            var cond1 = ReadIO(IOName1);
            var cond2 = ReadIO(IOName2);

            return !cond1 && cond2;
        }

        public bool LiftUpRelatedTraySensorCheck(ConveyorStation workstationNum)
        {
            IO_INFunction_Table IOName1;
            IO_INFunction_Table IOName2;
            IO_INFunction_Table IOName3;
            IO_INFunction_Table IOName4;
            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    break;

                default:
                    return false;
            }

            return !ReadIO(IOName1) && !ReadIO(IOName2) && ReadIO(IOName3) && ReadIO(IOName4); //FFTT

        }
        public bool LiftDownRelatedTraySensorCheck(ConveyorStation workstationNum)
        {
            IO_INFunction_Table IOName1;
            IO_INFunction_Table IOName2;
            IO_INFunction_Table IOName3;
            IO_INFunction_Table IOName4;
            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName2 = IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos;
                    IOName3 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    IOName4 = IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos;
                    break;

                default:
                    return false;
            }

            return ReadIO(IOName1) && ReadIO(IOName2) && !ReadIO(IOName3) && !ReadIO(IOName4); //TTFF

        }
        public bool TrayPresenceCheck(ConveyorStation workstationNum)
        {
            IO_INFunction_Table IOName1;
            IO_INFunction_Table IOName2;

            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_INFunction_Table.IN1_0Slowdown_Sign1;
                    IOName2 = IO_INFunction_Table.IN1_4Stop_Sign1;
                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_INFunction_Table.IN1_1Slowdown_Sign2;
                    IOName2 = IO_INFunction_Table.IN1_5Stop_Sign2;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_INFunction_Table.IN1_2Slowdown_Sign3;
                    IOName2 = IO_INFunction_Table.IN1_6Stop_Sign3;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_INFunction_Table.IN1_3Slowdown_Sign4;
                    IOName2 = IO_INFunction_Table.IN1_7Stop_Sign4;
                    return ReadIO(IOName2);
                default:
                    return false;
            }
            return !ReadIO(IOName1) && ReadIO(IOName2);
        }

        public bool TrayAtRejectStation()
        {
            IO_INFunction_Table IOName1 = IO_INFunction_Table.IN6_0NG_plate_1_in_position;

            return !ReadIO(IOName1);
        }
        public bool RejectCoverClose()
        {
            IO_INFunction_Table IOName1 = IO_INFunction_Table.IN1_8NG_cover_plate1;
            IO_INFunction_Table IOName2 = IO_INFunction_Table.IN1_9NG_cover_plate2;


            return !ReadIO(IOName1) && !ReadIO(IOName2);
        }
        public bool TrayLeaveAndClearCheck(ConveyorStation workstationNum)
        {
            IO_INFunction_Table IOName1;
            IO_INFunction_Table IOName2;

            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_INFunction_Table.IN1_0Slowdown_Sign1;
                    IOName2 = IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1;
                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_INFunction_Table.IN1_1Slowdown_Sign2;
                    IOName2 = IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_INFunction_Table.IN1_2Slowdown_Sign3;
                    IOName2 = IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_INFunction_Table.IN1_2Slowdown_Sign3;
                    IOName2 = IO_INFunction_Table.IN6_7plate_has_left_Behind_the_stopping_cylinder4;
                    break;
                default:
                    return false;
            }

            var cond1 = ReadIO(IOName1);
            var cond2 = ReadIO(IOName2);

            return cond1 && !cond2;
        }
        public bool TraySeatProperly(ConveyorStation workstationNum)
        {
            IO_INFunction_Table IOName1;

            switch (workstationNum)
            {
                case ConveyorStation.Laser:
                    IOName1 = IO_INFunction_Table.IN1_12bord_lift_in_position1;
                    break;
                case ConveyorStation.Foam:
                    IOName1 = IO_INFunction_Table.IN1_13bord_lift_in_position2;
                    break;
                case ConveyorStation.Recheck:
                    IOName1 = IO_INFunction_Table.IN1_14bord_lift_in_position3;
                    break;
                case ConveyorStation.Reject:
                    IOName1 = IO_INFunction_Table.IN1_15bord_lift_in_position4; //NG plate in pos?
                    break;
                default:
                    return false;
            }

            return ReadIO(IOName1);
        }

        #endregion
        public override bool AutoRun() //rayner version 2
        {
            var byPassLaserProcess = true;
            var byPassFoamProcess = false;
            var byPassRecheckProcess = true;
            //if (MoveConveyorAll() != 0) return false;
            try
            {
                //while (!token.IsCancellationRequested)
                {

                    //todo: check machine stop to exit thread.
                    //todo: process SMEMA signal
                    CheckSMEMAInput(out var canSendTrayOut, out var isGoodTrayAvailable, out var isBypassTrayAvailable);
                    SendSMEMAOutput(canReceiveTray, canSendGoodTray, canSendBypassTray);

                    if (isBypassTrayAvailable)
                    {
                        //todo: bypass tray handling
                        //ConveyorTrays[(int)ConveyorStation.Laser].isBypass = true;
                        //ConveyorTrays[(int)ConveyorStation.Foam].isBypass = true;
                        //ConveyorTrays[(int)ConveyorStation.Recheck].isBypass = true;
                    }


                    switch (station[(int)currentstation]) //process 4 station
                    {
                        case StationState.Empty:
                            switch (steps[(int)currentstation])
                            {
                                case 0: //if station is empty, can allow tray from previous machine

                                    //process SMEMA input tray/////////////////////////
                                    if (isGoodTrayAvailable && currentstation == ConveyorStation.Laser)
                                    {
                                        TraySendingNextStation[(int)ConveyorStation.Laser] = true;
                                    }
                                    //////////////////////////////////////////////////

                                    if (TraySendingNextStation[(int)currentstation])
                                    {
                                        //REJECT only - Check cover closed 
                                        if (currentstation == ConveyorStation.Reject)
                                        {
                                            if (!RejectCoverClose())
                                            {
                                                return ErrorManager.Current.Insert(ErrorCode.RejectCoverOpened, $"!RejectCoverClose()");
                                            }
                                        }
                                        ///////////////////////////////
                                        steps[(int)currentstation] = 1;
                                        starttime[(int)currentstation] = DateTime.Now;
                                    }

                                    break;
                                case 1: //move end stopper up when clear
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (TrayLeaveAndClearCheck(currentstation) && !ConveyorTrays[(int)currentstation].HasTray) /*&& GateDownSensorCheck(currentstation)*/ //tbc if need gatedowncheck
                                        {
                                            if (counters[(int)currentstation] > 2)  //use counter to delay
                                            {

                                                status[(int)currentstation] = GateUp(currentstation, false);
                                                if (!status[(int)currentstation])
                                                {
                                                    return ErrorManager.Current.Insert(ErrorCode.IOErr, $"TrayLeaveAndClearCheck({currentstation},false)");
                                                    //throw new Exception("Output trigger failed");
                                                }
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 2;
                                                starttime[(int)currentstation] = DateTime.Now;
                                            }
                                            counters[(int)currentstation]++;
                                        }
                                    }
                                    else
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.TrayLeaveSensorErr, $"(TrayLeaveAndClearCheck(currentstation) && !ConveyorTrays[(int)currentstation].hasTray)");

                                    }
                                    break;
                                case 2: //wait stopper up
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (GateUpSensorCheck(currentstation))
                                        {
                                            if (counters[(int)currentstation] > 2)  //use counter to delay
                                            {
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 9;
                                            }
                                        }
                                        counters[(int)currentstation]++;
                                    }
                                    else
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.GateReedSwitchTimeOut, $"GateUpSensorCheck{currentstation}");
                                    }
                                    break;
                                case 9: //goto next station state
                                    steps[(int)currentstation] = 0;
                                    TraySendingNextStation[(int)currentstation] = false;
                                    station[(int)currentstation] = StationState.TrayIncoming;
                                    starttime[(int)currentstation] = DateTime.Now;
                                    break;
                            }

                            break;
                        case StationState.TrayIncoming: // to wait for tray reach sensor and lift tray for process
                            switch (steps[(int)currentstation]) //(laserstep)(laserstep)
                            {
                                case 0: //wait tray reach end stopper

                                    //slowdown sensor sequence - not in use
                                    //if (ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1))
                                    //{
                                    //}
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 5000)
                                    {
                                        //if detect tray
                                        if (TrayPresenceCheck(currentstation))
                                        {
                                            if (counters[(int)currentstation] > 2)
                                            {
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 1;
                                                if (currentstation == ConveyorStation.Reject) //disable interlock if tray detected
                                                {
                                                    rejectraymoving = false;
                                                }
                                            }
                                        }
                                        counters[(int)currentstation]++;

                                    }
                                    else
                                    {
                                        counters[(int)currentstation] = 0;
                                        return ErrorManager.Current.Insert(ErrorCode.IncomingTrayTimeOut, $"TrayPresenceCheck({currentstation})");

                                    }
                                    break;
                                case 1: //lift tray
                                    status[(int)currentstation] = LiftUpRelatedTray(currentstation, false);
                                    if (!status[(int)currentstation])
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.IOErr, $"LiftUpRelatedTray({currentstation},false)");
                                        //throw new Exception("Output trigger failed");
                                    }
                                    steps[(int)currentstation] = 2;
                                    starttime[(int)currentstation] = DateTime.Now;
                                    break;
                                case 2: //wait tray lifted
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (LiftUpRelatedTraySensorCheck(currentstation))
                                        {
                                            if (counters[(int)currentstation] > 2)  //use counter to delay
                                            {
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 3;
                                                starttime[(int)currentstation] = DateTime.Now;
                                            }
                                        }
                                        counters[(int)currentstation]++;
                                    }
                                    else
                                    {
                                        counters[(int)currentstation] = 0;
                                        return ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"LiftUpRelatedTraySensorCheck({currentstation})");

                                    }
                                    break;
                                case 3: //confirm tray seat properly
                                        //if (ReadIO(IO_INFunction_Table
                                        //        .IN1_12bord_lift_in_position1)) //todo:function to check station IO
                                        //{
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (TraySeatProperly(currentstation))
                                        {
                                            //// REMOVE
                                            //steps[(int)currentstation] = 4;
                                            //break;
                                            ////
                                            switch (currentstation)
                                            {
                                                case ConveyorStation.Laser:
                                                    //ConveyorTrays[(int)ConveyorStation.Laser] =
                                                    //    new TrayData("LaserStationTray", TrackerType.Tray, 3, 4)
                                                    //    {
                                                    //        hasTray = true,
                                                    //    };
                                                    ConveyorTrays[(int)ConveyorStation.Laser].Reset();
                                                    ConveyorTrays[(int)ConveyorStation.Laser].HasTray = true;

                                                    steps[(int)currentstation] = 9;
                                                    break;
                                                case ConveyorStation.Foam:
                                                    ConveyorTrays[(int)ConveyorStation.Foam].Copy(
                                                        (TrayData)ConveyorTraysSending[(int)ConveyorStation.Laser]);
                                                    ConveyorTraysSending[(int)ConveyorStation.Laser].Reset();
                                                    steps[(int)currentstation] = 9;
                                                    break;
                                                case ConveyorStation.Recheck:
                                                    ConveyorTrays[(int)ConveyorStation.Recheck].Copy(
                                                        (TrayData)ConveyorTraysSending[(int)ConveyorStation.Foam]);
                                                    ConveyorTraysSending[(int)ConveyorStation.Foam].Reset();
                                                    steps[(int)currentstation] = 9;
                                                    break;
                                                case ConveyorStation.Reject:
                                                    //todo: track reject tray
                                                    steps[(int)currentstation] = 4;
                                                    break;
                                            }
                                            counters[(int)currentstation] = 0;
                                        }
                                        counters[(int)currentstation]++;

                                    }
                                    else
                                    {
                                        counters[(int)currentstation] = 0;
                                        return ErrorManager.Current.Insert(ErrorCode.TrayPresentSensorTimeOut, $"TraySeatProperly({currentstation})");
                                    }
                                    break;
                                case 4: //reject only - lifter down
                                    status[(int)currentstation] = LiftDownRelatedTray(currentstation, false);
                                    if (!status[(int)currentstation])
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.IOErr, $"LiftDownRelatedTray({currentstation},false)");
                                        //throw new Exception("Output trigger failed");
                                    }
                                    steps[(int)currentstation] = 5;
                                    starttime[(int)currentstation] = DateTime.Now;
                                    break; ;
                                case 5: //reject only - check lifter down sensor
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (LiftDownRelatedTraySensorCheck(currentstation))
                                        {
                                            if (counters[(int)currentstation] > 2)  //use counter to delay
                                            {
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 6;
                                                starttime[(int)currentstation] = DateTime.Now;
                                            }
                                        }
                                        counters[(int)currentstation]++;
                                    }
                                    else
                                    {
                                        counters[(int)currentstation] = 0;
                                        return ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"LiftDownRelatedTraySensorCheck({currentstation})");
                                    }
                                    break;
                                case 6: // reject only - check tray at reject top station
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (TrayAtRejectStation())
                                        {
                                            if (counters[(int)currentstation] > 10)  //use counter to delay
                                            {
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 9;
                                            }
                                        }
                                        counters[(int)currentstation]++;
                                    }
                                    else
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.MissingNGTray, $"TrayAtRejectStation({currentstation})");
                                    }
                                    break;
                                case 9: //goto next station state
                                    steps[(int)currentstation] = 0;
                                    station[(int)currentstation] = StationState.InProcess;
                                    break;
                            }

                            break;
                        case StationState.InProcess: //process station interlock vs conveyor + stop gate lower
                            switch (steps[(int)currentstation])
                            {
                                case 0: //set station ready to process
                                    StationReadyStatus[(int)currentstation] = true;
                                    //if (!ConveyorTrays[(int)currentstation].isBypass)
                                    //{
                                    switch (currentstation)
                                    {
                                        case ConveyorStation.Laser:
                                            if (!byPassLaserProcess)
                                            {
                                                LaiLiao.Current.SetTrayReadyToProcess();
                                            }
                                            break;

                                        case ConveyorStation.Foam:
                                            if (!byPassFoamProcess)
                                            {
                                                ZuZhuang.Current.SetTrayReadyToProcess();
                                            }
                                            break;

                                        case ConveyorStation.Recheck:
                                            if (!byPassRecheckProcess)
                                            {
                                                FuJian.Current.SetTrayReadyToProcess();
                                            }
                                            break;
                                    }
                                    //}
                                    steps[(int)currentstation] = 1;
                                    break;
                                case 1: // Processing ongoing, wait till station says done
                                        //switch (currentstation)
                                        //{
                                        //    case ConveyorStation.Laser:
                                        //        if (!LaiLiao.Current.IsProcessOngoing())
                                        //        {
                                    steps[(int)currentstation] = 2;
                                    //        }
                                    //        break;
                                    //}
                                    break;
                                case 2: //lower stopper
                                    status[(int)currentstation] = GateDown(currentstation, false);
                                    if (!status[(int)currentstation])
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.IOErr, $"GateDown({currentstation}, false)");
                                        //throw new Exception("Output trigger failed");
                                    }
                                    steps[(int)currentstation] = 3;
                                    starttime[(int)currentstation] = DateTime.Now;
                                    break;
                                case 3: // check gate down sensor
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (GateDownSensorCheck(currentstation))
                                        {
                                            if (counters[(int)currentstation] > 2)  //use counter to delay
                                            {
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 4;
                                            }
                                        }
                                        counters[(int)currentstation]++;
                                    }
                                    else
                                    {

                                        return ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"GateDownSensorCheck({currentstation})");
                                    }
                                    break;
                                case 4: //wait station complete signal from main process - decide pass or fail
                                    if (currentstation != ConveyorStation.Reject)
                                    {
                                        ////// BYPASS PROCESSING
                                        //StationReadyStatus[(int)currentstation] = false;
                                        //StationTrayStatus[(int)currentstation] = false;
                                        //////
                                        //if (!ConveyorTrays[(int)currentstation].isBypass)
                                        //{

                                        if (currentstation == ConveyorStation.Laser && byPassLaserProcess)
                                        {
                                            ProcessingDone(currentstation, true); //bypass tray, set as pass
                                            steps[(int)currentstation] = 5;
                                        }

                                        if (currentstation == ConveyorStation.Foam && byPassFoamProcess)
                                        {
                                            ProcessingDone(currentstation, true); //bypass tray, set as pass
                                            steps[(int)currentstation] = 5;
                                        }

                                        if (currentstation == ConveyorStation.Recheck && byPassRecheckProcess)
                                        {
                                            ProcessingDone(currentstation, true); //bypass tray, set as pass
                                            steps[(int)currentstation] = 5;
                                        }


                                        if (!StationReadyStatus[(int)currentstation])
                                        {
                                            ConveyorTrays[(int)currentstation].IsFail =
                                                !StationTrayStatus[(int)currentstation];
                                            steps[(int)currentstation] = 5;
                                        }
                                        //}
                                        //else
                                        //{
                                        //    ProcessingDone(currentstation, true); //bypass tray, set as pass
                                        //    steps[(int)currentstation] = 5;
                                        //}


                                    }
                                    else //reject handle
                                    {
                                        if (!StationReadyStatus[(int)currentstation]) //if receive ready signal
                                        {
                                            if (!TrayAtRejectStation() && RejectCoverClose()) //if no tray and cover close
                                            {
                                                steps[(int)currentstation] = 9;
                                            }
                                            else
                                            {
                                                StationReadyStatus[(int)currentstation] = true;
                                                return ErrorManager.Current.Insert(ErrorCode.NGOccupied, $"(!TrayAtRejectStation() && RejectCoverClose())");
                                                //throw new Exception("Reject not cleared");
                                            }
                                        }
                                    }

                                    break;
                                case 5: //wait next station empty, or reject empty
                                    if (ConveyorTrays[(int)currentstation].IsFail)
                                    {
                                        if (rejectstation != StationState.Empty)
                                            return ErrorManager.Current.Insert(ErrorCode.NGOccupied, $"ConveyorTrays[(int){currentstation}].isFail");
                                        //throw new Exception("Reject station not available");
                                        //error if tray still on reject station
                                        if (currentstation == ConveyorStation.Laser ||
                                            currentstation == ConveyorStation.Foam)
                                        {
                                            rejectraymoving = true; //move to reject station, need to bypass
                                        }

                                        TraySendingNextStation[(int)ConveyorStation.Reject] = true;
                                        steps[(int)currentstation] = 9;
                                    }
                                    else
                                    {
                                        switch (currentstation)
                                        {
                                            case ConveyorStation.Laser:
                                                if (station[(int)ConveyorStation.Foam] == StationState.Empty)
                                                {
                                                    TraySendingNextStation[(int)ConveyorStation.Foam] = true;
                                                    steps[(int)currentstation] = 9;
                                                }

                                                break;
                                            case ConveyorStation.Foam:
                                                if (station[(int)ConveyorStation.Recheck] == StationState.Empty)
                                                {
                                                    TraySendingNextStation[(int)ConveyorStation.Recheck] = true;
                                                    steps[(int)currentstation] = 9;
                                                }

                                                break;
                                            case ConveyorStation.Recheck:
                                                //send SMEMA output tray ready
                                                canSendGoodTray = true;
                                                if (canSendTrayOut) //If SMEMA ask for tray
                                                {
                                                    steps[(int)currentstation] = 9;
                                                }
                                                break;
                                        }
                                    }

                                    break;
                                case 9: //goto next station state
                                    steps[(int)currentstation] = 0;
                                    station[(int)currentstation] = StationState.TrayOutgoing;
                                    break;
                            }

                            break;
                        case StationState.TrayOutgoing: //lifter down and clear tray
                            switch (steps[(int)currentstation]) //(laserstep)
                            {
                                case 0://Blocking until tray can send out
                                       //if (currentstation == ConveyorStation.Recheck ) //SMEMA request tray
                                       //{
                                       //    steps[(int)currentstation] = 1;

                                    //}
                                    //else //check bypass tray moving. if moving then block
                                    //{
                                    var currenttrayfailed = ConveyorTrays[(int)currentstation].IsFail; //if current tray is fail, reject
                                    if (/*!bypasstraymoving && */!rejectraymoving || currenttrayfailed)
                                        steps[(int)currentstation] = 1;
                                    //}

                                    break;
                                case 1: //Lifter down
                                    if (currentstation != ConveyorStation.Reject)
                                    {
                                        status[(int)currentstation] = LiftDownRelatedTray(currentstation, false);
                                        if (!status[(int)currentstation])
                                        {
                                            return ErrorManager.Current.Insert(ErrorCode.IOErr, $"LiftDownRelatedTray({currentstation},false)");
                                            //throw new Exception("Output trigger failed");
                                        }
                                    }

                                    steps[(int)currentstation] = 2;
                                    starttime[(int)currentstation] = DateTime.Now;
                                    break;
                                case 2://check lifter down sensor
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (LiftDownRelatedTraySensorCheck(currentstation))
                                        {
                                            if (counters[(int)currentstation] > 2)  //use counter to delay
                                            {
                                                counters[(int)currentstation] = 0;
                                                steps[(int)currentstation] = 3;
                                                starttime[(int)currentstation] = DateTime.Now;
                                            }
                                        }
                                        counters[(int)currentstation]++;
                                    }
                                    else
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"LiftDownRelatedTraySensorCheck({currentstation})");

                                    }
                                    break;
                                case 3: //wait tray leave zone sensor & side slow sensor 
                                    if ((DateTime.Now - starttime[(int)currentstation]).TotalMilliseconds <= 3000)
                                    {
                                        if (TrayLeaveAndClearCheck(currentstation))
                                        {
                                            ConveyorTraysSending[(int)currentstation].Copy((TrayData)ConveyorTrays[(int)currentstation]); //transfer data to sending object
                                            //var trackerName = "";
                                            //switch (currentstation)
                                            //{
                                            //    case ConveyorStation.Laser:
                                            //        trackerName = "LaserStationTray";
                                            //        break;
                                            //    case ConveyorStation.Foam:
                                            //        trackerName = "FoamAssemblyStationTray";
                                            //        break;
                                            //    case ConveyorStation.Recheck:
                                            //        trackerName = "RecheckStationTray";
                                            //        break;
                                            //    case ConveyorStation.Reject:
                                            //        trackerName = "GoodOutGoingStationTray";
                                            //        break;
                                            //    default:
                                            //        trackerName = "";
                                            //        break;
                                            //}
                                            //ConveyorTrays[(int)currentstation] = new TrayData(trackerName, TrackerType.Tray, 3, 4); //clear data
                                            ConveyorTrays[(int)currentstation].Reset(); //clear data
                                            steps[(int)currentstation] = 9;
                                        }
                                    }
                                    else
                                    {
                                        return ErrorManager.Current.Insert(ErrorCode.PneumaticErr, $"TrayLeaveAndClearCheck({currentstation})");
                                    }
                                    break;
                                case 9: //goto next station state
                                    steps[(int)currentstation] = 0;
                                    station[(int)currentstation] = StationState.Empty;
                                    //TraySendingNextStation[(int)ConveyorStation.Laser] = false;
                                    break;
                            }

                            break;
                    }

                    if (currentstation >= ConveyorStation.Reject)
                        currentstation = ConveyorStation.Laser;
                    else currentstation++;

                    Thread.Sleep(0);
                }
            }
            catch (Exception ex)
            {
                StopConveyor();
                //todo log error
            }

            return true;
        }

        public void ProcessingDone(ConveyorStation station, bool isPass)
        {
            StationReadyStatus[(int)station] = false;
            StationTrayStatus[(int)station] = isPass;
        }
        // M1 = previous,M2 = Akribis, M3 =next 

        //Machine ready to receive (output M2 to M1) = send to previous machine to send board over
        //Board available (input M1 to M2) = check signal from previous machine to check if good board available
        //Fail available (input M1 to M2) = check signal from previous machine to check if bypass board available

        //Machine ready to receive (input M3 to M2) = check signal from next machine to check if can send board 
        //Board available (output M2 to M3) = send to next machine if good board can be sent
        //Fail available (output M2 to M3) = send to next machine if fail board can be sent
        public void CheckSMEMAInput(out bool canSendTrayOut, out bool isGoodTrayAvailable, out bool isBypassTrayAvailable)
        {
            if (true) // Bypass SMEMA TODO: add bypass in settings
            {
                canSendTrayOut = true;
                isGoodTrayAvailable = true;
                isBypassTrayAvailable = true;
                return;
            }
            canSendTrayOut = ReadIO(IO_INFunction_Table.IN7_2MACHINE_READY_TO_RECEIVE);
            isGoodTrayAvailable = ReadIO(IO_INFunction_Table.IN7_0BOARD_AVAILABLE);
            isBypassTrayAvailable = ReadIO(IO_INFunction_Table.IN7_2MACHINE_READY_TO_RECEIVE);

        }
        public void SendSMEMAOutput(bool canReceiveTray, bool canSendGoodTray, bool canSendBypassTray)
        {
            canReceiveTray = ReadIO(IO_INFunction_Table.IN7_2MACHINE_READY_TO_RECEIVE);
            canSendGoodTray = ReadIO(IO_INFunction_Table.IN7_0BOARD_AVAILABLE);
            canSendBypassTray = ReadIO(IO_INFunction_Table.IN7_2MACHINE_READY_TO_RECEIVE);

        }
        public void ResetAllStatusStatus()
        {
            laserstation = foamstation = recheckstation = rejectstation = StationState.Empty;
        }

        public void ResetAllTraySendingNextStation()
        {
            for (int i = 0; i < TraySendingNextStation.Length; i++)
                TraySendingNextStation[i] = false;
        }
        /// <summary>
        /// To reset all station ready status
        /// </summary>
        public void ResetAllStationReadyStatus()
        {
            for (int i = 0; i < StationReadyStatus.Length; i++)
                StationReadyStatus[i] = false;
        }
        /// <summary>
        /// To reset all tray status (pass / fail) for all station
        /// </summary>
        public void ResetAllStationTrayStatus()
        {
            for (int i = 0; i < StationTrayStatus.Length; i++)
                StationTrayStatus[i] = false;
        }

        public override void Paused()
        {
            StopConveyor();
        }

        public override void ResetAfterPause()
        {
            for (int i = 0; i < starttime.Length; i++)
            {
                starttime[i] = DateTime.Now;
            }
            for (int i = 0; i < counters.Length; i++)
            {
                counters[i] = 0;
            }

            //Restart if havent receive a tray
            if (station[(int)currentstation] == StationState.Empty || station[(int)currentstation] == StationState.TrayIncoming)
            {
                station[(int)currentstation] = StationState.Empty;
                steps[(int)currentstation] = 0;
                LiftDownRelatedTray(currentstation);
            }

            if (Conveyor.Current.station[(int)currentstation] == StationState.InProcess)
            {
                if (LiftUpRelatedTray(currentstation, true))
                {
                    if (!TrayPresenceCheck(currentstation))
                    {

                    }
                }
            }
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT2_15FFU, 1);

            Conveyor.Current.MoveConveyorAll();
            return;
        }

        public enum ConveyorStation
        {
            Laser,
            Foam,
            Recheck,
            Reject
        }
        public enum StationState
        {
            Empty,
            TrayIncoming,
            InProcess,
            TrayOutgoing

        }

        private ConveyorStation currentstation = ConveyorStation.Laser;
        private StationState laserstation, foamstation, recheckstation, rejectstation = StationState.Empty;

        private StationState[] station = new StationState[4];
        private bool[] actionstate = new bool[4];
        private bool[] status = new bool[4];
        private int[] steps = new int[4];
        private int[] counters = new int[4];
        private DateTime[] starttime = new DateTime[4];
        private Thread[] threads = new Thread[4];

        private bool actionstate_laser, actionstate_foam, actionstate_recheck, actionstate_reject = false;
        private bool laserstatus, foamstatus, recheckstatus, rejectstatus = false;
        private int laserstep, foamstep, recheckstep, rejectstep = 0;
        private int lasercounter, foamcounter, recheckcounter, rejectcounter = 0;
        private Thread laserthread, foamthread, recheckthread, rejecthread;

        private bool bypasstraymoving, rejectraymoving = false;
        public static bool[] StationReadyStatus = new bool[4];
        public static bool[] StationTrayStatus = new bool[4];
        private bool[] TraySendingNextStation = new bool[4];
        private bool canReceiveTray, canSendGoodTray, canSendBypassTray;

        public static TrayData[] ConveyorTrays = new TrayData[4];
        //{
        //    new TrayData("LaserStationTray",TrackerType.Tray,3,4),
        //    new TrayData("FoamAssemblyStationTray",TrackerType.Tray,3,4),
        //    new TrayData("RecheckStationTray",TrackerType.Tray,3,4),
        //    new TrayData("RejectOutGoingStationTray",TrackerType.Tray,3,4)
        //};
        private TrayData[] ConveyorTraysSending = new TrayData[4];
        //{
        //    new TrayData(),
        //    new TrayData(),
        //    new TrayData(),
        //    new TrayData()
        //};

        public class TrayData : ProductTracker
        {
            public TrayData() { }
            public TrayData(string trackerName, TrackerType type, int row, int col) : base(trackerName, type, row, col)
            {
            }
            public void Copy(TrayData trayData)
            {
                IsFail = trayData.IsFail;
                IsBypass = trayData.IsBypass;
                //CurrentStation = trayData.CurrentStation;
                HasTray = trayData.HasTray;
                Barcode = trayData.Barcode;
                //Name = this.Name,
                TrackerType = trayData.TrackerType;
                PartArray = trayData.PartArray.Select(x => new ProductData(x)).ToArray();
                Row = trayData.Row;
                Column = trayData.Column;
                TotalSize = trayData.TotalSize;

            }
            private bool _isFail = false;
            public bool IsFail
            {
                get { return _isFail; }
                set { _isFail = value; OnPropertyChanged(); }
            }
            private bool _isBypass = false;
            public bool IsBypass
            {
                get { return _isBypass; }
                set { _isBypass = value; OnPropertyChanged(); }
            }
            private string _barcode = "";
            public string Barcode
            {
                get { return _barcode; }
                set { _barcode = value; OnPropertyChanged(); }
            }
            //private ConveyorStation _currentStation ;
            //public ConveyorStation CurrentStation
            //{
            //    get { return _currentStation; }
            //    set { _currentStation = value; OnPropertyChanged(); }
            //}

            private bool _hasTray = false;
            public bool HasTray
            {
                get { return _hasTray; }
                set { _hasTray = value; OnPropertyChanged(); }
            }
            public override bool Reset()
            {
                IsFail = false;
                IsBypass = false;
                HasTray = false;
                Barcode = string.Empty;
                return base.Reset();
            }
        }
        #region Old Code
        //public override void AutoRun(CancellationToken token) //org CN team
        //{
        //    while (true)
        //    {
        //    STEP_Init:
        //        int data_AllStationTrayNumber = 0;                       //不包含NG工位

        //        GlobalManager.Current.flag_RangeFindingTrayArrived = 0;  //测距工位
        //        GlobalManager.Current.flag_assembleTrayArrived = 0;      //贴装工位
        //        GlobalManager.Current.flag_RecheckTrayArrived = 0;       //复检工位

        //        GlobalManager.Current.flag_TrayProcessCompletedNumber = 0;  //每个工位完成料盘处理后对此变量自增1
        //        GlobalManager.Current.flag_TrayArrivedNumber = 0;

        //        GlobalManager.Current.flag_RecheckStationHaveTray = 0;
        //        GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 0;

        //        GlobalManager.Current.flag_Bypass = 0;


        //    STEP_JudgeAllStationTrayNumberIsZero:
        //        Logger.WriteLog("皮带任务_判断设备内有无料盘");
        //        if (data_AllStationTrayNumber == 0)
        //        {
        //            goto STEP_WaitUpstreamEquipmentHaveTray;
        //        }


        //    //如果设备内已经有料盘，执行下面的逻辑
        //    STEP_WaitingAllTrayProcessCompleted:
        //        Logger.WriteLog("皮带任务_等待所有工位料盘产品处理完成");
        //        Logger.WriteLog("flag_TrayProcessCompletedNumber : " + GlobalManager.Current.flag_TrayProcessCompletedNumber.ToString());

        //        while (data_AllStationTrayNumber != GlobalManager.Current.flag_TrayProcessCompletedNumber) { System.Threading.Thread.Sleep(30); }


        //    STEP_JudgeIsBypass:
        //        Logger.WriteLog("皮带任务_判断是否为bypass");
        //        if (GlobalManager.Current.flag_Bypass == 1)
        //        {
        //            goto STEP_BypassStart;
        //        }

        //        GlobalManager.Current.flag_TrayProcessCompletedNumber = 0;


        //    STEP_JudeRecheckStationHaveTray:
        //        Logger.WriteLog("皮带任务_判断复检工位是否有料盘");
        //        if (GlobalManager.Current.flag_RecheckStationHaveTray == 0)
        //        {
        //            goto STEP_JudeUpstreamDeviceHaveTray;
        //        }

        //        Logger.WriteLog("皮带任务_等待NG工位允许料盘进入");
        //        while (GlobalManager.Current.flag_NGStationAllowTrayEnter != 1) { System.Threading.Thread.Sleep(30); }

        //    STEP_SetRecheckStationRequestOutflowTray:
        //        Logger.WriteLog("皮带任务_设置复检工位请求流出料盘");
        //        GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 1;
        //        GlobalManager.Current.flag_RecheckStationHaveTray = 0;

        //        Logger.WriteLog("皮带任务_所有工位料盘数量自减1(不包含NG工位)");
        //        data_AllStationTrayNumber -= 1;


        //    STEP_JudeUpstreamDeviceHaveTray:
        //        Logger.WriteLog("皮带任务_判断上游设备是否有料盘");
        //        //if (!ReadIO(IO_INFunction_Table.IN7_0BOARD_AVAILABLE))
        //        //{
        //        //    goto STEP_AllWorkTrayGoDown;
        //        //}
        //        if (!GlobalManager.Current.IO_test1)
        //        {
        //            goto STEP_AllWorkTrayGoDown;
        //        }
        //        GlobalManager.Current.IO_test1 = false;

        //    STEP_JudeUpstreamDeviceTrayisFailed:

        //        bool isFliledTray = WaitIO(999, IO_INFunction_Table.IN7_1FAILED_BOARD_AVAILABLE_OPTIONAL, true);
        //        if (isFliledTray)
        //        {
        //            GlobalManager.Current.flag_Bypass = 1;
        //            Logger.WriteLog("皮带任务_上游设备输送failed料盘后等待NG工位允许进板");
        //            while (GlobalManager.Current.flag_NGStationAllowTrayEnter != 1) { System.Threading.Thread.Sleep(30); }

        //            Logger.WriteLog("皮带任务_isFliledTray后允许上游设备送料盘");
        //            SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);
        //            System.Threading.Thread.Sleep(1000);
        //            SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);
        //            //ToBypassStep
        //        }



        //    STEP_SetDeviceAllowEnterTray:

        //        Logger.WriteLog("皮带任务_所有工位顶升气缸下降(不包含NG工位,先控制阻挡气缸下降再向上游设备要板)");
        //        AllWorkLiftCylinderRetract();

        //        Logger.WriteLog("皮带任务_再次允许上游设备送料盘");
        //        SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);

        //        Logger.WriteLog("皮带任务_所有工位料盘数量再次自增1(不包含NG工位)");
        //        data_AllStationTrayNumber += 1;


        //    STEP_AllWorkTrayGoDown:
        //        Logger.WriteLog("皮带任务_所有工位顶升气缸下降(不包含NG工位)");
        //        AllWorkLiftCylinderRetract();

        //    STEP_WaitTrayThroughStopCylinder:
        //        Logger.WriteLog("皮带任务_皮带再次高速运行");
        //        MoveConveyor((int)AxisSpeed.BL1);

        //        bool IN1_10 = ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1);
        //        bool IN1_11 = ReadIO(IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2);
        //        bool IN6_6 = ReadIO(IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3);

        //        Logger.WriteLog("皮带任务_等待任一料盘触发流出阻挡气缸光电信号");
        //        System.Threading.Thread.Sleep(3000);
        //        //while (IN1_10 == false && IN1_11 == false && IN6_6 == false)   //任一光电被触发后，就退出循环
        //        //{
        //        //    IN1_10 = ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1);
        //        //    IN1_11 = ReadIO(IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2);
        //        //    IN6_6 = ReadIO(IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3);

        //        //    System.Threading.Thread.Sleep(300);

        //        //}

        //        Logger.WriteLog("皮带任务_等待所有料盘流出阻挡气缸光电信号");
        //        //如果上游设备延迟给料太久，此次会有bug，不影响整体测试。后续再想方案处理TODO
        //        //while (IN1_10 == true || IN1_11 == true || IN6_6 == true)     //所有料盘流出此光电后，退出循环
        //        //{
        //        //    IN1_10 = ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1);
        //        //    IN1_11 = ReadIO(IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2);
        //        //    IN6_6 = ReadIO(IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3);

        //        //    System.Threading.Thread.Sleep(50);

        //        //}

        //        Logger.WriteLog("皮带任务_再次禁止上游设备送料盘");
        //        SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);

        //    STEP_WaitStopCylinderExtend:
        //        Logger.WriteLog("皮带任务_所有阻挡气缸伸出");
        //        AllWorkStopCylinderAct(1, 0);  //阻挡气缸伸出

        //        Logger.WriteLog("皮带任务_等待任一减速光电信号");

        //        bool IN1_0 = ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1);
        //        bool IN1_1 = ReadIO(IO_INFunction_Table.IN1_1Slowdown_Sign2);
        //        bool IN1_2 = ReadIO(IO_INFunction_Table.IN1_2Slowdown_Sign3);
        //        while (IN1_0 == true && IN1_1 == true && IN1_2 == true)
        //        {
        //            IN1_0 = ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1);
        //            IN1_1 = ReadIO(IO_INFunction_Table.IN1_1Slowdown_Sign2);
        //            IN1_2 = ReadIO(IO_INFunction_Table.IN1_2Slowdown_Sign3);
        //            System.Threading.Thread.Sleep(20);
        //        }
        //    STEP_BeltSlowDown:
        //        Logger.WriteLog("皮带任务_皮带减速");
        //        MoveConveyor(20);

        //    STEP_WaitAnyTrayArrived:

        //        Logger.WriteLog("皮带任务_等待任一料盘到阻挡位");
        //        bool IN1_4 = ReadIO(IO_INFunction_Table.IN1_4Stop_Sign1);
        //        bool IN1_5 = ReadIO(IO_INFunction_Table.IN1_5Stop_Sign2);
        //        bool IN1_6 = ReadIO(IO_INFunction_Table.IN1_6Stop_Sign3);
        //        while (IN1_4 == false && IN1_5 == false && IN1_6 == false)
        //        {
        //            IN1_4 = ReadIO(IO_INFunction_Table.IN1_4Stop_Sign1);
        //            IN1_5 = ReadIO(IO_INFunction_Table.IN1_5Stop_Sign2);
        //            IN1_6 = ReadIO(IO_INFunction_Table.IN1_6Stop_Sign3);

        //            System.Threading.Thread.Sleep(10);
        //        }
        //        Logger.WriteLog("皮带任务_皮带停止");
        //        StopConveyor();
        //        System.Threading.Thread.Sleep(200);

        //    STEP_LiftUpRelatedTray:
        //        //优先判断贴装位料盘
        //        Logger.WriteLog("皮带任务_判断贴装位料盘是否到位");
        //        if (IN1_5 == true)
        //        {
        //            Logger.WriteLog("皮带任务_贴装位料盘顶起");
        //            LiftUpRelatedTray(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend,
        //                              IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract,
        //                              IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend,
        //                              IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract,
        //                              IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos,
        //                              IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos);

        //            Logger.WriteLog("皮带任务_设置贴装位料盘就位");
        //            GlobalManager.Current.flag_assembleTrayArrived = 1;
        //        }

        //        Logger.WriteLog("皮带任务_判断测距位料盘是否到位");
        //        if (IN1_4 == true)
        //        {
        //            Logger.WriteLog("皮带任务_测距位料盘顶起");
        //            LiftUpRelatedTray(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend,
        //                              IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract,
        //                              IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend,
        //                              IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract,
        //                              IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos,
        //                              IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos);

        //            Logger.WriteLog("皮带任务_设置测距位料盘就位");
        //            GlobalManager.Current.flag_RangeFindingTrayArrived = 1;
        //        }

        //        Logger.WriteLog("皮带任务_判断复检位料盘是否到位");
        //        if (IN1_6 == true)
        //        {
        //            Logger.WriteLog("皮带任务_复检位料盘顶起");
        //            LiftUpRelatedTray(IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend,
        //                              IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract,
        //                              IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend,
        //                              IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract,
        //                              IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos,
        //                              IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos);
        //            Logger.WriteLog("皮带任务_设置复检位料盘就位");
        //            GlobalManager.Current.flag_RecheckTrayArrived = 1;
        //            GlobalManager.Current.flag_RecheckStationHaveTray = 1;
        //        }

        //    STEP_WaitAllTrayIsArrived:
        //        Logger.WriteLog("皮带任务_料盘就位数量自增1");
        //        GlobalManager.Current.flag_TrayArrivedNumber += 1;
        //        Logger.WriteLog("皮带任务_判断料盘是否全部就位");
        //        if (GlobalManager.Current.flag_TrayArrivedNumber != data_AllStationTrayNumber)
        //        {
        //            goto STEP_BeltSlowDown;
        //        }

        //    STEP_WaitStopCylinderRetract:
        //        Logger.WriteLog("皮带任务_料盘就位数量和料盘处理完成数量清零");
        //        GlobalManager.Current.flag_TrayArrivedNumber = 0;

        //        Logger.WriteLog("皮带任务_所有料盘顶升气缸下降(不包含NG工位)");
        //        AllWorkStopCylinderAct(0, 1);
        //        goto STEP_JudgeAllStationTrayNumberIsZero;





        //    //处理bypass料盘
        //    STEP_BypassStart:
        //        GlobalManager.Current.flag_Bypass = 0;

        //        SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
        //        SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);
        //        SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
        //        SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 1);

        //        WaitIO(1999, IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos, true);
        //        WaitIO(1999, IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos, true);

        //        GlobalManager.Current.flag_RecheckStationRequestOutflowTray = 1;

        //    STEP_BypassBeltStart:
        //        MoveConveyor((int)AxisSpeed.BL1);

        //    STEP_BypassWaitSlowDownSig4:
        //        WaitIO(99999, IO_INFunction_Table.IN1_7Stop_Sign4, true);
        //        WaitIO(99999, IO_INFunction_Table.IN1_7Stop_Sign4, false);
        //        StopConveyor();
        //        System.Threading.Thread.Sleep(200);
        //        goto STEP_JudgeAllStationTrayNumberIsZero;


        //    //处理进入设备内的第一个料盘
        //    STEP_WaitUpstreamEquipmentHaveTray:
        //        Logger.WriteLog("皮带任务_等待上游设备有料盘");
        //        while (!GlobalManager.Current.IO_test1)
        //        {
        //            Thread.Sleep(300);
        //        }
        //        GlobalManager.Current.IO_test1 = false;
        //        //WaitIO(99999999, IO_INFunction_Table.IN7_0BOARD_AVAILABLE, true);

        //        Logger.WriteLog("皮带任务_允许上游设备送料盘");
        //        SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);

        //        Logger.WriteLog("皮带任务_皮带高速运行");
        //        MoveConveyor((int)AxisSpeed.BL1);

        //    STEP_WaitSlowDownSig1:
        //        Logger.WriteLog("皮带任务_等待测距位减速光电信号");
        //        WaitIO(99999, IO_INFunction_Table.IN1_0Slowdown_Sign1, false);  //有信号时输入模块信号为0

        //        Logger.WriteLog("皮带任务_禁止上游设备送料盘");
        //        SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);

        //        Logger.WriteLog("皮带任务_皮带低速运行");
        //        MoveConveyor(20);

        //    STEP_WaitStopSig1:
        //        Logger.WriteLog("皮带任务_等待料盘到达测距位挡停气缸信号");
        //        WaitIO(99999, IO_INFunction_Table.IN1_4Stop_Sign1, true);
        //        Logger.WriteLog("皮带任务_皮带停止");
        //        StopConveyor();
        //        System.Threading.Thread.Sleep(200);

        //    STEP_LiftCylinderExtend1:
        //        Logger.WriteLog("皮带任务_顶起测距位料盘");
        //        SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 1);
        //        SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 0);
        //        SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 1);
        //        SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 0);

        //        Logger.WriteLog("皮带任务_等待测距位料盘被顶起");
        //        WaitIO(99999999, IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos, true);
        //        WaitIO(99999999, IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos, true);

        //        Logger.WriteLog("皮带任务_设置测距位料盘就位");
        //        GlobalManager.Current.flag_RangeFindingTrayArrived = 1;

        //        Logger.WriteLog("皮带任务_所有工位料盘数量自增1(不包含NG工位)");
        //        data_AllStationTrayNumber += 1;
        //        goto STEP_JudgeAllStationTrayNumberIsZero;


        //    }




        //}

        //public override void AutoRun(CancellationToken token) //rayner version
        //{
        //    while (true)
        //    {

        //        //todo: check machine stop to exit thread.
        //        //todo: process SMEMA signal
        //        CheckSMEMAInput(out var canSendTrayOut, out var isGoodTrayAvailable, out var isBypassTrayAvailable);
        //        SendSMEMAOutput(canReceiveTray, canSendGoodTray, canSendBypassTray);

        //        if (isBypassTrayAvailable)
        //        {
        //            //todo: bypass tray handling

        //        }

        //        switch (currentstation)
        //        {
        //            #region Laser station

        //            case ConveyorStation.Laser:
        //                switch (laserstation)
        //                {
        //                    case StationState.Empty:

        //                        switch (laserstep)
        //                        {
        //                            case 0: //if station is empty, can allow tray from previous machine

        //                                if (isGoodTrayAvailable)
        //                                {
        //                                    TraySendingNextStation[(int)ConveyorStation.Laser] = true;
        //                                    laserstep = 1;
        //                                }
        //                                break;
        //                            case 1: //confirm end stopper is up, else trigger gate
        //                                if (!actionstate_laser)
        //                                {
        //                                    if (!ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1) &&
        //                                        ReadIO(IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos))
        //                                    {

        //                                        laserthread = new Thread(() => laserstatus = GateUp(ConveyorStation.Laser));
        //                                        actionstate_laser = true;
        //                                    }
        //                                    else
        //                                    {
        //                                        laserstep = 9;
        //                                        break;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (!laserthread.IsAlive && laserstatus)
        //                                    {
        //                                        actionstate_laser = false;
        //                                        laserstep = 9;
        //                                    }
        //                                    else if (!laserthread.IsAlive && !laserstatus)
        //                                    {
        //                                        //need to throw error if cylinder fail
        //                                        actionstate_laser = false;
        //                                        return;
        //                                    }
        //                                }

        //                                break;
        //                            case 9: //goto next station state
        //                                laserstep = 0;
        //                                TraySendingNextStation[(int)ConveyorStation.Laser] = false;
        //                                laserstation = StationState.TrayIncoming;
        //                                break;
        //                        }

        //                        break;

        //                    case StationState.TrayIncoming: // to wait for tray reach sensor and lift tray for process
        //                        switch (laserstep)
        //                        {
        //                            case 0: //wait tray reach end stopper

        //                                //slowdown sensor sequence - not in use
        //                                //if (ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1))
        //                                //{
        //                                //}

        //                                //if detect tray
        //                                if (ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1) &&
        //                                    ReadIO(IO_INFunction_Table.IN1_4Stop_Sign1))
        //                                {
        //                                    laserstep = 1;
        //                                }

        //                                break;
        //                            case 1: //lift tray
        //                                if (!actionstate_laser)
        //                                {

        //                                    laserthread = new Thread(() =>
        //                                        laserstatus = LiftUpRelatedTray(ConveyorStation.Laser));
        //                                    actionstate_laser = true;
        //                                }
        //                                else
        //                                {
        //                                    if (!laserthread.IsAlive && laserstatus)
        //                                    {
        //                                        actionstate_laser = false;
        //                                        laserstep = 2;
        //                                    }
        //                                    else if (!laserthread.IsAlive && !laserstatus)
        //                                    {
        //                                        //need to throw error if cylinder fail
        //                                        actionstate_laser = false;
        //                                        return;
        //                                    }
        //                                }

        //                                break;

        //                            case 2: //confirm tray seat properly
        //                                if (ReadIO(IO_INFunction_Table.IN1_12bord_lift_in_position1))
        //                                {
        //                                    ConveyorTrays[(int)ConveyorStation.Laser] = new TrayData { hasTray = true };
        //                                    laserstep = 9;
        //                                }
        //                                else lasercounter++;

        //                                if (lasercounter > 100)
        //                                {
        //                                    lasercounter = 0;
        //                                    //error handle if sensor not detected.
        //                                    return;
        //                                }

        //                                break;

        //                            case 9: //goto next station state
        //                                laserstep = 0;
        //                                laserstation = StationState.InProcess;
        //                                break;
        //                        }

        //                        break;
        //                    case StationState.InProcess: //process station interlock vs conveyor + stop gate lower

        //                        switch (laserstep)
        //                        {
        //                            case 0: //set station ready to process
        //                                StationReadyStatus[(int)ConveyorStation.Laser] = true;
        //                                laserstep = 1;
        //                                break;
        //                            case 1: //lower stopper
        //                                if (!actionstate_laser)
        //                                {

        //                                    laserthread = new Thread(() =>
        //                                        laserstatus = GateDown(ConveyorStation.Laser));
        //                                    actionstate_laser = true;
        //                                }
        //                                else
        //                                {
        //                                    if (!laserthread.IsAlive && laserstatus)
        //                                    {
        //                                        actionstate_laser = false;
        //                                        laserstep = 2;
        //                                    }
        //                                    else if (!laserthread.IsAlive && !laserstatus)
        //                                    {
        //                                        //need to throw error if cylinder fail
        //                                        actionstate_laser = false;
        //                                        return;
        //                                    }
        //                                }

        //                                break;
        //                            case 2: //wait station complete signal from main process - decide pass or fail
        //                                if (!StationReadyStatus[(int)ConveyorStation.Laser])
        //                                {
        //                                    ConveyorTrays[(int)ConveyorStation.Laser].isFail = !StationTrayStatus[(int)ConveyorStation.Laser];
        //                                    laserstep = 3;
        //                                }

        //                                break;
        //                            case 3: //wait next station empty, or reject empty
        //                                if (ConveyorTrays[(int)ConveyorStation.Laser].isFail)
        //                                {
        //                                    if (rejectstation == StationState.InProcess)
        //                                        return; //error if tray still on reject station
        //                                    bypasstraymoving = true; //move to reject station, need to bypass
        //                                    TraySendingNextStation[(int)ConveyorStation.Reject] = true;
        //                                    laserstep = 9;
        //                                }
        //                                else
        //                                {
        //                                    if (foamstation == StationState.Empty)
        //                                    {
        //                                        TraySendingNextStation[(int)ConveyorStation.Foam] = true;
        //                                        laserstep = 9;
        //                                    }
        //                                }

        //                                break;
        //                            case 9: //goto next station state
        //                                laserstep = 0;
        //                                laserstation = StationState.TrayOutgoing;
        //                                break;
        //                        }

        //                        break;
        //                    case StationState.TrayOutgoing: //lifter down and clear tray
        //                        switch (laserstep)
        //                        {
        //                            case 0: //Lifter down
        //                                if (!actionstate_laser)
        //                                {

        //                                    laserthread = new Thread(() =>
        //                                        laserstatus = LiftDownRelatedTray(ConveyorStation.Laser));
        //                                    actionstate_laser = true;
        //                                }
        //                                else
        //                                {
        //                                    if (!laserthread.IsAlive && laserstatus)
        //                                    {
        //                                        actionstate_laser = false;
        //                                        laserstep = 1;
        //                                    }
        //                                    else if (!laserthread.IsAlive && !laserstatus)
        //                                    {
        //                                        //need to throw error if cylinder fail
        //                                        actionstate_laser = false;
        //                                        return;
        //                                    }
        //                                }

        //                                break;
        //                            case 1: //wait tray leave zone sensor & side slow sensor
        //                                if (!ReadIO(IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1) &&
        //                                    !ReadIO(IO_INFunction_Table.IN1_0Slowdown_Sign1))

        //                                {
        //                                    laserstep = 9;
        //                                }

        //                                break;
        //                            case 9: //goto next station state
        //                                laserstep = 0;
        //                                laserstation = StationState.Empty;
        //                                //TraySendingNextStation[(int)ConveyorStation.Laser] = false;
        //                                break;
        //                        }

        //                        break;

        //                    default: break;
        //                }

        //                break;

        //            #endregion

        //            #region Foam Station

        //            case ConveyorStation.Foam:

        //                switch (foamstation)
        //                {
        //                    case StationState.Empty:
        //                        switch (foamstep)
        //                        {
        //                            case 0: //if station empty, wait for station trigger

        //                                if (TraySendingNextStation[(int)ConveyorStation.Foam])
        //                                {
        //                                    foamstep = 1;
        //                                }
        //                                break;
        //                            case 1: //confirm end stopper is up, else trigger gate
        //                                if (!actionstate_foam)
        //                                {
        //                                    if (!ReadIO(IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2) &&
        //                                        ReadIO(IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos))
        //                                    {

        //                                        foamthread = new Thread(() => foamstatus = GateUp(ConveyorStation.Foam));
        //                                        actionstate_foam = true;
        //                                    }
        //                                    else
        //                                    {
        //                                        laserstep = 9;
        //                                        break;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (!foamthread.IsAlive && actionstate_foam)
        //                                    {
        //                                        actionstate_foam = false;
        //                                        laserstep = 9;
        //                                    }
        //                                    else if (!foamthread.IsAlive && !foamstatus)
        //                                    {
        //                                        //need to throw error if cylinder fail
        //                                        actionstate_foam = false;
        //                                        return;
        //                                    }
        //                                }

        //                                break;
        //                            case 9: //goto next station state
        //                                foamstep = 0;
        //                                TraySendingNextStation[(int)ConveyorStation.Foam] = false;
        //                                foamstation = StationState.TrayIncoming;
        //                                break;
        //                        }
        //                        break;
        //                    case StationState.TrayIncoming:
        //                        break;
        //                    case StationState.InProcess:
        //                        break;
        //                    case StationState.TrayOutgoing:
        //                        break;
        //                    default: break;
        //                }

        //                break;

        //            #endregion

        //            #region Recheck station

        //            case ConveyorStation.Recheck:

        //                switch (recheckstation)
        //                {
        //                    case StationState.Empty:
        //                        break;
        //                    case StationState.TrayIncoming:
        //                        break;
        //                    case StationState.InProcess:
        //                        break;
        //                    case StationState.TrayOutgoing:
        //                        break;
        //                    default: break;
        //                }

        //                break;

        //            #endregion

        //            #region Reject station

        //            case ConveyorStation.Reject:

        //                switch (rejectstation)
        //                {
        //                    case StationState.Empty:
        //                        break;
        //                    case StationState.TrayIncoming:
        //                        break;
        //                    case StationState.InProcess:
        //                        break;
        //                    case StationState.TrayOutgoing:
        //                        break;
        //                    default:
        //                        break;
        //                }

        //                break;
        //        }

        //        #endregion

        //        if (currentstation == ConveyorStation.Reject)
        //        {
        //            currentstation = ConveyorStation.Laser;
        //        }
        //        else currentstation++;

        //        Thread.Sleep(10);
        //    }
        //}

        #endregion
    }
}
