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
            StateManager.Current.DetectTimeDeltaThread();
            //启动与AGM800的连接
            StartConnectAGM800();

            ModbusTCPWorker.GetInstance().Connect();
            IOManager.Instance.ReadIO_status();

            //调试用
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend, 0);
            IOManager.Instance.IO_ControlStatus(IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract, 0);

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
