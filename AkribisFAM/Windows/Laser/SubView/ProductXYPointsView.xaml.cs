using AkribisFAM.WorkStation;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.Windows.LaserHeighCheckView;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for ProductXYPointsView.xaml
    /// </summary>
    public partial class ProductXYPointsView : UserControl
    {
        public ProductXYPointsView()
        {
            InitializeComponent();
        }

        private void btnMoveToPosLaserCheckAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var dc = (ObservableCollection<SinglePoint>)DataContext;
                foreach (var pts in dc)
                {

                    if (AkrAction.Current.MoveNoWait(AxisName.LSX, (int)pts.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX) != 0 ||
                    AkrAction.Current.Move(AxisName.LSY, (int)pts.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Failed to move position");
                        return;
                    }

                    if (!App.laser.Measure(out int readout))
                    {
                        System.Windows.Forms.MessageBox.Show("Failed to measure");
                        return;
                    }

                    Thread.Sleep(50);
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
