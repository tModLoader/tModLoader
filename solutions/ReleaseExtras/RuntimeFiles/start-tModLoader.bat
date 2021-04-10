:: Set all the parameters for the install script
set CHANNELSEL=5.0
set VERSIONSEL=5.0.5
set RUNTIMESELECT=dotnet

:: Set the old version so we know what to delete when updating versions of NET
set OLDVERSION=0.0.0

:: install directories
set INSTALLDIR=NET_%VERSIONSEL%
set OLDDIR=NET_%OLDVERSION%

:: Check if the install for our target NET already exists, and install if not
if Not exist %INSTALLDIR%\ (
	echo Installing_NETFramework
	powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -Channel %CHANNELSEL% -InstallDir %INSTALLDIR%\ -Version %VERSIONSEL% -Runtime %RUNTIMESELECT%"
)

:: Check if old install exists and delete if so
if exist %OLDDIR%\ (
	echo RemovingOldInstall
	::TODO the removal of old install. How do I get this to work?
	:: rmdir OLDDIR 
)

:: Run the game
echo NowRunning
dotnet tModLoader.dll