using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Models
{
    public class OeeRecord
    {
        public string LotID { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int GoodProducts { get; set; }
        public int GoodVision1 { get; set; }
        public int GoodVision2 { get; set; }
        public int GoodVision3 { get; set; }
        public int RejectVision1 { get; set; }
        public int RejectVision2 { get; set; }
        public int RejectVision3 { get; set; }
        public int AlarmsCount { get; set; }
        public double PlannedUPH { get; set; }
        public double PlannedProductionTime { get; set; }
        public double UpTimeHr { get; set; }
        public double DownTimeHr { get; set; }
        public double IdleTimeHr { get; set; }
        public double TotalTimeHr { get; set; }
        public double ProductiveTimeHr { get; set; }
        public double MTBF { get; set; }
        public double MTTR { get; set; }
        public double Availability { get; set; }
        public double Performance { get; set; }
        public double Quality { get; set; }
        public double Oee { get; set; }
    }
}
