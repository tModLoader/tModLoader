@echo off
cd /D "%~dp0"

echo "%PROCESSOR_ARCHITECTURE%"
if "%PROCESSOR_ARCHITECTURE%"=="x86" (
	start "" "LaunchUtils/busybox.exe" bash "./LaunchUtils/ScriptCaller.sh" %*
)
else (
	start "" "LaunchUtils/busybox64.exe" bash "./LaunchUtils/ScriptCaller.sh" %*
)