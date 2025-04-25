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
using System.Windows.Threading;
using AkribisFAM.Windows;

namespace AkribisFAM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            // 创建定时器
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 订阅 Loaded 事件
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时立即更新一次时间
            Timer_Tick(this, null);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 更新 TextBlock 显示当前日期和时间
            currentTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
