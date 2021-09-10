@echo off

echo Cleaning up output folders...

dotnet clean tModLoader.sln -c Debug --nologo -v q
dotnet nuget locals all -c

echo Building Debug (1/1)
dotnet build tModLoader.sln -c Debug --nologo -v q /clp:ErrorsOnly
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)
