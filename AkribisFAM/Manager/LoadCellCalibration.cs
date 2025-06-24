using AkribisFAM.CommunicationProtocol;
using AkribisFAM.DeviceClass;
using AkribisFAM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using AkribisFAM.Util;
using System.Threading.Tasks;
using static AkribisFAM.DeviceClass.AssemblyGantryControl;
using System.Windows.Markup;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace AkribisFAM.Manager
{
    public class LoadCellCalibration
    {
        public LoadCellCalibration()
        {

            LoadCellCalibration calib;
            if (!FileHelper.Load(out calib) && calib != null)
            {
                Models = calib.Models;
            }

        }
        public LoadCellModel[] Models = new LoadCellModel[]
        {
            new LoadCellModel()
            {
                picker = Picker.Picker1,
            },
             new LoadCellModel()
            {
                picker = Picker.Picker2,
            },
              new LoadCellModel()
            {
                picker = Picker.Picker3,
            },
               new LoadCellModel()
            {
                picker = Picker.Picker4,
            },
        };
        public class LoadCellModel
        {
            public AssemblyGantryControl.Picker picker { get; set; }
            public double m { get; set; }
            public double C { get; set; }
            public List<NewtonCurrent> NewtonCurrentList { get; set; } = new List<NewtonCurrent>();
            public DateTime LastUpdate { get; set; } = DateTime.MinValue;
            public string Description { get; set; } = string.Empty;

            public LoadCellModel() { }
            public LoadCellModel(LoadCellModel model)
            {
                picker = model.picker;
                m = model.m;
                C = model.C;
                NewtonCurrentList = new List<NewtonCurrent>(model.NewtonCurrentList);
                LastUpdate = model.LastUpdate;
                Description = model.Description;
            }
         
            public bool UpdateList(List<NewtonCurrent> _list, string description = "")
            {
                if (_list.Count < 1)
                {
                    return false;
                }
                NewtonCurrentList.Clear();
                var _listX = _list.Select(x => x.Current).ToList();
                var _listY = _list.Select(x => x.Newton).ToList();
                var _coefficient = CalculateLinearCoefficients(_listX, _listY);
                NewtonCurrentList = _list;
                m = _coefficient.m;
                C = _coefficient.c;
                LastUpdate = DateTime.Now;
                return true;
            }

            private (double m, double c) CalculateLinearCoefficients(List<double> x, List<double> y) //x=newton, y=current
            {
                if (x.Count != y.Count || x.Count == 0)
                    throw new ArgumentException("x and y must be the same length and not empty.");

                double meanX = x.Average();
                double meanY = y.Average();

                double sumXY = 0;
                double sumXX = 0;

                for (int i = 0; i < x.Count; i++)
                {
                    sumXY += (x[i] - meanX) * (y[i] - meanY);
                    sumXX += (x[i] - meanX) * (x[i] - meanX);
                }

                double m = sumXY / sumXX;
                double c = meanY - m * meanX;

                return (m, c);
            }

            public bool CurrentToNetwon(double current, out double newton)
            {
                newton = 0;
                if (m == 0 || C == 0)
                {
                    return false;
                }

                newton = m * current + C;
                return true;
            }
            public bool NewtonToCurrent(double newton, out double current)
            {
                current = 0;
                if (m == 0 || C == 0)
                {
                    return false;
                }

                current = (newton - C) / m;
                return true;
            }

        }

        private string FloatToHexString(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }
        public void Zeroing()
        {
            Task.Run(new Action(() =>
            {
                TCPNetworkManage.InputLoop(ClientNames.Pressure_sensor, "#CLR01-01");
            }));
        }
        public void ChangeToSensitivityCalib(int port = 502)
        {
            //step 1 
            byte[] data = HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = HexStringToBytes("00 00 00 00 00 0B 01 10 01 26 00 02 04 3F 80 00 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 3
            data = HexStringToBytes("00 00 00 00 00 06 01 03 01 26 00 02");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public void ChangeToWeightCalib(int port = 502)
        {
            //step 1 
            byte[] data = HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = HexStringToBytes("00 00 00 00 00 0B 01 10 01 26 00 02 04 00 00 00 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 3
            data = HexStringToBytes("00 00 00 00 00 06 01 03 01 26 00 02");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public void ChangeCalibWeight(float weightf, int port = 502)
        {
            //step 1 
            byte[] data = HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = HexStringToBytes("00 00 00 00 00 0B 01 10 01 2E 00 02 04 " + FloatToHexString(weightf));
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 3
            data = HexStringToBytes("00 00 00 00 00 06 01 03 01 2E 00 02");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public void ChannelCAL0(int port = 502)
        {
            //step 1 
            byte[] data = HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = HexStringToBytes("00 00 00 00 00 0B 01 10 01 2A 00 02 04 00 00 00 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }

        public void ChannelCALF0(int port = 502)
        {
            //step 1 
            byte[] data = HexStringToBytes("00 00 00 00 00 0B 01 10 00 00 00 02 04 44 8A E0 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
            //step 2
            data = HexStringToBytes("00 00 00 00 00 0B 01 10 01 2C 00 02 04 00 00 00 00");
            SendRawModbusTcp(data, TCPNetworkManage.namedClients[ClientNames.Pressure_sensor].host);
        }
        /// <summary>
        /// 解析形如 =+0000.36+9999.99 的字符串，返回第 index 个值（1 或 2），并将其转换为克（g）。
        /// </summary>
        public double TryParseTwoValuesInGrams(string input, int index = 1,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            double valueInNewtons = Parser.TryParseTwoValues(input, index, filePath, memberName, lineNumber);

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
        public class NewtonCurrent
        {

            private double current;

            public double Current
            {
                get { return current; }
                set { current = value; }
            }

            private double newton;

            public double Newton
            {
                get { return newton; }
                set { newton = value; }
            }
        }



    }
}
