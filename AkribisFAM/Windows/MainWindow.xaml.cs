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
using AkribisFAM.WorkStation;
using System.ComponentModel;
using AkribisFAM.Manager;
using AkribisFAM.ViewModel;

namespace AkribisFAM
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private CancellationTokenSource _cancellationTokenSource;
        public ErrorIconViewModel ViewModel { get; }

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

            ViewModel = new ErrorIconViewModel();
            this.DataContext = ViewModel;

            ErrorManager.Current.UpdateErrorCnt += UpdateIcon;
        }

        private void UpdateIcon()
        {
            // 使用 Dispatcher 来确保在 UI 线程上更新 UI
            Dispatcher.Invoke(() => ViewModel.UpdateIcon());
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
            //对轴初始化使能 改到登录之后
            GlobalManager.Current.InitializeAxis();

            Logger.WriteLog("MainWindow.xaml.cs.StartAutoRun_Click() Start Autorun");
            try
            {
                // 使用 Task.Run 来异步运行 AutoRunMain

                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;

                await Task.Run(() => AutorunManager.Current.AutoRunMain());
                if (AutorunManager.Current.isRunning)
                {
                    StartAutoRunButton.IsEnabled = false;
                }
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
                GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
                GlobalManager.Current.Lailiao_delta[GlobalManager.Current.current_Lailiao_step] = 0;
                GlobalManager.Current.Zuzhuang_state[GlobalManager.Current.current_Zuzhuang_step] = 0;
                GlobalManager.Current.Zuzhuang_delta[GlobalManager.Current.current_Zuzhuang_step] = 0;
                GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
                GlobalManager.Current.FuJian_delta[GlobalManager.Current.current_FuJian_step] = 0;
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalManager.Current.current_Lailiao_step = 0;
            GlobalManager.Current.current_Zuzhuang_step = 0;
            GlobalManager.Current.current_FuJian_step = 0;
            LaiLiao.Current.board_count = 0;
            ZuZhuang.Current.has_board = false;
            FuJian.Current.has_board = false;
            GlobalManager.Current.Lailiao_exit = false;
            GlobalManager.Current.Zuzhuang_exit = false;
            GlobalManager.Current.FuJian_exit = false;
            AutorunManager.Current.hasReseted = true;
            //button.PromptCount += 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(container);
            layer.Add(new PromptAdorner(button));
        }
    }

    internal class PromptableButton : Button
    {

        public ImageSource CoverImageSource
        {
            get { return (ImageSource)GetValue(CoverImageSourceProperty); }
            set { SetValue(CoverImageSourceProperty, value); }
        }

        public static readonly DependencyProperty CoverImageSourceProperty =
            DependencyProperty.Register("CoverImageSource", typeof(ImageSource), typeof(PromptableButton), new UIPropertyMetadata(null));


        public int PromptCount
        {
            get { return (int)GetValue(PromptCountProperty); }
            set { SetValue(PromptCountProperty, value); }
        }

        public static readonly DependencyProperty PromptCountProperty =
            DependencyProperty.Register("PromptCount", typeof(int), typeof(PromptableButton),
            new FrameworkPropertyMetadata(0, new PropertyChangedCallback(PromptCountChangedCallBack), new CoerceValueCallback(CoercePromptCountCallback)));


        public PromptableButton()
        {

        }

        static PromptableButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PromptableButton), new FrameworkPropertyMetadata(typeof(PromptableButton)));
        }

        private static object CoercePromptCountCallback(DependencyObject d, object value)
        {
            int promptCount = (int)value;
            promptCount = Math.Max(0, promptCount);

            return promptCount;
        }

        public static void PromptCountChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

    }

    internal class PromptAdorner : Adorner
    {

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        public PromptAdorner(UIElement adornedElement)
            : base(adornedElement)
        {

            _chrome = new PromptChrome();
            _chrome.DataContext = adornedElement;
            this.AddVisualChild(_chrome);
        }


        protected override Visual GetVisualChild(int index)
        {
            return _chrome;
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        {
            _chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        PromptChrome _chrome;
    }

    internal class PromptChrome : Control
    {
        static PromptChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PromptChrome), new FrameworkPropertyMetadata(typeof(PromptChrome)));
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size arrangeBounds)
        {

            this.Width = 34;
            this.Height = 34;

            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            TranslateTransform tt = new TranslateTransform();
            tt.X = 10;
            tt.Y = -10;
            this.RenderTransform = tt;

            return base.ArrangeOverride(arrangeBounds);
        }


    }
}
