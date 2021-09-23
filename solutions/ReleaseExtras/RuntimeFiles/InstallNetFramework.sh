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
_uname=$(uname)
if [ "$_uname" = Darwin ]; then
  library_dir="$script_dir/Libraries/Native/OSX"
  export DYLD_LIBRARY_PATH="$library_dir"
  ln -sf "$library_dir/libSDL2-2.0.0.dylib" "$library_dir/libSDL2.dylib"
elif [ "$_uname" = Windows_NT ]; then
  # Pass, nothing to do here
else
  library_dir="$script_dir/Libraries/Native/Linux"
  export LD_LIBRARY_PATH="$library_dir"
  ln -sf "$library_dir/libSDL2-2.0.so.0" "$library_dir/libSDL2.so"
fi

if [ "$_uname" != Windows_NT ]; then
  # Ensure Unix builds have the right version of Steamworks.NET
  unixSteamworks="PlatformVariantLibs/UNIX.Steamworks.NET.dll"
  if [ -f "$unixSteamworks" ]; then
    echo "Deploying Steamworks.NET for this platform..."
    steamworksVersion=$(find ./Libraries/Steamworks.NET -maxdepth 1 -type d -name '*.*.*' -printf %f -quit)
    defaultSteamworks="Libraries/Steamworks.NET/$steamworksVersion/Steamworks.NET.dll"
    mv "$unixSteamworks" "$defaultSteamworks"
  fi
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

# Sourced from dotnet-install.sh
# Check if a program is present or not
machine_has() {
  eval $invocation

  command -v "$1" > /dev/null 2>&1
  return $?
}

file_download() {
  if machine_has "curl"; then
    curl -sLo "$1" "$2"
  elif machine_has "wget" then
    wget -q -O "$1" "$2"
  else
    echo "Missing dependency: neither curl nor wget was found."
    return false # @TODO: Should hard fail?
  fi
  return true
}

#If the install directory for this specific dotnet version doesnt exist, grab the installer script and run it.
if [ ! -d "$install_dir" ]; then
  installLogs="LaunchLogs/install.log"
  echo "Logging to $installLogs"
  if [ -f "$installLogs" ]; then
    rm "$installLogs" 
  fi
  exec 3<&1 4<&2 1>>$installLogs 2>&1
  
  dotnet_runtime=dotnet

  if [ "$_uname" = Windows_NT ]; then
    file_download dotnet-install.ps1 https://dot.net/v1/dotnet-install.ps1
    chmod +x dotnet-install.ps1
    
    # @TODO: Should update powershell to 4+ because required by the script (and not present by default in win 7/8)
    powershell -NoProfile -ExecutionPolicy unrestricted dotnet-install.ps1 -Channel "$channel" -InstallDir "$install_dir" -Runtime "$dotnet_runtime" -Version "$version"
  else
    file_download dotnet-install.sh https://dot.net/v1/dotnet-install.sh

    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel "$channel" --install-dir "$install_dir" --runtime "$dotnet_runtime" --version "$version"
  fi

  exec 1>&3 2>&4
fi

# Technically can happen on any system, but Windows_NT is the one expected to fail if powershell is not 4+
# so it's treated differently with step-by-step manual install
if [ "$_uname" = Windows_NT ]; then
  # If the install failed, provide a link to get the portable directly, and instructions on where to do with it.
  recidive_install=0
  while [[ ! -f "$install_dir/dotnet" && ! -f "$install_dir/dotnet.exe" ]]; do
    if [ $recidive_install = 1 ]; then
      read -p "\"$install_dir/dotnet.exe\" not detected. Please ensure step is complete before continuing with Enter."
      goto :loop
    )
    mkdir "$install_dir"

    echo "It has been detected that your system failed to install the dotnet portables automatically. Will now proceed manually."
    start "" "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-"$version"-windows-x64-binaries"
    echo "Now manually downloading the x64 .NET portable. Please find it in the opened browser."
    read -p "Press 'ENTER' to proceed to the next step..."
    echo "Please extract the downloaded Zip file contents in to \"$install_dir\""
    read -p "Please press Enter when this step is complete."

    recidive_install=1
  done
fi
