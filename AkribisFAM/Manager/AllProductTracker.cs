using AkribisFAM.CommunicationProtocol;
using AkribisFAM.DeviceClass;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System.Collections.Generic;
using System.Linq;
using static AkribisFAM.DeviceClass.AssemblyGantryControl;

namespace AkribisFAM.Manager
{
    public class AllProductTracker : ViewModelBase
    {


        public List<ProductTracker> Trackers { get; set; } = new List<ProductTracker>();
        public List<ProductTracker> TrayTrackers { get; set; } = new List<ProductTracker>();
        public List<ProductTracker> NGTrayTrackers { get; set; } = new List<ProductTracker>();
        public List<ProductTracker> FoamTrackers { get; set; } = new List<ProductTracker>();
        public List<ProductTracker> PickerTrackers { get; set; } = new List<ProductTracker>();
        private Conveyor.TrayData laserStationTray;

        public Conveyor.TrayData LaserStationTray
        {
            get { return laserStationTray; }
            set { laserStationTray = value; OnPropertyChanged(); }
        }
        private Conveyor.TrayData foamAssemblyStationTray;

        public Conveyor.TrayData FoamAssemblyStationTray
        {
            get { return foamAssemblyStationTray; }
            set { foamAssemblyStationTray = value; OnPropertyChanged(); }
        }
        private Conveyor.TrayData recheckStationTray;

        public Conveyor.TrayData RecheckStationTray
        {
            get { return recheckStationTray; }
            set { recheckStationTray = value; OnPropertyChanged(); }
        }
        private Conveyor.TrayData rejectOutGoingStationTray;

        public Conveyor.TrayData RejectOutGoingStationTray
        {
            get { return rejectOutGoingStationTray; }
            set { rejectOutGoingStationTray = value; OnPropertyChanged(); }
        }
        private Conveyor.TrayData goodOutGoingStationTray;

        public Conveyor.TrayData GoodOutGoingStationTray
        {
            get { return goodOutGoingStationTray; }
            set { goodOutGoingStationTray = value; OnPropertyChanged(); }
        }
        private ProductTracker feeder1Foams;

        public ProductTracker Feeder1Foams
        {
            get { return feeder1Foams; }
            set { feeder1Foams = value; OnPropertyChanged(); }
        }

        private ProductTracker feder2Foams;

        public ProductTracker Feeder2Foams
        {
            get { return feder2Foams; }
            set { feder2Foams = value; OnPropertyChanged(); }
        }
        private ProductTracker gantryPickerFoams;

        public ProductTracker GantryPickerFoams
        {
            get { return gantryPickerFoams; }
            set { gantryPickerFoams = value; OnPropertyChanged(); }
        }
        public AllProductTracker()
        {
            LaserStationTray = new Conveyor.TrayData("LaserStationTray", TrackerType.Tray, 3, 4);
            FoamAssemblyStationTray = new Conveyor.TrayData("FoamAssemblyStationTray", TrackerType.Tray, 3, 4);
            RecheckStationTray = new Conveyor.TrayData("RecheckStationTray", TrackerType.Tray, 3, 4);
            GoodOutGoingStationTray = new Conveyor.TrayData("GoodOutGoingStationTray", TrackerType.Tray, 3, 4);
            RejectOutGoingStationTray = new Conveyor.TrayData("RejectOutGoingStationTray", TrackerType.Tray, 3, 4);

            Conveyor.Current.ConveyorTrays[0] = LaserStationTray;
            Conveyor.Current.ConveyorTrays[1] = FoamAssemblyStationTray;
            Conveyor.Current.ConveyorTrays[2] = RecheckStationTray;
            Conveyor.Current.ConveyorTrays[3] = GoodOutGoingStationTray;

            NGTrayTrackers.Add(RejectOutGoingStationTray);

            Feeder1Foams = new ProductTracker("Feeder1Foams", TrackerType.Foam, 1, 4);
            Feeder2Foams = new ProductTracker("Feeder2Foams", TrackerType.Foam, 1, 4);

            GantryPickerFoams = new ProductTracker("GantryPickerFoams", TrackerType.Pickers, 1, 4);

            Trackers.Add(LaserStationTray);
            Trackers.Add(FoamAssemblyStationTray);
            Trackers.Add(RecheckStationTray);
            //Trackers.Add(RejectOutGoingStationTray);
            Trackers.Add(GoodOutGoingStationTray);
            Trackers.Add(Feeder1Foams);
            Trackers.Add(Feeder2Foams);
            Trackers.Add(GantryPickerFoams);

            TrayTrackers = Trackers.Where(x => x.TrackerType == TrackerType.Tray).ToList();
            FoamTrackers = Trackers.Where(x => x.TrackerType == TrackerType.Foam).ToList();
            PickerTrackers = Trackers.Where(x => x.TrackerType == TrackerType.Pickers).ToList();

        }

        public bool PickerPicked(CognexVisionControl.FeederNum feeder, int foamNumber, int pickerNumber)
        {
            ProductTracker trackerSource;

            switch (feeder)
            {
                case CognexVisionControl.FeederNum.Feeder1:
                    trackerSource = Feeder1Foams;
                    break;
                case CognexVisionControl.FeederNum.Feeder2:
                    trackerSource = Feeder2Foams;
                    break;
                default:
                    return false;
            }

            GantryPickerFoams.PartArray[pickerNumber].Consume(trackerSource.PartArray[foamNumber]);
            return true;
        }

        public bool PickerPickFail(CognexVisionControl.FeederNum feeder, int foamNumber)
        {
            ProductTracker trackerFeeder;

            switch (feeder)
            {
                case CognexVisionControl.FeederNum.Feeder1:
                    trackerFeeder = App.productTracker.Feeder1Foams;
                    break;
                case CognexVisionControl.FeederNum.Feeder2:
                    trackerFeeder = App.productTracker.Feeder2Foams;
                    break;
                default:
                    return false;
            }

            trackerFeeder.PartArray[foamNumber].SetFail(FailReason.FailToPick);
            return true;
        }
        public bool IsAllAvailablePartPlaceDone => FoamAssemblyStationTray.PartArray
            .Where(x => x.IsNormal())
            .All(x => x.IsFoamPlaced);
        public bool PickerPlaceFail(AssemblyGantryControl.Picker picker, int trayIndex)
        {
            var source = App.productTracker.GantryPickerFoams.PartArray[(int)picker - 1];
            var target = App.productTracker.FoamAssemblyStationTray.PartArray[trayIndex];

            target.SetFail(FailReason.FailToPlace);
            //target.SetFail(FailReason.FailToPlace);
            return true;
        }
        public bool PickerPlaced(AssemblyGantryControl.Picker picker, int trayIndex)
        {
            var source = App.productTracker.GantryPickerFoams.PartArray[(int)picker - 1];
            var target = App.productTracker.FoamAssemblyStationTray.PartArray[trayIndex];

            target.Consume(source);
            return true;
        }
        public bool SetRecheckVision(RecheckCamrea.Acceptcommand.AcceptTFCRecheckAppend result, int trayIndex)
        {
            char delimiter = '_';
            if (result == null)
            {
                return false;
            }

            var productData = App.productTracker.RecheckStationTray.PartArray[trayIndex];
            productData.present = true;
            productData.failed = (result.Errcode != "1");

            if (productData.failed)
            {
                productData.Station = StationType.Recheck;
                productData.FailReason = FailReason.FailToPlace;
            }
            string[] measurements = result.Datan.Split(delimiter);
            for (int i = 0; i < measurements.Length; i++)
            {
                RecheckVisionMeasurement measurement = new RecheckVisionMeasurement()
                {
                    MeasurementCount = i,
                    DateTimeMeasure = System.DateTime.Now,
                    Measurement = double.Parse(measurements[i]),
                };
                productData.VisionMeasurements.Add(measurement);
            }
            return true;
        }

        public bool PickerCanDoPick(int pickerNumber)
        {
            var pd = App.productTracker.GantryPickerFoams.PartArray[pickerNumber - 1];
            return !pd.present && !pd.failed;
        }
        public bool FeederCanBePick(CognexVisionControl.FeederNum feeder, int foamNumber)
        {
            ProductTracker trackerFeeder;

            switch (feeder)
            {
                case CognexVisionControl.FeederNum.Feeder1:
                    trackerFeeder = App.productTracker.Feeder1Foams;
                    break;
                case CognexVisionControl.FeederNum.Feeder2:
                    trackerFeeder = App.productTracker.Feeder2Foams;
                    break;
                default:
                    return false;
            }

            return trackerFeeder.PartArray[foamNumber].CanPick();
        }

        public bool PickerCanDoPlace(int pickerNumber)
        {
            var pd = App.productTracker.GantryPickerFoams.PartArray[pickerNumber - 1];
            return pd.present && !pd.failed;
        }

        public bool TrayCanBePlace(int trayIndex)
        {
            var pd = App.productTracker.FoamAssemblyStationTray.PartArray[trayIndex];
            return pd.present && !pd.failed;
        }
    }
}
