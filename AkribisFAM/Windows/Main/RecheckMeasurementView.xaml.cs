using AkribisFAM.WorkStation;
using System.Windows.Controls;
using static AkribisFAM.WorkStation.Conveyor;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for RecheckMeasurementView.xaml
    /// </summary>
    public partial class RecheckMeasurementView : UserControl
    {
        public RecheckMeasurementView()
        {
            InitializeComponent();
            DataContext = Conveyor.Current.ConveyorTrays[(int)ConveyorStation.Recheck];
        }
    }
}
