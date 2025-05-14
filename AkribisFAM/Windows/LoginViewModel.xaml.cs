using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// LoginViewModel.xaml 的交互逻辑
    /// </summary>
    public partial class LoginViewModel : Window
    {

        private int attemptCount = 0;  // 用于记录用户尝试的次数
        private const int maxAttempts = 5;  // 最大尝试次数

        public LoginViewModel()
        {
            InitializeComponent();

            Trace.WriteLine("应用程序启动");

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;



            // 检查用户名和密码是否为空
            //if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            //{
            //    MessageBox.Show("Username and password cannot be empty. Please try again.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}


            // 简单的验证（你可以用更复杂的验证方式）
            if (username == "" && password == "")
            {
                this.DialogResult = true;  // 登录成功
                this.Close();
            }
            else
            {
                attemptCount++;  // 尝试次数加1

                if (attemptCount >= maxAttempts)
                {
                    MessageBox.Show("You have reached the maximum number of attempts. Closing the application.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;  // 登录失败，关闭窗口
                    this.Close();
                }
                else
                {
                    // 提示用户剩余尝试次数
                    MessageBox.Show($"Incorrect username or password. You have {maxAttempts - attemptCount} attempt(s) left.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
