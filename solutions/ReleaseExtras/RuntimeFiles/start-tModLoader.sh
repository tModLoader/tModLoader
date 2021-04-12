#!/bin/sh
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")
cd "$script_dir"

. InstallNetFramework.sh

dotnet tModLoader.dll