#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
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

# This should reflect build purpose somehow???
LaunchLogs="tModLoader-Logs"

if [ ! -d "$LaunchLogs" ]; then
	mkdir -p "$LaunchLogs"
fi
LogFile="$LaunchLogs/Launch.log"
NativeLog="$LaunchLogs/Natives.log"
if [ -f "$LogFile" ]; then
	rm "$LogFile"
fi
touch "$NativeLog"
if [ -f "$NativeLog" ]; then
	rm "$NativeLog"
fi
touch "$NativeLog"

echo "Verifying .NET...."
echo "This may take a few moments."
echo "Logging to $LogFile" 2>&1 | tee -a "$LogFile"

if [[ "$_uname" == *"_NT"* ]]; then
	run_script ./Remove13_64Bit.sh 2>&1 | tee -a "$LogFile"
fi

. ./UnixLinkerFix.sh
run_script ./PlatformLibsDeploy.sh 2>&1 | tee -a "$LogFile"

#Parse version from runtimeconfig, jq would be a better solution here, but its not installed by default on all distros.
echo "Parsing .NET version requirements from runtimeconfig.json"  2>&1 | tee -a "$LogFile"
dotnet_version=$(sed -n 's/^.*"version": "\(.*\)"/\1/p' <../tModLoader.runtimeconfig.json) #sed, go die plskthx
export dotnet_version=${dotnet_version%$'\r'} # remove trailing carriage return that sed may leave in variable, producing a bad folder name
#echo $version
# use this to check the output of sed. Expected output: "00000000 35 2e 30 2e 30 0a |5.0.0.| 00000006"
# echo $(hexdump -C <<< "$version")
export dotnet_dir="$root_dir/dotnet"
export install_dir="$dotnet_dir/$dotnet_version"
echo "Success!"  2>&1 | tee -a "$LogFile"

run_script ./InstallNetFramework.sh 2>&1 | tee -a "$LogFile"

echo "Attempting Launch..."

# Actually run tML with the passed arguments
# Move to the root folder
cd "$root_dir"

if [[ "$_uname" == *"_NT"* ]]; then
	# Replace / with \\ in WINDIR env var to not confuse MonoMod about the current platform
	# somehow busybox-w64 replaces paths in envs with normalized paths (no clue why..., maybe open an issue there?)
	export WINDIR=${WINDIR////\\}
fi

clear
if [[ -f "$install_dir/dotnet" || -f "$install_dir/dotnet.exe" ]]; then
	echo "Launched Using Local Dotnet"  2>&1 | tee -a "$LogFile"
	chmod a+x "$install_dir/dotnet"
	exec "$install_dir/dotnet" tModLoader.dll "$@" 2>"$NativeLog"
else
	echo "Launched Using System Dotnet"  2>&1 | tee -a "$LogFile"
	exec dotnet tModLoader.dll "$@" 2>"$NativeLog"
fi
