using System.Windows;
using System.Windows.Controls;
using AkribisFAM.AAmotionFAM;
using AGM800 = AkribisFAM.AAmotionFAM.AGM800;
using AAMotion;
using System;
using static AAComm.Extensions.AACommFwInfo;
namespace AkribisFAM.Windows
{
    /// <summary>
    /// Axis1ViewModel.xaml 的交互逻辑
    /// </summary>
    public partial class Axis1ViewModel : UserControl
    {
        public Axis1ViewModel()
        {
            InitializeComponent();
        }


        private void ConnectAGM800_Click(object sender, RoutedEventArgs e)
        {
            //string ipAddress = IpAddressTextBox.Text;

            //if (AAMotionAPI.Connect(GlobalManager.Current._Agm800.controller0, ipAddress))
            //{
            //    MessageBox.Show("连接成功");
            //}
            //else
            //{
            //    MessageBox.Show("连接失败");
            //}
        }

        private void ReturnToZero_Click(object sender, RoutedEventArgs e)
        {
            //string Axis = returnZeroAxisString.Text;

            //if (GlobalManager.Current._Agm800.controller0.IsConnected)
            //{
            //    // 使用Enum.Parse将字符串转换为AxisRef枚举值
            //    if (Enum.TryParse<AxisRef>(Axis, out AxisRef axisRef))
            //    {
            //        AAMotionAPI.SetPosition(GlobalManager.Current._Agm800.controller0, axisRef, 0);
            //        //controller.GetAxis(axisRef).SetPosition(0);
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("未连接");
            //}

        }

        private void AbsoulteMove_Click(object sender, RoutedEventArgs e)
        {
            //string Axis = returnZeroAxisString.Text;
            //int targetPos = int.Parse(AbsoultePostion.Text);

            //if (!GlobalManager.Current._Agm800.controller0.IsConnected) return;

            //if (Enum.TryParse<AxisRef>(Axis, out AxisRef axisRef))
            //{
            //    AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller0, axisRef, targetPos);
            //}
        }

        private void MotorOn_Click(object sender, RoutedEventArgs e)
        {
            //string Axis = returnZeroAxisString.Text;
            //if (!GlobalManager.Current._Agm800.controller0.IsConnected) return;

            //if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
            //{
            //    try
            //    {
            //        AAMotionAPI.MotorOn(GlobalManager.Current._Agm800.controller0, axisRef);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show("轴使能报错 :" +ex.Message);
            //    }
            //}
        }

        private void MotorOff_Click(object sender, RoutedEventArgs e)
        {
            //string Axis = returnZeroAxisString.Text;
            //if (!GlobalManager.Current._Agm800.controller0.IsConnected) return;

            //if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(Axis, out AxisRef axisRef))
            //{
            //    try
            //    {
            //        AAMotionAPI.MotorOff(GlobalManager.Current._Agm800.controller0, axisRef);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show("轴使能报错 :" + ex.Message);
            //    }
            //}
        }
    }
}
