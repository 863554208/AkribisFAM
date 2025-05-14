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
        OUT1_0Left_1_lift_cylinder_extend = 0,
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
        OUT1_12_lift4_cylinder_extend,
        OUT1_13_lift4_cylinder_retract,
        OUT1_14backup,
        OUT1_15backup,

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
        OUT2_12backup,
        OUT2_13backup,
        OUT2_14backup,
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
        OUT4_4backup,
        OUT4_5backup,
        OUT4_6backup,
        OUT4_7backup,
        OUT4_8Stop_feeder1,
        OUT4_9Run_feeder1,
        OUT4_10initialize_feeder1,
        OUT4_11Backup,
        OUT4_12Stop_feeder2,
        OUT4_13Run_feeder2,
        OUT4_14initialize_feeder2,
        OUT4_15Backup,

        OUT5_0Feeder1_limit_cylinder_extend,
        OUT5_1Feeder1_limit_cylinder_retract,
        OUT5_2Feeder2_limit_cylinder_extend,
        OUT5_3Feeder2_limit_cylinder_retract,
        OUT5_4Backup,
        OUT5_5backup,
        OUT5_6backup,
        OUT5_7backup,
        OUT5_8Feeder_vacuum1_Supply,
        OUT5_9Feeder_vacuum1_Release,
        OUT5_10Feeder_vacuum2_Supply,
        OUT5_11Feeder_vacuum2_Release,
        OUT5_12Feeder_vacuum3_Supply,
        OUT5_13Feeder_vacuum3_Release,
        OUT5_14Feeder_vacuum4_Supply,
        OUT5_15Feeder_vacuum4_Release,


        OUT6_0Tri_color_light_red,
        OUT6_1Tri_color_light_yellow,
        OUT6_2Tri_color_light_green,
        OUT6_3light1,
        OUT6_4light2,
        OUT6_5Buzzer,
        OUT6_6backup,
        OUT6_7backup,
        OUT6_8Run_light,
        OUT6_9Stop_light,
        OUT6_10Feeder1_light,
        OUT6_11Feeder2_light,
        OUT6_12Reset_light,
        OUT6_13backup,
        OUT6_14backup,
        OUT6_15backup,

        OUT7_0MACHINE_READY_TO_RECEIVE,
        OUT7_1BOARD_AVAILABLE,
        OUT7_2FAILED_BOARD_AVAILABLE_OPTIONAL,
        OUT7_3backup,
        OUT7_4backup,
        OUT7_5backup,
        OUT7_6backup,
        OUT7_7backup,
        OUT7_8backup,
        OUT7_9backup,
        OUT7_10backup,
        OUT7_11backup,
        OUT7_12backup,
        OUT7_13backup,
        OUT7_14backup,
        OUT7_15backup

    }

    public enum IO_INFunction_Table
    {
        IN1_0Slowdown_Sign1 = 0,
        IN1_1Slowdown_Sign2,
        IN1_2Slowdown_Sign3,
        IN1_3Slowdown_Sign4,
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
        IN1_15bord_lift_in_position4,

        IN2_0Left_1_lift_cylinder_Extend_InPos,
        IN2_1Left_1_lift_cylinder_retract_InPos,
        IN2_2Right_1_lift_cylinder_Extend_InPos,
        IN2_3Right_1_lift_cylinder_retract_InPos,
        IN2_4Left_2_lift_cylinder_Extend_InPos,
        IN2_5Left_2_lift_cylinder_retract_InPos,
        IN2_6Right_2_lift_cylinder_Extend_InPos,
        IN2_7Right_2_lift_cylinder_retract_InPos,
        IN2_8Left_3_lift_cylinder_Extend_InPos,
        IN2_9Left_3_lift_cylinder_retract_InPos,
        IN2_10Right_3_lift_cylinder_Extend_InPos,
        IN2_11Right_3_lift_cylinder_retract_InPos,
        IN2_12_lift4_cylinder_Extend_InPos,
        IN2_13_lift4_cylinder_retract_InPos,
        IN2_14backup,
        IN2_15backup,


        IN3_0Stopping_cylinder_1_extend_InPos,
        IN3_1Stopping_cylinder_1_react_InPos,
        IN3_2Stopping_cylinder_2_extend_InPos,
        IN3_3Stopping_cylinder_2_react_InPos,
        IN3_4Stopping_cylinder_3_extend_InPos,
        IN3_5Stopping_cylinder_3_react_InPos,
        IN3_6Stopping_cylinder_4_extend_InPos,
        IN3_7Stopping_cylinder_4_react_InPos,
        IN3_8backup,
        IN3_9Claw_extend_in_position,
        IN3_10Claw_retract_in_position,
        IN3_11Peeling_Recheck_vacuum1_Pressure_feedback,
        IN3_12PNP_Gantry_vacuum1_Pressure_feedback,
        IN3_13PNP_Gantry_vacuum2_Pressure_feedback,
        IN3_14PNP_Gantry_vacuum3_Pressure_feedback,
        IN3_15PNP_Gantry_vacuum4_Pressure_feedback,

        IN4_0Backup_Platform_2_has_label_feeder1,
        IN4_1Platform_has_label_feeder1,
        IN4_2Alarm_feeder1,
        IN4_3Initialized_feeder1,
        IN4_4Backup_Platform_2_has_label_feeder2,
        IN4_5Platform_has_label_feeder2,
        IN4_6Alarm_feeder2,
        IN4_7Initialized_feeder2,
        IN4_8Feeder1_limit_cylinder_extend_InPos,
        IN4_9Feeder1_limit_cylinder_retract_InPos,
        IN4_10Feeder2_limit_cylinder_extend_InPos,
        IN4_11Feeder2_limit_cylinder_retract_InPos,
        IN4_12Backup,
        IN4_13backup,
        IN4_14backup,
        IN4_15backup,

        IN5_0Feeder_vacuum1_Pressure_feedback,
        IN5_1Feeder_vacuum2_Pressure_feedback,
        IN5_2Feeder_vacuum3_Pressure_feedback,
        IN5_3Feeder_vacuum4_Pressure_feedback,
        IN5_4Door_closed_lock1,
        IN5_5Door_closed_lock2,
        IN5_6Door_closed_lock3,
        IN5_7Door_closed_lock4,
        IN5_8Run,
        IN5_9Stop,
        IN5_10Feeder1,
        IN5_11Feeder2,
        IN5_12Reset,
        IN5_13emergency_stop,
        IN5_14SSR1_OK_emergency_stop,
        IN5_15SSR2_OK_LOCK,


        IN6_0NG_plate_1_in_position,
        IN6_1plate_type1,
        IN6_2plate_type2,
        IN6_3plate_type3,
        IN6_4plate_type4,
        IN6_5plate_type5,
        IN6_6plate_has_left_Behind_the_stopping_cylinder3,
        IN6_7plate_has_left_Behind_the_stopping_cylinder4,
        IN6_8backup,
        IN6_9backup,
        IN6_10backup,
        IN6_11backup,
        IN6_12backup,
        IN6_13backup,
        IN6_14backup,
        IN6_15backup,


        IN7_0BOARD_AVAILABLE,
        IN7_1FAILED_BOARD_AVAILABLE_OPTIONAL,
        IN7_2MACHINE_READY_TO_RECEIVE,
        IN7_3backup,
        IN7_4backup,
        IN7_5backup,
        IN7_6backup,
        IN7_7backup,
        IN7_8backup,
        IN7_9backup,
        IN7_10backup,
        IN7_11backup,
        IN7_12Reset,
        IN7_13backup,
        IN7_14backup,
        IN7_15backup
    }

    class IOManager
    {
        private Dictionary<IO_OutFunction_Table, int> IO_OutFunctionnames = new Dictionary<IO_OutFunction_Table, int>();
        private Dictionary<IO_INFunction_Table, int> IO_INFunctionnames = new Dictionary<IO_INFunction_Table, int>();
        public bool[] OutIO_status = new bool[200];
        public bool[] INIO_status = new bool[200];

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
                    Thread.Sleep(5);
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
                    Thread.Sleep(5);
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