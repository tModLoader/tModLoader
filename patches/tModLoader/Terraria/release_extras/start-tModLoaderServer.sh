#!/usr/bin/env bash
cd "$(dirname "$0")"

launch_args="$@ -server"
if [[ ! "$launch_args" == *"-config"* ]]; then
	launch_args="$launch_args -config serverconfig.txt"
fi

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

chmod +x ./LaunchUtils/ScriptCaller.sh
./start-tModLoader.sh $launch_args