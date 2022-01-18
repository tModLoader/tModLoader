@echo off
cd /D "%~dp0"
set Args=%*

call LaunchUtils/busybox64.exe LaunchUtils/ScriptCaller.sh true

dotnetV=6.0.0
localNet="/dotnet/%dotnetV%/dotnet.exe"

if exists %localNet% ( 
	start %localNet% tModLoader.dll %Args% 
) else (
	dotnet tModLoader.dll %Args%
)