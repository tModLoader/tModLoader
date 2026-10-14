#!/bin/sh

# For the best development experience, alias this script in your shell config.
# For example:
# (ba)sh  - alias tml=~/dev/terraria/tModLoader/setup-cli.sh
# Nushell - alias tml = ~/dev/terraria/tModLoader/setup-cli.sh

# CD to the script's directory and allow execution from elsewhere.
cd "$(dirname "$0")"

if ! command -v git > /dev/null; 
then
    echo "git not found in PATH"
    exit 1
fi

submoduleupdatemarker=".git/tml-setup-module-init.touch"
if ! [ ".git/index" -ot "$submoduleupdatemarker" ]
then
	echo "Restoring git submodules"
	git submodule update --init --recursive
	if [ $? -ne 0 ]
	then
		exit $?
	fi
	touch "$submoduleupdatemarker"
fi

if ! command -v dotnet > /dev/null
then
    echo "dotnet not found in PATH"
    exit 1
fi

dotnet run --project setup/CLI/Setup.CLI.csproj -c Release -p:WarningLevel=0 -v q -- "$@"

