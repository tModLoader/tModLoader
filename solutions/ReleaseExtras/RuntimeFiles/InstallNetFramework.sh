#!/bin/sh
#Author: covers1624
# Provided for use in tModLoader deployment. 

#Get path of the script
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")
#CD to the script location.
cd "$script_dir"

echo "Verifying Net Framework...."
echo "This may take a few moments."

#Parse version from runtimeconfig, jq would be a better solution here, but its not installed by default on all distros.
version=$(sed -n 's/^.*"version": "\(.*\)"/\1/p' <tModLoader.runtimeconfig.json) #sed, go die plskthx
#Cut everything before the second dot
channel=$(echo "$version" | cut -f1,2 -d'.')
dotnet_dir="$script_dir/NetFramework/dotnet"
install_dir="$dotnet_dir/$version"

#If the dotnet dir exists, we need to do some cleanup
if [ -d "$dotnet_dir" ]; then
  # Find all folders inside the dotnet dir that don't match our target version and nuke it
  for folder in $(ls $script_dir/Libraries/dotnet/); do
    if [ ! $version = "$folder" ]; then
      old_version="$script_dir/NetFramework/dotnet/$folder"
      echo "Cleaning $old_version"
      rm -rf "$old_version"
    fi
  done
fi

#If the install directory for this specific dotnet version doesnt exist, grab the installer script and run it.
if [ ! -d "$install_dir" ]; then
  #TODO, fallback to wget if curl is unavailable
  curl -sLo dotnet-install.sh https://dot.net/v1/dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --channel "$channel" --install-dir "$install_dir" --runtime "dotnet" --version "$version"
  
  #TODO: Attempt to change icon of dotnet for legacy, current
  #gvfs-set-attribute -t string "NetFramework/dotnet/$version/dotnet" metadata::custom-icon file:"Libraries/Native/tModLoader.png"
  #gio set "NetFramework/dotnet/$version/dotnet" metadata::custom-icon file:"Libraries/Native/tModLoader.png"
fi