using AkribisFAM.WorkStation;
using System.Windows;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for PointXYMoveView.xaml
    /// </summary>
    public partial class PointXYMoveView : UserControl
    {
        public PointXYMoveView()
        {
            InitializeComponent();
        }

        private void btnMoveToPos_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var dc = (SinglePoint)DataContext;
                if (AkrAction.Current.Move(AxisName.LSX, (int)dc.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX) != 0 ||
                AkrAction.Current.Move(AxisName.LSY, (int)dc.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY) != 0)
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
                if (AkrAction.Current.Move(AxisName.LSX, (int)dc.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX) != 0 ||
                AkrAction.Current.Move(AxisName.LSY, (int)dc.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY) != 0)
                {
                    MessageBox.Show("Failed to move position");
                    return;
                }

                if (!App.laser.Measure(out int readout))
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
