#!/usr/bin/env bash
cd "$(dirname "$0")" ||
{ read -n 1 -s -r -p "Can't cd to script directory. Press any button to exit..." && exit 1; }

chmod a+x ./LaunchUtils/ScriptCaller.sh
./LaunchUtils/ScriptCaller.sh "$@" &
