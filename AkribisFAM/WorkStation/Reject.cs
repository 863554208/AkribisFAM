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

namespace AkribisFAM.WorkStation
{
    internal class Reject : WorkStationBase
    {

        private static Reject _instance;
        public override string Name => nameof(Reject);

        public event Action OnTriggerStep1;
        public event Action OnStopStep1;
        public event Action OnTriggerStep2;
        public event Action OnStopStep2;
        public event Action OnTriggerStep3;
        public event Action OnStopStep3;

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


        public void MoveConveyor(int vel)
        {
            //TODO 移动传送带
        }

        public void StopConveyor()
        {
            //TODO 停止传送带
        }

        public bool ReadIO(IO index)
        {
            return GlobalManager.Current.IOTable[(int)index];
        }

        public void SetIO(IO index, bool value)
        {
            GlobalManager.Current.IOTable[(int)index] = value;
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
        }


        public bool Step1()
        {
            //Reject
            while (GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] == false)
            {
                Thread.Sleep(100);
            }
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 1);
            //顶板
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 1);
            while (GlobalManager.Current.IOTable[(int)GlobalManager.IO.Reject_JianSu] == true)
            {
                Thread.Sleep(100);
            }
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 0);
            //顶板
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 0);

            return true;


            if (!BoardIn()) return false;

            Console.WriteLine("Reject step1");

            GlobalManager.Current.current_Reject_step = 1;
            //触发 UI 动画
            OnTriggerStep1?.Invoke();
            //用thread.sleep模拟实际生成动作
            System.Threading.Thread.Sleep(1000);

            CheckState();
            //触发 UI 动画
            OnStopStep1?.Invoke();
            //ErrorManager.Current.Insert(ErrorCode.AGM800Disconnect);
            return true;
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
                        bool ret = Step1();
                        continue;
                        if (GlobalManager.Current.Reject_exit) break;
                        if (!ret) continue;

                    step2:
                        Step2();
                        if (GlobalManager.Current.Reject_exit) break;

                    step3:
                        Step3();
                        if (GlobalManager.Current.Reject_exit) break;

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
