@echo off
:: Configuration of all parameters
::Tmod Constants
set appid=1281930

::User Constants
set publishid=%1
set contentfolder=%~2

::PublishData variables
set changenote=%3
set description=%4
set modversion=%5
set tmodversion=%6

::Steam Login Info variables
set username=%7
set password=%8
set steamguard=%9

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

:: Publish the item
@echo on
steamcmd.exe +login %username% %password% %steamguard% +workshop_build_item publish.vdf +quit