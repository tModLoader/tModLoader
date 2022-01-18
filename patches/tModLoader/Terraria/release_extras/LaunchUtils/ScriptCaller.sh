#!/bin/bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
script_dir="$(pwd -P)"
root_dir="$(dirname "$script_dir")"
_uname=$(uname)

# Call a script setting its permission right for execution
run_script() {
  chmod +x $1
  echo "'$*'"
  $*
}

echo "You are on platform: \"$_uname\""
read -p "[DEBUG] Press enter to continue... "

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
	if [ "$LaunchLogs" = "" ]; then
		LaunchLogs="$HOME/.local/share"
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

read -p "[DEBUG] Press enter to continue... "

if [ "$wnd" = true ]; then
	run_script ./Remove13_64Bit.sh 2>&1 | tee "$LogFile"
	read -p "[DEBUG] Press enter to continue... "
fi

run_script ./UnixLinkerFix.sh $wnd 2>&1 | tee -a "$LogFile"
read -p "[DEBUG] Press enter to continue... "
run_script ./PlatformLibsDeploy.sh $wnd 2>&1 | tee -a "$LogFile"
read -p "[DEBUG] Press enter to continue... "
run_script ./InstallNetFramework.sh $wnd 2>&1 | tee -a "$LogFile"
read -p "[DEBUG] Press enter to continue... "

echo "Attempting Launch..."
sleep 1
