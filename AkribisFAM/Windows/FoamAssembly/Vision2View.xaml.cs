using System;
using System.Windows.Controls;
using AkribisFAM.Manager;

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
            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;
        }
    }
}
