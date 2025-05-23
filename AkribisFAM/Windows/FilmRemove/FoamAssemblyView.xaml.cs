using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for FileRemoveView.xaml
    /// </summary>
    public partial class FileRemoveView : UserControl
    {
        public FileRemoveView()
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
