#!/usr/bin/env bash
if [ ! -t 0 ]; then
	cd "$(dirname "$0")"
	echo "Not in a terminal, attempting to open terminal" >&2
	. ../LaunchUtils/BashUtils.sh
	if machine_has "konsole"; then
		konsole -e "$0" "$@"
	elif machine_has "gnome-terminal"; then
		gnome-terminal -- "$0" "$@"
	elif machine_has "xterm"; then
		xterm -e "$0" "$@"
	elif [ "$_uname" = Darwin ]; then
		osascript \
			-e "on run(argv)" \
			-e "set tmodporter to item 1 of argv" \
			-e "set csproj to item 2 of argv" \
			-e "tell application \"Terminal\" to activate" \
			-e "tell application \"Terminal\" to do script the quoted form of tmodporter & \" \" & the quoted form of csproj" \
			-e "end" \
			-- "$0" "$@"
	else
		echo "Could not find terminal app"
		exit 1
	fi
	echo "Launched in another terminal. See tModPorter.log for details"
	exit
fi

cd "$(dirname "$0")"/..
if [[ ! -z "$DOTNET_ROOT" ]]; then
	echo "DOTNET_ROOT is $DOTNET_ROOT"
	export DOTNET_ROOT=$HOME/.dotnet
	export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
fi
export DOTNET_ROLL_FORWARD=Minor
dotnet tModLoader.dll -tModPorter "$@" 2>&1 | tee ./tModLoader-Logs/tModPorter.log
