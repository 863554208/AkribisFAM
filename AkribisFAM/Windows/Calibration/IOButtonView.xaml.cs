using AkribisFAM.CommunicationProtocol;
using System.Windows.Controls;
using static AkribisFAM.Windows.IOControlView;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for IOButtonView.xaml
    /// </summary>
    public partial class IOButtonView : UserControl
    {
        public IOButtonView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is DigitalOutputVM)
            {
                var vm = (DigitalOutputVM)DataContext;
                bool toggleStatus = !vm.Status;
                IOManager.Instance.IO_ControlStatus(vm.Enum.Value, toggleStatus ? 1 : 0);
            }

        }
    }
}
