@echo off
call InstallNetFramework.bat

set /P steam=Use Steam Server (y)/(n) steam:
echo %steam%

if Not %steam%==y (dotnet tModLoaderServer.dll -config serverconfig.txt)
if Not %steam%==y (exit)

set /p lobby=Select Lobby Type (f)riends/(p)rivate lobby:
echo %lobby%

if %lobby%=="f" (dotnet tModLoaderServer.dll -steam -lobby friends -config serverconfig.txt)
if %lobby%=="friends" (exit)

dotnet dotnet tModLoaderServer.dll -steam -lobby private -config serverconfig.txt