::Steam Login Info variables
set username=%1
set password=%2

::Mod variables
set changenote=%3

::Script variables
set currdir="%~dp0"
cd /D %currdir%
cd ..
set parentdir="%cd%"

cd /D %currdir%

:: Fetch tModLoader 1.4 from Steam
::steamcmd +login %username% %password% +force_install_dir tMod "+app_update 1281930 -validate -beta public-1.4-alpha" +quit 

cd /D %currdir%/tMod
dotnet tmodloader.dll -server -build %parentdir% -tmlsavedirectory %currdir% -ciprep %changenote%
cd ..

:: Publish the item
@echo on
::steamcmd +login %username% +workshop_build_item publish.vdf +quit