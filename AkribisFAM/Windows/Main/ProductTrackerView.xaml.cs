using System.Collections.ObjectModel;
using System.Windows.Controls;
using AkribisFAM.Util;
using AkribisFAM.ViewModel;
using AkribisFAM.WorkStation;
using static AkribisFAM.WorkStation.Conveyor;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for ProductTrackerView.xaml
    /// </summary>
    public partial class ProductTrackerView : UserControl
    {
        public LaserStationVM vm = new LaserStationVM();
        ProductData pd ;
        public ProductTrackerView()
        {
            InitializeComponent();
            DataContext = vm.ProductTracker;
        }

        private void SingleProductTrackerView_ButtonPressed(object sender, System.EventArgs e)
        {
            Button clickedButton = (Button)sender;
            ProductData selectedProduct = (ProductData)clickedButton.DataContext;
            if (selectedProduct != null)
            {
                pd = selectedProduct;
                pd.HeightMeasurements.Add(new LaserMeasurement()
                {
                    XMeasurePosition = 11,
                    YMeasurePosition = 22,
                    HeightMeasurement = 333,
                });
                var res = Parser.ToPropertyDictionary(pd);
                listviewSelectedProduct.ItemsSource = res;

            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            //DataContext = vm.ProductTracker;
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible) { return; }
            //Conveyor.ConveyorTrays[(int)ConveyorStation.Laser].Barcode = "TESTING";
            //vm.ProductTracker.FoamAssemblyStationTray.Copy((Conveyor.TrayData)vm.ProductTracker.LaserStationTray);
            //vm.ProductTracker.LaserStationTray.Reset();
            //vm.ProductTracker.FoamAssemblyStationTray.Reset();
            //DataContext = vm.ProductTracker;

            //App.productTracker.LaserStationTray.Reset();
            //App.productTracker.LaserStationTray = App.productTracker.LaserStationTray;
            //laser.DataContext = App.productTracker.LaserStationTray;
            //foam.DataContext = App.productTracker.FoamAssemblyStationTray;
            //recheck.DataContext = App.productTracker.RecheckStationTray;
            //good.DataContext = App.productTracker.GoodOutGoingStationTray;
            //reject.DataContext = App.productTracker.RejectOutGoingStationTray;
            //vm.ProductTracker = App.productTracker;
            //DataContext = vm.ProductTracker;
            //pick.DataContext = App.productTracker.LaserStationTray;
        }
    }
}
