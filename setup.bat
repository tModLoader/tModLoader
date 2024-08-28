@echo off
setlocal enabledelayedexpansion

where git >NUL
if !errorlevel! neq 0 (
	echo git not found on PATH
    pause
    exit /b %errorlevel%
)

set busybox=patches\tModLoader\Terraria\release_extras\LaunchUtils\busybox64.exe
set submoduleupdatemarker=.git\tml-setup-module-init.touch
%busybox% [ .git\index -ot %submoduleupdatemarker% ]
rem a 0 exit code means true, a 1 exit code indicates false, or missing file
if !errorlevel! neq 0 (
	echo Restoring git submodules
	git submodule update --init --recursive
	if !errorlevel! neq 0 (
		pause
		exit /b %errorlevel%
	)
	%busybox% touch %submoduleupdatemarker%
)

where dotnet >NUL
if !errorlevel! neq 0 (
	echo dotnet not found on PATH. Install .NET Core!
    pause
    exit /b %errorlevel%
)

endlocal
If "%1"=="auto" (
	echo building Setup.CLI.csproj
	dotnet build setup/CLI/Setup.CLI.csproj -c Release --output "setup/bin/Release/net8.0" -p:WarningLevel=0 -v q

	if NOT ["%errorlevel%"]==["0"] (
		pause
		exit /b %errorlevel%
	)

	"setup/bin/Release/net8.0/setup-cli.exe" "setup-auto" %2 "steam_build" "--plain-progress"
) Else (
	echo building Setup.GUI.csproj
	dotnet build setup/GUI/Setup.GUI.csproj -c Release --output "setup/bin/Release/net8.0-windows" -p:WarningLevel=0 -v q

	if NOT ["%errorlevel%"]==["0"] (
		pause
		exit /b %errorlevel%
	)

	start "" "setup/bin/Release/net8.0-windows/setup-gui.exe" %*
)