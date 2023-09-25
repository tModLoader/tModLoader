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

echo "Verifying .NET...."  2>&1 | tee -a "$LogFile"
echo "This may take a few moments."
echo "Logging to $LogFile"  2>&1 | tee -a "$LogFile"

if [[ "$_uname" == *"_NT"* ]]; then
	run_script ./Remove13_64Bit.sh  2>&1 | tee -a "$LogFile"
fi

. ./UnixLinkerFix.sh

#Parse version from runtimeconfig, jq would be a better solution here, but its not installed by default on all distros.
echo "Parsing .NET version requirements from runtimeconfig.json"  2>&1 | tee -a "$LogFile"
dotnet_version=$(sed -n 's/^.*"version": "\(.*\)"/\1/p' <../tModLoader.runtimeconfig.json) #sed, go die plskthx
export dotnet_version=${dotnet_version%$'\r'} # remove trailing carriage return that sed may leave in variable, producing a bad folder name
#echo $version
# use this to check the output of sed. Expected output: "00000000 35 2e 30 2e 30 0a |5.0.0.| 00000006"
# echo $(hexdump -C <<< "$version")
export dotnet_dir="$root_dir/dotnet"
if [[ -n "$IS_WSL" || -n "$WSL_DISTRO_NAME" ]]; then
	echo "wsl detected. Setting dotnet_dir=dotnet_wsl"
	export dotnet_dir="$root_dir/dotnet_wsl"
fi
export install_dir="$dotnet_dir/$dotnet_version"
echo "Success!"  2>&1 | tee -a "$LogFile"

if [[ ! -f "$LaunchLogs/client.log" && ! -f "$LaunchLogs/server.log" ]]; then
	echo "Last Run Attempt Failed to Start tModLoader. Deleting dotnet_dir and resetting"
	rm -rf "$dotnet_dir"
	mkdir "$dotnet_dir"
fi

run_script ./InstallNetFramework.sh  2>&1 | tee -a "$LogFile"


echo "Attempting Launch..."

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
	echo "Launched Using Local Dotnet"  2>&1 | tee -a "$LogFile"
	[[ -f "$install_dir/dotnet" ]] && chmod a+x "$install_dir/dotnet"
	exec "$install_dir/dotnet" tModLoader.dll "$customargs" "$@" 2>"$NativeLog"
else
	echo "Launched Using System Dotnet"  2>&1 | tee -a "$LogFile"
	exec dotnet tModLoader.dll "$customargs" "$@" 2>"$NativeLog"
fi
