:: After Pulling, Patching, and making sure the version number is changed in src, this bat will compile and create zips for all release.
:: It will also create a zip for ExampleMod

@ECHO off
:: Compile/Build exe 
echo "Building Debug"
set tModLoaderVersion=v0.11.9.0
call buildDebug.bat

set destinationFolder=.\tModLoader %tModLoaderVersion% Debug
If "%1"=="skipzip" (
	set destinationFolder=.\Debug
)
@IF %ERRORLEVEL% NEQ 0 (
	pause
	EXIT /B %ERRORLEVEL%
)

:: Folder for release
mkdir "%destinationFolder%"

:: Temp Folders
set shared=%destinationFolder%\Shared
set win=%destinationFolder%\tModLoader %tModLoaderVersion%

mkdir "%shared%"

:: Generic debug release
robocopy /S ..\src\tModLoader\Terraria\bin\Debug\net5.0 "%shared%"
del "%shared%\tModLoader.exe"
rmdir /S /Q "%shared%\ref"
robocopy /S ReleaseExtras\RuntimeFiles "%shared%"
robocopy /S ReleaseExtras\SteamFiles "%shared%"
copy ReleaseExtras\tModLoader.png "%shared%\Libraries\Native\tModLoader.png"

If "%1"=="skipzip" (
	echo Skipping zipping.
) Else (
	:: Windows
	call python ZipAndMakeExecutable.py "%shared%" "%win%.zip"

	:: CleanUp, Delete temp Folders
	:: rmdir "%shared%" /S /Q

	:: ExampleMod.zip (TODO, other parts of ExampleMod release)
	rmdir ..\ExampleMod\bin /S /Q
	rmdir ..\ExampleMod\obj /S /Q
	rmdir ..\ExampleMod\.vs /S /Q
	call python ZipAndMakeExecutable.py "..\ExampleMod" "%destinationFolder%\ExampleMod %tModLoaderVersion%.zip" ExampleMod\
)


echo(
echo(
echo(
echo tModLoader %tModLoaderVersion% ready to testing.
echo Upload the 2 zip files to github/discord.
echo(
echo(
If NOT "%1"=="skipzip" (
	pause
)
