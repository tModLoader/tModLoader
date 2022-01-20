#!/usr/bin/env bash
cd "$(dirname "$0")"

script_dir="$(pwd -P)"
launch_args="-server -config serverconfig.txt"

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

./start-tModLoader.sh $launch_args
