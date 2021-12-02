#!/bin/sh
#Author: covers1624
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
script_dir="$(pwd -P)"

echo "Verifying Net Framework...."
echo "This may take a few moments."

# The following is a workaround for the system's SDL2 library being preferred by the linkers for some reason.
# Additionally, something in dotnet is requesting 'libSDL2.so' (instead of 'libSDL2-2.0.so.0' that is specified in dependencies)
# without actually invoking managed NativeLibrary resolving events!
if [ "$(uname)" = Darwin ]; then
  library_dir="$script_dir/Libraries/Native/OSX"
  export DYLD_LIBRARY_PATH="$library_dir"
  export VK_ICD_FILENAMES="./Libraries/Native/OSX/MoltenVK_icd.json"
  ln -sf "$library_dir/libSDL2-2.0.0.dylib" "$library_dir/libSDL2.dylib"
else
  library_dir="$script_dir/Libraries/Native/Linux"
  export LD_LIBRARY_PATH="$library_dir"
  ln -sf "$library_dir/libSDL2-2.0.so.0" "$library_dir/libSDL2.so"
fi

# Ensure Unix builds have the right version of Steamworks.NET - WARNING, 15.0.1.0 is hardcoded and can change
unixSteamworks="PlatformVariantLibs/UNIX.Steamworks.NET.dll"
if [ -f "$unixSteamworks" ]; then
  echo "Deploying Steamworks.NET for this platform..."
  steamworksVersion=$(find ./Libraries/Steamworks.NET -maxdepth 1 -type d -name '*.*.*' -printf %f -quit)
  defaultSteamworks="Libraries/Steamworks.NET/$steamworksVersion/Steamworks.NET.dll"
  mv "$unixSteamworks" "$defaultSteamworks"
fi

# Ensure sufficient stack size (4MB) on MacOS secondary threads, doesn't hurt for Linux. 16^5 = 1MB, value in hex 
export COMPlus_DefaultStackSize=400000

#Parse version from runtimeconfig, jq would be a better solution here, but its not installed by default on all distros.
version=$(sed -n 's/^.*"version": "\(.*\)"/\1/p' <tModLoader.runtimeconfig.json) #sed, go die plskthx
version=${version%$'\r'} # remove trailing carriage return that sed may leave in variable, producing a bad folder name
#echo $version
# use this to check the output of sed. Expected output: "00000000 35 2e 30 2e 30 0a |5.0.0.| 00000006"
# echo $(hexdump -C <<< "$version")
#Cut everything before the second dot
channel=$(echo "$version" | cut -f1,2 -d'.')
dotnet_dir="$script_dir/dotnet"
install_dir="$dotnet_dir/$version"

if [ ! -d "LaunchLogs" ]; then
  mkdir "LaunchLogs"
fi

#If the dotnet dir exists, we need to do some cleanup
if [ -d "$dotnet_dir" ]; then
  # Find all folders inside the dotnet dir that don't match our target version and nuke it
  for folder in $(ls "$dotnet_dir"); do
    if [ ! $version = "$folder" ]; then
      old_version="$dotnet_dir/$folder"
      echo "Cleaning $old_version"
      rm -rf "$old_version"
    fi
  done
fi

#If the install directory for this specific dotnet version doesnt exist, grab the installer script and run it.
if [ ! -d "$install_dir" ]; then
  installLogs="LaunchLogs/install.log"
  echo "Logging to $installLogs"
  if [ -f "$installLogs" ]; then
    rm "$installLogs" 
  fi
  exec 3<&1 4<&2 1>>$installLogs 2>&1
  #TODO, fallback to wget if curl is unavailable
  curl -sLo dotnet-install.sh https://dot.net/v1/dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --channel "$channel" --install-dir "$install_dir" --runtime "dotnet" --version "$version"
  exec 1>&3 2>&4 
fi
