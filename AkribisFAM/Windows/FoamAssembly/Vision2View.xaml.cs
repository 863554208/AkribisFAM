using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for Vision2View.xaml
    /// </summary>
    public partial class Vision2View : UserControl
    {
        public Vision2View()
        {
            InitializeComponent();
            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;

            Task_PrecisionDownCamreaFunction.OnMessageReceive += Task_PrecisionDownCamreaFunction_OnMessageReceive;
            Task_PrecisionDownCamreaFunction.OnMessageSent += Task_PrecisionDownCamreaFunction_OnMessageSent;
        }

        private void Task_PrecisionDownCamreaFunction_OnMessageSent(object sender, string message)
        {
            Dispatcher.Invoke(() =>

              txtResult.Text += $"Message sent : {message}"
          );
        }

        private void Task_PrecisionDownCamreaFunction_OnMessageReceive(object sender, string message)
        {
            Thread.Sleep(1);
            Dispatcher.Invoke(() =>

                txtResult.Text += $"Message received : {message}"
            );
        }

        private void btnMoveStandby_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!App.vision1.MoveVision2StandbyPos())
            {
                MessageBox.Show("Fail to move vision 2 standby position");
            }
        }

        private void btnMoveEnding_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!App.vision1.MoveVision2EndingPos())
            {
                MessageBox.Show("Fail to move vision 2 ending position");
            }
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AkrAction.Current.StopAllAxis();
        }



        private void btnVis2OTF_Click(object sender, RoutedEventArgs e)
        {
            if (!App.vision1.Vision2OnTheFlyTrigger())
            {
                MessageBox.Show("Fail to move vision 2 ending position");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtResult.Text = "";
        }
    }
}
