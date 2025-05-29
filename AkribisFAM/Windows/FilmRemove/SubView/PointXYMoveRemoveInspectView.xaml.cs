using AkribisFAM.WorkStation;
using System;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for PointXYMoveRemoveInspectView.xaml
    /// </summary>
    public partial class PointXYMoveRemoveInspectView : UserControl
    {
        public event EventHandler VisionInspectPressed;
        public event EventHandler MovePressed;
        public event EventHandler RemoveFilmPressed;
        public PointXYMoveRemoveInspectView()
        {
            InitializeComponent();
        }

        private void btnMoveToPos_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            MovePressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnRemoveFilm_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            RemoveFilmPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnInspect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VisionInspectPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
