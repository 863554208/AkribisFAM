using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.Manager;
using LiveCharts.SeriesAlgorithms;
using YamlDotNet.Core;
using HslCommunication;
using static AkribisFAM.GlobalManager;
using AkribisFAM.CommunicationProtocol;

namespace AkribisFAM.WorkStation
{
    internal class Reject : WorkStationBase
    {

        private static Reject _instance;
        public override string Name => nameof(Reject);

        private ErrorCode errorCode;

        private static DateTime startTime = DateTime.Now;

        int delta = 0;
        public int board_count = 0;

        public static Reject Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new Reject();
                    }
                }
                return _instance;
            }
        }



        public override void Initialize()
        {
            startTime = DateTime.Now;
            return;
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
            if (IOManager.Instance.INIO_status[(int)index] == 0)
            {
                return false;
            }
            else if (IOManager.Instance.INIO_status[(int)index] == 1)
            {
                return true;
            }
            else
            {
                ErrorManager.Current.Insert(ErrorCode.IOErr, $"Failed to read {index.ToString()}");
                return false;
            }
        }


        public void MoveConveyor(int vel)
        {
            //TODO 移动传送带
        }

        public void StopConveyor()
        {
            //TODO 停止传送带
        }

        public void StopNGConveyor()
        {
            AkrAction.Current.StopNGConveyor();
        }

        public void StopConveyor()
        {
            AkrAction.Current.StopConveyor();
        }


        public int DropNGPallete()
        {
            return 0;
        }

        public void WaitConveyor(int delta, IO[] IOarr, int type)
        {
            DateTime time = DateTime.Now;

            if (delta != 0 && IOarr != null)
            {
                while ((DateTime.Now - time).TotalMilliseconds < delta)
                {
                    int judge = 0;
                    foreach (var item in IOarr)
                    {
                        var res = ReadIO(item) ? 1 : 0;
                        judge += res;
                    }

                    if (judge > 0)
                    {
                        break;
                    }
                    Thread.Sleep(50);
                }
            }
            else
            {
                switch (type)
                {
                    case 2:
                        while (DropNGPallete() == 1) ;
                        break;

                }
            }
        }

        public bool BoardIn()
        {
            if (ReadIO(IO.Reject_BoardIn) && board_count == 0)
            {
                //传送带高速移动
                MoveConveyor(200);

                IO[] IOArray = new IO[] { IO.Reject_JianSu };
                WaitConveyor(9999, IOArray, 0);

                //顶板气缸上气
                SetIO(IO.Reject_QiGang ,true);

                //传送带减速
                MoveConveyor(100);

                //TODO 这边有没有告诉已经到位的IO信号？
                StopConveyor();

                //实际生产时要把这行注释掉，进板IO信号不是我们软件给
                SetIO(IO.ZuZhuang_BoardIn, false);

                board_count +=1 ;
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
            
            SetIO(IO.Reject_BoardOut, true);
            board_count--;
        }

        public void CheckState()
        {
            GlobalManager.Current.Reject_state[GlobalManager.Current.current_Reject_step] = 0;
            GlobalManager.Current.Reject_CheckState();
            WarningManager.Current.WaiReject();
            return 0;
        }

        public bool Step2()
        {
            if (GlobalManager.Current.isNGPallete)
            {
                StateManager.Current.TotalOutputNG++;
                if (!hasNGboard)
                {
                    //NG位无料
                    return ActionNG();
                }
                else
                {
                    //NG位有料
                    CheckState(false);
                    return false;
                }
            }
            else
            {
                StateManager.Current.TotalOutputOK++;
                //OK料
                return ActionOK();
            }
        }

        public int Step1()
        {
            int ret = CheckBoardIn();

            Console.WriteLine("Reject step1");

            GlobalManager.Current.current_Reject_step = 1;

            //用thread.sleep模拟实际生成动作
            System.Threading.Thread.Sleep(1000);

            CheckState();

            return ret;
        }

        public bool Step2()
        {
            Console.WriteLine("step2");

            //触发 UI 动画
            OnTriggerStep2?.Invoke();

            GlobalManager.Current.current_FuJian_step = 2;

            //NG顶升
            WaitConveyor(0, null, GlobalManager.Current.current_FuJian_step);

            CheckState();
            //触发 UI 动画
            OnStopStep2?.Invoke();

            return true;
        }

        public bool Step3()
        {
            Console.WriteLine("step3");

            //触发 UI 动画
            OnTriggerStep2?.Invoke();

            GlobalManager.Current.current_FuJian_step = 3;

            //用thread.sleep模拟实际生成动作
            System.Threading.Thread.Sleep(1000);

            CheckState();
            //触发 UI 动画
            OnStopStep2?.Invoke();

            return true;
        }

        public override void AutoRun()
        {

            try
            {
                while (true)
                {
                    step1:
                        int ret = Step1();
                        if (GlobalManager.Current.Reject_exit) break;
                        if (ret == 0) continue;
                        if (ret == 1) goto step5;

                    step2:
                        Step2();
                        if (GlobalManager.Current.Reject_exit) break;

                    step3:
                        Step3();
                        if (GlobalManager.Current.Reject_exit) break;  
                        
                    BoardOut();

                    step5:
                        ;
                        

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
