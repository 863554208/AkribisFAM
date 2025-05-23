using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for FoamAssemblyView.xaml
    /// </summary>
    public partial class FoamAssemblyView : UserControl
    {
        public FoamAssemblyView()
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
