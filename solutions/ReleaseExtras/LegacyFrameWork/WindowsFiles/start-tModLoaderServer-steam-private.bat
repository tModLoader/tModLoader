@echo off
cls
:start
tModLoaderServer.exe -steam -lobby private -config serverconfig.txt
@echo.
@echo Restarting server...
@echo.
goto start