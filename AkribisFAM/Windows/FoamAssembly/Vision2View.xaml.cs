using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for Vision2View.xaml
    /// </summary>
    public partial class Vision2View : UserControl
    {
        public Vision2View()
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
    }
}
