#!/bin/sh
cd "$(dirname "$0")"
script_dir="$(pwd -P)"
launch_args="-server -config serverconfig.txt"

. ./InstallNetFramework.sh

read -p "Use Steam Server (y)/(n) " steam

if [ $steam = "y" ]; then
	launch_args="$launch_args -steam"
	
	clear
	read -p "Select Lobby Type (f)riends/(p)rivate " lobby
	
	if [ $lobby == "f" ]; then 
		launch_args="$launch_args -lobby friends"
	else
		launch_args="$launch_args -lobby private"
	fi
fi

launch_args="$launch_args $*"

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
  exec 3>>"$runLogs" 2>&3
  echo "Attempting Launch.."
  dotnet tModLoader.dll $launch_args
fi
