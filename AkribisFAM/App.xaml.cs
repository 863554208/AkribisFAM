using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using AAMotion;
using AkribisFAM.Manager;
using AkribisFAM.Windows;
using AkribisFAM.CommunicationProtocol;
using Newtonsoft.Json.Linq;
using static AkribisFAM.Manager.StateManager;
using AkribisFAM.Interfaces;
using System.IO;
using AkribisFAM.DeviceClass;
using AkribisFAM.WorkStation;

namespace AkribisFAM
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static IDatabaseManager DbManager { get; private set; }
        public static DirectoryManager DirManager;
        public static CriticalIOManager CioManager;

        public static RecipeManager recipeManager;
        public static KeyenceLaserControl laser;
        public static CognexVisionControl vision1;
        public static AssemblyGantryControl assemblyGantryControl;
        public static FilmRemoveGantryControl filmRemoveGantryControl;
        public static FeederControl feeder1;
        public static FeederControl feeder2;
        public static CognexBarcodeScanner scanner;
        public static LoadCellCalibration calib;
        public static RejectControl reject;
        public static BuzzerControl buzzer;
        public static DoorControl door;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var _globalManager = GlobalManager.Current;
            var _warningManager = WarningManager.Current;
            var _agm800 = AAmotionFAM.AGM800.Current;

            TCPNetworkManage.TCPInitialize();
            StateManager.Current.DetectTimeDeltaThread();
            //启动与AGM800的连接
            StartConnectAGM800();

            ModbusTCPWorker.GetInstance().Connect();
            IOManager.Instance.ReadIO_loop();


            //调试用
            StateManager.Current.State = StateCode.IDLE;
            StateManager.Current.StateLightThread();
            DirManager = new DirectoryManager();
            DbManager = new DatabaseManager(Path.Combine(DirManager.GetDirectoryPath(DirectoryType.Database), "Alpha_FAM_Database.sqlite"));

            recipeManager = new RecipeManager();
            laser = new KeyenceLaserControl();
            vision1 = new CognexVisionControl();
            feeder1 = new FeederControl(1);
            feeder2 = new FeederControl(2);
            scanner = new CognexBarcodeScanner();
            assemblyGantryControl = new AssemblyGantryControl();
            filmRemoveGantryControl = new FilmRemoveGantryControl();
            buzzer = new BuzzerControl();
            CioManager = new CriticalIOManager();
            filmRemoveGantryControl.XOffset = 25.4;
            filmRemoveGantryControl.YOffset = 56.3;
            calib = new LoadCellCalibration();
            door = new DoorControl();
            reject = new RejectControl();
            App.assemblyGantryControl.BypassPicker4 = true;
            App.assemblyGantryControl.BypassPicker3 = true;
            //TODO
            //try
            //{
            //    // 初始化数据库连接
            //    DatabaseManager.Initialize();

            //20250530 测试mes的tcp连接 【史彦洋】 Start

            //20250530 测试mes的tcp连接 【史彦洋】 End

            SetLanguage("en-US");


            if (new LoginViewModel().ShowDialog() == true)
            {
                new MainWindow().ShowDialog();
            }

            //关闭与AGM800进行通讯的AACommonServer进程
            CloseAACommServer();
            //关闭主进程
            Application.Current.Shutdown();
        }

        private static void SetLanguage(string culture)
        {
            // 设置当前线程的文化信息
            CultureInfo cultureInfo = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        private void StartConnectAGM800()
        {
            try
            {
                string[] agm800_IP = new string[]
                {
                    "172.1.1.105",
                    "172.1.1.102",
                    "172.1.1.103",
                    "172.1.1.104"
                };

                // 初始化控制器并连接到指定的 IP 地址
                for (int i = 0; i < AAmotionFAM.AGM800.Current.controller.Length; i++)
                {
                    AAmotionFAM.AGM800.Current.controller[i] = AAMotionAPI.Initialize(ControllerType.AGM800);
                    AAMotionAPI.Connect(AAmotionFAM.AGM800.Current.controller[i], agm800_IP[i]);

                }
            }
            catch (Exception ex) { }

        }
        private void CloseAACommServer()
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName("AACommServer");
                foreach (var proc in processes)
                {
                    proc.Kill();
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                // 如果失败可以记录日志或忽略
                MessageBox.Show($"关闭 AACommServer 失败: {ex.Message}", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void LoadLaserPoints()
        {
            try
            {
                string filePath = "D:\\akribisfam_config\\scanpoints.json";
                string jsonString = System.IO.File.ReadAllText(filePath);
                var json = JObject.Parse(jsonString);
                var flatList = new List<(double X, double Y)>();
                foreach (var prop in json.Properties())
                {
                    if (prop.Name.StartsWith("module"))
                    {
                        var module = prop.Name;
                        var points = (JObject)prop.Value;

                        foreach (var pointProp in points.Properties())
                        {
                            var point = pointProp.Name;
                            var coords = pointProp.Value;

                            double x = coords["X"].Value<double>();
                            double y = coords["Y"].Value<double>();
                            double z = coords["Z"].Value<double>();

                            flatList.Add((x, y));
                        }
                    }
                }
                //GlobalManager.Current.laserPoints = flatList;
            }
            catch { }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            TCPNetworkManage.StopAllClients();
            // Dispose of resources
            DbManager?.Dispose();

            base.OnExit(e); // Always call the base
        }

    }
}
