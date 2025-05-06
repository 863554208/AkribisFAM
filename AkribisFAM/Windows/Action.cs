using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AAMotion;
using AkribisFAM.AAmotionFAM;
using AkribisFAM.WorkStation;

namespace AkribisFAM.WorkStation
{
    public class AkrAction
    {
        enum ACTTION_ERR
        {
            NONE = 0,
            CONNECT = 1,
            AXIS = 2,
            MOTORALARM = 3,
            GTOUPERR = 4
        }


        private static AkrAction _instance;

        public static AkrAction Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new AkrAction();
                    }
                }
                return _instance;
            }
        }

        public int axisSetParams(String AxisName, Int32 iSpeed, Int32 iAcc, Int32 iDec, Int32 iKDec, Int32 iJerk)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            if (Enum.TryParse<AxisRef>(AxisName, out AxisRef axisRef))
            {
                try
                {
                    var axis = GlobalManager.Current._Agm800.controller.GetAxis(axisRef);
                    axis.Speed = iSpeed;
                    axis.Accel = iAcc;
                    axis.Decel = iDec;
                    axis.EmrgDecel = iKDec;
                    axis.SmoothFact = iJerk;
                }
                catch
                {
                    return (int)ACTTION_ERR.MOTORALARM;
                }
            }
            else
            {
                return (int)ACTTION_ERR.AXIS;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int axisPTP(String Axis, bool bAbsRelMode, Int32 targetPos, Int32 iSpeed, Int32 iAcc, Int32 iDec)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            if (Enum.TryParse<AxisRef>(Axis, out AxisRef axisRef))
            {
                try
                {
                    var axis = GlobalManager.Current._Agm800.controller.GetAxis(axisRef);
                    axis.Speed = iSpeed;
                    axis.Accel = iAcc;
                    axis.Decel = iDec;
                }
                catch
                {
                    return (int)ACTTION_ERR.MOTORALARM;
                }

                try
                {
                    if (bAbsRelMode)
                    {
                        GlobalManager.Current._Agm800.controller.GetAxis(axisRef).MoveAbs(targetPos);
                    }
                    else
                    {
                        GlobalManager.Current._Agm800.controller.GetAxis(axisRef).MoveRel(targetPos);
                    }
                    while (GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat != 4)
                    {
                        System.Threading.Thread.Sleep(300);
                    }
                }
                catch
                {
                    return (int)ACTTION_ERR.MOTORALARM;
                }
            }
            else
            {
                return (int)ACTTION_ERR.AXIS;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int axisJog(String Axis, Int32 dir, Int32 vel, Int32 iAcc, Int32 iDec)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
            {
                try
                {
                    var axis = GlobalManager.Current._Agm800.controller.GetAxis(axisRef);
                    axis.Accel = iAcc;
                    axis.Decel = iDec;
                }
                catch
                {
                    return (int)ACTTION_ERR.MOTORALARM;
                }
                try
                {
                    GlobalManager.Current._Agm800.controller.GetAxis(axisRef).Jog(vel * dir);
                }
                catch
                {
                    return (int)ACTTION_ERR.MOTORALARM;
                }
            }
            else
            {
                return (int)ACTTION_ERR.AXIS;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int axisHome(String Axis, String path)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
            {
                try
                {
                    AAMotionAPI.Home(GlobalManager.Current._Agm800.controller, axisRef, path);
                }
                catch
                {
                    return (int)ACTTION_ERR.MOTORALARM;
                }
            }
            else
            {
                return (int)ACTTION_ERR.AXIS;
            }
            return (int)ACTTION_ERR.NONE;
        }

        // A,B,C选两个轴
        public int axisCNC_A2(String AxisX, String AxisY, Int32 xPos, Int32 yPos, Int32 velCruise, Int32 velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).ClearBuffer();  //组A
            GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisX, out AxisRef axisRefX);         
            GlobalManager.Current._Agm800.controller.GetAxis(axisRefX).MotionMode = 11;
            GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisY, out AxisRef axisRefY);
            GlobalManager.Current._Agm800.controller.GetAxis(axisRefY).MotionMode = 11;
            try
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).SetMotionProfile(percentage, Acc, Dec, sfactor);
            }
            catch
            {
                return (int)ACTTION_ERR.MOTORALARM;
            }
            if (axisRefX == AxisRef.A && axisRefY == AxisRef.B)
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).LinearAbsolute(xPos, yPos, null, velCruise, velEnd);
            }
            else if (axisRefX == AxisRef.A && axisRefY == AxisRef.C)
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).LinearAbsolute(xPos, null, yPos, velCruise, velEnd);
            }
            else if (axisRefX == AxisRef.B && axisRefY == AxisRef.C)
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).LinearAbsolute(null, xPos, yPos, velCruise, velEnd);
            }
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Begin();
            while (GlobalManager.Current._Agm800.controller.GetAxis(axisRefX).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(axisRefY).InTargetStat != 4)
            {
                System.Threading.Thread.Sleep(300);
            }

            return (int)ACTTION_ERR.NONE;
        }

        // A,B,C三轴
        public int axisCNC_A3(Int32 xPos, Int32 yPos, Int32 zPos, Int32 velCruise, Int32 velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).ClearBuffer();  //组A
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.C).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).SetMotionProfile(percentage, Acc, Dec, sfactor);
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).LinearAbsolute(xPos, yPos, zPos, velCruise, velEnd);
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).Begin();
            while (GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.C).InTargetStat != 4)
            {
                System.Threading.Thread.Sleep(300);
            }

            return (int)ACTTION_ERR.NONE;
        }

        // D,E,F选两个轴
        public int axisCNC_B2(String AxisX, String AxisY, Int32 xPos, Int32 yPos, Int32 velCruise, Int32 velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).ClearBuffer();  //组B
            GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisX, out AxisRef axisRefX);
            GlobalManager.Current._Agm800.controller.GetAxis(axisRefX).MotionMode = 11;
            GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisY, out AxisRef axisRefY);
            GlobalManager.Current._Agm800.controller.GetAxis(axisRefY).MotionMode = 11;
            try
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).SetMotionProfile(percentage, Acc, Dec, sfactor);
            }
            catch
            {
                return (int)ACTTION_ERR.MOTORALARM;
            }
            if (axisRefX == AxisRef.D && axisRefY == AxisRef.E)
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, null, xPos, yPos, null, velCruise, velEnd);
            }
            else if (axisRefX == AxisRef.D && axisRefY == AxisRef.F)
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, null, xPos, null, yPos, velCruise, velEnd);
            }
            else if (axisRefX == AxisRef.E && axisRefY == AxisRef.F)
            {
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, null, null, xPos, yPos, velCruise, velEnd);
            }
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).Begin();
            while (GlobalManager.Current._Agm800.controller.GetAxis(axisRefX).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(axisRefY).InTargetStat != 4)
            {
                System.Threading.Thread.Sleep(300);
            }

            return (int)ACTTION_ERR.NONE;
        }

        // D,E,F三轴
        public int axisCNC_B3(int xPos, int yPos, int zPos, int velCruise, int velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;

            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).ClearBuffer();  //组B
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.D).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.E).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.F).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).SetMotionProfile(percentage, Acc, Dec, sfactor);
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).LinearAbsolute(xPos, yPos, zPos, velCruise, velEnd);
            GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).Begin();
            while (GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.D).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.E).InTargetStat != 4 || GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.F).InTargetStat != 4)
            {
                System.Threading.Thread.Sleep(300);
            }

            return (int)ACTTION_ERR.NONE;
        }
        public int axisStop(string Axis)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return (int)ACTTION_ERR.CONNECT;
            if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
            {
                try
                {
                    GlobalManager.Current._Agm800.controller.GetAxis(axisRef).Stop();
                }
                catch
                {
                    return (int)ACTTION_ERR.MOTORALARM;
                }
            }
            else
            {
                return (int)ACTTION_ERR.AXIS;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int axisEnable(string Axis, bool enable)
        {
            if (!GlobalManager.Current._Agm800.controller.IsConnected)
            {
                if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
                {
                    try
                    {
                        if (enable)
                            GlobalManager.Current._Agm800.controller.GetAxis(axisRef).MotorOn = 1;
                        else
                            GlobalManager.Current._Agm800.controller.GetAxis(axisRef).MotorOn = 0;
                    }
                    catch
                    {
                        return (int)ACTTION_ERR.MOTORALARM;
                    }
                }
                else
                {
                    return (int)ACTTION_ERR.AXIS;
                }
            }
            else
            {
                return (int)ACTTION_ERR.CONNECT;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int axisAllEnable(bool enable)
        {
            int ret = 0;
            ret += axisEnable("A", enable);
            ret += axisEnable("B", enable);
            ret += axisEnable("C", enable);
            ret += axisEnable("D", enable);
            ret += axisEnable("E", enable);
            ret += axisEnable("F", enable);
            ret += axisEnable("G", enable);
            ret += axisEnable("H", enable);
            if(ret != 0)
            {
                return  (int)ACTTION_ERR.GTOUPERR;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int IOSend(int addr, bool enable)
        {

            return (int)ACTTION_ERR.NONE;
        }

    }
}
