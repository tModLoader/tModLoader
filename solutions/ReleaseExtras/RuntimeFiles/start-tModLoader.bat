@echo off
cd /D "%~dp0"
set Args=""
setlocal EnableDelayedExpansion

call InstallNetFramework.bat
start dotnet\%VERSIONSEL%\dotnet.exe tModLoader.dll %Args%