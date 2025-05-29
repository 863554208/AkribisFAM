using AkribisFAM.WorkStation;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for PointXYMoveRemoveInspectView.xaml
    /// </summary>
    public partial class PointXYMoveRemoveInspectView : UserControl
    {
        public PointXYMoveRemoveInspectView()
        {
            InitializeComponent();
        }

        private void btnMoveToPos_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var dc = (SinglePointExt)DataContext;
                if (AkrAction.Current.Move(AxisName.LSX, (int)dc.X, (int)AxisSpeed.LSX, (int)AxisAcc.LSX) != 0 ||
                AkrAction.Current.Move(AxisName.LSY, (int)dc.Y, (int)AxisSpeed.LSY, (int)AxisAcc.LSY) != 0)
                {
                    System.Windows.Forms.MessageBox.Show("failed");
                }

            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private void btnRemoveFilm_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnInspect_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
