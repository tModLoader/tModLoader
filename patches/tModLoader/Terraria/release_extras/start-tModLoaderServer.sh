#!/usr/bin/env bash
cd "$(dirname "$0")"

launchArgs="-server"

# Parse parameters, passing through everything else to ScriptCaller.sh
while [[ $# -gt 0 ]]
do
	case $1 in
		-steam)
			steamServer=true
			shift
			;;
		-nosteam)
			steamServer=false
			shift
			;;
		*) # If it's not one of the args above, then pass it to ScriptCaller.sh
			launchArgs="$launchArgs $1"
			shift
			;;
	esac
done

# Use serverconfig.txt as config if not already specified
if ! [[ "$launchArgs" == *"-config"* ]]
then
	launchArgs="$launchArgs -config serverconfig.txt"
fi

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

if $steamServer
then
	launchArgs="$launchArgs -steam"
fi

if ! [[ "$launchArgs" == *"-lobby"* ]]
then
	read -p "Select lobby type ([f]riends/[p]rivate): " lobbyTypeResponse
	if [[ $lobbyTypeResponse == f* ]]
	then
		launchArgs="$launchArgs -lobby friends"
	else
		launchArgs="$launchArgs -lobby private"
	fi
fi

chmod +x ./LaunchUtils/ScriptCaller.sh
./LaunchUtils/ScriptCaller.sh $launchArgs
