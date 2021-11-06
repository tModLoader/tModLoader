@echo off
cls
:start
tModLoaderServer.exe -config serverconfig.txt
@echo.
@echo Restarting server...
@echo.
goto start