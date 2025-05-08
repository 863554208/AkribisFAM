using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using AkribisFAM.Windows;
using AkribisFAM.Util;
using System.Globalization;
using System.Windows.Markup;
using WpfExtensions.Xaml;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace AkribisFAM
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private CancellationTokenSource _cancellationTokenSource;

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

        //private void StartAutoRun_Click(object sender, RoutedEventArgs e)
        //{
        //    AutorunManager.Current.AutoRunMain();
        //}

        private async void StartAutoRun_Click(object sender, RoutedEventArgs e)
        {
            Logger.WriteLog("123");
            try
            {
                StartAutoRunButton.IsEnabled = false;
                // 使用 Task.Run 来异步运行 AutoRunMain

                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;

                await Task.Run(() => AutorunManager.Current.AutoRunMain());
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error running AutoRunMain: {ex.Message}");
            }
        }

        private void PauseAutoRun_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalManager.Current.IsPause == false)
            {
                GlobalManager.Current.IsPause = true;
                //AutorunManager.Current.PauseAutoRun();  // 异步执行暂停
                PauseAutoRunButton.Background = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                GlobalManager.Current.IsPause = false;
                //AutorunManager.Current.ResumeAutoRun();
                PauseAutoRunButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }


        private void StopAutoRun_Click(object sender, RoutedEventArgs e)
        {
            AutorunManager.Current.StopAutoRun();
            StartAutoRunButton.IsEnabled = true;
        }

        private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            SetLanguage("en-US");

            // 刷新窗口，重新加载资源
            RefreshUI();
        }

        private void SetLanguage(string culture)
        {
            // 设置当前线程的文化信息
            CultureInfo cultureInfo = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        private void RefreshUI()
        {
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.Name);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            string Culture = "/Dict/AppResource_{0}.xaml";
            int selectedItem = comboBox.SelectedIndex;
            if (selectedItem == 0)
            {
                Culture = string.Format(Culture, "zh_CN");
                List<string> ll = new List<string>();
                foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
                {
                    if (dictionary.Source != null && dictionary.Source.OriginalString.Contains("AppResource"))
                    {
                        bool b = Application.Current.Resources.MergedDictionaries.Remove(dictionary);
                        dictionary.Source = new Uri(Culture, UriKind.RelativeOrAbsolute);
                        Application.Current.Resources.MergedDictionaries.Add(dictionary);
                        break;
                    }
                }
            }
            else if (selectedItem == 1)
            {
                Culture = string.Format(Culture, "en_US");
                List<string> ll = new List<string>();
                foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
                {
                    if (dictionary.Source != null && dictionary.Source.OriginalString.Contains("AppResource"))
                    {
                        bool b = Application.Current.Resources.MergedDictionaries.Remove(dictionary);
                        dictionary.Source = new Uri(Culture, UriKind.RelativeOrAbsolute);
                        Application.Current.Resources.MergedDictionaries.Add(dictionary);
                        break;
                    }
                }
            }
        }
    }
}
