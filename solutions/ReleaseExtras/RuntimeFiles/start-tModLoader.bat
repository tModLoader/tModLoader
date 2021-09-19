@echo off
cd /D "%~dp0"
set Args=%*

setlocal EnableDelayedExpansion 
call InstallNetFramework.bat skip

if "%~1"=="-sysdotnet" (
	echo Launching using System Installed .NET
	pause
	dotnet tModLoader.dll %Args%
) else (
	start dotnet\%VERSIONSEL%\dotnet.exe tModLoader.dll %Args% 
)