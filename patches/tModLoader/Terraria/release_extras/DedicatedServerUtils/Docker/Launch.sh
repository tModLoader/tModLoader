#!/usr/bin/env bash
# NOTE: Only for use within the docker container.

function pseudoStdin {
	tmux send-keys "$1" Enter
}

function stop {
	pseudoStdin "exit"
	pid=$(pgrep dotnet)
	while [[ -e /proc/$pid ]]
	do
		sleep .5
	done
	pkill tmux
}

steamcmd=true
while [[ $# -gt 0 ]];
do
	case $1 in
		-l|--location)
			location="$2" # Full path to tML folder
			shift; shift
			;;
		-g|--github)
			steamcmd=false
			shift
			;;
		--username)
			username="$2"
			shift; shift
			;;
		--lobbytype)
			lobbyType="2"
			shift; shift
			;;
	esac
done

trap stop SIGTERM SIGINT

scriptdir=/home/tml/.local/share/Terraria/scripdir

echo "Checking updates"
$scriptdir/Setup_tModLoaderServer.sh --updatescript

if $steamcmd
then
	$scriptdir/Setup_tModLoaderServer.sh -u --username "$username"
else
	$scriptdir/Setup_tModLoaderServer.sh -u -g
fi

echo "Launching tModLoader..."
cd "$location"
tmux new-session -d "./start-tModLoaderServer.sh -config $HOME/.local/share/Terraria/scriptdir/serverconfig.txt"

wait ${!}
