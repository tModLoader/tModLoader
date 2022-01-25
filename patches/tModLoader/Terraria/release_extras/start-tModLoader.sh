#!/usr/bin/env bash
cd "$(dirname "$0")"

chmod +x ./LaunchUtils/ScriptCaller.sh
./LaunchUtils/ScriptCaller.sh "$@"
