using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using AAMotion;
using AkribisFAM.WorkStation;
using Newtonsoft.Json.Linq;
using static AkribisFAM.GlobalManager;

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
    enum MovingCameraCalibposition
    {
        FeedDischarging,
        Vehicles
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
        public async Task CombineCalibrationprocess()//相机联合标定过程
        {
            await Task.Run(new Action(() =>
            {
                //解析josn中拍照运动坐标
                string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
                string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
                JObject obj = JObject.Parse(json);
                double[] MoveVehiclesPhotoposition = ((Newtonsoft.Json.Linq.JArray)obj["CombineCalibProcessposition"]["MoveVehiclesPhotoposition"]).ToObject<double[]>();//载具标定拍照位
                double[] MoveReservepickmylar = ((Newtonsoft.Json.Linq.JArray)obj["CombineCalibProcessposition"]["MoveReservepickmylar"]).ToObject<double[]>();//预取标定片位
                double[] Movepickmylar = ((Newtonsoft.Json.Linq.JArray)obj["CombineCalibProcessposition"]["Movepickmylar"]).ToObject<double[]>();//取标定片位
                double[] MoveFeedPhotoposition = ((Newtonsoft.Json.Linq.JArray)obj["CombineCalibProcessposition"]["MoveFeedPhotoposition"]).ToObject<double[]>();//飞达标定拍照位
                double[] MoveReservePutmylar = ((Newtonsoft.Json.Linq.JArray)obj["CombineCalibProcessposition"]["MoveReservePutmylar"]).ToObject<double[]>();//预放标定片位
                double[] Moveputmylar = ((Newtonsoft.Json.Linq.JArray)obj["CombineCalibProcessposition"]["Moveputmylar"]).ToObject<double[]>();//放标定片位
                //标定开始
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinestart, "CombineStart"))
                {
                    return;
                }
                //移动到载具标定拍照位
                MoveAxisDirectControl(MoveVehiclesPhotoposition[0], MoveVehiclesPhotoposition[1], MoveVehiclesPhotoposition[2], MoveVehiclesPhotoposition[3], NozzleNumber.Nozzle1);

                //移动到预取标定片位
                MoveAxisDirectControl(MoveReservepickmylar[0], MoveReservepickmylar[1], MoveReservepickmylar[2], MoveReservepickmylar[3], NozzleNumber.Nozzle1);
                //移动到取标定片位
                MoveAxisDirectControl(Movepickmylar[0], Movepickmylar[1], Movepickmylar[2], Movepickmylar[3], NozzleNumber.Nozzle1);

                Thread.Sleep(500);//延时
                //取标定片指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinepick, $"{Movepickmylar[0].ToString()},{Movepickmylar[1].ToString()},{Movepickmylar[3].ToString()}"))
                {
                    return;
                }
                //移动到预取标定片位
                MoveAxisDirectControl(MoveReservepickmylar[0], MoveReservepickmylar[1], MoveReservepickmylar[2], MoveReservepickmylar[3], NozzleNumber.Nozzle1);

                //移动到载具标定拍照位
                MoveAxisDirectControl(MoveVehiclesPhotoposition[0], MoveVehiclesPhotoposition[1], MoveVehiclesPhotoposition[2], MoveVehiclesPhotoposition[3], NozzleNumber.Nozzle1);


                Thread.Sleep(500);//延时
                //关联取指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.CombineRelationpick, $"{MoveVehiclesPhotoposition[0].ToString()},{MoveVehiclesPhotoposition[1].ToString()},0"))
                {
                    return;
                }
                //移动到预取标定片位
                MoveAxisDirectControl(MoveReservepickmylar[0], MoveReservepickmylar[1], MoveReservepickmylar[2], MoveReservepickmylar[3], NozzleNumber.Nozzle1);

                //移动到取标定片位
                MoveAxisDirectControl(Movepickmylar[0], Movepickmylar[1], Movepickmylar[2], Movepickmylar[3], NozzleNumber.Nozzle1);

                Thread.Sleep(500);//延时
                                  //吸嘴吸气
                NozzleInhale(NozzleNumber.Nozzle1);
                Thread.Sleep(500);//延时

                //移动到预取标定片位
                MoveAxisDirectControl(MoveReservepickmylar[0], MoveReservepickmylar[1], MoveReservepickmylar[2], MoveReservepickmylar[3], NozzleNumber.Nozzle1);

                //移动到载具标定拍照位
                MoveAxisDirectControl(MoveVehiclesPhotoposition[0], MoveVehiclesPhotoposition[1], MoveVehiclesPhotoposition[2], MoveVehiclesPhotoposition[3], NozzleNumber.Nozzle1);

                //进入11点位置循环
                if (!Point11Calibongoing(NozzleNumber.Nozzle1))
                {
                    return;
                }
                //移动到飞达标定拍照位
                MoveAxisDirectControl(MoveFeedPhotoposition[0], MoveFeedPhotoposition[1], MoveFeedPhotoposition[2], MoveFeedPhotoposition[3], NozzleNumber.Nozzle1);

                //移动到预放标定片位
                MoveAxisDirectControl(MoveReservePutmylar[0], MoveReservePutmylar[1], MoveReservePutmylar[2], MoveReservePutmylar[3], NozzleNumber.Nozzle1);

                //移动到放标定片位
                MoveAxisDirectControl(Moveputmylar[0], Moveputmylar[1], Moveputmylar[2], Moveputmylar[3], NozzleNumber.Nozzle1);

                Thread.Sleep(500);
                //吸嘴停止吸气
                NozzleStopInhale(NozzleNumber.Nozzle1);
                Thread.Sleep(500);
                //放标定片指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combineput,$"{Moveputmylar[0].ToString()},{Moveputmylar[1].ToString()},{Moveputmylar[3].ToString()}"))
                {
                    return;
                }
                //移动到预放标定片位
                MoveAxisDirectControl(MoveReservePutmylar[0], MoveReservePutmylar[1], MoveReservePutmylar[2], MoveReservePutmylar[3], NozzleNumber.Nozzle1);

                //移动到飞达标定拍照位
                MoveAxisDirectControl(MoveFeedPhotoposition[0], MoveFeedPhotoposition[1], MoveFeedPhotoposition[2], MoveFeedPhotoposition[3],NozzleNumber.Nozzle1);


                Thread.Sleep(500);
                //关联放指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.CombineRelationput, $"{MoveFeedPhotoposition[0].ToString()},{MoveFeedPhotoposition[1].ToString()},0"))
                {
                    return;
                }
                //标定结束等待结果
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinecalibend,"calibend"))
                {
                    return;
                }
                //此次标定结束
                CamerCombinePhotoCalib(CombineCalibProcess.Combineprocessend, "processend");
            }));
        }

        public async Task Point11Calibprocess(NozzleNumber nozzleNumber)//11点相机标定吸嘴过程
        {
            await Task.Run(new Action(() =>
            {
                //吸嘴吸气
                //NozzleInhale(nozzleNumber);
                //标定开始
                if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.start, nozzleNumber,"Alonestart"))
                {
                    return;
                }
                //进入11点位置循环
                if (!Point11Calibongoing(nozzleNumber))
                {
                    return;
                }
                //标定结束等待结果
                if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.calibend, nozzleNumber,"Aloneend"))
                {
                    return;
                }
                //吸嘴停止吸气
               // NozzleStopInhale(nozzleNumber);
                //此次标定结束
                CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.processend, nozzleNumber,"processend");
            }));
        }

        public async Task Point9Calibprocess(MovingCameraCalibposition movingCameraCalibposition)//九点相机标定吸嘴过程
        {
            await Task.Run(new Action(() =>
            {
                //标定开始
                if (!MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess.start, movingCameraCalibposition,"Movestart"))
                {
                    return;
                }

                //九点运动过程
                Func<MovingCameraCalibposition, bool> Point9Calibongoing = e =>
                {
                    string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
                    string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
                    JObject obj = JObject.Parse(json);
                    var MoveCameraCalibmoveAxisNozzle = obj["MoveCameraCalibMoveAxisCalibposition"]?[movingCameraCalibposition.ToString()] as JArray;//获取轴Nozzle 数组 

                    if (MoveCameraCalibmoveAxisNozzle == null && MoveCameraCalibmoveAxisNozzle.Count == 0)
                    {
                        Console.WriteLine($"{movingCameraCalibposition.ToString()}不存在或为空");
                        MessageBox.Show($"{movingCameraCalibposition.ToString()}不存在或为空");
                        return false;
                    }

                    for (int i = 0; i < MoveCameraCalibmoveAxisNozzle.Count; i++)
                    {
                        var PointArray = MoveCameraCalibmoveAxisNozzle[i] as JArray;//第几个移动点
                        double[] values = PointArray.Select(x => (double)x).ToArray();//x,y,z,r

                        if (PointArray == null)
                        {
                            MessageBox.Show($"九点标定{PointArray}点位空");
                            return false;
                        }
                        //移动到拍照位
                        MoveAxisDirectControl(values[0], values[1], values[2], values[3], NozzleNumber.Nozzle1);//x,y,z,r


                        Thread.Sleep(800);

                    //单独九点标定
                    if (!MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, movingCameraCalibposition,$"{values[0].ToString()},{values[1].ToString()},0"))
                        {
                            return false;
                        }
                    }
                    return true;
                };


                //进入9点位置循环
                if (!Point9Calibongoing(movingCameraCalibposition))
                {
                    return;
                }
                //标定结束等待结果
                if (!MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess.calibend, movingCameraCalibposition, "MoveCalibEnd"))
                {
                    return;
                }
                //此次标定结束
                MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess.processend, movingCameraCalibposition,"MoveprocessEnd");
            }));
        }

        private bool Point11Calibongoing(NozzleNumber nozzleNumber)//11点运动过程
        {
            //解析josn中拍照运动坐标
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "NozzleCalib.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            DownCameramoveAxisNozzle = obj["DownCameraMoveAxisCalibposition"]?[nozzleNumber.ToString()] as JArray;//获取轴Nozzle 数组 

            if (DownCameramoveAxisNozzle == null && DownCameramoveAxisNozzle.Count == 0)
            {
                Console.WriteLine($"{nozzleNumber.ToString()}不存在或为空");
                MessageBox.Show($"{nozzleNumber.ToString()}不存在或为空");
                return false;
            }

            for (int i = 0; i < DownCameramoveAxisNozzle.Count; i++)
            {
                //移动到拍照位
                MoveAxis(i, nozzleNumber);

                Thread.Sleep(1000);

                var PointArray = DownCameramoveAxisNozzle[i] as JArray;//第几个移动点
                if (PointArray == null)
                {
                    MessageBox.Show($"{PointArray}点位空");
                    return false;
                }
                double[] values = PointArray.Select(x => (double)x).ToArray();//x,y,z,r
                string Data = $"{values[0]},{values[1]},{values[3]}";//x,y,r     
                //发送标定指令
                switch (nozzleNumber)
                {
                    case NozzleNumber.Nozzle1:
                        {
                            //联合标定 
                            if (!CamerCombinePhotoCalib(CombineCalibProcess.Combineongoing, Data))
                            {
                                return false;
                            }
                        }
                        break;
                    case NozzleNumber.Nozzle2:
                        {
                            //单独标定
                            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, nozzleNumber, Data))
                            {
                                return false;
                            }
                            break;
                        }
                    case NozzleNumber.Nozzle3:
                        {
                            //单独标定
                            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, nozzleNumber, Data))
                            {
                                return false;
                            }
                        }
                        break;
                    case NozzleNumber.Nozzle4:
                        {
                            //单独标定
                            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, nozzleNumber, Data))
                            {
                                return false;
                            }
                        }
                        break;
                    default:
                        {
                            ////单独标定
                            //if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, nozzleNumber, Data))
                            //{
                            //    return false;
                            //}
                        }
                        break;
                }
            }
            return true;
        }

        private void MoveAxis(int Index, NozzleNumber nozzleNumber)//移动轴到目标点位
        {
            var PointArray = DownCameramoveAxisNozzle[Index] as JArray;//第几个移动点
            double[] values = PointArray.Select(x => (double)x).ToArray();//x,y,z,r

            if (PointArray == null)
            {
                MessageBox.Show($"{PointArray}点位空");
                return;
            }
            MoveAxisDirectControl(values[0], values[1], values[2], values[3], nozzleNumber);
        }

        private bool CamerCombinePhotoCalib(CombineCalibProcess combineCalibProcess, string Data)//联合标定拍照判断
        {
            CalibCommunicationProcess.SendCombineCalibration(combineCalibProcess, Data);
            if (CalibCommunicationProcess.CalibStatus())
            {
                return true;
            }
            return false;
        }

        private bool CamerAlonePhotoCalib(DownCamreaAloneCalibProcess downCamreaAloneCalibProcess, NozzleNumber nozzleNumber,string Data)//11点标定拍照判断
        {
            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    CalibCommunicationProcess.SendAloneCalib(downCamreaAloneCalibProcess, DownCamreaNozzleCalibNumber.C2,Data);
                    break;
                case NozzleNumber.Nozzle2:
                    CalibCommunicationProcess.SendAloneCalib(downCamreaAloneCalibProcess, DownCamreaNozzleCalibNumber.C7, Data);
                    break;
                case NozzleNumber.Nozzle3:
                    CalibCommunicationProcess.SendAloneCalib(downCamreaAloneCalibProcess, DownCamreaNozzleCalibNumber.C8, Data);
                    break;
                case NozzleNumber.Nozzle4:
                    CalibCommunicationProcess.SendAloneCalib(downCamreaAloneCalibProcess, DownCamreaNozzleCalibNumber.C9, Data);
                    break;
                default:
                    break;
            }
            if (CalibCommunicationProcess.CalibStatus())
            {
                return true;
            }
            return false;
        }

        private bool MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess downCamreaAloneCalibProcess, MovingCameraCalibposition movingCameraCalibposition,string Data)//9点标定拍照判断
        {
            switch (movingCameraCalibposition)
            {
                case MovingCameraCalibposition.FeedDischarging:
                    CalibCommunicationProcess.SendMoveCameraCalib(downCamreaAloneCalibProcess, MoveCameraCalibPositionNumber.C5,Data);
                    break;
                case MovingCameraCalibposition.Vehicles:
                    CalibCommunicationProcess.SendMoveCameraCalib(downCamreaAloneCalibProcess, MoveCameraCalibPositionNumber.C4,Data);
                    break;
                default:
                    break;
            }
            if (CalibCommunicationProcess.CalibStatus())
            {
                return true;
            }
            return false;
        }

        private void NozzleInhale(NozzleNumber nozzleNumber)//吸嘴吸气
        {
            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    {
                        //Nozzle1吸嘴吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 1);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    }
                    break;
                case NozzleNumber.Nozzle2:
                    {
                        //Nozzle2吸嘴吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 1);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
                    }
                    break;
                case NozzleNumber.Nozzle3:
                    {
                        //Nozzle3吸嘴吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 1);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
                    }
                    break;
                case NozzleNumber.Nozzle4:
                    {
                        //Nozzle4吸嘴吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 1);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
                    }
                    break;
                default:
                    break;
            }
        }

        private void NozzleStopInhale(NozzleNumber nozzleNumber)//吸嘴停止吸气
        {
            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    {
                        //Nozzle1吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 1);
                    }
                    break;
                case NozzleNumber.Nozzle2:
                    {
                        //Nozzle2吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 1);
                    }
                    break;
                case NozzleNumber.Nozzle3:
                    {
                        //Nozzle3吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 1);
                    }
                    break;
                case NozzleNumber.Nozzle4:
                    {
                        //Nozzle4吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 1);
                    }
                    break;
                default:
                    break;
            }
        }

        private void MoveAxisDirectControl(double x, double y, double z, double r, NozzleNumber nozzleNumber)//直接控制轴运动，及到位判断
        {
            AkrAction.Current.Move(GlobalManager.AxisName.FSX,x, (int)AxisSpeed.FSX);//x轴运动x
            AkrAction.Current.Move(GlobalManager.AxisName.FSY,y , (int)AxisSpeed.FSY);//y轴运动y

            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    {
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK1_Z, z, (int)AxisSpeed.PICK1_Z); //z轴运动z
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK1_T, r, (int)AxisSpeed.PICK1_T); //r轴运动r
                    }
                    break;
                case NozzleNumber.Nozzle2:
                    {
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK2_Z, z, (int)AxisSpeed.PICK2_Z); //z轴运动z
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK2_T, r, (int)AxisSpeed.PICK2_T); //r轴运动r

                    }
                    break;
                case NozzleNumber.Nozzle3:
                    {
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK3_Z, z, (int)AxisSpeed.PICK3_Z); //z轴运动z
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK3_T, r, (int)AxisSpeed.PICK3_T); //r轴运动r

                    }
                    break;
                case NozzleNumber.Nozzle4:
                    {
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK4_Z, z, (int)AxisSpeed.PICK4_Z); //z轴运动z
                        AkrAction.Current.Move(GlobalManager.AxisName.PICK4_T, r, (int)AxisSpeed.PICK4_T); //r轴运动r

                    }
                    break;
                default:
                    break;
            }




        }
    }
















}
