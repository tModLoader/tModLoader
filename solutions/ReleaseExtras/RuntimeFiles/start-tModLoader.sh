#!/bin/sh
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")
cd "$script_dir"

. InstallNetFramework.sh

NetFramework\dotnet\5.0.0\dotnet tModLoader.dll