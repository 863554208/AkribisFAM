using AkribisFAM.WorkStation;
using System.Windows.Controls;
using static AkribisFAM.WorkStation.Conveyor;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for LaserMeasurementView.xaml
    /// </summary>
    public partial class LaserMeasurementView : UserControl
    {
        public LaserMeasurementView()
        {
            InitializeComponent();
            DataContext = Conveyor.ConveyorTrays[(int)ConveyorStation.Laser];
        }
    }
}
