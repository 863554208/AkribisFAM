using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Util
{
    using System;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using AAMotion;
    using AkribisFAM.CommunicationProtocol;

    public class Parser
    {
        /// <summary>
        /// 解析形如 =+0000.36+9999.99 的字符串，返回第 index 个值（1 或 2）。
        /// </summary>
        public static double TryParseTwoValues(string input, int index = 1,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            void Log(string reason)
            {
                Console.WriteLine(
                    $"[Parser.{memberName}] (Line {lineNumber}) 错误：{reason}\n" +
                    $"→ 原始输入: \"{input}\"\n" +
                    $"→ 期望格式: =+0000.36+9999.99 或 =-123.45-678.90\n" +
                    $"→ 来源文件: {filePath}\n"
                );
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Log("输入为空或仅包含空白字符。");
                return double.NaN;
            }

            if (!input.StartsWith("="))
            {
                Log("输入不以 '=' 开头。");
                return double.NaN;
            }

            if (input.Length < 3)
            {
                Log("输入过短，无法包含两个合法值。");
                return double.NaN;
            }

            try
            {
                string trimmed = input.Substring(1); // 去掉 '='

                if (trimmed[0] != '+' && trimmed[0] != '-')
                {
                    Log("第一个数缺少正负号。");
                    return double.NaN;
                }

                int secondSignIndex = -1;
                for (int i = 1; i < trimmed.Length; i++)
                {
                    if (trimmed[i] == '+' || trimmed[i] == '-')
                    {
                        secondSignIndex = i;
                        break;
                    }
                }

                if (secondSignIndex == -1)
                {
                    Log("未找到第二个数的正负号，缺少分隔符。");
                    return double.NaN;
                }

                string part1 = trimmed.Substring(0, secondSignIndex);
                string part2 = trimmed.Substring(secondSignIndex);

                if (index == 1)
                    return double.Parse(part1, CultureInfo.InvariantCulture);
                else if (index == 2)
                    return double.Parse(part2, CultureInfo.InvariantCulture);
                else
                {
                    Log("索引非法，仅支持 1 或 2。");
                    return double.NaN;
                }
            }
            catch (FormatException fe)
            {
                Log($"格式解析错误: {fe.Message}");
                return double.NaN;
            }
            catch (Exception ex)
            {
                Log($"未知错误: {ex.Message}");
                return double.NaN;
            }
        }

        /// <summary>
        /// 解析形如 =+0000.36+9999.99 的字符串，返回第 index 个值（1 或 2），并将其转换为克（g）。
        /// </summary>
        public static double TryParseTwoValuesInGrams(string input, int index = 1,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            double valueInNewtons = TryParseTwoValues(input, index, filePath, memberName, lineNumber);

            if (double.IsNaN(valueInNewtons))
            {
                // 如果 TryParseTwoValues 返回 NaN，说明解析失败，直接返回 NaN
                return double.NaN;
            }

            // 将牛顿转换为克
            const double gravity = 9.8; // 重力加速度
            return valueInNewtons / gravity * 1000;
        }

        public static byte[] HexStringToBytes(string hex)
        {
            hex = hex.Replace(" ", ""); // 清除空格
            return Enumerable.Range(0, hex.Length / 2)
                             .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                             .ToArray();
        }
        public static void SendRawModbusTcp(byte[] request, string ip, int port = 502)
        {
            using (var client = new TcpClient())
            {
                client.Connect(ip, port);
                var stream = client.GetStream();

                stream.Write(request, 0, request.Length);

                // 接收响应（通常 5~260 字节）
                byte[] buffer = new byte[256];
                int read = stream.Read(buffer, 0, buffer.Length);

                //string responseHex = BitConverter.ToString(buffer, 0, read).Replace("-", " ");
            }
        }

        public static string FloatToHexString(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }

        public static void ChangeToSensitivityCalib(int port = 502)
        {
            //step 1 
            byte[] data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 01 26 00 02 04 3F 80 00 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 3
            data = Parser.HexStringToBytes("00 00 00 00 00 06 01 03 01 26 00 02");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public static void ChangeToWeightCalib(int port = 502)
        {
            //step 1 
            byte[] data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 01 26 00 02 04 00 00 00 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 3
            data = Parser.HexStringToBytes("00 00 00 00 00 06 01 03 01 26 00 02");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public static void ChangeCalibWeight(float weightf, int port = 502)
        {
            //step 1 
            byte[] data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 01 2E 00 02 04 " + Parser.FloatToHexString(weightf));
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 3
            data = Parser.HexStringToBytes("00 00 00 00 00 06 01 03 01 2E 00 02");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public static void ChannelCAL0(int port = 502)
        {
            //step 1 
            byte[] data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 01 2A 00 02 04 00 00 00 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public static void ChannelCALF0(int port = 502)
        {
            //step 1 
            byte[] data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = Parser.HexStringToBytes("00 00 00 00 00 0B 01 10 01 2C 00 02 04 00 00 00 00");
            Parser.SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }
    }
}
