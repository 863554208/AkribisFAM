using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAMotion;
namespace AkribisFAM.WorkStation
{
    internal class TestStation2 :WorkStationBase
    {
        private static TestStation2 _instance;

        public static TestStation2 Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new TestStation2();
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

                while (!processCompleted)
                {
                    switch (WorkState)
                    {
                        case 11: // 上料
                            LoadMaterial();
                            WorkState = 20; // 切到贴装
                            break;

                        case 20: // 贴装
                            AttachPart();
                            WorkState = 30; // 切到检测
                            break;

                        case 30: // 检测
                            InspectPart();
                            WorkState = 40; // 切到下料
                            break;

                        case 40: // 下料
                            UnloadMaterial();
                            WorkState = 11; // 切回上料，表示一个完整流程结束
                            processCompleted = true; // 标记流程完成，退出 while
                            break;

                        default:
                            Console.WriteLine($"Unknown WorkState: {WorkState}. Reset to 11.");
                            WorkState = 11;
                            processCompleted = true; // 出错也退出
                            break;
                    }
                }
            }
            catch (Exception ex) { }


        }

        private void UnloadMaterial()
        {
            string axisName = "B";
            int targetPos = -200000;

            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (Enum.TryParse<AxisRef>(axisName, out AxisRef axisRef))
            {
                AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, axisRef, targetPos);
            }

            Console.WriteLine("MoveFirst 2");
        }

        private void InspectPart()
        {
            string axisName = "B";
            int targetPos = -30000;

            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (Enum.TryParse<AxisRef>(axisName, out AxisRef axisRef))
            {
                AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, axisRef, targetPos);
            }

            Console.WriteLine("MoveSecond 2");
        }

        private void AttachPart()
        {
            Console.WriteLine("AttachPart 2");
        }

        private void LoadMaterial()
        {
            Console.WriteLine("LoadMaterial 2");
        }

        
    }
}
