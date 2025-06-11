using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Conveyor.ConveyorTrays[0] = LaserStationTray;
            Conveyor.ConveyorTrays[1] = FoamAssemblyStationTray;
            Conveyor.ConveyorTrays[2] = RecheckStationTray;
            Conveyor.ConveyorTrays[3] = GoodOutGoingStationTray;

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
    }
}
