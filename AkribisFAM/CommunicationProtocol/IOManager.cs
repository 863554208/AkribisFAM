using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using HslCommunication.ModBus;
using Newtonsoft.Json.Linq;

namespace AkribisFAM.CommunicationProtocol
{
    enum IO_OutFunction_Table
    {
       OUT1_0Left_1_lift_cylinder_extend,
        OUT1_1Left_1_lift_cylinder_retract,
        OUT1_2Right_1_lift_cylinder_extend,
        OUT1_3Right_1_lift_cylinder_retract,
        OUT1_4Left_2_lift_cylinder_extend,
        OUT1_5Left_2_lift_cylinder_retract,
        OUT1_6Right_2_lift_cylinder_extend,
        OUT1_7Right_2_lift_cylinder_retract,
        OUT1_8Left_3_lift_cylinder_extend,
        OUT1_9Left_3_lift_cylinder_retract,
        OUT1_10Right_3_lift_cylinder_extend,
        OUT1_11Right_3_lift_cylinder_retract,
        OUT1_124_lift_cylinder_extend,
        OUT1_134_lift_cylinder_retract,
        OUT1_14Reserve,
        OUT1_15Reserve,

        OUT2_0Stopping_Cylinder1_extend,
        OUT2_1Stopping_Cylinder1_retract,
        OUT2_2Stopping_Cylinder2_extend,
        OUT2_3Stopping_Cylinder2_retract,
        OUT2_4Stopping_Cylinder3_extend,
        OUT2_5Stopping_Cylinder3_retract,
        OUT2_6Stopping_Cylinder4_extend,
        OUT2_7Stopping_Cylinder4_retract,
        OUT2_8LOCK1,
        OUT2_9LOCK2,
        OUT2_10LOCK3,
        OUT2_11LOCK4,
        OUT2_12Reserve,
        OUT2_13Reserve,
        OUT2_14Reserve,
        OUT2_15FFU,

        OUT3_0PNP_Gantry_vacuum1_Supply,
        OUT3_1PNP_Gantry_vacuum1_Release,
        OUT3_2PNP_Gantry_vacuum2_Supply,
        OUT3_3PNP_Gantry_vacuum2_Release,
        OUT3_4PNP_Gantry_vacuum3_Supply,
        OUT3_5PNP_Gantry_vacuum3_Release,
        OUT3_6PNP_Gantry_vacuum4_Supply,
        OUT3_7PNP_Gantry_vacuum4_Release,
        OUT3_8solenoid_valve1_A,
        OUT3_9solenoid_valve1_B,
        OUT3_10solenoid_valve2_A,
        OUT3_11solenoid_valve2_B,
        OUT3_12solenoid_valve3_A,
        OUT3_13solenoid_valve3_B,
        OUT3_14solenoid_valve4_A,
        OUT3_15solenoid_valve4_B,

        OUT4_0Pneumatic_Claw_A,
        OUT4_1Pneumatic_Claw_B,
        OUT4_2Peeling_Recheck_vacuum1_Supply,
        OUT4_3Peeling_Recheck_vacuum1_Release,
        OUT4_4Reserve,
        OUT4_5Reserve,
        OUT4_6Reserve,
        OUT4_7Reserve,
        OUT4_8Stop_feeder1,
        OUT4_9Run_feeder1,
        OUT4_10initialize_feeder1,
        OUT4_11Backup,
        OUT4_12Stop_feeder2,
        OUT4_13Run_feeder2,
        OUT4_14initialize_feeder2,
        OUT4_15Backup
    }

    enum IO_INFunction_Table
    {
        IN1_0Acceleration_Sign1,
        IN1_1Reserve,
        IN1_2Reserve,
        IN1_3Reserve,
        IN1_4Stop_Sign1,
        IN1_5Stop_Sign2,
        IN1_6Stop_Sign3,
        IN1_7Stop_Sign4,
        IN1_8NG_cover_plate1,
        IN1_9NG_cover_plate2,
        IN1_10plate_has_left_Behind_the_stopping_cylinder1,
        IN1_11plate_has_left_Behind_the_stopping_cylinder2,
        IN1_12bord_lift_in_position1,
        IN1_13bord_lift_in_position2,
        IN1_14bord_lift_in_position3,
        IN1_15bord_lift_in_position4
    }

    class IOManager
    {
        private Dictionary<IO_OutFunction_Table, int> IO_OutFunctionnames = new Dictionary<IO_OutFunction_Table, int>();
        private Dictionary<IO_INFunction_Table, int> IO_INFunctionnames = new Dictionary<IO_INFunction_Table, int>();
        public bool[] OutIO_status = new bool[100];
        public bool[] INIO_status = new bool[100];

        private static IOManager _instance;
        private IOManager()
        {
            LoadIOPoint();
        }

        public static IOManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IOManager();
                }
                return _instance;
            }
        }

        private void LoadIOPoint()
        {
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "IOPoint.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            foreach (string name in Enum.GetNames(typeof(IO_OutFunction_Table)))
            {
                if (Enum.TryParse<IO_OutFunction_Table>(name, out var func))
                {
                    short value = obj["Out"][name].Value<short>();
                    IO_OutFunctionnames.Add(func, value);
                }
                else
                {
                    Console.WriteLine("转换失败：不是有效的枚举名");
                }
            }

            foreach (string name in Enum.GetNames(typeof(IO_INFunction_Table)))
            {
                if (Enum.TryParse<IO_INFunction_Table>(name, out var func))
                {
                    short value = obj["IN"][name].Value<short>();
                    IO_INFunctionnames.Add(func, value);
                }
                else
                {
                    Console.WriteLine("转换失败：不是有效的枚举名");
                }
            }
        }

        public void ReadIO_status()
        {
            //foreach (var IOname in IO_OutFunctionnames)
            //{
            //    var IOnamekey = IOname.Key;
            //    var IOnamevalue = IOname.Value;
            //    bool results = ModbusTCPWorker.GetInstance().Read_Coil(520);

            //    //OutIO_status[(int)IOnamekey] = ModbusTCPWorker.GetInstance().Read_Coil(IOname.Value);
            //}
            //循环读取输出IO
            Task.Run(new Action(() =>
            {
                Thread.CurrentThread.Name = "OutIO_statusThread";

                while (true)
                {
                    foreach (var IOname in IO_OutFunctionnames)
                    {
                        var IOnamekey = IOname.Key;
                        var IOnamevalue = IOname.Value;
                        OutIO_status[(int)IOnamekey] = ModbusTCPWorker.GetInstance().Read_Coil(IOname.Value);
                    }
                    Thread.Sleep(50);
                }
            }));
            //循环读取输入IO
            Task.Run(new Action(() =>
            {
                while (true)
                {
                    foreach (var IOname in IO_INFunctionnames)
                    {
                        var IOnamekey = IOname.Key;
                        var IOnamevalue = IOname.Value;
                        INIO_status[(int)IOnamekey] = ModbusTCPWorker.GetInstance().Read_Coil(IOname.Value);
                    }
                    Thread.Sleep(50);
                }
            }));
        }

        public bool IO_ControlStatus(IO_OutFunction_Table iO_OutFunction_Table, int writestatus)
        {
            if (writestatus == 1)
            {
                if (!OutIO_status[(int)iO_OutFunction_Table])//写IO状态为True
                {
                    bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil(IO_OutFunctionnames[iO_OutFunction_Table], true);
                    if (!Sucessstatus)
                    {
                        return false;
                    }

                    OutIO_status[(int)iO_OutFunction_Table] = true;
                    return true;
                }
                return true;
            }

            if (writestatus == 0)
            {
                if (OutIO_status[(int)iO_OutFunction_Table])//写IO状态为False
                {
                    bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil(IO_OutFunctionnames[iO_OutFunction_Table], false);
                    if (!Sucessstatus)
                    {
                        return false;
                    }
                    OutIO_status[(int)iO_OutFunction_Table] = false;
                    return true;
                }
                return true;
            }
            return false;
        }


        //public bool WriteIO_Truestatus(IO_OutFunction_Table iO_OutFunction_Table)//写IO状态为True
        //{
        //    if (!OutIO_status[(int)iO_OutFunction_Table])
        //    {
        //        bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil(IO_OutFunctionnames[iO_OutFunction_Table], true);
        //        if (!Sucessstatus)
        //        {
        //            return false;
        //        }

        //        OutIO_status[(int)iO_OutFunction_Table] = true;
        //        return true;
        //    }
        //    return true;
        //    //Console.WriteLine(  
        //}

        //public bool WriteIO_Falsestatus(IO_OutFunction_Table iO_OutFunction_Table)//写IO状态为False
        //{
        //    if (OutIO_status[(int)iO_OutFunction_Table])
        //    {
        //        bool Sucessstatus = ModbusTCPWorker.GetInstance().Write_Coil(IO_OutFunctionnames[iO_OutFunction_Table], false);
        //        if (!Sucessstatus)
        //        {
        //            return false;
        //        }
        //        OutIO_status[(int)iO_OutFunction_Table] = false;
        //        return true;
        //    }
        //    return true;
        //    //Console.WriteLine(  
        //}
    }
}