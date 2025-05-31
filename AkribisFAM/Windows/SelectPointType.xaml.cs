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
    /// SelectPointType.xaml 的交互逻辑
    /// </summary>
    public partial class SelectPointType : Window
    {
        public int SelectedType { get; private set; }
        public int SelectedRow { get; private set; }
        public int SelectedCol { get; private set; }

        public List<int> AxexIndexList { get; private set; }
        public SelectPointType()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SelectedType = -1;
            SelectedRow = 1;
            SelectedCol = 2;

            RowInput.PreviewTextInput += PositiveIntTextBox_PreviewTextInput;
            RowInput.PreviewKeyDown += PositiveIntTextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(RowInput, PositiveIntTextBox_Pasting);
            InputMethod.SetIsInputMethodEnabled(RowInput, false);

            ColInput.PreviewTextInput += PositiveIntTextBox_PreviewTextInput;
            ColInput.PreviewKeyDown += PositiveIntTextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(ColInput, PositiveIntTextBox_Pasting);
            InputMethod.SetIsInputMethodEnabled(ColInput, false);

            for (int i = 0; i < 25; i++)
            {
                string axisName = GlobalManager.Current.GetAxisStringFromInteger(i+1);
                cBoxX.Items.Add(axisName);
                cBoxY.Items.Add(axisName);
                cBoxZ.Items.Add(axisName);
                cBoxR.Items.Add(axisName);
            }
            cBoxX.SelectedIndex = 0;
            cBoxY.SelectedIndex = 1;
            cBoxZ.SelectedIndex = 2;
            cBoxR.SelectedIndex = 3;

            AxexIndexList = new List<int>();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (raBtnSingle.IsChecked == true)
            {
                SelectedType = 0;

                AxexIndexList.Add(cBoxX.SelectedIndex);
                AxexIndexList.Add(cBoxY.SelectedIndex);
                AxexIndexList.Add(cBoxZ.SelectedIndex);
                AxexIndexList.Add(cBoxR.SelectedIndex);

                this.Close();
            }else if (raBtnMatrix.IsChecked == true)
            {
                SelectedType = 1;
                SelectedRow = int.Parse(RowInput.Text.Trim());
                SelectedCol = int.Parse(ColInput.Text.Trim());

                AxexIndexList.Add(cBoxX.SelectedIndex);
                AxexIndexList.Add(cBoxY.SelectedIndex);
                AxexIndexList.Add(cBoxZ.SelectedIndex);
                AxexIndexList.Add(cBoxR.SelectedIndex);

                this.Close();
            }
            else if (raBtnGeneral.IsChecked == true)
            {
                SelectedType = 2;
                this.Close();
            }
        }

        private void raBtnSingle_Checked(object sender, RoutedEventArgs e)
        {
            if (MatrixStackPanel == null)
            {
                return;
            }
            MatrixStackPanel.Visibility= Visibility.Collapsed;
        }

        private void raBtnMatrix_Checked(object sender, RoutedEventArgs e)
        {
            if (MatrixStackPanel == null)
            {
                return;
            }
            MatrixStackPanel.Visibility = Visibility.Visible;
        }

        private void raBtnGeneral_Checked(object sender, RoutedEventArgs e)
        {
            if (MatrixStackPanel == null)
            {
                return;
            }
            MatrixStackPanel.Visibility = Visibility.Collapsed;
        }

        private void PositiveIntTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox tb)
            {
                string newText = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                        .Insert(tb.SelectionStart, e.Text);

                e.Handled = !IsValidPositiveInt(newText);
            }
        }

        private void PositiveIntTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidPositiveInt(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void PositiveIntTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputMethod.Current.ImeState = InputMethodState.Off; // 禁用中文输入法
        }

        private bool IsValidPositiveInt(string input)
        {
            return Regex.IsMatch(input, @"^[1-9]\d*$"); // 只允许大于0的整数
        }
    }
}
