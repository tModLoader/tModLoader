#!/usr/bin/env bash
cd "$(dirname "$(realpath "$0")")"

chmod +x ./LaunchUtils/ScriptCaller.sh
./LaunchUtils/ScriptCaller.sh $*
