#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname $(realpath "$0"))"
. ./BashUtils.sh

echo "You are on platform: \"$_uname\""

# Try to prevent "misleading" execution inside WSL
# Check from: https://stackoverflow.com/questions/38086185/how-to-check-if-a-program-is-run-in-bash-on-ubuntu-on-windows-and-not-just-plain
if [[ -n "$IS_WSL" || -n "$WSL_DISTRO_NAME" ]]; then
	read -p "You seem to be running this script in WSL write y to continue anyway: " _answer
	if [[ $_answer != "y" ]]; then
		exit 1
	fi
fi

if [ "$_uname" = Darwin ]; then
	LaunchLogs="$HOME/Library/Application Support/Terraria/ModLoader/Beta/Logs"
elif [[ "$_uname" == *"_NT"* ]]; then
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

if [[ "$_uname" == *"_NT"* ]]; then
	run_script ./Remove13_64Bit.sh 2>&1 | tee "$LogFile"
fi

run_script ./UnixLinkerFix.sh 2>&1 | tee -a "$LogFile"
run_script ./PlatformLibsDeploy.sh 2>&1 | tee -a "$LogFile"

# Source InstallNetFramework so you bring in $install_dir containing the version and no guessing
# Redirecting stream seems to spawn a different thing...
. ./InstallNetFramework.sh #2>&1 | tee -a "$LogFile"

echo "Attempting Launch..."
sleep 1

# Actually run tML with the passed arguments
# Move to the root folder
cd "$root_dir"

if [[ "$_uname" == *"_NT"* ]]; then
	# Replace / with \\ in WINDIR env var to not confuse MonoMod about the current platform
	# somehow busybox-w64 replaces paths in envs with normalized paths (no clue why..., maybe open an issue there?)
	WINDIR=${WINDIR////\\}
fi

if [[ -f "$install_dir/dotnet" || -f "$install_dir/dotnet.exe" ]]; then
	"$install_dir/dotnet" tModLoader.dll $*
else
	dotnet tModLoader.dll $*
fi
