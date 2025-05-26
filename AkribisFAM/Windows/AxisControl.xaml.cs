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
using System.Text.RegularExpressions;
using static AkribisFAM.Windows.AxisControl;
using LiveCharts.Wpf;
using static AkribisFAM.GlobalManager;
using AkribisFAM.WorkStation;
using System.Windows.Threading;
using System.Reflection;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {

        const int AxisNum = 25;
        public int nowAxisIndex = 0;
        List<Ellipse> statList = new List<Ellipse>();
        SingleAxis temp = new SingleAxis();
        private DispatcherTimer _timer; 

        private bool isJogging = false;


        private List<SingleAxis> _axisDataList = new List<SingleAxis>();
        private List<AxisRowControls> axisRows = new List<AxisRowControls>();

        //private Dictionary<int, string> _homefileDict = new Dictionary<int, string>();

        public List<SingleAxis> AxisDataList
        {
            get { return _axisDataList; }
            set { _axisDataList = value; }
        }

        public class AxisRowControls
        {
            public int AxisIndex { get; set; } // 轴索引

            public TextBlock AxisNameText { get; set; }
            public TextBlock CurrentPosText { get; set; }
            public TextBlock TargetPosText { get; set; }

            public Ellipse EnableStatus { get; set; }
            public Ellipse InPosStatus { get; set; }
            public Ellipse HomeStatus { get; set; }
            public Ellipse ErrStatus { get; set; }
            public Ellipse LimitPStatus { get; set; }
            public Ellipse LimitNStatus { get; set; }
            public Ellipse SwLimitPStatus { get; set; }
            public Ellipse SwLimitNStatus { get; set; }
        }

        public class SingleAxis
        {
            public double nowPos;
            public double tarpos;
            public double vel;

            //依次使能，到位，原点，硬限位+，硬限位-。报警，软限位+，软限位-
            public List<bool> AxisStat = new List<bool>() { false, false, false, false, false, false, false, false };
        }


        public AxisControl()
        {
            InitializeComponent();

            initAxisUI("Axis");

            statList.Add(AxisEnStat);
            statList.Add(AxisInPosStat);
            statList.Add(AxisHomeStat);
            statList.Add(AxisLimitStatP);
            statList.Add(AxisLimitStatN);
            statList.Add(AxisErrStat);
            statList.Add(AxisSwLimitStatP);
            statList.Add(AxisSwLimitStatN);
    
            DataContext = this;

            AxisDataList.Clear();
            for (int i = 0; i < 25; i++)
            {
                SingleAxis temp = new SingleAxis();
                AxisDataList.Add(temp);
            }

            initUIAllAxisStat();


            //示例
            // 设置第3个轴的目标位置
            axisRows[2].TargetPosText.Text = "123.456";

            // 设置第5个轴的报警状态为红色
            axisRows[4].ErrStatus.Fill = Brushes.Red;

            // 设置第10个轴的报警状态为绿色
            axisRows[9].HomeStatus.Fill = Brushes.Green;

            // 设置第25个轴的目标位置
            axisRows[24].CurrentPosText.Text = "99925.55";


            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(300);
            _timer.Tick += Timer_Tick;
            _timer.Start();


        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (AAmotionFAM.AGM800.Current.controller[0].IsConnected)
            {
                if (nowAxisIndex < 0)
                {

                }
                else
                {
                    #region 获取轴的信息
                    AxisName axisName = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
                    int agmIndex = (int)axisName / 8;
                    int axisRefNum = (int)axisName % 8;
                    var nowPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Pos;
                    var tarPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).AbsTrgt;
                    //var vel = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Vel.Value;
                    bool MotorOn = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MotorOn == 1 ? true : false;
                    bool inPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat == 4 ? true : false;
                    bool homing = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).HomingStat == 4 ? true : false;
                    //mode2
                    temp.nowPos = Math.Round(AkrAction.Current.ToMilimeter(axisName, nowPos), 3);
                    temp.tarpos = Math.Round(AkrAction.Current.ToMilimeter(axisName, tarPos), 3);
                    //temp.vel = AkrAction.Current.ToMilimeter(axisName, vel);
                    temp.AxisStat[0] = MotorOn;
                    temp.AxisStat[1] = inPos;
                    temp.AxisStat[2] = homing;
                    #endregion
                    Console.WriteLine("123123123");
                    //updateAxisData(temp);
                    AsyncUpdateAxisData(temp);
                }
            }
            _pressProgress += 300;

            if (_pressProgress >= PressDuration * 1000 && Home_trigger == 1)
            {
                MessageBoxResult result = MessageBox.Show("Start Homing?", "Confirming", MessageBoxButton.OKCancel);
                AxisName axisName = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
                if (result == MessageBoxResult.OK)
                {
                    string path = Directory.GetCurrentDirectory() + "\\homming\\" + axisName.ToString() + "_homing.hseq";
                    int agmIndex = (int)axisName / 8;
                    int axisRefNum = (int)axisName % 8;
                    try
                    {
                        AAMotionAPI.Home(AAmotionFAM.AGM800.Current.controller[agmIndex], GlobalManager.Current.GetAxisRefFromInteger(axisRefNum), path);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Home Axis Failed！:" + ex.Message);
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {

                }

                Home_trigger = 0;
            }
        }

        private int _pressProgress;
        private const int PressDuration = 3; // 3秒
        private int Home_trigger = 0;
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _pressProgress = 0;
            Home_trigger = 1;
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }


        //由于CboxNowAxis和AxisListBox后台生成，没有用上绑定。切换语言时需要调用。
        private void initAxisUI(string prenName)
        {
            for (int i = 1; i <= AxisNum; i++)
            {
                string axisName = GlobalManager.Current.GetAxisStringFromInteger(i);

                //string axisName = $"{prenName} {i}";

                // 添加到 ComboBox（CboxNowAxis）
                CboxNowAxis.Items.Add(axisName);

                // 添加到 ListBox（AxisListBox）
                AxisListBox.Items.Add(axisName);

            }

            // 可选：设置默认选中第一个
            if (CboxNowAxis.Items.Count > 0)
                CboxNowAxis.SelectedIndex = 0;

            if (AxisListBox.Items.Count > 0)
                AxisListBox.SelectedIndex = 0;
        }

        private void updateAxisData(SingleAxis axis)
        {
            Dispatcher.Invoke(() =>
            {
                tbNowPos.Text = axis.nowPos.ToString();
                tbTargetPos.Text = axis.tarpos.ToString();
                tbAxisVel.Text = axis.vel.ToString();

                if (axis.AxisStat.Count < statList.Count)
                {
                    return;
                }
                for (int i = 0; i < statList.Count; ++i)
                {
                    if (axis.AxisStat[i])
                    {
                        statList[i].Fill = Brushes.Green;
                    }
                    else
                    {
                        statList[i].Fill = Brushes.Gray;
                    }
                }
            });
        }

        private async Task AsyncUpdateAxisData(SingleAxis axis)
        {
            // 异步方式更新 UI，避免阻塞 UI 线程
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                tbNowPos.Text = axis.nowPos.ToString();
                tbTargetPos.Text = axis.tarpos.ToString();
                tbAxisVel.Text = axis.vel.ToString();

                if (axis.AxisStat.Count < statList.Count)
                {
                    return;
                }

                for (int i = 0; i < statList.Count; ++i)
                {
                    if (axis.AxisStat[i])
                    {
                        statList[i].Fill = Brushes.Green;
                    }
                    else
                    {
                        statList[i].Fill = Brushes.Gray;
                    }
                }
            }));
        }


        private void setUIAxisData(int index)
        {
            if (index < 0 || index >= _axisDataList.Count)
            {
                return; 
            }
            SingleAxis axis = _axisDataList[index]; // 安全访问

            #region 获取轴的信息
            AxisName axisName = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
            int agmIndex = (int)axisName / 8;
            int axisRefNum = (int)axisName % 8;
            var nowPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Pos;
            var tarPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).AbsTrgt;
            //var vel = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).Vel.Value;
            int vel = 0;
            bool MotorOn = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).MotorOn ==1?true:false;
            bool inPos = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).InTargetStat == 4 ? true : false;
            bool homing = AAmotionFAM.AGM800.Current.controller[agmIndex].GetAxis(GlobalManager.Current.GetAxisRefFromInteger(axisRefNum)).HomingStat==100 ? true: false;
            //mode2
            axis.nowPos = AkrAction.Current.ToMilimeter(axisName,nowPos);
            axis.tarpos = AkrAction.Current.ToMilimeter(axisName,tarPos);
            axis.vel = AkrAction.Current.ToMilimeter(axisName, vel);
            axis.AxisStat[0] = MotorOn;
            axis.AxisStat[1] = inPos;
            axis.AxisStat[2] = homing;
            #endregion

            updateAxisData(axis);
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

        private void CboxNowAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int nowAxis = CboxNowAxis.SelectedIndex;
            tbAxisName.Text = CboxNowAxis.SelectedValue.ToString();
            if (AxisListBox.SelectedIndex != nowAxis)
            {
                AxisListBox.SelectedIndex = nowAxis;
            }
            nowAxisIndex = nowAxis;

            if (!CheckConn()) return;

            setUIAxisData(nowAxis);

        }

        private void AxisListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int nowAxis = AxisListBox.SelectedIndex;
            tbAxisName.Text = AxisListBox.SelectedItem.ToString();
            if (CboxNowAxis.SelectedIndex != nowAxis)
            {
                CboxNowAxis.SelectedIndex = nowAxis;
            }

            nowAxisIndex = nowAxis;

            if (!CheckConn()) return;

            setUIAxisData(nowAxis);
        }

        private async void BtnMove_Click(object sender, RoutedEventArgs e)
        {

            double posValue, velValue, accValue;

            bool posSuccess = double.TryParse(TbTargetPos.Text.Trim(), out posValue);
            bool velSuccess = double.TryParse(TbVel.Text.Trim(), out velValue);
            bool accSuccess = double.TryParse(TbAccDec.Text.Trim(), out accValue);

            bool success = posSuccess && velSuccess && accSuccess;
            //bool success =
            //    double.TryParse(TbTargetPos.Text.Trim(), out double posValue) &&
            //    double.TryParse(TbVel.Text.Trim(), out double velValue) &&
            //    double.TryParse(TbAccDec.Text.Trim(), out double accValue);

            if (!success)
            {
                MessageBox.Show("输入格式有误，请检查目标位置、速度和加速度！");
                return;
            }

            if (!CheckConn()) return;

            //Todo MoveAbs(nowAxisIndex,posValue,velValue,accValue)
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex+1);
            //AkrAction.Current.Move(axis, posValue, velValue, accValue);
            await Task.Run(() =>
            {
                AkrAction.Current.Move(axis, posValue, velValue, accValue);
            });
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConn()) return;

            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
            AkrAction.Current.Stop(axis);


        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConn()) return;
            //Todo Home(nowAxisIndex)

        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConn()) return;
            var btn = sender as System.Windows.Controls.Primitives.ToggleButton;
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex+1);
            if (btn != null && btn.IsChecked == true)
            {
                AkrAction.Current.axisEnable(axis, true);
            }
            else
            {
                AkrAction.Current.axisEnable(axis, false);
            }
        }
        private void ToggleAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConn()) return;
            var btn = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (btn != null && btn.IsChecked == true)
            {
                AkrAction.Current.axisAllEnable(true);
            }
            else
            {
                AkrAction.Current.axisAllEnable(false);
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
            //// 禁止 Ctrl+V 粘贴
            //if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.V))
            //{
            //    e.Handled = true;
            //}

            // 禁用中文输入法（保险起见）
            InputMethod.Current.ImeState = InputMethodState.Off;
        }

        private bool IsValidFloatInput(string input)
        {
            // 支持合法浮点数格式：123、0.5、.5、123.
            return Regex.IsMatch(input, @"^-?(\d+(\.\d*)?|\.\d+)?$");
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (AxisListBox.Items.Count == 0)
                return;

            int currentIndex = AxisListBox.SelectedIndex;

            if (e.Delta > 0) // 滚轮向上
            {
                currentIndex--;
            }
            else if (e.Delta < 0) // 滚轮向下
            {
                currentIndex++;
            }

            //AxisListBox.SelectedIndex = currentIndex;
        }

        private void JogForwordButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            if (!isJogging)
            {
                isJogging = true;
                StartJogging_Forword();
            }
        }
        private void JogForwordButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            isJogging = false;
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
            AkrAction.Current.Stop(axis);
        }
        private void JogBackwordButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            if (!isJogging)
            {
                isJogging = true;
                StartJogging_Backword();
            }
        }
        private void JogBackwordButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!CheckConn()) return;

            isJogging = false;
            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
            AkrAction.Current.Stop(axis);
        }
        private void StartJogging_Forword()
        {
            double velValue;
            bool velSuccess = double.TryParse(TbVel.Text.Trim(), out velValue);
            bool success = velSuccess;

            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
            AkrAction.Current.JogMove(axis,1, velValue);
        }
        private void StartJogging_Backword()
        {
            double velValue;
            bool velSuccess = double.TryParse(TbVel.Text.Trim(), out velValue);
            bool success = velSuccess;

            AxisName axis = GlobalManager.Current.GetAxisNameFromInteger(nowAxisIndex + 1);
            AkrAction.Current.JogMove(axis, -1, velValue);
        }

        private void initUIAllAxisStat()
        {
            const int AxisNum = 25;
            axisRows.Clear();

            for (int i = 0; i < AxisNum; i++)
            {
                GridAllAxisStat.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var lightGrayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                var bgColor = (i % 2 == 0) ? Brushes.White : lightGrayBrush;

                string axisName = GlobalManager.Current.GetAxisStringFromInteger(i);

                TextBlock axisNameTb, currentPosTb, targetPosTb;

                GridAllAxisStat.Children.Add(CreateTextBlock(axisName, i + 1, 0, bgColor, out axisNameTb));
                GridAllAxisStat.Children.Add(CreateTextBlock("", i + 1, 1, bgColor, out currentPosTb));
                GridAllAxisStat.Children.Add(CreateTextBlock("", i + 1, 2, bgColor, out targetPosTb));

                var rowControls = new AxisRowControls
                {
                    AxisIndex = i,
                    AxisNameText = axisNameTb,
                    CurrentPosText = currentPosTb,
                    TargetPosText = targetPosTb,
                };



                // 创建 8 个状态圆
                var ellipses = new List<Ellipse>();
                for (int col = 3; col <= 10; col++)
                {
                    var ellipse = CreateEllipseCell(i + 1, col, bgColor);
                    GridAllAxisStat.Children.Add(ellipse.container);
                    ellipses.Add(ellipse.ellipse);
                }

                // 分配给对应字段
                rowControls.EnableStatus = ellipses[0];
                rowControls.InPosStatus = ellipses[1];
                rowControls.HomeStatus = ellipses[2];
                rowControls.ErrStatus = ellipses[3];
                rowControls.LimitPStatus = ellipses[4];
                rowControls.LimitNStatus = ellipses[5];
                rowControls.SwLimitPStatus = ellipses[6];
                rowControls.SwLimitNStatus = ellipses[7];

                axisRows.Add(rowControls);
            }

        }

        private Border CreateTextBlock(string text, int row, int col, Brush bg, out TextBlock tbOut)
        {
            var tb = new TextBlock
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2)
            };

            var border = new Border
            {
                Background = bg,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Child = tb
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);

            tbOut = tb;
            return border;
        }

        private (Ellipse ellipse, Border container) CreateEllipseCell(int row, int col, Brush bg)
        {
            var ellipse = new Ellipse
            {
                Width = 15,
                Height = 15,
                Fill = Brushes.Gray,
                StrokeThickness = 1
            };

            var viewbox = new Viewbox
            {
                Width = 15,
                Height = 15,
                Stretch = Stretch.Uniform,
                Child = ellipse
            };

            var border = new Border
            {
                Background = bg,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Child = viewbox
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            return (ellipse, border);
        }

    }
}
