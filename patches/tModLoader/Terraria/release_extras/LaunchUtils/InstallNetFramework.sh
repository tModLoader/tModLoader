#!/bin/bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#chdir to path of the script and save it
cd "$(dirname "$0")"
script_dir="$(pwd -P)"
root_dir="$(dirname "$script_dir")"
_uname=$(uname)
wnd="$1"

#Parse version from runtimeconfig, jq would be a better solution here, but its not installed by default on all distros.
echo "Parsing .NET version requirements from runtimeconfig.json"
version=$(sed -n 's/^.*"version": "\(.*\)"/\1/p' <../tModLoader.runtimeconfig.json) #sed, go die plskthx
version=${version%$'\r'} # remove trailing carriage return that sed may leave in variable, producing a bad folder name
#echo $version
# use this to check the output of sed. Expected output: "00000000 35 2e 30 2e 30 0a |5.0.0.| 00000006"
# echo $(hexdump -C <<< "$version")
#Cut everything before the second dot
channel=$(echo "$version" | cut -f1,2 -d'.')
dotnet_dir="$root_dir/dotnet"
install_dir="$dotnet_dir/$version"
echo "Success!"

echo "Checking for old .NET versions to remove from folder"
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
echo "Cleanup Complete"

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
  elif machine_has "wget"; then
    wget -q -O "$1" "$2"
  else
    echo "Missing dependency: neither curl nor wget was found."
    return 123 # @TODO: Should hard fail?
  fi
  return 0
}

#If the install directory for this specific dotnet version doesnt exist, grab the installer script and run it.
echo "Checking for Updates to .NET"
if [ ! -d "$install_dir" ]; then
  installLogs="LaunchLogs/install.log"
  dotnet_runtime=dotnet

  if [ "$wnd" = true ]; then
    file_download dotnet-install.ps1 https://dot.net/v1/dotnet-install.ps1
    
    # @TODO: Should update powershell to 4+ because required by the script (and not present by default in win 7/8)
    powershell.exe -NoProfile -ExecutionPolicy unrestricted -File dotnet-install.ps1 -Channel "$channel" -InstallDir "$install_dir" -Runtime "$dotnet_runtime" -Version "$version"
  else
    file_download dotnet-install.sh https://dot.net/v1/dotnet-install.sh

    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel "$channel" --install-dir "$install_dir" --runtime "$dotnet_runtime" --version "$version"
  fi
fi
echo "Finished Checking for Updates"

# Technically can happen on any system, but Windows_NT is the one expected to fail if powershell is not 4+
# so it's treated differently with step-by-step manual install
if [ "$wnd" = true ]; then
  # If the install failed, provide a link to get the portable directly, and instructions on where to do with it.
  if [[ ! -f "$install_dir/dotnet" && ! -f "$install_dir/dotnet.exe" ]]; then
    mkdir "$dotnet_dir"

    echo "It has been detected that your system failed to install the dotnet portables automatically. Will now proceed manually."
    file_download "../dotnet/$version.zip" "https://dotnetcli.azureedge.net/dotnet/Runtime/$version/dotnet-runtime-$version-win-x64.zip"
    echo "Now downloading the x64 .NET portable for manual install. Please extract the zip file at $install_dir"
    read -p "Please press Enter when this step is complete."
  fi
fi
