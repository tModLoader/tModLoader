@echo off

where git >NUL
if NOT ["%errorlevel%"]==["0"] (
	echo git not found on PATH
    pause
    exit /b %errorlevel%
)

echo Restoring git submodules
git submodule update --init --recursive
if NOT ["%errorlevel%"]==["0"] (
    pause
    exit /b %errorlevel%
)

where dotnet >NUL
if NOT ["%errorlevel%"]==["0"] (
	echo dotnet not found on PATH. Install .NET Core!
    pause
    exit /b %errorlevel%
)

echo building Setup.CLI.csproj
dotnet build setup/CLI/Setup.CLI.csproj -c Release --output "setup/bin/Release/net8.0" -p:WarningLevel=0 -v q

if NOT ["%errorlevel%"]==["0"] (
	pause
	exit /b %errorlevel%
)

"setup/bin/Release/net8.0/setup-cli.exe" %*