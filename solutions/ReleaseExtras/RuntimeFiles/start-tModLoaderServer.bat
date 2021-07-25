@echo off
cd /D "%~dp0"
set Args=-server -config serverconfig.txt
setlocal EnableDelayedExpansion

call InstallNetFramework.bat

set /P steam=Use Steam Server [y]/[n] steam:
if NOT %steam%==y ( goto start )

set Args=%Args% -steam
set /p lobby=Select Lobby Type [f]riends/[p]rivate lobby:
if NOT %lobby%==p ( set Args=%Args% -lobby friends )
if %lobby%==p ( set Args=%Args% -lobby private )

:start
if exist %INSTALLDIR%\dotnet.exe ( 
	start dotnet\%VERSIONSEL%\dotnet.exe tModLoader.dll %Args% 
) else (
	echo Installation of dotnet portable failed. Launching manual installed Net runtimes.
	echo Logs for manual install are located in LaunchLogs\runtime.log
	call :LOG_R 3> LaunchLogs\runtime.log 2>&3
)
exit /B

:LOG_R
echo Attempting Launch..
dotnet tModLoader.dll %Args%