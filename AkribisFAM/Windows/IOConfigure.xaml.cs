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
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Threading;
using YamlDotNet.Core.Tokens;
using System.Windows.Media.TextFormatting;
using AkribisFAM.Util;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// IOConfigure.xaml 的交互逻辑
    /// </summary>
    public partial class IOConfigure : UserControl
    {

        private Dictionary<string, int> OutputIOPairs { get; set; }
        private Dictionary<string ,int> InputIOPairs { get; set; }

        public IOConfigure()
        {
            InitializeComponent();

            // 初始化字典
            OutputIOPairs = new Dictionary<string, int> { };//{{ "button1",1 }}
            foreach (IO_OutFunction_Table outitem in Enum.GetValues(typeof(IO_OutFunction_Table)))
            {
                OutputIOPairs.Add($"Out{(int)outitem}", (int)outitem);
            }

            InputIOPairs = new Dictionary<string, int> { };//{{ "button1",1 } };
            foreach (IO_INFunction_Table initem in Enum.GetValues(typeof(IO_INFunction_Table)))
            {
                InputIOPairs.Add($"IN{(int)initem}", (int)initem);
            }


            Task task1 = new Task(UpdateUI_IO);
            task1.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 创建样式
            Style derivedStyle = new Style(typeof(Button), (Style)FindResource("RoundCornerButton"));
            // 遍历窗口中的所有按钮并应用样式
            ApplyStyleToButtons(this, derivedStyle);
        }

        private void ApplyStyleToButtons(DependencyObject parent, Style style)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Button button)
                {
                    button.Style = style;
                    button.Width = double.NaN;
                    button.Height = 55;
                    button.HorizontalAlignment = HorizontalAlignment.Stretch;
                    button.VerticalAlignment = VerticalAlignment.Center;
                }

                if (child is TextBlock textblock)
                {
                    textblock.Width = 120;
                    textblock.Height = 50;
                    textblock.HorizontalAlignment = HorizontalAlignment.Center;
                    textblock.VerticalAlignment = VerticalAlignment.Center;
                    textblock.TextAlignment = TextAlignment.Justify;
                }

                // 递归查找子元素
                ApplyStyleToButtons(child, style);
            }
        }

        private void UpdateUI_IO()
        {
            while (true)
            {
                foreach (var Inkvp in InputIOPairs)//ShowInputIO
                {
                    var inbuttonname = Inkvp.Key;
                    var InputIOPairskey = Inkvp.Value;
                    ShowChangeInIOState(inbuttonname, IOManager.Instance.INIO_status[InputIOPairskey]);
                }

                foreach (var Outkvp in OutputIOPairs)//ShowOutputIO
                {
                    var buttonname = Outkvp.Key;
                    var OutputIOPairsvalue = Outkvp.Value;
                    ShowChangeOutIOState(buttonname, IOManager.Instance.OutIO_status[OutputIOPairsvalue]);
                }
                Thread.Sleep(200);
            }
        }

        private void ShowChangeInIOState(string inbuttonname, int state)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Button button = this.FindName(inbuttonname) as Button;
                if (state == 0)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4ECE4E"));//#FF4ECE4E
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF919791"));//#FF919791
                }
            }));
        }

        private void ShowChangeOutIOState(string ButtonName, int state)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Button button = this.FindName(ButtonName) as Button;
                if (state == 0)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4ECE4E"));//#FF4ECE4E
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF919791"));//#FF919791
                }
            }));
        }
























        bool IO_Clickstatus = true;
        private async void Out_Click(object sender, RoutedEventArgs e)//单独触发点亮IO
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
                            IOManager.Instance.IO_ControlStatus(outEnum, IOManager.Instance.OutIO_status[(int)outEnum]);
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
