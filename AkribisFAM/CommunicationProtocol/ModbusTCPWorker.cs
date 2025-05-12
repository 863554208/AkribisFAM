
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
        private bool connect_state = false;
        private string m_ip = "172.1.1.13";
        private int port;
        private ModbusTcpNet modbus = null;
        //private object locker = new object();
        private static readonly object locker = new object();
        private ModbusTCPWorker()
        {

        }

        public static ModbusTCPWorker GetInstance()
        {
            lock (locker)
            {

                if (_instance == null)
                {
                    _instance = new ModbusTCPWorker();
                }
            }
            Console.WriteLine(_instance);
            return _instance;

        }
        public bool Initializate()
        {
            return Connect(m_ip = "172.1.1.13", port = 502);
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
            Console.WriteLine(connect_state ? "连接成功！" : $"连接失败：{modbus.ConnectServer().Message}");
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
                Console.WriteLine("已断开连接。");

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
                Console.WriteLine("未连接到服务器，无法读取保持寄存器。");
                return -1;
            }

            try
            {
                // 使用功能码 0x03 读取保持寄存器
                OperateResult<short> readResult = modbus.ReadInt16("3;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"保持寄存器 {index} 的值为：{readResult.Content}");
                    return readResult.Content;
                }
                else
                {
                    Console.WriteLine($"读取保持寄存器 {index} 失败：{readResult.Message}");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取保持寄存器 {index} 时发生异常：{ex.Message}");
                return -1;
            }
        }

        // 读取输入寄存器
        public short Read_Input_Register(int index)
        {
            if (!connect_state)
            {
                Console.WriteLine("未连接到服务器，无法读取输入寄存器。");
                return -1;
            }

            try
            {
                // 使用功能码 0x04 读取输入寄存器
                OperateResult<short> readResult = modbus.ReadInt16("4;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"输入寄存器 {index} 的值为：{readResult.Content}");
                    return readResult.Content;
                }
                else
                {
                    Console.WriteLine($"读取输入寄存器 {index} 失败：{readResult.Message}");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取输入寄存器 {index} 时发生异常：{ex.Message}");
                return -1;
            }
        }

        // 读取离散输入
        public bool Read_Discrete_Input(int index)
        {
            if (!connect_state)
            {
                Console.WriteLine("未连接到服务器，无法读取离散输入。");
                return false;
            }

            try
            {
                // 使用功能码 0x02 读取离散输入
                OperateResult<bool> readResult = modbus.ReadDiscrete("2;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"离散输入 {index} 的状态为：{readResult.Content}");
                    return readResult.Content;
                }
                else
                {
                    Console.WriteLine($"读取离散输入 {index} 失败：{readResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取离散输入 {index} 时发生异常：{ex.Message}");
                return false;
            }
        }

        // 写单个保持寄存器
        public bool Write_Holding_Register(int index, short value)
        {
            if (!connect_state)
            {
                Console.WriteLine("未连接到服务器，无法写入保持寄存器。");
                return false;
            }

            try
            {
                // 使用功能码 0x06 写单个保持寄存器
                OperateResult writeResult = modbus.Write("3;" + index, value);
                if (writeResult.IsSuccess)
                {
                    Console.WriteLine($"保持寄存器 {index} 写入成功，值为：{value}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"写入保持寄存器 {index} 失败：{writeResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入保持寄存器 {index} 时发生异常：{ex.Message}");
                return false;
            }
        }

        // 读取线圈状态
        public bool Read_Coil(int index)
        {
            if (!connect_state)
            {
                Console.WriteLine("未连接到服务器，无法读取线圈状态。");
                return false;
            }

            try
            {
                // 使用功能码 0x01 读取线圈状态
                OperateResult<bool> readResult = modbus.ReadBool("1;" + index);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"线圈 {index} 的状态为：{readResult.Content}");
                    Console.WriteLine($"1;{index.ToString()}");
                    return readResult.Content;
                }
                else
                {
                    Console.WriteLine($"读取线圈 {index} 失败：{readResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取线圈 {index} 时发生异常：{ex.Message}");
                return false;
            }
        }
        // 写单个线圈
        public bool Write_Coil(int index, bool value)
        {
            if (!connect_state)
            {
                Console.WriteLine("未连接到服务器，无法写入线圈。");
                return false;
            }

            try
            {
                // 使用功能码 0x05 写单个线圈
                OperateResult writeResult = modbus.Write("1;" + index, value);
                if (writeResult.IsSuccess)
                {
                    Console.WriteLine($"线圈 {index} 写入成功，状态为：{value}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"写入线圈 {index} 失败：{writeResult.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入线圈 {index} 时发生异常：{ex.Message}");
                return false;
            }
        }
    }
}
