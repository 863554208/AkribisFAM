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
using AkribisFAM.Windows;
using AkribisFAM.WorkStation;
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

            SetLanguage("zh-CHS");

            //在启动程序时就开始跟AGM800的通信
            StartConnectAGM800();

            //对轴初始化使能
            InitializeAxis();


            if (new LoginViewModel().ShowDialog() == true)
            {
                new MainWindow().ShowDialog();
            }

            //关闭与AGM800进行通讯的AACommonServer进程
            CloseAACommServer();
            //关闭主进程
            Application.Current.Shutdown();
        }

        private void SetLanguage(string culture)
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

        private void InitializeAxis()
        {
            try
            {
                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).MotorOn = 1;
                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).MotorOn = 1;
                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.C).MotorOn = 1;
                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.D).MotorOn = 1;


                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).MotionMode = 11;
                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).MotionMode = 11;
                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.C).MotionMode = 11;
                GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.D).MotionMode = 11;

                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.A).ClearBuffer();
                GlobalManager.Current._Agm800.controller.GetCiGroup(AxisRef.B).ClearBuffer();
            }
            catch { 

            }
        }

    }
}
