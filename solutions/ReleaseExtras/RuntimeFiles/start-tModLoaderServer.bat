@echo off
cd /D "%~dp0"

call InstallNetFramework.bat
cls

set Server=tModLoader.dll -server
set net=dotnet\5.0.0\dotnet.exe

set /P steam=Use Steam Server (y)/(n) steam:

if Not %steam%==y (cls)
if Not %steam%==y (start %net% %Server% -config serverconfig.txt)
if Not %steam%==y (exit)

set /p lobby=Select Lobby Type (f)riends/(p)rivate lobby:
cls

if %lobby%=="f" (start %net% %Server% -steam -lobby friends -config serverconfig.txt)
if %lobby%=="f" (exit)

start %net% %Server% -steam -lobby private -config serverconfig.txt