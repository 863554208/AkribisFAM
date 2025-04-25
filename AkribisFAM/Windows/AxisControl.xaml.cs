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

namespace AkribisFAM.Windows
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {
        public AxisControl()
        {
            InitializeComponent();
        }

        //private void Axis_1_ControlButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AxisContorlDisplay.Content =  new Axis1ViewModel();
        //}

        //private void Axis_2_ControlButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AxisContorlDisplay.Content = new Axis2ViewModel();
        //}

        //private void Axis_3_ControlButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AxisContorlDisplay.Content = new Axis3ViewModel();
        //}

        private void Axis_Click(object sender, RoutedEventArgs e)
        {
        }

        

        private void AxiscomboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
