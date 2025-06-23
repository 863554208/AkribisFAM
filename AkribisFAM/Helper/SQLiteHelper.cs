using AAMotion;
using AkribisFAM.Manager;
using AkribisFAM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace AkribisFAM.Helper
{
    public class SQLiteHelper : IDisposable
    {
        #region Private Member

        private readonly string _connectionString;
        private SQLiteConnection _connection;

        #endregion Private Member

        #region Private Properties

        #endregion Private Properties

        #region Public Properties

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the SQLiteHelper class with the specified database path.
        /// </summary>
        /// <param name="databasePath">The full path to the SQLite database file.</param>
        public SQLiteHelper(string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            _connectionString = $"Data Source={databasePath};Version=3;";
            _connection = new SQLiteConnection(_connectionString);
        }

        #endregion Constructors

        #region Event Handlers/Action Methods

        #endregion Event Handlers/Action Methods

        #region Private Methods

        /// <summary>
        /// Adds parameters to a SQLiteCommand if provided.
        /// </summary>
        /// <param name="cmd">The command to which parameters will be added.</param>
        /// <param name="parameters">The parameters dictionary.</param>
        private void AddParameters(SQLiteCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters == null) return;

            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Opens the SQLite database connection if it is not already open.
        /// </summary>
        public void OpenConnection()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// Closes the SQLite database connection if it is open.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// Executes a non-query SQL command (e.g., INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional parameters for the query.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (var cmd = new SQLiteCommand(query, _connection))
            {
                AddParameters(cmd, parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a SQL query that returns a single value.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional parameters for the query.</param>
        /// <returns>The value returned by the query.</returns>
        public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            using (var cmd = new SQLiteCommand(query, _connection))
            {
                AddParameters(cmd, parameters);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes a SQL query that returns multiple rows and columns.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="parameters">Optional parameters for the query.</param>
        /// <returns>A <see cref="DataTable"/> containing the result set.</returns>
        public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (var cmd = new SQLiteCommand(query, _connection))
            {
                AddParameters(cmd, parameters);
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Adds a new OEE (Overall Equipment Effectiveness) record to the database.
        /// </summary>
        /// <param name="oee">The <see cref="OeeRecord"/> containing OEE data.</param>
        /// <returns>True if the record is successfully inserted; otherwise, false.</returns>
        public bool AddOeeRecord(OeeRecord oee)
        {
            const string query = @"
        INSERT INTO Oee_History (LotId,
            StartDateTime, EndDateTime, GoodProducts,
            GoodVision1, GoodVision2, GoodVision3,
            RejectVision1, RejectVision2, RejectVision3,
            AlarmsCount, PlannedUPH, PlannedProductionTime,
            UpTimeHr, DownTimeHr, IdleTimeHr, TotalTimeHr,
            ProductiveTimeHr, MTBF, MTTR,
            Availability, Performance, Quality, Oee
        )
        VALUES (@lotId,
            @startDateTime, @endDateTime, @goodProducts,
            @goodVision1, @goodVision2, @goodVision3,
            @rejectVision1, @rejectVision2, @rejectVision3,
            @alarmsCount, @plannedUPH, @plannedProductionTime,
            @upTimeHr, @downTimeHr, @idleTimeHr, @totalTimeHr,
            @productiveTimeHr, @mtbf, @mttr,
            @availability, @performance, @quality, @oee
        );";

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@lotId", oee.LotID);
                        command.Parameters.AddWithValue("@startDateTime", oee.StartDateTime);
                        command.Parameters.AddWithValue("@endDateTime", oee.EndDateTime);
                        command.Parameters.AddWithValue("@goodProducts", oee.GoodProducts);
                        command.Parameters.AddWithValue("@goodVision1", oee.GoodVision1);
                        command.Parameters.AddWithValue("@goodVision2", oee.GoodVision2);
                        command.Parameters.AddWithValue("@goodVision3", oee.GoodVision3);
                        command.Parameters.AddWithValue("@rejectVision1", oee.RejectVision1);
                        command.Parameters.AddWithValue("@rejectVision2", oee.RejectVision2);
                        command.Parameters.AddWithValue("@rejectVision3", oee.RejectVision3);
                        command.Parameters.AddWithValue("@alarmsCount", oee.AlarmsCount);
                        command.Parameters.AddWithValue("@plannedUPH", oee.PlannedUPH);
                        command.Parameters.AddWithValue("@plannedProductionTime", oee.PlannedProductionTime);
                        command.Parameters.AddWithValue("@upTimeHr", oee.UpTimeHr);
                        command.Parameters.AddWithValue("@downTimeHr", oee.DownTimeHr);
                        command.Parameters.AddWithValue("@idleTimeHr", oee.IdleTimeHr);
                        command.Parameters.AddWithValue("@totalTimeHr", oee.TotalTimeHr);
                        command.Parameters.AddWithValue("@productiveTimeHr", oee.ProductiveTimeHr);
                        command.Parameters.AddWithValue("@mtbf", oee.MTBF);
                        command.Parameters.AddWithValue("@mttr", oee.MTTR);
                        command.Parameters.AddWithValue("@availability", oee.Availability);
                        command.Parameters.AddWithValue("@performance", oee.Performance);
                        command.Parameters.AddWithValue("@quality", oee.Quality);
                        command.Parameters.AddWithValue("@oee", oee.Oee);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception as needed
                return false;
            }
        }

        /// <summary>
        /// Adds a new alarm record to the database.
        /// </summary>
        /// <param name="alarm">The <see cref="AlarmRecord"/> containing alarm details.</param>
        /// <returns>True if the insert was successful; otherwise, false.</returns>
        public bool AddAlarm(AlarmRecord alarm)
        {
            const string query = @"
        INSERT INTO Alarm_History (AlarmLevel, AlarmCode, AlarmMessage, LotID, TimeOccurred, TimeResolved, UserID)
        VALUES (@level, @code, @message, @lot, @occur, @resolve, @user);";

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@level", alarm.AlarmLevel);
                        command.Parameters.AddWithValue("@code", alarm.AlarmCode);
                        command.Parameters.AddWithValue("@message", alarm.AlarmMessage);
                        command.Parameters.AddWithValue("@lot", alarm.LotID); // corrected here
                        command.Parameters.AddWithValue("@occur", alarm.TimeOccurred);
                        command.Parameters.AddWithValue("@resolve", (object)alarm.TimeResolved ?? DBNull.Value);
                        command.Parameters.AddWithValue("@user", alarm.UserID);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return false;
            }
        }
        /// <summary>
        /// Adds a new lot record to the database.
        /// </summary>
        /// <param name="alarm">The <see cref="LotRecord"/> containing lot details.</param>
        /// <returns>True if the insert was successful; otherwise, false.</returns>
        public bool AddLot(LotRecord lot)
        {
            const string query = @"
        INSERT INTO Lot_History (LotId, Creator, StartDateTime, EndDateTime, LotState, Recipe)
        VALUES (@lotid, @creator, @startdatetime, @enddatetime, @state, @recipe);";

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@lotid", lot.LotID);
                        command.Parameters.AddWithValue("@creator", lot.Creator);
                        command.Parameters.AddWithValue("@startdatetime", lot.StartDateTime);
                        command.Parameters.AddWithValue("@enddatetime", lot.EndDateTime); // corrected here
                        command.Parameters.AddWithValue("@state", lot.LotState);
                        command.Parameters.AddWithValue("@recipe", lot.RecipeName);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return false;
            }
        }

        /// <summary>
        /// Retrieves a list of lots records from the database.
        /// </summary>
        /// <returns>List of LotRecord objects retrieved from the database.</returns>
        public List<LotRecord> GetLots()
        {
            var records = new List<LotRecord>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Lot_History";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var record = new LotRecord
                        {
                            LotID = reader.GetString(reader.GetOrdinal("LotId")),
                            StartDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime")),
                            EndDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime")),
                            LotState = reader.GetInt32(reader.GetOrdinal("LotState")),
                            RecipeName = reader.GetString(reader.GetOrdinal("Recipe")),
                            Creator = reader.GetString(reader.GetOrdinal("Creator"))
                        };

                        records.Add(record);
                    }
                }
            }

            return records;
        }
        public bool UpdateLotHistory(LotRecord lot)
        {
            string query = @"
            UPDATE Lot_History SET
                Creator = @Creator,
                StartDateTime = @StartDateTime,
                EndDateTime = @EndDateTime,
                LotState = @LotState,
                Recipe = @Recipe
            WHERE LotId = @lotId;";

            using (SQLiteConnection conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Creator", lot.Creator);
                    cmd.Parameters.AddWithValue("@StartDateTime", lot.StartDateTime);
                    cmd.Parameters.AddWithValue("@EndDateTime", lot.EndDateTime); // corrected here
                    cmd.Parameters.AddWithValue("@Recipe", lot.RecipeName);
                    cmd.Parameters.AddWithValue("@LotState", (int)Lot.LotState.Completed);
                    cmd.Parameters.AddWithValue("@lotId", lot.LotID);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} record(s) updated.");
                }
            }
            return true;
        }
        /// <summary>
        /// Retrieves a list of OEE records from the database.
        /// </summary>
        /// <returns>List of OeeRecord objects retrieved from the database.</returns>
        public List<OeeRecord> GetOeeRecords()
        {
            var records = new List<OeeRecord>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Oee_History";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var record = new OeeRecord
                        {
                            StartDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime")),
                            EndDateTime = reader.GetDateTime(reader.GetOrdinal("EndDateTime")),
                            GoodProducts = reader.GetInt32(reader.GetOrdinal("GoodProducts")),
                            GoodVision1 = reader.GetInt32(reader.GetOrdinal("GoodVision1")),
                            GoodVision2 = reader.GetInt32(reader.GetOrdinal("GoodVision2")),
                            GoodVision3 = reader.GetInt32(reader.GetOrdinal("GoodVision3")),
                            RejectVision1 = reader.GetInt32(reader.GetOrdinal("RejectVision1")),
                            RejectVision2 = reader.GetInt32(reader.GetOrdinal("RejectVision2")),
                            RejectVision3 = reader.GetInt32(reader.GetOrdinal("RejectVision3")),
                            AlarmsCount = reader.GetInt32(reader.GetOrdinal("AlarmsCount")),
                            PlannedUPH = reader.GetDouble(reader.GetOrdinal("PlannedUPH")),
                            PlannedProductionTime = reader.GetDouble(reader.GetOrdinal("PlannedProductionTime")),
                            UpTimeHr = reader.GetDouble(reader.GetOrdinal("UpTimeHr")),
                            DownTimeHr = reader.GetDouble(reader.GetOrdinal("DownTimeHr")),
                            IdleTimeHr = reader.GetDouble(reader.GetOrdinal("IdleTimeHr")),
                            TotalTimeHr = reader.GetDouble(reader.GetOrdinal("TotalTimeHr")),
                            ProductiveTimeHr = reader.GetDouble(reader.GetOrdinal("ProductiveTimeHr")),
                            MTBF = reader.GetDouble(reader.GetOrdinal("MTBF")),
                            MTTR = reader.GetDouble(reader.GetOrdinal("MTTR")),
                            Availability = reader.GetDouble(reader.GetOrdinal("Availability")),
                            Performance = reader.GetDouble(reader.GetOrdinal("Performance")),
                            Quality = reader.GetDouble(reader.GetOrdinal("Quality")),
                            Oee = reader.GetDouble(reader.GetOrdinal("Oee"))
                        };

                        records.Add(record);
                    }
                }
            }

            return records;
        }
         /// <summary>
        /// Retrieves a running lot record from the database.
        /// </summary>
        /// <returns>List of LotRecord objects retrieved from the database.</returns>
        public LotRecord GetCurrentLotRecord()
        {
            var record = new LotRecord();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Lot_History WHERE LotState = @lotstate;";
                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@lotstate", (int)Lot.LotState.Running_Lot);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                             record = new LotRecord()
                            {
                                LotID = reader.GetString(reader.GetOrdinal("LotId")),
                                StartDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime")),
                                EndDateTime = reader.GetDateTime(reader.GetOrdinal("EndDateTime")),
                                Creator = reader.GetString(reader.GetOrdinal("Creator")),
                                RecipeName = reader.GetString(reader.GetOrdinal("Recipe")),
                            };

                        }
                    }

                }
             }

            return record;
        }
        /// <summary>
        /// Retrieves a list of alarm records from the database.
        /// </summary>
        /// <returns>List of AlarmRecord objects retrieved from the database.</returns>
        public List<AlarmRecord> GetAlarms()
        {
            var records = new List<AlarmRecord>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Alarm_History";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var record = new AlarmRecord
                        {
                            AlarmLevel = reader.GetString(reader.GetOrdinal("AlarmLevel")),
                            AlarmCode = reader.GetString(reader.GetOrdinal("AlarmCode")),
                            AlarmMessage = reader.GetString(reader.GetOrdinal("AlarmMessage")),
                            LotID = reader.GetString(reader.GetOrdinal("LotID")),
                            TimeOccurred = reader.GetDateTime(reader.GetOrdinal("TimeOccurred")),
                            TimeResolved = reader.IsDBNull(reader.GetOrdinal("TimeResolved")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("TimeResolved")),
                            UserID = reader.GetString(reader.GetOrdinal("UserID"))
                        };

                        records.Add(record);
                    }
                }
            }

            return records;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="SQLiteHelper"/> class.
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                CloseConnection();
                _connection.Dispose();
                _connection = null;
            }
        }

        #endregion Public Methods
    }
}
