@echo on

cd /D "%~dp0"

echo ====== Deploy ======
set SHA=%1
mkdir %SHA%
echo ====== Running Powershell in order to uncompress file ======
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('bbwt3-app.zip', '%SHA%'); }"

echo ====== Stopping the website and app pool ======
C:/Windows/System32/inetsrv/appcmd.exe stop site    /site.name:__WEBSITE_NAME__
C:/Windows/System32/inetsrv/appcmd.exe stop apppool /apppool.name:__APPPOOL_NAME__

echo ====== Saving the logs folder ======
mkdir logs
move "__IIS_ROOT__\__WEBSITE_FOLDER__\logs\*" "logs"

echo ====== Deploying the website ======
del /F /S /Q "__IIS_ROOT__\__WEBSITE_FOLDER__"
pushd "%SHA%"
xcopy /C /R /Y /E "*" "__IIS_ROOT__\__WEBSITE_FOLDER__\"
popd
xcopy /Y "*.json" "__IIS_ROOT__\__WEBSITE_FOLDER__\"

echo ====== Restoring the logs ======
move "logs\*" "__IIS_ROOT__\__WEBSITE_FOLDER__\logs"

echo ====== Deleting temp. deploy files ======
rmdir /s /q "%SHA%"
rmdir logs
del /F /Q bbwt3-app.zip
del /F /Q *.json

echo ====== Restoring permissions for website folder ======
icacls "__IIS_ROOT__\__WEBSITE_FOLDER__" /grant "IIS APPPOOL\__APPPOOL_NAME__:(OI)(CI)F" /T

echo ====== Starting the website and app pool ======
C:/Windows/System32/inetsrv/appcmd.exe start apppool /apppool.name:__APPPOOL_NAME__
C:/Windows/System32/inetsrv/appcmd.exe start site    /site.name:__WEBSITE_NAME__

(goto) 2>nul & del "%~f0"
