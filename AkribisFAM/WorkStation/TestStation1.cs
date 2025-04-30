using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AAMotion;
using static AAComm.Extensions.AACommFwInfo;

namespace AkribisFAM.WorkStation
{
    public class TestStation1 :WorkStationBase
    {
        private static TestStation1 _instance;

        public static TestStation1 Current
        {
            get
            {
                if (_instance == null)
                {
                        if (_instance == null)
                        {
                            _instance = new TestStation1();
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

        public override void AutoRun()
        {

            int WorkState = 11;
            try
            {
                // 开始一个工序流程
                bool processCompleted = false;
                bool start = true;
                while (!processCompleted)
                {
                    switch (WorkState)
                    {
                        case 11: // 上料
                            if (GlobalManager.Current.IsBInTarget || start)
                            {
                                if (start)
                                {
                                    start = false;
                                    Console.WriteLine("A轴开始执行的一次");
                                }
                                LoadMaterial();
                                WorkState = 20; // 切到贴装
                            }
                            break;

                        case 20: // 贴装
                            if (GlobalManager.Current.IsBInTarget)
                            {                                
                                AttachPart();
                                WorkState = 11; // 切到检测
                            }
                            break;


                        default:
                            Console.WriteLine($"Unknown WorkState: {WorkState}. Reset to 11.");
                            WorkState = 11;
                            processCompleted = true; // 出错也退出
                            break;
                    }
                    System.Threading.Thread.Sleep(300);
                }
            }
            catch (Exception ex) { }
        }

        private void LoadMaterial()
        {
            GlobalManager.Current.IsBInTarget = false;
            string axisName = "A";
            int targetPos = 200000;

            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;


            if (Enum.TryParse<AxisRef>(axisName, out AxisRef axisRef))
            {
                AAMotionAPI.MotorOn(GlobalManager.Current._Agm800.controller, axisRef);
                AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, axisRef, targetPos);
            }


            Console.WriteLine("loadMaterial 1");

            while (GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat != 4)
            {
                Console.WriteLine("当前轴A运动状态1 " + GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat);
                System.Threading.Thread.Sleep(300);
            }

            GlobalManager.Current.IsAInTarget = true;
        }

        private void AttachPart()
        {
            GlobalManager.Current.IsBInTarget = false;
            string axisName = "A";
            int targetPos = -30000;

            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (Enum.TryParse<AxisRef>(axisName, out AxisRef axisRef))
            {
                AAMotionAPI.MotorOn(GlobalManager.Current._Agm800.controller, axisRef);
                AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, axisRef, targetPos);
            }

            while (GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat != 4)
            {
                Console.WriteLine("当前轴A运动状态2 " + GlobalManager.Current._Agm800.controller.GetAxis(axisRef).InTargetStat);
                System.Threading.Thread.Sleep(300);
            }
            GlobalManager.Current.IsAInTarget = true;
            Console.WriteLine("AttachPart 1");
        }



    }
}
