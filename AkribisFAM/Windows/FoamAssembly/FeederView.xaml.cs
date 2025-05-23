using System;
using System.Windows.Controls;
using AkribisFAM.Manager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for FeederView.xaml
    /// </summary>
    public partial class FeederView : UserControl
    {
        public FeederView()
        {
            InitializeComponent();

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;
        }
    }
}
