using System;
using System.Windows.Controls;
using AkribisFAM.Manager;

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

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;
        }
    }
}
