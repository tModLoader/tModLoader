#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
. ./BashUtils.sh

echo "You are on platform: \"$_uname\" arch: \"$_arch\""

# Detect arm64 launches and the presence of Rosetta (oahd running on the system is how the dotnet official install script does it)
if [ "$_arch" = "arm64" ] && [ "$(/usr/bin/pgrep oahd >/dev/null 2>&1;echo $?)" -eq 0 ]; then
	echo "arm64 environment with Rosetta detected, restarting under arch -x86_64"
	# Note this only changes the environment, so that dotnet install scripts download an x86 version. Launching an x86 process from an arm shell or vice versa does not require using arch, or otherwise intentionally invoking Rosetta.
	exec arch -x86_64 ./ScriptCaller.sh "$@"
fi

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

# Environment variable fixes & Platform Cleanups
. ./EnvironmentFix.sh

echo "Verifying .NET...."  2>&1 | tee -a "$LogFile"
echo "This may take a few moments."

# Get Dotnet Version expecting to have installed
source ./DotNetVersion.sh

# Attempt to fix first time Crash To Desktop due to dotnet install failure
if [[ ! -f "$LaunchLogs/client.log" && ! -f "$LaunchLogs/server.log" ]]; then
	echo "Last Run Attempt Failed to Start tModLoader. Deleting dotnet_dir and resetting"  2>&1 | tee -a "$LogFile"
	rm -rf "$dotnet_dir"
	mkdir "$dotnet_dir"
fi

# Dotnet binaries Fixes (Proton, AppleSilicon)
if [[ "$_uname" == *"_NT"* ]]; then
	if [[ -f "$dotnet_dir/dotnet" ]]; then
		echo "A non-Windows dotnet executable was detected. Deleting dotnet_dir and resetting"  2>&1 | tee -a "$LogFile"
		rm -rf "$dotnet_dir"
		mkdir "$dotnet_dir"
	fi
else
	if [[ -f "$dotnet_dir/dotnet.exe" ]]; then
		echo "A Windows dotnet executable was detected, possibly from a previous Proton launch. Deleting dotnet_dir and resetting"  2>&1 | tee -a "$LogFile"
		rm -rf "$dotnet_dir"
		mkdir "$dotnet_dir"
	elif [[ "$_arch" != "arm64" ]] && [[ "$(file "$dotnet_dir/dotnet")" == *"arm64"* ]]; then
		echo "An arm64 install of dotnet was detected. Deleting dotnet_dir and resetting"  2>&1 | tee -a "$LogFile"
		rm -rf "$dotnet_dir"
		mkdir "$dotnet_dir"
	fi
fi

# Installing Dotnet
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

if [[ -f "$dotnet_dir/dotnet" || -f "$dotnet_dir/dotnet.exe" ]]; then
	export DOTNET_ROLL_FORWARD=Disable
	echo "Launched Using Local Dotnet. Launch command: \"$dotnet_dir/dotnet\" tModLoader.dll \"$@\"" 2>&1 | tee -a "$LogFile"
	[[ -f "$dotnet_dir/dotnet" ]] && chmod a+x "$dotnet_dir/dotnet"
	exec "$dotnet_dir/dotnet" tModLoader.dll "$@" 2>"$NativeLog"
else
	echo "Launched Using System Dotnet. Launch command: dotnet tModLoader.dll \"$@\"" 2>&1 | tee -a "$LogFile"
	exec dotnet tModLoader.dll "$@" 2>"$NativeLog"
fi
