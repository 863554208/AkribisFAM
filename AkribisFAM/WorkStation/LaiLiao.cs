using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using AAMotion;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Windows;
using static AAComm.Extensions.AACommFwInfo;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using System.CodeDom;
using static AkribisFAM.CommunicationProtocol.KEYENCEDistance.Acceptcommand;
using Microsoft.SqlServer.Server;
using AkribisFAM.Util;
using System.Windows;
namespace AkribisFAM.WorkStation
{
    internal class LaiLiao : WorkStationBase
    {

        public enum LailiaoStep
        {
            Step1,
            Step2,
            Step3,
            Complete
        }
        private static LaiLiao _instance;
        public override string Name => nameof(LaiLiao);

        public int board_count = 0;
        int delta = 0;

        List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend> AcceptKDistanceAppend = new List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend>();
        List<KEYENCEDistance.Pushcommand.SendKDistanceAppend> sendKDistanceAppend = new List<KEYENCEDistance.Pushcommand.SendKDistanceAppend>();

        public static LaiLiao Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new LaiLiao();
                    }
                }
                return _instance;
            }
        }

        public override void ReturnZero()
        {
            throw new NotImplementedException();
        }


        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public static void Get(string propertyName)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.GetValue(GlobalManager.Current);
            }
        }

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }

        public override bool Ready()
        {
            return true;
        }

        public int CheckState(int state)
        {
            if (GlobalManager.Current.Lailiao_exit) return 0;
            if (state == 0)
            {
                GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
            }
            else
            { 
                GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 1;
                ShowWarningMessage(state);
            }
            GlobalManager.Current.Lailiao_CheckState();
            WarningManager.Current.WaitLaiLiao();
            return 0;
        }

        public bool ReadIO(IO_INFunction_Table index)
        {
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
                ErrorManager.Current.Insert(ErrorCode.IOErr);
                return false;
            }
        }

        public void SetIO(IO_OutFunction_Table index , int value)
        {
            IOManager.Instance.IO_ControlStatus( index , value);
        }

        private int AddToLaserList(double height , int count)
        {
            try
            {
                int row = count / GlobalManager.Current.laser_point_length;
                int col = count % GlobalManager.Current.laser_point_length;
                GlobalManager.Current.laser_data[row][col] = height;
                return (int)ErrorCode.NoError;
            }
            catch (Exception ex)
            {
                return (int)ErrorCode.Laser_Failed;
            }
        }

        private int TriggerLaser(int count)
        {
            try
            {
                Thread.Sleep(GlobalManager.Current.LaserHeightDelay);

                if (!Task_KEYENCEDistance.SendMSData()) return (int)ErrorCode.Laser_Failed;
                //得到测量结果
                AcceptKDistanceAppend = Task_KEYENCEDistance.AcceptMSData();

                var res = AcceptKDistanceAppend[0].MeasurData;    
                
                Logger.WriteLog("激光测距结果:" + res);

                double height = AkribisFAM.Util.Parser.TryParseTwoValues("="+res);

                return AddToLaserList(height, count);
            }
            catch (Exception ex) 
            {
                Logger.WriteLog("激光测距报错 : " + ex.ToString());
                return (int)ErrorCode.Laser_Failed;
            }
        }

        private int WaitFor_X_AxesArrival()
        {
            return MoveView.WaitAxisArrived(new object[] { AxisName.LSX});
        }

        private int WaitFor_Y_AxesArrival()
        {
            return MoveView.WaitAxisArrived(new object[] { AxisName.LSY });
        }

        public int ScanBarcode()
        {
            var (barcode, error) = Task_Scanner.TriggScannerAcceptData();

            if (error == ErrorCode.BarocdeScan_Failed)
            {
                return (int)ErrorCode.BarocdeScan_Failed;
            }

            Logger.WriteLog($"Readout scanner : {barcode} ");
            GlobalManager.Current.BarcodeQueue.Enqueue(barcode ?? "NULL");

            //global switch for using mes system
            if (GlobalManager.Current.IsUseMES)
            {
                // TODO:Upload barcode to Bali MES Sytem , then judge bypass 
            }
            else
            {
                GlobalManager.Current.IsByPass = false;
            }

            return (int)ErrorCode.NoError;
        }

        public int LaserHeight()
        {

            int count=0;
            foreach (var point in GlobalManager.Current.laserPoints)
            {
                if (count % 4 == 0) 
                {
                    //var arr1 = new object[] { AxisName.LSX, (int)point.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX };
                    //var arr2 = new object[] { AxisName.LSY, (int)point.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY };


                    int x_move = AkrAction.Current.Move(AxisName.LSX, (int)point.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX);
                    int y_move = AkrAction.Current.Move(AxisName.LSY, (int)point.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY);

                    if (x_move != 0) return x_move;
                    CheckState(x_move);

                    if(y_move != 0) return y_move;
                    CheckState(y_move);

                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);
                    count++;
                }
                if (count % 4 == 1) 
                {
                    int x_move = AkrAction.Current.Move(AxisName.LSX, (int)point.X+ GlobalManager.Current.laserpoint1_shift_X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX);
                    int y_move = AkrAction.Current.Move(AxisName.LSY, (int)point.Y+ GlobalManager.Current.laserpoint1_shift_Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY);

                    if (x_move != 0) return x_move;
                    CheckState(x_move);

                    if (y_move != 0) return y_move;
                    CheckState(y_move);

                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);

                    count++;
                }
                if (count %4 == 2)
                {
                    int x_move = AkrAction.Current.Move(AxisName.LSX, (int)point.X + GlobalManager.Current.laserpoint2_shift_X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX);
                    int y_move = AkrAction.Current.Move(AxisName.LSY, (int)point.Y + GlobalManager.Current.laserpoint2_shift_Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY);

                    if (x_move != 0) return x_move;
                    CheckState(x_move);

                    if (y_move != 0) return y_move;
                    CheckState(y_move);

                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);

                    count++;
                }
                if (count % 4 == 3)
                {
                    int x_move = AkrAction.Current.Move(AxisName.LSX, (int)point.X + GlobalManager.Current.laserpoint3_shift_X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX);
                    int y_move = AkrAction.Current.Move(AxisName.LSY, (int)point.Y + GlobalManager.Current.laserpoint3_shift_Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY);

                    if (x_move != 0) return x_move;
                    CheckState(x_move);

                    if (y_move != 0) return y_move;
                    CheckState(y_move);

                    //int moveToPointX = MoveView.MovePTP(arr1);
                    //int moveToPointY = MoveView.MovePTP(arr2);

                    //if ((int)moveToPointX > 0x1000) return moveToPointX;
                    //CheckState(moveToPointX);

                    //if ((int)moveToPointY > 0x1000) return moveToPointY;
                    //CheckState(moveToPointY);

                    //int waitPointX = WaitFor_X_AxesArrival();
                    //if((int)waitPointX > 0x1000) return waitPointX;
                    //CheckState(waitPointX);

                    //int waitPointY = WaitFor_Y_AxesArrival();
                    //if ((int)waitPointY > 0x1000) return waitPointY;
                    //CheckState(waitPointY);

                    int laserProc = TriggerLaser(count);
                    if ((int)laserProc >= 0x1000) return laserProc;
                    CheckState(laserProc);

                    count++;
                }

                Thread.Sleep(100);
            }

            return 0;
        }

        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveConveyor(vel);
        }

        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
        }

        private int[] signalval = new int[10];
        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
            DateTime time = DateTime.Now;
            bool ret = false;
            int cnt = 0;
            for (int i = 0; i < signalval.Length; i++)
            {
                signalval[i] = 0;
            }
            while ((DateTime.Now - time).TotalMilliseconds < delta)
            {
                int validx = 0;
                if (cnt < 10)
                {
                    validx = cnt;
                }
                else {
                    validx = cnt%10;
                }
                if (ReadIO(index) == value)
                {
                    signalval[validx] = 1;
                }
                else
                {
                    signalval[validx] = 0;
                }
                cnt++;
                if (signalval.Sum() >= 8) { 
                    ret = true;
                    break;
                }
                Thread.Sleep(1);
            }

            return ret;
        }

        public int WaitConveyor(int type)
        {
            switch (type)
            {
                case 2: 
                    return ScanBarcode();

                case 3:
                    return LaserHeight();

                default:
                    return (int)ErrorCode.ProcessErr;
            }
        }

        public void ResumeConveyor()
        {
            if (GlobalManager.Current.station2_IsBoardInLowSpeed || GlobalManager.Current.station3_IsBoardInLowSpeed || GlobalManager.Current.station4_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(10);
                
            }
            else if (GlobalManager.Current.station2_IsBoardInHighSpeed || GlobalManager.Current.station3_IsBoardInHighSpeed || GlobalManager.Current.station4_IsBoardInHighSpeed)
            {
                MoveConveyor((int)AxisSpeed.BL1);
            }
        }

        public bool BoradIn()
        {
            //给上游发要板信号
            SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 1);

            if ((ReadIO(IO_INFunction_Table.IN7_0BOARD_AVAILABLE) && board_count == 0) || (GlobalManager.Current.IO_test1&& board_count==0))
            {
                StateManager.Current.TotalInput++;
                Set("station1_IsBoardInHighSpeed", true);


                //将要板信号清空
                SetIO(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);

                //传送带高速移动
                MoveConveyor((int)AxisSpeed.BL1);

                //等待减速光电1
                if(!WaitIO(999999, IO_INFunction_Table.IN1_0Slowdown_Sign1 ,false)) throw new Exception();

                //阻挡气缸1上气
                SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, 1);
                SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, 0);

                //标志位转换
                Set("station1_IsBoardInHighSpeed", false);
                Set("station1_IsBoardInLowSpeed", true);

                //传送带低速运动
                MoveConveyor(10);

                //等待料盘挡停到位信号1
                if (!WaitIO(999999, IO_INFunction_Table.IN1_4Stop_Sign1, true)) throw new Exception();

                //停止皮带移动，直到该工位顶升完成，才能继续移动皮带
                Set("station1_IsBoardInLowSpeed", false);
                Set("station1_IsLifting", true);
                
                StopConveyor();

                //执行测距位顶升气缸顶升                

                SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 0);
                SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 0);
                
                Set("station1_IsLifting", false);
                Set("station1_IsBoardIn", false);

                ResumeConveyor();

                board_count += 1;

                return true;
            }
            else
            {
                Thread.Sleep(100);
                return false;
            }
        }

        public void Boardout()
        {
            Logger.WriteLog(" 测距工站执行加一");
            GlobalManager.Current.flag_TrayProcessCompletedNumber++;

            #region 使用新的传送带控制逻辑
            //Set("station1_IsBoardOut", true);

            //while (ZuZhuang.Current.board_count != 0)
            //{
            //    Thread.Sleep(300);
            //}

            ////模拟给下一个工位发进板信号
            //if (GlobalManager.Current.IsByPass)
            //{
            //    GlobalManager.Current.SendByPassToStation2 = true;
            //}
            //GlobalManager.Current.IO_test2 = true;


            ////如果后续工站正在执行出站，就不要让该工位的气缸放气和下降
            ////while (GlobalManager.Current.station2_IsBoardOut || GlobalManager.Current.station3_IsBoardOut || GlobalManager.Current.station4_IsBoardOut)
            ////{
            ////    Thread.Sleep(100);
            ////}       



            ////执行气缸放气，下降
            //StopConveyor();
            //SetIO(IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract, 1);
            //SetIO(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 1);
            //SetIO(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
            //SetIO(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 1);

            //if (!WaitIO(99999,IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos, true))
            //{
            //    throw new Exception();
            //}
            //ResumeConveyor();
            //if (!WaitIO(9999, IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1, true))
            //{
            //    throw new Exception();
            //}
            ////时间预测
            //if (!WaitIO(9999, IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1, false))
            //{
            //    throw new Exception();
            //}
            ////checkState();
            ////GlobalManager.Current.IO_test1 = true;
            //Set("station1_IsBoardOut", false);
            //board_count -= 1;

            #endregion

        }
        public void checkState()
        {
            //TODO 检查状态
            if (!WaitIO(9999, IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1, false))
            {
                throw new Exception();
            }
        }

        public void CheckState()
        {
            GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
            GlobalManager.Current.Lailiao_CheckState();
            WarningManager.Current.WaitLaiLiao();
        }

        public bool Step1()
        {
            //Debug.WriteLine("LaiLiao.Current.Step1()");

            //进板
            //if (!BoradIn())
            //    return false;
            Logger.WriteLog("测距工站等待进板");

            while (GlobalManager.Current.flag_RangeFindingTrayArrived != 1)
            {
                Thread.Sleep(300);
            }

            Logger.WriteLog("测距工站进板完成");
            GlobalManager.Current.flag_RangeFindingTrayArrived = 0;
            GlobalManager.Current.currentLasered = 0;

            Thread.Sleep(300);

            GlobalManager.Current.current_Lailiao_step = 1;
            Logger.WriteLog("测距工站进板Checkstate开始");
            CheckState();
            Logger.WriteLog("测距工站进板Checkstate完成");

            return true;
        }

        public int Step2()
        {
            Console.WriteLine("LaiLiao.Current.Step2()");

            GlobalManager.Current.current_Lailiao_step = 2;

            //扫码
            Logger.WriteLog("测距工站扫码开始");
            int ret = WaitConveyor(GlobalManager.Current.current_Lailiao_step);
            Logger.WriteLog("测距工站扫码结束");
            Logger.WriteLog("测距工站扫码Checkstate开始");
            CheckState();
            Logger.WriteLog("测距工站扫码Checkstate结束");
            return ret;
        }

        public int Step3()
        {
            Console.WriteLine("LaiLiao.Current.Step3()");

            GlobalManager.Current.current_Lailiao_step = 3;

            //激光测距
            Logger.WriteLog("测距工站测距开始");
            int ret = WaitConveyor(GlobalManager.Current.current_Lailiao_step);
            Logger.WriteLog("测距工站测距结束");
            Logger.WriteLog("测距工站测距Checkstate开始");
            CheckState();
            Logger.WriteLog("测距工站测距Checkstate结束");
            return ret;
        }

        private void ShowErrorMessage(int error)
        {
            string errorName = Enum.IsDefined(typeof(ErrorCode), error)
                                ? Enum.GetName(typeof(ErrorCode), error)
                                : "未知错误";

            // 弹出错误提示框
            System.Windows.MessageBox.Show(
                $"测距工位发生致命错误：{errorName}\n 错误代码: {error}\n 即将退出该工站的运行流程",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void ShowWarningMessage(int error)
        {
            string errorName = Enum.IsDefined(typeof(ErrorCode), error)
                                ? Enum.GetName(typeof(ErrorCode), error)
                                : "未知错误";

            // 弹出错误提示框
            System.Windows.MessageBox.Show(
                $"测距工位发生报警：{errorName}\n 报警代码: {error}\n 请检查后按Resume后恢复运行",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public override void AutoRun(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    //20250519 测试 【史彦洋】 追加 Start
                    //Console.WriteLine("lailiao ceshi 1");
                    //Thread.Sleep(1000);
                    //continue;


                    step1: bool ret = Step1();
                        if (GlobalManager.Current.Lailiao_exit) break;
                        if (!ret) continue;

                    step2: 
                        int ret2 = Step2();
                        if (ret2 != 0)
                        {
                            ShowErrorMessage(ret2);
                            break;
                        }
                        if (GlobalManager.Current.Lailiao_exit) break;
                        if (GlobalManager.Current.IsByPass) goto step4;

                    step3: 
                        int ret3 = Step3();
                        if(ret3 != 0)
                        {
                            ShowErrorMessage(ret3);
                            break;
                        }
                        if (GlobalManager.Current.Lailiao_exit) break;

                    //出板
                    step4:
                        Boardout();

                    #region 老代码
                    //if (GlobalManager.Current.lailiao_ChuFaJinBan)
                    //{
                    //    //TODO 执行进板
                    //    GlobalManager.Current.lailiao_ChuFaJinBan = false;


                    //    WorkState = 1;
                    //    has_board = true;
                    //    Console.WriteLine("检测到进板信号，已进板");
                    //    GlobalManager.Current.lailiao_JinBanWanCheng = true;
                    //}

                    //// 处理板
                    //if (has_board && WorkState == 1)
                    //{
                    //    try
                    //    {
                    //        //执行完才能改变workstatiion
                    //        WorkState = 2;

                    //        //TODO 扫码枪扫码
                    //        System.Threading.Thread.Sleep(1000);
                    //        OnJinBanExecuted?.Invoke();
                    //        GlobalManager.Current.lailiao_SaoMa = true;
                    //        Console.WriteLine("扫码枪扫码已完成");

                    //        bool asd = false;
                    //        //TODO 上传条码，等待HIVE返回该板是否组装的指令
                    //        if (asd)
                    //        {
                    //            GlobalManager.Current.hive_Result = false;
                    //        }
                    //        else
                    //        {
                    //            //TODO 基恩士激光测距
                    //            System.Threading.Thread.Sleep(1000);
                    //            GlobalManager.Current.lailiao_JiGuangCeJu = true;
                    //            OnLaserExecuted.Invoke();
                    //            Console.WriteLine("激光测距已完成");
                    //        }

                    //        WorkState = 3; // 更新状态为出板
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        has_error = true; // 标记为出错
                    //        Console.WriteLine($"处理过程中发生异常: {ex.Message}");
                    //    }
                    //}

                    //// 出板
                    //if (WorkState == 3 || has_error)
                    //{
                    //    if (has_error)
                    //    {
                    //        AutorunManager.Current.isRunning = false;
                    //    }
                    //    System.Threading.Thread.Sleep(1000);
                    //    OnMovePalleteExecuted.Invoke();
                    //    WorkState = 0;
                    //    has_board = false;
                    //    Console.WriteLine("来料工站所有工序完成，流至下一工站");
                    //    GlobalManager.Current.IO_test2 = true;
                    //}

                    #endregion

                    System.Threading.Thread.Sleep(100);
                }
            }

            catch (Exception ex)
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }
    }
}
