@echo off
:: Configuration of all parameters
::Tmod Constants
set appid=1281930
set pass2=IUNDERSTANDTHATTHISALPHAISMOSTLYFORDEVELOPERS

::User Constants
set root=%1
set item=%2

::Steam Login Info variables
set username=%4
set password=%5
set steamguard=%6

:: Fetch tModLoader 1.4 from Steam
steamcmd.exe +login %username% %password% %steamguard% +force_install_dir %root%/tMod +app_update %appid% -validate -beta public-1.4-alpha -betapassword %pass2% +quit

dotnet %root%/tMod/tmodloader.dll -server -build %item% -tmlsavedirectory %root%/build -ciprep

:: All the following should be done in tmodloader.dll build action when -ciprep is included as an argument [
:: Verify workshop.json
if Not exist %contentfolder%\workshop.json (echo "ERROR: no workshop.json found")

:: Create the vdf file 
set descriptionFinal=[quote=GithubActions(Don't Modify)]Version %modversion% built for tModLoader v%tmodversion%[/quote]%description%
(
	echo "workshopitem"
	echo {
	echo "appid" "%appid%"
	echo "publishedfileid" "%publishid%"
	echo "contentfolder" "%contentfolder%"
	echo "changenote" "%changenote%"
	echo "description" "%descriptionFinal%"
	echo }
)>publish.vdf
::] End comment

:: Publish the item
@echo on
steamcmd.exe +login %username% +workshop_build_item publish.vdf +quit