using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// connection window
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
            connectState.Add("Pressure_sensor", false);
            Readdevicesjson();
            RegisterHandlersInContainer(Networkgrid);
        }
        private void Readdevicesjson()//读IP地址和端口号
        {
            try
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
                //this.Pressure_sensor_IP.Text = (obj["Pressure_sensor"]["IP"]).ToString();
                //this.Pressure_sensor_Port.Text = (obj["Pressure_sensor"]["Port"]).ToString();
            }
            catch (Exception ex) {
                MessageBox.Show("Read device IP failed!");
            }
        }


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

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t)
                        yield return t;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private void RegisterHandlersInContainer(object container)
        {
            if (container is DependencyObject depObj)
            {
                foreach (var child in FindVisualChildren<TextBox>(depObj))
                {
                    // 禁用输入法
                    InputMethod.SetIsInputMethodEnabled(child, false);
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // 使用正则表达式检查输入是否为数字
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void IPAddressTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // IP地址正则表达式 (简单版本)
            Regex regex = new Regex(@"^([0-9]{1,3}\.){0,3}[0-9]{0,3}$");
            e.Handled = !regex.IsMatch(newText);
        }

        private void IPAddressTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidIPAddress(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsValidIPAddress(string ip)
        {
            // 更严格的IP地址验证
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(ip, pattern);
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

        private void sendMess(ClientNames device, string mess) {
            Task.Run(new Action(() =>
            {
                TCPNetworkManage.InputLoop(device, mess);
            }));
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (device.SelectedIndex == 0)
                {
                    sendMess(ClientNames.camera1_Feed, Command.Text + "\r\n");
                }
                else if (device.SelectedIndex == 1)
                {
                    sendMess(ClientNames.camera1_Runner, Command.Text + "\r\n");
                }
                else if (device.SelectedIndex == 2)
                {
                    sendMess(ClientNames.camera2, Command.Text + "\r\n");
                }
                else if (device.SelectedIndex == 3)
                {
                    sendMess(ClientNames.camera3, Command.Text + "\r\n");
                }
                else if (device.SelectedIndex == 4)
                {
                    sendMess(ClientNames.lazer, Command.Text);
                }
                else if (device.SelectedIndex == 5)
                {
                    sendMess(ClientNames.scanner, Command.Text);
                }
            }));
        }

    }
}
