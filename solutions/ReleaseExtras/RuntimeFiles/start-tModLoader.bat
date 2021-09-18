@echo off
cd /D "%~dp0"
set Args=%*

if "%~1"=="-sysdotnet"  ( 
	dotnet tModLoader.dll %Args%
) else (
	call InstallNetFramework.bat
	start dotnet\%VERSIONSEL%\dotnet.exe tModLoader.dll %Args%
)