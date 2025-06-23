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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static AkribisFAM.CommunicationProtocol.Task_TTNCamreaFunction;
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

        public int AllCalibrationFinished()
        {
            CalibCommunicationProcess.CalibPushcommand($"UN,1" + "\r\n");
            return 0;
        }

        public int ReCheckCalibration()
        {
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 0);
            CalibCommunicationProcess.CalibPushcommand($"CB,6" + "\r\n");
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig, 1);
            return 0;
        }


        private JArray DownCameramoveAxisNozzle;//下相机机构动作执行点位

        public StationPoints CalibrationPoints = new StationPoints();
        public async Task CombineCalibrationprocess()//相机联合标定过程
        {
            await Task.Run(new Action(() =>
            {
                AkrAction.Current.MoveFoamZ1(0); //z轴运动z
                AkrAction.Current.MoveFoamZ2(-1); //z轴运动z
                //AkrAction.Current.Move(GlobalManager.AxisName.PICK3_Z, -1, (int)AxisSpeed.PICK3_Z); //z轴运动z
                //AkrAction.Current.Move(GlobalManager.AxisName.PICK4_Z, -1, (int)AxisSpeed.PICK4_Z); //z轴运动z

                //移动到载具标定拍照位
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[11].X, CalibrationPoints.ZuZhuangPointList[11].Y, CalibrationPoints.ZuZhuangPointList[11].Z, CalibrationPoints.ZuZhuangPointList[11].R, NozzleNumber.Nozzle1);

                //标定开始
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinestart, "CombineStart"))
                {
                    return;
                }

                Thread.Sleep(800);//延时
                //取标定片指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinepick, $"{CalibrationPoints.ZuZhuangPointList[13].X.ToString()},{CalibrationPoints.ZuZhuangPointList[13].Y.ToString()},{CalibrationPoints.ZuZhuangPointList[13].R.ToString()}"))
                {
                    return;
                }

                //关联取指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.CombineRelationpick, $"{CalibrationPoints.ZuZhuangPointList[11].X.ToString()},{CalibrationPoints.ZuZhuangPointList[11].Y.ToString()},0"))
                {
                    return;
                }
                //移动到取标定片位XY
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[12].X, CalibrationPoints.ZuZhuangPointList[12].Y, CalibrationPoints.ZuZhuangPointList[12].Z, CalibrationPoints.ZuZhuangPointList[12].R, NozzleNumber.Nozzle1);

                //移动到取标定片位
                //
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[13].X, CalibrationPoints.ZuZhuangPointList[13].Y, CalibrationPoints.ZuZhuangPointList[13].Z, CalibrationPoints.ZuZhuangPointList[13].R, NozzleNumber.Nozzle1);

                Thread.Sleep(1000);//延时
                                   //吸嘴吸气
                NozzleInhale(NozzleNumber.Nozzle1);
                Thread.Sleep(1000);//延时

                //移动到取标定片位XY
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[12].X, CalibrationPoints.ZuZhuangPointList[12].Y, CalibrationPoints.ZuZhuangPointList[12].Z, CalibrationPoints.ZuZhuangPointList[12].R, NozzleNumber.Nozzle1);

                //移动到11点位置起点XY
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[14].X, CalibrationPoints.ZuZhuangPointList[14].Y, CalibrationPoints.ZuZhuangPointList[14].Z, CalibrationPoints.ZuZhuangPointList[14].R, NozzleNumber.Nozzle1);

                //进入11点位置循环
                if (!Point11Calibongoing(NozzleNumber.Nozzle1))
                {
                    return;
                }

                //移动到11点位置起点XY
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[14].X, CalibrationPoints.ZuZhuangPointList[14].Y, CalibrationPoints.ZuZhuangPointList[14].Z, CalibrationPoints.ZuZhuangPointList[14].R, NozzleNumber.Nozzle1);

                //移动到放标定片位XY
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[15].X, CalibrationPoints.ZuZhuangPointList[15].Y, CalibrationPoints.ZuZhuangPointList[15].Z, CalibrationPoints.ZuZhuangPointList[15].R, NozzleNumber.Nozzle1);

                //移动到放标定片位XYZ
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[16].X, CalibrationPoints.ZuZhuangPointList[16].Y, CalibrationPoints.ZuZhuangPointList[16].Z, CalibrationPoints.ZuZhuangPointList[16].R, NozzleNumber.Nozzle1);

                Thread.Sleep(800);
                //吸嘴停止吸气
                NozzleStopInhale1(NozzleNumber.Nozzle1);
                Thread.Sleep(10);
                NozzleStopInhale(NozzleNumber.Nozzle1);
                Thread.Sleep(10);
                NozzleStopInhale1(NozzleNumber.Nozzle1);

                Thread.Sleep(500);
                //移动到放标定片位XY
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[15].X, CalibrationPoints.ZuZhuangPointList[15].Y, CalibrationPoints.ZuZhuangPointList[15].Z, CalibrationPoints.ZuZhuangPointList[15].R, NozzleNumber.Nozzle1);

                //移动到飞达标定拍照位
                MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[17].X, CalibrationPoints.ZuZhuangPointList[17].Y, CalibrationPoints.ZuZhuangPointList[17].Z, CalibrationPoints.ZuZhuangPointList[17].R, NozzleNumber.Nozzle1);

                Thread.Sleep(1000);
                //放标定片指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combineput, $"{CalibrationPoints.ZuZhuangPointList[16].X.ToString()},{CalibrationPoints.ZuZhuangPointList[16].Y.ToString()},{CalibrationPoints.ZuZhuangPointList[16].R.ToString()}"))
                {
                    return;
                }

                //Thread.Sleep(500);
                //关联放指令发送
                if (!CamerCombinePhotoCalib(CombineCalibProcess.CombineRelationput, $"{CalibrationPoints.ZuZhuangPointList[17].X.ToString()},{CalibrationPoints.ZuZhuangPointList[17].Y.ToString()},0"))
                {
                    return;
                }
                //标定结束等待结果
                if (!CamerCombinePhotoCalib(CombineCalibProcess.Combinecalibend, "calibend"))
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
                //AkrAction.Current.MoveFoamZ1( 0); //z轴运动z
                //AkrAction.Current.MoveFoamZ2( 0); //z轴运动z
                //AkrAction.Current.Move(GlobalManager.AxisName.PICK3_Z, 0, (int)AxisSpeed.PICK3_Z); //z轴运动z
                //AkrAction.Current.Move(GlobalManager.AxisName.PICK4_Z, 0, (int)AxisSpeed.PICK4_Z); //z轴运动z



                //吸嘴吸气
                //NozzleInhale(nozzleNumber);
                //标定开始
                if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.start, nozzleNumber, "Alonestart"))
                {
                    return;
                }
                //进入11点位置循环
                if (!Point11Calibongoing(nozzleNumber))
                {
                    return;
                }
                //标定结束等待结果
                if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.calibend, nozzleNumber, "Aloneend"))
                {
                    return;
                }
                //吸嘴停止吸气
                // NozzleStopInhale(nozzleNumber);
                //此次标定结束
                CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.processend, nozzleNumber, "processend");
            }));
        }

        public async Task Point9Calibprocess(MovingCameraCalibposition movingCameraCalibposition)//九点相机标定吸嘴过程
        {
            await Task.Run(new Action(() =>
            {
                AkrAction.Current.MoveFoamZ1(0); //z轴运动z

                //标定开始
                if (!MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess.start, movingCameraCalibposition, "Movestart"))
                {
                    return;
                }

                //九点运动过程
                Func<MovingCameraCalibposition, bool> Point9Calibongoing = e =>
                {

                    for (int i = 0; i < 9; i++)
                    {
                        //移动到拍照位
                        int startidx = (int)movingCameraCalibposition * 9 + i;
                        MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[startidx].X, CalibrationPoints.ZuZhuangPointList[startidx].Y, CalibrationPoints.ZuZhuangPointList[startidx].Z, CalibrationPoints.ZuZhuangPointList[startidx].R, NozzleNumber.Nozzle2);//x,y,z,r


                        Thread.Sleep(800);

                        //单独九点标定
                        if (!MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, movingCameraCalibposition, $"{CalibrationPoints.ZuZhuangPointList[startidx].X.ToString()},{CalibrationPoints.ZuZhuangPointList[startidx].Y.ToString()},0"))
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
                MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess.processend, movingCameraCalibposition, "MoveprocessEnd");
            }));
        }

        private bool Point11Calibongoing(NozzleNumber nozzleNumber)//11 calibration process
        {

            for (int i = 0; i < 11; i++)
            {
                MoveAxis(i, nozzleNumber);

                Thread.Sleep(1000);

                int startidx = ((int)nozzleNumber - 1) * 11 + i;//for 11 calibration, becuase it does not have nozzle 1
                if (startidx < 0)
                {
                    startidx = ((int)nozzleNumber) * 11 + i;//for common calibration
                }
                string Data = $"{CalibrationPoints.ZuZhuangPointList[startidx].X},{CalibrationPoints.ZuZhuangPointList[startidx].Y},{CalibrationPoints.ZuZhuangPointList[startidx].R}";//x,y,r     
                //发送标定指令
                switch (nozzleNumber)
                {
                    case NozzleNumber.Nozzle1:
                        {
                            //joint calib
                            if (!CamerCombinePhotoCalib(CombineCalibProcess.Combineongoing, Data))
                            {
                                return false;
                            }
                        }
                        break;
                    case NozzleNumber.Nozzle2:
                        {
                            //11 points alone
                            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, nozzleNumber, Data))
                            {
                                return false;
                            }
                            break;
                        }
                    case NozzleNumber.Nozzle3:
                        {
                            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, nozzleNumber, Data))
                            {
                                return false;
                            }
                        }
                        break;
                    case NozzleNumber.Nozzle4:
                        {
                            if (!CamerAlonePhotoCalib(DownCamreaAloneCalibProcess.Ongoing, nozzleNumber, Data))
                            {
                                return false;
                            }
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
            return true;
        }

        private void MoveAxis(int Index, NozzleNumber nozzleNumber)//移动轴到目标点位 move to target position
        {

            int startidx = ((int)nozzleNumber - 1) * 11 + Index;//for 11 calibration, becuase it does not have nozzle 1
            if (startidx < 0)
            {
                startidx = ((int)nozzleNumber) * 11 + Index;//for common calibration
            }
            MoveAxisDirectControl(CalibrationPoints.ZuZhuangPointList[startidx].X, CalibrationPoints.ZuZhuangPointList[startidx].Y, CalibrationPoints.ZuZhuangPointList[startidx].Z, CalibrationPoints.ZuZhuangPointList[startidx].R, nozzleNumber);
        }

        private bool CamerCombinePhotoCalib(CombineCalibProcess combineCalibProcess, string Data)//joint calibration judgement
        {
            CalibCommunicationProcess.SendCombineCalibration(combineCalibProcess, Data);
            if (CalibCommunicationProcess.CalibStatus())
            {
                return true;
            }
            return false;
        }

        private bool CamerAlonePhotoCalib(DownCamreaAloneCalibProcess downCamreaAloneCalibProcess, NozzleNumber nozzleNumber, string Data)//11点标定拍照判断
        {
            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    CalibCommunicationProcess.SendAloneCalib(downCamreaAloneCalibProcess, DownCamreaNozzleCalibNumber.C2, Data);
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

        private bool MoveCamerAlonePhotoCalib(DownCamreaAloneCalibProcess downCamreaAloneCalibProcess, MovingCameraCalibposition movingCameraCalibposition, string Data)//9点标定拍照判断
        {
            switch (movingCameraCalibposition)
            {
                case MovingCameraCalibposition.FeedDischarging:
                    CalibCommunicationProcess.SendMoveCameraCalib(downCamreaAloneCalibProcess, MoveCameraCalibPositionNumber.C5, Data);
                    break;
                case MovingCameraCalibposition.Vehicles:
                    CalibCommunicationProcess.SendMoveCameraCalib(downCamreaAloneCalibProcess, MoveCameraCalibPositionNumber.C4, Data);
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

        private void NozzleStopInhale1(NozzleNumber nozzleNumber)//吸嘴停止吸气
        {
            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    {
                        //Nozzle1吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release, 0);
                    }
                    break;
                case NozzleNumber.Nozzle2:
                    {
                        //Nozzle2吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release, 0);
                    }
                    break;
                case NozzleNumber.Nozzle3:
                    {
                        //Nozzle3吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release, 0);
                    }
                    break;
                case NozzleNumber.Nozzle4:
                    {
                        //Nozzle4吸嘴停止吸气
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply, 0);
                        IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release, 0);
                    }
                    break;
                default:
                    break;
            }
        }
        private void MoveAxisDirectControl(double x, double y, double z, double r, NozzleNumber nozzleNumber)//直接控制轴运动，及到位判断
        {
            AkrAction.Current.MoveFoamXY(x, y);//x轴运动x

            switch (nozzleNumber)
            {
                case NozzleNumber.Nozzle1:
                    {
                        AkrAction.Current.MoveFoamT1(r); //r轴运动r
                        AkrAction.Current.MoveFoamZ1(z); //z轴运动z
                    }
                    break;
                case NozzleNumber.Nozzle2:
                    {
                        AkrAction.Current.MoveFoamT2(r); //r轴运动r
                        AkrAction.Current.MoveFoamZ2(z); //z轴运动z

                    }
                    break;
                case NozzleNumber.Nozzle3:
                    {
                        AkrAction.Current.MoveFoamT3(r); //r轴运动r
                        AkrAction.Current.MoveFoamZ3(z); //z轴运动z

                    }
                    break;
                case NozzleNumber.Nozzle4:
                    {
                        AkrAction.Current.MoveFoamT4(r); //r轴运动r
                        AkrAction.Current.MoveFoamZ4(z); //z轴运动z

                    }
                    break;
                default:
                    break;
            }




        }

        public bool TrainNozzle(int pickernum)
        {
            //抬起Z轴
            //lift up pickerZ
            if (pickernum == 0)
            {
                AkrAction.Current.MoveFoamZ1(0);
            }
            else if (pickernum == 1)
            {
                AkrAction.Current.MoveFoamZ2(0);
            }
            else if (pickernum == 2)
            {
                AkrAction.Current.MoveFoamZ3(0);
            }
            else if (pickernum == 3)
            {
                AkrAction.Current.MoveFoamZ4(0);
            }
            //移动到飞拍起始位置
            //move to start position of fly photograph
            AkrAction.Current.MoveLaserXY(CalibrationPoints.ZuZhuangPointList[4].X, CalibrationPoints.ZuZhuangPointList[4].Y);
            if (pickernum == 0)
            {
                AkrAction.Current.MoveFoamZ1(CalibrationPoints.ZuZhuangPointList[4].Z);
                AkrAction.Current.MoveFoamT1(CalibrationPoints.ZuZhuangPointList[4].R);
            }
            else if (pickernum == 1)
            {
                AkrAction.Current.MoveFoamZ2(CalibrationPoints.ZuZhuangPointList[4].Z);
                AkrAction.Current.MoveFoamT2(CalibrationPoints.ZuZhuangPointList[4].R);
            }
            else if (pickernum == 2)
            {
                AkrAction.Current.MoveFoamZ3(CalibrationPoints.ZuZhuangPointList[4].Z);
                AkrAction.Current.MoveFoamT3(CalibrationPoints.ZuZhuangPointList[4].R);
            }
            else if (pickernum == 3)
            {
                AkrAction.Current.MoveFoamZ4(CalibrationPoints.ZuZhuangPointList[4].Z);
                AkrAction.Current.MoveFoamT4(CalibrationPoints.ZuZhuangPointList[4].R);
            }
            //给Cognex发拍照信息
            //send Cognex message
            string command = "SN" + "123456," + $"{pickernum + 1}," + "Foam," + $"{CalibrationPoints.ZuZhuangPointList[pickernum].X},{CalibrationPoints.ZuZhuangPointList[pickernum].Y},{CalibrationPoints.ZuZhuangPointList[pickernum].R}";
            TriggTTNCamreaSendData(TTNProcessCommand.TTN, command);
            Thread.Sleep(100);
            int cnt = 0;
            //wait OK or tinme out 10s
            while (true)
            {
                if (TriggTTNCamreaready() == "OK" || cnt == 200)
                {
                    break;
                }
                cnt++;
                Thread.Sleep(50);
            }

            //飞拍移动到结束位置
            //set fly photograph event , the third param is event number which should check PCsuite. Now event 2 is CCD2.
            //We prefer to use IO trigger, so that we can avoid using agito to trigger. Just move to the position of photograph and trigger IO.
            AkrAction.Current.SetSingleEvent(AxisName.FSX, CalibrationPoints.ZuZhuangPointList[pickernum].X, 2);
            //start move
            AkrAction.Current.MoveLaserXY(CalibrationPoints.ZuZhuangPointList[pickernum].X, CalibrationPoints.ZuZhuangPointList[pickernum].Y);

            //接受Cognex结果
            //get Cognex result
            string Errcode = TriggTTNCamreaAcceptData(TTNProcessCommand.TTN)[0].Errcode1;
            if (Errcode != "1")
            {
                return false;
            }
            //抬起Z轴
            //lift up pickerZ
            if (pickernum == 0)
            {
                AkrAction.Current.MoveFoamZ1(0);
            }
            else if (pickernum == 1)
            {
                AkrAction.Current.MoveFoamZ2(0);
            }
            else if (pickernum == 2)
            {
                AkrAction.Current.MoveFoamZ3(0);
            }
            else if (pickernum == 3)
            {
                AkrAction.Current.MoveFoamZ4(0);
            }
            return true;
        }

        public async Task TrainNozzles(int nozzlenum)
        {
            Task<bool> task = new Task<bool>(() =>
            {
                bool ret = TrainNozzle(nozzlenum);

                return true;
            });
            task.Start();
            await task;
        }
    }
















}
