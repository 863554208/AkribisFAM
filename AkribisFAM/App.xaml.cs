using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AkribisFAM.Windows;

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
            SetLanguage("zh-CHS");

            if(new LoginViewModel().ShowDialog() == true)
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
