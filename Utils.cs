using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;

namespace AbuseIPDBCacheComponent
{
    public class Logger
    {
        public static void LogToFile(string message)
        {
#if DEBUG
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string dllDirectory = Path.GetDirectoryName(dllPath);
            string path = Path.Combine(dllDirectory, "log.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
#endif
        }
    }

    public static class HttpClientSingleton
    {
        private static readonly HttpClient instance = CreateHttpClient();
        private static HttpClient CreateHttpClient()
        {
            Logger.LogToFile("Client created");
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.abuseipdb.com/api/v2/")
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        public static HttpClient Instance => instance;
    }
}
