using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using static AkribisFAM.WorkStation.Conveyor;
using System.Xml.Linq;

namespace AkribisFAM.Util
{
    public struct PartData
    {
        public bool present;
        public bool failed;
        public bool inserted;
        public string partid; //serial number
        public string partno; //part number
        public string vendor;
        public string lotno;
        public string datecode;
        public string machine;
        public string grippers;
        public string nests;
        public FailReason failreason;
    }
    public enum TrackerType
    {
        None,
        Foam,
        Pickers,
        Tray

    }

    public class ProductTracker : ViewModelBase
    {
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }
        private TrackerType _trackerType = TrackerType.None;

        public TrackerType TrackerType
        {
            get { return _trackerType; }
            set { _trackerType = value; OnPropertyChanged(); }
        }
        private ProductData[] _partArray = null;

        public ProductData[] PartArray
        {
            get { return _partArray; }
            set { _partArray = value; OnPropertyChanged(); }
        }
        private int _row = -1;
        public int Row
        {
            get { return _row; }
            set { _row = value; OnPropertyChanged(); }
        }

        private int _column = -1;
        public int Column
        {
            get { return _column; }
            set { _column = value; OnPropertyChanged(); }
        }
        private int _totalSize = -1;
        public int TotalSize
        {
            get { return _totalSize; }
            set { _totalSize = value; OnPropertyChanged(); }
        }

        private bool _isFoamPlaced = false;
        public bool IsFoamPlaced
        {
            get { return _isFoamPlaced; }
            set { _isFoamPlaced = value; OnPropertyChanged(); }
        }
        public ProductTracker() { }

        public ProductTracker(string trackerName, TrackerType type, int row, int col)
        {
            TrackerType = type;
            Name = trackerName;
            TotalSize = row * col;
            Row = row;
            Column = col;
           var tempPartArray = new ProductData[TotalSize];
            for (var i = 0; i < TotalSize; i++)
            {
                tempPartArray[i] = new ProductData(i + 1);
            }
            PartArray = tempPartArray;
        }

        public virtual bool Reset()
        {
            //PartArray = new ProductData[TotalSize];
            for (var i = 0; i < TotalSize; i++)
            {
                PartArray[i].Reset();
            }
            return true;
        }

        #region Get
        public bool IsTrayIdSet()
        {
            return PartArray.All(x => x.TrayId != string.Empty);
        }
        public bool IsMeasurement(int TrayIndex)
        {
            var found = PartArray.FirstOrDefault(X => X.Index == TrayIndex);

            if (found != null)
            {

                return found.HeightMeasurements.All(x => x.HeightMeasurement == -1);
            }

            return false;
        }
        #endregion

        //public void SetFailReason(StationType goodStation, FailReason fr, bool forceOverwrite = false)
        //{
        //    var pd = GetProductData(goodStation);
        //    if (pd == null) return;

        //    if (pd.failreason == FailReason.None || forceOverwrite)
        //        pd.failreason = fr;
        //}
        #region Set
        public bool SetHeightMeasurement(int TrayIndex, double height, double x, double y)
        {
            if (TrackerType != TrackerType.Tray || PartArray.Length < TrayIndex)
            {
                PartArray[TrayIndex].Index = TrayIndex;
                PartArray[TrayIndex].HeightMeasurements.Add(new LaserMeasurement()
                {
                    XMeasurePosition = x,
                    YMeasurePosition = y,
                    HeightMeasurement = height,

                });
            }
            return true;
        }
        public bool SetTrayId(string trayId)
        {
            if (TrackerType != TrackerType.Tray)
            {
                foreach (var part in PartArray)
                {
                    part.TrayId = trayId;
                }
            }
            return true;
        }
        #endregion


    }
    public class ProductData : ViewModelBase, ICloneable
    {
        #region Public Members

        #endregion Public Members

        #region Private Variables

        #endregion Private Variables

        #region Public Properties

        /// <summary>
        /// Index in tray
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Unique serial number for this product 
        /// </summary>
        private string _serialNumber  = string.Empty;
        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// True if have part/product
        /// </summary>
        private bool _present;
        public bool present
        {
            get { return _present; }
            set { _present = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// True if failed part/product
        /// </summary>
        private bool _failed;
        public bool failed
        {
            get { return _failed; }
            set { _failed = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// True if foam process has been executed for this product
        /// </summary>
        private bool _isFoamPlaced;
        public bool IsFoamPlaced
        {
            get { return _isFoamPlaced; }
            set { _isFoamPlaced = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Reason for failed part/product
        /// </summary>

        private FailReason _failReason;
        public FailReason FailReason
        {
            get { return _failReason; }
            set { _failReason = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Name of turret where the failed part/product state was set
        /// </summary>

        private StationType _failStation = StationType.Default;
        public StationType FailStation
        {
            get { return _failStation; }
            set { _failStation = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Name of station where product data instantiate
        /// </summary>
        private StationType _station = StationType.Default;
        public StationType Station
        {
            get { return _station; }
            set { _station = value; OnPropertyChanged(); }
        }


        private PartData _foam;
        public PartData Foam
        {
            get { return _foam; }
            set { _foam = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Unique generated ID for this product (not used for partOnly)
        /// </summary>
        private string _uuid = string.Empty;
        public string UUID
        {
            get { return _uuid; }
            set { _uuid = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Start time for product assembly process (not used for partOnly)
        /// </summary>
        private DateTime _timeIn = DateTime.Now;
        public DateTime TimeIn
        {
            get { return _timeIn; }
            set { _timeIn = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// End time for product assembly process (not used for partOnly)
        /// </summary>
        private DateTime _timeOut = DateTime.Now;
        public DateTime TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; OnPropertyChanged(); }
        }

        private string _trayId = string.Empty;
        public string TrayId
        {
            get { return _trayId; }
            set { _trayId = value; OnPropertyChanged(); }
        }
        public ObservableCollection<LaserMeasurement> HeightMeasurements { get; set; } = new ObservableCollection<LaserMeasurement>();
        public ObservableCollection<RecheckVisionMeasurement> VisionMeasurements { get; set; } = new ObservableCollection<RecheckVisionMeasurement>();

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="tn">The name of the turret using this</param>
        public ProductData(int index)
        {
            Reset();
            Index = index;
            UUID = Guid.NewGuid().ToString().ToUpper();
            Station = StationType.Laser;
        }
        public ProductData(ProductData pd)
        {
            Index = pd.Index;
            present = pd.present;
            failed = pd.failed;
            SerialNumber = pd.SerialNumber;
            FailReason = pd.FailReason;
            FailStation = pd.FailStation;
            Station = pd.Station;
            Foam = pd.Foam;
            UUID = pd.UUID;
            TimeIn = pd.TimeIn;
            TimeOut = pd.TimeOut;
            TrayId  = pd.TrayId;
            HeightMeasurements = pd.HeightMeasurements;
            VisionMeasurements = pd.VisionMeasurements;
        }
        public void Copy(ProductData pd)
        {
            Index = pd.Index;
            present = pd.present;
            failed = pd.failed;
            SerialNumber = pd.SerialNumber;
            FailReason = pd.FailReason;
            FailStation = pd.FailStation;
            Station = pd.Station;
            Foam = pd.Foam;
            UUID = pd.UUID;
            TimeIn = pd.TimeIn;
            TimeOut = pd.TimeOut;
            TrayId = pd.TrayId;
            HeightMeasurements = pd.HeightMeasurements;
            VisionMeasurements = pd.VisionMeasurements;
        }
        public void Consume(ProductData pd)
        {
            Copy(pd);
            pd.Reset();
        }
        public void SetFail(FailReason failreason)
        {
            present = true;
            failed = false;
            FailReason = failreason;
        }
        public void SetPass()
        {
            present = true;
            failed = true;
        }
        public object Clone()
        {
            return new ProductData(this);
        }
        public string FormatProductData()
        {
            string formattedString = string.Empty;

            //if (present || (ReelCount!=0 && SPCCount!=0))
            if (present || true)
                if (present)
                {

                    formattedString += $"{TrayId}";
                    formattedString += $",{UUID}";
                    formattedString += failed ? ",FAILED" : ",PASSED";
                    formattedString += $",{(int)FailReason:D3}";
                    formattedString += $",{FailStation}";

                    var durationSec = (TimeIn - TimeOut).TotalSeconds;
                    formattedString += $",{Foam.partno}";
                    formattedString += $",{Foam.partid}";

                    formattedString += $",";
                    foreach (var laserMeasurement in HeightMeasurements)
                    {
                        formattedString += $"{laserMeasurement.HeightMeasurement}";
                        formattedString += $"_{laserMeasurement.XMeasurePosition}";
                        formattedString += $"_{laserMeasurement.YMeasurePosition}";
                        formattedString += $";";
                    }
                    //visData += $",";
                    //foreach (var visionMeasurement in visionMeasurements)
                    //{
                    //    visData += $"{visionMeasurement}";
                    //    visData += $"_{visionMeasurement}";
                    //    visData += $"_{visionMeasurement}";
                    //    visData += $";";
                    //}
                }
                else
                    return formattedString;

            return formattedString;
        }
        #endregion Constructors

        #region Private Methods



        #endregion Private d

        #region Public Methods

        /// <summary>
        /// Reset this product data
        /// </summary>
        public void Reset()
        {
            // General
            //Index = -1;
            SerialNumber = string.Empty;
            present = false;
            failed = false;
            IsFoamPlaced = false;
            FailReason = FailReason.None;
            FailStation = StationType.Default;
            Station = StationType.Default;
            Foam = new PartData();
            TimeIn = DateTime.Now;
            TimeOut = DateTime.Now;
            TrayId = string.Empty;
            UUID = Guid.NewGuid().ToString().ToUpper();
            HeightMeasurements.Clear();
            VisionMeasurements.Clear();

        }

        /// <summary>
        /// Call to check if can measure laser height
        /// </summary>
        /// <param name="partName">Name of part to insert</param>
        /// <returns>Can proceed with insert if True</returns>
        public bool CanMeasureHeight()
        {
            return present && !failed;
        }
        /// <summary>
        /// Call to check if can insert part (not product)
        /// </summary>
        /// <param name="partName">Name of part to insert</param>
        /// <returns>Can proceed with insert if True</returns>
        public bool CanPick()
        {
            return present && !failed;
        }
        /// <summary>
        /// Call to check if able to perform action on the part
        /// </summary>
        /// <returns>Can proceed with run process if True</returns>
        public bool IsNormal()
        {
            return present && !failed;
        }

        /// <summary>
        /// Check if APC is holding part
        /// </summary>
        /// <returns></returns>
        public bool CanPickerPlaceFoam()
        {
            return Foam.present && !Foam.failed;
        }
        /// <summary>
        /// Check if APC is not holding any part
        /// </summary>
        /// <returns></returns>
        public bool CanPickerPickFoam()
        {
            return !Foam.present;
        }
        public bool CanTrayPlacePart()
        {

            return present && !failed && TrayId != string.Empty && !IsFoamPlaced ;
        }
        //public bool CanIndex(StationType stationType)
        //{
        //    var retval = true;
        //    switch (stationType)
        //    {
        //        case StationType.Input:
        //            retval = present;
        //            break;

        //        case StationType.ShieldAssembly:
        //            retval = !present || failed;
        //            break;

        //        case StationType.Output:
        //            retval = !present;
        //            break;

        //        default:
        //            retval = !present;
        //            break;
        //    }

        //    return retval;
        //}
        /// <summary>
        /// Call to check if can out part/product
        /// </summary>
        /// <returns>Can proceed with output if True</returns>
        //public bool CanOutputProduct(OutputType ot)
        //{
        //    var retval = false;
        //    switch (ot)
        //    {
        //        case OutputType.GoodOnly:
        //            retval = present && !failed;
        //            break;
        //        case OutputType.BadOnly:
        //            retval = present && failed;
        //            break;
        //        case OutputType.Both:
        //            retval = present;
        //            break;
        //        case OutputType.Special:
        //            retval = present;
        //            break;
        //        default:
        //            break;
        //    }
        //    return retval;
        //}
        /// <summary>
        /// Call to update info after outputing part/product
        /// </summary>
        /// <param name="lastout">Set to True if this is the final output (reject bin or square turret)</param>
        /// <returns>Outputed part/product info</returns>
        //public ProductData OutputedProduct(bool lastout)
        //{
        //    if (lastout)
        //    {
        //        timeout = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //    }
        //    return this;
        //}
        /// <summary>
        /// Call to update info after inserting part
        /// </summary>
        /// <param name="pname">Name of inserted part</param>
        /// <param name="pdata">Info on inserted part</param>
        //public void InsertedPart(PartName pname, PartData pdata, bool newprod = false)
        //{
        //    if (_partOnly)
        //    {
        //        present = true;
        //    }
        //    else
        //    {
        //        if (newprod)
        //        {
        //            present = true;
        //            uuid = Guid.NewGuid().ToString().ToUpper();
        //            timein = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //        }
        //    }
        //    if (pdata.failed)
        //    {
        //        failed = pdata.failed;
        //        failreason = pdata.failreason;
        //        failturret = pdata.failturret;
        //    }
        //    switch (pname)
        //    {
        //        case PartName.Top:
        //            Top = NewPart(pdata.partid, pdata.partno, pdata.vendor, pdata.lotno, pdata.datecode, pdata.machine, _partOnly,
        //                pdata.grippers, pdata.nests, pdata.failed, pdata.failreason, pdata.failturret);
        //            break;
        //        case PartName.Base:
        //            Base = NewPart(pdata.partid, pdata.partno, pdata.vendor, pdata.lotno, pdata.datecode, pdata.machine, _partOnly,
        //                pdata.grippers, pdata.nests, pdata.failed, pdata.failreason, pdata.failturret);
        //            break;
        //        case PartName.Shield:
        //            Shield = NewPart(pdata.partid, pdata.partno, pdata.vendor, pdata.lotno, pdata.datecode, pdata.machine, _partOnly,
        //                pdata.grippers, pdata.nests, pdata.failed, pdata.failreason, pdata.failturret);
        //            ShieldInserted = true;
        //            break;
        //        default:
        //            break;
        //    }
        //}
        /// <summary>
        /// Call to update info after inserting product
        /// </summary>
        /// <param name="pdata">Info on inserted product</param>
        //public void InsertedProduct(ProductData pdata, bool allIn = false, bool newprod = false, OutputReelState outstate = OutputReelState.None)
        //{
        //    if (_partOnly)
        //    {
        //        return;
        //    }

        //    if (newprod)
        //    {
        //        present = true;
        //        uuid = Guid.NewGuid().ToString().ToUpper();
        //        timein = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //        _mnfPart = pdata._mnfPart;
        //        TopPresent = pdata.TopPresent;
        //        BasePresent = pdata.BasePresent;
        //        TopInserted = pdata.TopInserted;
        //        ShieldInserted = pdata.ShieldInserted;
        //    }
        //    else
        //    {
        //        Base = pdata.Base;
        //        Top = pdata.Top;
        //        Shield = pdata.Shield;
        //        present = pdata.present;
        //        ShieldInserted = pdata.ShieldInserted;
        //        TopInserted = pdata.TopInserted;
        //        TopPresent = pdata.TopPresent;
        //        BasePresent = pdata.BasePresent;
        //        //outputreelstate = outstate;
        //        //ShieldInserted = pdata.ShieldInserted;
        //        uuid = pdata.uuid;
        //        timein = pdata.timein;
        //        timeout = pdata.timeout;
        //        switch (pdata._mnfPart)
        //        {
        //            case MnFPart.Top:
        //                TopInserted = true;
        //                _mnfPart = MnFPart.Base;
        //                break;
        //            case MnFPart.Shield:
        //                ShieldInserted = true;
        //                break;
        //            case MnFPart.Base:
        //                TopInserted = pdata.TopInserted;
        //                ShieldInserted = pdata.ShieldInserted;
        //                _mnfPart = pdata._mnfPart;
        //                break;
        //            case MnFPart.SPC:
        //                TopInserted = pdata.TopInserted;
        //                ShieldInserted = pdata.ShieldInserted;
        //                _mnfPart = pdata._mnfPart;
        //                break;
        //        }
        //    }

        //    if (pdata.failed)
        //    {
        //        failed = pdata.failed;
        //        failreason = pdata.failreason;
        //        failturret = pdata.failturret;
        //    }
        //    continuityTestResult = pdata.continuityTestResult;
        //    visionresults = pdata.visionresults;

        //}
        //public static PartData NewPart(string PartID, string PartNo, string Vendor, string LotNo, string DateCode, string Machine, bool PartOnly,
        //   string Grippers = "", string Nests = "", bool Failed = false, FailReason FailR = FailReason.None, TurretName FailT = TurretName.None, int[] vsn = null)
        //{
        //    PartData np = new PartData()
        //    {
        //        failed = Failed,
        //        inserted = !PartOnly,
        //        partid = PartID,
        //        partno = PartNo,
        //        vendor = Vendor,
        //        lotno = LotNo,
        //        datecode = DateCode,
        //        machine = Machine,
        //        grippers = Grippers,
        //        nests = Nests,
        //        failreason = FailR,
        //        //failturret = FailT,
        //        //vision = vsn
        //    };
        //    if (vsn == null)
        //    {
        //        //np.vision = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //    }
        //    return np;
        //}
        public void AddLaserHeight(double x, double y, double height)
        {
            HeightMeasurements.Add(new LaserMeasurement()
            {
                XMeasurePosition = x,
                YMeasurePosition = y,
                HeightMeasurement = height,

            });
        }


        /// <summary>
        /// Call to format the product data into string (for logging and other purposes)
        /// </summary>
        /// <param name="pd">The product data to format</param>
        /// <returns>The string formatted data</returns>
        public static string FormatProductData(ProductData pd)
        {
            return pd.FormatProductData();
        }
        /// <summary>
        /// Call to get the header for the results file
        /// </summary>
        /// <param name="index">The index of the header to return (0 for title header, 1 for columns header)</param>
        /// <returns>One line of header texts</returns>
        public static string FormatProductHeader(int index = 1)
        {
            var head = string.Empty;
            switch (index)
            {
                case 0:
                    //head = $"======== PRODUCTION RESULTS FOR STEAM CHIP ID MACHINE ========";
                    break;
                case 1:
                    head = "Date Time, Result, Fail Code, Fail Reason, Cont Result, Vis1 Result, Vis1 Data," +
                        "Vis2 Result, Vis2 Data," +
                        "Vis3 Result, Vis3 Data,";
                    break;
                default:
                    break;
            }
            return head;
        }

      
        #endregion Public Methods
    }
    /// <summary>
    /// List of fail reasons
    /// </summary>
    public enum FailReason
    {
        None = 0,

        HeightFail,

        FailToPick,
        FailToPlace,

        FoamOnTheFlyFail,
        BottomOnTheFLyFail,
        TrayOnTheFLyFail,

        Runaway,
        Missing,

        RecheckFail,

        Purged,
        Rework,
    }


    /// <summary>
    /// Types of station on the turret
    /// </summary>
    public enum StationType
    {
        Default = 0,
        Laser = 1,
        FoamAssembly = 2,
        Recheck = 3,
        Reject = 4,

    };

    /// <summary>
    /// Types of feeder used
    /// </summary>
    public enum FeederType
    {
        Feeder1 = 1,
        Feeder2 = 1,

    };


    public class LaserMeasurement
    {
        public int MeasurementCount { get; set; } = -1;
        public DateTime DateTimeMeasure { get; set; } = DateTime.MinValue;
        public double XMeasurePosition { get; set; } = -1;
        public double YMeasurePosition { get; set; } = -1;
        public double HeightMeasurement { get; set; } = -1;
        public double Nominal { get; set; } = -1;
        public double Tolerance { get; set; } = -1;
        public bool IsPass  => (HeightMeasurement >= Nominal - Tolerance) && (HeightMeasurement <= Nominal + Tolerance);
    }

    public class RecheckVisionMeasurement // TBC
    {
        public int MeasurementCount { get; set; } = -1;
        public DateTime DateTimeMeasure { get; set; } = DateTime.MinValue;
        public double Measurement { get; set; } = -1;
    }
}
