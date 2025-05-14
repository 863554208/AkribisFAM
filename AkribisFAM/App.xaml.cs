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
                string agm800_IP0 = "172.1.1.101";
                string agm800_IP1 = "172.1.1.102";
                string agm800_IP2 = "172.1.1.103";
                string agm800_IP3 = "172.1.1.104";
                GlobalManager.Current.AGM800Connection = AAMotionAPI.Connect(GlobalManager.Current._Agm800.controller, agm800_IP0);

                //20250514 增加多个AGM800的控制 【史彦洋】 追加 Start
                AAMotionAPI.Connect(GlobalManager.Current._Agm800.controller1, agm800_IP1);
                //AAMotionAPI.Connect(GlobalManager.Current._Agm800.controller1, agm800_IP2);
                //AAMotionAPI.Connect(GlobalManager.Current._Agm800.controller1, agm800_IP3);
                //20250514 增加多个AGM800的控制 【史彦洋】 追加 End
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
