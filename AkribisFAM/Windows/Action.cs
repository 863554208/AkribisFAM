using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AAMotion;
using AkribisFAM.AAmotionFAM;
using AkribisFAM.WorkStation;
using HslCommunication.Profinet.Delta;
using LiveCharts.Wpf;
using YamlDotNet.Core.Tokens;
using static AkribisFAM.GlobalManager;

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

        //public int axisSetParams(String AxisName, Int32 iSpeed, Int32 iAcc, Int32 iDec, Int32 iKDec, Int32 iJerk)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    if (Enum.TryParse<AxisRef>(AxisName, out AxisRef axisRef))
        //    {
        //        try
        //        {
        //            var axis = GlobalManager.Current._Agm800.controller0.GetAxis(axisRef);
        //            axis.Speed = iSpeed;
        //            axis.Accel = iAcc;
        //            axis.Decel = iDec;
        //            axis.EmrgDecel = iKDec;
        //            axis.SmoothFact = iJerk;
        //        }
        //        catch
        //        {
        //            return (int)ACTTION_ERR.MOTORALARM;
        //        }
        //    }
        //    else
        //    {
        //        return (int)ACTTION_ERR.AXIS;
        //    }
        //    return (int)ACTTION_ERR.NONE;
        //}

        //public int axisPTP(String Axis, bool bAbsRelMode, Int32 targetPos, Int32 iSpeed, Int32 iAcc, Int32 iDec)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    if (Enum.TryParse<AxisRef>(Axis, out AxisRef axisRef))
        //    {
        //        try
        //        {
        //            var axis = GlobalManager.Current._Agm800.controller0.GetAxis(axisRef);
        //            axis.Speed = iSpeed;
        //            axis.Accel = iAcc;
        //            axis.Decel = iDec;
        //        }
        //        catch
        //        {
        //            return (int)ACTTION_ERR.MOTORALARM;
        //        }

        //        try
        //        {
        //            if (bAbsRelMode)
        //            {
        //                GlobalManager.Current._Agm800.controller0.GetAxis(axisRef).MoveAbs(targetPos);
        //            }
        //            else
        //            {
        //                GlobalManager.Current._Agm800.controller0.GetAxis(axisRef).MoveRel(targetPos);
        //            }
        //            while (GlobalManager.Current._Agm800.controller0.GetAxis(axisRef).InTargetStat != 4)
        //            {
        //                System.Threading.Thread.Sleep(300);
        //            }
        //        }
        //        catch
        //        {
        //            return (int)ACTTION_ERR.MOTORALARM;
        //        }
        //    }
        //    else
        //    {
        //        return (int)ACTTION_ERR.AXIS;
        //    }
        //    return (int)ACTTION_ERR.NONE;
        //}

        //public int axisJog(String Axis, Int32 dir, Int32 vel, Int32 iAcc, Int32 iDec)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
        //    {
        //        try
        //        {
        //            var axis = GlobalManager.Current._Agm800.controller0.GetAxis(axisRef);
        //            axis.Accel = iAcc;
        //            axis.Decel = iDec;
        //        }
        //        catch
        //        {
        //            return (int)ACTTION_ERR.MOTORALARM;
        //        }
        //        try
        //        {
        //            GlobalManager.Current._Agm800.controller0.GetAxis(axisRef).Jog(vel * dir);
        //        }
        //        catch
        //        {
        //            return (int)ACTTION_ERR.MOTORALARM;
        //        }
        //    }
        //    else
        //    {
        //        return (int)ACTTION_ERR.AXIS;
        //    }
        //    return (int)ACTTION_ERR.NONE;
        //}

        //public int axisHome(String Axis, String path)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
        //    {
        //        try
        //        {
        //            AAMotionAPI.Home(GlobalManager.Current._Agm800.controller0, axisRef, path);
        //        }
        //        catch
        //        {
        //            return (int)ACTTION_ERR.MOTORALARM;
        //        }
        //    }
        //    else
        //    {
        //        return (int)ACTTION_ERR.AXIS;
        //    }
        //    return (int)ACTTION_ERR.NONE;
        //}

        public void SetSingleEvent(GlobalManager.AxisName axisName , double? pos ,int eventSelect , int? eventPulseRes = null, int? eventPulseWid = null)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            AAMotionAPI.SetSingleEventPEG(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), ToPulse(axisName,pos), eventSelect,eventPulseRes, eventPulseWid);
        }

        public void SetEventFixedGapPEG(GlobalManager.AxisName axisName, double? beginPos, double? eventGap, double? eventEndPos, int eventSelect , int? eventPulseRes = null, int? eventPulseWid = null)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            AAMotionAPI.SetEventFixedGapPEG(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), ToPulse(axisName, beginPos), ToPulse(axisName, eventGap), ToPulse(axisName, eventEndPos), eventSelect, eventPulseRes, eventPulseWid);
        }


        public void MoveNoWait(GlobalManager.AxisName axisName, double? position, double? speed = null, double? accel = null, double? decel = null)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            if (accel == null)
            {
                accel = speed * 3;
            }
            if (decel == null)
            {
                decel = accel;
            }
                
            AAMotionAPI.MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
            AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveAbs(ToPulse(axisName, position), ToPulse(axisName, speed), ToPulse(axisName, accel), ToPulse(axisName, decel));

        }


        


        public void WaitAxis(GlobalManager.AxisName axisName)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName / 8;
            while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
            {
                //TODO 加入退出机制
                Thread.Sleep(500);
            }
        }

        public void WaitAxisAll()
        {
            AkrAction.Current.WaitAxis(AxisName.FSX);
            AkrAction.Current.WaitAxis(AxisName.FSY);
            //AkrAction.Current.WaitAxis(AxisName.LSX);
            //AkrAction.Current.WaitAxis(AxisName.LSY);
            //AkrAction.Current.WaitAxis(AxisName.PRX);
            //AkrAction.Current.WaitAxis(AxisName.PRY);
            Thread.Sleep(5000);
        }



        public void Move(GlobalManager.AxisName axisName, double? position, double? speed = null, double? accel = null, double? decel = null)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            AAMotionAPI.MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
            if (decel == null) decel = accel;
            AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveAbs(ToPulse(axisName, position), ToPulse(axisName, speed), ToPulse(axisName, accel), ToPulse(axisName, decel));
            DateTime time = DateTime.Now;

            while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
            {
                if ((DateTime.Now - time).TotalMilliseconds > 20000) break;
                //TODO 加入退出机制
                Thread.Sleep(50);
            }
        }

        public void MoveRel(GlobalManager.AxisName axisName, double? position, double? speed = null, double? accel = null, double? decel = null)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            if(decel== null) decel = accel;
            AAMotionAPI.MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
            AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveRel(ToPulse(axisName, position), ToPulse(axisName, speed), ToPulse(axisName, accel), ToPulse(axisName, decel));
            //while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
            //{
            //    //TODO 加入退出机制
            //    Thread.Sleep(50);
            //}
        }

        //public void MoveRelNoWait(GlobalManager.AxisName axisName, int distance, int? speed = null, int? accel = null, int? decel = null)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    distance = ToPulse(axisName, distance);
        //    speed = ToPulse(axisName, speed);
        //    accel = ToPulse(axisName, accel);
        //    decel = ToPulse(axisName, decel);
        //    AAMotionAPI.MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
        //    AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveRel(distance, speed, accel, decel);
        //}

        public void JogMove(GlobalManager.AxisName axisName , int dir , double vel)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            AAMotionAPI.MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
            AAMotionAPI.Jog(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), dir * ToPulse(axisName, vel));
        }

        public void Stop(GlobalManager.AxisName axisName)
        {

            try
            {
                int agmIndex = (int)axisName / 8;
                int axisRefNum = (int)axisName % 8;
                AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Stop();
            }
            catch (Exception e) { }
            
        }

        public bool MoveNGConveyor(int vel)
        {
            JogMove(GlobalManager.AxisName.BL5, 1, vel);
            JogMove(GlobalManager.AxisName.BR5, 1, vel);
            return true;
        }

        public bool StopNGConveyor()
        {
            Stop(GlobalManager.AxisName.BL5);
            Stop(GlobalManager.AxisName.BR5);
            return true;
        }


        public bool MoveConveyor(int vel)
        {
            JogMove(GlobalManager.AxisName.BL1 ,1 , vel);
            JogMove(GlobalManager.AxisName.BL2, 1, vel);
            JogMove(GlobalManager.AxisName.BL3, 1, vel);
            JogMove(GlobalManager.AxisName.BL4, 1, vel);
            //JogMove(GlobalManager.AxisName.BL5, 1, 10000);
            JogMove(GlobalManager.AxisName.BR1, 1, vel);
            JogMove(GlobalManager.AxisName.BR2, 1, vel);
            JogMove(GlobalManager.AxisName.BR3, 1, vel);
            JogMove(GlobalManager.AxisName.BR4, 1, vel);
            //JogMove(GlobalManager.AxisName.BR5, 1, 10000);
            return true;
        }

        public bool StopConveyor()
        {
            Stop(GlobalManager.AxisName.BL1);
            Stop(GlobalManager.AxisName.BL2);
            Stop(GlobalManager.AxisName.BL3);
            Stop(GlobalManager.AxisName.BL4);
            Stop(GlobalManager.AxisName.BL5);
            Stop(GlobalManager.AxisName.BR1);
            Stop(GlobalManager.AxisName.BR2);
            Stop(GlobalManager.AxisName.BR3);
            Stop(GlobalManager.AxisName.BR4);
            Stop(GlobalManager.AxisName.BR5);
            return true;
        }
        public bool StopAllAxis()
        {
            Stop(AxisName.LSX);
            Stop(AxisName.LSY);
            Stop(AxisName.FSX);
            Stop(AxisName.FSY);
            Stop(AxisName.PICK1_Z);
            Stop(AxisName.PICK1_T);
            Stop(AxisName.PICK2_Z);
            Stop(AxisName.PICK2_T);
            Stop(AxisName.PICK3_Z);
            Stop(AxisName.PICK3_T);
            Stop(AxisName.PICK4_Z);
            Stop(AxisName.PICK4_T);
            Stop(AxisName.PRX);
            Stop(AxisName.PRY);
            Stop(AxisName.PRZ);

            return true;
        }

        public int axisAllHome(String path)
        {
            try
            {
                //扩展名
                string[] fileNames = Directory.GetFiles(path);
                //var a = "D:\\akribisfam_config\\HomeFile\\LSX_homing.hseq";
                //string Axisname = "LSX";
                //int temp = (int)GlobalManager.Current.GetAxisNameFromString(Axisname);
                //int agmIndex = temp / 8;
                //int axisRefNum = temp % 8;
                //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), a);

                //////新的回原方式
                foreach (string fileName in fileNames)
                {
                    string name = Path.GetFileName(fileName).Trim();
                    string[] parts = name.Split('_');
                    string Axisname = parts[0];
                    var temp = (int)GlobalManager.Current.GetAxisNameFromString(Axisname);
                    int agmIndex = temp / 8;
                    int axisRefNum = temp % 8;

                    if (Axisname == "PRZ")
                    {
                        AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), fileName);
                        while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
                        {
                            //TODO 加入退出机制
                            Thread.Sleep(500);
                        }

                    }


                }
                foreach (string fileName in fileNames)
                {
                    string name = Path.GetFileName(fileName).Trim();
                    string[] parts = name.Split('_');
                    string Axisname = parts[0];
                    var temp = (int)GlobalManager.Current.GetAxisNameFromString(Axisname);
                    int agmIndex = temp / 8;
                    int axisRefNum = temp % 8;
                    if (Axisname != "PRZ")
                    {
                        AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), fileName);
                    }


                }




                #region 老的回原点方式
                //先对Z轴回原
                //foreach (string fileName in fileNames)
                //{
                //    string name = Path.GetFileName(fileName).Trim();

                //    string[] parts = name.Split('_');

                //    if (parts.Length >= 2)
                //    {
                //        int numericPart = int.Parse(parts[0]);
                //        int axisNumber;
                //        string AxisPart = parts[1];
                //        switch (AxisPart.ToUpper())
                //        {
                //            case "A":
                //                axisNumber = 0;
                //                break;
                //            case "B":
                //                axisNumber = 1;
                //                break;
                //            case "C":
                //                axisNumber = 2;
                //                break;
                //            case "D":
                //                axisNumber = 3;
                //                break;
                //            case "E":
                //                axisNumber = 4;
                //                break;
                //            case "F":
                //                axisNumber = 5;
                //                break;
                //            case "G":
                //                axisNumber = 6;
                //                break;
                //            case "H":
                //                axisNumber = 7;
                //                break;
                //            default:
                //                axisNumber = -1;
                //                break;
                //        }
                //        int index = numericPart * 8 + axisNumber;
                //        if (index == (int)AxisName.PICK1_Z || index == (int)AxisName.PICK2_Z || index == (int)AxisName.PICK3_Z || index == (int)AxisName.PICK4_Z || index == (int)AxisName.PRZ)
                //        {
                //            AAmotionFAM.AGM800.Current.axisRefs.TryGetValue(AxisPart, out AxisRef axisRef);
                //            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[numericPart], axisRef, fileName);
                //        }

                //    }
                //    else
                //    {
                //        Console.WriteLine("Invalid file name format: " + name);
                //    }
                //}

                ////再对其他轴回原
                //foreach (string fileName in fileNames)
                //{
                //    string name = Path.GetFileName(fileName).Trim();
                //    string[] parts = name.Split('_');
                //    if (parts.Length >= 2)
                //    {
                //        int numericPart = int.Parse(parts[0]);
                //        int axisNumber;
                //        string AxisPart = parts[1];
                //        switch (AxisPart.ToUpper())
                //        {
                //            case "A":
                //                axisNumber = 0;
                //                break;
                //            case "B":
                //                axisNumber = 1;
                //                break;
                //            case "C":
                //                axisNumber = 2;
                //                break;
                //            case "D":
                //                axisNumber = 3;
                //                break;
                //            case "E":
                //                axisNumber = 4;
                //                break;
                //            case "F":
                //                axisNumber = 5;
                //                break;
                //            case "G":
                //                axisNumber = 6;
                //                break;
                //            case "H":
                //                axisNumber = 7;
                //                break;
                //            default:
                //                axisNumber = -1;
                //                break;
                //        }
                //        int index = numericPart * 8 + axisNumber;
                //        if (index == (int)AxisName.PICK1_Z || index == (int)AxisName.PICK2_Z || index == (int)AxisName.PICK3_Z || index == (int)AxisName.PICK4_Z || index == (int)AxisName.PRZ)
                //        {
                //            break;
                //        }
                //        AAmotionFAM.AGM800.Current.axisRefs.TryGetValue(AxisPart, out AxisRef axisRef);
                //        AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[numericPart], axisRef, fileName);

                //    }
                //}
                #endregion
            }
            catch (Exception e) { }

            return (int)ACTTION_ERR.NONE;
        }

        public int axisAllZHome(String path)
        {
            int agmIndex;
            int axisRefNum;
            int temp;
            string[] fileNames = Directory.GetFiles(path);

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK1_Z");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK1_Z_homing.hseq");

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK2_Z");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK2_Z_homing.hseq");

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK3_Z");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK3_Z_homing.hseq");

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK4_Z");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK4_Z_homing.hseq");

            Thread.Sleep(10000);
            return (int)ACTTION_ERR.NONE;
        }

        public int axisAllTHome(String path)
        {
            int agmIndex;
            int axisRefNum;
            int temp;
            string[] fileNames = Directory.GetFiles(path);

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK1_T");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK1_T_homing.hseq");

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK2_T");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK2_T_homing.hseq");

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK3_T");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK3_T_homing.hseq");

            temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK4_T");
            agmIndex = temp / 8;
            axisRefNum = temp % 8;
            AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK4_T_homing.hseq");


            return (int)ACTTION_ERR.NONE;
        }

        // A,B,C选两个轴
        //public int axisCNC_A2(String AxisX, String AxisY, Int32 xPos, Int32 yPos, Int32 velCruise, Int32 velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).ClearBuffer();  //组A
        //    GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisX, out AxisRef axisRefX);         
        //    GlobalManager.Current._Agm800.controller0.GetAxis(axisRefX).MotionMode = 11;
        //    GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisY, out AxisRef axisRefY);
        //    GlobalManager.Current._Agm800.controller0.GetAxis(axisRefY).MotionMode = 11;
        //    try
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).SetMotionProfile(percentage, Acc, Dec, sfactor);
        //    }
        //    catch
        //    {
        //        return (int)ACTTION_ERR.MOTORALARM;
        //    }
        //    if (axisRefX == AxisRef.A && axisRefY == AxisRef.B)
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).LinearAbsolute(xPos, yPos, null, velCruise, velEnd);
        //    }
        //    else if (axisRefX == AxisRef.A && axisRefY == AxisRef.C)
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).LinearAbsolute(xPos, null, yPos, velCruise, velEnd);
        //    }
        //    else if (axisRefX == AxisRef.B && axisRefY == AxisRef.C)
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).LinearAbsolute(null, xPos, yPos, velCruise, velEnd);
        //    }
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).Begin();
        //    while (GlobalManager.Current._Agm800.controller0.GetAxis(axisRefX).InTargetStat != 4 || GlobalManager.Current._Agm800.controller0.GetAxis(axisRefY).InTargetStat != 4)
        //    {
        //        System.Threading.Thread.Sleep(300);
        //    }

        //    return (int)ACTTION_ERR.NONE;
        //}

        // A,B,C三轴
        //public int axisCNC_A3(Int32 xPos, Int32 yPos, Int32 zPos, Int32 velCruise, Int32 velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).ClearBuffer();  //组A
        //    GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.A).MotionMode = 11;
        //    GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.B).MotionMode = 11;
        //    GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.C).MotionMode = 11;
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).SetMotionProfile(percentage, Acc, Dec, sfactor);
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).LinearAbsolute(xPos, yPos, zPos, velCruise, velEnd);
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.A).Begin();
        //    while (GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.A).InTargetStat != 4 || GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.A).InTargetStat != 4 || GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.C).InTargetStat != 4)
        //    {
        //        System.Threading.Thread.Sleep(300);
        //    }

        //    return (int)ACTTION_ERR.NONE;
        //}

        // D,E,F选两个轴
        //public int axisCNC_B2(String AxisX, String AxisY, Int32 xPos, Int32 yPos, Int32 velCruise, Int32 velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).ClearBuffer();  //组B
        //    GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisX, out AxisRef axisRefX);
        //    GlobalManager.Current._Agm800.controller0.GetAxis(axisRefX).MotionMode = 11;
        //    GlobalManager.Current._Agm800.axisRefs.TryGetValue(AxisY, out AxisRef axisRefY);
        //    GlobalManager.Current._Agm800.controller0.GetAxis(axisRefY).MotionMode = 11;
        //    try
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).SetMotionProfile(percentage, Acc, Dec, sfactor);
        //    }
        //    catch
        //    {
        //        return (int)ACTTION_ERR.MOTORALARM;
        //    }
        //    if (axisRefX == AxisRef.D && axisRefY == AxisRef.E)
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, null, xPos, yPos, null, velCruise, velEnd);
        //    }
        //    else if (axisRefX == AxisRef.D && axisRefY == AxisRef.F)
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, null, xPos, null, yPos, velCruise, velEnd);
        //    }
        //    else if (axisRefX == AxisRef.E && axisRefY == AxisRef.F)
        //    {
        //        GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).LinearAbsolute(null, null, null, null, xPos, yPos, velCruise, velEnd);
        //    }
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Begin();
        //    while (GlobalManager.Current._Agm800.controller0.GetAxis(axisRefX).InTargetStat != 4 || GlobalManager.Current._Agm800.controller0.GetAxis(axisRefY).InTargetStat != 4)
        //    {
        //        System.Threading.Thread.Sleep(300);
        //    }

        //    return (int)ACTTION_ERR.NONE;
        //}

        // D,E,F三轴
        //public int axisCNC_B3(int xPos, int yPos, int zPos, int velCruise, int velEnd, Int32 percentage, Int32 Acc, Int32 Dec, double sfactor)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;

        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).ClearBuffer();  //组B
        //    GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.D).MotionMode = 11;
        //    GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.E).MotionMode = 11;
        //    GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.F).MotionMode = 11;
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).SetMotionProfile(percentage, Acc, Dec, sfactor);
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).LinearAbsolute(xPos, yPos, zPos, velCruise, velEnd);
        //    GlobalManager.Current._Agm800.controller0.GetCiGroup(AxisRef.B).Begin();
        //    while (GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.D).InTargetStat != 4 || GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.E).InTargetStat != 4 || GlobalManager.Current._Agm800.controller0.GetAxis(AxisRef.F).InTargetStat != 4)
        //    {
        //        System.Threading.Thread.Sleep(300);
        //    }

        //    return (int)ACTTION_ERR.NONE;
        //}
        //public int axisStop(string Axis)
        //{
        //    if (!GlobalManager.Current._Agm800.controller0.IsConnected) return (int)ACTTION_ERR.CONNECT;
        //    if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
        //    {
        //        try
        //        {
        //            GlobalManager.Current._Agm800.controller0.GetAxis(axisRef).Stop();
        //        }
        //        catch
        //        {
        //            return (int)ACTTION_ERR.MOTORALARM;
        //        }
        //    }
        //    else
        //    {
        //        return (int)ACTTION_ERR.AXIS;
        //    }
        //    return (int)ACTTION_ERR.NONE;
        //}




        public int axisEnable(GlobalManager.AxisName axisName, bool enable)
        {
            try
            {
                int agmIndex = (int)axisName / 8;
                int axisRefNum = (int)axisName % 8;
                if (enable == true)
                {
                    AAMotionAPI.MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
                }
                else
                {
                    AAMotionAPI.MotorOff(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
                }
            }
            catch (Exception e) { }

            return (int)ACTTION_ERR.NONE;
        }

        public int ToPulse(AxisName axisName ,double? mm)
        {
            if( mm==null) mm = 0;
            switch (axisName)
            {
                case AxisName.LSX:
                    return (int)(20000 * mm);

                case AxisName.LSY:
                    return (int)(20000 * mm);

                case AxisName.FSX:
                    return (int)(10000 * mm);

                case AxisName.FSY:
                    return (int)(10000 * mm);

                case AxisName.BL1:
                    return (int)(51200 / 78 * mm);

                case AxisName.BL2:
                    return (int)(51200 / 78 * mm);

                case AxisName.BL3:
                    return (int)(51200 / 78 * mm);

                case AxisName.BL4:
                    return (int)(51200 / 78 * mm);

                case AxisName.BL5:
                    return (int)(51200 / 78 * mm);

                case AxisName.BR1:
                    return (int)(51200 / 78 * mm);

                case AxisName.BR2:
                    return (int)(51200 / 78 * mm);

                case AxisName.BR3:
                    return (int)(51200 / 78 * mm);

                case AxisName.BR4:
                    return (int)(51200 / 78 * mm);

                case AxisName.BR5:
                    return (int)(51200/78 * mm);

                case AxisName.PICK1_Z:
                    return (int)(2000 * mm);

                case AxisName.PICK1_T:
                    return (int)(192000  * mm / 360);

                case AxisName.PICK2_Z:
                    return (int)(2000 * mm);

                case AxisName.PICK2_T:
                    return (int)(192000 * mm / 360);

                case AxisName.PICK3_Z:
                    return (int)(2000 * mm);

                case AxisName.PICK3_T:
                    return (int)(192000 * mm / 360);

                case AxisName.PICK4_Z:
                    return (int)(2000 * mm);

                case AxisName.PICK4_T:
                    return (int)(192000 * mm / 360);

                case AxisName.PRX:
                    return (int)(20000 * mm);

                case AxisName.PRY:
                    return (int)(20000 * mm);

                case AxisName.PRZ:
                    return (int)(10000 * mm);

                default:
                    return (int)(10000 * mm);
            }

        }

        public double ToMilimeter(AxisName axisName, int? pulse)
        {
            switch (axisName)
            {
                case AxisName.LSX:
                    return (double)(pulse / 20000.0);

                case AxisName.LSY:
                    return (double)(pulse / 20000.0);

                case AxisName.FSX:
                    return (double)(pulse / 10000.0);

                case AxisName.FSY:
                    return (double)(pulse / 10000.0);

                case AxisName.BL1:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BL2:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BL3:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BL4:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BL5:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BR1:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BR2:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BR3:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BR4:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.BR5:
                    return (double)(pulse * 78.0 / 51200.0);

                case AxisName.PICK1_Z:
                    return (double)(pulse / 2000.0);

                case AxisName.PICK1_T:
                    return (double)((pulse / 192000.0) * 360);

                case AxisName.PICK2_Z:
                    return (double)(pulse / 2000.0);

                case AxisName.PICK2_T:
                    return (double)((pulse / 192000.0) * 360);

                case AxisName.PICK3_Z:
                    return (double)(pulse / 2000.0);

                case AxisName.PICK3_T:
                    return (double)( (pulse / 192000.0) * 360 );

                case AxisName.PICK4_Z:
                    return (double)(pulse / 2000.0);

                case AxisName.PICK4_T:
                    return (double)((pulse / 192000.0) * 360);

                case AxisName.PRX:
                    return (double)(pulse / 20000.0);

                case AxisName.PRY:
                    return (double)(pulse / 20000.0);

                case AxisName.PRZ:
                    return (double)(pulse / 10000.0);

                default:
                    return (double)(pulse / 10000.0);
            }

        }

        public int SetZero(AxisName axisName)
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            AAMotionAPI.SetPosition(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum),0);
            return 1;
        }

        public int SetZeroAll()
        {
            try
            {
                int ret = 0;
                ret += SetZero(AxisName.PICK1_Z);
                ret += SetZero(AxisName.PICK1_T);
                ret += SetZero(AxisName.PICK2_Z);
                ret += SetZero(AxisName.PICK2_T);
                ret += SetZero(AxisName.PICK3_Z);
                ret += SetZero(AxisName.PICK3_T);
                ret += SetZero(AxisName.PICK4_Z);
                ret += SetZero(AxisName.PICK4_T);

                if (ret != 0)
                {
                    return (int)ACTTION_ERR.GTOUPERR;
                }
                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception e)
            { 
                return (int)ACTTION_ERR.GTOUPERR;
            }

        }

        public int axisAllZAxisEnable(bool enable)
        {
            int ret = 0;

            ret += axisEnable(AxisName.PICK1_Z, enable);
            ret += axisEnable(AxisName.PICK1_T, enable);
            ret += axisEnable(AxisName.PICK2_Z, enable);
            ret += axisEnable(AxisName.PICK2_T, enable);
            ret += axisEnable(AxisName.PICK3_Z, enable);
            ret += axisEnable(AxisName.PICK3_T, enable);
            ret += axisEnable(AxisName.PICK4_Z, enable);
            ret += axisEnable(AxisName.PICK4_T, enable);


            if (ret != 0)
            {
                return (int)ACTTION_ERR.GTOUPERR;
            }
            return (int)ACTTION_ERR.NONE;
        }


        public int axisAllEnable(bool enable)
        {
            int ret = 0;
            ret += axisEnable(AxisName.LSX, enable);
            ret += axisEnable(AxisName.LSY, enable);
            ret += axisEnable(AxisName.FSX, enable);
            ret += axisEnable(AxisName.FSY, enable);
            ret += axisEnable(AxisName.BL5, enable);
            ret += axisEnable(AxisName.BR5, enable);
            ret += axisEnable(AxisName.BL1, enable);
            ret += axisEnable(AxisName.BL2, enable);
            ret += axisEnable(AxisName.BL3, enable);
            ret += axisEnable(AxisName.BL4, enable);
            ret += axisEnable(AxisName.BR1, enable);
            ret += axisEnable(AxisName.BR2, enable);
            ret += axisEnable(AxisName.BR3, enable);
            ret += axisEnable(AxisName.BR4, enable);
            ret += axisEnable(AxisName.PICK1_Z, enable);
            ret += axisEnable(AxisName.PICK1_T, enable);
            ret += axisEnable(AxisName.PICK2_Z, enable);
            ret += axisEnable(AxisName.PICK2_T, enable);
            ret += axisEnable(AxisName.PICK3_Z, enable);
            ret += axisEnable(AxisName.PICK3_T, enable);
            ret += axisEnable(AxisName.PICK4_Z, enable);
            ret += axisEnable(AxisName.PICK4_T, enable);
            ret += axisEnable(AxisName.PRX, enable);
            ret += axisEnable(AxisName.PRY, enable);
            ret += axisEnable(AxisName.PRZ, enable);

            if (ret != 0)
            {
                return (int)ACTTION_ERR.GTOUPERR;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int IOSend(int addr, bool enable)
        {

            return (int)ACTTION_ERR.NONE;
        }

    }
}
