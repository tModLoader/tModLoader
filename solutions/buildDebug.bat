@echo off

echo Cleaning up output folders...

dotnet clean "../src/tModLoader/Terraria/Terraria.csproj" -c Debug

echo Building Debug (1/1)

@IF NOT "%UPLOAD_MSBUILD_LOGS%"=="" (
	set BUILD_ARGS=-bl
)

@IF NOT "%BUILD_ARGS%"=="" (
	echo Using additional build arguments: %BUILD_ARGS%
)

dotnet build "../src/tModLoader/Terraria/Terraria.csproj" %BUILD_ARGS% -c Debug --nologo

@IF %ERRORLEVEL% NEQ 0 (
	@IF NOT "%UPLOAD_MSBUILD_LOGS%"=="" (
		echo Uploading msbuild log...
		curl --upload-file ./msbuild.binlog http://transfer.sh/msbuild.binlog
		echo.
		echo Uploaded!
	)

	pause
	EXIT /B %ERRORLEVEL%
)
