#!/bin/sh
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")
launch_args=""
cd "$script_dir"

./InstallNetFramework.sh
clear

read -p "Use Steam Server (y)/(n) " steam

if [ $steam == "y" ]; then
	launch_args="$launch_args -steam"
	
	clear
	read -p "Select Lobby Type (f)riends/(p)rivate " lobby
	
	if [ $lobby == "f" ]; then 
		launch_args="$launch_args -lobby friends"
	else
		launch_args="$launch_args -lobby private"
	fi
	
fi

clear
./NetFramework/dotnet/5.0.0/dotnet tModLoader.dll -server $launch_args -config serverconfig.txt