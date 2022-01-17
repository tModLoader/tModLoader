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