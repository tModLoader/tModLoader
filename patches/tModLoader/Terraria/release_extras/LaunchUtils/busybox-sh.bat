for /f "tokens=4-5 delims=. " %%i in ('ver') do (
	set WINDOWS_MAJOR=%%i
	set WINDOWS_MINOR=%%j
)

start "" "LaunchUtils/busybox64.exe" bash %*