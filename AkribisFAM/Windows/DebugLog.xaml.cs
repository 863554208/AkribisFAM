using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AkribisFAM.ViewModel;
using AkribisFAM.Util;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// DebugLog.xaml 的交互逻辑
    /// </summary>
    public partial class DebugLog : UserControl
    {
        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        public DebugLog()
        {
            InitializeComponent();
            MessageListView.ItemsSource = _messages;

            // 启动后台线程读取 BlockingCollection
            Task.Run(() => ProcessQueue(_cts.Token));
        }

        private void ProcessQueue(CancellationToken token)
        {
            foreach (var item in Logger._logQueue.GetConsumingEnumerable(token))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _messages.Add(item);
                    if (_messages.Count > 200)
                        _messages.RemoveAt(0);
                    if (_messages.Count > 0) {
                        MessageListView.ScrollIntoView(_messages[_messages.Count - 1]);
                    }
                }));
            }
        }
    }
}
