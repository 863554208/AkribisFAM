using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// ManualControl.xaml 的交互逻辑
    /// </summary>
    public partial class ManualControl : UserControl
    {

        //--************************************************************************************************************
        // --//Add By YXW 2025-5-16 ************************************************************
        //--************************************************************************************************************
        AxisControl axisControl;
        AirControl airControl;
        CameraControl cameraControl;
        IOConfigure iOConfigure;
        private Button _selectedButton;

        public ManualControl()
        {
            InitializeComponent();

            axisControl = new AxisControl();
            airControl = new AirControl();
            cameraControl = new CameraControl();
            iOConfigure = new IOConfigure();

            ManualControlDisplay.Content = axisControl;

            this.AxisDebug.Tag = "Selected";


        }

        private void AxisConfigure_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            SetSelectedButton(sender as Button);
            ManualControlDisplay.Content = axisControl;  // MainScreen 是你定义的一个用户控件或界面
        }

        private void AirControl_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            SetSelectedButton(sender as Button);
            ManualControlDisplay.Content = airControl;  // MainScreen 是你定义的一个用户控件或界面
        }

        private void CameraControl_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            SetSelectedButton(sender as Button);
            ManualControlDisplay.Content = cameraControl;  // MainScreen 是你定义的一个用户控件或界面
        }

        private void IOConfigure_Click(object sender, RoutedEventArgs e)
        {
            // 将 ContentControl 显示的内容更改为 "主界面" 内容
            SetSelectedButton(sender as Button);
            ManualControlDisplay.Content = iOConfigure;  // MainScreen 是你定义的一个用户控件或界面
        }

        private void SetSelectedButton(Button btn)
        {
            // 清除所有按钮的选中状态
            AxisDebug.Tag = null;
            AirDebug.Tag = null;
            CameraDebug.Tag = null;
            IoDebug.Tag = null;

            if (_selectedButton != null)
                _selectedButton.Tag = null; // 取消之前选中的样式

            btn.Tag = "Selected"; // 设置当前选中
            _selectedButton = btn;


        }
    }
}
