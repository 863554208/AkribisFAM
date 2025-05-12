using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AkribisFAM.CommunicationProtocol;
using static System.Windows.Forms.AxHost;
using System.Threading;
using System.Collections;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// IOConfigure.xaml 的交互逻辑
    /// </summary>
    public partial class IOConfigure : UserControl
    {

        private Dictionary<string, int> UIIOPairs { get; set; }
        public IOConfigure()
        {
            InitializeComponent();
            ChangeInIOState(rect1, 1);
            // 初始化字典
            UIIOPairs = new Dictionary<string, int>
            {
                { "button1", 1 }
            };
        }

        private void ChangeInIOState(Rectangle rect, int state)
        {
            if (state == 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.LightGreen);
                }));
            }
            else {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rect.Fill = new SolidColorBrush(Colors.LightPink);
                }));
            }
        }

        int ret = 1;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                //IO输出接口
                int IOID = UIIOPairs["button1"];
                //int ret = SendIO(IOID);
                if (ret == 0)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        button.Background = new SolidColorBrush(Colors.LightGreen);
                    }));
                    ret = 1;
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        button.Background = new SolidColorBrush(Colors.LightPink);
                    }));
                    ret = 0;
                }
            }
        }
    }
}
