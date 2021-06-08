@echo off

chdir "../src/tModLoader/Terraria"

echo Cleaning up output folders...

dotnet clean -c WindowsRelease --nologo -v q
dotnet clean -c WindowsServerRelease --nologo -v q
dotnet clean -c MacRelease --nologo -v q
dotnet clean -c MacServerRelease --nologo -v q
dotnet clean -c LinuxRelease --nologo -v q
dotnet clean -c LinuxServerRelease --nologo -v q

echo.
echo Building WindowsRelease (1/6)
dotnet build -c WindowsRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.
echo Building WindowsServerRelease (2/6)
dotnet build -c WindowsServerRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.
echo Building MacRelease (3/6)
dotnet build -c MacRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.
echo Building MacServerRelease (4/6)
dotnet build -c MacServerRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.
echo Building LinuxRelease (5/6)
dotnet build -c LinuxRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.
echo Building LinuxServerRelease (6/6)
dotnet build -c LinuxServerRelease --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)
