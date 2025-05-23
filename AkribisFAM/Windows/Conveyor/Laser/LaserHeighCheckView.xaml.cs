using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for LaserHeighCheckView.xaml
    /// </summary>
    public partial class LaserHeighCheckView : UserControl
    {
        public LaserHeighCheckView()
        {
            InitializeComponent();
            cbxTrayType.ItemsSource = new string[]
            {
                "PAM_230_144",
                "RIM_292_120",
                "SLAW_360_260",
                "VAM_300_220",
                "RUM_300_200",
            };
            cbxTrayType.SelectedIndex = 0;
        }

        private void cbxTrayType_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void cbxTrayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }



    }
}
