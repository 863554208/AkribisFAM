using AkribisFAM.CommunicationProtocol;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static AkribisFAM.GlobalManager;
namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for ConveyorControlView.xaml
    /// </summary>
    public partial class ConveyorControlView : UserControl
    {
        ObservableCollection<ConveyorWorkStationControl> stations = new ObservableCollection<ConveyorWorkStationControl>();

        private readonly System.Timers.Timer _timer;

        public ConveyorControlView()
        {
            InitializeComponent();

            _timer = new System.Timers.Timer(1000);


            ConveyorWorkStationControl laserStation = new ConveyorWorkStationControl()
            {
                StationNumber = 1,
                StationName = "Laser Check",
                ConveyorInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN1_0Slowdown_Sign1
                },
                LifterInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN2_0Left_1_lift_cylinder_Extend_InPos ,
                    IO_INFunction_Table.IN2_1Left_1_lift_cylinder_retract_InPos ,

                    IO_INFunction_Table.IN2_2Right_1_lift_cylinder_Extend_InPos,
                    IO_INFunction_Table.IN2_3Right_1_lift_cylinder_retract_InPos,


                    IO_INFunction_Table.IN1_12bord_lift_in_position1 ,

                },
                LifterOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend ,
                    IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract ,
                    IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend,
                    IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract,
                },
                GateInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN3_0Stopping_cylinder_1_extend_InPos ,
                    IO_INFunction_Table.IN3_1Stopping_cylinder_1_react_InPos ,

                    IO_INFunction_Table.IN1_4Stop_Sign1,
                    IO_INFunction_Table.IN1_10plate_has_left_Behind_the_stopping_cylinder1,
                },
                GateOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend ,
                    IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract,

                },

            };
            stations.Add(laserStation);
            ConveyorWorkStationControl assemblyStation = new ConveyorWorkStationControl()
            {
                StationNumber = 2,
                StationName = "Foam assembly",
                ConveyorInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN1_1Slowdown_Sign2
                },
                LifterInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN2_4Left_2_lift_cylinder_Extend_InPos ,
                    IO_INFunction_Table.IN2_5Left_2_lift_cylinder_retract_InPos ,

                    IO_INFunction_Table.IN2_6Right_2_lift_cylinder_Extend_InPos,
                    IO_INFunction_Table.IN2_7Right_2_lift_cylinder_retract_InPos,


                    IO_INFunction_Table.IN1_13bord_lift_in_position2 ,

                },
                LifterOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend ,
                    IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract ,
                    IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend ,
                    IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract ,

                },
                GateInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN3_2Stopping_cylinder_2_extend_InPos ,
                    IO_INFunction_Table.IN3_3Stopping_cylinder_2_react_InPos ,

                    IO_INFunction_Table.IN1_5Stop_Sign2,
                    IO_INFunction_Table.IN1_11plate_has_left_Behind_the_stopping_cylinder2,
                },
                GateOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT2_2Stopping_Cylinder2_extend ,
                    IO_OutFunction_Table.OUT2_3Stopping_Cylinder2_retract,

                },

            };

            stations.Add(assemblyStation);

            ConveyorWorkStationControl filmRemoveStation = new ConveyorWorkStationControl()
            {
                StationNumber = 3,
                StationName = "Film Remove",
                ConveyorInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN1_2Slowdown_Sign3
                },
                LifterInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN2_8Left_3_lift_cylinder_Extend_InPos ,
                    IO_INFunction_Table.IN2_9Left_3_lift_cylinder_retract_InPos ,

                    IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos,
                    IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos,


                    IO_INFunction_Table.IN1_14bord_lift_in_position3 ,

                },
                LifterOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT1_8Left_3_lift_cylinder_extend ,
                    IO_OutFunction_Table.OUT1_9Left_3_lift_cylinder_retract ,
                    IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend ,
                    IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract ,

                },
                GateInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN3_4Stopping_cylinder_3_extend_InPos ,
                    IO_INFunction_Table.IN3_5Stopping_cylinder_3_react_InPos ,

                    IO_INFunction_Table.IN1_6Stop_Sign3,
                    IO_INFunction_Table.IN6_6plate_has_left_Behind_the_stopping_cylinder3,
                },
                GateOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT2_4Stopping_Cylinder3_extend ,
                    IO_OutFunction_Table.OUT2_5Stopping_Cylinder3_retract,
                },

            };

            stations.Add(filmRemoveStation);

            ConveyorWorkStationControl NGStation = new ConveyorWorkStationControl()
            {
                StationNumber = 4,
                StationName = "NG Reject",
                ConveyorInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN1_2Slowdown_Sign3,
                    IO_INFunction_Table.IN6_0NG_plate_1_in_position
                },
                LifterInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN2_124_lift_cylinder_Extend_InPos ,
                    IO_INFunction_Table.IN2_134_lift_cylinder_retract_InPos ,

                    IO_INFunction_Table.IN1_15bord_lift_in_position4 ,

                    IO_INFunction_Table.IN1_8NG_cover_plate1 ,
                    IO_INFunction_Table.IN1_9NG_cover_plate2 ,

                    //IO_INFunction_Table.IN2_10Right_3_lift_cylinder_Extend_InPos,
                    //IO_INFunction_Table.IN2_11Right_3_lift_cylinder_retract_InPos,

                },
                LifterOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT1_124_lift_cylinder_extend ,
                    IO_OutFunction_Table.OUT1_134_lift_cylinder_retract ,
                    //IO_OutFunction_Table.OUT1_10Right_3_lift_cylinder_extend ,
                    //IO_OutFunction_Table.OUT1_11Right_3_lift_cylinder_retract ,

                },
                GateInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN3_6Stopping_cylinder_4_extend_InPos ,
                    IO_INFunction_Table.IN3_7Stopping_cylinder_4_react_InPos ,

                    IO_INFunction_Table.IN1_7Stop_Sign4,
                    IO_INFunction_Table.IN6_7plate_has_left_Behind_the_stopping_cylinder4,
                    IO_INFunction_Table.IN6_7plate_has_left_Behind_the_stopping_cylinder4,
                },
                GateOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT2_6Stopping_Cylinder4_extend ,
                    IO_OutFunction_Table.OUT2_7Stopping_Cylinder4_retract,
                },

            };
            stations.Add(NGStation);
            itemControlStation.ItemsSource = stations;

            _timer = new System.Timers.Timer(200);
            _timer.Elapsed += (s, e) => TickTime();
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void TickTime()
        {

            stations = new ObservableCollection<ConveyorWorkStationControl>(stations);
        }
        private void btnMove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Conveyor.Current.MoveConveyorAll((int)AxisSpeed.BL1);
        }

        private void btnSlowMove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Conveyor.Current.MoveConveyorAll((int)20);
        }

        private void btnStop_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Conveyor.Current.StopConveyor();
        }

        private void GateControlView_GateZDownPressed(object sender, System.EventArgs e)
        {
            var control = (GateControlView)sender;
            var station = (ConveyorWorkStationControl)control.DataContext;
            Task.Run(() =>
            {

                if (!Conveyor.Current.GateDown(station.StationNumber))
                {
                    //MessageBox.Show("Failed");
                }
            }
          );
        }

        private void GateControlView_GateZUpPressed(object sender, System.EventArgs e)
        {
            var control = (GateControlView)sender;
            var station = (ConveyorWorkStationControl)control.DataContext;
            Task.Run(() =>
            {

                if (!Conveyor.Current.GateUp(station.StationNumber))
                {
                    //MessageBox.Show("Failed");
                }
            }
            );


        }

        private void GateControlView_LifterZDownPressed(object sender, System.EventArgs e)
        {

            var control = (GateControlView)sender;
            var station = (ConveyorWorkStationControl)control.DataContext;
            //Conveyor.Current.LiftDownRelatedTray(station.StationNumber);
            Task.Run(() =>
            {

                if (!Conveyor.Current.LiftDownRelatedTray(station.StationNumber))
                {
                    //MessageBox.Show("Failed");
                }
            });
        }

        private void GateControlView_LifterZUpPressed(object sender, System.EventArgs e)
        {
            var control = (GateControlView)sender;
            var station = (ConveyorWorkStationControl)control.DataContext;
            //Conveyor.Current.LiftUpRelatedTray(station.StationNumber);
            Task.Run(() =>
            {

                if (!Conveyor.Current.LiftUpRelatedTray(station.StationNumber))
                {
                    //MessageBox.Show("Failed");
                }
            });
        }
    }


    public class ConveyorWorkStationControl : ViewModelBase
    {
        private int stationNumber;

        public int StationNumber
        {
            get { return stationNumber; }
            set { stationNumber = value; OnPropertyChanged(); }
        }

        private string stationName;

        public string StationName
        {
            get { return stationName; }
            set { stationName = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IO_INFunction_Table> conveyorInList = new ObservableCollection<IO_INFunction_Table>();

        public ObservableCollection<IO_INFunction_Table> ConveyorInList
        {
            get { return conveyorInList; }
            set { conveyorInList = value; OnPropertyChanged(); }
        }


        private ObservableCollection<IO_OutFunction_Table> conveyorOutList = new ObservableCollection<IO_OutFunction_Table>();

        public ObservableCollection<IO_OutFunction_Table> ConveyorOutList
        {
            get { return conveyorOutList; }
            set { conveyorOutList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IO_INFunction_Table> lifterInList = new ObservableCollection<IO_INFunction_Table>();

        public ObservableCollection<IO_INFunction_Table> LifterInList
        {
            get { return lifterInList; }
            set { lifterInList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IO_OutFunction_Table> lifterOutList = new ObservableCollection<IO_OutFunction_Table>();

        public ObservableCollection<IO_OutFunction_Table> LifterOutList
        {
            get { return lifterOutList; }
            set { lifterOutList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IO_INFunction_Table> gateInList = new ObservableCollection<IO_INFunction_Table>();

        public ObservableCollection<IO_INFunction_Table> GateInList
        {
            get { return gateInList; }
            set { gateInList = value; OnPropertyChanged(); }
        }
        private ObservableCollection<IO_OutFunction_Table> gateOutList = new ObservableCollection<IO_OutFunction_Table>();

        public ObservableCollection<IO_OutFunction_Table> GateOutList
        {
            get { return gateOutList; }
            set { gateOutList = value; OnPropertyChanged(); }
        }

        public ConveyorWorkStationControl() { }
        public ConveyorWorkStationControl(string name,
            ObservableCollection<IO_INFunction_Table> conveyIn,
            ObservableCollection<IO_OutFunction_Table> conveyOut,
            ObservableCollection<IO_INFunction_Table> lifterIn,
            ObservableCollection<IO_OutFunction_Table> lifterOut,
            ObservableCollection<IO_INFunction_Table> gateIn,
            ObservableCollection<IO_OutFunction_Table> gateOut)
        {
            ConveyorInList = conveyIn;
            ConveyorOutList = conveyOut;
            LifterInList = lifterIn;
            LifterOutList = lifterOut;
            GateInList = gateIn;
            GateOutList = gateOut;

            _timer = new System.Timers.Timer(200);
            _timer.Elapsed += (s, e) => TickTime();
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void TickTime()
        {
            ConveyorInList = new ObservableCollection<IO_INFunction_Table>(ConveyorInList);
            ConveyorOutList = new ObservableCollection<IO_OutFunction_Table>(ConveyorOutList) ;
            LifterInList = new ObservableCollection<IO_INFunction_Table>(LifterInList); ;
            LifterOutList = new ObservableCollection<IO_OutFunction_Table>(LifterOutList); ;
            GateInList = new ObservableCollection<IO_INFunction_Table>(GateInList); ;
            GateOutList = new ObservableCollection<IO_OutFunction_Table>(GateOutList); ;

        }
    }

}
