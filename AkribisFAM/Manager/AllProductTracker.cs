using AkribisFAM.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Manager
{
    public class AllProductTracker
    {
        public List<ProductTracker> Trackers { get; set; } = new List<ProductTracker>();
        public List<ProductTracker> TrayTrackers { get; set; } = new List<ProductTracker>();
        public List<ProductTracker> FoamTrackers { get; set; } = new List<ProductTracker>();
        public List<ProductTracker> PickerTrackers { get; set; } = new List<ProductTracker>();
        public ProductTracker LaserStationTray { get; set; }
        public ProductTracker FoamAssemblyStationTray { get; set; }
        public ProductTracker RecheckStationTray { get; set; }
        public ProductTracker NGStationTray { get; set; }
        public ProductTracker Feeder1Foams { get; set; }
        public ProductTracker Feeder2Foams { get; set; }
        public ProductTracker GantryPickerFoams { get; set; }
        public AllProductTracker() 
        {
            LaserStationTray = new ProductTracker("LaserStationTray", TrackerType.Tray, 3, 4);
            FoamAssemblyStationTray = new ProductTracker("FoamAssemblyStationTray", TrackerType.Tray, 3, 4);
            RecheckStationTray = new ProductTracker("RecheckStationTray", TrackerType.Tray, 3, 4);
            NGStationTray = new ProductTracker("NGStationTray", TrackerType.Tray, 3, 4);

            Feeder1Foams = new ProductTracker("Feeder1Foams", TrackerType.Foam, 1, 4);
            Feeder2Foams = new ProductTracker("Feeder2Foams", TrackerType.Foam, 1, 4);

            GantryPickerFoams = new ProductTracker("GantryPickerFoams", TrackerType.Pickers, 1, 4);

            Trackers.Add(LaserStationTray);
            Trackers.Add(FoamAssemblyStationTray);
            Trackers.Add(RecheckStationTray);
            Trackers.Add(NGStationTray);
            Trackers.Add(Feeder1Foams);
            Trackers.Add(Feeder2Foams);
            Trackers.Add(GantryPickerFoams);

            TrayTrackers = Trackers.Where(x=>x.TrackerType == TrackerType.Tray).ToList();
            FoamTrackers = Trackers.Where(x=>x.TrackerType == TrackerType.Foam).ToList();
            PickerTrackers = Trackers.Where(x=>x.TrackerType == TrackerType.Pickers).ToList();

        }
    }
}
