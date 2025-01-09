using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace AbuseIPDBCacheComponent
{
    public static class DatabaseManager
    {
        private static readonly object _lock = new object();
        private static SQLiteConnection _connection;
        private static SQLiteCommand _command;

        public static string ConnectionString => $"Data Source={DatabasePath};Version=3;";

        public static string DatabasePath
        {
            get
            {
                string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string dllDirectory = Path.GetDirectoryName(dllPath);
                return Path.Combine(dllDirectory, "AbuseIPDBCache.db");
            }
        }

        public static void CreateConnection()
        {
            _connection = new SQLiteConnection(ConnectionString);
            _connection.Open();
            _command = _connection.CreateCommand();
            InitializeDatabase();
            return;
        }

        public static void CloseConnection()
        {
            if(_connection != null && _connection.State != System.Data.ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        public static void InitializeDatabase()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS CachedResponses (
                            IpAddress TEXT PRIMARY KEY,
                            IsPublic INTEGER,
                            IpVersion INTEGER,
                            IsWhitelisted INTEGER,
                            AbuseConfidenceScore INTEGER,
                            CountryCode TEXT,
                            CountryName TEXT,
                            UsageType TEXT,
                            ISP TEXT,
                            Domain TEXT,
                            IsTor INTEGER,
                            TotalReports INTEGER,
                            NumDistinctUsers INTEGER,
                            LastReportedAt TEXT,
                            ExpirationDateTime TEXT
                        )";
            _command.CommandText = sql;
            _command.ExecuteNonQuery();
            _command.CommandText = null;
        }

        /// <summary>
        /// Cache the response from AbuseIPDB
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task CacheResponse(string ip, AbuseIpDbResponse response)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    _command.CommandText = @"INSERT OR REPLACE INTO CachedResponses (
                                IpAddress,
                                IsPublic,
                                IpVersion,
                                IsWhitelisted,
                                AbuseConfidenceScore,
                                CountryCode,
                                CountryName,
                                UsageType,
                                ISP,
                                Domain,
                                IsTor,
                                TotalReports,
                                NumDistinctUsers,
                                LastReportedAt,
                                ExpirationDateTime) 
                            VALUES (
                                @IpAddress,
                                @IsPublic,
                                @IpVersion,
                                @IsWhitelisted,
                                @AbuseConfidenceScore,
                                @CountryCode,
                                @CountryName,
                                @UsageType,
                                @ISP,
                                @Domain,
                                @IsTor,
                                @TotalReports,
                                @NumDistinctUsers,
                                @LastReportedAt,
                                @ExpirationDateTime)";

                    _command.Parameters.AddWithValue("@IpAddress", ip);
                    _command.Parameters.AddWithValue("@IsPublic", response.data.isPublic);
                    _command.Parameters.AddWithValue("@IpVersion", response.data.ipVersion);
                    _command.Parameters.AddWithValue("@IsWhitelisted", response.data.isWhitelisted);
                    _command.Parameters.AddWithValue("@AbuseConfidenceScore", response.data.abuseConfidenceScore);
                    _command.Parameters.AddWithValue("@CountryCode", response.data.countryCode);
                    _command.Parameters.AddWithValue("@CountryName", response.data.countryName);
                    _command.Parameters.AddWithValue("@UsageType", response.data.usageType);
                    _command.Parameters.AddWithValue("@ISP", response.data.isp);
                    _command.Parameters.AddWithValue("@Domain", response.data.domain);
                    _command.Parameters.AddWithValue("@IsTor", response.data.isTor);
                    _command.Parameters.AddWithValue("@TotalReports", response.data.totalReports);
                    _command.Parameters.AddWithValue("@NumDistinctUsers", response.data.numDistinctUsers);
                    _command.Parameters.AddWithValue("@LastReportedAt", response.data.lastReportedAt);
                    _command.Parameters.AddWithValue("@ExpirationDateTime", DateTime.Now.AddHours(1));
                    _command.ExecuteNonQuery();
                    _command.Parameters.Clear();
                    _command.CommandText = null;
                }
            });
        }


        /// <summary>
        /// Check if he have the data
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool TryGetCachedResponse(string ip, out AbuseIpDbResponse response)
        {
            response = null;

            _command.CommandText = "SELECT * FROM CachedResponses WHERE IpAddress = @IpAddress";
            _command.Parameters.AddWithValue("@IpAddress", ip);
            using (SQLiteDataReader reader = _command.ExecuteReader())
            {
                _command.Parameters.Clear();
                if (reader.Read())
                {
                    DateTime expirationDateTime = reader.GetDateTime(reader.GetOrdinal("ExpirationDateTime"));
                    if (expirationDateTime > DateTime.Now)
                    {
                        response = new AbuseIpDbResponse
                        {
                            data = new Data
                            {
                                isFromCache = true,
                                isSuccess = true,
                                ipAddress = Convert.ToString(reader["IpAddress"]),
                                isPublic = Convert.ToBoolean(reader["IsPublic"]),
                                ipVersion = Convert.ToInt32(reader["IpVersion"]),
                                isWhitelisted = reader.IsDBNull(reader.GetOrdinal("IsWhitelisted")) ? false : Convert.ToBoolean(reader["IsWhitelisted"]),
                                abuseConfidenceScore = Convert.ToInt32(reader["AbuseConfidenceScore"]),
                                countryCode = Convert.ToString(reader["CountryCode"]),
                                countryName = Convert.ToString(reader["CountryName"]),
                                usageType = Convert.ToString(reader["UsageType"]),
                                isp = Convert.ToString(reader["ISP"]),
                                domain = Convert.ToString(reader["Domain"]),
                                isTor = reader.IsDBNull(reader.GetOrdinal("IsTor")) ? false : Convert.ToBoolean(reader["IsTor"]),
                                totalReports = reader.IsDBNull(reader.GetOrdinal("TotalReports")) ? 0 : Convert.ToInt32(reader["TotalReports"]),
                                numDistinctUsers = reader.IsDBNull(reader.GetOrdinal("NumDistinctUsers")) ? 0 : Convert.ToInt32(reader["NumDistinctUsers"]),
                                lastReportedAt = reader.IsDBNull(reader.GetOrdinal("LastReportedAt")) ? (DateTime?)null : Convert.ToDateTime(reader["LastReportedAt"])
                            }
                        };
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static void ExecuteDatabaseOperation(string commandString)
        {
            _command.CommandText = commandString;
            _command.ExecuteNonQuery();
            _command.Parameters.Clear();
        }

        public static void DBOperation(string commandString, bool lockThis = false)
        {
            if (lockThis)
            {
                lock (_lock)
                {
                    ExecuteDatabaseOperation(commandString);
                }
            }
            else
            {
                ExecuteDatabaseOperation(commandString);
            }
        }
    }
}
