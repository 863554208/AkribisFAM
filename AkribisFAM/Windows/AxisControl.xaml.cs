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
using AAMotion;
using AkribisFAM.Util;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {
        private int _currentAxis;
        private AxisIntegerToStringDic _axisDic;
        public int CurrentAxis
        {
            get => _currentAxis;
            set
            {
                _currentAxis = value;
                UpdateUI();
            }
        }

        public AxisControl()
        {
            InitializeComponent();
            _axisDic = new AxisIntegerToStringDic();
        }

        private void Axis_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button btn)
            {
                string axisId = btn.Name; 

                var match = System.Text.RegularExpressions.Regex.Match(axisId, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int axisNumber))
                {
                    CurrentAxis = axisNumber;                   
                }
            }
        }

        private void UpdateUI()
        {
            currentAxisLabel.Content = $"Axis {CurrentAxis}";

            if (CurrentAxis >= 1 && CurrentAxis <= AxiscomboBox.Items.Count)
            {
                AxiscomboBox.SelectedIndex = CurrentAxis - 1;
            }
            else
            {
                AxiscomboBox.SelectedIndex = -1;
            }
        }

        private void MotorOn_Click(object sender, RoutedEventArgs e)
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(axisName, out AxisRef axisRef))
            {
                try
                {
                    AAMotionAPI.MotorOn(GlobalManager.Current._Agm800.controller, axisRef);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("轴使能报错 :" + ex.Message);
                }
            }
        }

        private void MotorOff_Click(object sender, RoutedEventArgs e)
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(axisName, out AxisRef axisRef))
            {
                try
                {
                    AAMotionAPI.MotorOff(GlobalManager.Current._Agm800.controller, axisRef);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("轴下使能报错 :" + ex.Message);
                }
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            int targetPos = int.Parse(Targetpos.Text);

            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (Enum.TryParse<AxisRef>(axisName, out AxisRef axisRef))
            {
                AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, axisRef, targetPos);
            }
        }

        private void AxiscomboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
