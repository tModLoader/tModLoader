@echo off
set BatPath=%~dp0
cd batPath

call InstallNetFramework.bat
cls

set Server=tModLoaderServer.dll

set /P steam=Use Steam Server (y)/(n) steam:

if Not %steam%==y (cls)
if Not %steam%==y (dotnet %Server% -config serverconfig.txt)
if Not %steam%==y (exit)

set /p lobby=Select Lobby Type (f)riends/(p)rivate lobby:
cls

if %lobby%=="f" (dotnet %Server% -steam -lobby friends -config serverconfig.txt)
if %lobby%=="f" (exit)

dotnet %Server% -steam -lobby private -config serverconfig.txt