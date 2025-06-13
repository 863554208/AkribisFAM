using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using AkribisFAM.Properties;
using AkribisFAM.Util;
using AkribisFAM.ViewModel;
using AkribisFAM.WorkStation;
using static System.Windows.Forms.AxHost;
using static AkribisFAM.WorkStation.Conveyor;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for ProductTrackerView.xaml
    /// </summary>
    public partial class ProductTrackerView : UserControl
    {
        public LaserStationVM vm = new LaserStationVM();
        ProductData pd;
        public ProductTrackerView()
        {
            InitializeComponent();
            DataContext = vm;
            App.Current.Exit += Current_Exit;
        }
        private void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            vm.PauseUpdateThread();
            vm.TerminateUpdateThread();
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
            itemControlStation.ItemsSource = vm.ConveyorTraysSending;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            vm.ResumeUpdateThread();
            //DataContext = vm.ProductTracker;
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {

            vm.PauseUpdateThread();
        }
        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible) { return; }
            itemControlStation.ItemsSource = vm.ConveyorTraysSending;
            //Conveyor.Current.ConveyorTrays[(int)ConveyorStation.Laser].Barcode = "TESTING";
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

        private void btnSet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int station = -1;
            int state = -1;
            if (btnLaser.IsChecked == true)
            {
                station = 0;
            }
            else if (btnFoam.IsChecked == true)
            {
                station = 1;
            }
            else if (btnRecheck.IsChecked == true)
            {
                station = 2;
            }
            else if (btnReject.IsChecked == true)
            {
                station = 3;
            }


            if (btnEmpty.IsChecked == true)
            {
                state = 0;
            }
            else if (btnTrayIncoming.IsChecked == true)
            {
                state = 1;
            }
            else if (btnInProgress.IsChecked == true)
            {
                state = 2;
            }
            else if (btnTrayOutgoing.IsChecked == true)
            {
                state = 3;
            }
            var step = Int32.Parse(txtStep.Text);
            try
            {
                Conveyor.Current.Go[station, state, step] = btnTrue.IsChecked == true;

            }
            catch (Exception)
            {

            }
        }

        private void btnSet2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            reset();
        }
        private void reset()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 20; k++)
                    {
                        Conveyor.Current.Go[i, j, k] = false;
                    }
                }
            }
        }

        private void btnSet3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Conveyor.Current.count = 0;
        }
    }
}
