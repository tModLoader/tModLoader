#!/bin/bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
script_dir="$(pwd -P)"
root_dir="$(dirname "$script_dir")"
_uname=$(uname)
wnd="$1"

if [ "$_uname" = Darwin ]; then
	LaunchLogs="$HOME/Library/Application Support/Terraria/ModLoader/Beta/Logs"
elif [ "$wnd" = true ]; then
	LaunchLogs="$USERPROFILE/Documents/My Games"
	if [ ! -d "$LaunchLogs" ]; then
	  LaunchLogs="$USERPROFILE/OneDrive/Documents/My Games"
	fi
	LaunchLogs="$LaunchLogs/Terraria/ModLoader/Beta/Logs"
else
	LaunchLogs="$XDG_DATA_HOME"
	if [ "$launchLogs" = "" ]; then
		launchLogs="$HOME/.local/share"
	fi
	LaunchLogs="$LaunchLogs/Terraria/ModLoader/Beta/Logs"
fi
if [ ! -d "$LaunchLogs" ]; then
  mkdir "$LaunchLogs"
fi

LogFile="$LaunchLogs/Launch.log"
if [ ! -f "$LogFile" ]; then
  touch "$LogFile"
fi

echo "Verifying .NET...."
echo "This may take a few moments."
echo "Logging to $LogFile"

read -p "Use Steam Server (y)/(n) " steam

if [ "$wnd" != true ]; then
	chmod +x UnixLinkerFix.sh
	./UnixLinkerFix.sh 2>&1 false | tee "$LogFile"
	read -p "Use Steam Server (y)/(n) " steam
	chmod +x PlatformLibsDeploy.sh
	./PlatformLibsDeploy.sh false 2>&1 | tee -a "$LogFile"
	read -p "Use Steam Server (y)/(n) " steam
	chmod +x InstallNetFramework.sh
	./InstallNetFramework.sh false 2>&1 | tee -a "$LogFile"
	read -p "Use Steam Server (y)/(n) " steam
else
	./Remove13_64Bit.bat 2>&1 | tee "$LogFile"
	read -p "Use Steam Server (y)/(n) " steam
	./busybox64.exe ./UnixLinkerFix.sh true 2>&1 | tee -a "$LogFile"
	read -p "Use Steam Server (y)/(n) " steam
	./busybox64.exe ./PlatformLibsDeploy.sh true 2>&1 | tee -a "$LogFile"
	read -p "Use Steam Server (y)/(n) " steam
	./InstallNetFramework.sh true 2>&1 | tee -a "$LogFile"
	read -p "Use Steam Server (y)/(n) " steam
fi

echo "Attempting Launch..."
sleep 1
