using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.CommunicationProtocol.CamerCalibProcess;
using Newtonsoft.Json.Linq;


namespace AkribisFAM.Windows
{
    /// <summary>
    /// InternetConfig.xaml 的交互逻辑
    /// </summary>
    public partial class InternetConfig : UserControl
    {
        public Dictionary<string, bool> connectState = new Dictionary<string, bool>();

        public InternetConfig()
        {
            InitializeComponent();
            connectState.Add("camera1_Feed", false);
            connectState.Add("camera1_Runner", false);
            connectState.Add("camera2", false);
            connectState.Add("camera3", false);
            connectState.Add("lazer", false);
            connectState.Add("scanner", false);
            connectState.Add("mes", false);
            connectState.Add("ModbusTCP", false);
            Readdevicesjson();
        }
        private void Readdevicesjson()//读IP地址和端口号
        {
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "devices.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            this.camera1_Feed_IP.Text = (obj["camera1_Feed"]["IP"]).ToString();
            this.camera1_Feed_Port.Text = (obj["camera1_Feed"]["Port"]).ToString();
            this.camera1_Runner_IP.Text = (obj["camera1_Runner"]["IP"]).ToString();
            this.camera1_Runner_Port.Text = (obj["camera1_Runner"]["Port"]).ToString();
            this.camera2_IP.Text = (obj["camera2"]["IP"]).ToString();
            this.camera2_Port.Text = (obj["camera2"]["Port"]).ToString();
            this.camera3_IP.Text = (obj["camera3"]["IP"]).ToString();
            this.camera3_Port.Text = (obj["camera3"]["Port"]).ToString();
            this.lazer_IP.Text = (obj["lazer"]["IP"]).ToString();
            this.lazer_Port.Text = (obj["lazer"]["Port"]).ToString();
            this.scanner_IP.Text = (obj["scanner"]["IP"]).ToString();
            this.scanner_Port.Text = (obj["scanner"]["Port"]).ToString();
            this.mes_IP.Text = (obj["mes"]["IP"]).ToString();
            this.mes_Port.Text = (obj["mes"]["Port"]).ToString();
            this.ModbusTCP_IP.Text = (obj["ModbusTCP"]["IP"]).ToString();
            this.ModbusTCP_Port.Text = (obj["ModbusTCP"]["Port"]).ToString();
        }

        //public void ConnectionThread() {
        //    Task.Run(new Action(() =>
        //    {
        //        while (true)
        //        {
        //            connectState["camera1_Feed"] = TCPNetworkManage.namedClients[ClientNames.camera1_Feed].isConnected;
        //            connectState["camera1_Runner"] = TCPNetworkManage.namedClients[ClientNames.camera1_Runner].isConnected;
        //            connectState["camera2"] = TCPNetworkManage.namedClients[ClientNames.camera2].isConnected;
        //            connectState["camera3"] = TCPNetworkManage.namedClients[ClientNames.camera3].isConnected;
        //            connectState["lazer"] = TCPNetworkManage.namedClients[ClientNames.lazer].isConnected;
        //            connectState["scanner"] = TCPNetworkManage.namedClients[ClientNames.scanner].isConnected;
        //            connectState["mes"] = TCPNetworkManage.namedClients[ClientNames.mes].isConnected;
        //            connectState["ModbusTCP"] = ModbusTCPWorker.GetInstance().connect_state;
        //            Thread.Sleep(1000);
        //        }
        //    }
        //    ));
        //}

        private void Writedevicesjson(string Devices,string IP_Port)//写相应设备的IP地址或端口号
        {
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "devices.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);
            string Device = Devices.Substring(0,Devices.LastIndexOf('_'));
            string DeviceNumber= Devices.Substring(Devices.LastIndexOf('_')+1);
            if (!obj[Device][DeviceNumber].ToString().Equals(IP_Port))
            {
                obj[Device][DeviceNumber] = IP_Port;
                File.WriteAllText(filePath, obj.ToString());
            } 
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            foreach (string key in connectState.Keys)
            {
                try
                {
                    string name = key + "_IP";
                    TextBox iptextBox = (TextBox)FindObject(name);
                    Writedevicesjson(iptextBox.Name, iptextBox.Text);
                    name = key + "_Port";
                    TextBox porttextBox = (TextBox)FindObject(name);
                    Writedevicesjson(porttextBox.Name, porttextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save failed:" + ex.Message);
                }  
            }
        }

        private Object FindObject(string name)
        {
            Object obj = this.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
            return obj;
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name.ToString().Substring(8, button.Name.ToString().Length - 8);
            string iptextboxname = name + "_IP";
            TextBox iptextBox = (TextBox)FindObject(iptextboxname);
            string porttextboxname = name + "_Port";
            TextBox porttextBox = (TextBox)FindObject(porttextboxname);
            try
            {
                ClientNames clientName = (ClientNames)Enum.Parse(typeof(ClientNames), name);
                int port = int.Parse(porttextBox.Text);
                TCPNetworkManage.clientNameToEndpoint[clientName] = (iptextBox.Text, port);
                await Task.Run(new Action(() =>
                {
                    TCPNetworkManage.ConnectClient(clientName);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during connection:" + ex.Message);
            }
        }

        private async void Disonnect_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name.ToString().Substring(11, button.Name.ToString().Length - 11);
            string iptextboxname = name + "_IP";
            TextBox iptextBox = (TextBox)FindObject(iptextboxname);
            string porttextboxname = name + "_Port";
            TextBox porttextBox = (TextBox)FindObject(porttextboxname);
            try
            {
                ClientNames clientName = (ClientNames)Enum.Parse(typeof(ClientNames), name);
                int port = int.Parse(porttextBox.Text);
                TCPNetworkManage.clientNameToEndpoint[clientName] = (iptextBox.Text, port);
                await Task.Run(new Action(() =>
                {
                    TCPNetworkManage.StopClient(clientName);
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during disconnection:" + ex.Message);
            }
        }

        private void ModbusTCPConnect_Click(object sender, RoutedEventArgs e)
        {
            if (ModbusTCPWorker.GetInstance().Connect())
            {
                IOManager.Instance.ReadIO_status();
            }
        }

        private void ModbusTCPDisonnect_Click(object sender, RoutedEventArgs e)
        {
            ModbusTCPWorker.GetInstance().Disconnect();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (device.SelectedIndex == 0)
                    {
                        TCPNetworkManage.InputLoop(ClientNames.camera1_Feed, Command.Text);
                    }
                    else if (device.SelectedIndex == 1)
                    {
                        TCPNetworkManage.InputLoop(ClientNames.camera1_Runner, Command.Text);
                    }
                    else if (device.SelectedIndex == 2)
                    {
                        TCPNetworkManage.InputLoop(ClientNames.camera2, Command.Text);
                    }
                    else if (device.SelectedIndex == 3)
                    {
                        TCPNetworkManage.InputLoop(ClientNames.camera3, Command.Text);
                    }
                    else if (device.SelectedIndex == 4)
                    {
                        TCPNetworkManage.InputLoop(ClientNames.lazer, Command.Text);
                    }
                    else if (device.SelectedIndex == 5)
                    {
                        TCPNetworkManage.InputLoop(ClientNames.scanner, Command.Text);
                    }
                }));
            });
        }
    }
}
