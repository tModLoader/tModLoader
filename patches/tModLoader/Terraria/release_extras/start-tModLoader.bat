@echo off
cd /D "%~dp0"

start "" "LaunchUtils/busybox64.exe" bash "./LaunchUtils/ScriptCaller.sh" %*
