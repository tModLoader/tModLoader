#!/usr/bin/env bash
cd "$(dirname "$0")" || exit

launch_args="-server"

# Parse parameters, passing through everything else to ScriptCaller.sh
while [[ $# -gt 0 ]]; do
	case $1 in
		-steam)
			steam_server=true
			shift
			;;
		-nosteam)
			steam_server=false
			shift
			;;
		*) # If it's not one of the args above, then pass it to ScriptCaller.sh
			launch_args="$launch_args $1"
			shift
			;;
	esac
done

# Use serverconfig.txt as config if not already specified
if ! [[ "$launch_args" == *"-config"* ]]
then
	launch_args="$launch_args -config serverconfig.txt"
fi

# Prompt user for lobby type and steam server if not specified in args
if [ -z "${steam_server}" ]; then
	read -p "Use steam server (y/n): " steam_server_response
	if [[ $steam_server_response == y* ]]
	then
		steam_server=true
	else
		steam_server=false
	fi
fi

if $steam_server; then
	launch_args="$launch_args -steam"

	if ! [[ "$launch_args" == *"-lobby"* ]]; then
		read -p "Select lobby type ([f]riends/[p]rivate): " lobby_type_response
		if [[ $lobby_type_response == f* ]]; then
			launch_args="$launch_args -lobby friends"
		else
			launch_args="$launch_args -lobby private"
		fi
	fi
fi

chmod +x ./LaunchUtils/ScriptCaller.sh
./LaunchUtils/ScriptCaller.sh $launch_args
