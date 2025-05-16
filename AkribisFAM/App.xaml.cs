using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AAMotion;
using AkribisFAM.DB;
using AkribisFAM.Manager;
using AkribisFAM.Windows;
using AkribisFAM.WorkStation;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.AAmotionFAM;
using static AkribisFAM.GlobalManager;
namespace AkribisFAM
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var _globalManager = GlobalManager.Current;
            var _warningManager = WarningManager.Current;
            var _agm800 = AAmotionFAM.AGM800.Current;

            TCPNetworkManage.TCPInitialize();
            //AkrAction.Current.axisAllHome("D:\\akribisfam_config\\HomeFile");
            StateManager.Current.DetectTimeDeltaThread();
            //启动与AGM800的连接
            StartConnectAGM800();


            ModbusTCPWorker.GetInstance().Connect();
            IOManager.Instance.ReadIO_status();

            //调试用


            //TODO
            try
            {
                // 初始化数据库连接
                DatabaseManager.Initialize();

                // 插入数据
                DatabaseManager.Insert("MyDatabase.db");

                Console.WriteLine("数据插入成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作失败: {ex.Message}");
            }
            finally
            {
                // 关闭数据库连接
                DatabaseManager.Shutdown();
            }

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
                    "172.1.1.101",
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


    }
}
