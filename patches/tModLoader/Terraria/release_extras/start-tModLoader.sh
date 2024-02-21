#!/usr/bin/env bash
cd "$(dirname "$0")"

chmod a+x ./LaunchUtils/ScriptCaller.sh
./LaunchUtils/ScriptCaller.sh "$@" &
