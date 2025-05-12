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
            var _testStation1 = TestStation1.Current;
            var _testStation2 = TestStation2.Current;
            var _warningManager = WarningManager.Current;

            TCPNetworkManage.TCPInitialize();
            //启动与AGM800的连接
            StartConnectAGM800();

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
            string agm800_IP = "172.1.1.101";            
            GlobalManager.Current.AGM800Connection = AAMotionAPI.Connect(GlobalManager.Current._Agm800.controller, agm800_IP);
        }
        private void CloseAACommServer()
        {
            try
            {
                // 获取所有名为 "AACommServer" 的进程（去掉 .exe）
                var processes = System.Diagnostics.Process.GetProcessesByName("AACommServer");
                foreach (var proc in processes)
                {
                    proc.Kill();       // 强制终止进程
                    proc.WaitForExit(); // 等待它完全退出
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
