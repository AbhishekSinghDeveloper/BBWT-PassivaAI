cd ../..
dotnet sln bbwt.sln remove modules\bbwm.demo\BBWM.Demo.csproj
dotnet sln bbwt.sln remove modules\bbwm.demo.data\BBWM.Demo.Data.csproj
dotnet sln bbwt.server.sln remove modules\bbwm.demo\BBWM.Demo.csproj
dotnet sln bbwt.server.sln remove modules\bbwm.demo.data\BBWM.Demo.Data.csproj

dotnet remove project/BBWT.Server/BBWT.Server.csproj reference modules/bbwm.demo/BBWM.Demo.csproj
dotnet remove project/BBWT.Server/BBWT.Server.csproj reference modules/bbwm.demo.data/BBWM.Demo.data.csproj

git rm -r "modules/BBWM.Demo"
git rm -r "modules/BBWM.Demo.Data"
git rm -r "project/BBWT.Client/src/app/main/demo"
git rm -r "project/BBWT.Server/data/demo"

RMDIR /Q /S "modules/BBWM.Demo"
RMDIR /Q /S "modules/BBWM.Demo.Data"
RMDIR /Q /S "project/BBWT.Client/src/app/main/demo"