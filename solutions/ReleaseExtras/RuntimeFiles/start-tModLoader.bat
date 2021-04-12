@echo off
cd /D "%~dp0"
call InstallNetFramework.bat
start NetFramework\dotnet\5.0.0\dotnet.exe tModLoader.dll