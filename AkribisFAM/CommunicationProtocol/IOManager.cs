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
        //Out1
        Left_1_lift_cylinder_extend = 0,
        Left_1_lift_cylinder_retract,
        Right_1_lift_cylinder_extend,
        Right_1_lift_cylinder_retract,
        Left_2_lift_cylinder_extend,
        Left_2_lift_cylinder_retract,
        Right_2_lift_cylinder_extend,
        Right_2_lift_cylinder_retract,
        Left_3_lift_cylinder_extend,
        Left_3_lift_cylinder_retract,
        Right_3_lift_cylinder_extend,
        Right_3_lift_cylinder_retract,
        C4_lift_cylinder_extend,
        C4_lift_cylinder_retract
    }

    enum IO_INFunction_Table
    {
        //IN1
        Acceleration_Sign1 = 0,
        Stop_Sign1,
        Stop_Sign2,
        Stop_Sign3,
        Stop_Sign4,
        NG_cover_plate1,
        NG_cover_plate2,
        plate_has_left_Behind_the_stopping_cylinder1,
        plate_has_left_Behind_the_stopping_cylinder2,
        bord_lift_in_position1,
        bord_lift_in_position2,
        bord_lift_in_position3,
        bord_lift_in_position4
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
            //ReadIO_status();
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

        public bool WriteIO_Truestatus(IO_OutFunction_Table iO_OutFunction_Table)//写IO状态为True
        {
            if (!OutIO_status[(int)iO_OutFunction_Table])
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
            //Console.WriteLine(  
        }

        public bool WriteIO_Falsestatus(IO_OutFunction_Table iO_OutFunction_Table)//写IO状态为False
        {
            if (OutIO_status[(int)iO_OutFunction_Table])
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
            //Console.WriteLine(  
        }
    }
}