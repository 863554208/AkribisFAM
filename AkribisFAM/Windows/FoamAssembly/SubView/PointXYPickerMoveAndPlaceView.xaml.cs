using System;
using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for PointXYPickerMoveAndPlaceView.xaml
    /// </summary>
    public partial class PointXYPickerMoveAndPlaceView : UserControl
    {

        public event EventHandler PickerMovePressed;
        public event EventHandler PickerPlacePressed;
        public int SelectedPicker
        {
            get
            {
                if (btnPicker1.IsSelected) return 1;
                if (btnPicker2.IsSelected) return 2;
                if (btnPicker3.IsSelected) return 3;
                if (btnPicker4.IsSelected) return 4;
                else
                    return -999;
            }
        }
        public PointXYPickerMoveAndPlaceView()
        {
            InitializeComponent();
        }

        private void btnPicker_Selected(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnMoveToPos_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PickerMovePressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnPlace_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PickerPlacePressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
