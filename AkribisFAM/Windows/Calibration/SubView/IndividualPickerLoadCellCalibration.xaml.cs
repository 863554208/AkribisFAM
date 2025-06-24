using AkribisFAM.DeviceClass;
using AkribisFAM.Helper;
using AkribisFAM.WorkStation;
using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static AkribisFAM.Manager.LoadCellCalibration;
using static AkribisFAM.Windows.LoadCellCalibrationView;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for IndividualPickerLoadCellCalibration.xaml
    /// </summary>
    public partial class IndividualPickerLoadCellCalibration : UserControl
    {
        private bool IsCalibratingLoadCell = false;
        private bool IsPickerInLoadCellPosition = false;
        System.Timers.Timer _timer;
        public IndividualPickerLoadCellCalibration()
        {
            InitializeComponent();
            btnEndCalibLoadCellNewton.IsEnabled = IsCalibratingLoadCell;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PickerCalibrationVM vm;
            int pickerNum = -1;
            if (IsVisible)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    vm = (PickerCalibrationVM)DataContext;
                    pickerNum = (int)vm.Model.picker;
                });
                SinglePoint point = ZuZhuang.Current.GetLoadCellPosition((int)pickerNum);
                if (point.X == 0 && point.Y == 0)
                {
                    IsPickerInLoadCellPosition = false;
                }
                else
                {
                    IsPickerInLoadCellPosition = AkrAction.Current.IsMoveLaserXYDone(point.X, point.Y);

                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    btnApplyForce.IsEnabled = IsPickerInLoadCellPosition;
                    btnCalibration.IsEnabled = IsPickerInLoadCellPosition;
                });

            }
        }

        private void btnMoveToLoadCell_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PickerCalibrationVM)DataContext;

            if (!App.assemblyGantryControl.MoveXYLoadCellPos(vm.Model.picker))
            {
                MessageBox.Show("Failed to move to load cell position");
                return;
            }
            return;
            vm.Values.Clear();
            var points = ConvertToPoint(vm.Model);
            for (int i = 0; i < points.Count; i++)
            {
                vm.Values.Add(points[i]);
            }

            vm.Model.NewtonCurrentList = ConvertToNewtonCurrentList(vm.Model);
            var mc = AssemblyGantryControl.CalculateLinearCoefficients(vm.Model.NewtonCurrentList.Select(x => x.Current).ToList(),
                vm.Model.NewtonCurrentList.Select(x => x.Newton).ToList());

            vm.Model.m = mc.m;
            vm.Model.C = mc.c;

            vm.LineValues = ConvertMCToPoint(vm.Model);

        }
        public List<NewtonCurrent> ConvertToNewtonCurrentList(LoadCellModel model)
        {
            Random rnd = new Random();
            double max = 1000;
            double min = 100;
            List<NewtonCurrent> list = new List<NewtonCurrent>();
            //foreach (var newtonCurrent in model.NewtonCurrentList)
            //{
            //    points.Add(new ObservablePoint()
            //    {
            //        X = newtonCurrent.Current,
            //        Y = newtonCurrent.Newton,
            //    });
            //}
            for (int i = 0; i < 100; i++)
            {
                list.Add(new NewtonCurrent()
                {
                    //X = rnd.NextDouble() * (3000 - 2000) + 2000,
                    Current = rnd.NextDouble() * (3000 - 2000) + 2000,
                    Newton = rnd.NextDouble() * (5 - 0) + 0,
                });
            }

            return list;
        }
        public ChartValues<ObservablePoint> ConvertMCToPoint(LoadCellModel model)
        {
            Random rnd = new Random();
            double max = 1000;
            double min = 100;
            ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();
            //foreach (var newtonCurrent in model.NewtonCurrentList)
            //{
            //    points.Add(new ObservablePoint()
            //    {
            //        X = newtonCurrent.Current,
            //        Y = newtonCurrent.Newton,
            //    });
            //}
            for (int i = 2000; i < 3000; i += 10)
            {
                points.Add(new ObservablePoint()
                {
                    X = i,
                    Y = model.m * i + model.C,
                    //X = newtonCurrent.Current,
                    //Y = newtonCurrent.Newton,
                    //Y = newtonCurrent.Newton,
                });
            }

            return points;
        }
        private async void btnCalibration_Click(object sender, RoutedEventArgs e)
        {
            if (txtSampleSize.Text == "0" || txtStepCurrent.Text == "0" || txtInitCurrent.Text == "0")
            {
                MessageBox.Show("Please insert valid input");
                return;
            }

            if (!Int32.TryParse(txtSampleSize.Text, out int size) ||
                !double.TryParse(txtStepCurrent.Text, out double step) ||
                !double.TryParse(txtInitCurrent.Text, out double intCurrent))
            {
                MessageBox.Show("Please insert valid input");
                return;
            }

            var vm = (PickerCalibrationVM)DataContext;
            if (MessageBox.Show($"Are you sure you want to start force control calibration for {vm.Model.picker}?",
                "Attention",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (!App.assemblyGantryControl.TriggerCalib(vm.Model.picker, intCurrent, step, size))
                {
                    MessageBox.Show("Failed to perform calibration process");
                    return;
                }
            }
            return;
            //res.Values.Clear();
            await Task.Run(() =>
            {
                var points = ConvertToPoint(vm.Model);
                for (int i = 0; i < points.Count; i++)
                {
                    vm.Values.Add(points[i]);
                }
                Thread.Sleep(100);
                var mc = AssemblyGantryControl.CalculateLinearCoefficients(vm.Model.NewtonCurrentList.Select(x => x.Current).ToList(),
vm.Model.NewtonCurrentList.Select(x => x.Newton).ToList());

                vm.Model.NewtonCurrentList = ConvertToNewtonCurrentList(vm.Model);
                vm.Model.m = mc.m;
                vm.Model.C = mc.c;
                vm.LineValues = ConvertMCToPoint(vm.Model);

            });
        }
        public ChartValues<ObservablePoint> ConvertToPoint(LoadCellModel model)
        {
            Random rnd = new Random();
            double max = 1000;
            double min = 100;
            ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();
            //foreach (var newtonCurrent in model.NewtonCurrentList)
            //{
            //    points.Add(new ObservablePoint()
            //    {
            //        X = newtonCurrent.Current,
            //        Y = newtonCurrent.Newton,
            //    });
            //}
            for (int i = 0; i < 100; i++)
            {
                points.Add(new ObservablePoint()
                {
                    X = rnd.NextDouble() * (3000 - 2000) + 2000,
                    Y = rnd.NextDouble() * (5 - 0) + 0,
                    //X = newtonCurrent.Current,
                    //Y = newtonCurrent.Newton,
                    //Y = newtonCurrent.Newton,
                });
            }
            return points;
        }

        private void btnApplyForce_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PickerCalibrationVM)DataContext;
            if (double.TryParse(txtCurrent.Text, out double current))
            {
                if (!App.assemblyGantryControl.TestCurrent(vm.Model.picker, current))
                {
                    MessageBox.Show($"Failed to apply force {txtNewtonCalib.Text} N, {txtCurrent.Text} mA");
                    return;
                }
            }

        }

        private void btnZup_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PickerCalibrationVM)DataContext;

            if (!App.assemblyGantryControl.ZUp(vm.Model.picker))
            {
                MessageBox.Show($"Failed to z up");
                return;
            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var vm = (PickerCalibrationVM)DataContext;

            if (MessageBox.Show($"Are you sure you want to save this a new model?",
                "Attention",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                vm.Model.LastUpdate = DateTime.Now;
                vm.Model.Description = DateTime.Now.ToString();
                App.calib.Models[(int)vm.Model.picker - 1] = new LoadCellModel(vm.Model);
                vm.Model = vm.Model;
                FileHelper.Save<LoadCellModel>(App.calib.Models[(int)vm.Model.picker - 1]);
            }
        }

        private void btnStartCalibLoadCellNewton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"To start load cell's calibration, please remove any weight above the load cell first.\n\r Press 'YES' to continue", "Attention", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                IsCalibratingLoadCell = true;
                btnEndCalibLoadCellNewton.IsEnabled = IsCalibratingLoadCell;
                try
                {
                    App.calib.ChangeToWeightCalib();
                    App.calib.ChannelCAL0();
                    MessageBox.Show($"Without any other weight, place a load and key in the load's weight.\n\r Press 'End calibration' to complete the calibration process");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    IsCalibratingLoadCell = false;
                    btnEndCalibLoadCellNewton.IsEnabled = IsCalibratingLoadCell;
                }

            }

        }

        private void btnEndCalibLoadCellNewton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (float.TryParse(txtNewtonCalib.Text, out float weight))
                {
                    App.calib.ChangeCalibWeight(weight);
                }
                App.calib.ChannelCALF0();
                IsCalibratingLoadCell = false;
            }
            catch (Exception ex)
            {
                IsCalibratingLoadCell = false;
                MessageBox.Show(ex.Message);
            }
            btnEndCalibLoadCellNewton.IsEnabled = IsCalibratingLoadCell;

        }

        private void btnZeroing_Click(object sender, RoutedEventArgs e)
        {
            App.calib.Zeroing();
        }
        private void txtCurrent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtCurrent.Text == "")
                txtCurrent.Text = "0";

            if (!txtCurrent.IsFocused)
                return;

            if (double.TryParse(txtCurrent.Text, out double current))
            {
                LoadCellModel model = ((PickerCalibrationVM)DataContext).Model;
                if (model.CurrentToNetwon(current, out double newton))
                {
                    txtNewton.Text = newton.ToString();
                }
            }
        }
        private void txtNewton_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNewton.Text == "")
                txtNewton.Text = "0";

            if (!txtNewton.IsFocused)
                return;

            if (double.TryParse(txtNewton.Text, out double newton))
            {
                LoadCellModel model = ((PickerCalibrationVM)DataContext).Model;
                if (model.NewtonToCurrent(newton, out double current))
                {
                    txtCurrent.Text = current.ToString();
                }
            }
        }
        private void txtIntOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            e.Handled = !e.Text.All(char.IsDigit);
        }
        private void txtDoubleOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            //e.Handled = !e.Text.All(char.IsDigit);
            TextBox textBox = sender as TextBox;

            // Allow digits
            if (char.IsDigit(e.Text, 0))
            {
                e.Handled = false;
            }
            // Allow only one dot, and it must not already exist
            else if (e.Text == "." && !textBox.Text.Contains("."))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true; // Block anything else
            }
        }

        private void txtNumberOnly_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Allow control keys like backspace, delete, left/right arrows
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Tab)
            {
                e.Handled = false;
            }
        }


    }
}
