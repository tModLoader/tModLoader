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

echo "Verifying .NET...."  2>&1 | tee -a "$LogFile"
echo "This may take a few moments."
echo "Logging to $LogFile"  2>&1 | tee -a "$LogFile"

if [[ "$_uname" == *"_NT"* ]]; then
	run_script ./Remove13_64Bit.sh  2>&1 | tee -a "$LogFile"
fi

. ./UnixLinkerFix.sh
run_script ./PlatformLibsDeploy.sh  2>&1 | tee -a "$LogFile"

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

run_script ./InstallNetFramework.sh  2>&1 | tee -a "$LogFile"

# Actually run tML with the passed arguments
# Move to the root folder
cd "$root_dir"

# Move to new Preview System file saves
if [ "$_uname" = Darwin ]; then
	BaseDir="$HOME/Library/Application Support"
elif [[ "$_uname" == *"_NT"* ]]; then
	BaseDir="$USERPROFILE/Documents/My Games"
	if [ ! -d "$BaseDir" ]; then
	  BaseDir="$USERPROFILE/OneDrive/Documents/My Games"
	fi
else
	BaseDir="$XDG_DATA_HOME"
	if [ "$BaseDir" = "" ]; then
		BaseDir="$HOME/.local/share"
	fi
fi
OldDir="$BaseDir/Terraria/ModLoader/Beta"
if [ -d "$OldDir" ]; then
	echo "Migrating Terraria/ModLoader/Beta to Terraria/tModLoader-preview" 2>&1 | tee -a "$LogFile"
	NewDir="$BaseDir/Terraria/tModLoader-preview"
	mv "$OldDir" "$NewDir"

	echo "Renaming Mod Sources, Mod Config, Mod Reader folders to remove spaces" 2>&1 | tee -a "$LogFile"
	mv "$NewDir/Mod Sources" "$NewDir/ModSources"  
	mv "$NewDir/Mod Config" "$NewDir/ModConfig"  
	mv "$NewDir/Mod Reader" "$NewDir/ModReader"

	echo "Copying files to tModLoader-dev directory for Contributors" 2>&1 | tee -a "$LogFile"
	cp -r "$NewDir" "$BaseDir/Terraria/tModLoader-dev" 
	echo "Copying files to tModLoader-dev directory for Players" 2>&1 | tee -a "$LogFile"
	cp -r "$NewDir" "$BaseDir/Terraria/tModLoader" 
fi

echo "Attempting Launch..." 2>&1 | tee -a "$LogFile"

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
