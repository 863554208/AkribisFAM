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
using System.Windows.Threading;
using AkribisFAM.CommunicationProtocol;
using Xceed.Wpf.Toolkit;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// AirControl.xaml 的交互逻辑
    /// </summary>
    public partial class AirControl : UserControl
    {
        private int[] CylinderState = new int[12];
        private int ThreadRun;
        private Dictionary<string, IO_OutFunction_Table> OutputCylinderExtendPairs { get; set; }
        private Dictionary<string, IO_OutFunction_Table> OutputCylinderRetractPairs { get; set; }
        public AirControl()
        {
            InitializeComponent();
            ThreadRun = 1;
            OutputCylinderExtendPairs = new Dictionary<string, IO_OutFunction_Table>
            {
                { "Cylinder1", IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend },
                { "Cylinder2", IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend },
                { "Cylinder3", IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend },
                { "Cylinder4", IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend },
                { "Cylinder5", IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend },
                { "Cylinder6", IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend },
                { "Cylinder7", IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend },
                { "Cylinder8", IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend },
                { "Cylinder9", IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend },
                { "Cylinder10", IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend },
                { "Cylinder11", IO_OutFunction_Table.OUT5_0Feeder1_limit_cylinder_extend },
                { "Cylinder12", IO_OutFunction_Table.OUT5_2Feeder2_limit_cylinder_extend }
            };
            OutputCylinderRetractPairs = new Dictionary<string, IO_OutFunction_Table>
            {
                { "Cylinder1", IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract },
                { "Cylinder2", IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract },
                { "Cylinder3", IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract },
                { "Cylinder4", IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract },
                { "Cylinder5", IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract },
                { "Cylinder6", IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract },
                { "Cylinder7", IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract },
                { "Cylinder8", IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract },
                { "Cylinder9", IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract },
                { "Cylinder10", IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract },
                { "Cylinder11", IO_OutFunction_Table.OUT5_1Feeder1_limit_cylinder_retract },
                { "Cylinder12", IO_OutFunction_Table.OUT5_3Feeder2_limit_cylinder_retract },
            };
        }

        public void DetectCylinderThread()
        {
            Task.Run(new Action(() =>
            {
                while (ThreadRun == 1)
                {
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos])
                    {
                        C1pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[0] = 1;
                    }
                    else
                    {
                        C1pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos])
                    {
                        C1pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[0] = 0;
                    }
                    else
                    {
                        C1pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos])
                    {
                        C2pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[1] = 1;
                    }
                    else
                    {
                        C2pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos])
                    {
                        C2pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[1] = 0;
                    }
                    else
                    {
                        C2pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos])
                    {
                        C3pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[2] = 1;
                    }
                    else
                    {
                        C3pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos])
                    {
                        C3pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[2] = 0;
                    }
                    else
                    {
                        C3pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos])
                    {
                        C4pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[3] = 1;
                    }
                    else
                    {
                        C4pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos])
                    {
                        C4pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[3] = 0;
                    }
                    else
                    {
                        C4pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos])
                    {
                        C5pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[4] = 1;
                    }
                    else
                    {
                        C5pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos])
                    {
                        C5pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[4] = 0;
                    }
                    else
                    {
                        C5pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos])
                    {
                        C6pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[5] = 1;
                    }
                    else
                    {
                        C6pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos])
                    {
                        C6pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[5] = 0;
                    }
                    else
                    {
                        C6pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos])
                    {
                        C7pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[6] = 1;
                    }
                    else
                    {
                        C7pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos])
                    {
                        C7pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[6] = 0;
                    }
                    else
                    {
                        C7pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos])
                    {
                        C8pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[7] = 1;
                    }
                    else
                    {
                        C8pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos])
                    {
                        C8pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[7] = 0;
                    }
                    else
                    {
                        C8pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos])
                    {
                        C9pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[8] = 1;
                    }
                    else
                    {
                        C9pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos])
                    {
                        C9pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[8] = 0;
                    }
                    else
                    {
                        C9pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_6Stopping_cylinder_4_extend_InPos])
                    {
                        C10pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[9] = 1;
                    }
                    else
                    {
                        C10pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos])
                    {
                        C10pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[9] = 0;
                    }
                    else
                    {
                        C10pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_8Feeder1_limit_cylinder_extend_InPos])
                    {
                        C11pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[10] = 1;
                    }
                    else
                    {
                        C11pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_9Feeder1_limit_cylinder_retract_InPos])
                    {
                        C11pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[10] = 0;
                    }
                    else
                    {
                        C11pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }

                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_10Feeder2_limit_cylinder_extend_InPos])
                    {
                        C12pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[11] = 1;
                    }
                    else
                    {
                        C12pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    if (IOManager.Instance.INIO_status[(int)IO_INFunction_Table.IN4_11Feeder2_limit_cylinder_retract_InPos])
                    {
                        C12pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                        CylinderState[11] = 0;
                    }
                    else
                    {
                        C12pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                    }
                    Thread.Sleep(200);
                }
            }
            ));
        }


        private int f = 1;
        private void Cylinder_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = int.Parse(button.Name.ToString().Substring(8, button.Name.ToString().Length-8));
            //if (CylinderState[index-1] == 0)
            //{
            //    IOManager.Instance.IO_ControlStatus(OutputCylinderExtendPairs[button.Name.ToString()], 1);
            //    IOManager.Instance.IO_ControlStatus(OutputCylinderRetractPairs[button.Name.ToString()], 0);
            //}
            //else
            //{
            //    IOManager.Instance.IO_ControlStatus(OutputCylinderExtendPairs[button.Name.ToString()], 0);
            //    IOManager.Instance.IO_ControlStatus(OutputCylinderRetractPairs[button.Name.ToString()], 1);
            //}
            if (f == 1)
            {
                C1pos1.Fill = new SolidColorBrush(Colors.LightGreen);
                C1pos2.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                f = 0;
            }
            else {
                C1pos1.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                C1pos2.Fill = new SolidColorBrush(Colors.LightGreen);
                f = 1;
            }
        }
    }
}
