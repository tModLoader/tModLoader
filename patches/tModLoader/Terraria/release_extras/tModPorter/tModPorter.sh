#!/usr/bin/env bash
if [ ! -t 0 ]; then
	cd "$(dirname "$0")"
	echo "Not in a terminal, attempting to open terminal"
	. ../LaunchUtils/BashUtils.sh
	if machine_has "konsole"; then
		konsole -e "$0 $@"
	elif machine_has "gnome-terminal"; then
		gnome-terminal -- "$0 $@"
	else
		xterm -e "$0 $@"
	fi
	echo "Done"
	exit
fi

cd "$(dirname "$0")"/..
if [[ ! -z "$DOTNET_ROOT" ]]; then
	echo "DOTNET_ROOT is $DOTNET_ROOT"
	export DOTNET_ROOT=$HOME/.dotnet
	export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
fi
export DOTNET_ROLL_FORWARD=Minor
dotnet tModLoader.dll -tModPorter $@ 2>&1 | tee ./tModLoader-Logs/tModPorter.log
