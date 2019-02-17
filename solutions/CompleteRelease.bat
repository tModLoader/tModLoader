:: After Pulling, Patching, and making sure the version number is changed in src, this bat will compile and create zips for all release.
:: It will also create a zip for ExampleMod

@ECHO off
:: Compile/Build exe 
echo "Building Release"
set version=v0.11 Beta 2
call buildRelease.bat

set destinationFolder=.\tModLoader %version% Release
@IF %ERRORLEVEL% NEQ 0 (
	pause
	EXIT /B %ERRORLEVEL%
)
@ECHO on

:: Make up-to-date Installers
cd ..\installer2
call createInstallers.bat
cd ..\solutions

:: Folder for release
mkdir "%destinationFolder%"

:: Temp Folders
set win=%destinationFolder%\tModLoader Windows %version%
set mac=%destinationFolder%\tModLoader Mac %version%
set lnx=%destinationFolder%\tModLoader Linux %version%
set winmc=%destinationFolder%\ModCompile_Windows
set monomc=%destinationFolder%\ModCompile_Mono

mkdir "%win%"
mkdir "%mac%"
mkdir "%lnx%"
mkdir "%winmc%"
mkdir "%monomc%"
mkdir "%mac%\mono"

:: TODO: Automatically create version string file. Or have setup.sln copy it to ReleaseExtras

:: Windows release
copy ..\src\tModLoader\bin\x86\WindowsRelease\Terraria.exe "%win%\Terraria.exe" /y
copy ..\src\tModLoader\bin\x86\WindowsServerRelease\Terraria.exe "%win%\tModLoaderServer.exe" /y
copy ..\installer2\WindowsInstaller.jar "%win%\tModLoaderInstaller.jar" /y
copy ReleaseExtras\README_Windows.txt "%win%\README.txt" /y
copy ReleaseExtras\start-tModLoaderServer.bat "%win%\start-tModLoaderServer.bat" /y
copy ReleaseExtras\start-tModLoaderServer-steam-friends.bat "%win%\start-tModLoaderServer-steam-friends.bat" /y
copy ReleaseExtras\start-tModLoaderServer-steam-private.bat "%win%\start-tModLoaderServer-steam-private.bat" /y

call zipjs.bat zipDirItems -source "%win%" -destination "%win%.zip" -keep yes -force yes

:: Windows ModCompile
copy ..\src\tModLoader\bin\x86\MacRelease\Terraria.exe "%winmc%\tModLoaderMac.exe" /y
copy ..\references\FNA.dll "%winmc%\FNA.dll" /y
copy ..\references\Mono.Cecil.Pdb.dll "%winmc%\Mono.Cecil.Pdb.dll" /y
copy ..\RoslynWrapper\bin\Release\RoslynWrapper.dll "%winmc%\RoslynWrapper.dll" /y
copy ..\RoslynWrapper\bin\Release\System.Reflection.Metadata.dll "%winmc%\System.Reflection.Metadata.dll" /y
copy ..\RoslynWrapper\bin\Release\System.Collections.Immutable.dll "%winmc%\System.Collections.Immutable.dll" /y
copy ..\RoslynWrapper\bin\Release\Microsoft.CodeAnalysis.dll "%winmc%\Microsoft.CodeAnalysis.dll" /y
copy ..\RoslynWrapper\bin\Release\Microsoft.CodeAnalysis.CSharp.dll "%winmc%\Microsoft.CodeAnalysis.CSharp.dll" /y
copy ..\src\tModLoader\bin\x86\WindowsRelease\Terraria.xml "%winmc%\Terraria.xml" /y
copy ReleaseExtras\version "%winmc%\version" /y

call zipjs.bat zipDirItems -source "%winmc%" -destination "%winmc%.zip" -keep yes -force yes

:: Mac release
copy ..\src\tModLoader\bin\x86\MacRelease\Terraria.exe "%mac%\Terraria.exe" /y
copy ..\src\tModLoader\bin\x86\MacServerRelease\Terraria.exe "%mac%\tModLoaderServer.exe" /y
copy ReleaseExtras\tModLoaderServer_Mac "%mac%\tModLoaderServer" /y

copy ..\installer2\MacInstaller.jar "%mac%\tModLoaderInstaller.jar" /y
copy ReleaseExtras\README_Mac.txt "%mac%\README.txt" /y
copy ReleaseExtras\Terraria.exe.config "%mac%\Terraria.exe.config" /y

copy ReleaseExtras\macconfig "%mac%\mono\config" /y

call zipjs.bat zipDirItems -source "%mac%" -destination "%mac%.zip" -keep yes -force yes

:: Linux release
copy ..\src\tModLoader\bin\x86\LinuxRelease\Terraria.exe "%lnx%\Terraria.exe" /y
copy ..\src\tModLoader\bin\x86\LinuxServerRelease\Terraria.exe "%lnx%\tModLoaderServer.exe" /y
copy ReleaseExtras\tModLoaderServer_Linux "%lnx%\tModLoaderServer" /y

copy ..\installer2\LinuxInstaller.jar "%lnx%\tModLoaderInstaller.jar" /y
copy ReleaseExtras\README_Linux.txt "%lnx%\README.txt" /y
copy ReleaseExtras\Terraria.exe.config "%lnx%\Terraria.exe.config" /y

call zipjs.bat zipDirItems -source "%lnx%" -destination "%lnx%.zip" -keep yes -force yes

:: Mono ModCompile
copy ..\src\tModLoader\bin\x86\WindowsRelease\Terraria.exe "%monomc%\tModLoaderWindows.exe" /y
copy ReleaseExtras\Microsoft.Xna.Framework.dll "%monomc%\Microsoft.Xna.Framework.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Game.dll "%monomc%\Microsoft.Xna.Framework.Game.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Graphics.dll "%monomc%\Microsoft.Xna.Framework.Graphics.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Xact.dll "%monomc%\Microsoft.Xna.Framework.Xact.dll" /y
copy ..\src\tModLoader\bin\x86\WindowsRelease\Terraria.xml "%monomc%\Terraria.xml" /y
copy ReleaseExtras\version "%monomc%\version" /y

call zipjs.bat zipDirItems -source "%monomc%" -destination "%monomc%.zip" -keep yes -force yes

:: CleanUp, Delete temp Folders
rmdir "%win%" /S /Q
rmdir "%mac%" /S /Q
rmdir "%lnx%" /S /Q
rmdir "%winmc%" /S /Q
rmdir "%monomc%" /S /Q

:: Copy to public DropBox Folder
::copy "%win%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\tModLoader Windows %version%.zip"
::copy "%mac%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\tModLoader Mac %version%.zip"
::copy "%lnx%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\tModLoader Linux %version%.zip"

:: ExampleMod.zip (TODO, other parts of ExampleMod release)
rmdir ..\ExampleMod\bin /S /Q
rmdir ..\ExampleMod\obj /S /Q
:: TODO: ignore .vs folder
call zipjs.bat zipItem -source "..\ExampleMod" -destination "%destinationFolder%\ExampleMod %version%.zip" -keep yes -force yes
::copy "%destinationFolder%\ExampleMod %version%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\"

echo(
echo(
echo(
echo tModLoader %version% ready to release.
echo Upload the 6 zip files to github.
echo(
echo(
pause
