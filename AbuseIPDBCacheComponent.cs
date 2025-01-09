using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;

namespace AbuseIPDBCacheComponent
{
    // Define a COM interface
    [Guid("c1f9d247-e82d-4612-aa5b-ca3dde103a27")]
    public interface IAbuseIPDB
    {
        bool Block(string ip);
        bool Report(string ip, string comment, string categories);
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
        void ClearOldDB();
    }

    // Define a class that implements the COM interface
    [Guid("ECED3D83-2DE5-4C53-8B57-E95C6D90422D")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class AbuseIPDBClient : IAbuseIPDB, IDisposable
    {
        private int maxAgeInDays = 30; // Default value
        private int maxConfidenceScore = 50;
        private string apiKey = "";
        private AbuseIpDbResponse response;

        static AbuseIPDBClient()
        {
            Logger.LogToFile("Init()");
            DatabaseManager.CreateConnection();
            DatabaseManager.InitializeDatabase();
        }

        public void Dispose()
        {
            Logger.LogToFile("Dispose()");
            DatabaseManager.CloseConnection();
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
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                HttpClient client = HttpClientSingleton.Instance;
                using (var request = new HttpRequestMessage(HttpMethod.Post, "report"))
                {
                    request.Headers.Add("Key", apiKey);

                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString.Add("ip", ip);
                    queryString.Add("comment", comment);
                    queryString.Add("categories", categories);

                    request.RequestUri = new Uri(client.BaseAddress, $"report?{queryString}");

                    HttpResponseMessage httpResponse = client.SendAsync(request).Result;

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var content = httpResponse.Content.ReadAsStringAsync().Result;
                        response = Newtonsoft.Json.JsonConvert.DeserializeObject<AbuseIpDbResponse>(content);
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
            // Check if cached data exists and is not expired
            if (DatabaseManager.TryGetCachedResponse(ip, out response))
            {
                response.data.isSuccess = true;
                response.data.isFromCache = true;

                if (response != null && response.data != null && response.data.abuseConfidenceScore < maxConfidenceScore)
                    return false;
                else
                    return true;
            }

            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                HttpClient client = HttpClientSingleton.Instance;
                using (var request = new HttpRequestMessage(HttpMethod.Get, "report"))
                {
                    request.Headers.Add("Key", apiKey);

                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString.Add("ipAddress", ip);
                    queryString.Add("maxAgeInDays", maxAgeInDays.ToString());
                    request.RequestUri = new Uri(client.BaseAddress, $"check?{queryString}");
                    HttpResponseMessage httpResponse = client.SendAsync(request).Result;

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var content = httpResponse.Content.ReadAsStringAsync().Result;
                        response = Newtonsoft.Json.JsonConvert.DeserializeObject<AbuseIpDbResponse>(content);
                        response.data.isSuccess = true;

                        // Cache the response
#pragma warning disable 4014
                        DatabaseManager.CacheResponse(ip, response);
#pragma warning restore 4014

                        return response.data.abuseConfidenceScore >= maxConfidenceScore;
                    }
                    else
                    {
                        string msg = $"request to {request.RequestUri} unsuccessful {httpResponse.ReasonPhrase} {httpResponse.StatusCode}";
                        Logger.LogToFile(msg);
                        // Allow the client to connect if API failure code
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile($"Exception occured in Block() {ex.Message}");
                return false;
            }
        }



        public bool IsFromCache()
        {
            return response?.data?.isFromCache ?? false;
        }

        public bool IsSuccess()
        {
            return response?.data?.isSuccess ?? false;
        }

        public string GetIpAddress()
        {
            return response?.data?.ipAddress;
        }

        public bool IsPublic()
        {
            return response?.data?.isPublic ?? false;
        }

        public int GetIpVersion()
        {
            return response?.data?.ipVersion ?? 0;
        }

        public bool IsWhitelisted()
        {
            return response?.data?.isWhitelisted ?? false;
        }

        public int GetAbuseConfidenceScore()
        {
            return response?.data?.abuseConfidenceScore ?? 0;
        }

        public string GetCountryCode()
        {
            return response?.data?.countryCode;
        }

        public string GetCountryName()
        {
            return response?.data?.countryName;
        }

        public string GetUsageType()
        {
            return response?.data?.usageType;
        }

        public string GetISP()
        {
            return response?.data?.isp;
        }

        public string GetDomain()
        {
            return response?.data?.domain;
        }

        public bool IsTor()
        {
            return response?.data?.isTor ?? false;
        }

        public int GetTotalReports()
        {
            return response?.data?.totalReports ?? 0;
        }

        public int GetNumDistinctUsers()
        {
            return response?.data?.numDistinctUsers ?? 0;
        }

        public DateTime GetLastReportedAt()
        {
            return response?.data?.lastReportedAt ?? DateTime.MinValue;
        }

        public void SetMaxAgeInDays(int maxAgeInDays)
        {
            this.maxAgeInDays = maxAgeInDays;
        }

        public void SetApiKey(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public void SetMaxConfidenceScore(int maxConfidenceScore)
        {
            this.maxConfidenceScore = maxConfidenceScore;
        }

        public void VacuumDB()
        {
            DatabaseManager.DBOperation("VACUUM", true);
        }

        public void ClearDB()
        {
            DatabaseManager.DBOperation("DELETE FROM CachedResponses", true);
        }

        public void ClearOldDB()
        {
            DatabaseManager.DBOperation("DELETE FROM CachedResponses WHERE DATETIME(SUBSTR(ExpirationDateTime, 0, 20)) <= DATETIME('NOW', '-1 day')", true);
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

    // Helper class for COM registration
    public static class RegistrationHelper
    {
        public static void RegisterAssembly(System.Reflection.Assembly assembly, AssemblyRegistrationFlags flags)
        {
            RegistrationServices regsrv = new RegistrationServices();
            regsrv.RegisterAssembly(assembly, flags);
        }

        public static void UnregisterAssembly(System.Reflection.Assembly assembly)
        {
            RegistrationServices regsrv = new RegistrationServices();
            regsrv.UnregisterAssembly(assembly);
        }
    }
}