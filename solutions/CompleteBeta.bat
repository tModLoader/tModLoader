:: After Pulling, Patching, and making sure the version number is changed in src, this bat will compile and create zips for all release.
:: It will also create a zip for ExampleMod

@ECHO off
:: Compile/Build exe 
echo "Building Beta"
set tModLoaderVersion=v0.11.9.0
call buildDebug.bat

set destinationFolder=.\tModLoader %tModLoaderVersion% Beta
@IF %ERRORLEVEL% NEQ 0 (
	pause
	EXIT /B %ERRORLEVEL%
)

:: Folder for release
mkdir "%destinationFolder%"

:: Temp Folders
set rls=%destinationFolder%\tModLoader %tModLoaderVersion%
mkdir "%rls%"

set steam=%destinationFolder%\tModLoaderSteam %tModLoaderVersion%
mkdir "%steam%"

:: Generic release
robocopy /S ReleaseExtras\Content "%rls%\Content"
robocopy /S ..\src\tModLoader\Terraria\bin\Debug\net5.0 "%rls%"
robocopy /S ..\src\tModLoader\Terraria\bin\ServerDebug\net5.0 "%rls%"
robocopy /S ReleaseExtras\RuntimeFiles "%rls%"
del "%rls%\tModLoaderServerDebug.exe"
del "%rls%\tModLoaderDebug.exe"

call python ZipAndMakeExecutable.py "%rls%" "%rls%.zip"
call python ZipAndMakeExecutable.py "%rls%" "%rls%.tar.gz"

:: Steam release
robocopy /S "%rls%" "%steam%"
robocopy /S ReleaseExtras\SteamFiles "%steam%"

call python ZipAndMakeExecutable.py "%steam%" "%steam%.zip"
call python ZipAndMakeExecutable.py "%steam%" "%steam%.tar.gz"

:: CleanUp, Delete temp Folders
:: rmdir "%rls%" /S /Q
:: rmdir "%steam%" /S /Q

:: ExampleMod.zip (TODO, other parts of ExampleMod release)
::rmdir ..\ExampleMod\bin /S /Q
::rmdir ..\ExampleMod\obj /S /Q
::rmdir ..\ExampleMod\.vs /S /Q
::call python ZipAndMakeExecutable.py "..\ExampleMod" "%destinationFolder%\ExampleMod %tModLoaderVersion%.zip" ExampleMod\

echo(
echo(
echo(
echo tModLoader %tModLoaderVersion% ready to release.
echo Upload the 2ish zip files to github/discord.
echo(
echo(
pause
