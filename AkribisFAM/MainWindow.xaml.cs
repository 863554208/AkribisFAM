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
using AkribisFAM.Windows;

namespace AkribisFAM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            ContentDisplay.Content = new MainContent();  // MainScreen 是你定义的一个用户控件或界面
        }

        // 点击 "手动调试" 按钮
        private void ManualControlButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = new ManualControl(); // ManualDebugScreen 是你定义的用户控件或界面
        }

        private void ParameterConfigButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = new ParameterConfig(); // ManualDebugScreen 是你定义的用户控件或界面
        }
        private void InternetConfigButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = new InternetConfig(); // ManualDebugScreen 是你定义的用户控件或界面
        }
        private void DebugLogButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = new DebugLog(); // ManualDebugScreen 是你定义的用户控件或界面
        }



    }
}
