#!/usr/bin/env bash
cd "$(dirname "$0")"

launchArgs="-server"

# Use serverconfig.txt as config if not already specified
if ! [[ "$@" == *"-config"* ]]
then
	launchArgs="$launchArgs -config serverconfig.txt"
fi

# Parse parameters, passing through everything else to ScriptCaller.sh
while [[ $# -gt 0 ]]
do
	case $1 in
		--steamserver)
			steamServer=true
			shift
			;;
		--nosteamserver)
			steamServer=false
			shift
			;;
		--lobbytype)
			lobbyType="$2"
			shift; shift
			;;
		*) # If it's not one of the args above, then pass it to ScriptCaller.sh
			launchArgs="$launchArgs $2"
			shift
			;;
	esac
done

# Prompt user for lobby type and steam server if not specified in args
if ! [[ -v steamServer ]]
then
	read -p "Use steam server (y/n): " steamServerResponse
	if [[ $steamServerResponse == y* ]]
	then
		steamServer=true
	else
		steamServer=false
	fi
fi

if ! [[ -v lobbyType ]]
then
	read -p "Select lobby type ([f]riends/[p]rivate): " lobbyTypeResponse
	if [[ $lobbyTypeResponse == f* ]]
	then
		lobbyType="friends"
	else
		lobbyType="private"
	fi
fi

chmod +x ./LaunchUtils/ScriptCaller.sh
./LaunchUtils/ScriptCaller.sh $launchArgs -lobby $lobbyType
