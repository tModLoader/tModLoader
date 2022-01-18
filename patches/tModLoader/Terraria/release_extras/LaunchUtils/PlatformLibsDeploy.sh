#!/bin/bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
script_dir="$(pwd -P)"
root_dir="$(dirname "$script_dir")"
wnd="$1"

_uname=$(uname)
echo "Verifying .NET platform specific libraries are correctly deployed"
if [ "wnd" = true ]; then
	sleep 0.01
else
  # Ensure Unix builds have the right version of Steamworks.NET
  unixSteamworks="PlatformVariantLibs/UNIX.Steamworks.NET.dll"
  if [ -f "$unixSteamworks" ]; then
    echo "Deploying Steamworks.NET for this platform..."
    steamworksVersion=$(find ./Libraries/Steamworks.NET -maxdepth 1 -type d -name '*.*.*' -printf %f -quit)
    defaultSteamworks="Libraries/Steamworks.NET/$steamworksVersion/Steamworks.NET.dll"
    mv "$unixSteamworks" "$defaultSteamworks"
  fi
fi
echo "Success!"