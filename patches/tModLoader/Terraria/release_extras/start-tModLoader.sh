#!/usr/bin/env bash
cd "$(dirname "$0")"

chmod a+x ./LaunchUtils/ScriptCaller.sh
# forward our parent process id to the child in case ScriptCaller needs to kill the parent to break free of steam's process lifetime tracker (reaper)
PPID=$PPID ./LaunchUtils/ScriptCaller.sh "$@" &
