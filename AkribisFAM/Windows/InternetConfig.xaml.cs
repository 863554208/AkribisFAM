using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Newtonsoft.Json.Linq;


namespace AkribisFAM.Windows
{
    /// <summary>
    /// InternetConfig.xaml 的交互逻辑
    /// </summary>
    public partial class InternetConfig : UserControl
    {
        public InternetConfig()
        {
            InitializeComponent();
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

        private void camera1_Feed_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name,textBox.Text);

        }

        private void camera1_Feed_Port_Changed(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void camera1_Runner_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void camera1_Runner_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void camera2_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void camera2_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void camera3_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void camera3_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void lazer_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void lazer_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void scanner_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void scanner_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void Mes_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        private void Mes_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Writedevicesjson(textBox.Name, textBox.Text);
        }

        
    }
}
