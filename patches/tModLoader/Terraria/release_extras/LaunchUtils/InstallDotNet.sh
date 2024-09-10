#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#
# THIS SCRIPT EXPECTS AS INPUTS:
#   dotnet_dir     : Folder where the versions of dotnet will be extracted
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
#If the dotnet dir exists, we may need to do some cleanup
#We delete any of the following:
#   1) dotnet/#.#.# folders
#   2) folders containing dotnet/shared/Microsoft.NETCore.App/#.#.# where #.#.# != dotnet_version
reg1='^[0-9]+'
if [ -d "$dotnet_dir" ]; then
	# Find all folders inside the dotnet dir that don't match our target version and nuke it
	for folder in $(ls "$dotnet_dir"); do
		if [[ $folder =~ $reg1 ]] ; then
			old_version="$dotnet_dir/$folder"	
			echo "Removing Legacy Dotnet install $old_version"
			rm -rf "$old_version"
		else 
			for subfolder in $(ls "$dotnet_dir/shared/Microsoft.NETCore.App"); do
				if [ ! $dotnet_version = "$subfolder" ]; then
					old_version="$dotnet_dir/$folder"
					echo "Cleaning $old_version"
					rm -rf "$old_version"
				fi
			done
		fi
	done
else
	mkdir "$dotnet_dir"
fi
echo "Cleanup Complete"

echo "Checking dotnet install..."
if [[ ! -f "$dotnet_dir/dotnet.exe" && "$_uname" == *"_NT"* && "$(uname -m)" == "x86_64" ]]; then
	echo "Update Required. Checking for Windows pre-deployed x64 dotnet files"
	# Allow for zip to be already delivered by steam win on system and placed in the root directory with this name convention:
	dotnet_portable_archive_name="dotnet-runtime-$dotnet_version-win-x64.zip"
	dotnet_portable_archive="$dotnet_portable_archive_name"
	
	# @TODO: Needs more thought. Seems wasteful of disk space. Seems required as installers on windows are just not 100% in all setups
	if [ -f "$dotnet_portable_archive" ]; then
		echo "Found \"$dotnet_portable_archive_name\""
		echo "Extracting..."
		unzip -qq "$dotnet_portable_archive" -d "$dotnet_dir"
		# Do not auto-delete if already present to avoid steam file checks to fail and redownload it
	else
		echo "None Found. Attempting downloading win x64 portable dotnet runtime directly..."
		echo "This can take up to 5 minutes"
		file_download "$dotnet_dir/$dotnet_version.zip" "https://dotnetcli.azureedge.net/dotnet/Runtime/$dotnet_version/dotnet-runtime-$dotnet_version-win-x64.zip"

		echo "Extracting..."
		unzip -qq "$dotnet_dir/$dotnet_version" -d "$dotnet_dir"
		# Will get cleaned up in the Cleaning step on next run. We don't want to use more disk space than we need
	fi
fi

#If the installed dotnet for this specific dotnet version still doesnt exist, grab the official installer script and run it.
if [[ ! -f "$dotnet_dir/dotnet" && ! -f "$dotnet_dir/dotnet.exe" ]]; then
	echo "Update Required. Will now attempt downloading using official scripts."
	echo "This can take up to 5 minutes"
	if [[ "$_uname" == *"_NT"* ]]; then
			# Intended as a catchall for all Windows architectures. This doesn't work on Win7. 
			# Neither of Win7_32 or Win7_Arm are expected, so we just keep this as is
			file_download dotnet-install.ps1 https://dot.net/v1/dotnet-install.ps1
			
			if [ ! -f dotnet-install.ps1 ]; then
				echo "Failed to download dotnet-install.ps1. Relying on Powershell to work"
				powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -Channel $channel -InstallDir \"$dotnet_dir\" -Version $dotnet_version -Runtime dotnet"
			else
				powershell.exe -NoProfile -ExecutionPolicy unrestricted -File dotnet-install.ps1 -Channel "$channel" -InstallDir "$dotnet_dir" -Runtime "dotnet" -Version "$dotnet_version"
			fi
	else
		# *nix binaries are various and not worth detecting the required one here, always use "on-the-fly" script install
		file_download dotnet-install.sh https://dot.net/v1/dotnet-install.sh

		run_script ./dotnet-install.sh --channel "$channel" --install-dir "$dotnet_dir" --runtime "dotnet" --version "$dotnet_version" --verbose
	fi
fi

echo "Dotnet should be present in \"$dotnet_dir\""
if [[ ! -f "$dotnet_dir/dotnet" && ! -f "$dotnet_dir/dotnet.exe" ]]; then
	echo -e "${TXTCOLOR_RED}Download of portable dotnet runtime seems to have failed,${TXTCOLOR_NC}"
	echo -e "${TXTCOLOR_RED}Proceeding will probably fall back to system wide installed runtime${TXTCOLOR_NC}"
fi
