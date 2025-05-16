using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using AkribisFAM.ViewModel;
using System.Diagnostics;
using AkribisFAM.CommunicationProtocol;
using static MaterialDesignThemes.Wpf.Theme;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {

        int AxisNum = 24;

        List<bool> AxisList = new List<bool>();

        public AxisControl()
        {
            InitializeComponent();
            
            initUI();


        }


        private void initUI()
        {
            for (int i = 1; i <= AxisNum; i++)
            {
                string axisName = $"Axis {i}";

                // 添加到 ComboBox（CboxNowAxis）
                CboxNowAxis.Items.Add(axisName);

                // 添加到 ListBox（AxisListBox）
                AxisListBox.Items.Add(axisName);

                AxisList.Add(false);

            }

            // 可选：设置默认选中第一个
            if (CboxNowAxis.Items.Count > 0)
                CboxNowAxis.SelectedIndex = 0;

            if (AxisListBox.Items.Count > 0)
                AxisListBox.SelectedIndex = 0;
        }

        private void CboxNowAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int nowAxis = CboxNowAxis.SelectedIndex;
            tbAxisName.Text = CboxNowAxis.SelectedValue.ToString();
            if (AxisListBox.SelectedIndex != nowAxis)
            {
                AxisListBox.SelectedIndex = nowAxis;
            }
            if (AxisList[nowAxis] != IsAxisEnable.IsChecked)
            {
                IsAxisEnable.IsChecked = AxisList[nowAxis];
            }
        }

        private void AxisListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int nowAxis = AxisListBox.SelectedIndex;
            tbAxisName.Text = AxisListBox.SelectedItem.ToString();
            if (CboxNowAxis.SelectedIndex != nowAxis)
            {
                CboxNowAxis.SelectedIndex = nowAxis;
            }

            if (AxisList[nowAxis] != IsAxisEnable.IsChecked)
            {
                IsAxisEnable.IsChecked = AxisList[nowAxis];
            }

        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (btn != null && btn.IsChecked == true)
            {
                // 打开逻辑
            }
            else
            {
                //关闭
            }
        }
    }
}
