@echo off

dotnet clean tModLoader.sln -c Debug --nologo -v q

echo Building Debug (1/1)
dotnet build tModLoader.sln -c Debug --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)
