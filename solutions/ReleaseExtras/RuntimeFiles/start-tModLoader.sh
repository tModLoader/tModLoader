#!/bin/sh
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")
cd "$script_dir"

chmod +x InstallNetFramework.sh
./InstallNetFramework.sh
dotnet tModLoader.dll