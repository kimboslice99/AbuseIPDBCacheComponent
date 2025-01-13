function TestJScript {
    param (
        [string]$IP,
        [bool]$x86 = $false
    )
    $JScript = @"
function isBlockedByAbuseIPDB(strIP) {
    var restClient = new ActiveXObject("AbuseIPDBCacheComponent.AbuseIPDBClient");

    var Blocked = restClient.Block(strIP);
    
    var Score = restClient.GetAbuseConfidenceScore();
    
    var fromCache = restClient.IsFromCache();
    // we could extract other data that abuseipdb provides
    // var isTor = restClient.IsTor();
    // var Isp = restClient.GetISP();
    // var totalReports = restClient.GetTotalReports();

    return Blocked;
}
if(isBlockedByAbuseIPDB('$IP')){
    WScript.Echo('true');
} else {
    WScript.Echo('false');
}
"@
    $tempFile = $Env:Temp + ((Get-Date).ToUniversalTime() - (Get-Date "1970-01-01")).TotalSeconds + '.js'
    Set-Content -Path $tempFile -Value $JScript -Encoding ASCII
    if($x86 -eq $false){
        & C:\Windows\System32\cscript.exe //nologo $tempFile
    } else {
        & C:\Windows\sysWOW64\cscript.exe //nologo $tempFile
    }
    Remove-Item -Path $tempFile -Force
}

function TestWScript {
    param (
        [string]$IP,
        [bool]$x86 = $false
    )
        $WScript = @"
Function IsBlockedByAbuseIPDB(strIP) : ListedInAbuseIPDB = false
	With CreateObject("AbuseIPDBCacheComponent.AbuseIPDBClient")
		On Error Resume Next
		ListedInAbuseIPDB = .Block(strIP)
		If Err.Number <> 0 Then
			WScript.Echo("AbuseIPDB Error: " & Err.Description)
		End If
		On Error Goto 0
	End With
    IsBlockedByAbuseIPDB = ListedInAbuseIPDB
End Function


If IsBlockedByAbuseIPDB("$IP") Then
    WScript.Echo("true")
Else
    WScript.Echo("false")
End If
"@
    $tempFile = $Env:Temp + ((Get-Date).ToUniversalTime() - (Get-Date "1970-01-01")).TotalSeconds + '.vbs'
    Set-Content -Path $tempFile -Value $WScript -Encoding ASCII
    if($x86 -eq $false){
        & C:\Windows\System32\cscript.exe //nologo $tempFile
    } else {
        & C:\Windows\sysWOW64\cscript.exe //nologo $tempFile
    }
    Remove-Item -Path $tempFile -Force
}

function TestPowershell {
    param (
        [string]$IP,
        [bool]$x86 = $false
    )
    $Powershell = @"
function IsBlockedByAbuseIPDB {
    param (
        [string]`$IP
    )
    `$obj = New-Object -ComObject AbuseIPDBCacheComponent.AbuseIPDBClient
    return `$obj.Block("`$IP")
}
`$result = IsBlockedByAbuseIPDB $IP
Write-Host `$result
"@
    $tempFile = $Env:Temp + ((Get-Date).ToUniversalTime() - (Get-Date "1970-01-01")).TotalSeconds + '.ps1'
    Set-Content -Path $tempFile -Value $Powershell -Encoding ASCII
    if($x86 -eq $false){
        & C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe -File $tempFile
    } else {
        & C:\Windows\SysWOW64\WindowsPowerShell\v1.0\Powershell.exe -File $tempFile
    }
    Remove-Item -Path $tempFile -Force
}

function TestPowershell7 {
    param (
        [string]$IP,
        [bool]$x86 = $false
    )
    $Powershell = @"
function IsBlockedByAbuseIPDB {
    param (
        [string]`$IP
    )
    `$obj = New-Object -ComObject AbuseIPDBCacheComponent.AbuseIPDBClient
    return `$obj.Block("`$IP")
}
`$result = IsBlockedByAbuseIPDB $IP
Write-Host `$result
"@
    $tempFile = $Env:Temp + ((Get-Date).ToUniversalTime() - (Get-Date "1970-01-01")).TotalSeconds + ".ps1"
    Set-Content -Path $tempFile -Value $Powershell -Encoding ASCII
    if($x86 -eq $false){
        & pwsh.exe -File $tempFile
    } else {
        & pwh.exe -File $tempFile
    }
    Remove-Item -Path $tempFile -Force
}

$IPToTest = "127.0.0.1"

Write-Host "Testing JScriptx64"
TestJScript $IPToTest
Write-Host "Testing JScriptx86"
TestJScript $IPToTest $true

Write-Host "Testing WScriptx64"
TestWScript $IPToTest
Write-Host "Testing WScriptx86"
TestWScript $IPToTest $true

Write-Host "Testing Powershell 5.1 x64"
TestPowershell $IPToTest
Write-Host "Testing Powershell 5.1 x86"
TestPowershell $IPToTest $true

# Element not found? hmm
Write-Host "Testing Powershell 7 x64"
TestPowershell7 $IPToTest
