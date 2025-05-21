using AkribisFAM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

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
        /// Initializes a new instance of the <see cref="SQLiteHelper"/> class with the specified database path.
        /// </summary>
        /// <param name="databasePath">The full path to the SQLite database file.</param>
        public SQLiteHelper(string databasePath)
        {
            _connectionString = $"Data Source={databasePath};Version=3;";
            _connection = new SQLiteConnection(_connectionString);
        }

        #endregion Constructors

        #region Event Handlers/Action Methods

        #endregion Event Handlers/Action Methods

        #region Private Methods

        /// <summary>
        /// Adds parameters to a <see cref="SQLiteCommand"/> if provided.
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
                _connection.Open();
        }

        /// <summary>
        /// Closes the SQLite database connection if it is open.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
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
        /// Inserts an OEE record into the database.
        /// </summary>
        /// <param name="record">The OEE record containing all relevant data.</param>
        public void InsertOeeRecord(OeeRecord record)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO OEERecords (
                            StartDateTime, EndDateTime, GoodProducts, GoodVision1, GoodVision2, GoodVision3,
                            RejectVision1, RejectVision2, RejectVision3, AlarmsCount,
                            PlannedUPH, PlannedProductionTime, UpTimeHr, DownTimeHr, IdleTimeHr,
                            TotalTimeHr, ProductiveTimeHr, MTBF, MTTR, Availability, Performance, Quality, OEE
                        )
                        VALUES (
                            @StartDateTime, @EndDateTime, @GoodProducts, @GoodVision1, @GoodVision2, @GoodVision3,
                            @RejectVision1, @RejectVision2, @RejectVision3, @AlarmsCount,
                            @PlannedUPH, @PlannedProductionTime, @UpTimeHr, @DownTimeHr, @IdleTimeHr,
                            @TotalTimeHr, @ProductiveTimeHr, @MTBF, @MTTR, @Availability, @Performance, @Quality, @OEE
                        );";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDateTime", record.StartDateTime);
                    command.Parameters.AddWithValue("@EndDateTime", record.EndDateTime);
                    command.Parameters.AddWithValue("@GoodProducts", record.GoodProducts);
                    command.Parameters.AddWithValue("@GoodVision1", record.GoodVision1);
                    command.Parameters.AddWithValue("@GoodVision2", record.GoodVision2);
                    command.Parameters.AddWithValue("@GoodVision3", record.GoodVision3);
                    command.Parameters.AddWithValue("@RejectVision1", record.RejectVision1);
                    command.Parameters.AddWithValue("@RejectVision2", record.RejectVision2);
                    command.Parameters.AddWithValue("@RejectVision3", record.RejectVision3);
                    command.Parameters.AddWithValue("@AlarmsCount", record.AlarmsCount);
                    command.Parameters.AddWithValue("@PlannedUPH", record.PlannedUPH);
                    command.Parameters.AddWithValue("@PlannedProductionTime", record.PlannedProductionTime);
                    command.Parameters.AddWithValue("@UpTimeHr", record.UpTimeHr);
                    command.Parameters.AddWithValue("@DownTimeHr", record.DownTimeHr);
                    command.Parameters.AddWithValue("@IdleTimeHr", record.IdleTimeHr);
                    command.Parameters.AddWithValue("@TotalTimeHr", record.TotalTimeHr);
                    command.Parameters.AddWithValue("@ProductiveTimeHr", record.ProductiveTimeHr);
                    command.Parameters.AddWithValue("@MTBF", record.MTBF);
                    command.Parameters.AddWithValue("@MTTR", record.MTTR);
                    command.Parameters.AddWithValue("@Availability", record.Availability);
                    command.Parameters.AddWithValue("@Performance", record.Performance);
                    command.Parameters.AddWithValue("@Quality", record.Quality);
                    command.Parameters.AddWithValue("@OEE", record.Oee);

                    command.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Inserts a new alarm event into the database.
        /// </summary>
        /// <param name="alarm">The alarm record object containing all relevant information.</param>
        public void InsertAlarm(AlarmRecord alarm)
        {
            string sql = @"INSERT INTO Alarm_Log 
                   (AlarmLevel, AlarmCode, AlarmMessage, LodID, TimeOccurred, TimeResolved, UserID)
                   VALUES 
                   (@level, @code, @msg, @lot, @occur, @resolve, @user)";

            ExecuteNonQuery(sql, new Dictionary<string, object>
            {
                { "@level", alarm.AlarmLevel },
                { "@code", alarm.AlarmCode },
                { "@msg", alarm.AlarmMessage },
                { "@lot", alarm.LotID },
                { "@occur", alarm.TimeOccurred },
                { "@resolve", (object)alarm.TimeResolved ?? DBNull.Value },
                { "@user", alarm.UserID }
            });
        }

        /// <summary>
        /// Inserts a new error event into the database.
        /// </summary>
        /// <param name="source">The source or module where the error occurred.</param>
        /// <param name="description">The error description.</param>
        /// <param name="timestamp">The time the error occurred.</param>
        public void InsertError(string source, string description, DateTime timestamp)
        {
            string sql = @"INSERT INTO Error_Log (Source, Description, Timestamp)
                           VALUES (@src, @desc, @ts)";
            ExecuteNonQuery(sql, new Dictionary<string, object>
            {
                { "@src", source },
                { "@desc", description },
                { "@ts", timestamp }
            });
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
