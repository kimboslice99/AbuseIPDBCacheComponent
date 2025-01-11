# AbuseIPDBCacheComponent
- This component caches any check for 6 hours by default, saving api calls as well as speeding up response time.
- check and report endpoints have been implemented
- GUI configuration tool

## Example report
```JavaScript
var restClient = new ActiveXObject("AbuseIPDBCacheComponent.AbuseIPDBClient");
restClient.SetApiKey("ABUSEIPDB_APIKEY");
var report = restClient.Report("127.0.0.1", "18", "comment");
if(report){
    // report was successful
}
```

## Example check endpoint function
```JavaScript
/**
 * response time on initial lookup depends upon your connection, but its 1-2ms if cached! :)
 * @param {string} strIP - The IP to check
 * @returns {boolean} - Returns true if the IP address is listed and if confidence > max, false otherwise.
 */
function isBlockedByAbuseIPDB(strIP) {
    var restClient = new ActiveXObject("AbuseIPDBCacheComponent.AbuseIPDBClient");
    restClient.SetApiKey(ABUSEIPDB_APIKEY);
    restClient.SetMaxConfidenceScore(60);
    restClient.SetMaxAgeInDays(30);

    var Blocked = restClient.Block(strIP);
    
    var Score = restClient.GetAbuseConfidenceScore();
    
    var fromCache = restClient.IsFromCache();
    // we could extract other data that abuseipdb provides
    // var isTor = restClient.IsTor();
    // var Isp = restClient.GetISP();
    // var totalReports = restClient.GetTotalReports();
    
    DEBUG ? EventLog.Write("INFO: IsFromCache: " + fromCache + " IsSuccess: " + restClient.IsSuccess()) : null ;

    if (Blocked) {
        EventLog.Write("Blocked: " + strIP + " ListedInAbuseIPDB Score: " + Score);
    }

    return Blocked;
}
```

## Example use in hMailServer OnClientConnect()
```JavaScript
if (isBlockedByAbuseIPDB(oClient.IPAddress)) {
    Result.Value = 1;
    return;
}
```

Reduction in API calls after implementation
![api-savings](https://github.com/kimboslice99/AbuseIPDBCacheComponent/blob/main/img/ApiSavings.PNG?raw=true)

Config tool
![config-tool](https://github.com/kimboslice99/AbuseIPDBCacheComponent/blob/main/img/configtool.png?raw=true)

## Build and register for development
- Clone the repo
```cmd
git clone https://github.com/kimboslice99/AbuseIPDBCacheComponent.git
```

- Build the assembly with msbuild.exe
```cmd
msbuild AbuseIPDBCacheComponent.csproj /p:Configuration=Release
```

- Register the assembly with Regasm.exe
```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\Regasm.exe /codebase AbuseIPDBCacheComponent.dll
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Regasm.exe /codebase AbuseIPDBCacheComponent.dll
```