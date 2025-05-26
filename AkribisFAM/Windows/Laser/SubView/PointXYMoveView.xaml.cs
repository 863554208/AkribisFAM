using AkribisFAM.WorkStation;
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
                AkrAction.Current.MoveNoWait(AxisName.LSX, (int)dc.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX);
                AkrAction.Current.Move(AxisName.LSY, (int)dc.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY);

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
                AkrAction.Current.MoveNoWait(AxisName.LSX, (int)dc.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX);
                AkrAction.Current.Move(AxisName.LSY, (int)dc.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY);
                App.laser.Measure(out int readout);
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
