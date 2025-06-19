using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Helper;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.Manager.LoadCellCalibration;

namespace AkribisFAM.DeviceClass
{
    public class AssemblyGantryControl
    {
        private bool bypassPicker1;

        public bool BypassPicker1
        {
            get { return bypassPicker1; }
            set { bypassPicker1 = value; }
        }

        private bool bypassPicker2;

        public bool BypassPicker2
        {
            get { return bypassPicker2; }
            set { bypassPicker2 = value; }
        }

        private bool bypassPicker3;

        public bool BypassPicker3
        {
            get { return bypassPicker3; }
            set { bypassPicker3 = value; }
        }

        private bool bypassPicker4;

        public bool BypassPicker4
        {
            get { return bypassPicker4; }
            set { bypassPicker4 = value; }
        }
        public enum Picker
        {
            Picker1 = 1,
            Picker2 = 2,
            Picker3 = 3,
            Picker4 = 4,
        }
        private double xOffset;

        public double XOffset
        {
            get { return xOffset; }
            set { xOffset = value; }
        }

        public bool IsPicker1VacOk => IOManager.Instance.ReadIO(IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback);
        public bool IsPicker2VacOk => IOManager.Instance.ReadIO(IO_INFunction_Table.IN3_13PNP_Gantry_vacuum2_Pressure_feedback);
        public bool IsPicker3VacOk => IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_2Feeder_vacuum3_Pressure_feedback);
        public bool IsPicker4VacOk => IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_3Feeder_vacuum4_Pressure_feedback);
        public AssemblyGantryControl() { }


        public bool VacOn(Picker picker)
        {
            if (IsBypass(picker))
            {
                return true;
            }

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
            return (IOManager.Instance.IO_ControlStatus(vacSupply, 1) &&
            IOManager.Instance.IO_ControlStatus(vacRelease, 0));
        }
        public bool IsVacOn(Picker picker)
        {
            if (IsBypass(picker))
            {
                return true;
            }
            IO_OutFunction_Table checkBit1;
            IO_OutFunction_Table checkBit2;
            switch (picker)
            {
                case Picker.Picker1:
                    checkBit1 = IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply;
                    checkBit2 = IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release;
                    break;
                case Picker.Picker2:
                    checkBit1 = IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply;
                    checkBit2 = IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release;
                    break;
                case Picker.Picker3:

                    checkBit1 = IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply;
                    checkBit2 = IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release;
                    break;
                case Picker.Picker4:
                    checkBit1 = IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply;
                    checkBit2 = IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release;
                    break;
                default:
                    return false;
            }
            return (IOManager.Instance.GetOutputStatus(checkBit1) &&
                !IOManager.Instance.GetOutputStatus(checkBit2));
        }
        public bool Purge(Picker picker)
        {
            if (IsBypass(picker))
            {
                return true;
            }
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

            return (IOManager.Instance.IO_ControlStatus(vacSupply, 0) &&
            IOManager.Instance.IO_ControlStatus(vacRelease, 1));
        }

        public bool VacOff(Picker picker)
        {
            if (IsBypass(picker))
            {
                return true;
            }
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
            return (IOManager.Instance.IO_ControlStatus(vacSupply, 0) &&
             IOManager.Instance.IO_ControlStatus(vacRelease, 0));
        }

        public bool VacOffAll()
        {
            return VacOff(Picker.Picker1) && VacOff(Picker.Picker2) && VacOff(Picker.Picker3) && VacOff(Picker.Picker4);
        }
        public bool ZPickPositionAll()
        {
            return ZPickDownPosition(Picker.Picker1) && ZPickDownPosition(Picker.Picker2) && ZPickDownPosition(Picker.Picker3) && ZPickDownPosition(Picker.Picker4);
        }
        public bool ZPickDownPosition(Picker picker)
        {
            if (IsBypass(picker))
            {
                return false;
            }

            SinglePoint sp = ZuZhuang.Current.GetZPickPosition((int)picker);
            sp.Z = GlobalManager.Current.CurrentMode == ProcessMode.Dryrun ? sp.Z / 2 : sp.Z;
            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.MoveFoamZ1(sp.Z) == 0;
                case Picker.Picker2:
                    return AkrAction.Current.MoveFoamZ2(sp.Z) == 0;
                case Picker.Picker3:
                    return AkrAction.Current.MoveFoamZ3(sp.Z) == 0;
                case Picker.Picker4:
                    return AkrAction.Current.MoveFoamZ4(sp.Z) == 0;
                default:
                    return false;
            }
        }

        public bool ZLoadCellPosition(Picker picker)
        {
            if (IsBypass(picker))
            {
                return false;
            }

            SinglePoint sp = ZuZhuang.Current.GetLoadCellPosition((int)picker);
            sp.Z = 21.5;
            AxisName axis;
            AxisSpeed speed;
            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.MoveFoamZ1(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker2:
                    return AkrAction.Current.MoveFoamZ2(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker3:
                    return AkrAction.Current.MoveFoamZ3(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker4:
                    return AkrAction.Current.MoveFoamZ4(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                default:
                    return false;
            }
        }

        public static (double m, double c) CalculateLinearCoefficients(List<double> x, List<double> y) //x=newton, y=current
        {
            if (x.Count != y.Count || x.Count == 0)
                throw new ArgumentException("x and y must be the same length and not empty.");

            double meanX = x.Average();
            double meanY = y.Average();

            double sumXY = 0;
            double sumXX = 0;

            for (int i = 0; i < x.Count; i++)
            {
                sumXY += (x[i] - meanX) * (y[i] - meanY);
                sumXX += (x[i] - meanX) * (x[i] - meanX);
            }

            double m = sumXY / sumXX;
            double c = meanY - m * meanX;

            return (m, c);
        }

        public double PredictCurrent(double newton, double m, double c)
        {
            return m * newton + c;
        }


        public bool ZCamPosAll(bool waitMotionDone = true)
        {
            SinglePoint sp = ZuZhuang.Current.GetZCam2Position((int)Picker.Picker1);
            return AkrAction.Current.MoveFoamZ1Z2Z3Z4(sp.Z, sp.Z, sp.Z, sp.Z, waitMotionDone) == (int)AkrAction.ACTTION_ERR.NONE;

        }
        public bool ZCamPos(Picker picker, bool waitMotionDone = true)
        {
            if (IsBypass(picker))
            {
                return true;
            }
            SinglePoint sp = ZuZhuang.Current.GetZCam2Position((int)picker);
            //AkrAction.Current.MoveFoamZ1Z2Z3Z4(sp.Z, sp.Z, sp.Z, sp.Z);
            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.MoveFoamZ1(sp.Z, waitMotionDone) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker2:
                    return AkrAction.Current.MoveFoamZ2(sp.Z, waitMotionDone) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker3:
                    return AkrAction.Current.MoveFoamZ3(sp.Z, waitMotionDone) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker4:
                    return AkrAction.Current.MoveFoamZ4(sp.Z, waitMotionDone) == (int)AkrAction.ACTTION_ERR.NONE;
                default:
                    return false;
            }

        }
        public bool IsBypass(Picker picker)
        {
            switch (picker)
            {
                case Picker.Picker1:
                    return (bypassPicker1);
                case Picker.Picker2:
                    return (bypassPicker2);
                case Picker.Picker3:
                    return (bypassPicker3);
                case Picker.Picker4:
                    return (bypassPicker4);
                default:
                    return false;
            }
        }
        public bool ZSafe(Picker picker)
        {
            SinglePoint sp = ZuZhuang.Current.GetZSafePosition((int)picker);
            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.MoveFoamZ1(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker2:
                    return AkrAction.Current.MoveFoamZ2(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker3:
                    return AkrAction.Current.MoveFoamZ3(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker4:
                    return AkrAction.Current.MoveFoamZ4(sp.Z) == (int)AkrAction.ACTTION_ERR.NONE;
                default:
                    return false;
            }

        }
        public bool ZUpAll()
        {
            return AkrAction.Current.MoveFoamZ1Z2Z3Z4(0, 0, 0, 0) == (int)AkrAction.ACTTION_ERR.NONE;
        }
        /// <summary>
        /// Use only after On the fly
        /// </summary>
        /// <param name="pickerNum"></param>
        /// <param name="fovNum"></param>
        /// <param name="waitMotionDone"></param>
        /// <returns></returns>
        public bool MovePickPos(Picker pickerNum, int fovNum, bool waitMotionDone = true)
        {
            if (!ZUpAll())
            {
                return false;
            }

            if (!ZuZhuang.Current.GetPickPosition((int)pickerNum, fovNum, out SinglePoint point))
            {
                return false;
            }

            if (point.X == 0 && point.Y == 0 && point.Z == 0 && point.R == 0)
            {
                return false;
            }
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// move to bin reject position
        /// </summary>
        /// <param name="pickerNum"></param>
        /// <param name="fovNum"></param>
        /// <param name="waitMotionDone"></param>
        /// <returns></returns>
        public bool MoveRejectPos(bool waitMotionDone = true)
        {
            if (!ZUpAll())
            {
                return false;
            }

            if (!ZuZhuang.Current.GetPickPosition((int)1, 1, out SinglePoint point)) // and a getrejectposition
            {
                return false;
            }

            if (point.X == 0 && point.Y == 0 && point.Z == 0 && point.R == 0)
            {
                return false;
            }
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        private bool WaitMovePickPosDone(Picker pickerNum, int fovNum)
        {
            if (!ZuZhuang.Current.GetPickPosition((int)pickerNum, fovNum, out SinglePoint point))
            {
                return false;
            }

            if (point.X == 0 && point.Y == 0 && point.Z == 0 && point.R == 0)
            {
                return false;
            }
            if (AkrAction.Current.WaitFoamXYMotionDone(point.X, point.Y) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        private bool WaitMoveStandbyPickPosDone(Picker pickerNum, int fovNum, int feederNum)
        {
            if (!ZuZhuang.Current.GetStandbyPickPosition((int)pickerNum, fovNum, feederNum, out SinglePoint point))
            {
                return false;
            }

            if (point.X == 0 && point.Y == 0 && point.Z == 0 && point.R == 0)
            {
                return false;
            }
            if (AkrAction.Current.WaitFoamXYMotionDone(point.X, point.Y) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        private bool WaitPickerTDone(Picker picker, double angle)
        {
            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.WaitFoamT1MotionDone(angle) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker2:
                    return AkrAction.Current.WaitFoamT2MotionDone(angle) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker3:
                    return AkrAction.Current.WaitFoamT3MotionDone(angle) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker4:
                    return AkrAction.Current.WaitFoamT4MotionDone(angle) == (int)AkrAction.ACTTION_ERR.NONE;
                default:
                    return false;
            }
        }
        public bool MoveStandbyPickPos(Picker pickerNum, int fovNum, int feederNum, bool waitMotionDone = true)
        {
            if (!ZUpAll())
            {
                return false;
            }

            if (!ZuZhuang.Current.GetStandbyPickPosition((int)pickerNum, fovNum, feederNum, out SinglePoint point))
            {
                return false;
            }

            if (point.X == 0 && point.Y == 0 && point.Z == 0 && point.R == 0)
            {
                return false;
            }
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        public bool IsMoveStandbyPickPosDone(Picker pickerNum, int fovNum, int feederNum, bool waitMotionDone = true)
        {
            if (!ZuZhuang.Current.GetStandbyPickPosition((int)pickerNum, fovNum, feederNum, out SinglePoint point))
            {
                return false;
            }

            if (point.X == 0 && point.Y == 0 && point.Z == 0 && point.R == 0)
            {
                return false;
            }
            if (AkrAction.Current.IsMoveFoamXYDone(point.X, point.Y))
            {
                return false;
            }
            return true;
        }
        public bool MoveXYLoadCellPos(Picker pickerNum)
        {
            if (!ZUpAll())
            {
                return false;
            }

            SinglePoint point = ZuZhuang.Current.GetLoadCellPosition((int)pickerNum);

            if (point.X == 0 && point.Y == 0 && point.Z == 0)
            {
                return false;
            }
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y) != (int)AkrAction.ACTTION_ERR.NONE)
            {

                return false;
            }

            return true;
        }
        public bool MoveZLoadCellZPlacePosition(Picker pickerNum)
        {

            if (!ZUpAll())
            {
                return false;
            }

            SinglePoint point = ZuZhuang.Current.GetLoadCellPosition((int)pickerNum);

            if (point.X == 0 && point.Y == 0 && point.Z == 0)
            {
                return false;
            }
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y) != (int)AkrAction.ACTTION_ERR.NONE)
            {

                return false;
            }
            return true;
        }
        public bool TriggerCalib(Picker pickerNum)
        {
            if (IsBypass(pickerNum))
            {
                return true;
            }
            if (!VacOffAll())
            {
                return false;
            }
            if (!ZUpAll())
            {
                return false;
            }
            if (!MoveXYLoadCellPos(pickerNum))
            {
                return false;
            }


            if (!ZLoadCellPosition(pickerNum))
            {
                ZUp(pickerNum);
                return false;
            }

            //if (!ApplyForce((int)pickerNum, 3.5))
            //{

            //    ZUp(pickerNum);
            //    return false;
            //}

            if (!CallCalib(pickerNum))
            {
                ZUp(pickerNum);
                return false;
            }

            return ZUp(pickerNum);
        }
        public bool IsPtpMode(Picker picker)
        {

            string axis = "";
            switch (picker)
            {
                case Picker.Picker1:
                    axis = "A";
                    break;
                case Picker.Picker2:
                    axis = "C";
                    break;
                case Picker.Picker3:
                    axis = "E";
                    break;
                case Picker.Picker4:
                    axis = "G";
                    break;
                default:
                    break;
            }
            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}OperationMode", out string response) && response == "3"))//1
                return false;

            return true;

        }
        public bool IsCurrentMode(Picker picker)
        {

            string axis = "";
            switch (picker)
            {
                case Picker.Picker1:
                    axis = "A";
                    break;
                case Picker.Picker2:
                    axis = "C";
                    break;
                case Picker.Picker3:
                    axis = "E";
                    break;
                case Picker.Picker4:
                    axis = "G";
                    break;
                default:
                    break;
            }
            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}OperationMode", out string response) && response == "1"))
                return false;

            return true;

        }
        public bool SetPositionMode(Picker picker)
        {
            string axis = "";
            switch (picker)
            {
                case Picker.Picker1:
                    axis = "A";
                    break;
                case Picker.Picker2:
                    axis = "C";
                    break;
                case Picker.Picker3:
                    axis = "E";
                    break;
                case Picker.Picker4:
                    axis = "G";
                    break;
                default:
                    break;
            }

            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}GoToPosMode", out string response) && response == "OK"))
                return false;

            return true;

        }
        public bool SetPositionMode(string axiscode)
        {

            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axiscode}GoToPosMode", out string response) && response == "OK"))
                return false;

            return true;

        }
        public bool SetCurrMode(Picker picker)
        {
            string axis = "";
            switch (picker)
            {
                case Picker.Picker1:
                    axis = "A";
                    break;
                case Picker.Picker2:
                    axis = "C";
                    break;
                case Picker.Picker3:
                    axis = "E";
                    break;
                case Picker.Picker4:
                    axis = "G";
                    break;
                default:
                    break;
            }

            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}GoToCurrMode", out string response) && response == "OK"))
                return false;

            return true;

        }
        public bool CallCalib(Picker picker)
        {
            string programNumber = "-1";
            string axis = "";
            int startCurrent = 0;
            int stepSize = 20;
            int stepMultiply = 70;
            switch (picker)
            {
                case Picker.Picker1:
                    programNumber = "6";
                    axis = "A";
                    break;
                case Picker.Picker2:
                    programNumber = "7";
                    axis = "C";
                    break;
                case Picker.Picker3:
                    programNumber = "8";
                    axis = "E";
                    break;
                case Picker.Picker4:
                    programNumber = "9";
                    axis = "G";
                    break;
                default:
                    return false;
            }
            //AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}GenData[101]=1000", out string response44); //current
            //Logger.WriteLog("开始发送力控信号");
            //Thread.Sleep(100);
            //AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}GenData[102]=5000", out string response123); // time
            //Logger.WriteLog("力控信号111");
            //Thread.Sleep(50);
            if (!GetMotorCurrent(axis, out string current))
            {
                return false;
            }
            startCurrent = int.Parse(current);

            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}GenData[{((int)picker)}01]={startCurrent}", out string response9) && response9 == "OK"))
                return false;

            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}GenData[{(int)picker}12]={stepSize}", out string response5) && response5 == "OK"))
                return false;

            if (!(AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axis}GenData[800]={programNumber}", out string response4) && response4 == "OK"))
                return false;



            App.calib.NewtonCurrentList[(int)picker - 1].Clear();
            NewtonCurrent data = new NewtonCurrent();
            List<NewtonCurrent> list = new List<NewtonCurrent>();
            for (int i = 0; i < stepMultiply; i++)
            {
                if (!IsCurrentMode(picker) && !SetCurrMode(picker))
                {
                    return false;
                }
                IncreaseForce(axis, ((int)picker).ToString());
                if (!GetData(axis, ((int)picker).ToString(), out data))
                {
                    return false;
                }
                Thread.Sleep(200);
                list.Add(data);
            }

            var usefuldata = list.Where(x => x.Newton > 0.05).ToList();
            App.calib.NewtonCurrentList[(int)picker - 1] = usefuldata;

            var (m, c) = CalculateLinearCoefficients(usefuldata.Select(x => x.Newton).ToList(), usefuldata.Select(x => x.Current).ToList());
            App.calib.Models[(int)picker - 1] = new LoadCellModel()
            {
                m = m,
                C = c,
            };

            FileHelper.Save(App.calib);

            if (!EndCalibProcess(axis, ((int)picker).ToString()))
            {
                return false;
            }

            return true;
        }
        public bool ApplyForce(int axis, double newton)
        {
            int timePush = 1000;
            int curret_whole = 2000;
            var current = PredictCurrent(newton, App.calib.Models[axis - 1].m, App.calib.Models[axis - 1].C);
            current = 1500;
            if (current >= 2500)
            {
                return false;
            }
            curret_whole = (int)current;
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"AGenData[{axis}01]={curret_whole}", out string response44);
            Logger.WriteLog("开始发送力控信号");
            Thread.Sleep(100);
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"AGenData[{axis}02]={timePush}", out string response123);
            Logger.WriteLog("力控信号111");
            Thread.Sleep(50);
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"AGenData[800]={axis + 1}", out string response4);
            //double Newton;
            //double count =0;
            //while (count < 200)
            //{

            //     Newton = ReadLoadCell();
            //    count += 1;
            //}

            if (!WaitForceDone())
            {
                SetPositionMode($"{axis}");
                return false;
            }


            if (!WaitModeToPositionControl($"{axis}"))
            {
                SetPositionMode($"{axis}");
                return false;
            }

            return true;

        }
        private bool WaitForceDone()
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < 3000)
            {
                //wait done signal
                AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[800]", out string done);
                if (done.Equals("0"))
                {
                    return true;
                }
                Thread.Sleep(10);
            }
            return false;
        }



        private void IncreaseForce(string axisCode, string pickerNum)
        {
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axisCode}GenData[{pickerNum}10]=1", out string response5);

        }
        private bool GetData(string axisCode, string pickerNum, out NewtonCurrent data)
        {
            data = null;
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < 3000)
            {
                //wait done signal
                AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axisCode}GenData[{pickerNum}10]", out string done);
                if (done.Equals("0"))
                {
                    GetCurrent(axisCode, out string current);
                    double N = ReadLoadCell();
                    data = new NewtonCurrent()
                    {
                        Newton = N,
                        Current = double.Parse(current),
                    };
                    return true;
                }
                Thread.Sleep(10);
            }

            return false;

        }
        public enum OperationMode
        {
            CurrentControl = 1,
            VelocityControl = 2,
            PositionControl = 3
        }
        private bool WaitProcessDone(string axisCode)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < 3000)
            {
                //wait done signal
                AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axisCode}GenData[800]", out string done); // operation
                if (done.Equals("0"))
                {
                    return true;
                }
                Thread.Sleep(10);
            }
            return false;
        }
        private bool WaitModeToPositionControl(string axisCode)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < 3000)
            {
                //wait done signal
                AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axisCode}OperationMode", out string done); // operation
                if (done.Equals(((int)OperationMode.PositionControl).ToString()))
                {
                    return true;
                }
                Thread.Sleep(10);
            }
            return false;
        }

        public bool EndCalibProcess(string axisCode, string pickerNUm)
        {
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axisCode}GenData[{pickerNUm}11]=1", out string response5); // close agito


            if (!WaitModeToPositionControl(axisCode))
            {
                SetPositionMode(axisCode);
                return false;
            }
            if (!WaitProcessDone(axisCode))
            {
                SetPositionMode(axisCode);
                return false;
            }
            return true;


        }
        public bool GetCurrent(string axisCode, out string current)
        {
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axisCode}CurrCmdVal[1]", out current);
            return true;
        }
        public bool GetMotorCurrent(string axisCode, out string current)
        {
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString($"{axisCode}MotorCurr", out current);
            return true;
        }
        public double ReadLoadCell()
        {
            string press = TCPNetworkManage.GetLastMessage(ClientNames.Pressure_sensor);
            return Parser.TryParseTwoValues(press);
        }




        public bool MovePlacePos(Picker pickerNum, int fovNum)
        {
            if (!ZUpAll())
            {
                return false;
            }

            if (!ZuZhuang.Current.GetPlacePosition((int)pickerNum, fovNum, out SinglePoint point))
            {
                return false;
            }
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        public bool MoveStandbyPlacePos(Picker pickerNum, int fovNum)
        {
            if (!ZUpAll())
            {
                return false;
            }

            if (!ZuZhuang.Current.GetStandbyPlacePosition((int)pickerNum, fovNum, out SinglePoint point))
            {
                return false;
            }
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        public SinglePoint GetPlacePosition(int Nozzlenum, int Fovnum)
        {
            SinglePoint singlePoint = new SinglePoint();
            string command = "GT,1," + $"{Nozzlenum}" + ",Foam," + $"{Fovnum}," + "Foam->Module";
            Task_FeedupCameraFunction.PushcommandFunction(command);
            var GMout = Task_FeedupCameraFunction.TriggFeedUpCamreaGTAcceptData()[0];
            if (GMout.Subareas_Errcode == "1")
            {
                singlePoint.X = double.Parse(GMout.Unload_X);
                singlePoint.Y = double.Parse(GMout.Unload_Y);
                singlePoint.R = double.Parse(GMout.Unload_R);
                singlePoint.Z = 0.0;
            }
            return singlePoint;
        }
        private int _step = 0;
        public bool CanPlaceRetry => _step < 5;
        public bool PlaceFoam(Picker pickerNum, int fovNum)
        {
            _step = 0;
            if (_step == 0)
            {
                if (IsBypass(pickerNum)) return true;
                _step = 1;
            }
            if (_step == 1)
            {
                if (!IsVacOn(pickerNum) || !IsVacOk(pickerNum)) return false;
                _step = 2;
            }

            if (_step == 2)
            {
                if (!ZUpAll()) return false;
                _step = 3;
            }

            if (_step == 3)
            {
                if (!MovePlacePos(pickerNum, fovNum)) return false;
                _step = 4;
            }
            if (_step == 4)
            {
                if (!TCompensatePlace(pickerNum)) return false;
                _step = 5;
            }

            if (_step == 5)
            {
                //if (!ApplyForce((int)pickerNum,1)) // change to force mode
                //{
                //    return false;
                //}
                if (!ZPickDownPosition(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 6;
                }
            }

            if (_step == 6)
            {
                if (!VacOff(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 7;
                }
            }

            if (_step == 7)
            {
                if (!ZUp(pickerNum))
                {
                    return false;
                }
            }

            if (_step == -1)
            {
                ZUp(pickerNum);
                return false;
            }

            _step = 0;
            return true;
        }

        public bool PlaceFoamDryrun(Picker pickerNum, int fovNum)
        {
            _step = 0;
            if (_step == 0)
            {
                if (IsBypass(pickerNum)) return true;
                _step = 1;
            }
            if (_step == 1)
            {
                if (!IsVacOn(pickerNum) || !IsVacOk(pickerNum)) return false;
                _step = 2;
            }
            if (_step == 2)
            {
                if (!ZUpAll()) return false;
                _step = 3;
            }

            if (_step == 3)
            {
                if (!MoveStandbyPlacePos(pickerNum, fovNum)) return false;
                _step = 5;
            }

            if (_step == 5)
            {
                //if (!ApplyForce((int)pickerNum,1)) // change to force mode
                //{
                //    return false;
                //}
                if (!ZPickDownPosition(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 6;
                }
            }

            if (_step == 6)
            {
                if (!VacOff(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 7;
                }
            }

            if (_step == 7)
            {
                if (!ZUp(pickerNum)) return false;
            }

            if (_step == -1)
            {
                ZUp(pickerNum);
                return false;
            }

            _step = 0;
            return true;
        }
        public bool IsVacOk(Picker pickerNum)
        {
            switch (pickerNum)
            {
                case Picker.Picker1:
                    return IsPicker1VacOk;
                case Picker.Picker2:
                    return IsPicker2VacOk;
                case Picker.Picker3:
                    return IsPicker3VacOk;
                case Picker.Picker4:
                    return IsPicker4VacOk;
                default:
                    return false;
            }
        }

        public bool CanPickRetry => _step < 7;
        public bool PickFoam(Picker pickerNum, int fovNum, bool checkPressure = true)
        {
            _step = 0;
            if (_step == 0)
            {
                if (IsBypass(pickerNum)) return true;
                _step = 1;
            }
            if (_step == 1)
            {
                if (IsVacOn(pickerNum) && IsVacOk(pickerNum)) return false;
                _step = 2;
            }
            if (_step == 2)
            {
                if (!ZUpAll()) return false;
                _step = 3;
            }
            if (_step == 3)
            {
                if (!TZero(pickerNum)) return false;
                _step = 4;
            }
            if (_step == 4)
            {
                if (!MovePickPos(pickerNum, fovNum, false)) return false;
                _step = 5;
            }
            if (_step == 5)
            {
                if (!TCompensatePick(pickerNum)) return false;
                _step = 6;
            }
            if (_step == 6)
            {
                if (!WaitMovePickPosDone(pickerNum, fovNum)) return false;
                _step = 7;
            }
            if (_step == 7)
            {
                if (!ZPickDownPosition(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 8;
                }
            }
            if (_step == 8)
            {
                if (!VacOn(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 9;
                }
            }
            if (_step == 9)
            {
                if (!ZUp(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 10;
                }
            }
            if (_step == 10)
            {
                if (!TRotate(pickerNum, 90))
                {
                    _step = -1;
                }
                else
                {
                    _step = 11;
                }
            }
            if (_step == 11)
            {
                if (checkPressure)
                {
                    if (!IsVacOk(pickerNum))
                    {
                        return false;
                    }
                }
            }

            if (_step == -1)
            {
                ZUp(pickerNum);
                return false;
            }
            return true;

        }
        public bool PickFoamDryRun(Picker pickerNum, int fovNum, int feederNum, bool checkPressure = true)
        {
            _step = 0;
            if (_step == 0)
            {
                if (IsBypass(pickerNum)) return true;
                _step = 1;
            }
            if (_step == 1)
            {
                if (IsVacOn(pickerNum) && IsVacOk(pickerNum)) return false;
                _step = 2;
            }
            if (_step == 2)
            {
                if (!ZUpAll()) return false;
                _step = 3;
            }
            if (_step == 3)
            {
                if (!TZero(pickerNum)) return false;
                _step = 4;
            }
            if (_step == 4)
            {
                if (!MoveStandbyPickPos(pickerNum, fovNum, feederNum, false)) return false;
                _step = 5;
            }

            if (_step == 5)
            {
                if (!WaitMoveStandbyPickPosDone(pickerNum, fovNum, feederNum)) return false;
                _step = 7;
            }
            if (_step == 7)
            {
                if (!ZPickDownPosition(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 8;
                }
            }

            if (_step == 8)
            {
                if (!VacOn(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 9;
                }
            }

            if (_step == 9)
            {
                if (!ZUp(pickerNum))
                {
                    _step = -1;
                }
                else
                {
                    _step = 10;
                }
            }
            if (_step == 10)
            {
                if (!TRotate(pickerNum, 90))
                {
                    _step = -1;
                }
                else
                {
                    _step = 11;
                }
            }
            if (_step == 11)
            {
                if (checkPressure)
                {
                    if (!IsVacOk(pickerNum))
                    {
                        return false;
                    }
                }
            }
            if (_step == -1)
            {
                ZUp(pickerNum);
                return false;
            }
            _step = 0;
            return true;
        }

        public bool RejectFoam(Picker pickerNum)
        {
            if (IsBypass(pickerNum))
            {
                return true;
            }

            if (!ZUpAll())
            {
                return false;
            }
            if (!MoveRejectPos())
            {
                return false;
            }

            if (!Purge(pickerNum))
            {
                return false;
            }
            return true;
        }
        public bool RejectAllFoam()
        {
            if (!ZUpAll())
            {
                return false;
            }
            if (!MoveRejectPos())
            {
                return false;
            }

            if (!Purge(Picker.Picker1) & Purge(Picker.Picker2) & Purge(Picker.Picker3) & Purge(Picker.Picker4))
            {
                return false;
            }
            return true;
        }
        public bool RejectFailedFoam()
        {
            if (!ZUpAll())
            {
                return false;
            }
            if (!MoveRejectPos())
            {
                return false;
            }
            var productDatas = App.productTracker.GantryPickerFoams.PartArray;
            bool result = true;
            for (int i = 0; i < productDatas.Count(); i++)
            {
                if (productDatas[i].present && productDatas[i].failed)
                {
                    result &= Purge((Picker)i + 1);
                }
            }
            return result;
        }

      
        public bool PickAllFoam()
        {
            if (!ZUpAll())
            {
                return false;
            }
            if (!MovePickPos(Picker.Picker1, 1))
            {
                return false;
            }

            if (!TCompensatePickAll())
            {
                return false;
            }


            if (!ZPickPositionAll())
            {
                ZUpAll();
                return false;
            }

            if (!VacOnAll())
            {
                return false;
            }
            if (!ZUpAll())
            {
                return false;
            }
            if (!TRotateAll(90))
            {
                return false;
            }

            return true;
        }

        public bool TRotateAll(double angle)
        {
            return TRotate(Picker.Picker1, angle) & TRotate(Picker.Picker2, angle) & TRotate(Picker.Picker3, angle) & TRotate(Picker.Picker4, angle);
        }
        public bool TRotate(Picker picker, double angle, bool waitMotionDone = true)
        {
            if (IsBypass(picker))
            {
                return true;
            }

            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.MoveFoamT1(angle, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker2:
                    return AkrAction.Current.MoveFoamT2(angle, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker3:
                    return AkrAction.Current.MoveFoamT3(angle, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker4:
                    return AkrAction.Current.MoveFoamT4(angle, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                default:
                    return false;
            }
        }
        public bool TZeroAll(bool waitMotionDone = true)
        {
            return TZero(Picker.Picker1, waitMotionDone)
                & TZero(Picker.Picker2, waitMotionDone)
                & TZero(Picker.Picker3, waitMotionDone)
                & TZero(Picker.Picker4, waitMotionDone);
        }
        public bool TCompensatePickAll()
        {
            return (GlobalManager.Current.CurrentMode == ProcessMode.Dryrun ||
                (TCompensatePick(Picker.Picker1) && TCompensatePick(Picker.Picker2) &&
                TCompensatePick(Picker.Picker3) && TCompensatePick(Picker.Picker4)));
        }
        public bool TCompensatePlaceAll()
        {
            return TCompensatePlace(Picker.Picker1)
                & TCompensatePlace(Picker.Picker2)
                & TCompensatePlace(Picker.Picker3)
                & TCompensatePlace(Picker.Picker4);
        }
        public bool TCompensatePick(Picker picker, bool watiMotionDone = true)
        {
            if (ZuZhuang.Current.PickPositions[((int)picker - 1)] == null)
                return true;

            if (IsBypass(picker))
            {
                return true;
            }

            return TRotate(picker, ZuZhuang.Current.PickPositions[(int)picker].point.R, watiMotionDone);
        }

        public bool TCompensatePlace(Picker picker)
        {
            if (ZuZhuang.Current.PlacePositions[((int)picker - 1)] == null)
                return true;

            if (IsBypass(picker))
            {
                return true;
            }


            return TRotate(picker, ZuZhuang.Current.PlacePositions[(int)picker].point.R);
        }
        public bool VacOnAll()
        {
            return VacOn(Picker.Picker1) && VacOn(Picker.Picker2) && VacOn(Picker.Picker3) && VacOn(Picker.Picker4);
        }
        public bool TZero(Picker picker, bool waitMotionDone = true)
        {
            if (IsBypass(picker))
            {
                return true;
            }

            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.MoveFoamT1(0, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker2:
                    return AkrAction.Current.MoveFoamT2(0, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker3:
                    return AkrAction.Current.MoveFoamT3(0, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker4:
                    return AkrAction.Current.MoveFoamT4(0, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE;
                default:
                    return false;
            }
        }
        public bool ZUp(Picker picker)
        {
            if (IsBypass(picker))
            {
                return true;
            }
            switch (picker)
            {
                case Picker.Picker1:
                    return AkrAction.Current.MoveFoamZ1(0) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker2:
                    return AkrAction.Current.MoveFoamZ2(0) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker3:
                    return AkrAction.Current.MoveFoamZ3(0) == (int)AkrAction.ACTTION_ERR.NONE;
                case Picker.Picker4:
                    return AkrAction.Current.MoveFoamZ4(0) == (int)AkrAction.ACTTION_ERR.NONE;
                default:
                    return false;
            }

        }
    }
}
