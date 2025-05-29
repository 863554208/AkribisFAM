using AkribisFAM.CommunicationProtocol;
using AkribisFAM.WorkStation;
using LiveCharts.Wpf;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.DeviceClass
{
    public class FilmRemoveGantryControl
    {
        public enum Picker
        {
            Picker1 = 1,
            Picker2 = 2,
            Picker3 = 3,
            Picker4 = 4,
        }


        public FilmRemoveGantryControl() { }


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
            return (IOManager.Instance.IO_ControlStatus(vacSupply, 1) &&
            IOManager.Instance.IO_ControlStatus(vacRelease, 0));
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

            return (IOManager.Instance.IO_ControlStatus(vacSupply, 0) &&
            IOManager.Instance.IO_ControlStatus(vacRelease, 1));
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
            return (IOManager.Instance.IO_ControlStatus(vacSupply, 0) &&
             IOManager.Instance.IO_ControlStatus(vacRelease, 0));
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

            return AkrAction.Current.Move(axis, 15.5, (int)speed) == 0;

        }

        public bool ZSafe(Picker picker)
        {
            return false;
            //AxisName axis;
            //AxisSpeed speed;
            //switch (picker)
            //{
            //    case Picker.Picker1:
            //        axis = AxisName.PICK1_Z;
            //        speed = AxisSpeed.PICK1_Z;
            //        break;
            //    case Picker.Picker2:

            //        axis = AxisName.PICK2_Z;
            //        speed = AxisSpeed.PICK2_Z;
            //        break;
            //    case Picker.Picker3:

            //        axis = AxisName.PICK3_Z;
            //        speed = AxisSpeed.PICK3_Z;
            //        break;
            //    case Picker.Picker4:

            //        axis = AxisName.PICK4_Z;
            //        speed = AxisSpeed.PICK4_Z;
            //        break;
            //    default:
            //        return false;
            //}

            //return AkrAction.Current.Move(axis, 5.5, (int)speed) == 0;

        }
        public bool ZUp()
        {

            return AkrAction.Current.Move(AxisName.PRZ, 0, (int)AxisSpeed.PRZ) == 0;
        }
        public bool ZDown()
        {

            return AkrAction.Current.Move(AxisName.PRZ, 0, (int)AxisSpeed.PRZ) == 0;
        }
        public bool ClawClose()
        {
            bool res = false;
            Task.Run(() =>
            {

                res = IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 0) &&
     IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 1);
            });
            return res;
        }
        public bool ClawOpen()
        {
            return IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A, 1) &&
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B, 0);
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
        public bool MovePickPos(Picker pickerNum, int fovNum)
        {
            if (!ZUp())
            {
                return false;
            }

            SinglePoint res1 = ZuZhuang.Current.GetPickPosition((int)pickerNum, (int)pickerNum);
            if (AkrAction.Current.Move(AxisName.FSX, res1.X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0 ||
            AkrAction.Current.Move(AxisName.FSY, res1.Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) != 0)
            {

                return false;
            }
            return true;
        }
        //public bool MovePlacePos()
        //{
        //    //if (!ZUp())
        //    //{
        //    //    return false;
        //    //}

        //    ////SinglePoint res1 = ZuZhuang.Current.GetPlacePosition((int)pickerNum, fovNum);
        //    //if (AkrAction.Current.Move(AxisName.FSX, res1.X, (int)AxisSpeed.FSX, (int)AxisAcc.FSX) != 0 ||
        //    //AkrAction.Current.Move(AxisName.FSY, res1.Y, (int)AxisSpeed.FSY, (int)AxisAcc.FSY) != 0)
        //    //{

        //    //    return false;
        //    //}
        //    //return true;
        //}
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
        //public bool PlaceFoam(Picker pickerNum, int fovNum)
        //{
        //    //var temp = GetPlacePosition((int)pickerNum, fovNum);
        //    if (!ZUp())
        //    {
        //        return false;
        //    }
        //    if (!MovePlacePos(pickerNum,fovNum))
        //    {
        //        return false;
        //    }

        //    if (!ZSafe(pickerNum))
        //    {
        //        return false;
        //    }

        //    if (!Off(pickerNum))
        //    {
        //        return false;
        //    }

        //    Thread.Sleep(200);

        //    return ZUp(pickerNum);
        //}
        public bool PickFoam(Picker pickerNum, int fovNum)
        {
            if (!ZUp())
            {
                return false;
            }
            if (!MovePickPos(pickerNum, fovNum))
            {
                return false;
            }
            if (!VacOn(pickerNum))
            {
                return false;
            }


            if (!ZUp(pickerNum))
            {
                return false;
            }
            if (!ZDown(pickerNum))
            {
                return false;
            }
            return ZSafe(pickerNum);

        }

        public bool PickAllFoam()
        {
            if (!ZUp())
            {
                return false;
            }
            if (!MovePickPos(Picker.Picker1, 1))
            {
                return false;
            }
            if (!VacOn(Picker.Picker1))
            {
                return false;
            }
            if (!VacOn(Picker.Picker2))
            {
                return false;
            }
            if (!VacOn(Picker.Picker3))
            {
                return false;
            }
            if (!VacOn(Picker.Picker4))
            {
                return false;
            }

            var res = TZero(Picker.Picker1) || TZero(Picker.Picker2) || TZero(Picker.Picker3) || TZero(Picker.Picker4);
            res = res && ZDown(Picker.Picker1) || ZDown(Picker.Picker2) || ZDown(Picker.Picker3) || ZDown(Picker.Picker4);
            res = res && ZSafe(Picker.Picker1) || ZSafe(Picker.Picker2) || ZSafe(Picker.Picker3) || ZSafe(Picker.Picker4);
            return res;
        }
        public bool TRotate(Picker picker, int angle)
        {
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
        public bool TZero(Picker picker)
        {
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
