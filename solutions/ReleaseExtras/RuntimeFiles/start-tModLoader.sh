#!/bin/sh
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")
launch_args=""
cd "$script_dir"

source ./InstallNetFramework.sh

if [ -d "$install_dir" ]; then
  ./dotnet/$version/dotnet tModLoader.dll $launch_args
fi
if [ ! -d "$install_dir" ]; then
  runLogs="LaunchLogs/runtime.log"
  echo "Portable install failed. Running manual .Net install"
  echo "Logging to $runLogs"
  if [ -f "$runLogs" ]; then
    rm "$runLogs" 
  fi
  exec 3>>$runLogs 2>&3
  echo "Attempting Launch.."
  dotnet tModLoader.dll $launch_args
fi
