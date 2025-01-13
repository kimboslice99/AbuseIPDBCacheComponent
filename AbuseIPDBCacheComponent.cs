using System;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;
using System.Text.Json;
using System.Diagnostics;
using System.Data.SQLite;

namespace AbuseIPDBCacheComponent
{
    [Guid("ECED3D83-2DE5-4C53-8B57-E95C6D90422D")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class AbuseIPDBClient
    {
        private AbuseIpDbResponse _response;
        private string _apiKey;
        private int? _minConfidenceScore = null;
        private int? _maxAge = null;

        static AbuseIPDBClient()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Config.MyResolveEventHandler;
            SQLiteConnection connection = DatabaseManager.CreateConnectionAndOpen();
            SQLiteCommand command = connection.CreateCommand();
            DatabaseManager.InitializeDatabase(command);
            DatabaseManager.CloseConnection(connection);
        }

        public bool Report(string ip, string categories, string comment = "")
        {
            string localApiKey = string.IsNullOrEmpty(_apiKey) ? Config.ApiKey : _apiKey;
            if (string.IsNullOrEmpty(localApiKey))
            {
                Logger.LogToFile("Api key not set");
                return false;
            }
            try
            {
                Stopwatch stopwatch = null;
                if (Config.LoggingEnabled)
                    stopwatch = Stopwatch.StartNew();
                HttpClient client = HttpClientSingleton.Instance;
                comment = string.IsNullOrEmpty(comment) ? $"{DateTime.Now}" : comment;
                using (var request = new HttpRequestMessage(HttpMethod.Post, "report"))
                {
                    request.Headers.Add("Key", localApiKey);

                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString.Add("ip", ip);
                    queryString.Add("comment", comment);
                    queryString.Add("categories", categories);

                    request.RequestUri = new Uri(client.BaseAddress, $"report?{queryString}");

                    HttpResponseMessage httpResponse = client.SendAsync(request).GetAwaiter().GetResult();

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        if (Config.LoggingEnabled)
                            Logger.LogToFile($"request to {request.RequestUri} successful {httpResponse.StatusCode} process time {stopwatch.Elapsed.TotalMilliseconds}ms");

                        using (var responseStream = httpResponse.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                        {
                            _response = JsonSerializer.DeserializeAsync<AbuseIpDbResponse>(responseStream).GetAwaiter().GetResult();
                        }
                        _response.data.isSuccess = true;
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
                string msg = $"Exception occurred in Report() {ex.Message}";
                Logger.LogToFile(msg);
            }
            return false;
        }

        public bool Block(string ip)
        {
            int localMinConfidenceScore;
            int localMaxAge;
            string localApiKey;
            localMinConfidenceScore = _minConfidenceScore == null ? Config.MinConfidenceScore : (int)_minConfidenceScore;
            localMaxAge = _maxAge == null ? Config.MaxAgeInDays : (int)_maxAge;
            localApiKey = string.IsNullOrEmpty(_apiKey) ? Config.ApiKey : _apiKey;
            if (string.IsNullOrEmpty(localApiKey))
            {
                Logger.LogToFile("Api key not set");
                return false;
            }
            if (localMaxAge > 365 || localMaxAge < 1)
            {
                Logger.LogToFile("MaxAgeInDays out of valid range 1-365");
                return false;
            }
            if (localMinConfidenceScore < 1 || localMinConfidenceScore > 100)
            {
                Logger.LogToFile("MinConfidenceScore out of valid range 1-100");
                return false;
            }

            Stopwatch stopwatch = null;
            if (Config.LoggingEnabled)
                stopwatch = Stopwatch.StartNew();

            SQLiteConnection connection = DatabaseManager.CreateConnectionAndOpen();
            SQLiteCommand command = connection.CreateCommand();
            ClearExpiredDB(command);

            if (DatabaseManager.TryGetCachedResponse(command, ip, out _response))
            {
                _response.data.isSuccess = true;
                _response.data.isFromCache = true;
                if (Config.LoggingEnabled)
                    Logger.LogToFile($"retrieved cached data for {ip} score {_response.data.abuseConfidenceScore} expires {_response.data.expirationDateTime} process time {stopwatch.Elapsed.TotalMilliseconds}ms");

                if (_response != null && _response.data != null && _response.data.abuseConfidenceScore < localMinConfidenceScore)
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
                    request.Headers.Add("Key", localApiKey);

                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString.Add("ipAddress", ip);
                    queryString.Add("maxAgeInDays", Convert.ToString(localMaxAge));
                    request.RequestUri = new Uri(client.BaseAddress, $"check?{queryString}");
                    HttpResponseMessage httpResponse = client.SendAsync(request).GetAwaiter().GetResult();

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        if (Config.LoggingEnabled)
                            Logger.LogToFile($"request to {request.RequestUri} successful {httpResponse.StatusCode} process time {stopwatch.Elapsed.TotalMilliseconds}ms");

                        using (var responseStream = httpResponse.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                        {
                            _response = JsonSerializer.Deserialize<AbuseIpDbResponse>(responseStream);
                        }
                        _response.data.isSuccess = true;
                        DatabaseManager.CacheResponseAndClose(command, ip, _response);
                        return _response.data.abuseConfidenceScore >= localMinConfidenceScore;
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
                Logger.LogToFile($"Exception occurred in Block() {ex.Message}");
                Logger.LogToFile(ex.InnerException.Message);
                return false;
            }
        }

        public bool IsFromCache()
        {
            return _response?.data?.isFromCache ?? false;
        }

        public bool IsSuccess()
        {
            return _response?.data?.isSuccess ?? false;
        }

        public string GetIpAddress()
        {
            return _response?.data?.ipAddress;
        }

        public bool IsPublic()
        {
            return _response?.data?.isPublic ?? false;
        }

        public int GetIpVersion()
        {
            return _response?.data?.ipVersion ?? 0;
        }

        public bool IsWhitelisted()
        {
            return _response?.data?.isWhitelisted ?? false;
        }

        public int GetAbuseConfidenceScore()
        {
            return _response?.data?.abuseConfidenceScore ?? 0;
        }

        public string GetCountryCode()
        {
            return _response?.data?.countryCode;
        }

        public string GetCountryName()
        {
            return _response?.data?.countryName;
        }

        public string GetUsageType()
        {
            return _response?.data?.usageType;
        }

        public string GetISP()
        {
            return _response?.data?.isp;
        }

        public string GetDomain()
        {
            return _response?.data?.domain;
        }

        public bool IsTor()
        {
            return _response?.data?.isTor ?? false;
        }

        public int GetTotalReports()
        {
            return _response?.data?.totalReports ?? 0;
        }

        public int GetNumDistinctUsers()
        {
            return _response?.data?.numDistinctUsers ?? 0;
        }

        public DateTime GetLastReportedAt()
        {
            return _response?.data?.lastReportedAt ?? DateTime.MinValue;
        }

        public void SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void SetMinConfidenceScore(int minConfidenceScore)
        {
            _minConfidenceScore = minConfidenceScore;
        }

        public void SetMaxAge(int maxAge)
        {
            _maxAge = maxAge;
        }

        public void VacuumDB()
        {
            SQLiteConnection connection = DatabaseManager.CreateConnectionAndOpen();
            SQLiteCommand command = connection.CreateCommand();
            DatabaseManager.DBOperation(command, "VACUUM");
            DatabaseManager.CloseConnection(connection);
        }

        public void ClearDB()
        {
            SQLiteConnection connection = DatabaseManager.CreateConnectionAndOpen();
            SQLiteCommand command = connection.CreateCommand();
            DatabaseManager.DBOperation(command, "DELETE FROM CachedResponses");
            DatabaseManager.CloseConnection(connection);
        }

        private void ClearExpiredDB(SQLiteCommand command)
        {
            DatabaseManager.DBOperation(command, "DELETE FROM CachedResponses WHERE DATETIME(SUBSTR(ExpirationDateTime, 0, 20)) <= DATETIME('NOW')");
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
        public DateTime? expirationDateTime { get; set; }
        // Add more properties as needed for other data points
    }
}