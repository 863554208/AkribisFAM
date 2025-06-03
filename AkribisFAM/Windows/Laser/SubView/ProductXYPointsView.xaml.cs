using AkribisFAM.WorkStation;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;
using static AkribisFAM.Windows.LaserHeighCheckView;
using System.Windows;
using System.Collections.Generic;

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
                var dc = (List<SinglePointExt>)DataContext;
                foreach (var pts in dc)
                {

                    if (AkrAction.Current.MoveLaserXY(pts.X, pts.Y) != (int)AkrAction.ACTTION_ERR.NONE)
                    {
                        System.Windows.Forms.MessageBox.Show("Failed to move position");
                        return;
                    }

                    if (!App.laser.Measure(out double readout))
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
