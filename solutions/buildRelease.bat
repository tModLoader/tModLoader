@echo off

echo Cleaning up output folders...

dotnet clean tModLoader.sln -c Release --nologo -v q
dotnet nuget locals all -c

echo.
echo Building Release (1/1)

If "%BUILDPURPOSE%"=="" (
	dotnet build tModLoader.sln -c Release --nologo -v q /clp:ErrorsOnly
) Else (
	echo BuildPurpose set to %BUILDPURPOSE%
	dotnet build tModLoader.sln -c Release --nologo -v q /clp:ErrorsOnly -p:BuildPurpose=%BUILDPURPOSE%
)
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

pause