#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#
# THIS SCRIPT EXPECTS AS INPUTS:
#   dotnet_dir     : Folder where the versions of dotnet will be extracted
#   install_dir    : Target folder for the install (usually "$dotnet_dir/$version")
#   dotnet_version : Version of dotnet to install
#

#chdir to path of the script and save it
cd "$(dirname "$(realpath "$0")")"
. ./BashUtils.sh

#Cut everything before the second dot
channel=$(echo "$dotnet_version" | cut -f1,2 -d'.')

echo "Checking for old .NET versions to remove from folder"
#If the dotnet dir exists, we need to do some cleanup
if [ -d "$dotnet_dir" ]; then
  # Find all folders inside the dotnet dir that don't match our target version and nuke it
  for folder in $(ls "$dotnet_dir"); do
    if [ ! $dotnet_version = "$folder" ]; then
      old_version="$dotnet_dir/$folder"
      echo "Cleaning $old_version"
      rm -rf "$old_version"
    fi
  done
fi
echo "Cleanup Complete"

#If the install directory for this specific dotnet version doesnt exist, grab the installer script and run it.
echo "Checking for Updates to .NET"
if [ ! -d "$install_dir" ]; then
  installLogs="LaunchLogs/install.log"
  dotnet_runtime=dotnet

  if [[ "$_uname" == *"_NT"* ]]; then
    file_download dotnet-install.ps1 https://dot.net/v1/dotnet-install.ps1
    
    # @TODO: Should update powershell to 4+ because required by the script (and not present by default in win 7/8)
    powershell.exe -NoProfile -ExecutionPolicy unrestricted -File dotnet-install.ps1 -Channel "$channel" -InstallDir "$install_dir" -Runtime "$dotnet_runtime" -Version "$dotnet_version"
  else
    file_download dotnet-install.sh https://dot.net/v1/dotnet-install.sh

    run_script ./dotnet-install.sh --channel "$channel" --install-dir "$install_dir" --runtime "$dotnet_runtime" --version "$dotnet_version"
  fi
fi
echo "Finished Checking for Updates"

# Technically can happen on any system, but Windows_NT is the one expected to fail if powershell is not 4+
# so it's treated differently with step-by-step manual install
if [[ "$_uname" == *"_NT"* ]]; then
  # If the install failed, provide a link to get the portable directly, and instructions on where to do with it.
  if [[ ! -f "$install_dir/dotnet" && ! -f "$install_dir/dotnet.exe" ]]; then
    mkdir "$dotnet_dir"

    echo "It has been detected that your system failed to install the dotnet portables automatically. Will now proceed manually."
    file_download "$dotnet_dir/$dotnet_version.zip" "https://dotnetcli.azureedge.net/dotnet/Runtime/$dotnet_version/dotnet-runtime-$dotnet_version-win-x64.zip"
    unzip "$dotnet_dir/$dotnet_version.zip" -d "$install_dir"
    echo "Tryed to downlaod and extract x64 .NET portable. Please verify the extraction completed successfully to \"$install_dir\""
    read -p "Please press Enter when this step is complete."
  fi
fi
