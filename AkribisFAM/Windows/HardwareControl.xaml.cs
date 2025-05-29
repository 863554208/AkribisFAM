using System.Windows;
using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// ManualControl.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareControl : UserControl
    {

        public HardwareControl()
        {
            InitializeComponent();


            //ManualControlDisplay.Content = axisControl;

            //this.btnMotionControl.Tag = "Selected";


        }

        //private void AxisConfigure_Click(object sender, RoutedEventArgs e)
        //{
        //    // 将 ContentControl 显示的内容更改为 "主界面" 内容
        //    SetSelectedButton(sender as Button);
        //    ManualControlDisplay.Content = axisControl;  // MainScreen 是你定义的一个用户控件或界面
        //}

        //private void AirControl_Click(object sender, RoutedEventArgs e)
        //{
        //    // 将 ContentControl 显示的内容更改为 "主界面" 内容
        //    SetSelectedButton(sender as Button);
        //    ManualControlDisplay.Content = airControl;  // MainScreen 是你定义的一个用户控件或界面
        //}

        //private void CameraControl_Click(object sender, RoutedEventArgs e)
        //{
        //    // 将 ContentControl 显示的内容更改为 "主界面" 内容
        //    SetSelectedButton(sender as Button);
        //    ManualControlDisplay.Content = cameraControl;  // MainScreen 是你定义的一个用户控件或界面
        //}

        //private void IOConfigure_Click(object sender, RoutedEventArgs e)
        //{
        //    // 将 ContentControl 显示的内容更改为 "主界面" 内容
        //    SetSelectedButton(sender as Button);
        //    ManualControlDisplay.Content = iOConfigure;  // MainScreen 是你定义的一个用户控件或界面
        //}

        //private void ManualControl_Click(object sender, RoutedEventArgs e)
        //{
        //    // 将 ContentControl 显示的内容更改为 "主界面" 内容
        //    SetSelectedButton(sender as Button);
        //    ManualControlDisplay.Content = manualControl;  // MainScreen 是你定义的一个用户控件或界面
        //}
        //private void SetSelectedButton(Button btn)
        //{
        //    // 清除所有按钮的选中状态
        //    btnMotionControl.Tag = null;
        //    btnPneumaticControl.Tag = null;
        //    btnVisionControl.Tag = null;
        //    btnIoControl.Tag = null;
        //    btnManualControl.Tag = null;

        //    if (_selectedButton != null)
        //        _selectedButton.Tag = null; // 取消之前选中的样式

        //    btn.Tag = "Selected"; // 设置当前选中
        //    _selectedButton = btn;


        //}

    }
}
