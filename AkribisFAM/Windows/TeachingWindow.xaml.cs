using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using AAMotion;
using AkribisFAM.WorkStation;
using LiveCharts.Wpf;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// TeachingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TeachingWindow : Window
    {
        List<TextBox> textBoxes = new List<TextBox>();

        List<int> AxisIndexes = new List<int>();
        public event Action<double, double, double, double> TeachingDataReady;
        private DispatcherTimer _timer;
        int aixsIsChecked;
        string axisNow;
        bool isJogging;
        public TeachingWindow(List<int> axisIndexes)
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.2);
            _timer.Tick += Timer_Tick;
            AxisIndexes = axisIndexes;
            initUI();
            aixsIsChecked = -1;
            axisNow = null;
            isJogging = false;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateAxisInfo();

        }

        private bool CheckConn()
        {
            foreach (var item in AAmotionFAM.AGM800.Current.controller)
            {
                if (!item.IsConnected)
                {
                    return false;
                }
            }
            return true;
        }

        public static UIElement GetChildByIndex(Panel panel, int index)
        {
            if (panel == null || index < 0 || index >= panel.Children.Count)
                return null;

            return panel.Children[index];
        }

        private Object FindObject(string name)
        {
            Object obj = this.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
            return obj;
        }

        private void UpdateAxisInfo() {
            for (int i = 0; i < AxisPanel.Children.Count; ++i)
            {
                if (AxisPanel.Children[i] is RadioButton radioButton)
                {
                    if (radioButton.IsChecked == true) {
                        aixsIsChecked = i - 1;
                        axisNow = radioButton.Content.ToString();
                    }

                }
            }
            try
            {
                if (aixsIsChecked >= 0 && aixsIsChecked < 4 && CheckConn())
                {
                    AxisName axisName = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked]);
                    int agmIndex = (int)axisName / 8;
                    int axisRefNum = (int)axisName % 8;
                    var nowPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Pos;
                    string textname = "Tb" + axisNow + "Data";
                    TextBox tb = (TextBox)FindObject(textname);
                    tb.Text = AkrAction.Current.ToMilimeter(axisName, nowPos).ToString();
                }
            }
            catch {
                
            }

        }

        private void initUI()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            textBoxes.Add(TbXInput);
            textBoxes.Add(TbYInput);
            textBoxes.Add(TbZInput);
            textBoxes.Add(TbRInput);

            foreach (var item in textBoxes)
            {
                item.PreviewTextInput += FloatTextBox_PreviewTextInput;
                item.PreviewKeyDown += FloatTextBox_PreviewKeyDown;
                DataObject.AddPastingHandler(item, FloatTextBox_Pasting);

                // 禁用输入法
                InputMethod.SetIsInputMethodEnabled(item, false);
            }
        }

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                string newText = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                       .Insert(tb.SelectionStart, e.Text);

                e.Handled = !IsValidFloatInput(newText);
            }
        }

        private void FloatTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidFloatInput(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void FloatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 禁用中文输入法（保险起见）
            InputMethod.Current.ImeState = InputMethodState.Off;
        }

        private bool IsValidFloatInput(string input)
        {
            // 支持合法浮点数格式：123、0.5、.5、123.
            return Regex.IsMatch(input, @"^-?(\d+(\.\d*)?|\.\d+)?$");
        }

        private void BtnGetTeaching_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TbXData.Text, out double xVal) &&
                double.TryParse(TbYData.Text, out double yVal) &&
                double.TryParse(TbZData.Text, out double zVal) &&
                double.TryParse(TbRData.Text, out double rVal))
            {
                // 解析成功，使用 xVal, yVal, zVal, rVal
                if (TeachingDataReady != null)
                {
                    TeachingDataReady(xVal, yVal, zVal, rVal);
                }
            }
        }

        private void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckConn()) return;
                AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
                int agmIndex = (int)axis / 8;
                int axisRefNum = (int)axis % 8;
                AAMotionAPI.MotorOn(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
            }
            catch {
            }
        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckConn()) return;
                AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
                int agmIndex = (int)axis / 8;
                int axisRefNum = (int)axis % 8;
                AAMotionAPI.MotorOff(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum));
            }
            catch
            {

            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnMoveAbs_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConn()) return;
            double teachvel = 20;
            double posValue = 0.0;
            string tbname = "Tb" + axisNow + "Input";
            TextBox tb = (TextBox)FindObject(tbname);
            bool posSuccess = double.TryParse(tb.Text.Trim(), out posValue);
            if (!posSuccess) { return; }
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
            await Task.Run(() =>
            {
                AkrAction.Current.Move(axis, posValue, teachvel);
            });
        }

        private async void btnMoveRel_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConn()) return;
            double teachvel = 20;
            double posValue = 0.0;
            string tbname = "Tb" + axisNow + "Input";
            TextBox tb = (TextBox)FindObject(tbname);
            bool posSuccess = double.TryParse(tb.Text.Trim(), out posValue);
            if (!posSuccess) { return; }
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
            await Task.Run(() =>
            {
                AkrAction.Current.MoveRel(axis, posValue, teachvel);
            });
        }

        private async void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConn()) return;
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
            await Task.Run(() =>
            {
                AkrAction.Current.Stop(axis);
            });
        }

        private void JogForwordButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            if (!isJogging)
            {
                isJogging = true;
                double teachvel = 20;

                AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
                AkrAction.Current.JogMove(axis, 1, teachvel);
            }
        }
        private void JogForwordButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            isJogging = false;
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
            AkrAction.Current.Stop(axis);
        }

        private void JogBackwordButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            if (!isJogging)
            {
                isJogging = true;
                double teachvel = 20;
                int a = AxisIndexes[aixsIsChecked];
                AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
                AkrAction.Current.JogMove(axis, -1, teachvel);
            }
        }
        private void JogBackwordButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            isJogging = false;
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(AxisIndexes[aixsIsChecked] + 1);
            AkrAction.Current.Stop(axis);
        }
    }
}
