using AkribisFAM.WorkStation;
using System.Windows;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for PointXYVisionMeasurementView.xaml
    /// </summary>
    public partial class PointXYVisionMeasurementView : UserControl
    {
        public PointXYVisionMeasurementView()
        {
            InitializeComponent();
        }

        private void btnMoveToPos_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var dc = (SinglePoint)DataContext;
                if (AkrAction.Current.MoveLaserXY(dc.X, dc.Y) != (int)AkrAction.ACTTION_ERR.NONE)
                {
                    MessageBox.Show("failed");
                }

            }
            catch (System.Exception)
            {

                throw;
            }

        }

        private void btnMoveToPosLaserCheck_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var dc = (SinglePoint)DataContext;
                if (AkrAction.Current.MoveLaserXY(dc.X, dc.Y) != (int)AkrAction.ACTTION_ERR.NONE)
                {
                    MessageBox.Show("Failed to move position");
                    return;
                }

                if (!App.laser.Measure(out double readout))
                {
                    MessageBox.Show("Failed to measure");
                    return;
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
