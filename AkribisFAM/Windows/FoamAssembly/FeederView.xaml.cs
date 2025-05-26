using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.WorkStation;
using static AkribisFAM.Windows.FoamAssemblyView;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for FeederView.xaml
    /// </summary>
    public partial class FeederView : UserControl
    {
        ObservableCollection<FeederControlVM> feeders = new ObservableCollection<FeederControlVM>();
        FeederVM vm;
        bool stopAllMotion = false;

        public FeederView()
        {
            InitializeComponent();

            FeederControlVM feeder1 = new FeederControlVM()
            {
                FeederNumber = 1,
                FeederName = "Feeder 1",
                PickerInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback,
                    IO_INFunction_Table.IN3_13PNP_Gantry_vacuum2_Pressure_feedback,
                    IO_INFunction_Table.IN3_14PNP_Gantry_vacuum3_Pressure_feedback,
                    IO_INFunction_Table.IN3_15PNP_Gantry_vacuum4_Pressure_feedback,
                },
                FeederInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN4_0Initialized_feeder1 ,
                    IO_INFunction_Table.IN4_1Alarm_feeder1 ,

                    IO_INFunction_Table.IN4_2Platform_has_label_feeder1,
                    IO_INFunction_Table.IN4_3Backup_Platform_2_has_label_feeder1,


                    IO_INFunction_Table.IN4_8Feeder1_limit_cylinder_extend_InPos ,
                    IO_INFunction_Table.IN4_9Feeder1_limit_cylinder_retract_InPos ,
                    IO_INFunction_Table.IN4_12Feeder1_drawer_InPos ,

                    IO_INFunction_Table.IN5_0Feeder_vacuum1_Pressure_feedback ,
                    IO_INFunction_Table.IN5_1Feeder_vacuum2_Pressure_feedback ,

                    IO_INFunction_Table.IN5_10Feeder1 ,

                },
                PickerOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    //IO_OutFunction_Table.OUT1_0Left_1_lift_cylinder_extend ,
                    //IO_OutFunction_Table.OUT1_1Left_1_lift_cylinder_retract ,
                    //IO_OutFunction_Table.OUT1_2Right_1_lift_cylinder_extend,
                    //IO_OutFunction_Table.OUT1_3Right_1_lift_cylinder_retract,
                },
                FeederOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    //IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend ,
                    //IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract,

                },

            };

            feeders.Add(feeder1);



            FeederControlVM feeder2 = new FeederControlVM()
            {
                FeederNumber = 2,
                FeederName = "Feeder 2",
                PickerInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN3_12PNP_Gantry_vacuum1_Pressure_feedback,
                    IO_INFunction_Table.IN3_13PNP_Gantry_vacuum2_Pressure_feedback,
                    IO_INFunction_Table.IN3_14PNP_Gantry_vacuum3_Pressure_feedback,
                    IO_INFunction_Table.IN3_15PNP_Gantry_vacuum4_Pressure_feedback,
                },
                FeederInList = new ObservableCollection<IO_INFunction_Table>()
                {
                    IO_INFunction_Table.IN4_4Initialized_feeder2 ,
                    IO_INFunction_Table.IN4_5Alarm_feeder2 ,

                    IO_INFunction_Table.IN4_6Platform_has_label_feeder2,
                    IO_INFunction_Table.IN4_7Backup_Platform_2_has_label_feeder2,


                    IO_INFunction_Table.IN4_10Feeder2_limit_cylinder_extend_InPos ,
                    IO_INFunction_Table.IN4_11Feeder2_limit_cylinder_retract_InPos ,
                    IO_INFunction_Table.IN4_13Feeder2_drawer_InPos ,

                    IO_INFunction_Table.IN5_2Feeder_vacuum3_Pressure_feedback ,
                    IO_INFunction_Table.IN5_3Feeder_vacuum4_Pressure_feedback ,

                    IO_INFunction_Table.IN5_11Feeder2 ,

                },
                PickerOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT1_4Left_2_lift_cylinder_extend ,
                    IO_OutFunction_Table.OUT1_5Left_2_lift_cylinder_retract ,
                    IO_OutFunction_Table.OUT1_6Right_2_lift_cylinder_extend,
                    IO_OutFunction_Table.OUT1_7Right_2_lift_cylinder_retract,
                },
                FeederOutList = new ObservableCollection<IO_OutFunction_Table>()
                {
                    IO_OutFunction_Table.OUT2_0Stopping_Cylinder1_extend ,
                    IO_OutFunction_Table.OUT2_1Stopping_Cylinder1_retract,

                },

            };

            feeders.Add(feeder2);

            itemControlStation.ItemsSource = feeders;

            cbxTrayType.ItemsSource = Enum.GetNames(typeof(TrayType));
            cbxTrayType.SelectedIndex = 0;
        }
        class FeederVM
        {

            private ObservableCollection<SinglePointExt> points = new ObservableCollection<SinglePointExt>();

            public ObservableCollection<SinglePointExt> Points
            {
                get { return points; }
                set { points = value; }
            }
            private int col;

            public int Column
            {
                get { return col; }
                set { col = value; }
            }
            private int row;

            public int Row
            {
                get { return row; }
                set { row = value; }
            }
        }
        private void cbxTrayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = null;
            List<SinglePointExt> lsp = new List<SinglePointExt>();
            if (cbxTrayType.SelectedIndex < 0) return;
            if (cbxTrayType.SelectedIndex > 0) return;

            var stationsPoints = App.recipeManager.Get_RecipeStationPoints((TrayType)cbxTrayType.SelectedIndex);
            if (stationsPoints == null) return;

            var laser = stationsPoints.LaiLiaoPointList.FirstOrDefault(x => x.name != null && x.name.Equals("Feedar1 Points"));
            if (laser == null) return;

            lsp = laser.childList.Select((x,index) => new SinglePointExt
            {
                X = x.childPos[0],
                Y = x.childPos[1],
                Z = x.childPos[2],
                R = x.childPos[3],
                TeachPointIndex = index + 1
            }).ToList();


            var points = new ObservableCollection<SinglePoint>(lsp);
            ObservableCollection<SinglePointExt> pts = new ObservableCollection<SinglePointExt>();


            vm = new FeederVM()
            {
                Points = pts,
                Row = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartRow,
                Column = App.recipeManager.GetRecipe((TrayType)cbxTrayType.SelectedIndex).PartColumn,
            };
            DataContext = vm;
        }

        private void ManualFeederControlView_VisionStandbyPosPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            App.vision1.MoveFoamStandbyPos((DeviceClass.CognexVisionControl.FeederNum)station.FeederNumber);
        }

        private void ManualFeederControlView_VisionEndingPosPressed(object sender, EventArgs e)
        {

            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            App.vision1.MoveFoamEndingPos((DeviceClass.CognexVisionControl.FeederNum)station.FeederNumber);
        }

        private void ManualFeederControlView_VisionOTFPressed(object sender, EventArgs e)
        {

            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            App.vision1.FoamOnTheFlyTrigger((DeviceClass.CognexVisionControl.FeederNum)station.FeederNumber);
        }

        private void ManualFeederControlView_PickerZUpPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            App.assemblyGantryControl.ZUp((DeviceClass.AssemblyGantryControl.Picker)control.SelectedPicker);
        }

        private void ManualFeederControlView_PickerZDownPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            App.assemblyGantryControl.ZDown((DeviceClass.AssemblyGantryControl.Picker)control.SelectedPicker);
        }

        private void ManualFeederControlView_PickerVacOnPressed(object sender, EventArgs e)
        {

            var control = (ManualFeederControlView)sender;
            App.assemblyGantryControl.VacOn((DeviceClass.AssemblyGantryControl.Picker)control.SelectedPicker);
        }

        private void ManualFeederControlView_PickerPurgePressed(object sender, EventArgs e)
        {

            var control = (ManualFeederControlView)sender;
            App.assemblyGantryControl.Purge((DeviceClass.AssemblyGantryControl.Picker)control.SelectedPicker);
        }

        private void ManualFeederControlView_PickerOffAirPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            App.assemblyGantryControl.Off((DeviceClass.AssemblyGantryControl.Picker)control.SelectedPicker);
        }

        private void ManualFeederControlView_PickerMoveFoam1Pressed(object sender, EventArgs e)
        {
            //var control = (ManualFeederControlView)sender;
            //App.assemblyGantryControl.Off((DeviceClass.AssemblyGantryControl.Picker)control.SelectedPicker);
        }

        private void ManualFeederControlView_PickerMoveFoam2Pressed(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerMoveFoam3Pressed(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerMoveFoam4Pressed(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerPickFoam1Pressed(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerPickFoam2Pressed(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerMoveFoam3Pressed_1(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerMoveFoam4Pressed_1(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerPickFoam3Pressed(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_PickerPickFoam4Pressed(object sender, EventArgs e)
        {

        }

        private void ManualFeederControlView_FeederIndexPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            if (station.FeederNumber == 1)
            {
                App.feeder1.Index();
            }
            else
            {
                App.feeder2.Index();
            }
        }

        private void ManualFeederControlView_FeederExtendPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            if (station.FeederNumber == 1)
            {
                App.feeder1.Lock();
            }
            else
            {
                App.feeder2.Lock();
            }
        }

        private void ManualFeederControlView_FeederRetractPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            if (station.FeederNumber == 1)
            {
                App.feeder1.Unlock();
            }
            else
            {
                App.feeder2.Unlock();
            }
        }

        private void ManualFeederControlView_FeederOffAirPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            if (station.FeederNumber == 1)
            {
                App.feeder1.VacOff();
            }
            else
            {
                App.feeder2.VacOff();
            }
        }

        private void ManualFeederControlView_FeederPurgePressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            //var station = (FeederControlVM)control.DataContext;
            //if (station.FeederNumber == 1)
            //{
            //    App.feeder1.();
            //}
            //else
            //{
            //    App.feeder2.Index();
            //}
        }

        private void ManualFeederControlView_FeederVacOnPressed(object sender, EventArgs e)
        {
            var control = (ManualFeederControlView)sender;
            var station = (FeederControlVM)control.DataContext;
            if (station.FeederNumber == 1)
            {
                App.feeder1.VacOn();
            }
            else
            {
                App.feeder2.VacOn();
            }
        }
    }


    public class FeederControlVM
    {
        private int feederNumber;

        public int FeederNumber
        {
            get { return feederNumber; }
            set { feederNumber = value; }
        }

        private string feederName;

        public string FeederName
        {
            get { return feederName; }
            set { feederName = value; }
        }

        private ObservableCollection<IO_INFunction_Table> pickerInList = new ObservableCollection<IO_INFunction_Table>();

        public ObservableCollection<IO_INFunction_Table> PickerInList
        {
            get { return pickerInList; }
            set { pickerInList = value; }
        }


        private ObservableCollection<IO_OutFunction_Table> pickerOutList = new ObservableCollection<IO_OutFunction_Table>();

        public ObservableCollection<IO_OutFunction_Table> PickerOutList
        {
            get { return pickerOutList; }
            set { pickerOutList = value; }
        }

        private ObservableCollection<IO_INFunction_Table> feederInList = new ObservableCollection<IO_INFunction_Table>();

        public ObservableCollection<IO_INFunction_Table> FeederInList
        {
            get { return feederInList; }
            set { feederInList = value; }
        }

        private ObservableCollection<IO_OutFunction_Table> feederOutList = new ObservableCollection<IO_OutFunction_Table>();

        public ObservableCollection<IO_OutFunction_Table> FeederOutList
        {
            get { return feederOutList; }
            set { feederOutList = value; }
        }

        private ObservableCollection<IO_INFunction_Table> gateInList = new ObservableCollection<IO_INFunction_Table>();

        //public ObservableCollection<IO_INFunction_Table> GateInList
        //{
        //    get { return gateInList; }
        //    set { gateInList = value; }
        //}
        //private ObservableCollection<IO_OutFunction_Table> gateOutList = new ObservableCollection<IO_OutFunction_Table>();

        //public ObservableCollection<IO_OutFunction_Table> GateOutList
        //{
        //    get { return gateOutList; }
        //    set { gateOutList = value; }
        //}

        public FeederControlVM() { }
        public FeederControlVM(string name,
            ObservableCollection<IO_INFunction_Table> conveyIn,
            ObservableCollection<IO_OutFunction_Table> conveyOut,
            ObservableCollection<IO_INFunction_Table> lifterIn,
            ObservableCollection<IO_OutFunction_Table> lifterOut)
        {
            PickerInList = conveyIn;
            PickerOutList = conveyOut;
            FeederInList = lifterIn;
            FeederOutList = lifterOut;
            //GateInList = gateIn;
            //GateOutList = gateOut;
        }
    }
}
