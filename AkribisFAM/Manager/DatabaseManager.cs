using AkribisFAM.Helper;
using AkribisFAM.Interfaces;
using AkribisFAM.Models;
using System;
using System.Collections.Generic;

namespace AkribisFAM.Manager
{
    /// <summary>
    /// Provides a centralized manager for all database operations.
    /// Encapsulates access to the SQLiteHelper and offers business-level APIs.
    /// </summary>
    public class DatabaseManager : IDatabaseManager
    {
        #region Private Member

        private readonly SQLiteHelper _sqliteHelper;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the DatabaseManager with the SQLite database path.
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file.</param>
        public DatabaseManager(string databasePath)
        {
            _sqliteHelper = new SQLiteHelper(databasePath);
            _sqliteHelper.OpenConnection();
            InitializeDatabase();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new alarm record to the database.
        /// </summary>
        /// <param name="alarm">Alarm record to insert.</param>
        public bool AddAlarm(AlarmRecord alarm)
        {
            return _sqliteHelper.AddAlarm(alarm);
        }

        /// <summary>
        /// Adds an OEE record via the helper class.
        /// </summary>
        /// <param name="record">The OEE record to add.</param>
        public bool AddOeeRecord(OeeRecord record)
        {
            return _sqliteHelper.AddOeeRecord(record);
        }

        /// <summary>
        /// Adds an lot record via the helper class.
        /// </summary>
        /// <param name="record">The lot record to add.</param>
        public bool AddLotRecord(LotRecord lot)
        {
            return _sqliteHelper.AddLot(lot);
        }

        public bool UpdateLotRecord(LotRecord lot)
        {
            return _sqliteHelper.UpdateLotHistory(lot);
        }
        public LotRecord GetCurrentLot()
        {
            return _sqliteHelper.GetCurrentLotRecord();
        }
        public List<LotRecord> GetLotRecords()
        {
            return _sqliteHelper.GetLots(); 
        }
        /// <summary>
        /// Get alarms between two dates.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<AlarmRecord> GetAlarms(/*DateTime from, DateTime to*/)
        {
            return _sqliteHelper.GetAlarms(); // TODO: filter function
        }

        /// <summary>
        /// get OEE records between two dates.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<OeeRecord> GetOeeRecords(/*DateTime from, DateTime to*/)
        {
            return _sqliteHelper.GetOeeRecords(); // TODO: filter function
        }


        // Add more methods here:
        // - ResolveAlarm(...)
        // - GetActiveAlarms()

        /// <summary>
        /// Closes and disposes of the SQLite connection and helpers.
        /// </summary>
        public void Dispose()
        {
            _sqliteHelper?.Dispose();
        }



        #endregion

        #region Private Method
        /// <summary>
        /// Ensures the necessary database tables are created if they do not exist.
        /// </summary>
        private void InitializeDatabase()
        {
            // Create Alarms table
            string createAlarmsTable = @"
        CREATE TABLE IF NOT EXISTS Alarm_History (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AlarmLevel TEXT NOT NULL,
            AlarmCode TEXT NOT NULL,
            AlarmMessage TEXT NOT NULL,
            LotID TEXT NOT NULL,
            TimeOccurred TEXT NOT NULL,
            TimeResolved TEXT,
            UserID TEXT NOT NULL
        );";

            // Create Lots table
            string createLotsTable = @"
        CREATE TABLE IF NOT EXISTS Lot_History (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            LotId TEXT NOT NULL,
            Creator TEXT NOT NULL,
            StartDateTime DATETIME  NOT NULL,
            EndDateTime DATETIME  NOT NULL,
            LotState INTEGER NOT NULL,
            Recipe TEXT NOT NULL
        );";

            //    // Create Errors table
            //    string createErrorsTable = @"
            //CREATE TABLE IF NOT EXISTS Errors (
            //    Id INTEGER PRIMARY KEY AUTOINCREMENT,
            //    ErrorCode TEXT NOT NULL,
            //    ErrorMessage TEXT NOT NULL,
            //    LotID TEXT NOT NULL,
            //    TimeOccurred TEXT NOT NULL,
            //    TimeResolved TEXT,
            //    UserID TEXT NOT NULL
            //);";

            // Create OEE Records table
            string createOeeTable = @"
        CREATE TABLE IF NOT EXISTS Oee_History (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            LotId TEXT NOT NULL,
            StartDateTime TEXT NOT NULL,
            EndDateTime TEXT NOT NULL,
            GoodProducts INTEGER,
            GoodVision1 INTEGER,
            GoodVision2 INTEGER,
            GoodVision3 INTEGER,
            RejectVision1 INTEGER,
            RejectVision2 INTEGER,
            RejectVision3 INTEGER,
            AlarmsCount INTEGER,
            PlannedUPH REAL,
            PlannedProductionTime REAL,
            UpTimeHr REAL,
            DownTimeHr REAL,
            IdleTimeHr REAL,
            TotalTimeHr REAL,
            ProductiveTimeHr REAL,
            MTBF REAL,
            MTTR REAL,
            Availability REAL,
            Performance REAL,
            Quality REAL,
            Oee REAL
        );";

            _sqliteHelper.ExecuteNonQuery(createAlarmsTable);
            _sqliteHelper.ExecuteNonQuery(createOeeTable);
            _sqliteHelper.ExecuteNonQuery(createLotsTable);
        }



        #endregion
    }
}
