using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Util;
using AkribisFAM.ViewModel;
using AkribisFAM.Windows;
using AkribisFAM.WorkStation;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.Manager.StateManager;
using static AkribisFAM.WorkStation.AkrAction;
using static AkribisFAM.WorkStation.Conveyor;

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
        HardwareControl hardwareControl;
        ParameterConfig parameterConfig;
        Performance performance;
        InternetConfig internetConfig;
        DebugLog debugLog;

        public MainWindow()
        {
            InitializeComponent();

            App.buzzer.EnableBeep = true;

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
            hardwareControl = new HardwareControl();
            performance = new Performance();
            parameterConfig = new ParameterConfig();
            internetConfig = new InternetConfig();
            debugLog = new DebugLog();
            ContentDisplay.Content = mainContent;
            Logger.WriteLog("MainWindow init");
            _timer.Start();
            lblVersion.Content = $"Version v {Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
            //END Add
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

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
            button.PromptCount = ErrorManager.Current.ErrorCnt;
            NowState.Content = StateManager.Current.StateDict[StateManager.Current.State];
            if (StateManager.Current.State == StateCode.RUNNING)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_8Run_light, 1);
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_9Stop_light, 0);
                StateManager.Current.RunningTime = DateTime.Now - StateManager.Current.RunningStart;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {

                    performance.RunningTimeLB.Content = StateManager.Current.RunningTime.ToString(@"hh\:mm\:ss");
                }));
            }
            else
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_8Run_light, 0);
            }
            if (StateManager.Current.State == StateCode.STOPPED)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_9Stop_light, 1);
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
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_9Stop_light, 1);
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
                double availability = (StateManager.Current.IdleTime.TotalSeconds + StateManager.Current.RunningTime.TotalSeconds) / (StateManager.Current.IdleTime.TotalSeconds + StateManager.Current.RunningTime.TotalSeconds + StateManager.Current.MaintenanceTime.TotalSeconds + StateManager.Current.StoppedTime.TotalSeconds);
                performance.AVAILABILITYLB.Content = availability.ToString("0.000");
                if (StateManager.Current.RunningTime.TotalSeconds > 0.5)
                {
                    double perform = StateManager.Current.TotalInput / (StateManager.Current.RunningTime.TotalSeconds / 1200.0);
                    performance.PERFORMANCELB.Content = perform.ToString("0.000");
                }
                if (StateManager.Current.TotalOutputOK + StateManager.Current.TotalOutputNG > 0)
                {
                    double quality = StateManager.Current.TotalOutputOK / (StateManager.Current.TotalOutputOK + StateManager.Current.TotalOutputNG);
                    performance.QUALITYLB.Content = quality.ToString("0.000");
                }
                if (StateManager.Current.RunningTime.TotalSeconds > 3600 + StateManager.Current.RunningHourCnt * 3600)
                {
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
                        Yield = (int)UPH * 100 / (UPH + NG);
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
                if (ErrorManager.Current.IsAlarm)
                {
                    if (!App.buzzer.BeepStatus && App.buzzer.EnableBeep)
                    {
                        App.buzzer.BeepOn();
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
                }
                if (IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_12Reset))
                {
                    ErrorManager.Current.Clear();
                }
                //button panel
                BlinkLightFeeder1();
                BlinkLightFeeder2();
                ControlButton();
            }));
        }

        private void ControlButton()
        {
            if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN5_8Run] == 0)
            {
                StartAutoRun_Click(StartAutoRunButton, new RoutedEventArgs());
            }
            if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN5_9Stop] == 0)
            {
                PauseAutoRun_Click(PauseAutoRunButton, new RoutedEventArgs());
            }
            if (AutorunManager.Current.hasReseted == true)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_12Reset_light, 1);
            }
            else
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_12Reset_light, 0);
            }
        }

        private void BlinkLightFeeder1()
        {
            if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_12Feeder1_drawer_InPos] == 0 && IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_8Feeder1_limit_cylinder_extend_InPos] == 1)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_10Feeder1_light, 1);
                if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN5_10Feeder1] == 0)
                {
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_0Feeder1_limit_cylinder_extend, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_1Feeder1_limit_cylinder_retract, 0);
                }
            }
            else if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_12Feeder1_drawer_InPos] == 1)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_10Feeder1_light, 0);
            }
            else if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_12Feeder1_drawer_InPos] == 0 && IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_8Feeder1_limit_cylinder_extend_InPos] == 0)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_10Feeder1_light, 0);
                if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN5_10Feeder1] == 0)
                {
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_0Feeder1_limit_cylinder_extend, 0);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_1Feeder1_limit_cylinder_retract, 1);
                }
            }
        }

        private void BlinkLightFeeder2()
        {
            if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_13Feeder2_drawer_InPos] == 0 && IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_10Feeder2_limit_cylinder_extend_InPos] == 1)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_11Feeder2_light, 1);
                if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN5_11Feeder2] == 0)
                {
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_2Feeder2_limit_cylinder_extend, 1);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_3Feeder2_limit_cylinder_retract, 0);
                }
            }
            else if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_13Feeder2_drawer_InPos] == 1)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_11Feeder2_light, 0);
            }
            else if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_13Feeder2_drawer_InPos] == 0 && IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_10Feeder2_limit_cylinder_extend_InPos] == 0)
            {
                IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT6_11Feeder2_light, 0);
                if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN5_11Feeder2] == 0)
                {
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_2Feeder2_limit_cylinder_extend, 0);
                    IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT5_3Feeder2_limit_cylinder_retract, 1);
                }
            }
        }

        private void ConnectState()
        {
            TCPNetworkManage.CheckClients();
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
            ContentDisplay.Content = hardwareControl; // ManualDebugScreen 是你定义的用户控件或界面
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

        private int TriggerLaser(int count)
        {
            try
            {
                List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend> AcceptKDistanceAppend = new List<KEYENCEDistance.Acceptcommand.AcceptKDistanceAppend>();

                Thread.Sleep(GlobalManager.Current.LaserHeightDelay);

                if (!Task_KEYENCEDistance.SendMSData()) return (int)ErrorCode.Laser_Failed;
                //得到测量结果
                AcceptKDistanceAppend = Task_KEYENCEDistance.AcceptMSData();

                var res = AcceptKDistanceAppend[0].MeasurData;

                Logger.WriteLog("激光测距结果:" + res);

                double height = AkribisFAM.Util.Parser.TryParseTwoValues("=" + res);

                return 0;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("激光测距报错 : " + ex.ToString());
                return (int)ErrorCode.Laser_Failed;
            }
        }
        private void TestFeiPai_Click(object sender, RoutedEventArgs e)
        {
            var arr1 = new object[] { AxisName.LSX, 150, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX };
            var arr2 = new object[] { AxisName.LSY, 150, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY };

            int moveToPointX = MoveView.MovePTP(arr1, arr2);
            Thread.Sleep(300);
            var arr3 = new object[] { AxisName.LSX };
            var arr4 = new object[] { AxisName.LSY };
            int b = MoveView.WaitAxisArrived(arr3, arr4);








            // 力控

            //AkrAction.Current.MoveFoamZ2(20);
            ////TODO 改到程序打开的时候执行一次
            //AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AProgRun[1]=1", out string response45);
            //Thread.Sleep(100);

            //pick1的Z轴
            //// AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[101]=1000", out string response44);
            //// Thread.Sleep(100);
            //// AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[102]=5000", out string response123);
            ////Thread.Sleep(50);
            //// AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[800]=2", out string response4);

            //pick2的Z轴
            // AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[201]=2000", out string response4455);
            //Thread.Sleep(100);
            //AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[202]=5000", out string response22);
            //Thread.Sleep(50);
            //AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[800]=3", out string response54);

            //while (true)
            //{
            //    AAmotionFAM.AGM800.Current.controller[2].SendCommandString("AGenData[203]", out string response);
            //    if (response.Equals("1"))
            //    {
            //        break;
            //    }
            //    Thread.Sleep(500);
            //}

            //AkrAction.Current.Move(AxisName.PICK2_Z);


            //-------

            //Task_KEYENCEDistance.SendResetData();
            ////var a = Task_KEYENCEDistance.AcceptMSData()[0];

            //int count = 0;
            //foreach (var point in GlobalManager.Current.laserPoints)
            //{
            //    if (count % 4 == 0)
            //    {
            //        var arr1 = new object[] { AxisName.LSX, (int)point.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX };
            //        var arr2 = new object[] { AxisName.LSY, (int)point.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY };

            //        int moveToPoint = MoveView.MovePTP(arr1,arr2);
            //        MoveView.WaitAxisArrived(new object[] { AxisName.LSX , AxisName.LSY });

            //        //AkrAction.Current.MoveLaserXY (int)point.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX);
            //        //Thread.Sleep(20);
            //        //Logger.WriteLog("111111aaaa");
            //        //AkrAction.Current.Move(AxisName.LSY, (int)point.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY);
            //        int laserProc = TriggerLaser(count);

            //        count++;
            //    }
            //    if (count % 4 == 1)
            //    {
            //        var arr1 = new object[] { AxisName.LSX, (int)point.X + GlobalManager.Current.laserpoint1_shift_X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX };
            //        var arr2 = new object[] { AxisName.LSY, (int)point.Y, GlobalManager.Current.laserpoint1_shift_Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY };

            //        int moveToPoint = MoveView.MovePTP(arr1);
            //        MoveView.WaitAxisArrived(new object[] { AxisName.LSX});
            //        TriggerLaser(count);

            //        count++;
            //    }
            //    if (count % 4 == 2)
            //    {
            //        var arr1 = new object[] { AxisName.LSX, (int)point.X + GlobalManager.Current.laserpoint2_shift_X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX };
            //        var arr2 = new object[] { AxisName.LSY, (int)point.Y, GlobalManager.Current.laserpoint2_shift_Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY };

            //        MoveView.MovePTP(arr1);
            //        MoveView.WaitAxisArrived(new object[] { AxisName.LSX });
            //        TriggerLaser(count);

            //        count++;
            //    }
            //    if (count % 4 == 3)
            //    {
            //        var arr1 = new object[] { AxisName.LSX, (int)point.X + GlobalManager.Current.laserpoint3_shift_X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX, (int)AxisAcc.LSX };
            //        var arr2 = new object[] { AxisName.LSY, (int)point.Y, GlobalManager.Current.laserpoint3_shift_Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY, (int)AxisAcc.LSY };

            //        MoveView.MovePTP(arr1);
            //        MoveView.WaitAxisArrived(new object[] { AxisName.LSX });
            //        TriggerLaser(count);

            //        count++;
            //    }
            //    Thread.Sleep(1000000);

            //}
        }


        //ResetButton 按住3秒才能触发
        private void ResetButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //isResetButtonTriggered = false;
            //resetPressStopwatch.Restart();

            //resetTimer = new DispatcherTimer();
            //resetTimer.Interval = TimeSpan.FromSeconds(1);
            //resetTimer.Tick += (s, args) =>
            //{
            //    resetTimer.Stop();
            //    isResetButtonTriggered = true;
            //    ExecuteReset();
            //};
            //resetTimer.Start();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorManager.Current.Clear();
            App.buzzer.EnableBeep = true;
        }
        private void ResetButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resetTimer?.Stop();
            resetPressStopwatch.Stop();

            // 如果松开太早，提示用户
            if (!isResetButtonTriggered)
            {
                MessageBox.Show("Please hold down the button for at least 3 seconds to perform a reset!");
            }
        }
        public bool PreRunCheck()
        {

            var res1 = App.CioManager.IsSSR1Ok;
            var res2 = App.CioManager.IsEmergencyStopOk;
            if (!App.CioManager.IsEmergencyStopOk)
            {
                MessageBox.Show("E-Stop triggered!");
                return false;
            }

            if (!App.CioManager.IsSSR1Ok)
            {
                MessageBox.Show("E-Stop triggered!");
                return false;
            }
            if (!App.CioManager.IsSSR2Ok)
            {
                MessageBox.Show("Door SSR is not reseted");
                return false;
            }
            if (!App.CioManager.IsMainAirOn)
            {
                MessageBox.Show("No air supply");
                return false;
            }
            if (!App.door.IsDoor1Locked)
            {
                MessageBox.Show("Door 1 is not locked");
                return false;
            }
            if (!App.door.IsDoor2Locked)
            {
                MessageBox.Show("Door 2 is not locked");
                return false;
            }
            if (!App.door.IsDoor3Locked)
            {
                MessageBox.Show("Door 3 is not locked");
                return false;

            }
            if (!App.door.IsDoor4Locked)
            {
                MessageBox.Show("Door 4 is not locked");
                return false;
            }
            if (!App.reject.IsAllCoversClosed())
            {
                MessageBox.Show("Cover is not closed");
                return false;
            }


            if (!App.filmRemoveGantryControl.VacOff())
            {
                MessageBox.Show("Failed to off air");
                return false;
            }

            if (ErrorManager.Current.IsAlarm)
            {
                MessageBox.Show("Please acknowledge alarm first");
                return false;
            }

            #region Devices connectivity

            if (!AAmotionFAM.AGM800.Current.controller.All(x => x.IsConnected))
            {
                for (int i = 0; i < AAmotionFAM.AGM800.Current.controller.Count(); i++)
                {
                    MessageBox.Show($"Agito controller number {i} is offline");
                }
                return false;
            }
            if (!TCPNetworkManage.IsAllConnected)
            {
                var clients = TCPNetworkManage.GetDisconnectedClients();
                foreach (var client in clients)
                {
                    MessageBox.Show($"Device {client.Key} failed is offline");
                }
                return false;
            }
            #endregion


            #region motor
            if (!(AkrAction.Current.CheckAllAxisHomeCompleted(out bool res) == (int)ACTTION_ERR.NONE && res == true))
            {
                MessageBox.Show("Please home first before start process");
                return false;
            }

            for (int i = (int)DeviceClass.AssemblyGantryControl.Picker.Picker1; i <= (int)DeviceClass.AssemblyGantryControl.Picker.Picker4; i++)
            {
                DeviceClass.AssemblyGantryControl.Picker picker = (DeviceClass.AssemblyGantryControl.Picker)i;
                if (!App.assemblyGantryControl.IsPtpMode(picker))
                {
                    MessageBox.Show($"Picker {i} is not in PTP mode");
                    if (App.assemblyGantryControl.SetPositionMode(picker) && App.assemblyGantryControl.IsPtpMode(picker))
                    {
                        MessageBox.Show($"Picker {i} failed to setPTP mode");
                        return false;
                    }
                }
            }
            if (AkrAction.Current.EnableAllMotors(true) != (int)ACTTION_ERR.NONE)
            {
                MessageBox.Show("Failed to enable motor");
                return false;
            }


            if (AkrAction.Current.MoveFoamZ1Z2Z3Z4(0, 0, 0, 0) != 0)
            {
                MessageBox.Show("Pickers failed to return 0");
                return false;
            }

            if (AkrAction.Current.MoveRecheckZ(0) != 0)
            {
                MessageBox.Show("Reject failed to return 0");
                return false;
            }

            #endregion
            //if (App.feeder1.IsDrawerInPos && App.feeder1.IsLock)
            //{
            //    MessageBox.Show("Feeder 1 is not lock or not in position");
            //    return false;
            //}
            //if (App.feeder2.IsDrawerInPos && App.feeder2.IsLock)
            //{
            //    MessageBox.Show("Feeder 2 is not lock or not in position");
            //    return false;
            //}

         
            #region Feeder
            if (!App.feeder1.IsInitialized || !App.feeder1.IsAlarm)
            {
                if (!App.feeder1.ClearError())
                {
                    MessageBox.Show("Failed to clear feeder 1 alarm ");
                }
            }
            if (!App.feeder2.IsInitialized || !App.feeder2.IsAlarm)
            {
                if (!App.feeder2.ClearError())
                {
                    MessageBox.Show("Failed to clear feeder 2 alarm");
                }
            }
            if (!(App.feeder1.IsDrawerInPos && App.feeder1.IsLock))
            {
                if (App.feeder1.IsDrawerInPos)
                {
                    App.feeder1.Lock();
                    if (!App.feeder1.IsLock)
                    {
                        MessageBox.Show("Failed to lock feeder 1");
                        return false;
                    }
                }
            }
            if (!(App.feeder2.IsDrawerInPos && App.feeder2.IsLock))
            {
                if (App.feeder2.IsDrawerInPos)
                {
                    App.feeder2.Lock();
                    if (!App.feeder2.IsLock)
                    {
                        MessageBox.Show("Failed to lock feeder 2");
                        return false;
                    }
                }
            }
            if (!(App.feeder1.IsDrawerInPos && App.feeder1.IsLock) && !(App.feeder2.IsDrawerInPos && App.feeder2.IsLock))
            {
                MessageBox.Show("Please at least lock one feeder and feed in material");
                return false;
            }
  
            #endregion
            return true;
        }
        public bool PreruncheckComplete()
        {

            App.buzzer.Warn();


            AutorunManager.Current.IsPause = false;
            AutorunManager.Current.IsError = false;
            AutorunManager.Current.IsReset = true;

            return true;
        }
        private void StartAutoRun_Click(object sender, RoutedEventArgs e)
        {
            Logger.WriteLog("Start Button is clicked.");



            if (StateManager.Current.State == StateCode.IDLE && AutorunManager.Current.hasReseted == true || true)
            {
                Logger.WriteLog("Change from idle to running.");


                if (!PreRunCheck())
                {
                    return;
                }
                if (!PreruncheckComplete())
                {
                    return;
                }

                StateManager.Current.State = StateCode.RUNNING;
                StateManager.Current.RunningStart = DateTime.Now;
                StateManager.Current.IdleEnd = DateTime.Now;
                StateManager.Current.Guarding = 1;
                //对轴初始化使能 改到登录之后            
                //AkrAction.Current.EnableAllMotors(true);
                //GlobalManager.Current.InitializeAxisMode();

                GlobalManager.Current.flag_NGStationAllowTrayEnter = 1;

                //测试用
                GlobalManager.Current.isRun = true;
                AutorunManager.Current.IsPause = false;
                //StartAutoRunButton.IsEnabled = false;
                Logger.WriteLog("MainWindow.xaml.cs.StartAutoRun_Click() Start Autorun");
                try
                {
                    AutorunManager.Current.StartAutoRunThreads();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error running AutoRunMain: {ex.Message}");
                }
            }

        }

        private void PauseAutoRun_Click(object sender, RoutedEventArgs e)
        {
            Logger.WriteLog("Pause Button is clicked.");
            if (StateManager.Current.State == StateCode.RUNNING && AutorunManager.Current.IsPause == false)
            {
                Logger.WriteLog("Change from running to idle.");
                AutorunManager.Current.IsPause = true;
                StateManager.Current.IdleStart = DateTime.Now;
                StateManager.Current.RunningEnd = DateTime.Now;
                StateManager.Current.State = StateCode.IDLE;
                //AutorunManager.Current.PauseAutoRun();  // 异步执行暂停
                //PauseAutoRunButton.Background = new SolidColorBrush(Colors.Yellow);
            }
            else if (StateManager.Current.State == StateCode.IDLE && AutorunManager.Current.IsPause == true)
            {
                Logger.WriteLog("Change from idle to running.");
                AutorunManager.Current.IsPause = false;
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
            else
            {

            }
        }

        private async void TestBoardIn_Click(object sender, RoutedEventArgs e)
        {
            var a = GlobalManager.Current.stationPoints;
            GlobalManager.Current.IO_test1 = true;
            TestBoardIn.IsEnabled = false;

            // 等待 1 秒而不阻塞 UI 线程
            await Task.Delay(1000);

            TestBoardIn.IsEnabled = true;
        }

        //private void TestBoardIn_Click(object sender, RoutedEventArgs e)
        //{
        //    var a = GlobalManager.Current.stationPoints;
        //    GlobalManager.Current.IO_test1 = true;
        //    TestBoardIn.IsEnabled = false;
        //    Thread.Sleep(1000);
        //    TestBoardIn.IsEnabled = true;
        //    GlobalManager.Current.IO_test1 = false;
        //}

        private void StopAutoRun_Click(object sender, RoutedEventArgs e)
        {
            Logger.WriteLog("Stop Button is clicked.");
            if (StateManager.Current.State == StateCode.RUNNING)
            {
                Logger.WriteLog("Change from running to stopped.");
                StateManager.Current.StoppedStart = DateTime.Now;
                StateManager.Current.RunningEnd = DateTime.Now;
                StateManager.Current.State = StateCode.STOPPED;
                StateManager.Current.Guarding = 0;
                _cancellationTokenSource?.Cancel();
                AkrAction.Current.StopAllAxis();
                AkrAction.Current.EnableAllMotors(false);
                AutorunManager.Current.StopAutoRun();
                //StartAutoRunButton.IsEnabled = true;
            }
            else if (StateManager.Current.State == StateCode.MAINTENANCE)
            {
                Logger.WriteLog("Change from maintence to stopped.");
                StateManager.Current.StoppedStart = DateTime.Now;
                StateManager.Current.MaintenanceEnd = DateTime.Now;
                StateManager.Current.State = StateCode.STOPPED;
                StateManager.Current.Guarding = 0;
                _cancellationTokenSource?.Cancel();
                AkrAction.Current.StopAllAxis();
                AkrAction.Current.EnableAllMotors(false);
                AutorunManager.Current.StopAutoRun();
                //StartAutoRunButton.IsEnabled = true;
            }
            else
            {
                Logger.WriteLog("Change from idle to stopped.");
                StateManager.Current.StoppedStart = DateTime.Now;
                StateManager.Current.IdleEnd = DateTime.Now;
                StateManager.Current.State = StateCode.STOPPED;
                StateManager.Current.Guarding = 0;
                AkrAction.Current.StopAllAxis();
                AkrAction.Current.EnableAllMotors(false);
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

        private async void ExecuteReset()
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
            MessageBox.Show("Start Reseting");
            bool resetResult = await Task.Run(() => AutorunManager.Current.Reset());
            if (!resetResult)
            {
                AkrAction.Current.StopAllAxis();
                AkrAction.Current.EnableAllMotors(false);
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Reseting Failed!");
                });
                AutorunManager.Current.hasReseted = false;
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Reseting Successfully!");
                });
                AutorunManager.Current.hasReseted = true;
                GlobalManager.Current.Lailiao_exit = false;
                GlobalManager.Current.Zuzhuang_exit = false;
                GlobalManager.Current.FuJian_exit = false;
                GlobalManager.Current.Reject_exit = false;
                GlobalManager.Current.current_Lailiao_step = 0;
                GlobalManager.Current.current_Zuzhuang_step = 0;
                GlobalManager.Current.current_FuJian_step = 0;
                GlobalManager.Current.current_Reject_step = 0;
                IOManager.Instance.OutIO_status[(int)IO_OutFunction_Table.OUT6_12Reset_light] = 0;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(container);
            layer.Add(new PromptAdorner(button));

            DoorIcon.Kind = App.door.IsAllLockTriggered ? PackIconKind.LockOutline : PackIconKind.LockOpenVariantOutline;
            btnLock.Background = App.door.IsAllLockTriggered ? Brushes.Gray : Brushes.Gold;

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AutorunManager.CancelToken.Cancel();
        }

        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            //ErrorManager.Current.Insert(ErrorCode.IOErr, "this is weird");
            //if (App.door.IsAllDoorLocked)
            if (App.door.IsAllLockTriggered)
            {
                if (!App.door.UnlockAll())
                {
                    MessageBox.Show("Failed to unlock");
                }
                else
                {
                    //btnLock.Content = "Lock door";
                    DoorIcon.Kind = PackIconKind.LockOpenVariantOutline;
                    btnLock.Background = Brushes.Gold;
                }
            }
            else
            {
                if (!App.door.LockAll())
                {
                    MessageBox.Show("Failed to lock");
                }
                else
                {
                    //btnLock.Content = "Unlock door";
                    DoorIcon.Kind = PackIconKind.LockOutline;
                    btnLock.Background = Brushes.Gray;
                }
            }
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
