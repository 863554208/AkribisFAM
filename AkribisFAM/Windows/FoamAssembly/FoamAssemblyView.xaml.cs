using System;
using System.Windows.Controls;
using AkribisFAM.Manager;

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

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;
        }
    }
}
