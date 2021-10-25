@echo off
cd /D "%~dp0"
set Args=%*
setlocal EnableDelayedExpansion

call InstallNetFramework.bat
if exist %INSTALLDIR%\dotnet.exe ( 
	start dotnet\%VERSIONSEL%\dotnet.exe tModLoader.dll %Args% 
) else (
	dotnet tModLoader.dll %Args%
)