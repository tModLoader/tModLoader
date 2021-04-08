@echo off

dotnet clean tModLoader.sln -c Debug --nologo -v q
dotnet clean tModLoader.sln -c ServerDebug --nologo -v q

echo Building Debug (1/2)
dotnet build tModLoader.sln -c Debug --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

echo.

echo Building ServerDebug (2/2)
dotnet build tModLoader.sln -c ServerDebug --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)
