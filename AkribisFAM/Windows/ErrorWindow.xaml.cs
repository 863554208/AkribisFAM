using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AkribisFAM.Manager;
using YamlDotNet.Core;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
            //ErrorData.ItemsSource = ErrorManager.Current.ErrorInfos;

            var data = ErrorManager.Current.ErrorInfos;
            Console.WriteLine($"共有 {data.Count} 条错误信息"); // 或者设置断点检查

            ErrorManager.Current.Clear();

            this.DataContext = ErrorManager.Current;
        }
    }
}
