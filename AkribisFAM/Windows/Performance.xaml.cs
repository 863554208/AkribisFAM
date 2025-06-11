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
using System.Text.RegularExpressions;
using AkribisFAM.Manager;

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

        private static readonly Regex _integerRegex = new Regex("^[0-9]+$");

        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_integerRegex.IsMatch(e.Text);
        }

        private void IntegerOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                if (!_integerRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsValidFloatInput(string input)
        {
            // 支持合法浮点数格式：123、0.5、.5、123.
            return Regex.IsMatch(input, @"^-?(\d+(\.\d*)?|\.\d+)?$");
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

        private void FloatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //// 禁止 Ctrl+V 粘贴
            //if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.V))
            //{
            //    e.Handled = true;
            //}

            // 禁用中文输入法（保险起见）
            InputMethod.Current.ImeState = InputMethodState.Off;
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

        private void Applybtn_Click(object sender, RoutedEventArgs e)
        {
            string input = PlannedUPHtext.Text;
            if (int.TryParse(input, out int result))
            {
                if (result >= 0 && result <= 1400)
                {
                    for (int i = 0; i < targetUPHvalues.Count; ++i)
                    {
                        targetUPHvalues[i] = (double)result;
                    }
                    StateManager.Current.PlannedUPH = result;
                }
                else {
                    MessageBox.Show("Please input valid UPH！");
                }
            }
            else
            {
                MessageBox.Show("Please input valid UPH！");
            }
            input = PlannedYieldtext.Text;
            double doubleresult = 0;
            if (double.TryParse(input, out doubleresult))
            {
                if (doubleresult >= 0 && doubleresult <= 100)
                {
                    for (int i = 0; i < targetYieldvalues.Count; ++i)
                    {
                        targetYieldvalues[i] = (double)doubleresult;
                    }
                }
                else
                {
                    MessageBox.Show("Please input valid PlannedYield！");
                }
            }
            else
            {
                MessageBox.Show("Please input valid PlannedYield！");
            }
            input = PlannedProductionTimetext.Text;
            if (double.TryParse(input, out doubleresult))
            {
                StateManager.Current.PlannedProductionTime = doubleresult;
            }
            else
            {
                MessageBox.Show("Please input valid PlannedProductionTime！");
            }
        }
    }
}
