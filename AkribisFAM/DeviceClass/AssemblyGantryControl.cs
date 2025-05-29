using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System;
using System.Threading;
using static AkribisFAM.GlobalManager;

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
            return ZPickPosition(Picker.Picker1) && ZPickPosition(Picker.Picker2) && ZPickPosition(Picker.Picker3) && ZPickPosition(Picker.Picker4);
        }
        public bool ZPickPosition(Picker picker)
        {
            if (IsBypass(picker))
            {
                return false;
            }

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

            SinglePoint sp = ZuZhuang.Current.GetZPickPosition((int)picker);
            return AkrAction.Current.Move(axis, sp.Z, (int)speed) == 0;

        }
  
        public bool ZLoadCellPosition(Picker picker)
        {
            if (IsBypass(picker))
            {
                return false;
            }

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

            SinglePoint sp = ZuZhuang.Current.GetLoadCellPosition((int)picker);
            return AkrAction.Current.Move(axis, sp.Z, (int)speed) == 0;

        }
        public bool ZCamPos(Picker picker)
        {
            if (IsBypass(picker))
            {
                return false;
            }

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

            SinglePoint sp = ZuZhuang.Current.GetZCam2Position((int)picker);
            return AkrAction.Current.Move(axis, sp.Z, (int)speed) == 0;

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

            SinglePoint sp = ZuZhuang.Current.GetZSafePosition((int)picker);
            return AkrAction.Current.Move(axis, sp.Z, (int)speed) == 0;

        }
        public bool ZUpAll()
        {
            return ZUp(Picker.Picker1) && ZUp(Picker.Picker2) && ZUp(Picker.Picker3) && ZUp(Picker.Picker4);
        }
        public bool MovePickPos(Picker pickerNum, int fovNum)
        {
            if (!ZUpAll())
            {
                return false;
            }

            SinglePoint res1 = ZuZhuang.Current.GetPickPosition((int)pickerNum, (int)fovNum);

            if (res1.X == 0 && res1.Y == 0 && res1.Z == 0)
            {
                return false;
            }
            if (AkrAction.Current.Move(AxisName.FSX, res1.X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0 ||
                AkrAction.Current.Move(AxisName.FSY, res1.Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) != 0)
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

            SinglePoint res1 = ZuZhuang.Current.GetLoadCellPosition((int)pickerNum);

            if (res1.X == 0 && res1.Y == 0 && res1.Z == 0)
            {
                return false;
            }
            if (AkrAction.Current.Move(AxisName.FSX, res1.X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0)
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

            SinglePoint res1 = ZuZhuang.Current.GetLoadCellPosition((int)pickerNum);

            if (res1.X == 0 && res1.Y == 0 && res1.Z == 0)
            {
                return false;
            }
            if (AkrAction.Current.Move(AxisName.FSX, res1.X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0 ||
                AkrAction.Current.Move(AxisName.FSY, res1.Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) != 0)
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

            if (!CallCalib())
            {
                ZUp(pickerNum);
                return false;
            }
           
            return ZUp(pickerNum);
        }

        public bool CallCalib()
        {
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[101]=1000", out string response44);
            Logger.WriteLog("开始发送力控信号");
            Thread.Sleep(100);
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[102]=5000", out string response123);
            Logger.WriteLog("力控信号111");
            Thread.Sleep(50);
            AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[800]=2", out string response4);
            Logger.WriteLog("力控信号222");

            DateTime start = DateTime.Now;
            while ((DateTime.Now- start).TotalMilliseconds <3000)
            { 
                AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[800]", out string response5);
                if (response5.Equals("0"))
                {
                    return true;
                }
                Thread.Sleep(10);
            }


            //change back to ptp mode
            return false;
        }
        public bool MovePlacePos(Picker pickerNum, int fovNum)
        {
            if (!ZUpAll())
            {
                return false;
            }

            SinglePoint res1 = ZuZhuang.Current.GetPlacePosition((int)pickerNum, fovNum);
            if (AkrAction.Current.Move(AxisName.FSX, res1.X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0 ||
            AkrAction.Current.Move(AxisName.FSY, res1.Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) != 0)
            {

                return false;
            }
            return true;
        }
        public SinglePoint GetPlacePosition(int Nozzlenum, int Fovnum)
        {
            SinglePoint singlePoint = new SinglePoint();
            string command = "GT,1," + $"{Nozzlenum}" + ",Foam," + $"{Fovnum}," + "Foam->Moudel";
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
        public bool PlaceFoam(Picker pickerNum, int fovNum)
        {
            if (IsBypass(pickerNum))
            {
                return true;
            }
            //var temp = GetPlacePosition((int)pickerNum, fovNum);
            if (!ZUpAll())
            {
                return false;
            }
            if (!MovePlacePos(pickerNum, fovNum))
            {
                return false;
            }

            if (!ZSafe(pickerNum)) // change to force mode
            {
                ZUp(pickerNum);
                return false;
            }

            if (!VacOff(pickerNum))
            {
                return false;
            }

            if (!ZUp(pickerNum))
            {
                return false;
            }
            Thread.Sleep(200);

            return ZUp(pickerNum);
        }
        public bool PickFoam(Picker pickerNum, int fovNum)
        {
            if (IsBypass(pickerNum))
            {
                return true;
            }

            if (!ZUpAll())
            {
                return false;
            }
            if (!MovePickPos(pickerNum, fovNum))
            {
                return false;
            }

            if (!TRotate(pickerNum, 0))
            {
                return false;
            }

            if (!ZUp(pickerNum))
            {
                return false;
            }
            if (!ZPickPosition(pickerNum))
            {
                ZUp(pickerNum);
                return false;
            }
            if (!VacOn(pickerNum))
            {
                return false;
            }
            return ZUp(pickerNum);

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

            if (!TZeroAll())
            {
                return false;
            }


            if (!ZPickPositionAll())
            {
                return false;
            }

            if (!VacOnAll())
            {
                return false;
            }
            if (!TCompensate())
            {
                return false;
            }
            //rotate after picked
            var res = ZPickPosition(Picker.Picker1) && ZPickPosition(Picker.Picker2) && ZPickPosition(Picker.Picker3);
            //var res = res && ZDown(Picker.Picker1) || ZDown(Picker.Picker2) || ZDown(Picker.Picker3) || ZDown(Picker.Picker4);
            res = res && ZSafe(Picker.Picker1) && ZSafe(Picker.Picker2) && ZSafe(Picker.Picker3);

            res = TZero(Picker.Picker1) && TZero(Picker.Picker2);
            //var res = TZero(Picker.Picker1) || TZero(Picker.Picker2) || TZero(Picker.Picker3) || TZero(Picker.Picker4);
            //var res = ZDown(Picker.Picker1) || ZDown(Picker.Picker2) || ZDown(Picker.Picker3) || ZDown(Picker.Picker4);
            //var res = res && ZDown(Picker.Picker1) || ZDown(Picker.Picker2) || ZDown(Picker.Picker3) || ZDown(Picker.Picker4);
            // res = res && ZSafe(Picker.Picker1) || ZSafe(Picker.Picker2) || ZSafe(Picker.Picker3) || ZSafe(Picker.Picker4);
            return res;
        }

        public bool TRotate(Picker picker, int angle)
        {
            if (IsBypass(picker))
            {
                return true;
            }

            AxisName axis;
            AxisSpeed speed;
            switch (picker)
            {
                case Picker.Picker1:
                    axis = AxisName.PICK1_T;
                    speed = AxisSpeed.PICK1_T;
                    break;
                case Picker.Picker2:

                    axis = AxisName.PICK2_T;
                    speed = AxisSpeed.PICK2_T;
                    break;
                case Picker.Picker3:

                    axis = AxisName.PICK3_T;
                    speed = AxisSpeed.PICK3_T;
                    break;
                case Picker.Picker4:

                    axis = AxisName.PICK4_T;
                    speed = AxisSpeed.PICK4_T;
                    break;
                default:
                    return false;
            }


            return AkrAction.Current.Move(axis, angle, (int)speed) == 0;
        }
        public bool TZeroAll()
        {
            return TZero(Picker.Picker1) && TZero(Picker.Picker2) && TZero(Picker.Picker3) && TZero(Picker.Picker4);
        }
        public bool TCompensate()
        {
            return TRotate(Picker.Picker1, 0) && TRotate(Picker.Picker2, 0) && TRotate(Picker.Picker3, 0) && TRotate(Picker.Picker4, 0);
        }
        public bool VacOnAll()
        {
            return VacOn(Picker.Picker1) && VacOn(Picker.Picker2) && VacOn(Picker.Picker3) && VacOn(Picker.Picker4);
        }
        public bool TZero(Picker picker)
        {
            if (IsBypass(picker))
            {
                return true;
            }

            AxisName axis;
            AxisSpeed speed;
            switch (picker)
            {
                case Picker.Picker1:
                    axis = AxisName.PICK1_T;
                    speed = AxisSpeed.PICK1_T;
                    break;
                case Picker.Picker2:

                    axis = AxisName.PICK2_T;
                    speed = AxisSpeed.PICK2_T;
                    break;
                case Picker.Picker3:

                    axis = AxisName.PICK3_T;
                    speed = AxisSpeed.PICK3_T;
                    break;
                case Picker.Picker4:

                    axis = AxisName.PICK4_T;
                    speed = AxisSpeed.PICK4_T;
                    break;
                default:
                    return false;
            }


            return AkrAction.Current.Move(axis, 0, (int)speed) == 0;
        }
        public bool ZUp(Picker picker)
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

            return AkrAction.Current.Move(axis, 0, (int)speed) == 0;

        }
    }
}
