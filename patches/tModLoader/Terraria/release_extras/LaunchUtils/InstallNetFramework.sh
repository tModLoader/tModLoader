#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#
# THIS SCRIPT EXPECTS AS INPUTS:
#   dotnet_dir     : Folder where the versions of dotnet will be extracted
#   install_dir    : Target folder for the install (usually "$dotnet_dir/$version")
#   dotnet_version : Version of dotnet to install
#

#chdir to path of the script and save it
cd "$(dirname "$0")"
. ./BashUtils.sh

TXTCOLOR_RED='\033[31m'
TXTCOLOR_NC='\033[0m'

#Cut everything before the second dot
channel=$(echo "$dotnet_version" | cut -f1,2 -d'.')

echo "Checking for old .NET versions to remove from folder"
#If the dotnet dir exists, we need to do some cleanup
if [ -d "$dotnet_dir" ]; then
	# Find all folders inside the dotnet dir that don't match our target version and nuke it
	for folder in $(ls "$dotnet_dir"); do
		if [ ! $dotnet_version = "$folder" ]; then
			old_version="$dotnet_dir/$folder"
			echo "Cleaning $old_version"
			rm -rf "$old_version"
		fi
	done
fi
echo "Cleanup Complete"

#If the install directory for this specific dotnet version doesnt exist, grab the installer script and run it.
echo "Checking dotnet install..."
if [ ! -d "$install_dir" ]; then
	echo "Update Required. Will now attempt downloading. This can take up to 5 minutes"
	dotnet_runtime=dotnet

	mkdir "$dotnet_dir"

	if [[ "$_uname" == *"_NT"* ]]; then
		# Allow for zip to be already delivered by steam win on system
		# and placed in the root directory with this name convention:
		dotnet_portable_archive_name="dotnet-runtime-$dotnet_version-win-x64.zip"
		dotnet_portable_archive="$root_dir/$dotnet_portable_archive_name"

		is_win_64=0
		if [ "$(uname -m)" == "x86_64" ]; then
			is_win_64=1
		fi

		# First check if zip is already delivered
		if [[ "$is_win_64" == 1 && -f "$dotnet_portable_archive" ]]; then
			echo "Found \"$dotnet_portable_archive_name\""
			echo "Extracting..."
			unzip "$dotnet_portable_archive" -d "$install_dir"
			
			# Do not auto-delete if already present to avoid
			# steam file checks to fail and redownload it
		fi

		# If failed run powershell script to install
		if [[ ! -f "$install_dir/dotnet" && ! -f "$install_dir/dotnet.exe" ]]; then
			echo "Trying to install portable dotnet with powershell script (will probably fail on old PS < 4)..."
			file_download dotnet-install.ps1 https://dot.net/v1/dotnet-install.ps1

			# @TODO: Should update powershell to 4+ because required by the script (and not present by default in win 7/8)
			powershell.exe -NoProfile -ExecutionPolicy unrestricted -File dotnet-install.ps1 -Channel "$channel" -InstallDir "$install_dir" -Runtime "$dotnet_runtime" -Version "$dotnet_version"
		fi

		# If failed download supposing x64
		if [[ "$is_win_64" == 1 && ! -f "$install_dir/dotnet" && ! -f "$install_dir/dotnet.exe" ]]; then
			echo "Powershell script failed, downloading win x64 portable dotnet runtime manually..."
			file_download "$dotnet_portable_archive" "https://dotnetcli.azureedge.net/dotnet/Runtime/$dotnet_version/dotnet-runtime-$dotnet_version-win-x64.zip"

			echo "Extracting..."
			unzip "$dotnet_portable_archive" -d "$install_dir"

			echo "Removing temporary downloaded archive"
			rm "$dotnet_portable_archive"
		fi
	else
		# *nix binaries are various and not worth detecting the required one here, always use "on-the-fly" script install
		file_download dotnet-install.sh https://dot.net/v1/dotnet-install.sh

		run_script ./dotnet-install.sh --channel "$channel" --install-dir "$install_dir" --runtime "$dotnet_runtime" --version "$dotnet_version"
	fi
fi
echo "Dotnet should be present in \"$install_dir\""

if [[ ! -f "$install_dir/dotnet" && ! -f "$install_dir/dotnet.exe" ]]; then
	echo -e "${TXTCOLOR_RED}Download of portable dotnet runtime seems to have failed,${TXTCOLOR_NC}"
	echo -e "${TXTCOLOR_RED}proceeding will probably use system wide installed runtime${TXTCOLOR_NC}"
	read -p "Please press Enter to try to run the game anyway... "
fi
