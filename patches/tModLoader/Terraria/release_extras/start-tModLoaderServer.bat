@echo off
cd /D "%~dp0"

if "%PROCESSOR_ARCHITECTURE%"=="x86" (
	"LaunchUtils/busybox.exe" bash "./start-tModLoaderServer.sh" %*
)
else (
	"LaunchUtils/busybox64.exe" bash "./start-tModLoaderServer.sh" %*
)