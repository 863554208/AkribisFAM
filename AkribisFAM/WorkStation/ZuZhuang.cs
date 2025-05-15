using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AAMotion;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using System.Diagnostics;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.WorkStation
{
    internal class ZuZhuang : WorkStationBase
    {
        private static ZuZhuang _instance;
        public override string Name => nameof(ZuZhuang);

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;
        public event Action OnTriggerStep4;
        public event Action OnStopStep4;

        int delta = 0;
        public int board_count = 0;

        public static ZuZhuang Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new ZuZhuang();
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

        public override bool Ready()
        {
            return true;
        }

        public void CheckState()
        {
            GlobalManager.Current.Zuzhuang_state[GlobalManager.Current.current_Zuzhuang_step] = 0;
            GlobalManager.Current.ZuZhuang_CheckState();
            WarningManager.Current.WaitZuZhuang();
        }

        public static void Set(string propertyName, object value)
        {
            var propertyInfo = typeof(GlobalManager).GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(GlobalManager.Current, value);
            }
        }

        public bool WaitIO(int delta, IO_INFunction_Table index, bool value)
        {
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

        public void ResumeConveyor()
        {
            if (GlobalManager.Current.station1_IsBoardInLowSpeed || GlobalManager.Current.station3_IsBoardInLowSpeed || GlobalManager.Current.station4_IsBoardInLowSpeed)
            {
                //低速运动
                MoveConveyor(100);
            }
            else if (GlobalManager.Current.station1_IsBoardInHighSpeed || GlobalManager.Current.station3_IsBoardInHighSpeed || GlobalManager.Current.station4_IsBoardInHighSpeed)
            {
                MoveConveyor((int)AxisSpeed.BL1);
            }
        }


        public bool BoradIn()
        {
            if (GlobalManager.Current.IO_test2 && board_count==0)
            {

                //将要板信号清空
                Set("IO_test2", false);
                Set("station2_IsBoardInHighSpeed", true);

                //传送带高速移动
                MoveConveyor((int)AxisSpeed.BL1);

                Set("station2_IsBoardInHighSpeed", false);

                //等待减速光电2
                if (!WaitIO(999999, IO_INFunction_Table.IN1_1Slowdown_Sign2, true)) throw new Exception();

                //阻挡气缸2上气
                SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 1);
                SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 0);

                //标志位转换
                Set("station2_IsBoardInHighSpeed", false);
                Set("station2_IsBoardInLowSpeed", true);

                //传送带减速
                MoveConveyor(100);

                //等待料盘挡停到位信号1
                if (!WaitIO(999999, IO_INFunction_Table.IN1_5Stop_Sign2, true)) throw new Exception();

                //停止皮带移动，直到该工位顶升完成，才能继续移动皮带
                Set("station2_IsBoardInLowSpeed", false);
                Set("station2_IsBoardIn", false);
                Set("station2_IsLifting", true);

                StopConveyor();

                //执行测距位顶升气缸顶升                

                SetIO(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 0);
                SetIO(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 1);
                SetIO(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 0);

                Set("station1_IsLifting", false);

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
        public void BoardOut()
        {
            Set("station2_IsBoardOut", true);

            //模拟给下一个工位发进板信号
            GlobalManager.Current.IO_test3 = true;

            //如果后续工站正在执行出站，就不要让该工位的气缸放气和下降
            while (GlobalManager.Current.station3_IsBoardOut || GlobalManager.Current.station4_IsBoardOut)
            {
                Thread.Sleep(100);
            }
            StopConveyor();
            SetIO(IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend, 0);
            SetIO(IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract, 1);

            //出板时将穴位信息清空
            GlobalManager.Current.has_XueWeiXinXi = false;

            Set("station1_IsBoardOut", false);
            board_count--;

        }

        public void WaitConveyor(int type)
        {
            DateTime time = DateTime.Now;
            switch (type)
            {
                case 2:
                    while (SnapFeedar() == 1) ;
                    break;

                case 3:
                    while (PickFoam() == 1) ;
                    break;

                case 4:
                    while (LowerCCD() == 1) ;
                    break;

                case 5:
                    while (DropBadFoam() == 1) ;
                    break;

                case 6:
                    while (SnapPallete() == 1) ;
                    break;

                case 7:
                    while (PlaceFoam() == 1) ;
                    break;
            }
        }

        public void MoveConveyor(int vel)
        {
            AkrAction.Current.MoveConveyor(vel);
        }

        public void StopConveyor()
        {
            //TODO 停止传送带
        }

        public bool ReadIO(IO_INFunction_Table index)
        {
            return IOManager.Instance.INIO_status[(int)index];

        }

        public void SetIO(IO_OutFunction_Table index, int value)
        {
            IOManager.Instance.IO_ControlStatus(index, value);
        }

        public int SnapFeedar()
        {         
            foreach(var Point in GlobalManager.Current.feedarPoints)
            {
                AkrAction.Current.Move(AxisName.FSX, (int)Point.X, (int)AxisSpeed.FSX);
                AkrAction.Current.Move(AxisName.FSY, (int)Point.Y, (int)AxisSpeed.FSY);



            }
            
            GlobalManager.Current.BadFoamCount = 0;
            return 0;
        }

        public int PickFoam()
        {
            //这里要改成实际吸取了多少料
            GlobalManager.Current.current_FOAM_Count += 4;
            return 0;
        }

        public int DropBadFoam()
        {
            return 0;
        }

        public int LowerCCD()
        {
            return 0;
        }

        public int SnapPallete()
        {
            GlobalManager.Current.has_XueWeiXinXi = true;
            return 0;
        }

        public int PlaceFoam()
        {
            //这里要改成实际吸取了多少料
            GlobalManager.Current.current_FOAM_Count -= 4;

            GlobalManager.Current.current_Assembled += 4;

            return 0;
        }

        //public void WaitConveyor(int delta, IO[] IOarr, int type)
        //{
        //    DateTime time = DateTime.Now;

        //    if (delta != 0 && IOarr != null)
        //    {
        //        while ((DateTime.Now - time).TotalMilliseconds < delta)
        //        {
        //            int judge = 0;
        //            foreach (var item in IOarr)
        //            {
        //                var res = ReadIO(item) ? 1 : 0;
        //                judge += res;
        //            }

        //            if (judge > 0)
        //            {
        //                break;
        //            }
        //            Thread.Sleep(50);
        //        }
        //    }
        //    else
        //    {
        //        switch (type)
        //        {
        //            case 2:
        //                while (SnapFeedar() == 1);
        //                break;

        //            case 3:
        //                while (PickFoam() == 1) ;
        //                break;

        //            case 4:
        //                while (LowerCCD() == 1) ;
        //                break;

        //            case 5:
        //                while (DropBadFoam() == 1);
        //                break;

        //            case 6:
        //                while (SnapPallete() == 1) ;
        //                break;

        //            case 7:
        //                while (PlaceFoam() == 1) ;
        //                break;
        //        }
        //    }
        //}

        public bool Step1()
        {
            //测试用
            Debug.WriteLine("ZuZhuang.Current.Step1()");
            
            if (!BoradIn())
                return false;

            GlobalManager.Current.current_Zuzhuang_step = 1;

            //将当前穴位信息清空
            GlobalManager.Current.has_XueWeiXinXi = false;

            CheckState();


            return true;
        }

        public bool Step2()
        {
            Debug.WriteLine("ZuZhuang.Current.Step2()");

            GlobalManager.Current.current_Zuzhuang_step = 2;

            //到feedar上拍照
            WaitConveyor(GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public bool Step3()
        {
            Debug.WriteLine("ZuZhuang.Current.Step3-1()");

            GlobalManager.Current.current_Zuzhuang_step = 3;

            //触发 UI 动画
            OnTriggerStep3?.Invoke();

            //吸嘴取料
            //WaitConveyor(0, null, GlobalManager.Current.current_Zuzhuang_step);

            Debug.WriteLine("ZuZhuang.Current.Step3-2()");
            CheckState();
            //触发 UI 动画
            OnStopStep3?.Invoke();

            return true;
        }

        public bool Step4()
        {
            Console.WriteLine("ZuZhuang.Current.Step4()");

            GlobalManager.Current.current_Zuzhuang_step = 4;

            //触发 UI 动画
            OnTriggerStep4?.Invoke();

            //CCD2精定位
            //WaitConveyor(0, null, GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            //触发 UI 动画
            OnStopStep4?.Invoke();

            return true;
        }

        public bool Step5()
        {
            Console.WriteLine("ZuZhuang.Current.Step5()");

            GlobalManager.Current.current_Zuzhuang_step = 5;

            //拍Pallete料盘
            //WaitConveyor(0, null, GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public bool Step6()
        {
            Console.WriteLine("ZuZhuang.Current.Step5()");

            GlobalManager.Current.current_Zuzhuang_step = 6;

            //拍Pallete料盘
            //WaitConveyor(0, null, GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public bool Step7()
        {
            Console.WriteLine("ZuZhuang.Current.Step5()");

            GlobalManager.Current.current_Zuzhuang_step = 7;

            //拍Pallete料盘
            //WaitConveyor(0, null, GlobalManager.Current.current_Zuzhuang_step);

            CheckState();

            return true;
        }

        public override void AutoRun()
        {
            try
            {

                while (true)
                {
                step1:
                        bool ret = Step1();
                        if (GlobalManager.Current.Zuzhuang_exit) break;
                        if (!ret) continue;
                        //如果吸嘴上有料，直接跳去CCD2精定位
                        if (GlobalManager.Current.current_FOAM_Count > 0) goto step4;

                    step2:
                        //飞达上拍料
                        Step2();
                        if (GlobalManager.Current.Zuzhuang_exit) break;

                    step3:
                        //吸嘴取料
                        Step3();
                        if (GlobalManager.Current.Zuzhuang_exit) break;


                    step4:
                        //CCD2精定位
                        Step4();
                        if (GlobalManager.Current.Zuzhuang_exit) break;
                        if (GlobalManager.Current.BadFoamCount > 0)
                        {
                            goto step5;
                        }
                        else
                        {
                            goto step6;
                        }

                    step5:
                        //如果有坏料，放到坏料盒里
                        Step5();
                        if (GlobalManager.Current.Zuzhuang_exit) break;

                    step6:
                        //拍料盘
                        if (!GlobalManager.Current.has_XueWeiXinXi) goto step7;
                        Step6();
                        if (GlobalManager.Current.Zuzhuang_exit) break;

                    step7:
                        //放料
                        Step7();
                        if (GlobalManager.Current.Zuzhuang_exit) break;
                        //当前组装的料小于穴位数时，要一直取料
                        if (GlobalManager.Current.current_Assembled < GlobalManager.Current.total_Assemble_Count)
                        {
                            goto step2;
                        }

                        if (GlobalManager.Current.IsPass)
                        {
                            goto step2;
                        }
                        else
                        {
                            BoardOut();
                        }


                }

                #region 老代码
                //if (GlobalManager.Current.IO_test2 && !has_board)
                //{
                //    WorkState = 1;
                //    has_board = true;
                //    GlobalManager.Current.IO_test2 = false;
                //    Console.WriteLine("贴膜工位板已进");
                //}

                //// 处理板
                //if (has_board && WorkState == 1)
                //{
                //    try
                //    {
                //        WorkState = 2;
                //        GlobalManager.Current.total_Assemble_Count = 12;
                //        GlobalManager.Current.current_Assembled = 0;
                //        GlobalManager.Current.current_FOAM_Count = 0;
                //        while (GlobalManager.Current.current_Assembled < GlobalManager.Current.total_Assemble_Count)
                //        {
                //            if (GlobalManager.Current.current_FOAM_Count == 0)
                //            {
                //                //TODO 相机拍飞达上的料
                //                OnZuZhuangExecuted_2?.Invoke();
                //                while (!GlobalManager.Current.CCD1InPosition)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //                OnZuZhuangExecuted_1?.Invoke();
                //                while (!GlobalManager.Current.Feedar1Captured)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //                Console.WriteLine("已拍飞达上的料");
                //                //TODO 吸嘴吸取飞达上的4个料

                //                //现在吸嘴上实际吸了4个料
                //                GlobalManager.Current.current_FOAM_Count += 4;
                //            }

                //            //TODO 相机到CCD2拍照精定位

                //            OnZuZhuangExecuted_3?.Invoke();
                //            while (!GlobalManager.Current.CCD2Captured)
                //            {
                //                Thread.Sleep(300);
                //            }
                //            Console.WriteLine("已拍CCD2上的料");

                //            if (!GlobalManager.Current.has_XueWeiXinXi)
                //            {
                //                //TODO 对料盘拍照获取穴位信息

                //                OnZuZhuangExecuted_4?.Invoke();
                //                while (!GlobalManager.Current.MoveToLiaopan)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //                OnZuZhuangExecuted_5?.Invoke();
                //                while (!GlobalManager.Current.GrabLiaoPan)
                //                {
                //                    Thread.Sleep(300);
                //                }
                //            }

                //            //TODO 组装

                //            //目前料盘上组装了多少料
                //            GlobalManager.Current.current_Assembled += 4;

                //            //吸嘴上现在有多少foam（减去实际贴上去的料的数量） ： 如果没有foam，下一片板子走正常流程 ；如果有foam , 不再拍feeder上的料的图片
                //            GlobalManager.Current.current_FOAM_Count -= 4;

                //            Thread.Sleep(300);

                //        }

                //        WorkState = 3; // 更新状态为出板
                //    }
                //    catch (Exception ex)
                //    {
                //        has_error = true; // 标记为出错
                //    }
                //}

                //// 出板
                //if (WorkState == 3 || has_error)
                //{
                //    if (has_error)
                //    {
                //        AutorunManager.Current.isRunning = false;
                //    }

                //    WorkState = 0;
                //    has_board = false;
                //    Console.WriteLine("组装工位板已出");
                //}
                //System.Threading.Thread.Sleep(100);
                #endregion
            }
            catch (Exception ex)
            {
                AutorunManager.Current.isRunning = false;
                ErrorReportManager.Report(ex);
            }
        }

    }
}
