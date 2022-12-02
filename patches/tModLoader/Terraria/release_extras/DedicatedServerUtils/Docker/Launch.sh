#!/bin/bash
#shellcheck disable=2164

# Installing/updating mods
mkdir -p ~/.local/share/Terraria
./manage-tModLoaderServer.sh -u --mods-only --check-dir ~/.local/share/Terraria --folder ~/.local/share/Terraria/wsmods

# Symlink tML's local dotnet install so that it can persist through runs
mkdir -p ~/.local/share/Terraria/dotnet
ln -s /home/tml/.local/share/Terraria/dotnet/ /home/tml/tModLoader/dotnet

echo "Launching tModLoader..."
cd ~/tModLoader
# Maybe eventually steamcmd will allow for an actual steamserver. For now -nosteam is required.
exec ./start-tModLoaderServer.sh -config $HOME/.local/share/Terraria/serverconfig.txt -nosteam -steamworkshopfolder $HOME/.local/share/Terraria/wsmods/steamapps/workshop
