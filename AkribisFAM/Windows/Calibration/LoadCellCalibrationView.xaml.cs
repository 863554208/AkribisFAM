using System.Windows.Controls;
using static AkribisFAM.Manager.LoadCellCalibration;
using System;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Defaults;
using System.Linq;
using System.Timers;
using System.Collections.Generic;
using AkribisFAM.DeviceClass;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for LoadCellCalibrationView.xaml
    /// </summary>
    public partial class LoadCellCalibrationView : UserControl
    {
        Timer _timer;
        PickerCalibrationVM[] vms = new PickerCalibrationVM[4];
        public LoadCellCalibrationView()
        {
            InitializeComponent();
            _timer = new Timer(1000); // 1000ms = 1 second
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            for (int i = 0; i < 4; i++)
            {
                vms[i] = new PickerCalibrationVM()
                {
                    Model = new LoadCellModel(App.calib.Models[i]),
                };

                vms[i].Values = ConvertToPoint(vms[i].Model);
                vms[i].Model.NewtonCurrentList = ConvertToNewtonCurrentList(vms[i].Model);
                var mc = AssemblyGantryControl.CalculateLinearCoefficients(vms[i].Model.NewtonCurrentList.Select(x => x.Current).ToList(),
                   vms[i].Model.NewtonCurrentList.Select(x => x.Newton).ToList());

                vms[i].Model.m = mc.m;
                vms[i].Model.C = mc.c;
                vms[i].LineValues = ConvertMCToPoint(App.calib.Models[i]);
            }

            calibPicker1.DataContext = vms[0];
            calibPicker2.DataContext = vms[1];
            calibPicker3.DataContext = vms[2];
            calibPicker4.DataContext = vms[3];
            //calibPicker2.DataContext = new PickerCalibrationVM()
            //{
            //    Model = App.calib.Models[1],
            //    Values = ConvertToPoint(App.calib.Models[1]),
            //};
            //calibPicker3.DataContext = new PickerCalibrationVM()
            //{
            //    Model = App.calib.Models[2],
            //    Values = ConvertToPoint(App.calib.Models[2]),
            //};
            //calibPicker4.DataContext = new PickerCalibrationVM()
            //{
            //    Model = App.calib.Models[3],
            //    Values = ConvertToPoint(App.calib.Models[3]),
            //};

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
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsVisible)
            {

            }

        }

        private void AirControl_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

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
        public List<double> GenerateYValues(double m, double c, List<double> xValues)
        {
            List<double> yValues = new List<double>();

            foreach (double x in xValues)
            {
                double y = m * x + c;
                yValues.Add(y);
            }

            return yValues;
        }
        public class PickerCalibrationVM : ViewModelBase
        {
            private LoadCellModel _model;

            public LoadCellModel Model
            {
                get { return _model; }
                set { _model = value; OnPropertyChanged(); }
            }
            public ChartValues<ObservablePoint> _values = new ChartValues<ObservablePoint>();
            public ChartValues<ObservablePoint> Values
            {
                get { return _values; }
                set { _values = value; OnPropertyChanged(); }
            }
            public ChartValues<ObservablePoint> _linevalues = new ChartValues<ObservablePoint>();
            public ChartValues<ObservablePoint> LineValues
            {
                get { return _linevalues; }
                set { _linevalues = value; OnPropertyChanged(); }
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
