using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AAMotion;
using AkribisFAM;
using AkribisFAM.AAmotionFAM;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using HslCommunication.Profinet.Delta;
using LiveCharts.Wpf;
using YamlDotNet.Core.Tokens;
using static System.Net.WebRequestMethods;
using static AkribisFAM.GlobalManager;
public class MoveView
{
    private static int ProcessMotion(
        object[] parameters,
        int expectedLength,
        string errorTitle,
        Action<GlobalManager.AxisName, object[]> motionAction)
    {
        int error_code = 0;
        string error_message = string.Empty;

        if (parameters == null || parameters.Length == 0)
        {
            error_code = -100;
            error_message = "输入参数数组为空。";
        }
        else
        {
            foreach (var param in parameters)
            {
                if (error_code != 0)
                {
                    break;
                }
                if (param is object[] paramArray && paramArray.Length == expectedLength)
                {
                    if (paramArray[0] is GlobalManager.AxisName axisName)
                    {
                        try
                        {
                            int agmIndex = (int)axisName / 8;
                            int axisRefNum = (int)axisName % 8;
                            bool connectState = AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex].IsConnected;
                            if (!connectState)
                            {
                                error_message = $"执行运动指令失败。轴：" + agmIndex + "未连接！";
                                error_code = -5;

                            }
                            else
                            {
                                AAMotionAPI.MotorOn(AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));// 上使能
                                motionAction(axisName, paramArray);
                            }
                            //AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));


                        }
                        catch (TimeoutException tex)
                        {
                            error_code = -4;
                            error_message = $"超时异常：{tex.Message}";
                            break;
                        }
                        catch (Exception ex)
                        {
                            error_code = -3;
                            error_message = $"执行运动指令失败。\n异常信息：{ex.Message}";
                            break;
                        }
                    }
                    else
                    {
                        error_code = -1;
                        error_message = "参数格式错误：第一个元素不是 GlobalManager.AxisName 类型。";
                        break;
                    }
                }
                else
                {
                    error_code = -2;
                    error_message = $"参数无效：必须是 object[] 且长度为 {expectedLength}。";
                    break;
                }
            }
        }

        if (error_code != 0)
        {
            System.Windows.MessageBox.Show($"错误代码：{error_code}\n{error_message}", errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return error_code;
    }

    public static int MovePTP(params object[][] parameters)
    {
        return ProcessMotion(parameters, 5, "MovePTP 错误", (axisName, paramArray) =>
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;

            AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex]
                .GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum))
                .MoveAbs(
                    ToPulse(axisName, Convert.ToDouble(paramArray[1])),
                    ToPulse(axisName, Convert.ToDouble(paramArray[2])),
                    ToPulse(axisName, Convert.ToDouble(paramArray[3])),
                    ToPulse(axisName, Convert.ToDouble(paramArray[4]))
                );
        });
    }

    public static int MoveJog(params object[][] parameters)
    {
        return ProcessMotion(parameters, 3, "MoveJog 错误", (axisName, paramArray) =>
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;

            AAMotionAPI.Jog(
                AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex],
                GlobalManager.Current.GetAxisRefFromInteger(axisRefNum),
                (int)paramArray[1] * ToPulse(axisName, Convert.ToDouble(paramArray[2]))
            );
        });
    }

    public static int MoveRelative(params object[][] parameters)
    {
        return ProcessMotion(parameters, 5, "MoveRelative 错误", (axisName, paramArray) =>
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;

            AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex]
                .GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum))
                .MoveRel(
                    ToPulse(axisName, Convert.ToDouble(paramArray[1])),
                    ToPulse(axisName, Convert.ToDouble(paramArray[2])),
                    ToPulse(axisName, Convert.ToDouble(paramArray[3])),
                    ToPulse(axisName, Convert.ToDouble(paramArray[4]))
                );
        });
    }

    public static int MoveStop(params object[][] parameters)
    {
        return ProcessMotion(parameters, 1, "MoveStop 错误", (axisName, paramArray) =>
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;

            AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex]
                .GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum))
                .Stop();
        });
    }

    public static int WaitAxisArrived(params object[][] parameters)
    {
        return ProcessMotion(parameters, 1, "WaitAxisArrived 错误", (axisName, paramArray) =>
        {
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            DateTime start = DateTime.Now;

            while (AkribisFAM.AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat != 4)
            {
                if ((DateTime.Now - start).TotalMilliseconds > 20000)
                {
                    throw new TimeoutException("WaitAxisArrived 等待超时（20秒）。");
                }
                Thread.Sleep(50);
            }
        });
    }

    // 示例 ToPulse 方法（你需根据项目实现）
    public static int ToPulse(AxisName axisName, double? mm)
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
                return (int)(10000 * mm);

            default:
                return (int)(10000 * mm);
        }

    }

}
