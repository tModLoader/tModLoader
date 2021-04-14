@echo off

echo Cleaning up output folders...

dotnet clean tModLoader.sln -c Release --nologo -v q

echo.
echo Building Release (1/1)
dotnet build tModLoader.sln -c Release --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)