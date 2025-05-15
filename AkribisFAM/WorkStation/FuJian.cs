using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.Manager;
using LiveCharts.SeriesAlgorithms;
using YamlDotNet.Core;
using HslCommunication;
using static AkribisFAM.GlobalManager;
using AkribisFAM.CommunicationProtocol;
using System.Reflection;
using YamlDotNet.Core.Tokens;
using System.Xml.Linq;

namespace AkribisFAM.WorkStation
{
    internal class FuJian : WorkStationBase
    {

        private static FuJian _instance;
        public override string Name => nameof(FuJian);

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;

        int delta = 0;
        public int board_count = 0;

        public static FuJian Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new FuJian();
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

        public bool ReadIO(IO index)
        {
            return GlobalManager.Current.IOTable[(int)index];
        }

        public void SetIO(IO index, bool value)
        {
            GlobalManager.Current.IOTable[(int)index] = value;
        }

        public void MoveConveyor(int vel)
        {
            //TODO 移动传送带
        }

        public void StopConveyor()
        {
            //TODO 停止传送带
        }

        public bool BoardIn()
        {
            if (ReadIO(IO.ZuZhuang_BoardIn) && board_count == 0)
            {
                //进入后改回false
                SetIO(IO.ZuZhuang_BoardIn, false);
                //传送带高速移动
                MoveConveyor(200);

                IO[] IOArray = new IO[] { IO.LaiLiao_JianSu };
                while (true) {
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN1_2Slowdown_Sign3]) {
                        Thread.Sleep(100);
                        if(IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN1_2Slowdown_Sign3])
                            break;
                    }
                }
                WaitConveyor(9999, IOArray, 0);

                //顶板气缸上气
                SetIO(IO.FuJian_QiGang,true);

                //传送带减速
                MoveConveyor(100);

                //TODO 这边有没有告诉已经到位的IO信号？
                StopConveyor();

                //实际生产时要把这行注释掉，进板IO信号不是我们软件给
                SetIO(IO.FuJian_BoardIn, false);

                GlobalManager.Current.IO_test3 = false;
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
            SetIO(IO.FuJian_BoardOut, true);
            board_count--;
            GlobalManager.Current.IO_test4 = true;
        }

        public void CheckState()
        {
            GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
            GlobalManager.Current.FuJian_CheckState();
            WarningManager.Current.WaiFuJian();
        }

        public int RemoveFilm()
        {
            return 0;
        }
        public int CCD3ReCheck()
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
                        while (RemoveFilm() == 1);
                        break;
                    case 3:
                        while (CCD3ReCheck() == 1) ;
                        break;


                }
            }
        }

        public bool Step1()
        {
            //FuJian
            while (GlobalManager.Current.IOTable[(int)GlobalManager.IO.FuJian_JianSu] == false)
            {
                Thread.Sleep(100);
            }
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 1);
            //顶板
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 1);
            while (GlobalManager.Current.IOTable[(int)GlobalManager.IO.FuJian_JianSu] == true)
            {
                Thread.Sleep(100);
            }
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 0);
            //顶板
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 0);

            return true;

            if (!BoardIn()) return false;

            Console.WriteLine("Fujian step1");
            //触发 UI 动画
            OnTriggerStep1?.Invoke();
            //用thread.sleep模拟实际生成动作
            System.Threading.Thread.Sleep(1000);

            GlobalManager.Current.current_FuJian_step = 1;

            CheckState();
            //触发 UI 动画
            OnStopStep1?.Invoke();
            return true;
        }
        public bool Step2()
        {
            Console.WriteLine("step2");

            GlobalManager.Current.current_FuJian_step = 2;

            //触发 UI 动画
            OnTriggerStep2?.Invoke();

            //撕膜
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
            OnTriggerStep3?.Invoke();

            GlobalManager.Current.current_FuJian_step = 3;
            //CCD3复检
            WaitConveyor(0, null, GlobalManager.Current.current_FuJian_step);

            CheckState();
            //触发 UI 动画
            OnStopStep3?.Invoke();

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
                    if (GlobalManager.Current.FuJian_exit) break;
                    if (!ret) continue;

                step2:
                    Step2();
                    if (GlobalManager.Current.FuJian_exit)break;

                step3:
                    Step3();
                    if (GlobalManager.Current.FuJian_exit) break;

                BoardOut();
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
