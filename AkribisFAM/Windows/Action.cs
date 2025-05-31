using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using AAMotion;
using AkribisFAM.AAmotionFAM;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using HslCommunication.Profinet.Delta;
using HslCommunication.Profinet.Inovance;
using LiveCharts.Wpf;
using YamlDotNet.Core.Tokens;
using static AAMotion.AAMotionAPI;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.WorkStation
{
    public class AkrAction
    {
        public enum ACTTION_ERR
        {
            NONE = 0,
            CONNECT = 1,
            AXIS = 2,
            MOTORALARM = 3,
            GTOUPERR = 4,
            ERR = -1
        }

        #region Declaration
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

        public string ParameterPath = string.Empty;
        public OneAxisParams axisPrm;
        public OneAxisParams[] axisParamsArray = new OneAxisParams[Enum.GetValues(typeof(AxisName)).Length];
        private int speedmultiplier = 100;

        #endregion

        #region Low level function call / Single axis

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

        public void SetSingleEvent(AxisName axisName , double? pos ,int eventSelect , int? eventPulseRes = null, int? eventPulseWid = null)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            SetSingleEventPEG(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), ToPulse(axisName,pos), eventSelect,eventPulseRes, eventPulseWid);
        }

        public void SetEventFixedGapPEG(AxisName axisName, double? beginPos, double? eventGap, double? eventEndPos, int eventSelect , int? eventPulseRes = null, int? eventPulseWid = null)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            AAMotionAPI.SetEventFixedGapPEG(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), ToPulse(axisName, beginPos), ToPulse(axisName, eventGap), ToPulse(axisName, eventEndPos), eventSelect, eventPulseRes,220000);
        }

        public void EventEnable(AxisName axisName)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            AAMotionAPI.EventEnable(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));

        }

        public void EventDisable(AxisName axisName)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            EventDisEnable(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));

        }

        public int EnableMotor(AxisName axisName, bool enable)
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                if (enable)
                {
                    MotorOn(controller, axisnum);
                    Thread.Sleep(10);
                    if (axis.InTargetStat != 1) return -1;

                }
                else
                {
                    MotorOff(controller, axisnum);
                    Thread.Sleep(10);
                    if (axis.InTargetStat != 0) return -1;

                }
            }

            catch (Exception e) { }

            return (int)ACTTION_ERR.NONE;
        }

        /// <summary>
        /// Move axis to absolute position without motion done blocking
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="position"></param>
        /// <param name="speed"></param>

        /// <returns></returns>
        public int MoveAbs(AxisName axisName, double position, double speed )
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                //if (ZAxisInSafeZone(axisName) != 0) return -1;

                //temp remain enable motor.
                MotorOn(controller, axisnum);

                var pos = ToPulse(axisName, position);
                var vel = ToPulse(axisName, speed);


                axis.MoveAbs(pos, vel);
                //Thread.Sleep(10); //delay to confirm motor movement

            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR; ;
            }

            return (int)ACTTION_ERR.NONE;

        }
        /// <summary>
        /// Move axis to absolute position with motion done blocking option
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="position"></param>
        /// <param name="speed"></param>
        /// <param name="waitmotiondone"></param>
        /// <returns></returns>
        public int MoveAbs(AxisName axisName, double position, double speed, bool waitmotiondone)
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                //if (ZAxisInSafeZone(axisName) != 0) return -1;

                //temp remain enable motor.
                MotorOn(controller, axisnum);

                var pos = ToPulse(axisName, position);
                var vel = ToPulse(axisName, speed);


                axis.MoveAbs(pos, vel);
                Thread.Sleep(10); //delay to confirm motor movement

                if(waitmotiondone)
                    if (WaitMotionDone(axisName, position) != 0) return (int)ACTTION_ERR.ERR; ;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR; ;
            }

            return (int)ACTTION_ERR.NONE;

        }

        /// <summary>
        /// Move axis relative to current position without motion done blocking
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="distance"></param>
        /// <param name="speed"></param>

        /// <returns></returns>
        public int MoveRel(AxisName axisName, double distance, double speed)
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                //if (ZAxisInSafeZone(axisName) != 0) return -1;

                //temp remain enable motor.
                MotorOn(controller, axisnum);

                var dist = ToPulse(axisName, distance);
                var vel = ToPulse(axisName, speed);

                axis.MoveRel(dist, vel);
                //Thread.Sleep(10); //delay to confirm motor movement

            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR; ;
            }

            return (int)ACTTION_ERR.NONE;

        }
        /// <summary>
        /// Move axis relative to current position with motion done blocking option
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="distance"> distance of move with direction (+/-)</param>
        /// <param name="speed"></param>
        /// <param name="waitmotiondone"></param>

        /// <returns></returns>
        public int MoveRel(AxisName axisName, double distance, double speed, bool waitmotiondone)
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                //if (ZAxisInSafeZone(axisName) != 0) return -1;

                //temp remain enable motor.
                MotorOn(controller, axisnum);

                var dist = ToPulse(axisName, distance);
                var vel = ToPulse(axisName, speed);
                var currentpos = axis.Pos;
                
                axis.MoveAbs(dist, vel);
                Thread.Sleep(10); //delay to confirm motor movement

                if (waitmotiondone)
                {
                    if (WaitMotionDone(axisName, currentpos+dist) != 0) return (int)ACTTION_ERR.ERR;
                    
                }
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR; ;
            }

            return (int)ACTTION_ERR.NONE;

        }
        //public int MoveNoWait(AxisName axisName, double? position, double? speed = null, double? accel = null, double? decel = null)
        //{
        //    //if (ZAxisInSafeZone(axisName) != 0) return -1;
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    if (accel == null)
        //    {
        //        accel = speed * 3;
        //    }
        //    if (decel == null)
        //    {
        //        decel = accel;
        //    }

        //    MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
        //    AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveAbs(ToPulse(axisName, position), ToPulse(axisName, speed), ToPulse(axisName, accel), ToPulse(axisName, decel));
        //    return 0;
        //}


        //public int WaitAxis(GlobalManager.AxisName axisName)
        /// <summary>
        /// Wait axis motion complete with position check
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="checkpos"></param>
        /// <returns></returns>
        private int WaitMotionDone(AxisName axisName, double checkpos)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
            var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
            var axis = controller.GetAxis(axisnum);
            
            DateTime startTime = DateTime.Now;
            TimeSpan timeoutDuration = TimeSpan.FromSeconds(10);
            while (axis.InTargetStat != 4 && axis.MotionStat !=0)
            {
                if (DateTime.Now - startTime > timeoutDuration)
                {
                    Logger.WriteLog($"Motion timeout at axis {axisName}");
                    return -1;
                }
                Thread.Sleep(10);
            }

            var temp2 = $"Motion complete at axis {axisName}";
            Logger.WriteLog(temp2);

            //to confirm position reach desired 
            var currentpos = ToMilimeter(axisName, axis.Pos);
            if (Math.Abs(checkpos - currentpos) > 0.05)
            {
                var err = $"Motion incomplete at axis {axisName}";
                Logger.WriteLog(err);
                return (int)ACTTION_ERR.ERR; ;
            }
            return (int)ACTTION_ERR.NONE;
        }

        //public void WaitAxisAll()
        //{
        //    Current.WaitAxis(AxisName.FSX);
        //    //AkrAction.Current.WaitAxis(AxisName.FSY);
        //    //AkrAction.Current.WaitAxis(AxisName.LSX);
        //    //AkrAction.Current.WaitAxis(AxisName.LSY);
        //    //AkrAction.Current.WaitAxis(AxisName.PRX);
        //    //AkrAction.Current.WaitAxis(AxisName.PRY);
        //    Thread.Sleep(5000);
        //}

        //让所有Z轴回到安全位置
        //public int ZUp(AxisName axisName ,AxisSpeed axisSpeed)
        //{
        //    return MoveZ(axisName, -5 ,(double)axisSpeed);
        //}


        //public int MoveZ(AxisName axisName, double? position, double? speed = null, double? accel = null, double? decel = null)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;

        //    MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
        //    if (decel == null) decel = accel;

        //    AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveAbs(ToPulse(axisName, position), ToPulse(axisName, speed), ToPulse(axisName, accel), ToPulse(axisName, decel));

        //    //设定一个预期的移动时间
        //    int nowPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Pos;
        //    double timeThreshold = 0.0;
        //    if (speed != null)
        //    {
        //        double temp_now_Pos = nowPos;
        //        double temp_target_pos = (double)position;
        //        double temp_speed = (double)speed;
        //        timeThreshold = (Math.Abs(temp_target_pos - temp_now_Pos) / temp_speed) * 1.5;
        //    }


        //    DateTime now = DateTime.Now;
        //    while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
        //    {
        //        //TODO 加入退出机制
        //        if ((DateTime.Now - now).TotalMilliseconds > timeThreshold *1000)
        //        {
        //            string err = string.Format("第{0}个AGM800的第{1}个轴PTP运动失败", agmIndex.ToString(), axisRefNum.ToString());
        //            Logger.WriteLog(err);
        //            return -1;
        //        }

        //        Thread.Sleep(50);
        //    }
        //    return 0;
        //}
        //public bool JudgeZAxis(AxisName axisName)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    int nowPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Pos;
        //    if(ToMilimeter(axisName , nowPos) > 5 ) return false;
        //    return true;
        //}

        //public int ZAxisInSafeZone(AxisName axisName)
        //{
        //    if(axisName == AxisName.PRZ || axisName == AxisName.PICK1_Z || axisName == AxisName.PICK2_Z || axisName == AxisName.PICK3_Z && axisName != AxisName.PICK3_Z) return 0;

        //    if (!JudgeZAxis(AxisName.PRZ))
        //    {
        //        return -1;
        //        //if(ZUp(AxisName.PRZ, AxisSpeed.PRZ) !=0) return -1;
        //    }

        //    if (!JudgeZAxis(AxisName.PICK1_Z))
        //    {
        //        return -1;
        //        //if (ZUp(AxisName.PICK1_Z, AxisSpeed.PICK1_Z)!=0) return -1;
        //    }
        //    if (!JudgeZAxis(AxisName.PICK2_Z))
        //    {
        //        return -1;
        //        //if(ZUp(AxisName.PICK2_Z, AxisSpeed.PICK2_Z)!=0) return -1;
        //    }
        //    if (!JudgeZAxis(AxisName.PICK3_Z))
        //    {
        //        return -1;
        //        //if(ZUp(AxisName.PICK3_Z, AxisSpeed.PICK3_Z)!=0) return -1;
        //    }
        //    if (!JudgeZAxis(AxisName.PICK4_Z))
        //    {
        //        return -1;
        //        //if(ZUp(AxisName.PICK4_Z, AxisSpeed.PICK4_Z)!=0) return -1;
        //    }

        //    return 0;
        //}
        /// <summary>
        /// Get Current axis feedback position
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="currentpos"></param>
        /// <returns></returns>
        public int GetCurrentPosition(AxisName axisName, out double currentpos)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
            var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
            var axis = controller.GetAxis(axisnum);

            currentpos = axis.Pos;
            return (int)ACTTION_ERR.NONE;
        }

        /// <summary>
        /// Set motion parameters
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="speed"></param>
        /// <param name="accel"></param>
        /// <param name="decel"></param>
        /// <returns></returns>
        public int SetMotionParameters(AxisName axisName, double speed, double accel, double decel)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
            var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
            var axis = controller.GetAxis(axisnum);

            var vel = ToPulse(axisName, speed);
            var acc = ToPulse(axisName, accel);
            var dec = ToPulse(axisName, decel);

            axis.Speed = vel;
            axis.Accel = acc;
            axis.Decel = dec;

            return (int)ACTTION_ERR.NONE;
        }

        //public int MoveRel(AxisName axisName, double? position, double? speed = null, double? accel = null, double? decel = null)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    if(decel== null) decel = accel;
        //    MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
        //    AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveRel(ToPulse(axisName, position), ToPulse(axisName, speed), ToPulse(axisName, accel), ToPulse(axisName, decel));

        //    //设定一个预期的移动时间
        //    double nowPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Pos;
        //    double timeThreshold = 0.0;
        //    if (speed != null)
        //    {
        //        double temp_now_Pos = nowPos;
        //        double temp_target_pos = (double)position;
        //        double temp_speed = (double)speed;
        //        timeThreshold = (Math.Abs(temp_target_pos - temp_now_Pos) / temp_speed) * 1.5;
        //    }

        //    DateTime now = DateTime.Now;
        //    while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
        //    {
        //        //TODO 加入退出机制
        //        if ((DateTime.Now - now).TotalMilliseconds > timeThreshold * 1000)
        //        {
        //            string err = string.Format("第{0}个AGM800的第{1}个轴相对移动失败", agmIndex.ToString(), axisRefNum.ToString());
        //            Logger.WriteLog(err);
        //            return -1;
        //        }

        //        Thread.Sleep(50);
        //    }
        //    return 0;
        //}

        //public int MoveRelNoWait(AxisName axisName, double? position, double? speed = null, double? accel = null, double? decel = null)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    if (decel == null) decel = accel;
        //    MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
        //    AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MoveRel(ToPulse(axisName, position), ToPulse(axisName, speed), ToPulse(axisName, accel), ToPulse(axisName, decel));

        //    return 0;
        //}
        /// <summary>
        /// Jog Move for axis
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="dir"> 1 = Positive, -1 = Negative </param>
        /// <param name="vel">velocity (positive always)</param>
        /// <returns></returns>
        public int JogMove(AxisName axisName , int dir , double vel)
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);
                
                //temp motor on 
                MotorOn(controller, axisnum);
                Jog(controller, axisnum, dir * ToPulse(axisName, vel));
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }

            return (int)ACTTION_ERR.NONE;
        }

        public int Stop(AxisName axisName)
        {

            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);
                axis.Stop();
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }

            return (int)ACTTION_ERR.NONE;

        }

        /// <summary>
        /// Single Axis Homing feature. Home to limit =true , will home to limit only. Home to Index = true, will home to index only. both true, will home to limit then index.
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="toLimit"></param>
        /// <param name="toIndex"></param>
        /// <param name="isPositive"></param>
        /// <returns></returns>
        public int HomeAxis(AxisName axisName, bool toLimit, bool toIndex, bool isPositive)
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                var axisparam = axisParamsArray[(int)axisName];
                var homevel = ToPulse(axisName, axisparam.HomeVelocity);
                var homedecel = ToPulse(axisName, axisparam.Deceleration);
                var homeoffset = ToPulse(axisName, axisparam.HomeOffset);
                var direction = (isPositive) ? 1 : -1;

                axis.Home(toLimit, toIndex, homevel*direction, (uint)homedecel, (uint)homedecel, homeoffset, 30000);

                while (!IsHomeCompleted(axisName))
                {
                    if (axis.HomingStat < 0)
                    {
                        return axis.HomingStat;
                    }
                    Thread.Sleep(50);
                }
                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }

        public bool IsHomeCompleted(AxisName axisName)
        {
            var agmIndex = (int)axisName / 8;
            var axisRefNum = (int)axisName % 8;
            var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
            var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
            var axis = controller.GetAxis(axisnum);

            return axis.HomingStat == 100;
        }

        public int HomeAxisToHardstop(AxisName axisName,bool isPositive)
        {
            try
            {
                var agmIndex = (int)axisName / 8;
                var axisRefNum = (int)axisName % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                var axisparam = axisParamsArray[(int)axisName];
                var homevel = ToPulse(axisName, axisparam.HomeVelocity);
                var homedecel = ToPulse(axisName, axisparam.Deceleration);
                var homeoffset = ToPulse(axisName, axisparam.HomeOffset);
                var direction = (isPositive) ? 1 : -1;
                var threshold = ToPulse(axisName, 0.1);

                axis.HomeToHardStop(threshold,homevel, (uint)homedecel, (uint)homedecel, homeoffset, 30000);

                while (!IsHomeCompleted(axisName))
                {
                    if (axis.HomingStat < 0)
                    {
                        return axis.HomingStat;
                    }
                    Thread.Sleep(50);
                }
                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        //todo: motion status

        #endregion

        #region Module Gantry Move control

        /// <summary>
        /// Move Laser Gantry XY move
        /// </summary>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        /// <returns></returns>
        public int MoveLaserXY(double xpos, double ypos)
        {
            try
            {
                var xaxis = AxisName.LSX;
                var yaxis = AxisName.LSY;
                var xspeed = axisParamsArray[(int)AxisName.LSX].Velocity * speedmultiplier;
                var yspeed = axisParamsArray[(int)AxisName.LSY].Velocity * speedmultiplier;


                //start move XY
                if(MoveAbs(xaxis, xpos, xspeed)!=0 || MoveAbs(yaxis, ypos, yspeed) !=0)
                    return (int)ACTTION_ERR.ERR;

                //wait xy motion done
                if(WaitMotionDone(xaxis, xpos)!=0 || WaitMotionDone(yaxis, ypos)!=0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }

        /// <summary>
        /// Move Foam Gantry XY move
        /// </summary>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        /// <returns></returns>
        public int MoveFoamXY(double xpos, double ypos)
        {
            try
            {
                var xaxis = AxisName.FSX;
                var yaxis = AxisName.FSY;
                var xspeed = axisParamsArray[(int)AxisName.FSX].Velocity * speedmultiplier;
                var yspeed = axisParamsArray[(int)AxisName.FSY].Velocity * speedmultiplier;

                //Foam Gantry Z protection ///////////////////////////////////////////////////////////////////
                var z1 = AxisName.PICK1_Z;
                var z2 = AxisName.PICK2_Z;
                var z3 = AxisName.PICK3_Z;
                var z4 = AxisName.PICK4_Z;
                var zspeed = axisParamsArray[(int)AxisName.PICK1_Z].Velocity; //use z1 as ref for protection speed

                if (GetCurrentPosition(z1,out var z1pos)!=0|| GetCurrentPosition(z2, out var z2pos) != 0|| 
                   GetCurrentPosition(z3, out var z3pos) != 0 || GetCurrentPosition(z4, out var z4pos) != 0)
                    return (int)ACTTION_ERR.ERR;

                if (z1pos > 0.5) if (MoveAbs(z1, 0, zspeed, true) != 0) return (int)ACTTION_ERR.ERR;
                if (z2pos > 0.5) if (MoveAbs(z2, 0, zspeed, true) != 0) return (int)ACTTION_ERR.ERR;
                if (z3pos > 0.5) if (MoveAbs(z3, 0, zspeed, true) != 0) return (int)ACTTION_ERR.ERR;
                if (z4pos > 0.5) if (MoveAbs(z4, 0, zspeed, true) != 0) return (int)ACTTION_ERR.ERR;
                /////////////////////////////////////////////////////////////////////////////////////////////////

                //start move XY - to consider vector move
                if (MoveAbs(xaxis, xpos, xspeed) != 0 || MoveAbs(yaxis, ypos, yspeed) != 0)
                    return (int)ACTTION_ERR.ERR;

                //wait xy motion done
                if (WaitMotionDone(xaxis, xpos) != 0 || WaitMotionDone(yaxis, ypos) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        //todo: MoveFoamZ1Z2Z3Z4
        /// <summary>
        /// Gang move 4 Z pickers
        /// </summary>
        /// <param name="z1pos"></param>
        /// <param name="z2pos"></param>
        /// <param name="z3pos"></param>
        /// <param name="z4pos"></param>
        /// <returns></returns>
        public int MoveFoamZ1Z2Z3Z4(double z1pos, double z2pos, double z3pos, double z4pos)
        {
            try
            {
                var z1 = AxisName.PICK1_Z;
                var z2 = AxisName.PICK2_Z;
                var z3 = AxisName.PICK3_Z;
                var z4 = AxisName.PICK4_Z;
                var z1speed = axisParamsArray[(int)AxisName.PICK1_Z].Velocity * speedmultiplier; 
                var z2speed = axisParamsArray[(int)AxisName.PICK2_Z].Velocity * speedmultiplier; 
                var z3speed = axisParamsArray[(int)AxisName.PICK3_Z].Velocity * speedmultiplier; 
                var z4speed = axisParamsArray[(int)AxisName.PICK4_Z].Velocity * speedmultiplier;

                //start move 4Z 
                if (MoveAbs(z1, z1pos, z1speed) != 0 || MoveAbs(z2, z2pos, z2speed) != 0||
                    MoveAbs(z3, z3pos, z3speed) != 0 || MoveAbs(z4, z4pos, z4speed) != 0)
                    return (int)ACTTION_ERR.ERR;

                //wait 4Z motion done
                if (WaitMotionDone(z1, z1pos) != 0 || WaitMotionDone(z2, z2pos) != 0|| 
                    WaitMotionDone(z3, z3pos) != 0 || WaitMotionDone(z4, z4pos) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamZ1(double z1pos)
        {
            try
            {
                var z1 = AxisName.PICK1_Z;
                var z1speed = axisParamsArray[(int)AxisName.PICK1_Z].Velocity * speedmultiplier;


                //start move Z1 - to consider vector move
                if (MoveAbs(z1, z1pos, z1speed,true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamZ2(double z2pos)
        {
            try
            {
                var z2 = AxisName.PICK2_Z;
                var z2speed = axisParamsArray[(int)AxisName.PICK2_Z].Velocity * speedmultiplier;


                //start move Z2 - to consider vector move
                if (MoveAbs(z2, z2pos, z2speed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamZ3(double z3pos)
        {
            try
            {
                var z3 = AxisName.PICK3_Z;
                var z3speed = axisParamsArray[(int)AxisName.PICK3_Z].Velocity * speedmultiplier;


                //start move Z3 - to consider vector move
                if (MoveAbs(z3, z3pos, z3speed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamZ4(double z4pos)
        {
            try
            {
                var z4 = AxisName.PICK4_Z;
                var z4speed = axisParamsArray[(int)AxisName.PICK4_Z].Velocity * speedmultiplier;


                //start move Z4 - to consider vector move
                if (MoveAbs(z4, z4pos, z4speed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamT1T2T3T4(double t1pos, double t2pos, double t3pos, double t4pos)
        {
            try
            {
                var t1 = AxisName.PICK1_T;
                var t2 = AxisName.PICK2_T;
                var t3 = AxisName.PICK3_T;
                var t4 = AxisName.PICK4_T;
                var t1speed = axisParamsArray[(int)AxisName.PICK1_T].Velocity * speedmultiplier;
                var t2speed = axisParamsArray[(int)AxisName.PICK2_T].Velocity * speedmultiplier;
                var t3speed = axisParamsArray[(int)AxisName.PICK3_T].Velocity * speedmultiplier;
                var t4speed = axisParamsArray[(int)AxisName.PICK4_T].Velocity * speedmultiplier;

                //start move 4T 
                if (MoveAbs(t1, t1pos, t1speed) != 0 || MoveAbs(t2, t2pos, t2speed) != 0 ||
                    MoveAbs(t3, t3pos, t3speed) != 0 || MoveAbs(t4, t4pos, t4speed) != 0)
                    return (int)ACTTION_ERR.ERR;

                //wait 4T motion done
                if (WaitMotionDone(t1, t1pos) != 0 || WaitMotionDone(t2, t2pos) != 0 ||
                    WaitMotionDone(t3, t3pos) != 0 || WaitMotionDone(t4, t4pos) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamT1(double t1pos)
        {
            try
            {
                var t1 = AxisName.PICK1_T;
                var t1speed = axisParamsArray[(int)AxisName.PICK1_T].Velocity * speedmultiplier;


                //start move t1 - to consider vector move
                if (MoveAbs(t1, t1pos, t1speed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamT2(double t2pos)
        {
            try
            {
                var t2 = AxisName.PICK2_T;
                var t2speed = axisParamsArray[(int)AxisName.PICK2_T].Velocity * speedmultiplier;


                //start move t2 - to consider vector move
                if (MoveAbs(t2, t2pos, t2speed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
         public int MoveFoamT3(double t3pos)
        {
            try
            {
                var t3 = AxisName.PICK3_T;
                var t3speed = axisParamsArray[(int)AxisName.PICK3_T].Velocity * speedmultiplier;


                //start move t3 - to consider vector move
                if (MoveAbs(t3, t3pos, t3speed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveFoamT4(double t4pos)
        {
            try
            {
                var t4 = AxisName.PICK4_T;
                var t4speed = axisParamsArray[(int)AxisName.PICK4_T].Velocity * speedmultiplier;


                //start move t4 - to consider vector move
                if (MoveAbs(t4, t4pos, t4speed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveRecheckXY(double xpos, double ypos)
        {
            try
            {
                var xaxis = AxisName.PRX;
                var yaxis = AxisName.PRY;
                var xspeed = axisParamsArray[(int)AxisName.PRX].Velocity * speedmultiplier;
                var yspeed = axisParamsArray[(int)AxisName.PRY].Velocity * speedmultiplier;

                //Recheck Gantry Z protection ///////////////////////////////////////////////////////////////////
                var z1 = AxisName.PRZ;
                var zspeed = axisParamsArray[(int)AxisName.PRZ].Velocity; //use z1 as ref for protection speed

                if (GetCurrentPosition(z1, out var z1pos) != 0 )
                    return (int)ACTTION_ERR.ERR;

                if (z1pos > 0.5) if (MoveAbs(z1, 0, zspeed, true) != 0) return (int)ACTTION_ERR.ERR;
                /////////////////////////////////////////////////////////////////////////////////////////////////

                //start move XY - to consider vector move
                if (MoveAbs(xaxis, xpos, xspeed) != 0 || MoveAbs(yaxis, ypos, yspeed) != 0)
                    return (int)ACTTION_ERR.ERR;

                //wait xy motion done
                if (WaitMotionDone(xaxis, xpos) != 0 || WaitMotionDone(yaxis, ypos) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        public int MoveRecheckZ(double zpos)
        {
            try
            {
                var z = AxisName.PRZ;
                var zspeed = axisParamsArray[(int)AxisName.PRZ].Velocity * speedmultiplier;


                //start move Z - to consider vector move
                if (MoveAbs(z, zpos, zspeed, true) != 0)
                    return (int)ACTTION_ERR.ERR;

                return (int)ACTTION_ERR.NONE;
            }
            catch (Exception ex)
            {
                return (int)ACTTION_ERR.ERR;
            }
        }
        //todo: HomeLaserModule
        //todo: HomeFoamModule
        //todo: HomeRecheckModule

        #endregion

        #region System Control

        public int EnableAllMotors(bool toenable)
        {
            int ret = 0;
            foreach (AxisName axisname in (AxisName[])Enum.GetValues(typeof(AxisName)))
            {
                ret += EnableMotor(axisname, toenable);
            }
            if (ret != 0)
            {
                return (int)ACTTION_ERR.GTOUPERR;
            }
            return (int)ACTTION_ERR.NONE;
        }

        public int CheckAllEnableMotorState(out bool allEnable)
        {
            int ret = 0, total = 0;
            foreach (AxisName axisname in (AxisName[])Enum.GetValues(typeof(AxisName)))
            {
                var agmIndex = (int)axisname / 8;
                var axisRefNum = (int)axisname % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                ret += (axis.InTargetStat == 1) ? 1 : 0;
                total++;
            }

            allEnable = (ret == total);
           
            return (int)ACTTION_ERR.NONE;
        }
        //todo: SystemHoming - paul
        public int CheckAllAxisHomeCompleted(out bool allEnable)
        {
            int ret = 0, total = 0;
            foreach (AxisName axisname in (AxisName[])Enum.GetValues(typeof(AxisName)))
            {
                var agmIndex = (int)axisname / 8;
                var axisRefNum = (int)axisname % 8;
                var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
                var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
                var axis = controller.GetAxis(axisnum);

                ret += (axis.HomingStat == 100) ? 1 : 0;
                total++;
            }

            allEnable = (ret == total);

            return (int)ACTTION_ERR.NONE;
        }
        /// <summary>
        /// Set Machine speed multiplier
        /// </summary>
        /// <param name="percentage">10 to 100</param>
        public void SetSpeedMultiplier(int percentage)
        {
            if (percentage < 10) percentage = 10;
            if (percentage >100) percentage = 100;

            speedmultiplier = percentage/100;
        }
        /// <summary>
        /// Return current speed multiplier in percentage (10 to 100%)
        /// </summary>
        public int CurrentSpeedMultiplier => speedmultiplier * 100;
        #endregion
        public int StopAllAxis()
        {
            var ret = 0;
            ret += Stop(AxisName.LSX);
            ret += Stop(AxisName.LSY);
            ret += Stop(AxisName.FSX);
            ret += Stop(AxisName.FSY);
            ret += Stop(AxisName.PICK1_Z);
            ret += Stop(AxisName.PICK1_T);
            ret += Stop(AxisName.PICK2_Z);
            ret += Stop(AxisName.PICK2_T);
            ret += Stop(AxisName.PICK3_Z);
            ret += Stop(AxisName.PICK3_T);
            ret += Stop(AxisName.PICK4_Z);
            ret += Stop(AxisName.PICK4_T);
            ret += Stop(AxisName.PRX);
            ret += Stop(AxisName.PRY);
            ret += Stop(AxisName.PRZ);

            return (ret != 0) ? (int)ACTTION_ERR.ERR : (int)ACTTION_ERR.NONE;
        }

        #region Conveyor control
        public int StartNGConveyor()
        {
            var vel = axisParamsArray[(int)AxisName.BL5].Velocity;
            if (JogMove(AxisName.BL5, 1, vel) != 0 || JogMove(AxisName.BR5, 1, vel)!=0)
            {
                return (int)ACTTION_ERR.ERR;

            }
            return (int)ACTTION_ERR.NONE;
        }
        public int StopNGConveyor()
        {

            if (Stop(AxisName.BL5) != 0 || Stop(AxisName.BR5) != 0)
            {
                return (int)ACTTION_ERR.ERR;

            }
            return (int)ACTTION_ERR.NONE;
        }
        public int MoveAllConveyor()
        {
            var vel1 = axisParamsArray[(int)AxisName.BL1].Velocity;
            var vel2 = axisParamsArray[(int)AxisName.BL2].Velocity;
            var vel3 = axisParamsArray[(int)AxisName.BL3].Velocity;
            var vel4 = axisParamsArray[(int)AxisName.BL4].Velocity;
            var vel5 = axisParamsArray[(int)AxisName.BL5].Velocity;

            //use left side conveyor as reference velocity

            var ret = 0;
            ret += JogMove(AxisName.BL1, 1, vel1);
            ret += JogMove(AxisName.BL2, 1, vel2);
            ret += JogMove(AxisName.BL3, 1, vel3);
            ret += JogMove(AxisName.BL4, 1, vel4);
            ret += JogMove(AxisName.BL5, 1, vel5);

            ret += JogMove(AxisName.BR1, 1, vel1);
            ret += JogMove(AxisName.BR2, 1, vel2);
            ret += JogMove(AxisName.BR3, 1, vel3);
            ret += JogMove(AxisName.BR4, 1, vel4);
            ret += JogMove(AxisName.BR5, 1, vel5);

            return (ret != 0) ? (int)ACTTION_ERR.ERR : (int)ACTTION_ERR.NONE;
        }
        public int StopConveyor()
        {
            var ret = 0;
            ret += Stop(AxisName.BL1);
            ret += Stop(AxisName.BL2);
            ret += Stop(AxisName.BL3);
            ret += Stop(AxisName.BL4);
            ret += Stop(AxisName.BL5);
            ret += Stop(AxisName.BR1);
            ret += Stop(AxisName.BR2);
            ret += Stop(AxisName.BR3);
            ret += Stop(AxisName.BR4);
            ret += Stop(AxisName.BR5);
            return (ret != 0) ? (int)ACTTION_ERR.ERR : (int)ACTTION_ERR.NONE;
        }

        #endregion
        #region utilities
        public int ToPulse(AxisName axisName, double? mm)
        {
            if (mm == null) mm = 0;
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
                    return (int)(51200 / 78 * mm);

                case AxisName.PICK1_Z:
                    return (int)(2000 * mm);

                case AxisName.PICK1_T:
                    return (int)(192000 * mm / 360);

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
                    return (int)(2500 * mm);

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
                    return (double)((pulse / 192000.0) * 360);

                case AxisName.PICK4_Z:
                    return (double)(pulse / 2000.0);

                case AxisName.PICK4_T:
                    return (double)((pulse / 192000.0) * 360);

                case AxisName.PRX:
                    return (double)(pulse / 20000.0);

                case AxisName.PRY:
                    return (double)(pulse / 20000.0);

                case AxisName.PRZ:
                    return (double)(pulse / 2500.0);

                default:
                    return (double)(pulse / 10000.0);
            }

        }
        private bool save_parameters(AxisName axisName)
        {
            try
            {
                string fPath = Path.Combine(ParameterPath, axisName + ".xml");
                if (!axisPrm.Save(fPath))
                {
                    //LastErrorMessage = axisPrm.LastErrorMessage;
                    return false;
                }
                else
                {
                    //set_parameters(axisName);
                }
            }
            catch (Exception ex)
            {
                //LastErrorMessage = "[MotorGMAS|save_parameters] AxisName:" + ex.AxisName + ":CommandID:" + ex.CommandID + ":ErrorID: " + (ex.MMCError).ToString() + " Detail: " + ex.What;
                return false;
            }
            return true;
        }
        //public bool load_parameters(AxisName axisName)
        //{
        //    try
        //    {
        //        string fPath = Path.Combine(ParameterPath, axisName + ".xml");
        //        axisPrm = new OneAxisParams();
        //        if (!axisPrm.Load(fPath))
        //        {
        //            //axisPrm.SetAxisName(iAxis, sMotors);
        //        }
        //        //else
        //        //{
        //        //    set_default_parameters();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        //LastErrorMessage = "[MotorGMAS|load_parameters] AxisName:" + ex.AxisName + ":CommandID:" + ex.CommandID + ":ErrorID: " + (ex.MMCError).ToString() + " Detail: " + ex.What;
        //        return false;
        //    }
        //    return true;
        //}
        public bool load_all_parameters()
        {
            try
            {
                int i = 0;
                foreach (AxisName axisname in (AxisName[])Enum.GetValues(typeof(AxisName)))
                {
                    string fPath = Path.Combine(ParameterPath, axisname + ".xml");
                    axisPrm = new OneAxisParams();
                    if (!axisPrm.Load(fPath))
                    {
                        //axisPrm.SetAxisName(iAxis, sMotors);
                        save_parameters(axisname);
                    }
                    else
                    {
                        //todo: mask off during first run
                        SetMotionParameters(axisname, axisPrm.Velocity, axisPrm.Acceleration, axisPrm.Deceleration);
                    }

                    axisParamsArray[i] = axisPrm; //store param data into array.
                    i++;

                    //else
                    //{
                    //    set_default_parameters();
                    //}
                }
            }
            catch (Exception ex)
            {
                //LastErrorMessage = "[MotorGMAS|load_parameters] AxisName:" + ex.AxisName + ":CommandID:" + ex.CommandID + ":ErrorID: " + (ex.MMCError).ToString() + " Detail: " + ex.What;
                return false;
            }
            return true;
        }
        #endregion
        //public int WaitAllHomingFinished()
        //{
        //    int ret = 0;
        //    ret += WaitHomingFinished(AxisName.LSX);
        //    ret += WaitHomingFinished(AxisName.LSY);
        //    ret += WaitHomingFinished(AxisName.FSX);
        //    ret += WaitHomingFinished(AxisName.FSY);

        //    ret += WaitHomingFinished(AxisName.PRX);
        //    ret += WaitHomingFinished(AxisName.PRY);
        //    ret += WaitHomingFinished(AxisName.PRZ);

        //    ret += WaitHomingFinished(AxisName.PICK1_T);
        //    ret += WaitHomingFinished(AxisName.PICK2_T);
        //    if (ret != 0) return -1; 

        //    return 0;
        //}
        //public int WaitAllHomingZFinished()
        //{
        //    int ret = 0;

        //    ret += WaitHomingFinished(AxisName.PICK1_Z);
        //    ret += WaitHomingFinished(AxisName.PICK2_Z);
        //    //ret += WaitHomingFinished(AxisName.PICK3_Z);
        //    //ret += WaitHomingFinished(AxisName.PICK4_Z);


        //    if (ret > 0) return -1;

        //    return 0;
        //}
        //public int WaitHomingFinished(AxisName axisName)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    DateTime now = DateTime.Now;
        //    while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).HomingStat!= 100)
        //    {
        //        if ((DateTime.Now - now).TotalMilliseconds > 30000)
        //        {
        //            string temp = string.Format("第{0}个AGM800的第{1}个轴回零失败", (agmIndex+1).ToString(), axisRefNum.ToString());
        //            Logger.WriteLog(temp);
        //            return 1;
        //        }

        //        Thread.Sleep(50);
        //    }
        //    string err = string.Format("第{0}个AGM800的第{1}个轴回零成功", (agmIndex + 1).ToString(), axisRefNum.ToString());
        //    return 0;
        //}
        //public int axisAllHome(String path)
        //{
        //    try
        //    {
        //        //扩展名
        //        string[] fileNames = Directory.GetFiles(path);
        //        //var a = "D:\\akribisfam_config\\HomeFile\\LSX_homing.hseq";
        //        //string Axisname = "LSX";
        //        //int temp = (int)GlobalManager.Current.GetAxisNameFromString(Axisname);
        //        //int agmIndex = temp / 8;
        //        //int axisRefNum = temp % 8;
        //        //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), a);

        //        //////新的回原方式
        //        foreach (string fileName in fileNames)
        //        {
        //            string name = Path.GetFileName(fileName).Trim();
        //            string[] parts = name.Split('_');
        //            string Axisname = parts[0];
        //            var temp = (int)GlobalManager.Current.GetAxisNameFromString(Axisname);
        //            int agmIndex = temp / 8;
        //            int axisRefNum = temp % 8;

        //            if (Axisname == "PRZ")
        //            {
        //                Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), fileName);
        //                while (AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
        //                {
        //                    //TODO 加入退出机制
        //                    Thread.Sleep(50);
        //                }

        //            }


        //        }
        //        foreach (string fileName in fileNames)
        //        {
        //            string name = Path.GetFileName(fileName).Trim();
        //            string[] parts = name.Split('_');
        //            string Axisname = parts[0];
        //            var temp = (int)GlobalManager.Current.GetAxisNameFromString(Axisname);
        //            int agmIndex = temp / 8;
        //            int axisRefNum = temp % 8;
        //            if (Axisname != "PRZ")
        //            {
        //                Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), fileName);
        //            }
        //        }

        //    }
        //    catch (Exception e) { }

        //    return (int)ACTTION_ERR.NONE;
        //}

        //public int axisAllZHome(String path)
        //{
        //    int agmIndex;
        //    int axisRefNum;
        //    int temp;
        //    string[] fileNames = Directory.GetFiles(path);

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK1_Z");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK1_Z_homing.hseq");

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK2_Z");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK2_Z_homing.hseq");

        //    //temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK3_Z");
        //    //agmIndex = temp / 8;
        //    //axisRefNum = temp % 8;
        //    //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK3_Z_homing.hseq");

        //    //temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK4_Z");
        //    //agmIndex = temp / 8;
        //    //axisRefNum = temp % 8;
        //    //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZ\\PICK4_Z_homing.hseq");

        //    //Thread.Sleep(10000);
        //    return (int)ACTTION_ERR.NONE;
        //}

        //public int axisAllZHome_HardStop()
        //{
        //    int agmIndex;
        //    int axisRefNum;
        //    int temp;

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK1_Z");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK1_Z_hardstop.hseq");

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK2_Z");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK2_Z_hardstop.hseq");

        //    //temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK3_Z");
        //    //agmIndex = temp / 8;
        //    //axisRefNum = temp % 8;
        //    //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK3_Z_hardstop.hseq");

        //    //temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK4_Z");
        //    //agmIndex = temp / 8;
        //    //axisRefNum = temp % 8;
        //    //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK4_Z_hardstop.hseq");

        //    return (int)ACTTION_ERR.NONE;
        //}

        //public int axisAllTHome(String path)
        //{
        //    int agmIndex;
        //    int axisRefNum;
        //    int temp;
        //    string[] fileNames = Directory.GetFiles(path);

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK1_T");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK1_T_homing.hseq");

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK2_T");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK2_T_homing.hseq");

        //    //temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK3_T");
        //    //agmIndex = temp / 8;
        //    //axisRefNum = temp % 8;
        //    //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK3_T_homing.hseq");

        //    //temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK4_T");
        //    //agmIndex = temp / 8;
        //    //axisRefNum = temp % 8;
        //    //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileT\\PICK4_T_homing.hseq");


        //    return (int)ACTTION_ERR.NONE;
        //}

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




        //public int SetZero(AxisName axisName)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    SetPosition(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum),0);
        //    return 1;
        //}

        //public int SetZeroAll()
        //{
        //    try
        //    {
        //        int ret = 0;
        //        ret += SetZero(AxisName.PICK1_Z);
        //        ret += SetZero(AxisName.PICK1_T);
        //        ret += SetZero(AxisName.PICK2_Z);
        //        ret += SetZero(AxisName.PICK2_T);
        //        //ret += SetZero(AxisName.PICK3_Z);
        //        //ret += SetZero(AxisName.PICK3_T);
        //        ret += SetZero(AxisName.PICK4_Z);
        //        ret += SetZero(AxisName.PICK4_T);

        //        if (ret != 0)
        //        {
        //            return (int)ACTTION_ERR.GTOUPERR;
        //        }
        //        return (int)ACTTION_ERR.NONE;
        //    }
        //    catch (Exception e)
        //    { 
        //        return (int)ACTTION_ERR.GTOUPERR;
        //    }

        //}


        

        //public int IOSend(int addr, bool enable)
        //{

        //    return (int)ACTTION_ERR.NONE;
        //}
        //public int axisAllZHome_HardStop()
        //{
        //    int agmIndex;
        //    int axisRefNum;
        //    int temp;

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK1_Z");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK1_Z_hardstop.hseq");

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK2_Z");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK2_Z_hardstop.hseq");

        //    //temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK3_Z");
        //    //agmIndex = temp / 8;
        //    //axisRefNum = temp % 8;
        //    //AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK3_Z_hardstop.hseq");

        //    temp = (int)GlobalManager.Current.GetAxisNameFromString("PICK4_Z");
        //    agmIndex = temp / 8;
        //    axisRefNum = temp % 8;
        //    Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), "D:\\akribisfam_config\\HomeFileZHardStop\\PICK4_Z_hardstop.hseq");

        //    return (int)ACTTION_ERR.NONE;
        //}

        //private void set_parameters(AxisName axisName)
        //{
        //    int agmIndex = (int)axisName / 8;
        //    int axisRefNum = (int)axisName % 8;
        //    var controller = AAmotionFAM.AGM800.Current.controller[agmIndex];
        //    var axisnum = GlobalManager.Current.GetAxisRefFromInteger(axisRefNum);
        //    var axis = controller.GetAxis(axisnum);

        //    axis.Accel= ToPulse(axisName, axisPrm.Acceleration);
        //    axis.Decel = ToPulse(axisName, axisPrm.Deceleration);
        //    axis.Speed = ToPulse(axisName, axisPrm.Velocity);

        //}


    }
    public class OneAxisParams
    {
        #region Structures
        [Serializable]
        public struct OneAxisParameters
        {
            //public int AxisNumber;
            //public int HomeTimeout;
            //public int MoveTimeout;
            //public int HomingMethod;
            //public string AxisName;
            public double Velocity;
            public double Acceleration;
            public double Deceleration;
            //public double Jerk;
            public double HomeVelocity;
            //public double KillDeceleration;
            //public double ConversionFactor;
            public double HomeOffset;
        };
        #endregion

        #region PublicVariables
        public OneAxisParameters prmVal;
        public string LastErrorMessage = string.Empty;
        #endregion

        #region Constructors
        public OneAxisParams()
        {
        }
        #endregion

        #region ParameterDefinitions
        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Define the axis number of this motor in the system")]
        //public int AxisNumber {
        //    get { return prmVal.AxisNumber; }
        //    set { prmVal.AxisNumber = value; }
        //}

        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Displays the axis name of this motor (read only)")]
        //public string AxisName {
        //    get { return prmVal.AxisName; }
        //}

        [CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the velocity for this motor during move (in mm/sec)")]
        public double Velocity {
            get { return prmVal.Velocity; }
            set { prmVal.Velocity = value; }
        }

        [CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the acceleration for this motor during home and move (in mm/sec2)")]
        public double Acceleration {
            get { return prmVal.Acceleration; }
            set { prmVal.Acceleration = value; }
        }

        [CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the deceleration for this motor during home and move (in mm/sec2)")]
        public double Deceleration {
            get { return prmVal.Deceleration; }
            set { prmVal.Deceleration = value; }
        }

        [CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the velocity for this motor during home (in mm/sec)")]
        public double HomeVelocity {
            get { return prmVal.HomeVelocity; }
            set { prmVal.HomeVelocity = value; }
        }

        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the deceleration when this motor is killed (in mm/sec2)")]
        //public double KillDeceleration {
        //    get { return prmVal.KillDeceleration; }
        //    set { prmVal.KillDeceleration = value; }
        //}

        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the jerk speed for this motor during move (in mm/sec)")]
        //public double Jerk {
        //    get { return prmVal.Jerk; }
        //    set { prmVal.Jerk = value; }
        //}

        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the conversion factor from pulses to mm (read only)")]
        //public double ConversionFactor {
        //    get { return prmVal.ConversionFactor; }
        //}

        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the length of time before motor timeout occur during home (in milliseconds)")]
        //public int HomeTimeout {
        //    get { return prmVal.HomeTimeout; }
        //    set { prmVal.HomeTimeout = value; }
        //}

        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the length of time before motor timeout occur during move (in milliseconds)")]
        //public int MoveTimeout {
        //    get { return prmVal.MoveTimeout; }
        //    set { prmVal.MoveTimeout = value; }
        //}

        [CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the length of time before motor timeout occur during move (in milliseconds)")]
        public double HomeOffset {
            get { return prmVal.HomeOffset; }
            set { prmVal.HomeOffset = value; }
        }

        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Set the length of time before motor timeout occur during move (in milliseconds)")]
        //public int HomingMethod {
        //    get { return prmVal.HomingMethod; }
        //    set { prmVal.HomingMethod = value; }
        //}
        //[CategoryAttribute("Axis Parameters"), DescriptionAttribute("Define the axis name of this group axis in the system")]
        //public List<string> GroupAxisName {
        //    get { return prmVal.GroupAxisName; }
        //    set { prmVal.GroupAxisName = value; }
        //}
        #endregion

        #region PublicMethods
        public bool Load(string FullFilename)
        {
            XmlSerializer serializer = null;
            FileStream fStream = null;
            TextReader reader = null;
            try
            {
                prmVal = new OneAxisParameters();
                serializer = new XmlSerializer(prmVal.GetType());
                fStream = new FileStream(FullFilename, FileMode.OpenOrCreate);
                reader = new StreamReader(fStream);
                prmVal = (OneAxisParameters)serializer.Deserialize(reader);
                fStream.Close();
                reader.Close();
            }
            catch (Exception ex)
            {
                LastErrorMessage = "[OneAxisParams|Load] " + " Detail: " + ex.Message; //+ex.Message;
                if (fStream != null)
                    fStream.Close();
                if (reader != null)
                    reader.Close();
                SetDefaults();
                return false;
            }
            return true;
        }

        public bool Save(string FullFilename)
        {
            int si = FullFilename.LastIndexOf('\\');
            string path = FullFilename.Substring(0, si);
            string name = FullFilename.Substring(si + 1);
            BackupFile(path, name);
            XmlSerializer serializer = null;
            StreamWriter fWriter = null;
            try
            {
                serializer = new XmlSerializer(prmVal.GetType());
                fWriter = new StreamWriter(FullFilename);
                serializer.Serialize(fWriter, prmVal);
                fWriter.Close();
            }
            catch (Exception ex)
            {
                LastErrorMessage = "[OneAxisParams|Save] " + " Detail: " + ex.Message;
                if (fWriter != null)
                    fWriter.Close();
                return false;
            }
            return true;
        }

        //public void SetAxisName(int axisNo, string axisName)
        //{
        //    prmVal.AxisNumber = axisNo;
        //    prmVal.AxisName = axisName;
        //}
        #endregion

        #region PrivateMethods
        private bool BackupFile(string buPath, string buFilename)
        {
            try
            {
                string fileName = DateTime.Now.ToString("BU_yyyyMMdd_HHmmss_") + buFilename;
                string sourceFile = Path.Combine(buPath, buFilename);
                string targetPath = Path.Combine(buPath, "Backups");
                string destFile = Path.Combine(buPath, "Backups", fileName);
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);
                if (File.Exists(sourceFile))
                    File.Copy(sourceFile, destFile, true);
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }
            return true;
        }

        private void SetDefaults()
        {
            //prmVal.AxisNumber = 0;
            //prmVal.AxisName = string.Empty;
            prmVal.Velocity = 10000;
            prmVal.Acceleration = 10000;
            prmVal.Deceleration = 10000;
            prmVal.HomeVelocity = 10000;
            //prmVal.KillDeceleration = 10000;
            //prmVal.Jerk = 10000;
            //prmVal.ConversionFactor = 1;
            //prmVal.HomeTimeout = 30000;
            //prmVal.MoveTimeout = 3000;
            //prmVal.HomingMethod = 0;
            prmVal.HomeOffset = 0.0;
        }
        #endregion
    }

}
