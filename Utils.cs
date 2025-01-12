using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Configuration;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AbuseIPDBCacheComponent
{
    public class Logger
    {
        public static void LogToFile(string message)
        {
            if (!Config.LoggingEnabled)
                return;
            try {
                string dllPath = Assembly.GetExecutingAssembly().Location;
                string dllDirectory = Path.GetDirectoryName(dllPath);
                string path = Path.Combine(dllDirectory, "log.txt");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                } 
            }
            catch
            {
                // do nothing if we dont have permission or some other error
            }
        }
    }

    public static class Config
    {
        public static Configuration AssemblyConfig => ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

        public static bool LoggingEnabled => Convert.ToBoolean(AssemblyConfig.AppSettings.Settings["EnableLogging"]?.Value);

        public static string ApiKey => AssemblyConfig.AppSettings.Settings["ApiKey"]?.Value;

        public static string MaxAgeInDays => AssemblyConfig.AppSettings.Settings["MaxAgeInDays"]?.Value;

        public static int MinConfidenceScore => Convert.ToInt16(AssemblyConfig.AppSettings.Settings["MinConfidenceScore"]?.Value);

        public static SecurityProtocolType Protocol
        {
            get
            {
                string securityProtocolSetting = AssemblyConfig.AppSettings.Settings["HttpClientProtocol"]?.Value;
                switch (securityProtocolSetting)
                {
                    case "TLS1.2":
                        Logger.LogToFile("Found protocol config element, using TLS1.2");
                        return SecurityProtocolType.Tls12;
                    case "TLS1.3":
                        Logger.LogToFile("Found protocol config element, using TLS1.3");
                        return SecurityProtocolType.Tls13;
                    default:
                        Logger.LogToFile("Unable to locate protocol config element, using TLS1.2");
                        return SecurityProtocolType.Tls12;
                }
            }
        }

        public static int CacheTime
        {
            get
            {
                string cacheTimeSetting = AssemblyConfig.AppSettings.Settings["CacheTimeHours"]?.Value;
                int setting = !string.IsNullOrEmpty(cacheTimeSetting) ? Convert.ToInt32(cacheTimeSetting) : 6;
                Logger.LogToFile($"Cache time setting {setting} hours");
                return setting;
            }
        }

        /// <summary>
        /// Force the dependencies to load since bindingRedirects do not work here!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            // Get the simple name of the assembly, excluding version and other metadata
            string assemblyName = new Regex(",.*").Replace(args.Name, string.Empty);

            // Check if the assembly is already loaded
            Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            string dllPath = Assembly.GetExecutingAssembly().Location;
            string dllDirectory = Path.GetDirectoryName(dllPath);
            string assemblyPath = Path.Combine(dllDirectory, $"{assemblyName}.dll");
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            return null;
        }
    }

    public static class HttpClientSingleton
    {
        private static readonly HttpClient instance = CreateHttpClient();
        private static HttpClient CreateHttpClient()
        {
            ServicePointManager.SecurityProtocol = Config.Protocol;
            Logger.LogToFile("Client created");
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.abuseipdb.com/api/v2/")
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AbuseIPDBCacheComponent");
            return client;
        }

        public static HttpClient Instance => instance;
    }
}
