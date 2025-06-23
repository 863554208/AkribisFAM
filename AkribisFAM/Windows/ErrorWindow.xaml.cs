using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using AkribisFAM.Manager;
using System.IO;
using HslCommunication.Secs.Types;
using YamlDotNet.Core;
using System.ComponentModel;
using System.Windows.Data;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow : Window
    {
        private ICollectionView _collectionView;
        private int selectedtable;
        public ErrorWindow()
        {
            InitializeComponent();


            ErrorManager.Current.Clear();

            this.DataContext = ErrorManager.Current;

            Namebox.ItemsSource = ErrorData.Columns.Select(c => c.Header.ToString()).ToList();
            selectedtable = 1;
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
            //System.Windows.MessageBox.Show("Export Success！", "Tip", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if(selectedtable == 2)
                ExportDataGridToCsv(ErrorData, ErrorManager.Current.ErrorHistoryInfos);
            if (selectedtable == 1)
                ExportDataGridToCsv(ErrorData, ErrorManager.Current.ErrorInfos);
        }

        private void PopBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorManager.Current.Pop();
            if (ErrorManager.Current.ErrorInfos.Count == 0) {
                return;
            }
            ErrorManager.Current.ErrorInfos.Remove(ErrorManager.Current.ErrorInfos[ErrorManager.Current.ErrorInfos.Count-1]);
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorData.ItemsSource = ErrorManager.Current.ErrorHistoryInfos;
            selectedtable = 2;
        }

        private void CurrentBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorData.ItemsSource = ErrorManager.Current.ErrorInfos;
            selectedtable = 1;
        }

        public object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }

        private void Namebox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedName = Namebox.SelectedItem as string;
            if (selectedtable == 2)
            {
                var uniqueNames = ErrorManager.Current.ErrorHistoryInfos
                    .Select(p => GetPropertyValue(p, selectedName))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();
                uniqueNames.Insert(0, "all");
                Databox.ItemsSource = uniqueNames;
                Databox.SelectedIndex = 0;
            }
            if (selectedtable == 1)
            {
                var uniqueNames = ErrorManager.Current.ErrorInfos
                    .Select(p => GetPropertyValue(p, selectedName))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();
                uniqueNames.Insert(0, "all");
                Databox.ItemsSource = uniqueNames;
                Databox.SelectedIndex = 0;
            }
        }

        private void QueryBtn_Click(object sender, RoutedEventArgs e)
        {
            string filter = Databox.Text.Trim().ToLower();
            if (Namebox.SelectedItem == null || Databox.SelectedItem == null)
            {
                return;
            }
            string selectedName = Namebox.SelectedItem as string;
            string selectedData = Databox.SelectedItem.ToString();
            if (selectedtable == 2)
            {
                _collectionView = CollectionViewSource.GetDefaultView(ErrorManager.Current.ErrorHistoryInfos);
                ErrorData.ItemsSource = _collectionView;
                if (selectedData == "all")
                {
                    _collectionView.Filter = null;
                }
                else
                {
                    _collectionView.Filter = item =>
                    {
                        var value = item.GetType().GetProperty(selectedName)?.GetValue(item)?.ToString();
                        return value == selectedData;
                    };
                }

                _collectionView.Refresh();
            }
            if (selectedtable == 1)
            {
                _collectionView = CollectionViewSource.GetDefaultView(ErrorManager.Current.ErrorInfos);
                ErrorData.ItemsSource = _collectionView;
                if (selectedData == "all")
                {
                    _collectionView.Filter = null;
                }
                else
                {
                    _collectionView.Filter = item =>
                    {
                        var value = item.GetType().GetProperty(selectedName)?.GetValue(item)?.ToString();
                        return value == selectedData;
                    };
                }
                _collectionView.Refresh();
            }

        }

        private void MuteBtn_Click(object sender, RoutedEventArgs e)
        {
            App.buzzer.Off();
        }
    }
}
