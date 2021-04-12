:: After Pulling, Patching, and making sure the version number is changed in src, this bat will compile and create zips for all release.
:: It will also create a zip for ExampleMod

@ECHO off
:: Compile/Build exe 
echo "Building Release"
set tModLoaderVersion=v0.11.9.0
call buildRelease.bat

set destinationFolder=.\tModLoader %tModLoaderVersion% Release
@IF %ERRORLEVEL% NEQ 0 (
	pause
	EXIT /B %ERRORLEVEL%
)

:: Folder for release
mkdir "%destinationFolder%"

:: Generic release
robocopy /S ..\src\tModLoader\Terraria\bin\Release\net5.0 "%destinationFolder%"
robocopy /S ..\src\tModLoader\Terraria\bin\ServerRelease\net5.0 "%destinationFolder%"
del "%destinationFolder%\tModLoaderServer.exe"
del "%destinationFolder%\tModLoader.exe"
rmdir /S /Q "%destinationFolder%\ref"
robocopy /S ReleaseExtras\RuntimeFiles "%destinationFolder%"
robocopy /S ReleaseExtras\SteamFiles "%destinationFolder%"

call python ZipAndMakeExecutable.py "%destinationFolder%" "%destinationFolder%.zip"
call python ZipAndMakeExecutable.py "%destinationFolder%" "%destinationFolder%.tar.gz"

:: CleanUp, Delete temp Folders
:: rmdir "%destinationFolder%" /S /Q

:: ExampleMod.zip (TODO, other parts of ExampleMod release)
::rmdir ..\ExampleMod\bin /S /Q
::rmdir ..\ExampleMod\obj /S /Q
::rmdir ..\ExampleMod\.vs /S /Q
::call python ZipAndMakeExecutable.py "..\ExampleMod" "%destinationFolder%\ExampleMod %tModLoaderVersion%.zip" ExampleMod\

echo(
echo(
echo(
echo tModLoader %tModLoaderVersion% ready to release.
echo Upload the 1ish zip files to github/discord.
echo(
echo(
pause
