:: After Pulling, Patching, and making sure the version number is changed in src, this bat will compile and create zips for all release.
:: It will also create a zip for ExampleMod

@ECHO off
:: Compile/Build exe 
echo "Building Release"
set tModLoaderVersion=v0.11.7.2
call buildRelease.bat

set destinationFolder=.\tModLoader %tModLoaderVersion% Release
@IF %ERRORLEVEL% NEQ 0 (
	pause
	EXIT /B %ERRORLEVEL%
)

:: Make up-to-date Installers
::cd ..\installer2
::call createInstallers.bat
::cd ..\solutions

:: Folder for release
mkdir "%destinationFolder%"

:: Temp Folders
set win=%destinationFolder%\tModLoader Windows %tModLoaderVersion%
set mac=%destinationFolder%\tModLoader Mac %tModLoaderVersion%
set macReal=%destinationFolder%\tModLoader Mac %tModLoaderVersion%\tModLoader.app\Contents\MacOS
set lnx=%destinationFolder%\tModLoader Linux %tModLoaderVersion%
set mcfna=%destinationFolder%\ModCompile_FNA
set mcxna=%destinationFolder%\ModCompile_XNA
set pdbs=%destinationFolder%\pdbs
set winsteam=..\..\steamworks_sdk_150\sdk\tools\ContentBuilder\content\Windows
set lnxsteam=..\..\steamworks_sdk_150\sdk\tools\ContentBuilder\content\Linux
set macsteam=..\..\steamworks_sdk_150\sdk\tools\ContentBuilder\content\Mac
set sharedsteam=..\..\steamworks_sdk_150\sdk\tools\ContentBuilder\content\Shared

rmdir /S /Q "%winsteam%"
rmdir /S /Q "%lnxsteam%"
rmdir /S /Q "%macsteam%"
rmdir /S /Q "%sharedsteam%"

mkdir "%win%"
mkdir "%mac%"
mkdir "%lnx%"
mkdir "%mcfna%"
mkdir "%mcxna%"
mkdir "%pdbs%"
mkdir "%winsteam%"
mkdir "%lnxsteam%"
mkdir "%macsteam%"
mkdir "%sharedsteam%"

:: Windows release
robocopy /s ReleaseExtras\Content "%win%\Content"
robocopy /s ReleaseExtras\JourneysEndCompatibilityContent "%win%\Content"
robocopy /s ReleaseExtras\WindowsFiles "%win%"
copy ..\src\tModLoader\bin\WindowsRelease\net45\Terraria.exe "%win%\tModLoader.exe" /y
copy ..\src\tModLoader\bin\WindowsServerRelease\net45\Terraria.exe "%win%\tModLoaderServer.exe" /y
copy ..\src\tModLoader\bin\WindowsRelease\net45\tModLoader.pdb "%win%\tModLoader.pdb" /y
copy ..\src\tModLoader\bin\WindowsServerRelease\net45\tModLoaderServer.pdb "%win%\tModLoaderServer.pdb" /y
::copy ReleaseExtras\README_Windows.txt "%win%\README.txt" /y
::copy ..\installer2\WindowsInstaller.jar "%win%\tModLoaderInstaller.jar" /y

:: Windows Steam
robocopy /s "%win%" "%winsteam%"
del "%win%\steam_api.dll"
del "%win%\CSteamworks.dll"

call python ZipAndMakeExecutable.py "%win%" "%win%.zip"

:: Windows ModCompile
:: TODO: investigate why this isn't working on my machine
:: for /f %%i in ('..\setup\bin\setup --steamdir') do set steamdir=%%i
set steamdir=C:\Program Files (x86)\Steam\steamapps\common\tModLoader
:: Make sure to clear out ModCompile and run Setup Debugging so ModCompile folder is clean from old versions.
copy "%steamdir%\ModCompile" "%mcfna%"
del "%mcfna%"\buildlock 2>nul
copy ..\src\tModLoader\bin\WindowsRelease\net45\tModLoader.xml "%mcfna%" /y
copy ..\src\tModLoader\bin\WindowsRelease\net45\tModLoader.pdb "%mcfna%" /y
copy ..\references\MonoMod.RuntimeDetour.xml "%mcfna%" /y
copy ..\references\MonoMod.Utils.xml "%mcfna%" /y

call python ZipAndMakeExecutable.py "%mcfna%" "%mcfna%.zip"

:: Linux release
robocopy /s ReleaseExtras\LinuxFiles "%lnx%"
robocopy /s ReleaseExtras\LinuxMacSharedFiles "%lnx%"
robocopy /s ReleaseExtras\Content "%lnx%\Content"
robocopy /s ReleaseExtras\JourneysEndCompatibilityContent "%lnx%\Content"
copy ..\src\tModLoader\bin\LinuxRelease\net45\Terraria.exe "%lnx%\tModLoader.exe" /y
copy ..\src\tModLoader\bin\LinuxServerRelease\net45\Terraria.exe "%lnx%\tModLoaderServer.exe" /y
copy ..\src\tModLoader\bin\LinuxRelease\net45\tModLoader.pdb "%lnx%\tModLoader.pdb" /y
copy ..\src\tModLoader\bin\LinuxServerRelease\net45\tModLoaderServer.pdb "%lnx%\tModLoaderServer.pdb" /y
copy ReleaseExtras\tModLoader-mono "%lnx%\tModLoader-mono" /y
copy ReleaseExtras\tModLoader-kick "%lnx%\tModLoader-kick" /y
copy ReleaseExtras\tModLoader-kick "%lnx%\tModLoader" /y
copy ReleaseExtras\tModLoader-kick "%lnx%\tModLoaderServer" /y
::copy ReleaseExtras\README_Linux.txt "%lnx%\README.txt" /y
::copy ..\installer2\LinuxInstaller.jar "%lnx%\tModLoaderInstaller.jar" /y

:: Linux Steam
robocopy /s "%lnx%" "%lnxsteam%"
del "%lnx%\lib\libsteam_api.so"
del "%lnx%\lib64\libsteam_api.so"
del "%lnx%\lib\libCSteamworks.so"
del "%lnx%\lib64\libCSteamworks.so"

call python ZipAndMakeExecutable.py "%lnx%" "%lnx%.tar.gz"
call python ZipAndMakeExecutable.py "%lnx%" "%lnx%.zip"

:: Mac release
robocopy /s ReleaseExtras\MacFiles "%mac%"
robocopy /s ReleaseExtras\LinuxMacSharedFiles "%macReal%"
robocopy /s ReleaseExtras\Content "%macReal%\Content"
robocopy /s ReleaseExtras\JourneysEndCompatibilityContent "%macReal%\Content"
copy ..\src\tModLoader\bin\MacRelease\net45\Terraria.exe "%macReal%\tModLoader.exe" /y
copy ..\src\tModLoader\bin\MacServerRelease\net45\Terraria.exe "%macReal%\tModLoaderServer.exe" /y
copy ..\src\tModLoader\bin\MacRelease\net45\tModLoader.pdb "%macReal%\tModLoader.pdb" /y
copy ..\src\tModLoader\bin\MacServerRelease\net45\tModLoaderServer.pdb "%macReal%\tModLoaderServer.pdb" /y
copy ReleaseExtras\tModLoader-mono "%macReal%\tModLoader-mono" /y
copy ReleaseExtras\tModLoader-kick "%macReal%\tModLoader-kick" /y
copy ReleaseExtras\tModLoader-kick "%macReal%\tModLoader" /y
copy ReleaseExtras\tModLoader-kick "%macReal%\tModLoaderServer" /y
::copy ReleaseExtras\README_Mac.txt "%mac%\README.txt" /y
::copy ..\installer2\MacInstaller.jar "%mac%\tModLoaderInstaller.jar" /y

:: Mac Steam
robocopy /s "%mac%" "%macsteam%"
del "%macReal%\osx\libsteam_api.dylib"
del "%macReal%\osx\CSteamworks"

call python ZipAndMakeExecutable.py "%mac%" "%mac%.zip"

:: Mono ModCompile
copy "%mcfna%" "%mcxna%"
del "%mcxna%\tModLoader.FNA.exe"
del "%mcxna%\FNA.dll"
del "%mcxna%\tModLoader.pdb"
copy ..\src\tModLoader\bin\MacRelease\net45\tModLoader.pdb "%mcxna%\tModLoader_Mac.pdb" /y
copy ..\src\tModLoader\bin\LinuxRelease\net45\tModLoader.pdb "%mcxna%\tModLoader_Linux.pdb" /y
copy ..\src\tModLoader\bin\WindowsRelease\net45\Terraria.exe "%mcxna%\tModLoader.XNA.exe" /y
copy ..\src\tModLoader\bin\WindowsRelease\net45\Microsoft.Xna.Framework.dll "%mcxna%" /y
copy ..\src\tModLoader\bin\WindowsRelease\net45\Microsoft.Xna.Framework.Game.dll "%mcxna%" /y
copy ..\src\tModLoader\bin\WindowsRelease\net45\Microsoft.Xna.Framework.Graphics.dll "%mcxna%" /y
copy ..\src\tModLoader\bin\WindowsRelease\net45\Microsoft.Xna.Framework.Xact.dll "%mcxna%" /y

call python ZipAndMakeExecutable.py "%mcxna%" "%mcxna%.zip"

:: PDB backups
copy ..\src\tModLoader\bin\WindowsRelease\net45\tModLoader.pdb "%pdbs%\WindowsRelease.pdb" /y
copy ..\src\tModLoader\bin\WindowsServerRelease\net45\tModLoaderServer.pdb "%pdbs%\WindowsServerRelease.pdb" /y
copy ..\src\tModLoader\bin\MacRelease\net45\tModLoader.pdb "%pdbs%\MacRelease.pdb" /y
copy ..\src\tModLoader\bin\MacServerRelease\net45\tModLoaderServer.pdb "%pdbs%\MacServerRelease.pdb" /y
copy ..\src\tModLoader\bin\LinuxRelease\net45\tModLoader.pdb "%pdbs%\LinuxRelease.pdb" /y
copy ..\src\tModLoader\bin\LinuxServerRelease\net45\tModLoaderServer.pdb "%pdbs%\LinuxServerRelease.pdb" /y
call python ZipAndMakeExecutable.py "%pdbs%" "%pdbs%.zip"

:: SharedSteam
echo|set /p="1281930" > "%sharedsteam%\steam_appid.txt"

:: Add ModCompile folders to Beta Steam releases
echo.%tModLoaderVersion% | findstr /C:"Beta" 1>nul
if errorlevel 1 (
  echo. Non-Beta
) ELSE (
  echo. Beta release, ModCompile added to Steam release for simplicity
  robocopy /s "%mcfna%" "%winsteam%\ModCompile"
  robocopy /s "%mcxna%" "%lnxsteam%\ModCompile"
  robocopy /s "%mcxna%" "%macsteam%\tModLoader.app\Contents\MacOS\ModCompile"
)

:: CleanUp, Delete temp Folders
rmdir "%win%" /S /Q
rmdir "%mac%" /S /Q
rmdir "%lnx%" /S /Q
rmdir "%mcfna%" /S /Q
rmdir "%mcxna%" /S /Q
rmdir "%pdbs%" /S /Q

:: ExampleMod.zip (TODO, other parts of ExampleMod release)
rmdir ..\ExampleMod\bin /S /Q
rmdir ..\ExampleMod\obj /S /Q
rmdir ..\ExampleMod\.vs /S /Q
call python ZipAndMakeExecutable.py "..\ExampleMod" "%destinationFolder%\ExampleMod %tModLoaderVersion%.zip" ExampleMod\

echo(
echo(
echo(
echo tModLoader %tModLoaderVersion% ready to release.
echo Upload the 6 zip files to github.
echo(
echo(
pause
