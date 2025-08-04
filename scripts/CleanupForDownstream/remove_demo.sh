#!/usr/bin/env bash

echo "Preparing project."

cd ../..

dotnet sln BBWT.sln remove modules/BBWM.Demo/BBWM.Demo.csproj
dotnet sln BBWT.sln remove modules/BBWM.Demo.Data/BBWM.Demo.Data.csproj
dotnet sln BBWT.Server.sln remove modules/BBWM.Demo/BBWM.Demo.csproj
dotnet sln BBWT.Server.sln remove modules/BBWM.Demo.Data/BBWM.Demo.Data.csproj

dotnet remove project/BBWT.Server/BBWT.Server.csproj reference modules/BBWM.Demo/BBWM.Demo.csproj
dotnet remove project/BBWT.Server/BBWT.Server.csproj reference modules/BBWM.Demo.Data/BBWM.Demo.Data.csproj

git rm -r "modules/BBWM.Demo"
git rm -r "modules/BBWM.Demo.Data"
git rm -r "project/BBWT.Client/src/app/main/demo"
git rm -r "project/BBWT.Server/data/demo"

rm -rf "modules/BBWM.Demo"
rm -rf "modules/BBWM.Demo.Data"
rm -rf "project/BBWT.Ckuebt/src/app/main/demo"

echo "Done."
