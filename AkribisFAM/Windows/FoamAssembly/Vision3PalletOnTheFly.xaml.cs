using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using static AkribisFAM.Windows.FoamAssemblyView;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for Vision3PalletOnTheFly.xaml
    /// </summary>
    public partial class Vision3PalletOnTheFly : UserControl
    {
        public Vision3PalletOnTheFly()
        {
            InitializeComponent();
            //cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            //cbxTrayType.SelectedIndex = 0;
        }

        private void btnMoveStandby_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (!App.vision1.MoveVision2StandbyPos())
            //{
            //    MessageBox.Show("Fail to move vision 2 standby position"); 
            //}    
        }

        private void btnMoveEnding_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (!App.vision1.MoveVision2EndingPos())
            //{
            //    MessageBox.Show("Fail to move vision 2 ending position");
            //}
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AkrAction.Current.StopAllAxis();
        }

      

        private void btnVis3OTF_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void cbxTrayType_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void cbxTrayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
