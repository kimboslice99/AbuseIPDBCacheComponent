using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace AbuseIPDBCacheComponent
{
    public static class DatabaseManager
    {
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

        /// <summary>
        /// Creates SQLite connection and opens it
        /// </summary>
        /// <returns>Opened SQLite connection</returns>
        public static SQLiteConnection CreateConnectionAndOpen()
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "PRAGMA busy_timeout = 10000;";
            command.ExecuteNonQuery();
            return connection;
        }

        public static void CloseConnection(SQLiteConnection connection)
        {
            if(connection != null && connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        public static void InitializeDatabase(SQLiteCommand command)
        {
            // WAL
            command.CommandText = "PRAGMA journal_mode=WAL";
            command.ExecuteNonQuery();
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
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Cache the response from AbuseIPDB
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task CacheResponseAndClose(SQLiteCommand command, string ip, AbuseIpDbResponse response)
        {
            await Task.Run(() =>
            {
                command.CommandText = @"INSERT OR REPLACE INTO CachedResponses (
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

                command.Parameters.AddWithValue("@IpAddress", ip);
                command.Parameters.AddWithValue("@IsPublic", response.data.isPublic);
                command.Parameters.AddWithValue("@IpVersion", response.data.ipVersion);
                command.Parameters.AddWithValue("@IsWhitelisted", response.data.isWhitelisted);
                command.Parameters.AddWithValue("@AbuseConfidenceScore", response.data.abuseConfidenceScore);
                command.Parameters.AddWithValue("@CountryCode", response.data.countryCode);
                command.Parameters.AddWithValue("@CountryName", response.data.countryName);
                command.Parameters.AddWithValue("@UsageType", response.data.usageType);
                command.Parameters.AddWithValue("@ISP", response.data.isp);
                command.Parameters.AddWithValue("@Domain", response.data.domain);
                command.Parameters.AddWithValue("@IsTor", response.data.isTor);
                command.Parameters.AddWithValue("@TotalReports", response.data.totalReports);
                command.Parameters.AddWithValue("@NumDistinctUsers", response.data.numDistinctUsers);
                command.Parameters.AddWithValue("@LastReportedAt", response.data.lastReportedAt);
                command.Parameters.AddWithValue("@ExpirationDateTime", DateTime.Now.AddHours(Config.CacheTime));
                command.ExecuteNonQuery();
                command.Connection.Close();
            });
        }


        /// <summary>
        /// Check if we have data for this ip
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ip"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool TryGetCachedResponse(SQLiteCommand command, string ip, out AbuseIpDbResponse response)
        {
            response = null;
            command.CommandText = "SELECT * FROM CachedResponses WHERE IpAddress = @IpAddress";
            command.Parameters.AddWithValue("@IpAddress", ip);
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                command.Parameters.Clear();
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
                                lastReportedAt = reader.IsDBNull(reader.GetOrdinal("LastReportedAt")) ? (DateTime?)null : Convert.ToDateTime(reader["LastReportedAt"]),
                                expirationDateTime = reader.IsDBNull(reader.GetOrdinal("ExpirationDateTime")) ? (DateTime?)null : Convert.ToDateTime(reader["ExpirationDateTime"]),
                            }
                        };
                        return true;
                    }
                }
            }
            return false;
        }

        public static void DBOperation(SQLiteCommand command, string commandString)
        {
            command.CommandText = commandString;
            command.ExecuteNonQuery();
        }
    }
}
