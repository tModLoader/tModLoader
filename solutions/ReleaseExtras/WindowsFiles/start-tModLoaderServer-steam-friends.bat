@echo off
cls
:start
tModLoaderServer.exe -steam -lobby friends -config serverconfig.txt
@echo.
@echo Restarting server...
@echo.
goto start