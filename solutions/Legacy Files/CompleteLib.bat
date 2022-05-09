:: After Pulling, Patching, and making sure the version number is changed in src, this bat will compile and create zips for all release.
:: It will also create a zip for ExampleMod

@ECHO off
:: Read param for target
set target=%~1

:: Compile/Build dll 
echo "Building %target%"
set tModLoaderVersion=v0.11.9.0
call build%target%.bat

set destinationFolder=.\tModLoader %tModLoaderVersion% %target%
If "%2"=="skipzip" (
	set destinationFolder=.\%target%
)
@IF %ERRORLEVEL% NEQ 0 (
	pause
	EXIT /B %ERRORLEVEL%
)

:: Folder for release
mkdir "%destinationFolder%"

:: Temp Folders
set shared=%destinationFolder%\Shared
set win=%destinationFolder%\tModLoader Windows %tModLoaderVersion%

mkdir "%shared%"

:: Generic release
robocopy /S ..\src\tModLoader\Terraria\bin\%target%\net5.0 "%shared%"

If "%2"=="skipzip" (
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
echo tModLoader %tModLoaderVersion% ready to %target%.
echo Upload the 2 zip files to github/discord.
echo(
echo(
If NOT "%2"=="skipzip" (
	pause
)
