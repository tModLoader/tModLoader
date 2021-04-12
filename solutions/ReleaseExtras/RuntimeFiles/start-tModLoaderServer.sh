#!/bin/sh
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")
cd "$script_dir"

. InstallNetFramework.sh
clear

read -p "Use Steam Server (y)/(n) " steam

if [ ! $steam == "y" ]; then
	clear
	dotnet tModLoaderServer.dll -config serverconfig.txt
	exit
fi

read -p "Select Lobby Type (f)riends/(p)rivate " lobby
clear

if [ $lobby == "f" ]; then 
	dotnet tModLoaderServer.dll -steam -lobby friends -config serverconfig.txt
	exit
fi

dotnet tModLoaderServer.dll -steam -lobby private -config serverconfig.txt