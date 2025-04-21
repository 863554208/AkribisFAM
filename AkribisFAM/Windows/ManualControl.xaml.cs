using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// ManualControl.xaml 的交互逻辑
    /// </summary>
    public partial class ManualControl : UserControl
    {
        public ManualControl()
        {
            InitializeComponent();
        }

        private void AxisConfigure_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            ManualControlDisplay.Content = new AxisControl();  // MainScreen 是你定义的一个用户控件或界面
        }

        private void AirControl_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            ManualControlDisplay.Content = new AirControl();  // MainScreen 是你定义的一个用户控件或界面
            int a = 1;
        }
    }
}
