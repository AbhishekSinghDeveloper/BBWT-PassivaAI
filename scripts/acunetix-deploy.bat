@echo off
echo ====== Deploy ======
set SHA=%1
mkdir %SHA%
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('bbwt3-acunetix.zip', '%SHA%'); }"
copy /Y "C:\inetpub\bbwt3\appsettings.acunetix.json" "%SHA%"
C:/Windows/System32/inetsrv/appcmd.exe stop site /site.name:bbwt3-acunetix
mkdir logs
move "C:\inetpub\bbwt3\logs\*" "logs"
del /f /s /q "C:\inetpub\bbwt3"
pushd "%SHA%"
xcopy /E /Y "*" "C:\inetpub\bbwt3\"
popd
move "logs\*" "C:\inetpub\bbwt3\logs"
rmdir /s /q "%SHA%"
del /F /Q bbwt3-acunetix.zip
icacls "C:\inetpub\bbwt3" /grant "IIS APPPOOL\bbwt3-acunetix:(OI)(CI)F" /T
C:\Windows\System32\inetsrv\appcmd.exe start site /site.name:bbwt3-acunetix
rmdir logs
(goto) 2>nul & del "%~f0"