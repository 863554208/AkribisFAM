using AkribisFAM.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for ManualFeederControlView.xaml
    /// </summary>
    public partial class ManualFeederControlView : UserControl
    {
        public event EventHandler VisionStandbyPosPressed;
        public event EventHandler VisionEndingPosPressed;
        public event EventHandler VisionOTFPressed;
        
        public event EventHandler PickerZDownPressed;
        public event EventHandler PickerZUpPressed;

        public event EventHandler PickerVacOnPressed;
        public event EventHandler PickerPurgePressed;
        public event EventHandler PickerOffAirPressed;

        public event EventHandler PickerMoveFoam1Pressed;
        public event EventHandler PickerMoveFoam2Pressed;
        public event EventHandler PickerMoveFoam3Pressed;
        public event EventHandler PickerMoveFoam4Pressed;

        public event EventHandler PickerPickFoam1Pressed;
        public event EventHandler PickerPickFoam2Pressed;
        public event EventHandler PickerPickFoam3Pressed;
        public event EventHandler PickerPickFoam4Pressed;

        public event EventHandler PickerPickAllPressed;

        public event EventHandler FeederIndexPressed;
        public event EventHandler FeederExtendPressed;
        public event EventHandler FeederRetractPressed;

        public event EventHandler FeederVacOnPressed;
        public event EventHandler FeederPurgePressed;
        public event EventHandler FeederOffAirPressed;

        bool stopAllMotion = false;
       public int SelectedPicker
        {
            get
            {
                if (btnPicker1.IsSelected) return 1;
                if (btnPicker2.IsSelected) return 2;
                if (btnPicker3.IsSelected) return 3;
                if (btnPicker4.IsSelected) return 4;
                else
                    return -999;
            }
        }
        List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> snapFeederPath = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();
        List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> ccd2SnapPath = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> palletePath = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition>();
        List<AssUpCamrea.Pushcommand.SendGTCommandAppend> fetchMatrial = new List<AssUpCamrea.Pushcommand.SendGTCommandAppend>();

        public ManualFeederControlView()
        {
            InitializeComponent();
        }

        private void btnMoveStandby_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VisionStandbyPosPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnMoveEnding_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VisionEndingPosPressed?.Invoke(this, EventArgs.Empty);
        }

 

        private void btnVis1OTF_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.assemblyGantryControl.BypassPicker3 = true;
            App.assemblyGantryControl.BypassPicker4 = true;
            VisionOTFPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnPicker_Selected(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnZUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerZUpPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnZDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerZDownPressed?.Invoke(this, EventArgs.Empty);
        }


        private void btnPickerVacOn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerVacOnPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnPickerPurge_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerPurgePressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnPickerOff_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerOffAirPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnMoveToPos1_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerMoveFoam1Pressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnMoveToPos2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PickerMoveFoam2Pressed?.Invoke(this, EventArgs.Empty);

        }

        private void btnMoveToPos3_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerMoveFoam3Pressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnMoveToPos4_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerMoveFoam4Pressed?.Invoke(this, EventArgs.Empty);
        }


        private void btnPickFoam1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PickerPickFoam1Pressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnPickFoam2_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerPickFoam2Pressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnPickFoam3_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerPickFoam3Pressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnPickFoam4_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PickerPickFoam4Pressed?.Invoke(this, EventArgs.Empty);

        }

        private void btnPickFour_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            PickerPickAllPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnIndex_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FeederIndexPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnExtend_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            FeederExtendPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnRetract_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            FeederRetractPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnFeederVacOn_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            FeederVacOnPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnFeederPurge_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FeederPurgePressed?.Invoke(this, EventArgs.Empty);

        }

        private void btnFeederOff_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            FeederOffAirPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
