using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
using System.Xml.Linq;
using AkribisFAM.CommunicationProtocol;
using Xceed.Wpf.Toolkit;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// AirControl.xaml 的交互逻辑
    /// </summary>
    public partial class AirControl : UserControl
    {
        private int ThreadRun;
        private Dictionary<string, IO_OutFunction_Table> OutputCylinderExtendPairs { get; set; }
        private Dictionary<string, IO_OutFunction_Table> OutputCylinderRetractPairs { get; set; }
        private Dictionary<string, IO_OutFunction_Table> OutputNozzleSupplyPairs { get; set; }
        private Dictionary<string, IO_OutFunction_Table> OutputNozzleReleasePairs { get; set; }
        private Dictionary<string, IO_OutFunction_Table> OutputNozzleBlowPairs { get; set; }
        private Dictionary<string, IO_OutFunction_Table> OutputNozzleNoBlowPairs { get; set; }
        private Dictionary<Ellipse, IO_INFunction_Table> InputCylinderPairs { get; set; }
        private Dictionary<Ellipse, IO_INFunction_Table> InputNozzlePairs { get; set; }
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
                { "Cylinder7", IO_OutFunction_Table.OUT1_124_lift_cylinder_extend },
                { "Cylinder8", IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend },
                { "Cylinder9", IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend },
                { "Cylinder10", IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend },
                { "Cylinder11", IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend },
                { "Cylinder12", IO_OutFunction_Table.OUT5_0Feeder1_limit_cylinder_extend },
                { "Cylinder13", IO_OutFunction_Table.OUT5_2Feeder2_limit_cylinder_extend },
                { "Cylinder14", IO_OutFunction_Table.OUT4_0Pneumatic_Claw_A }
            };
            OutputCylinderRetractPairs = new Dictionary<string, IO_OutFunction_Table>
            {
                { "Cylinder1", IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract },
                { "Cylinder2", IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract },
                { "Cylinder3", IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract },
                { "Cylinder4", IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract },
                { "Cylinder5", IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract },
                { "Cylinder6", IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract },
                { "Cylinder7", IO_OutFunction_Table.OUT1_134_lift_cylinder_retract },
                { "Cylinder8", IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract },
                { "Cylinder9", IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract },
                { "Cylinder10", IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract },
                { "Cylinder11", IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract },
                { "Cylinder12", IO_OutFunction_Table.OUT5_1Feeder1_limit_cylinder_retract },
                { "Cylinder13", IO_OutFunction_Table.OUT5_3Feeder2_limit_cylinder_retract },
                { "Cylinder14", IO_OutFunction_Table.OUT4_1Pneumatic_Claw_B },
            };
            OutputNozzleSupplyPairs = new Dictionary<string, IO_OutFunction_Table>
            {
                { "Suctionnozzle1", IO_OutFunction_Table.OUT3_0PNP_Gantry_vacuum1_Supply },
                { "Suctionnozzle2", IO_OutFunction_Table.OUT3_2PNP_Gantry_vacuum2_Supply },
                { "Suctionnozzle3", IO_OutFunction_Table.OUT3_4PNP_Gantry_vacuum3_Supply },
                { "Suctionnozzle4", IO_OutFunction_Table.OUT3_6PNP_Gantry_vacuum4_Supply }
            };
            OutputNozzleReleasePairs = new Dictionary<string, IO_OutFunction_Table>
            {
                { "Suctionnozzle1", IO_OutFunction_Table.OUT3_1PNP_Gantry_vacuum1_Release },
                { "Suctionnozzle2", IO_OutFunction_Table.OUT3_3PNP_Gantry_vacuum2_Release },
                { "Suctionnozzle3", IO_OutFunction_Table.OUT3_5PNP_Gantry_vacuum3_Release },
                { "Suctionnozzle4", IO_OutFunction_Table.OUT3_7PNP_Gantry_vacuum4_Release }
            };
            OutputNozzleBlowPairs = new Dictionary<string, IO_OutFunction_Table>
            {
                { "Suctionnozzle11", IO_OutFunction_Table.OUT3_8solenoid_valve1_A },
                { "Suctionnozzle21", IO_OutFunction_Table.OUT3_10solenoid_valve2_A },
                { "Suctionnozzle31", IO_OutFunction_Table.OUT3_12solenoid_valve3_A },
                { "Suctionnozzle41", IO_OutFunction_Table.OUT3_14solenoid_valve4_A }
            };
            OutputNozzleNoBlowPairs = new Dictionary<string, IO_OutFunction_Table>
            {
                { "Suctionnozzle11", IO_OutFunction_Table.OUT3_9solenoid_valve1_B },
                { "Suctionnozzle21", IO_OutFunction_Table.OUT3_11solenoid_valve2_B },
                { "Suctionnozzle31", IO_OutFunction_Table.OUT3_13solenoid_valve3_B },
                { "Suctionnozzle41", IO_OutFunction_Table.OUT3_15solenoid_valve4_B }
            };
            InputCylinderPairs = new Dictionary<Ellipse, IO_INFunction_Table>
            {
                { C1pos1, IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos },
                { C1pos2, IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos },
                { C2pos1, IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos },
                { C2pos2, IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos },
                { C3pos1, IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos },
                { C3pos2, IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos },
                { C4pos1, IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos },
                { C4pos2, IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos },
                { C5pos1, IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos },
                { C5pos2, IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos },
                { C6pos1, IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos },
                { C6pos2, IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos },
                { C7pos1, IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos },
                { C7pos2, IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos },
                { C8pos1, IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos },
                { C8pos2, IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos },
                { C9pos1, IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos },
                { C9pos2, IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos },
                { C10pos1, IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos },
                { C10pos2, IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos },
                { C11pos1, IO_INFunction_Table.IN3_6Stopping_cylinder_4_extend_InPos },
                { C11pos2, IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos },
                { C12pos1, IO_INFunction_Table.IN4_8Feeder1_limit_cylinder_extend_InPos },
                { C12pos2, IO_INFunction_Table.IN4_9Feeder1_limit_cylinder_retract_InPos },
                { C13pos1, IO_INFunction_Table.IN4_10Feeder2_limit_cylinder_extend_InPos },
                { C13pos2, IO_INFunction_Table.IN4_11Feeder2_limit_cylinder_retract_InPos },
                { C14pos1, IO_INFunction_Table.IN3_9Claw_extend_in_position },
                { C14pos2, IO_INFunction_Table.IN3_10Claw_retract_in_position },
            };
            InputNozzlePairs = new Dictionary<Ellipse, IO_INFunction_Table>
            {
                { SN1negativepressure, IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback },
                { SN2negativepressure, IO_INFunction_Table.IN3_13PNP_Gantry_vacuum2_Pressure_feedback },
                { SN3negativepressure, IO_INFunction_Table.IN3_14PNP_Gantry_vacuum3_Pressure_feedback },
                { SN4negativepressure, IO_INFunction_Table.IN3_15PNP_Gantry_vacuum4_Pressure_feedback },
            };
            DetectIOThread();
            InitOutIOState();
        }

        private void InitOutIOState() {
            for (int i = 1; i < 14; i++)
            {
                string name = "Cylinder" + i.ToString();
                Button b1 = (Button)FindObject(name);
                if (IOManager.Instance.INIO_status[(int)OutputCylinderExtendPairs[name]] == 0)
                {
                    b1.Content = "Retract";
                }
                if (IOManager.Instance.INIO_status[(int)OutputCylinderRetractPairs[name]] == 0)
                {
                    b1.Content = "Extend";
                }
            }
            for (int i = 1; i < 5; i++)
            {
                string name1 = "Suctionnozzle" + i.ToString();
                Button b1 = (Button)FindObject(name1);
                if (IOManager.Instance.INIO_status[(int)OutputNozzleSupplyPairs[name1]] == 0)
                {
                    b1.Content = "Release";
                }
                if (IOManager.Instance.INIO_status[(int)OutputNozzleReleasePairs[name1]] == 0)
                {
                    b1.Content = "Supply";
                }
                string name2 = "Suctionnozzle" + i.ToString() + "1";
                Button b2 = (Button)FindObject(name2);
                b2.Content = "Blow";
            }
        }

        public void DetectIOThread()
        {
            Task.Run(new Action(() =>
            {
                while (ThreadRun == 1)
                {
                    foreach (KeyValuePair<Ellipse, IO_INFunction_Table > entry in InputCylinderPairs)
                    {
                        if (IOManager.Instance.INIO_status[(int)entry.Value] == 0)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                entry.Key.Fill = new SolidColorBrush(Colors.LightGreen);
                            }));
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                entry.Key.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                            }));
                        }
                    }
                    foreach (KeyValuePair<Ellipse, IO_INFunction_Table> entry in InputNozzlePairs)
                    {
                        if (IOManager.Instance.INIO_status[(int)entry.Value] == 0)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                entry.Key.Fill = new SolidColorBrush(Colors.LightGreen);
                            }));
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                entry.Key.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6C6C6"));
                            }));
                        }
                    }
                    Thread.Sleep(200);
                }
            }
            ));
        }

        private Object FindObject(string name)
        {
            Object obj = this.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
            return obj;
        }

        private int[] f = new int[14];
        private void Cylinder_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string p1name = "C" + button.Name.ToString().Substring(8, button.Name.ToString().Length - 8) + "pos1";
            string p2name = "C" + button.Name.ToString().Substring(8, button.Name.ToString().Length - 8) + "pos2";
            Ellipse p1 = (Ellipse)FindObject(p1name);
            Ellipse p2 = (Ellipse)FindObject(p2name);
            int index = int.Parse(button.Name.ToString().Substring(8, button.Name.ToString().Length - 8));

            if (IOManager.Instance.OutIO_status[(int)OutputCylinderExtendPairs[button.Name.ToString()]] == 1)
            {
                IOManager.Instance.IO_ControlStatus(OutputCylinderExtendPairs[button.Name.ToString()], 1);
                IOManager.Instance.IO_ControlStatus(OutputCylinderRetractPairs[button.Name.ToString()], 0);
                button.Content = "Retract";
            }
            else if (IOManager.Instance.OutIO_status[(int)OutputCylinderExtendPairs[button.Name.ToString()]] == 0)
            {
                IOManager.Instance.IO_ControlStatus(OutputCylinderExtendPairs[button.Name.ToString()], 0);
                IOManager.Instance.IO_ControlStatus(OutputCylinderRetractPairs[button.Name.ToString()], 1);
                button.Content = "Extend";
            }
        }

        private int[] n = new int[4];
        private void Suctionnozzle_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = int.Parse(button.Name.ToString().Substring(13, 1));
            if (IOManager.Instance.OutIO_status[(int)OutputNozzleSupplyPairs[button.Name.ToString()]] == 1)
            {
                IOManager.Instance.IO_ControlStatus(OutputNozzleSupplyPairs[button.Name.ToString()], 1);
                IOManager.Instance.IO_ControlStatus(OutputNozzleReleasePairs[button.Name.ToString()], 0);
                button.Content = "Release";
            }
            else if (IOManager.Instance.OutIO_status[(int)OutputNozzleSupplyPairs[button.Name.ToString()]] == 0)
            {
                IOManager.Instance.IO_ControlStatus(OutputNozzleSupplyPairs[button.Name.ToString()], 0);
                IOManager.Instance.IO_ControlStatus(OutputNozzleReleasePairs[button.Name.ToString()], 0);
                button.Content = "Supply";
            }
        }

        private void Suctionnozzle11_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = int.Parse(button.Name.ToString().Substring(13, 1));
            if (IOManager.Instance.OutIO_status[(int)OutputNozzleBlowPairs[button.Name.ToString()]] == 1)
            {
                IOManager.Instance.IO_ControlStatus(OutputNozzleBlowPairs[button.Name.ToString()], 1);
                IOManager.Instance.IO_ControlStatus(OutputNozzleNoBlowPairs[button.Name.ToString()], 0);
                button.Content = "Release";
                Thread.Sleep(50);
                IOManager.Instance.IO_ControlStatus(OutputNozzleBlowPairs[button.Name.ToString()], 0);
                IOManager.Instance.IO_ControlStatus(OutputNozzleNoBlowPairs[button.Name.ToString()], 0);
                button.Content = "Blow";
            }
        }
    }
}
