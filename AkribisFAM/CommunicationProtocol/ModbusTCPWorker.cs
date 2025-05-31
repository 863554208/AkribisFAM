
using HslCommunication;
using HslCommunication.ModBus;
using System;
using System.Net.Sockets;
using System.Timers;


namespace AkribisFAM.CommunicationProtocol
{
    public class ModbusTCPWorker
    {
        private static ModbusTCPWorker _instance;
        public bool connect_state = false;
        private string m_ip = "173.1.1.14";
        private int port;
        private ModbusTcpNet modbus = null;
        //private object locker = new object();
        private static readonly object locker = new object();
        private ModbusTCPWorker()
        {

        }

        public static ModbusTCPWorker GetInstance()
        {
            //lock (locker)
            //{

                if (_instance == null)
                {
                    _instance = new ModbusTCPWorker();
                }
                return _instance;
            //}
            //Console.WriteLine(_instance);
            

        }
        public bool Initializate()
        {
            return Connect(m_ip = "173.1.1.14", port = 502);
        }

        public bool Connect(string ip = null, int port = 502, int timeout = 1000)
        {
            if (connect_state) return true;

            string targetIp = ip ?? m_ip;
            modbus = new ModbusTcpNet(targetIp, port)
            {
                ConnectTimeOut = timeout
            };

            connect_state = modbus.ConnectServer().IsSuccess;
            Console.WriteLine(connect_state ? "Connect Successfully！" : $"Connect Failed:{modbus.ConnectServer().Message}");
            return connect_state;
        }

        //private void CheckConnection(object sender, ElapsedEventArgs e)
        //{
        //    // 检查连接是否仍然有效
        //    if (!modbus.ConnectServer().IsSuccess)
        //    {
        //        Console.WriteLine("连接已断开，尝试重新连接...");
        //        connect_state = modbus.ConnectServer().IsSuccess;
        //        if (connect_state)
        //        {
        //            Console.WriteLine("重新连接成功！");
        //        }
        //        else
        //        {
        //            Console.WriteLine($"重新连接失败：{modbus.ConnectServer().Message}");
        //        }
        //    }
        //}

        public bool Disconnect()
        {
            if (connect_state)
            {
                modbus?.ConnectClose();
                connect_state = false;
                Console.WriteLine("Disconnected.");

            }
            return connect_state;
        }

        ~ModbusTCPWorker()
        {
            Disconnect();
        }





        // 读取保持寄存器
        public short Read_Holding_Register(int index)
        {
            if (!connect_state)
            {
                Console.WriteLine("Not connected to the server, unable to read the hold register.");
                return -1;
            }

            try
            {
                // 使用功能码 0x03 读取保持寄存器
                OperateResult<short> readResult = modbus.ReadInt16("3;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"hold register {index} value is:{readResult.Content}");
                    return readResult.Content;
                }
                else
                {
                    Console.WriteLine($"read the hold register {index} failed: {readResult.Message}");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"read the hold register {index} with Exception: {ex.Message}");
                return -1;
            }
        }

        // 读取输入寄存器
        public short Read_Input_Register(int index)
        {
            if (!connect_state)
            {
                Console.WriteLine("Not connected to the server, unable to read input register.");
                return -1;
            }

            try
            {
                // 使用功能码 0x04 读取输入寄存器
                OperateResult<short> readResult = modbus.ReadInt16("4;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"input register {index} value is：{readResult.Content}");
                    return readResult.Content;
                }
                else
                {
                    Console.WriteLine($"read input register {index} failed：{readResult.Message}");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"read input register {index} with Exception: {ex.Message}");
                return -1;
            }
        }

        // 读取离散输入
        public bool Read_Discrete_Input(int index)
        {
            if (!connect_state)
            {
                Console.WriteLine("Not connected to the server, unable to read discrete input.");
                return false;
            }

            try
            {
                // 使用功能码 0x02 读取离散输入
                OperateResult<bool> readResult = modbus.ReadDiscrete("2;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"discrete input {index} state is: {readResult.Content}");
                    return readResult.Content;
                }
                else
                {
                    Console.WriteLine($"read discrete input {index} failed：{readResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"read discrete input {index} with Exception: {ex.Message}");
                return false;
            }
        }

        // 写单个保持寄存器
        public bool Write_Holding_Register(int index, short value)
        {
            if (!connect_state)
            {
                Console.WriteLine("Not connected to the server, unable to read the hold register.");
                return false;
            }

            try
            {
                // 使用功能码 0x06 写单个保持寄存器
                OperateResult writeResult = modbus.Write("3;" + index, value);
                if (writeResult.IsSuccess)
                {
                    Console.WriteLine($"hold register {index} write successfully，value is: {value}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"write to the hold register {index} failed：{writeResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"write to the hold register {index} with Exception: {ex.Message}");
                return false;
            }
        }

        // 读取线圈状态
        public bool Read_Coil(int index, ref bool result)
        {
            if (!connect_state)
            {
                return false;
            }

            try
            {
                // 使用功能码 0x01 读取线圈状态
                OperateResult<bool> readResult = modbus.ReadBool("1;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"Coil {index} state is: {readResult.Content}");
                    Console.WriteLine($"1;{index.ToString()}");
                    result = readResult.Content;
                    return true;
                }
                else
                {
                    //Console.WriteLine($"读取线圈 {index} 失败：{readResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"读取线圈 {index} 时发生异常：{ex.Message}");
                return false;
            }
        }
        // 写单个线圈
        public bool Write_Coil(int index, bool value)
        {
            if (!connect_state)
            {
                return false;
            }

            try
            {
                // 使用功能码 0x05 写单个线圈
                OperateResult writeResult = modbus.Write("1;" + index, value);
                if (writeResult.IsSuccess)
                {
                    Console.WriteLine($"Coil {index} write successfully, state is : {value}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"write to coil {index} failed: {writeResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"write to coil {index} with Exception: {ex.Message}");
                return false;
            }
        }
    }
}
