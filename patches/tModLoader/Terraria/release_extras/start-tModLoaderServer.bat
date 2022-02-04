@echo off
cd /D "%~dp0"

"LaunchUtils/busybox64.exe" bash "./start-tModLoaderServer.sh" %*
