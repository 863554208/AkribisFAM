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
using System.IO;

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

            ErrorManager.Current.Clear();

            this.DataContext = ErrorManager.Current;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorManager.Current.Clear();
            ErrorManager.Current.ErrorInfos.Clear();
            App.buzzer.BeepOff();
            App.buzzer.EnableBeep = true ;
            System.Windows.MessageBox.Show("Alarm cleared!", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        public static void ExportDataGridToCsv<T>(System.Windows.Controls.DataGrid dataGrid, IEnumerable<T> items)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = "Error.csv"
            };

            dialog.ShowDialog();

            var filePath = dialog.FileName;
            var sb = new StringBuilder();

            // 获取列标题
            var props = typeof(T).GetProperties();
            sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

            // 获取每一行数据
            foreach (var item in items)
            {
                var values = props.Select(p => (p.GetValue(item)?.ToString() ?? "").Replace(",", "，"));
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            System.Windows.MessageBox.Show("Export Success！", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportDataGridToCsv(ErrorData, ErrorManager.Current.ErrorInfos);
        }

        private void MuteBtn_Click(object sender, RoutedEventArgs e)
        {
            App.buzzer.Off();
        }
    }
}
