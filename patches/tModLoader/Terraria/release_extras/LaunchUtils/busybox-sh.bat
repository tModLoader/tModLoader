for /f "tokens=4-5 delims=. " %%i in ('ver') do (
	set WINDOWS_MAJOR=%%i
	set WINDOWS_MINOR=%%j
)

:: busybox64.exe won't launch on 32 bit OS, so we need to do this check here so we have at least some output other than the window closing immediately. Adapted from https://stackoverflow.com/a/24590583
reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set OS=32BIT || set OS=64BIT
if %OS%==32BIT echo This is a 32 bit operating system, tModLoader requires a 64 bit operating system to launch && pause && Exit /b

start "" "LaunchUtils/busybox64.exe" bash %*