#!/bin/bash
cd "$(dirname "$0")"
script_dir="$(pwd -P)"
launch_args="$*"

. ./LaunchUtils/ScriptCaller.sh false

dotnetV="6.0.0"
localNet="/dotnet/$dotnetV/dotnet"

if [ -f "$localNet" ]; then
  ./dotnet/$version/dotnet tModLoader.dll $launch_args
else
  dotnet tModLoader.dll $launch_args
fi
