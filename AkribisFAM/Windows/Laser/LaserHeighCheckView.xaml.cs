using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for LaserHeighCheckView.xaml
    /// </summary>
    public partial class LaserHeighCheckView : UserControl
    {
        LaserHeighCheckVM vm;
        bool stopAllMotion = false;
        string Result = "";
        class LaserHeighCheckVM : ViewModelBase
        {
            private int totalProcess;

            public int TotalProcess
            {
                get { return totalProcess; }
                set { totalProcess = value; OnPropertyChanged(); }
            }
            private int progress;

            public int Progress
            {
                get { return progress; }
                set { progress = value; OnPropertyChanged(); }
            }

            private List<List<SinglePointExt>> points = new List<List<SinglePointExt>>();

            public List<List<SinglePointExt>> Points
            {
                get { return points; }
                set { points = value; }
            }
            private int col;

            public int Column
            {
                get { return col; }
                set { col = value; }
            }
            private int row;

            public int Row
            {
                get { return row; }
                set { row = value; }
            }
        }
        public LaserHeighCheckView()
        {
            InitializeComponent();

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;

            Task_KEYENCEDistance.OnMessageReceive += Task_KEYENCEDistance_OnMessageReceive;
            Task_KEYENCEDistance.OnMessageSent += Task_KEYENCEDistance_OnMessageSent;
            Task_Scanner.OnMessageReceive += Task_Scanner_OnMessageReceive;
            Task_Scanner.OnMessageSent += Task_Scanner_OnMessageSent;


        }

        private void Task_Scanner_OnMessageSent(object sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (this.IsVisible)
                {
                    txtTrayBarcode.Text += $"Message sent: {message} \n\r";
                    scrollviewerTray.ScrollToEnd();
                }
            });
        }

        private void Task_Scanner_OnMessageReceive(object sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (this.IsVisible)
                {
                    txtTrayBarcode.Text += $"Message receive: {message} \n\r";
                    scrollviewerTray.ScrollToEnd();
                }
            });
        }

        private void Task_KEYENCEDistance_OnMessageSent(object sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (this.IsVisible)
                {
                    txtHeightResult.Text += $"Message sent: {message} \n\r";
                    scrollviewerTray.ScrollToEnd();
                }
            });
        }

        private void Task_KEYENCEDistance_OnMessageReceive(object sender, string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (this.IsVisible)
                {
                    txtHeightResult.Text += $"Message receive: {message} \n\r";
                    scrollviewerTray.ScrollToEnd();
                }
            });
        }

        private void cbxTrayType_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void cbxTrayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = null;
            List<List<SinglePointExt>> lsp = new List<List<SinglePointExt>>();
            if (cbxTrayType.SelectedIndex < 0) return;
            if (cbxTrayType.SelectedIndex > 4) return;

           if (!LaiLiao.Current.GetTeachPointList((TrayType)cbxTrayType.SelectedIndex, out lsp))
            {
                System.Windows.Forms.MessageBox.Show("Failed to get laser's teach point");
            }
            vm = new LaserHeighCheckVM()
            {
                Points = lsp,
                Row = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartRow,
                Column = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartColumn,
            };
            DataContext = vm;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            //Points = new ObservableCollection<SinglePoint>(GlobalManager.Current.laserPoints);
            //DataContext = Points;
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible) return;

            //Points = new ObservableCollection<SinglePoint>(GlobalManager.Current.laserPoints);
            //DataContext = Points;
        }

        private async void btnCheckAllTeachPoint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Result = "";
            stopAllMotion = false;
            stopAllMotion = false;
            vm.TotalProcess = 4 * vm.Row * vm.Column;
            vm.Progress = 0;
            grpControl.IsEnabled = false;
            pbProgress.Visibility = System.Windows.Visibility.Visible;

            if (vm != null)
            {
              await  Task.Run(() =>
                {
                    foreach (var pts in vm.Points)
                    {
                        foreach (var pt in pts)
                        {
                            if (stopAllMotion) return;

                            if (AkrAction.Current.MoveLaserXY(pt.X, pt.Y) != (int)AkrAction.ACTTION_ERR.NONE)
                            {
                                //MessageBox.Show("Failed to move position");
                                return;
                            }

                            if (!App.laser.Measure(out double readout))
                            {
                                //MessageBox.Show("Failed to measure");
                                return;
                            }
                            vm.Progress++;
                            Result = readout.ToString();

                            Thread.Sleep(50);
                        }
                    }

                });
            }

            txtHeightResult.Text += Result;
            vm.Progress = 0;
            grpControl.IsEnabled = true;
            pbProgress.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            stopAllMotion = true;
            Task.Run(() =>
            {
                AkrAction.Current.StopAllAxis();
            });
        }


        private void btnTriggerLaser_Click(object sender, RoutedEventArgs e)
        {
            if (!App.laser.Measure(out double readout))
            {
                MessageBox.Show("Failed to measure");
            }
            //Result += readout.ToString();
        }

        private void btnTriggerScanner_Click(object sender, RoutedEventArgs e)
        {
            if (App.scanner.ScanBarcode(out string readout) != 0)
            {
                MessageBox.Show("Failed to measure");
            }
            //Result += readout.ToString();
        }

        private void btnClearBarcode_Click(object sender, RoutedEventArgs e)
        {

            txtTrayBarcode.Text = "";
        }

        private void btnClearLaser_Click(object sender, RoutedEventArgs e)
        {
            txtHeightResult.Text = "";
        }
    }
}
