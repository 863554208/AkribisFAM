using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using System.ComponentModel;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Performance.xaml 的交互逻辑
    /// </summary>
    public partial class Performance : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ChartValues<double> targetUPHvalues = new ChartValues<double> { 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200 };
        public ChartValues<double> TargetUPHValues { get { return targetUPHvalues; } }
        public ChartValues<double> UPHvalues = new ChartValues<double> { };
        public ChartValues<double> UPHValues { get { return UPHvalues; } }

        public List<string> UPHLabels = new List<string> { "7:00", "8:00", "9:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "24:00", "1:00", "2:00", "3:00", "4:00", "5:00", "6:00" };
        public List<string> UPHXLabels
        {
            get { return UPHLabels; }
        }

        private ChartValues<double> targetYieldvalues = new ChartValues<double> { 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98, 98 };
        public ChartValues<double> TargetYieldValues { get { return targetYieldvalues; } }
        public ChartValues<double> Yieldvalues = new ChartValues<double> { };
        public ChartValues<double> YieldValues { get { return Yieldvalues; } }

        public List<string> YieldLabels = new List<string> { "7:00", "8:00", "9:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "24:00", "1:00", "2:00", "3:00", "4:00", "5:00", "6:00" };
        public List<string> YieldXLabels
        {
            get { return YieldLabels; }
        }

        public Performance()
        {
            InitializeComponent();
            this.DataContext = this;
        }

    }
}
