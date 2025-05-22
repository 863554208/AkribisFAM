using System;
using System.Collections.Generic;
using System.Linq;
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
        public TeachingWindow(List<int> axisIndexes)
        {
            InitializeComponent();

            AxisIndexes = axisIndexes;
            initUI();
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

        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveAbs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveRel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
