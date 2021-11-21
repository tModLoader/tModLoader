@echo off
cd /D "%~dp0"
set Args=%*
setlocal EnableDelayedExpansion

call InstallNetFramework.bat
if exist %INSTALLDIR%\dotnet.exe ( 
	start dotnet\%VERSIONSEL%\dotnet.exe Libraries\TerrariaConnection\MiddleManager.dll
	start dotnet\%VERSIONSEL%\dotnet.exe tModLoader.dll %Args% 
) else (
	dotnet Libraries\TerrariaConnection\MiddleManager.dll
	dotnet tModLoader.dll %Args%
)