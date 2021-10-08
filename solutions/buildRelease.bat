@echo off

echo Cleaning up output folders...

dotnet clean tModLoader.sln -c Release --nologo -v q

echo.
echo Building Release (1/1)

If "%BUILDPURPOSE%"=="" (
	dotnet build "../src/tModLoader/Terraria/Terraria.csproj" -c Release --nologo -v q /clp:ErrorsOnly
) Else (
	echo BuildPurpose set to %BUILDPURPOSE%
	dotnet build "../src/tModLoader/Terraria/Terraria.csproj" -c Release --nologo -v q /clp:ErrorsOnly -p:BuildPurpose=%BUILDPURPOSE%
)
@IF %ERRORLEVEL% NEQ 0 (pause && EXIT /B %ERRORLEVEL%)

pause