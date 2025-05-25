using System;
using System.Windows.Controls;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;

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
        }

        private void btnMoveStandby_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.vision1.MoveVision2StandbyPos();
        }

        private void btnMoveEnding_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.vision1.MoveVision2EndingPos();
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AkrAction.Current.StopAllAxis();
        }
    }
}
