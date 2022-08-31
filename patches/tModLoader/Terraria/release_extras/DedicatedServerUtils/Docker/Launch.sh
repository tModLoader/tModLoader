#!/bin/bash

#shellcheck disable=2164

function pseudo_stdin {
	tmux send-keys "$1" Enter
}

function stop {
	echo Saving
	pseudo_stdin "exit"
	pid=$(pgrep dotnet)
	while [[ -e /proc/$pid ]]
	do
		sleep .5
	done
	pkill tmux
	echo Exited
	exit 0
}

trap stop SIGTERM SIGINT

# Installing/updating mods
mkdir -p ~/.local/share/Terraria
./manage-tModLoaderServer.sh -u --mods-only --check-dir ~/.local/share/Terraria --folder ~/.local/share/Terraria/wsmods

# Symlink tML's local dotnet install so that it can persist through runs
mkdir -p ~/.local/share/Terraria/dotnet
ln -s /home/tml/.local/share/Terraria/dotnet/ /home/tml/tModLoader/dotnet

echo "Launching tModLoader..."
cd ~/tModLoader
# Maybe eventually steamcmd will allow for an actual steamserver. For now -nosteam is required.
tmux new-session -s "tml" -n "tmlwin" -d "./start-tModLoaderServer.sh -config $HOME/.local/share/Terraria/serverconfig.txt -nosteam -steamworkshopfolder $HOME/.local/share/Terraria/wsmods"
# Loop will just cause the script to hang forever, keeping SIGTERM and SIGINT trapped and allowing exits to happen cleanly.
until false
do
	sleep 1
	true
done
