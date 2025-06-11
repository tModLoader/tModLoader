for /f "tokens=4-5 delims=. " %%i in ('ver') do (
	set WINDOWS_MAJOR=%%i
	set WINDOWS_MINOR=%%j
)

:: busybox64.exe won't launch on 32 bit OS, so we need to do this check here so we have at least some output other than the window closing immediately. Adapted from https://stackoverflow.com/a/19409344
if /i "%PROCESSOR_ARCHITECTURE%"=="x86" (
	if not defined PROCESSOR_ARCHITEW6432 (
		echo This is a 32 bit operating system, tModLoader requires a 64 bit operating system to launch && pause && Exit /b
	)
)

start "" "LaunchUtils/busybox64.exe" bash %*