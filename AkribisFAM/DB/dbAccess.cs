using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace AkribisFAM.DB
{
    using System;
    using System.Data.SQLite;

    public static class DatabaseManager
    {
        private static SQLiteConnection _connection;
        private static string _connectionString;

        public static void Initialize(string dbFilePath = "D:\\DB\\myDatabase.sqlite")
        {
            _connectionString = $"Data Source={dbFilePath};Version=3;";
            _connection = new SQLiteConnection(_connectionString);
            _connection.Open();
            Console.WriteLine("数据库连接已建立");
        }

        public static void Insert(string dbFilePath)
        {
            var con = Connection;
            string insertQuery = "INSERT INTO students (name, age) VALUES ('John Doe', 25);";
            using (var command = new SQLiteCommand(insertQuery, _connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine("表 students 已创建（如果不存在）");
            }
        }


        public static SQLiteConnection Connection
        {
            get
            {
                if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
                    throw new InvalidOperationException("数据库连接未初始化或已关闭");
                return _connection;
            }
        }

        public static void Shutdown()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
                Console.WriteLine("数据库连接已断开");
            }
        }
    }

}
