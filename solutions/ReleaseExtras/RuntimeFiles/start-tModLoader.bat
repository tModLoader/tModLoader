@echo off
cd /D "%~dp0"
set Args=""
setlocal EnableDelayedExpansion

call InstallNetFramework.bat
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