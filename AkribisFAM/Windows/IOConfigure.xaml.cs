using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AkribisFAM.CommunicationProtocol;
using static System.Windows.Forms.AxHost;
using System.Collections;
using AkribisFAM.CommunicationProtocol;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Threading;
using YamlDotNet.Core.Tokens;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// IOConfigure.xaml 的交互逻辑
    /// </summary>
    public partial class IOConfigure : UserControl
    {

        private Dictionary<string, int> OutputIOPairs { get; set; }
        private Dictionary<int, Rectangle> InputIOPairs { get; set; }

        public IOConfigure()
        {
            InitializeComponent();
            // 初始化字典
            OutputIOPairs = new Dictionary<string, int> { };//{{ "button1",1 }}
            foreach (IO_OutFunction_Table outitem in Enum.GetValues(typeof(IO_OutFunction_Table)))
            {
                OutputIOPairs.Add($"Out{(int)outitem}", (int)outitem);
            }

            InputIOPairs = new Dictionary<int, Rectangle> { };//{{ 1, IN1 } };
            foreach (IO_INFunction_Table initem in Enum.GetValues(typeof(IO_INFunction_Table)))
            {
                InputIOPairs.Add((int)initem, this.FindName($"IN{(int)initem}") as Rectangle);
            }

            Task task1 = new Task(UpdateUI_IO);
            task1.Start();
        }

        private void UpdateUI_IO()
        {
            while (true)
            {
                foreach (var Inkvp in InputIOPairs)//ShowInputIO
                {
                    var InputIOPairskey = Inkvp.Key;
                    var rect = Inkvp.Value;
                    ShowChangeInIOState(rect, IOManager.Instance.INIO_status[InputIOPairskey] ? 1 : 0);
                }

                foreach (var Outkvp in OutputIOPairs)//ShowOutputIO
                {
                    var buttonname = Outkvp.Key;
                    var OutputIOPairsvalue = Outkvp.Value;
                    ShowChangeOutIOState(buttonname, IOManager.Instance.OutIO_status[OutputIOPairsvalue] ? 1 : 0);
                }
                Thread.Sleep(200);
            }
        }

        private void ShowChangeInIOState(Rectangle rect, int state)
        {
            if (state == 1)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.LightGreen);
                }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.LightGray);
                }));
            }
        }

        private void ShowChangeOutIOState(string ButtonName, int state)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Button button = this.FindName(ButtonName) as Button;
                if (state == 1)
                {
                    button.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    button.Background = new SolidColorBrush(Colors.LightGray);
                }
            }));
        }

        bool IO_Clickstatus = true;
        private async void Out_Click(object sender, RoutedEventArgs e)
        {
            if (IO_Clickstatus)
            {
                IO_Clickstatus = false;
                try
                {
                    await UI_IOControlStatus(((Button)sender).Name);
                }
                finally
                {
                    IO_Clickstatus = true;
                }
            }
        }
        private async Task UI_IOControlStatus(string IO_OutName)
        {
            try
            {
                if (!OutputIOPairs.ContainsKey(IO_OutName))
                {
                    Console.WriteLine($"The key does not exist in the dictionary：{IO_OutName}");
                    return;
                }
                int index = OutputIOPairs[IO_OutName];

                await Task.Run(() =>
                {
                    if (Enum.IsDefined(typeof(IO_OutFunction_Table), index))
                    {
                        IO_OutFunction_Table outEnum = (IO_OutFunction_Table)Enum.ToObject(typeof(IO_OutFunction_Table), index);


                        try
                        { 
                            bool currentStatus = IOManager.Instance.OutIO_status[(int)outEnum];
                            IOManager.Instance.IO_ControlStatus(outEnum, currentStatus ? 0 : 1);
                            return;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"IO Write Failure{ex.ToString()}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid enumeration value：{index.ToString()}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task failed: {ex}");
            }
        }
    }
}
