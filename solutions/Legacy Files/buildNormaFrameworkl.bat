@echo off

chdir "../src/tModLoader/Terraria"

echo Building WindowsRelease (1/3)
dotnet build -c WindowsRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.

echo Building WindowsServerRelease (2/3)
dotnet build -c WindowsServerRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.

echo Building MacRelease (3/3)
dotnet build -c MacRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)
