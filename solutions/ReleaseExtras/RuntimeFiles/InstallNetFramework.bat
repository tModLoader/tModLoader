::Author: Solxanich
:: Created for tModLoader deployment. 
@echo off
cd /D "%~dp0"
set LOGFILE=LaunchLogs\install.log

if Not exist LaunchLogs (mkdir LaunchLogs)

echo Verifying installation directory...

set IllegalFiles=avcodec-58.dll avdevice-58.dll avfilter-7.dll avformat-58.dll avutil-56.dll FAudio.dll ffmpeg.exe ffplay.exe ffprobe.exe FNA.dll Ionic.Zip.CF.dll Ionic.Zip.Reduced.dll libjpeg-9.dll libpng16-16.dll libtheorafile.dll libtiff-5.dll libwebp-7.dll log4net.dll MojoShader.dll Mono.Cecil.dll Mono.Cecil.Mdb.dll Mono.Cecil.Pdb.dll MonoMod.RuntimeDetour.dll MonoMod.RuntimeDetour.xml MonoMod.Utils.dll MonoMod.Utils.xml MP3Sharp.dll Newtonsoft.Json.dll NVorbis.dll postproc-55.dll ReLogicFNA.dll SDL2.dll SDL2_image.dll soft_oal.dll Steamworks.NET.64Bit.dll steam_api64.dll swresample-3.dll swscale-5.dll System.Windows.Forms.Mono.dll zlib1.dll
set ColorRed=[91m
set ColorDefault=[0m
set ShownFileDeletionPrompt=false

for %%f in (%IllegalFiles%) do (
	if exist %%f (
		if !ShownFileDeletionPrompt!==false (
			echo.
			echo %ColorRed%It has been detected that your installation directory contains third-party library files%ColorDefault%
			echo %ColorRed%that could potentially break tModLoader. If you have previously installed other tModLoader editions%ColorDefault%
			echo %ColorRed%- we recommend you to redownload them and install them into a folder separate from tModLoader's.%ColorDefault%
			echo %ColorRed%For example: 'steamapps/common/tModLoader64bit' ^(placed next to the 'Terraria' directory.^)%ColorDefault%
			echo.
			set /p DUMMY=Press 'ENTER' to proceed and automatically remove those files...
			
			set ShownFileDeletionPrompt=true
		)
		
		del %%f
	)
)

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

REM Check if the install for our target NET already exists, and install if not
if Not exist %INSTALLDIR%\dotnet.exe  (
	echo Logging to LaunchLogs\install.log
	call :LOG 1> %LOGFILE% 2>&1
)

REM If the install failed, provide a link to get the portable directly, and instructions on where to do with it.
if not exist %INSTALLDIR%\dotnet.exe (
	mkdir %INSTALLDIR%

	echo %ColorRed%It has been detected that your system failed to install the dotnet portables automatically. Will now proceed manually.%ColorDefault%
	start "" https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-%version%-windows-x64-binaries
	echo Now manually downloading the x64 .NET portable. Please find it in the opened browser.
	set /p DUMMY=Press 'ENTER' to proceed to the next step...
	echo Please extract the downloaded Zip file contents in to "%~dp0\%INSTALLDIR%"
	set /p DUMMY=Please press Enter when this step is complete.
	
	:loop
	if not exist %INSTALLDIR%\dotnet.exe (
		set /p DUMMY=%ColorRed%%INSTALLDIR%\dotnet.exe not detected. Please ensure step is complete before continuing with Enter.%ColorDefault%
		goto :loop
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

