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

namespace AkribisFAM.Windows
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {
        private int _currentAxis;
        private AxisIntegerToStringDic _axisDic;
        private bool isJogging = false;
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
            _currentAxis = 1;
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

            //设置脉冲触发的位置
            AAMotionAPI.SetSingleEventPEG(GlobalManager.Current._Agm800.controller, AxisRef.B, 70000, 1, null, null);
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

            //20250429 
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetGroup(AxisRef.A).ClearBuffer();

            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (Enum.TryParse<AxisRef>(axisName, out AxisRef axisRef))
            {
                AAMotionAPI.MoveAbs(GlobalManager.Current._Agm800.controller, axisRef, targetPos);
            }
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).Begin();
            Thread.Sleep(100);
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).Stop();
            //AAMotionAPI.Pause(GlobalManager.Current._Agm800.controller);
            Thread.Sleep(5000);
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).Begin();

        }

        private void JogForwordButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isJogging)
            {
                isJogging = true;
                StartJogging_Forword();
            }
        }
        private void JogForwordButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isJogging = false;
            StopMove(CurrentAxis);
        }

        private void JogBackwordButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isJogging)
            {
                isJogging = true;
                StartJogging_Backword();
            }
        }
        private void JogBackwordButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isJogging = false;
            StopMove(CurrentAxis);
        }



        private void StartJogging_Forword()
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            int dir = 1;
            int.TryParse(Velocitytext.Text,out int vel);
            if (GlobalManager.Current._Agm800.controller.IsConnected)
            {
                if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(axisName, out AxisRef axisRef))
                {
                    AAMotionAPI.Jog(GlobalManager.Current._Agm800.controller, axisRef, vel * dir);
                }
            }
        }

        private void StartJogging_Backword()
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            int dir = -1;
            int.TryParse(Velocitytext.Text, out int vel);
            if (GlobalManager.Current._Agm800.controller.IsConnected)
            {
                if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(axisName, out AxisRef axisRef))
                {
                    AAMotionAPI.Jog(GlobalManager.Current._Agm800.controller, axisRef, vel * dir);
                }
            }
        }

        //TODO 一定要判断以后轴号是不是跟当初设置移动的一致，如果修改了轴号再停止运动，需要提示
        public void StopMove(int Axis)
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            if (GlobalManager.Current._Agm800.controller.IsConnected)
            {
                if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(axisName, out AxisRef axisRef))
                {
                    GlobalManager.Current._Agm800.controller.GetAxis(axisRef).Stop();
                }
            }

        }
        

        private void ReturnToZero_Click(object sender, RoutedEventArgs e)
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            if (!GlobalManager.Current._Agm800.controller.IsConnected) return;

            if (GlobalManager.Current._Agm800.axisRefs.TryGetValue(axisName, out AxisRef axisRef))
            {
                try
                {
                    AAMotionAPI.Home(GlobalManager.Current._Agm800.controller, axisRef, "D:\\Home.hseq");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("轴使能报错 :" + ex.Message);
                }
            }
        }      

        private void AxiscomboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string axisName = _axisDic.GetAxisName(CurrentAxis);
            axisName = "A";
            GlobalManager.Current._Agm800.axisRefs.TryGetValue(axisName, out AxisRef axisRef);
            GlobalManager.Current._Agm800.controller.GetGroup(AxisRef.A).ClearBuffer();
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.A).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.B).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.C).MotionMode = 11;
            GlobalManager.Current._Agm800.controller.GetAxis(AxisRef.D).MotionMode = 11;

            AAMotionAPI.LinearAbsoluteXYZ(GlobalManager.Current._Agm800.controller, 500000, 400000, 300000, 120000, 20000);
            GlobalManager.Current._Agm800.controller.GetGroup(AxisRef.A).Begin();
            //GlobalManager.Current._Agm800.controller.GetGroup(AxisRef.A).Begin();
        }

    }
}
