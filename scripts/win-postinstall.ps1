if ($PSHOME -like "*SysWOW64*") {
    Write-Warning "Restarting this script under 64-bit Windows PowerShell."
    # Restart this script under 64-bit Windows PowerShell.
    & (Join-Path ($PSHOME -replace "SysWOW64", "SysNative") powershell.exe) -File (Join-Path $PSScriptRoot $MyInvocation.MyCommand) @args
    # Exit 32-bit script, pass error code to deployment agent.
    Exit $LastExitCode
}

$appCmd = (Join-Path -Path $Env:windir -ChildPath "system32\inetsrv\appcmd.exe")
$rgcli = "${env:Programfiles(x86)}\Raygun\RaygunAgent\rgc.exe"
$IISWebSite = "Default Web Site"
$IISAppPool = "DefaultAppPool"
$raygunProfilerx86RootPath = (Join-Path -Path ${env:Programfiles(x86)} -ChildPath "Raygun" | Join-Path -ChildPath "RaygunProfiler")
$raygunProfilerx64RootPath = (Join-Path -Path ${env:Programfiles} -ChildPath "Raygun" | Join-Path -ChildPath "RaygunProfiler")
$raygunProfilerCurrentVersionx86Path = (Join-Path -Path $raygunProfilerx86RootPath (Get-ChildItem $raygunProfilerx86RootPath -Directory | Sort-Object LastWriteTime | Select-Object -Last 1))
$raygunProfilerCurrentVersionx64Path = (Join-Path -Path $raygunProfilerx64RootPath (Get-ChildItem $raygunProfilerx64RootPath -Directory | Sort-Object LastWriteTime | Select-Object -Last 1))

# Stop website
Stop-WebSite -Name $IISWebSite
Stop-WebAppPool -Name $IISAppPool

Set-Location "C:\inetpub\AspNetCoreWebApps\bbwt3"
icacls "C:\inetpub\AspNetCoreWebApps\bbwt3" /grant "IIS APPPOOL\DefaultAppPool:(OI)(CI)F" /T

# Install IIS Websocket role

Install-WindowsFeature -Name "Web-WebSockets"

# Raygun setup
& $rgcli -register __raygun_api_key__
& $rgcli -enable BBWT.Web.dll -type dotnetcore -apikey __raygun_api_key__ -startup 30 -norecycle
& $appCmd set config "$IISWebSite" -section:system.webServer/aspNetCore /+"environmentVariables.[name='CORECLR_ENABLE_PROFILING',value='1']" /commit:site
& $appCmd set config "$IISWebSite" -section:system.webServer/aspNetCore /+"environmentVariables.[name='CORECLR_PROFILER',value='{e2338988-38cc-48cd-a6b6-b441c31f34f1}']" /commit:site
& $appCmd set config "$IISWebSite" -section:system.webServer/aspNetCore /+"environmentVariables.[name='CORECLR_PROFILER_PATH_32',value='$raygunProfilerCurrentVersionx86Path\x86\RaygunProfiler.dll']" /commit:site
& $appCmd set config "$IISWebSite" -section:system.webServer/aspNetCore /+"environmentVariables.[name='CORECLR_PROFILER_PATH_64',value='$raygunProfilerCurrentVersionx64Path\x64\RaygunProfiler.dll']" /commit:site
& $appCmd set config "$IISWebSite" -section:system.webServer/aspNetCore /+"environmentVariables.[name='COMPLUS_ProfAPI_ProfilerCompatibilitySetting',value='EnableV2Profiler']" /commit:site
& $appCmd set config "$IISWebSite" -section:system.webServer/aspNetCore /+"environmentVariables.[name='PROTON_STARTUP_PERIOD',value='60']" /commit:site

# Start website
Set-ItemProperty -Path "IIS:\AppPools\$IISAppPool" -Name managedRuntimeVersion ""
Start-WebSite -Name $IISWebSite
Start-WebAppPool -Name $IISAppPool
