:: After Pulling, Patching, and making sure the version number is changed in src, this bat will compile and create zips for all release.
:: It will also create a zip for ExampleMod

set version=v0.10.1
set destinationFolder=.\tModLoader %version% Release

:: Compile/Build exe 
call buildRelease.bat
@IF %ERRORLEVEL% NEQ 0 (
	pause
	EXIT /B %ERRORLEVEL%
)

:: Make up-to-date Installers
cd ..\installer2
call createInstallers.bat
cd ..\solutions

:: Folder for release
mkdir "%destinationFolder%"

:: Temp Folders
mkdir "%destinationFolder%\tModLoader Windows %version%"
mkdir "%destinationFolder%\tModLoader Mac %version%"
mkdir "%destinationFolder%\tModLoader Linux %version%"
mkdir "%destinationFolder%\tModLoader Windows %version%\ModCompile"
mkdir "%destinationFolder%\tModLoader Mac %version%\ModCompile"
mkdir "%destinationFolder%\tModLoader Mac %version%\mono"
mkdir "%destinationFolder%\tModLoader Linux %version%\ModCompile"

:: Windows release
copy ..\src\tModLoader\bin\x86\WindowsRelease\Terraria.exe "%destinationFolder%\tModLoader Windows %version%\Terraria.exe" /y
copy ..\src\tModLoader\bin\x86\WindowsServerRelease\Terraria.exe "%destinationFolder%\tModLoader Windows %version%\tModLoaderServer.exe" /y
:: ModCompile
copy ..\src\tModLoader\bin\x86\MacRelease\Terraria.exe "%destinationFolder%\tModLoader Windows %version%\ModCompile\tModLoaderMac.exe" /y
copy ..\references\FNA.dll "%destinationFolder%\tModLoader Windows %version%\ModCompile\FNA.dll" /y
copy ..\references\Microsoft.CodeAnalysis.dll "%destinationFolder%\tModLoader Windows %version%\ModCompile\Microsoft.CodeAnalysis.dll" /y
copy ..\references\Microsoft.CodeAnalysis.CSharp.dll "%destinationFolder%\tModLoader Windows %version%\ModCompile\Microsoft.CodeAnalysis.CSharp.dll" /y
copy ..\references\Mono.Cecil.Pdb.dll "%destinationFolder%\tModLoader Windows %version%\ModCompile\Mono.Cecil.Pdb.dll" /y
copy ..\references\System.Reflection.Metadata.dll "%destinationFolder%\tModLoader Windows %version%\ModCompile\System.Reflection.Metadata.dll" /y
copy ..\RoslynWrapper\bin\Release\RoslynWrapper.dll "%destinationFolder%\tModLoader Windows %version%\ModCompile\RoslynWrapper.dll" /y

copy ..\installer2\WindowsInstaller.jar "%destinationFolder%\tModLoader Windows %version%\tModLoaderInstaller.jar" /y
copy ReleaseExtras\README_Windows.txt "%destinationFolder%\tModLoader Windows %version%\README.txt" /y
copy ReleaseExtras\start-tModLoaderServer.bat "%destinationFolder%\tModLoader Windows %version%\start-tModLoaderServer.bat" /y
copy ReleaseExtras\start-tModLoaderServer-steam-friends.bat "%destinationFolder%\tModLoader Windows %version%\start-tModLoaderServer-steam-friends.bat" /y
copy ReleaseExtras\start-tModLoaderServer-steam-private.bat "%destinationFolder%\tModLoader Windows %version%\start-tModLoaderServer-steam-private.bat" /y

call zipjs.bat zipDirItems -source "%destinationFolder%\tModLoader Windows %version%" -destination "%destinationFolder%\tModLoader Windows %version%.zip" -keep yes -force yes

:: Mac release
copy ..\src\tModLoader\bin\x86\MacRelease\Terraria.exe "%destinationFolder%\tModLoader Mac %version%\Terraria.exe" /y
copy ..\src\tModLoader\bin\x86\MacServerRelease\Terraria.exe "%destinationFolder%\tModLoader Mac %version%\tModLoaderServer.exe" /y
copy ReleaseExtras\tModLoaderServer_Mac "%destinationFolder%\tModLoader Mac %version%\tModLoaderServer" /y
copy ReleaseExtras\tModLoaderServer.bin.osx "%destinationFolder%\tModLoader Mac %version%\tModLoaderServer.bin.osx" /y
:: ModCompile
copy ..\src\tModLoader\bin\x86\WindowsRelease\Terraria.exe "%destinationFolder%\tModLoader Mac %version%\ModCompile\tModLoaderWindows.exe" /y
copy ReleaseExtras\Microsoft.Xna.Framework.dll "%destinationFolder%\tModLoader Mac %version%\ModCompile\Microsoft.Xna.Framework.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Game.dll "%destinationFolder%\tModLoader Mac %version%\ModCompile\Microsoft.Xna.Framework.Game.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Graphics.dll "%destinationFolder%\tModLoader Mac %version%\ModCompile\Microsoft.Xna.Framework.Graphics.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Xact.dll "%destinationFolder%\tModLoader Mac %version%\ModCompile\Microsoft.Xna.Framework.Xact.dll" /y
:: References
copy ..\references\MP3Sharp.dll "%destinationFolder%\tModLoader Mac %version%\MP3Sharp.dll" /y
copy ..\references\Ionic.Zip.Reduced.dll "%destinationFolder%\tModLoader Mac %version%\Ionic.Zip.Reduced.dll" /y
copy ..\references\Mono.Cecil.dll "%destinationFolder%\tModLoader Mac %version%\Mono.Cecil.dll" /y

copy ..\installer2\MacInstaller.jar "%destinationFolder%\tModLoader Mac %version%\tModLoaderInstaller.jar" /y
copy ReleaseExtras\README_Mac.txt "%destinationFolder%\tModLoader Mac %version%\README.txt" /y
copy ReleaseExtras\Terraria.exe.config "%destinationFolder%\tModLoader Mac %version%\Terraria.exe.config" /y

copy ReleaseExtras\macconfig "%destinationFolder%\tModLoader Mac %version%\mono\config" /y

call zipjs.bat zipDirItems -source "%destinationFolder%\tModLoader Mac %version%" -destination "%destinationFolder%\tModLoader Mac %version%.zip" -keep yes -force yes

:: Linux release
copy ..\src\tModLoader\bin\x86\LinuxRelease\Terraria.exe "%destinationFolder%\tModLoader Linux %version%\Terraria.exe" /y
copy ..\src\tModLoader\bin\x86\LinuxServerRelease\Terraria.exe "%destinationFolder%\tModLoader Linux %version%\tModLoaderServer.exe" /y
copy ReleaseExtras\tModLoaderServer_Linux "%destinationFolder%\tModLoader Linux %version%\tModLoaderServer" /y
copy ReleaseExtras\tModLoaderServer.bin.x86 "%destinationFolder%\tModLoader Linux %version%\tModLoaderServer.bin.x86" /y
copy ReleaseExtras\tModLoaderServer.bin.x86_64 "%destinationFolder%\tModLoader Linux %version%\tModLoaderServer.bin.x86_64" /y
:: ModCompile
copy ..\src\tModLoader\bin\x86\WindowsRelease\Terraria.exe "%destinationFolder%\tModLoader Linux %version%\ModCompile\tModLoaderWindows.exe" /y
copy ReleaseExtras\Microsoft.Xna.Framework.dll "%destinationFolder%\tModLoader Linux %version%\ModCompile\Microsoft.Xna.Framework.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Game.dll "%destinationFolder%\tModLoader Linux %version%\ModCompile\Microsoft.Xna.Framework.Game.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Graphics.dll "%destinationFolder%\tModLoader Linux %version%\ModCompile\Microsoft.Xna.Framework.Graphics.dll" /y
copy ReleaseExtras\Microsoft.Xna.Framework.Xact.dll "%destinationFolder%\tModLoader Linux %version%\ModCompile\Microsoft.Xna.Framework.Xact.dll" /y
:: References
copy ..\references\MP3Sharp.dll "%destinationFolder%\tModLoader Linux %version%\MP3Sharp.dll" /y
copy ..\references\Ionic.Zip.Reduced.dll "%destinationFolder%\tModLoader Linux %version%\Ionic.Zip.Reduced.dll" /y
copy ..\references\Mono.Cecil.dll "%destinationFolder%\tModLoader Linux %version%\Mono.Cecil.dll" /y

copy ..\installer2\LinuxInstaller.jar "%destinationFolder%\tModLoader Linux %version%\tModLoaderInstaller.jar" /y
copy ReleaseExtras\README_Linux.txt "%destinationFolder%\tModLoader Linux %version%\README.txt" /y
copy ReleaseExtras\Terraria.exe.config "%destinationFolder%\tModLoader Linux %version%\Terraria.exe.config" /y

call zipjs.bat zipDirItems -source "%destinationFolder%\tModLoader Linux %version%" -destination "%destinationFolder%\tModLoader Linux %version%.zip" -keep yes -force yes

:: CleanUp, Delete temp Folders
rmdir "%destinationFolder%\tModLoader Windows %version%" /S /Q
rmdir "%destinationFolder%\tModLoader Mac %version%" /S /Q
rmdir "%destinationFolder%\tModLoader Linux %version%" /S /Q

:: Copy to public DropBox Folder
::copy "%destinationFolder%\tModLoader Windows %version%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\tModLoader Windows %version%.zip"
::copy "%destinationFolder%\tModLoader Mac %version%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\tModLoader Mac %version%.zip"
::copy "%destinationFolder%\tModLoader Linux %version%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\tModLoader Linux %version%.zip"

:: ExampleMod.zip (TODO, other parts of ExampleMod release)
rmdir ..\ExampleMod\bin /S /Q
rmdir ..\ExampleMod\obj /S /Q
call zipjs.bat zipItem -source "..\ExampleMod" -destination "%destinationFolder%\ExampleMod %version%.zip" -keep yes -force yes
::copy "%destinationFolder%\ExampleMod %version%.zip" "C:\Users\Javid\Dropbox\Public\TerrariaModding\tModLoaderReleases\"

:: TODO -- tModReader?

echo(
echo(
echo(
echo tModLoader %version% ready to release.
echo(
echo(
pause
