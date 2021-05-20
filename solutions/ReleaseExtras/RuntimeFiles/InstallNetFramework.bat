::Author: Solxanich
:: Created for tModLoader deployment. 
@echo off
cd /D "%~dp0"
set LOGFILE=LaunchLogs\install.log
if Not exist LaunchLogs (mkdir LaunchLogs)
echo Verifying dotnet....
echo This may take a few minutes on first run.

REM Read file "tModLoader.runtimeconfig.json" into variable string, removing line breaks.
set string=
for /f "delims=" %%x in (tModLoader.runtimeconfig.json) do set "string=!string!%%x"

REM Remove quotes
set string=%string:"=%
REM Remove braces
set "string=%string:~2,-2%"
REM Change colon+space by equal-sign
set "string=%string:: ==%"
REM Separate parts at comma into individual assignments
set "%string:, =" & set "%"
REM Cleanup version assignment
set version=%version:}=%
set "version=%version: =%"

REM Set all the parameters for the install script
set CHANNELSEL=%version:~0,3%
set VERSIONSEL=%version%
set RUNTIMESELECT=dotnet

REM install directories
set INSTALLDIR=dotnet\%VERSIONSEL%

REM Skip install check if runtime.log exists due to install having previously failed.
if Not exist LaunchLogs\runtime.log (
	REM Check if the install for our target NET already exists, and install if not
	if Not exist %INSTALLDIR%\dotnet.exe  (
		echo Logging to LaunchLogs\install.log
		call :LOG 1> %LOGFILE% 2>&1
	)
)

exit /B

:LOG
echo RemovingOldInstalls
if exist NetFramework (
	echo Removing NetFramework
	rmdir /S /Q NetFramework\
)
for /d %%i in (dotnet/*) do (
	echo Removing dotnet/%%i
	rmdir /S /Q "dotnet/%%i"
)
echo Installing_NewFramework
powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -Channel %CHANNELSEL% -InstallDir %INSTALLDIR%\ -Version %VERSIONSEL% -Runtime %RUNTIMESELECT%"

