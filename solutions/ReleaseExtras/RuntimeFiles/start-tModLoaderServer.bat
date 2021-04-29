@echo off
cd /D "%~dp0"

set Args=-server -config serverconfig.txt

set /P steam=Use Steam Server (y)/(n) steam:
if %steam%==y ( set Args=%Args% -steam )
if NOT %steam%==y (call InstallNetFramework.bat )
if NOT %steam%==y (exit )

set /p lobby=Select Lobby Type (f)riends/(p)rivate lobby:
if NOT %lobby%=="p" ( set Args=%Args% -lobby friends )
if %lobby%=="f" ( set Args=%Args% -lobby private )

call InstallNetFramework.bat