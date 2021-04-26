::Author: Solxanich
:: Created for tModLoader deployment. 
@echo off
cd /D "%~dp0"
set LOGFILE=dotnet\install.log
if Not exist dotnet (mkdir dotnet)
echo Verifying dotnet....
echo Logging to dotnet\install.log
echo This may take a few moments....
call :LOG 1> %LOGFILE% 2>&1
exit /B

:LOG
REM Read file "tModLoader.runtimeconfig.json" into variable string, removing line breaks.
setlocal EnableDelayedExpansion

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
set CHANNELSEL=%version:~0,3%"
set VERSIONSEL=%version%
set RUNTIMESELECT=dotnet

REM install directories
set INSTALLDIR=dotnet\%VERSIONSEL%

REM Check if the install for our target NET already exists, and install if not
if Not exist %INSTALLDIR%\dotnet.exe (
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
)

