@echo off
cd /D "%~dp0"
set Args=%*

"LaunchUtils/busybox64.exe" bash "LaunchUtils/ScriptCaller.sh" true

set dotnetV=6.0.0
set localNet="/dotnet/%dotnetV%/dotnet.exe"

if exists %localNet% ( 
	start %localNet% tModLoader.dll %Args% 
) else (
	dotnet tModLoader.dll %Args%
)