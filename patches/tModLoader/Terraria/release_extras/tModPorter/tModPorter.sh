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
else
    export DOTNET_ROOT=$HOME/.dotnet
fi
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

DOTNET_PATH=$(which dotnet)
if [[ "$?" != "0" ]]; then
  echo "dotnet not found on PATH. Either add dotnet to PATH, set DOTNET_ROOT to the install path, or provide an install in ~/.dotnet"
  read -n 1 -s -r -p "Press any key to continue"
  exit 1
fi
DOTNET_X64_PATH=$(dirname $DOTNET_PATH)/x64/dotnet
if [[ -f $DOTNET_X64_PATH ]]; then
	DOTNET_PATH=$DOTNET_X64_PATH
fi

export DOTNET_ROLL_FORWARD=Minor
$DOTNET_PATH tModLoader.dll -tModPorter "$@" 2>&1 | tee ./tModLoader-Logs/tModPorter.log
