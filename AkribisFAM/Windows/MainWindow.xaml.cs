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
using AAMotion;
using static AkribisFAM.Manager.StateManager;
using static System.Windows.Forms.AxHost;
using System.Net.Http;
using AkribisFAM.CommunicationProtocol;
using System.Reflection;
using YamlDotNet.Core.Tokens;
using System.Windows.Media.Media3D;

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
        private ErrorWindow ErrorWindow;

        //ResetButton 按住3秒才能触发
        private Stopwatch resetPressStopwatch = new Stopwatch();
        private DispatcherTimer resetTimer;
        private bool isResetButtonTriggered = false;

        MainContent mainContent;
        ManualControl manualControl;
        ParameterConfig parameterConfig;
        Performance performance;
        InternetConfig internetConfig;
        DebugLog debugLog;

        public MainWindow()
        {
            InitializeComponent();

            // 创建定时器
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            _timer.Tick += Timer_Tick;
            

            // 订阅 Loaded 事件
            this.Loaded += MainWindow_Loaded;

            ViewModel = new ErrorIconViewModel();
            this.DataContext = ViewModel;

            ErrorManager.Current.UpdateErrorCnt += UpdateIcon;
            StateManager.Current.State = StateCode.IDLE;
            StateManager.Current.RunningHourCnt = 0;
            StateManager.Current.TotalInput = 0;
            StateManager.Current.TotalOutputOK = 0;
            StateManager.Current.TotalOutputNG = 0;
            StateManager.Current.currentUPH = 0;
            StateManager.Current.currentNG = 0;
            //Add By YXW
            mainContent = new MainContent();
            manualControl = new ManualControl();
            parameterConfig = new ParameterConfig();
            performance = new Performance();
            internetConfig = new InternetConfig();
            debugLog = new DebugLog();
            ContentDisplay.Content = mainContent;

            _timer.Start();
            //END Add

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
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(container);
            layer.Add(new PromptAdorner(button));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 更新 TextBlock 显示当前日期和时间
            currentTimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            NowState.Content = StateManager.Current.StateDict[StateManager.Current.State];
            if (StateManager.Current.State == StateCode.RUNNING)
            {
                StateManager.Current.RunningTime = DateTime.Now - StateManager.Current.RunningStart;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {

                    performance.RunningTimeLB.Content = StateManager.Current.RunningTime.ToString(@"hh\:mm\:ss");
                }));
            }
            if (StateManager.Current.State == StateCode.STOPPED)
            {
                StateManager.Current.StoppedTime = DateTime.Now - StateManager.Current.StoppedStart;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    performance.StoppedTimeLB.Content = StateManager.Current.StoppedTime.ToString(@"hh\:mm\:ss");
                }));
            }
            if (StateManager.Current.State == StateCode.MAINTENANCE)
            {
                StateManager.Current.MaintenanceTime = DateTime.Now - StateManager.Current.MaintenanceStart;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    performance.MaintenanceTimeLB.Content = StateManager.Current.MaintenanceTime.ToString(@"hh\:mm\:ss");
                }));
            }
            if (StateManager.Current.State == StateCode.IDLE)
            {
                StateManager.Current.IdleTime = DateTime.Now - StateManager.Current.IdleStart;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    performance.IdleTimeLB.Content = StateManager.Current.IdleTime.ToString(@"hh\:mm\:ss");
                }));
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                performance.INPUTLB.Content = StateManager.Current.TotalInput.ToString();
                performance.OUTPUT_OKLB.Content = StateManager.Current.TotalOutputOK.ToString();
                performance.OUTPUT_NGLB.Content = StateManager.Current.TotalOutputNG.ToString();
                //performance.LOADINGLB.Content = 
                performance.AVAILABILITYLB.Content = (StateManager.Current.IdleTime.TotalSeconds + StateManager.Current.RunningTime.TotalSeconds) / (StateManager.Current.IdleTime.TotalSeconds + StateManager.Current.RunningTime.TotalSeconds + StateManager.Current.MaintenanceTime.TotalSeconds + StateManager.Current.StoppedTime.TotalSeconds);
                if (StateManager.Current.RunningTime.TotalSeconds > 0.5) {
                    performance.PERFORMANCELB.Content = StateManager.Current.TotalInput / (StateManager.Current.RunningTime.TotalSeconds / 1200.0);
                }
                if (StateManager.Current.TotalOutputOK + StateManager.Current.TotalOutputNG > 0) {
                    performance.QUALITYLB.Content = StateManager.Current.TotalOutputOK / (StateManager.Current.TotalOutputOK + StateManager.Current.TotalOutputNG);
                }
                if (StateManager.Current.RunningTime.TotalSeconds > 3600 + StateManager.Current.RunningHourCnt * 3600) {
                    StateManager.Current.RunningHourCnt++;
                    int UPH = StateManager.Current.TotalOutputOK - StateManager.Current.currentUPH;
                    StateManager.Current.currentUPH = StateManager.Current.TotalOutputOK;
                    if (performance.UPHvalues.Count >= 24)
                    {
                        performance.UPHvalues.RemoveAt(0);
                        performance.UPHvalues.Add(UPH);
                        string head = performance.UPHLabels[0];
                        performance.UPHLabels.RemoveAt(0);
                        performance.UPHLabels.Add(head);
                    }
                    else
                    {
                        performance.UPHvalues.Add(UPH);
                    }
                    int NG = StateManager.Current.TotalOutputNG - StateManager.Current.currentNG;
                    int Yield = 0;
                    if (UPH + NG > 0)
                    {
                        Yield = (int)UPH*100 / (UPH + NG);
                    }
                    StateManager.Current.currentNG = StateManager.Current.TotalOutputNG;
                    if (performance.Yieldvalues.Count >= 24)
                    {
                        performance.Yieldvalues.RemoveAt(0);
                        performance.Yieldvalues.Add(Yield);
                        string head = performance.YieldLabels[0];
                        performance.YieldLabels.RemoveAt(0);
                        performance.YieldLabels.Add(head);
                    }
                    else
                    {
                        performance.Yieldvalues.Add(Yield);
                    }
                }
                ConnectState();
            }));
        }

        private void ConnectState() {
            if (TCPNetworkManage.namedClients.ContainsKey(ClientNames.camera1_Feed))  // 检查字典中是否存在这个客户端连接
            {
                internetConfig.connectState["camera1_Feed"] = TCPNetworkManage.namedClients[ClientNames.camera1_Feed].isConnected;
            }
            else
            {
                internetConfig.connectState["camera1_Feed"] = false;
            }
            if (TCPNetworkManage.namedClients.ContainsKey(ClientNames.camera1_Runner))  // 检查字典中是否存在这个客户端连接
            {
                internetConfig.connectState["camera1_Runner"] = TCPNetworkManage.namedClients[ClientNames.camera1_Runner].isConnected;
            }
            else
            {
                internetConfig.connectState["camera1_Runner"] = false;
            }
            if (TCPNetworkManage.namedClients.ContainsKey(ClientNames.camera2))  // 检查字典中是否存在这个客户端连接
            {
                internetConfig.connectState["camera2"] = TCPNetworkManage.namedClients[ClientNames.camera2].isConnected;
            }
            else
            {
                internetConfig.connectState["camera2"] = false;
            }
            if (TCPNetworkManage.namedClients.ContainsKey(ClientNames.camera3))  // 检查字典中是否存在这个客户端连接
            {
                internetConfig.connectState["camera3"] = TCPNetworkManage.namedClients[ClientNames.camera3].isConnected;
            }
            else
            {
                internetConfig.connectState["camera3"] = false;
            }
            if (TCPNetworkManage.namedClients.ContainsKey(ClientNames.lazer))  // 检查字典中是否存在这个客户端连接
            {
                internetConfig.connectState["lazer"] = TCPNetworkManage.namedClients[ClientNames.lazer].isConnected;
            }
            else
            {
                internetConfig.connectState["lazer"] = false;
            }
            if (TCPNetworkManage.namedClients.ContainsKey(ClientNames.scanner))  // 检查字典中是否存在这个客户端连接
            {
                internetConfig.connectState["scanner"] = TCPNetworkManage.namedClients[ClientNames.scanner].isConnected;
            }
            else
            {
                internetConfig.connectState["scanner"] = false;
            }
            if (TCPNetworkManage.namedClients.ContainsKey(ClientNames.mes))  // 检查字典中是否存在这个客户端连接
            {
                internetConfig.connectState["mes"] = TCPNetworkManage.namedClients[ClientNames.mes].isConnected;
            }
            else
            {
                internetConfig.connectState["mes"] = false;
            }

            internetConfig.connectState["ModbusTCP"] = ModbusTCPWorker.GetInstance().connect_state;
            if (internetConfig.connectState["camera1_Feed"] || internetConfig.connectState["camera1_Runner"])
            {
                BtnCamera1.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF60DA68"));
            }
            else
            {
                BtnCamera1.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#bcbfc9"));
            }
            if (internetConfig.connectState["scanner"])
            {
                BtnScanningGun.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF60DA68"));
            }
            else
            {
                BtnScanningGun.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#bcbfc9"));
            }
            if (internetConfig.connectState["lazer"])
            {
                BtnRangefinder.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF60DA68"));
            }
            else
            {
                BtnRangefinder.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#bcbfc9"));
            }
            if (internetConfig.connectState["camera2"])
            {
                BtnCamera2.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF60DA68"));
            }
            else
            {
                BtnCamera2.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#bcbfc9"));
            }
            if (internetConfig.connectState["camera3"])
            {
                BtnCamera3.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF60DA68"));
            }
            else
            {
                BtnCamera3.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#bcbfc9"));
            }
            if (internetConfig.connectState["ModbusTCP"])
            {
                BtnDevice.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF60DA68"));
            }
            else
            {
                BtnDevice.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#bcbfc9"));
            }
        }
        private void MainWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            ContentDisplay.Content = mainContent;  // MainScreen 是你定义的一个用户控件或界面
        }

        // 点击 "手动调试" 按钮
        private void ManualControlButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = manualControl; // ManualDebugScreen 是你定义的用户控件或界面
        }

        private void ParameterConfigButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = parameterConfig; // ManualDebugScreen 是你定义的用户控件或界面
        }
        private void PerformanceButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = performance; // ManualDebugScreen 是你定义的用户控件或界面
        }
        private void InternetConfigButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = internetConfig; // ManualDebugScreen 是你定义的用户控件或界面
        }
        private void DebugLogButton_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "手动调试" 内容
            ContentDisplay.Content = debugLog; // ManualDebugScreen 是你定义的用户控件或界面
        }


        //ResetButton 按住3秒才能触发
        private void ResetButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isResetButtonTriggered = false;
            resetPressStopwatch.Restart();

            resetTimer = new DispatcherTimer();
            resetTimer.Interval = TimeSpan.FromSeconds(1);
            resetTimer.Tick += (s, args) =>
            {
                resetTimer.Stop();
                isResetButtonTriggered = true;
                ExecuteReset();
            };
            resetTimer.Start();
        }

        private void ResetButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetTimer?.Stop();
            resetPressStopwatch.Stop();

            // 如果松开太早，提示用户
            if (!isResetButtonTriggered)
            {
                MessageBox.Show("请按住按钮至少3秒以执行复位");
            }
        }

        private async void StartAutoRun_Click(object sender, RoutedEventArgs e)
        {
            if (StateManager.Current.State == StateCode.IDLE && AutorunManager.Current.hasReseted == true) {
                StateManager.Current.State = StateCode.RUNNING;
                StateManager.Current.RunningStart = DateTime.Now;
                StateManager.Current.IdleEnd = DateTime.Now;
                StateManager.Current.Guarding = 1;
                //对轴初始化使能 改到登录之后            
                //AkrAction.Current.axisAllEnable(true);
                //GlobalManager.Current.InitializeAxisMode();

                //测试用
                GlobalManager.Current.isRun = true;
                StartAutoRunButton.IsEnabled = false;
                Logger.WriteLog("MainWindow.xaml.cs.StartAutoRun_Click() Start Autorun");
                try
                {
                    // 使用 Task.Run 来异步运行 AutoRunMain
                    
                    _cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken token = _cancellationTokenSource.Token;

                    await Task.Run(() => AutorunManager.Current.AutoRunMain(token));
                    if (AutorunManager.Current.isRunning)
                    {
                        //StartAutoRunButton.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error running AutoRunMain: {ex.Message}");
                }
            }

        }

        private void PauseAutoRun_Click(object sender, RoutedEventArgs e)
        {
            if (StateManager.Current.State == StateCode.RUNNING && GlobalManager.Current.IsPause == false)
            {
                GlobalManager.Current.IsPause = true;
                StateManager.Current.IdleStart = DateTime.Now;
                StateManager.Current.RunningEnd = DateTime.Now;
                StateManager.Current.State = StateCode.IDLE;
                //AutorunManager.Current.PauseAutoRun();  // 异步执行暂停
                //PauseAutoRunButton.Background = new SolidColorBrush(Colors.Yellow);
            }
            else if (StateManager.Current.State == StateCode.IDLE && GlobalManager.Current.IsPause == true)
            {
                GlobalManager.Current.IsPause = false;
                StateManager.Current.IdleEnd = DateTime.Now;
                StateManager.Current.RunningStart = DateTime.Now;
                StateManager.Current.State = StateCode.RUNNING;
                GlobalManager.Current.Lailiao_state[GlobalManager.Current.current_Lailiao_step] = 0;
                GlobalManager.Current.Lailiao_delta[GlobalManager.Current.current_Lailiao_step] = 0;
                GlobalManager.Current.Zuzhuang_state[GlobalManager.Current.current_Zuzhuang_step] = 0;
                GlobalManager.Current.Zuzhuang_delta[GlobalManager.Current.current_Zuzhuang_step] = 0;
                GlobalManager.Current.FuJian_state[GlobalManager.Current.current_FuJian_step] = 0;
                GlobalManager.Current.FuJian_delta[GlobalManager.Current.current_FuJian_step] = 0;
                //AutorunManager.Current.ResumeAutoRun();
                //PauseAutoRunButton.Background = new SolidColorBrush(Colors.Transparent);
            }
            else { 

            }
        }

        private void StopAutoRun_Click(object sender, RoutedEventArgs e)
        {
            if (StateManager.Current.State == StateCode.RUNNING)
            {
                StateManager.Current.StoppedStart = DateTime.Now;
                StateManager.Current.RunningEnd = DateTime.Now;
                StateManager.Current.State = StateCode.STOPPED;
                StateManager.Current.Guarding = 0;
                _cancellationTokenSource?.Cancel();

                AutorunManager.Current.StopAutoRun();
                StartAutoRunButton.IsEnabled = true;
            }
            else if (StateManager.Current.State == StateCode.MAINTENANCE)
            {
                StateManager.Current.StoppedStart = DateTime.Now;
                StateManager.Current.MaintenanceEnd = DateTime.Now;
                StateManager.Current.State = StateCode.STOPPED;
                StateManager.Current.Guarding = 0;
                _cancellationTokenSource?.Cancel();

                AutorunManager.Current.StopAutoRun();
                StartAutoRunButton.IsEnabled = true;
            }
            else {
                _cancellationTokenSource?.Cancel();
                return;
            }
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

        private void ExecuteReset()
        {
            if (StateManager.Current.State == StateCode.STOPPED)
            {
                StateManager.Current.StoppedEnd = DateTime.Now;
                StateManager.Current.IdleStart = DateTime.Now;
                StateManager.Current.State = StateCode.IDLE;
            }
            else if (StateManager.Current.State == StateCode.MAINTENANCE)
            {
                StateManager.Current.MaintenanceEnd = DateTime.Now;
                StateManager.Current.IdleStart = DateTime.Now;
                StateManager.Current.State = StateCode.IDLE;
            }
            else if (StateManager.Current.State == StateCode.IDLE)
            {

            }
            else
            {
                return;
            }
            MessageBox.Show("开始复位");
            if (!AutorunManager.Current.Reset())
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("复位失败");
                });
                AutorunManager.Current.hasReseted = false;
            }
            else
            {
                AutorunManager.Current.hasReseted = true;
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalManager.Current.current_Lailiao_step = 0;
            GlobalManager.Current.current_Zuzhuang_step = 0;
            GlobalManager.Current.current_FuJian_step = 0;
            LaiLiao.Current.board_count = 0;
            //ZuZhuang.Current.has_board = false;
            //FuJian.Current.has_board = false;
            GlobalManager.Current.Lailiao_exit = false;
            GlobalManager.Current.Zuzhuang_exit = false;
            GlobalManager.Current.FuJian_exit = false;
            AutorunManager.Current.hasReseted = true;
            //button.PromptCount += 1;

            //20250512

            //AAMotionAPI.MotorOn(GlobalManager.Current._Agm800.controller, AxisRef.A);
            //AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, AxisRef.A, -1000000);
            //while (GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).InTargetStat != 4)
            //{
            //    Thread.Sleep(50);
            //}

            //AAMotionAPI.MotorOn(GlobalManager.Current._Agm800.controller, AxisRef.B);
            //AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, AxisRef.B, 0);
            //while (GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).InTargetStat != 4)
            //{
            //    Thread.Sleep(50);
            //}

            //20250512


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(container);
            layer.Add(new PromptAdorner(button));
        }


        private async void IdleButton_Click(object sender, RoutedEventArgs e)
        {
            if (StateManager.Current.State == StateCode.RUNNING)
            {
                //要板信号置0
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);
                //当前设备中的板运行结束
                await Task.Run(() =>
                {
                    StateManager.Current.DetectRemainBoard();
                });
                StateManager.Current.IdleStart = DateTime.Now;
                StateManager.Current.RunningEnd = DateTime.Now;
                StateManager.Current.State = StateCode.IDLE;
            }
        }

        private async void MaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (StateManager.Current.State == StateCode.RUNNING)
            {
                //要板信号置0
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT7_0MACHINE_READY_TO_RECEIVE, 0);
                //当前设备中的板运行结束
                await Task.Run(() =>
                {
                    StateManager.Current.DetectRemainBoard();
                });
                StateManager.Current.MaintenanceStart = DateTime.Now;
                StateManager.Current.RunningEnd = DateTime.Now;
                StateManager.Current.State = StateCode.MAINTENANCE;
                StateManager.Current.Guarding = 0;
            }
            else if (StateManager.Current.State == StateCode.IDLE)
            {
                StateManager.Current.MaintenanceStart = DateTime.Now;
                StateManager.Current.IdleEnd = DateTime.Now;
                StateManager.Current.State = StateCode.MAINTENANCE;
                StateManager.Current.Guarding = 0;
            }
        }

        private void Alarmbutton_Click(object sender, RoutedEventArgs e)
        {
            //ErrorWindow = new ErrorWindow();
            //ErrorWindow.Show();

            //Modify By YXW
            if (ErrorWindow == null)
            {
                ErrorWindow = new ErrorWindow();
                ErrorWindow.Closed += (s, args) => ErrorWindow = null; //窗口关闭时清空引用


                // 手动设置居中（相对于屏幕）
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                var windowWidth = ErrorWindow.Width;
                var windowHeight = ErrorWindow.Height;

                ErrorWindow.Left = (screenWidth - windowWidth) / 2;
                ErrorWindow.Top = (screenHeight - windowHeight) / 2;

                ErrorWindow.Show();
            }
            else
            {
                if (ErrorWindow.WindowState == WindowState.Minimized)
                {
                    ErrorWindow.WindowState = WindowState.Normal;
                }
                ErrorWindow.Activate(); // 激活已有窗口
            }
        }

        private void BtnDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnScanningGun_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRangefinder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCamera1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCamera2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCamera3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;

            ModbusTCPWorker.GetInstance().Read_Coil((int)IO_INFunction_Table.IN1_0Slowdown_Sign1, ref result);

            bool result2 = result;
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
