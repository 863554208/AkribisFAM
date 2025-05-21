using AkribisFAM.Helper;
using AkribisFAM.Interfaces;
using AkribisFAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new alarm record to the database.
        /// </summary>
        /// <param name="alarm">Alarm record to insert.</param>
        public void AddAlarm(AlarmRecord alarm)
        {
            _sqliteHelper.InsertAlarm(alarm);
        }

        /// <summary>
        /// Adds an OEE record via the helper class.
        /// </summary>
        /// <param name="record">The OEE record to add.</param>
        public void AddOeeRecord(OeeRecord record)
        {
            _sqliteHelper.InsertOeeRecord(record);
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
    }
}
