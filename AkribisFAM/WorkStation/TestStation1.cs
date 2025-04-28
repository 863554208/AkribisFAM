using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                // 双重锁定检查，确保线程安全并且只创建一次实例
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
            //string axisName = "A";
            //int targetPos = -200000;

            //if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            //if (Enum.TryParse<AxisRef>(axisName, out AxisRef axisRef))
            //{
            //    AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, axisRef, targetPos);
            //}
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
            Console.WriteLine("UnloadMaterial");
        }

        private void InspectPart()
        {
            Console.WriteLine("InspectPart");
        }

        private void AttachPart()
        {
            Console.WriteLine("AttachPart");
        }

        private void LoadMaterial()
        {
            Console.WriteLine("LoadMaterial");
        }
    }
}
