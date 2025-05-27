using System;
using System.Windows;
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
    }
}
