#!/usr/bin/env bash

# this file is meant to be run via source
# sets $dotnet_version, $dotnet_dir, $install_dir
# requires $root_dir,  $LogFile

if [[ -z "$root_dir" ]]; then
	echo "\$root_dir not set" 2>&1 | tee -a "$LogFile"
	exit 1
fi

#Parse version from runtimeconfig, jq would be a better solution here, but its not installed by default on all distros.
echo "Parsing .NET version requirements from runtimeconfig.json" 2>&1 | tee -a "$LogFile"
dotnet_version=$(sed -n 's/^.*"version": "\(.*\)"/\1/p' <"$root_dir/tModLoader.runtimeconfig.json") #sed, go die plskthx
export dotnet_version=${dotnet_version%$'\r'} # remove trailing carriage return that sed may leave in variable, producing a bad folder name
#echo $version
# use this to check the output of sed. Expected output: "00000000 35 2e 30 2e 30 0a |5.0.0.| 00000006"
# echo $(hexdump -C <<< "$version")
export dotnet_dir="$root_dir/dotnet"
if [[ -n "$IS_WSL" || -n "$WSL_DISTRO_NAME" ]]; then
	echo "wsl detected. Setting dotnet_dir=dotnet_wsl" 2>&1 | tee -a "$LogFile"
	export dotnet_dir="$root_dir/dotnet_wsl"
fi
if [ "$_arch" = "arm64" ]; then
	echo "arm64 detected. Setting dotnet_dir=dotnet_arm64" 2>&1 | tee -a "$LogFile"
	export dotnet_dir="$root_dir/dotnet_arm64"
fi
echo "Success! dotnet_version=$dotnet_version" 2>&1 | tee -a "$LogFile"