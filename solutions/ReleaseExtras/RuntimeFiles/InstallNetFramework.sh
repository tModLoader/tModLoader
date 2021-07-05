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
  for folder in $(ls $script_dir/dotnet/); do
    if [ ! $version = "$folder" ]; then
      old_version="$script_dir/dotnet/$folder"
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
