using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using AAMotion;
using Newtonsoft.Json.Linq;

namespace AkribisFAM.CommunicationProtocol.CamerCalibProcess
{
    enum NozzleNumber
    {
        Nozzle1,
        Nozzle2,
        Nozzle3,
        Nozzle4
    }
    struct Pointposition
    {
        public double X;
        public double Y;
        public double Z;
        public double R;
    }


    class CamerCalibProcess
    {

        private static CamerCalibProcess _instance;
        public static CamerCalibProcess Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CamerCalibProcess();
                }
                return _instance;
            }
        }
        private JArray DownCameramoveAxisNozzle;//下相机机构动作执行点位

        private JArray CombineCameramoveNozzle;
        private void LoadCombinePointPosition(NozzleNumber nozzleNumber)//加载11点位置
        {
            //解析josn中拍照坐标
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            DownCameramoveAxisNozzle = obj["DownCameraMoveAxisCalibposition"]?[nozzleNumber.ToString()] as JArray;//获取轴Nozzle 数组 
        }


        public void CombineCalibrationprocess()//相机联合标定过程
        {
            //解析josn中拍照坐标
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);


            //DownCameramoveAxisNozzle = obj["DownCameraMoveAxisCalibposition"]?[nozzleNumber.ToString()] as JArray;//获取轴Nozzle 数组 

            NozzleInhale( NozzleNumber.Nozzle1);//吸嘴吸气
            if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinestart))
            {
                return;
            }
            







            if (!Point11Calibongoing(NozzleNumber.Nozzle1))
            {
                return;
            }
            if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinecalibend))
            {
                return;
            }
            CamerCombinePhotoCalib(CombineCalibProcess.Combineprocessend);
        }





        public void Point11Calibprocess(NozzleNumber nozzleNumber)//11点相机标定吸嘴过程
        {
            NozzleInhale(nozzleNumber);//吸嘴吸气
            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.start))
            {
                return;
            }

            if (!Point11Calibongoing(nozzleNumber))
            {
                return;
            }
            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.calibend))
            {
                return;
            }
            CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.processend);
            NozzleBlow(nozzleNumber);//吸嘴停止吸气
        }
        private bool Point11Calibongoing(NozzleNumber nozzleNumber)
        {
            Load11PointPosition(nozzleNumber);//加载11点位置

            if (DownCameramoveAxisNozzle == null && DownCameramoveAxisNozzle.Count == 0)
            {
                Console.WriteLine($"{nozzleNumber.ToString()}不存在或为空");
                MessageBox.Show($"{nozzleNumber.ToString()}不存在或为空");
                return false;
            }

            for (int i = 0; i < DownCameramoveAxisNozzle.Count; i++)
            {
                if (!MoveAxis(i))//移动到拍照位
                {
                    return false;
                }

                Thread.Sleep(800);

                if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing))//触发相机)
                {
                    return false;
                }  
                
            }
            return true;
        }
        private void Load11PointPosition(NozzleNumber nozzleNumber)//加载11点位置
        {
            //解析josn中拍照坐标
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            DownCameramoveAxisNozzle = obj["DownCameraMoveAxisCalibposition"]?[nozzleNumber.ToString()] as JArray;//获取轴Nozzle 数组 
        }

        private bool MoveAxis(int Index)//移动轴到目标点位
        {
            var PointArray = DownCameramoveAxisNozzle[Index] as JArray;//第几个移动点
            double[] values = PointArray.Select(x => (double)x).ToArray();//x,y,z,r

            if (PointArray == null)
            {
                MessageBox.Show($"{PointArray}点位空");
                return false; 
            }
            AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).MoveAbs(250000, 100000, 80000, 20000);
            AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).MoveAbs(250000, 100000, 80000, 20000);
            AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).MoveAbs(250000, 100000, 80000, 20000);
            AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).MoveAbs(250000, 100000, 80000, 20000);

            while (AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).InTargetStat != 4 &
                    AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).InTargetStat != 4 &
                    AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).InTargetStat != 4 &
                    AAmotionFAM.AGM800.Current.controller[0].GetAxis(AxisRef.A).InTargetStat != 4)
            {
                Thread.Sleep(50);
            }
            return true;
        }



        private bool CamerCombinePhotoCalib(CombineCalibProcess combineCalibProcess)//多相机拍照判断逻辑
        {
            return true;
        }

        private bool CamerAlonePhotoCalib(DownCamreaAloneCalibProcess downCamreaAloneCalibProcess)//11单独相机拍照判断逻辑
        {
            return true;
        }
        private void NozzleInhale(NozzleNumber nozzleNumber)//吸嘴吸气
        {
            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                case NozzleNumber.Nozzle2:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                case NozzleNumber.Nozzle3:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                case NozzleNumber.Nozzle4:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                default:
                    break;
            }
        }
        private void NozzleBlow(NozzleNumber nozzleNumber)
        {
            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                case NozzleNumber.Nozzle2:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                case NozzleNumber.Nozzle3:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                case NozzleNumber.Nozzle4:
                    //吸嘴吸气
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    break;
                default:
                    break;
                }
            }







    }
















}
