@echo off

chdir "../src/tModLoader/Terraria"

echo Building WindowsDebug (1/3)
dotnet build -c WindowsDebug --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.

echo Building WindowsServerDebug (2/3)
dotnet build -c WindowsServerDebug --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.

echo Building MacRelease (3/3)
dotnet build -c MacRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)
