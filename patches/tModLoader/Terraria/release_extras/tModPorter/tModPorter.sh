#!/usr/bin/env bash
cd "$(dirname "$0")"/..
if [[ ! -z "$DOTNET_ROOT" ]]; then
	echo "DOTNET_ROOT is $DOTNET_ROOT"
	export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
fi
export DOTNET_ROLL_FORWARD=Minor
dotnet tModLoader.dll -tModPorter $@ &> ./tModLoader-Logs/tModPorter.log
