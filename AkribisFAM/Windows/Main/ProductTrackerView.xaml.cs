using System.Collections.ObjectModel;
using System.Windows.Controls;
using AkribisFAM.Util;
using AkribisFAM.ViewModel;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for ProductTrackerView.xaml
    /// </summary>
    public partial class ProductTrackerView : UserControl
    {
        public LaserStationVM vm = new LaserStationVM();
        public ProductTrackerView()
        {
            InitializeComponent();
            DataContext = vm.ProductTracker;
        }
    }
}
