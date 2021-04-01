@echo off

echo Cleaning up output folders...

dotnet clean tModLoader.sln -c Release --nologo -v q
dotnet clean tModLoader.sln -c ServerRelease --nologo -v q

echo.
echo Building Release (1/2)
dotnet build tModLoader.sln -c Release --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.
echo Building ServerRelease (2/2)
dotnet build tModLoader.sln -c ServerRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)