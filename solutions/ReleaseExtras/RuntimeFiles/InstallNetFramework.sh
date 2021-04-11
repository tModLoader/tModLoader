#!/bin/sh
script_path=$(readlink -f "$0")

script_dir=$(dirname "$script_path")

cd "$script_dir"

channel="5.0"
version="5.0.5"
dotnet_dir="$script_dir/dotnet"
install_dir="$dotnet_dir/$version"

if [ -d "$dotnet_dir" ]; then
  for folder in $(ls $script_dir/dotnet/); do
    if [ ! $version = "$folder" ]; then
      old_version="$script_dir/dotnet/$folder"
      echo "Cleaning $old_version"
      rm -rf "$old_version"
    fi
  done
fi

if [ ! -d "$install_dir" ]; then
  curl -sLo dotnet-install.sh https://dot.net/v1/dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --channel "$channel" --install-dir "$install_dir" --runtime "dotnet" --version "$version"
fi
