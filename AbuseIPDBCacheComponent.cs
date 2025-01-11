using System;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;
using System.Text.Json;
using System.Diagnostics;
using System.Data.SQLite;
using System.Net.Configuration;

namespace AbuseIPDBCacheComponent
{
    // Define a COM interface
    [Guid("ECED3D83-2DE5-4C53-8B57-E95C6D90422D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    [ComVisible(true)]
    public interface IAbuseIPDB
    {
        bool Block(string ip);
        bool Report(string ip, string categories, string comment);
        // all of the check items
        bool IsSuccess();
        bool IsFromCache();
        string GetIpAddress();
        bool IsPublic();
        int GetIpVersion();
        bool IsWhitelisted();
        int GetAbuseConfidenceScore();
        string GetCountryCode();
        string GetCountryName();
        string GetUsageType();
        string GetISP();
        string GetDomain();
        bool IsTor();
        int GetTotalReports();
        int GetNumDistinctUsers();
        DateTime GetLastReportedAt();
        // set global variables
        void SetMaxAgeInDays(int maxAgeInDays);
        void SetApiKey(string apiKey);
        void SetMaxConfidenceScore(int maxConfidenceScore);
        void VacuumDB();
        void ClearDB();
        void ClearExpiredDB();
    }

    // Define a class that implements the COM interface
    [Guid("c1f9d247-e82d-4612-aa5b-ca3dde103a27")]
    [ClassInterface(ClassInterfaceType.None)]
    public class AbuseIPDBClient : IAbuseIPDB
    {
        private int maxAgeInDays = 30; // Default value
        private int maxConfidenceScore = 50;
        private string apiKey = "";
        private AbuseIpDbResponse response;
        private readonly bool _loggingEnabled = Config.LoggingEnabled;
        private object _lock = new object();

        static AbuseIPDBClient()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Config.MyResolveEventHandler;
            Logger.LogToFile("Init()");
            SQLiteConnection connection = DatabaseManager.CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            DatabaseManager.InitializeDatabase(command);
            connection.Close();
        }

        /// <summary>
        /// Report an IP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="comment"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        public bool Report(string ip, string categories, string comment = "")
        {
            try
            {
                Stopwatch stopwatch = null;
                if(_loggingEnabled)
                    stopwatch = Stopwatch.StartNew();
                HttpClient client = HttpClientSingleton.Instance;
                comment = string.IsNullOrEmpty(comment) ? $"{DateTime.Now}" : comment;
                using (var request = new HttpRequestMessage(HttpMethod.Post, "report"))
                {
                    request.Headers.Add("Key", apiKey);

                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString.Add("ip", ip);
                    queryString.Add("comment", comment);
                    queryString.Add("categories", categories);

                    request.RequestUri = new Uri(client.BaseAddress, $"report?{queryString}");

                    HttpResponseMessage httpResponse = client.SendAsync(request).GetAwaiter().GetResult();

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        if (_loggingEnabled)
                            Logger.LogToFile($"request to {request.RequestUri} successful {httpResponse.StatusCode} process time {stopwatch.Elapsed.TotalMilliseconds}ms");

                        using (var responseStream = httpResponse.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                        {
                            response = JsonSerializer.DeserializeAsync<AbuseIpDbResponse>(responseStream).GetAwaiter().GetResult();
                        }
                        response.data.isSuccess = true;
                        return true;
                    }
                    else
                    {
                        string msg = $"request to {request.RequestUri} unsuccessful {httpResponse.ReasonPhrase} {httpResponse.StatusCode}";
                        Logger.LogToFile(msg);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = $"Exception occured in Report() {ex.Message}";
                Logger.LogToFile(msg);
            }
            return false;
        }

        /// <summary>
        /// Check if an IP is listed, and meets the criteria to be blocked
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>true if blocked</returns>
        public bool Block(string ip)
        {
            Stopwatch stopwatch = null;
            if (_loggingEnabled)
                stopwatch = Stopwatch.StartNew();

            ClearExpiredDB();
            SQLiteConnection connection = DatabaseManager.CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            // Check if cached data exists and is not expired
            if (DatabaseManager.TryGetCachedResponse(command, ip, out response))
            {
                lock (_lock)
                {
                    response.data.isSuccess = true;
                    response.data.isFromCache = true;
                }
                if(_loggingEnabled)
                    Logger.LogToFile($"retreived cached data for {ip} process time {stopwatch.Elapsed.TotalMilliseconds}ms");

                if (response != null && response.data != null && response.data.abuseConfidenceScore < maxConfidenceScore)
                {
                    DatabaseManager.CloseConnection(connection);
                    return false;
                }
                else
                {
                    DatabaseManager.CloseConnection(connection);
                    return true;
                }
            }

            try
            {
                HttpClient client = HttpClientSingleton.Instance;
                using (var request = new HttpRequestMessage(HttpMethod.Get, "report"))
                {
                    request.Headers.Add("Key", apiKey);

                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString.Add("ipAddress", ip);
                    queryString.Add("maxAgeInDays", maxAgeInDays.ToString());
                    request.RequestUri = new Uri(client.BaseAddress, $"check?{queryString}");
                    HttpResponseMessage httpResponse = client.SendAsync(request).GetAwaiter().GetResult();

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        if(_loggingEnabled)
                            Logger.LogToFile($"request to {request.RequestUri} successful {httpResponse.StatusCode} process time {stopwatch.Elapsed.TotalMilliseconds}ms");

                        using (var responseStream = httpResponse.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                        {
                            response = JsonSerializer.Deserialize<AbuseIpDbResponse>(responseStream);
                        }
                        response.data.isSuccess = true;

                        // Cache the response, dont wait on it
#pragma warning disable 4014
                        DatabaseManager.CacheResponseAndClose(command, ip, response);
#pragma warning restore 4014
                        return response.data.abuseConfidenceScore >= maxConfidenceScore;
                    }
                    else
                    {
                        DatabaseManager.CloseConnection(connection);
                        Logger.LogToFile($"request to {request.RequestUri} unsuccessful {httpResponse.ReasonPhrase} {httpResponse.StatusCode}");
                        // Allow the client to connect if API failure code
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DatabaseManager.CloseConnection(connection);
                Logger.LogToFile($"Exception occured in Block() {ex.Message}");
                Logger.LogToFile(ex.InnerException.Message);
                return false;
            }
        }



        public bool IsFromCache()
        {
            lock(_lock)
            {
                return response?.data?.isFromCache ?? false;
            }
        }

        public bool IsSuccess()
        {
            lock (_lock)
            {
                return response?.data?.isSuccess ?? false;
            }
        }

        public string GetIpAddress()
        {
            lock (_lock)
            {
                return response?.data?.ipAddress;
            }
        }

        public bool IsPublic()
        {
            lock (_lock)
            {
                return response?.data?.isPublic ?? false;
            }
        }

        public int GetIpVersion()
        {
            lock (_lock)
            {
                return response?.data?.ipVersion ?? 0;
            }
        }

        public bool IsWhitelisted()
        {
            lock (_lock)
            {
                return response?.data?.isWhitelisted ?? false;
            }
        }

        public int GetAbuseConfidenceScore()
        {
            lock (_lock)
            {
                return response?.data?.abuseConfidenceScore ?? 0;
            }
        }

        public string GetCountryCode()
        {
            lock (_lock)
            {
                return response?.data?.countryCode;
            }
        }

        public string GetCountryName()
        {
            lock (_lock)
            {
                return response?.data?.countryName;
            }
        }

        public string GetUsageType()
        {
            lock (_lock)
            {
                return response?.data?.usageType;
            }
        }

        public string GetISP()
        {
            lock (_lock)
            {
                return response?.data?.isp;
            }
        }

        public string GetDomain()
        {
            lock (_lock)
            {
                return response?.data?.domain;
            }
        }

        public bool IsTor()
        {
            lock (_lock)
            {
                return response?.data?.isTor ?? false;
            }
        }

        public int GetTotalReports()
        {
            lock (_lock)
            {
                return response?.data?.totalReports ?? 0;
            }
        }

        public int GetNumDistinctUsers()
        {
            lock (_lock)
            {
                return response?.data?.numDistinctUsers ?? 0;
            }
        }

        public DateTime GetLastReportedAt()
        {
            lock (_lock)
            {
                return response?.data?.lastReportedAt ?? DateTime.MinValue;
            }
        }

        public void SetMaxAgeInDays(int maxAgeInDays)
        {
            lock (_lock)
            {
                this.maxAgeInDays = maxAgeInDays;
            }
        }

        public void SetApiKey(string apiKey)
        {
            lock (_lock)
            {
                this.apiKey = apiKey;
            }
        }

        public void SetMaxConfidenceScore(int maxConfidenceScore)
        {
            lock (_lock)
            {
                this.maxConfidenceScore = maxConfidenceScore;
            }
        }

        public void VacuumDB()
        {
            SQLiteConnection connection = DatabaseManager.CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            DatabaseManager.DBOperation(command, "VACUUM", true);
            connection.Close();
        }

        public void ClearDB()
        {
            SQLiteConnection connection = DatabaseManager.CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            DatabaseManager.DBOperation(command, "DELETE FROM CachedResponses", true);
            connection.Close();
        }

        public void ClearExpiredDB()
        {
            SQLiteConnection connection = DatabaseManager.CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            DatabaseManager.DBOperation(command, "DELETE FROM CachedResponses WHERE DATETIME(SUBSTR(ExpirationDateTime, 0, 20)) <= DATETIME('NOW')", true);
            connection.Close();
        }
    }

    // Define a custom class to deserialize JSON response
    public class AbuseIpDbResponse
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public bool isSuccess { get; set; }
        public string errorMessage { get; set; }
        public bool isFromCache { get; set; }
        public string ipAddress { get; set; }
        public bool isPublic { get; set; }
        public int ipVersion { get; set; }
        public bool? isWhitelisted { get; set; }
        public int abuseConfidenceScore { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
        public string usageType { get; set; }
        public string isp { get; set; }
        public string domain { get; set; }
        public bool? isTor { get; set; }
        public int? totalReports { get; set; }
        public int? numDistinctUsers { get; set; }
        public DateTime? lastReportedAt { get; set; }
        // Add more properties as needed for other data points
    }
}