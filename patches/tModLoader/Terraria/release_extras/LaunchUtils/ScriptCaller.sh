#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
. ./BashUtils.sh

echo "You are on platform: \"$_uname\""

LaunchLogs="$root_dir/tModLoader-Logs"

if [ ! -d "$LaunchLogs" ]; then
	mkdir -p "$LaunchLogs"
fi

LogFile="$LaunchLogs/Launch.log"
if [ -f "$LogFile" ]; then
	rm "$LogFile"
fi
touch "$LogFile"
echo "Logging to $LogFile"  2>&1 | tee -a "$LogFile"

NativeLog="$LaunchLogs/Natives.log"
if [ -f "$NativeLog" ]; then
	rm "$NativeLog"
fi
touch "$NativeLog"

if [[ "$_uname" == *"_NT"* ]]; then
	echo "Windows Version $WINDOWS_MAJOR.$WINDOWS_MINOR" 2>&1 | tee -a "$LogFile"
	if [[ $WINDOWS_MAJOR -ge 10 ]]; then 
		./QuickEditDisable.exe 2>&1 | tee -a "$LogFile"
	fi
fi

if [[ "$WINDOWS_MAJOR" == "0" || ! -z "$WINEHOMEDIR" ]]; then
	echo "Proton has been detected. It is highly recommended to not use it as it causes all manner of issues. Please disable Proton and launch again. See https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#disable-proton for information on moving save data to the correct location." 2>&1 | tee -a "$LogFile"
fi

echo "Verifying .NET...."  2>&1 | tee -a "$LogFile"
echo "This may take a few moments."

if [[ "$_uname" == *"_NT"* ]]; then
	run_script ./Remove13_64Bit.sh  2>&1 | tee -a "$LogFile"
fi

. ./UnixLinkerFix.sh

source ./DotNetVersion.sh

if [[ ! -f "$LaunchLogs/client.log" && ! -f "$LaunchLogs/server.log" ]]; then
	echo "Last Run Attempt Failed to Start tModLoader. Deleting dotnet_dir and resetting"  2>&1 | tee -a "$LogFile"
	rm -rf "$dotnet_dir"
	mkdir "$dotnet_dir"
fi

if [[ "$_uname" == *"_NT"* ]]; then
	if [[ -f "$install_dir/dotnet" ]]; then
		echo "A non-Windows dotnet executable was detected. Deleting dotnet_dir and resetting"  2>&1 | tee -a "$LogFile"
		rm -rf "$dotnet_dir"
		mkdir "$dotnet_dir"
	fi
else
	if [[ -f "$install_dir/dotnet.exe" ]]; then
		echo "A Windows dotnet executable was detected, possibly from a previous Proton launch. Deleting dotnet_dir and resetting"  2>&1 | tee -a "$LogFile"
		rm -rf "$dotnet_dir"
		mkdir "$dotnet_dir"
	fi
fi

run_script ./InstallDotNet.sh  2>&1 | tee -a "$LogFile"


echo "Attempting Launch..."  2>&1 | tee -a "$LogFile"

# Actually run tML with the passed arguments
# Move to the root folder
cd "$root_dir"

if [[ "$_uname" == *"_NT"* ]]; then
	# Replace / with \\ in WINDIR env var to not confuse MonoMod about the current platform
	# somehow busybox-w64 replaces paths in envs with normalized paths (no clue why..., maybe open an issue there?)
	export WINDIR=${WINDIR////\\}

	clear
	sleep 1 # wait a little extra time for steam to realise that our parent process has exited
else
	# Kill the Steam reaper process on Linux/Mac?
	# Sed replace all null bytes(and spaces) with spaces, grep for reaper marker.
	if $(sed 's/\x0/ /g' /proc/$PPID/cmdline | grep -q "reaper SteamLaunch AppId=1281930"); then
		echo "Running under Steam reaper process. Killing.." 2>&1 | tee -a "$LogFile"
		kill -9 $PPID # _yeet_
	fi
fi

if [[ -f "$install_dir/dotnet" || -f "$install_dir/dotnet.exe" ]]; then
	export DOTNET_ROLL_FORWARD=Disable
	echo "Launched Using Local Dotnet. Launch command: \"$install_dir/dotnet\" tModLoader.dll \"$@\"" 2>&1 | tee -a "$LogFile"
	[[ -f "$install_dir/dotnet" ]] && chmod a+x "$install_dir/dotnet"
	exec "$install_dir/dotnet" tModLoader.dll "$@" 2>"$NativeLog"
else
	echo "Launched Using System Dotnet. Launch command: dotnet tModLoader.dll \"$@\"" 2>&1 | tee -a "$LogFile"
	exec dotnet tModLoader.dll "$@" 2>"$NativeLog"
fi
