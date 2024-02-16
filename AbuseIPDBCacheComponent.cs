using RestSharp;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AbuseIPDBCacheComponent
{
    // Define a COM interface
    [Guid("c1f9d247-e82d-4612-aa5b-ca3dde103a27")]
    public interface AbuseIPDB
    {
        bool Block(string ip);
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
    }

    // Define a class that implements the COM interface
    [Guid("23456789-2345-2345-2345-234567890ABC")]
    [ClassInterface(ClassInterfaceType.None)]
    public class AbuseIPDBClient : AbuseIPDB
    {
        private int maxAgeInDays = 30; // Default value
        private int maxConfidenceScore = 50;
        private string apiKey = "";
        private AbuseIpDbResponse response;

        // initialize the database, not sure I should call initialization once here or on every Check call?
        // for sake of speed, i figure this would be good
        static AbuseIPDBClient()
        {
            DatabaseManager.InitializeDatabase();
        }


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

            using (RestClient client = new RestClient("https://api.abuseipdb.com/api/v2/"))
            {
                var request = new RestRequest("check", Method.Get);

                request.AddHeader("Key", apiKey);
                request.AddHeader("Accept", "application/json");
                request.AddParameter("ipAddress", ip);
                request.AddParameter("maxAgeInDays", maxAgeInDays);
                var restResponse = client.Execute(request);
                if (restResponse.IsSuccessful && restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<AbuseIpDbResponse>(restResponse.Content);
                    response.data.isSuccess = true;

                    // Cache the response
                    DatabaseManager.CacheResponse(ip, response);

                    if (response.data.abuseConfidenceScore < maxConfidenceScore)
                        return false;
                    else
                        return true;
                }
                else
                {
                    Debug.WriteLine("response not successful " + restResponse.ErrorMessage + " " + restResponse.StatusCode);
                    // Allow the client to connect if api failure code
                    return false;
                }
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
    }

    // Define a custom class to deserialize JSON response
    public class AbuseIpDbResponse
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public bool isSuccess { get; set; }
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