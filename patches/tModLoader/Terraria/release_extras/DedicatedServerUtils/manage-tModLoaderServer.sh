#!/usr/bin/env bash

#shellcheck disable=2164

# Only update the major version when a breaking change is introduced
script_version="3.0.0.1"
script_url="https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh"

# Shut up both commands
function pushd {
	command pushd "$@" > /dev/null || return
}

function popd {
	command popd > /dev/null || return
}

function try_make_link {
	if ! [[ -L "$2" ]]; then
		ln -s "$1" "$2"
	fi
}

# NOTE: There is seemingly no official documentation on this file but other more "official" software does this same check.
# See: https://github.com/moby/moby/blob/v24.0.5/libnetwork/drivers/bridge/setup_bridgenetfiltering.go#L162-L165
function is_in_docker {
	if ([[ -f /.dockerenv ]] || [[ -f /run/.containerenv ]]); then
		return 0
	fi
	return 1
}

function update_script {
	if is_in_docker; then
		return
	fi

	echo "Checking for script updates"

	# Go to where the script currently is
	pushd "$(dirname $(realpath "$0"))"

	latest_script_version=$({
		curl -s "$script_url" 2>/dev/null || wget -q -O- "$script_url";
	} | grep "script_version=" | head -n1 | cut -d '"' -f2)

	local new_version=$(echo -e "$script_version\n$latest_script_version" | sort -rV | head -n1)
	if [[ "$script_version" = "$new_version" ]]; then
		echo "No script update found"
		return
	fi

	if is_in_docker; then
		echo "There is a script update from v$script_version to v$new_version, consider rebuilding your Docker container for the updated script"
		return
	fi

	if [[ "${script_version:0:1}" != "${new_version:0:1}" ]]; then
		read -t 15 -p "A major version change has been detected (v$script_version -> v$new_version) Major versions mean incompatibilities with previous versions, so you should check the wiki for any updates to how the script works. Update anyways? (y/n): " update_major
		if [[ "$update_major" != [Yy]* ]]; then
			echo "Skipping major version update"
			return
		fi
	else
		read -t 10 -p "An update for the management script is available (v$script_version -> v$new_version). Update now? (y/n): " update_minor
		if [[ "$update_minor" != [Yy]* ]]; then
			echo "Skipping version update"
			return
		fi
	fi

	echo "Updating from version v$script_version to v$latest_script_version"
	curl -s -O "$script_url" 2>/dev/null || wget -q "$script_url"
	mv manage-tModLoaderServer.sh.1 manage-tModLoaderServer.sh

	popd
}

# Check PATH and flags for required commands for tml/mod installation
function verify_download_tools {
	if $steamcmd; then
		if [[ -v steamcmd_path ]]; then
			if [[ -f "$steamcmd_path" ]]; then
				# TODO: Should any checks be done here?
				steam_cmd="$steamcmd_path"
				echo "steamcmd found in steamcmdpath..."
			else
				echo "steamcmd.sh was not found at the provided path, please make sure it exists"
				exit 1
			fi
		else
			steam_cmd=$(command -v steamcmd)
			if [[ -z "$steam_cmd" ]]; then
				echo "steamcmd could not be found in PATH, please install steamcmd or provide --steamcmdpath"
				exit 1
			else
				echo "steamcmd found in PATH..."
			fi
		fi
	else
		if ! command -v unzip &> /dev/null; then
			echo "unzip could not be found on the PATH, please install unzip"
			exit 1
		else
			echo "unzip found..."
		fi
	fi
}

# If serverconfig doesn't exist, move the one from the server files. If it does delete the server files one
function move_serverconfig {
	if [[ -f "$folder/serverconfig.txt" ]]; then
		if [[ -f "serverconfig.txt" ]]; then
			echo "Removing duplicate server/serverconfig.txt"
			rm serverconfig.txt
		fi
	elif ! is_in_docker; then # Only move the server config if it's not in Docker
		echo "Moving default serverconfig.txt"
		mv serverconfig.txt "$folder"
	fi
}

# Gets version of tML to install from github, prioritizing --tml-version and tmlversion.txt
function get_version {
	if [[ -v tmlversion ]]; then
		echo "$tmlversion"
	elif [[ -r "$folder/Mods/tmlversion.txt" ]]; then
		# Format the tmlversion file appropriately, as it is missing padded 0's on months/days
		echo "v$(cat $folder/Mods/tmlversion.txt | sed -E "s/\.([0-9])\./\.0\1\./g")"
	else
		# Get the latest release if no other options are provided
		local release_url="https://api.github.com/repos/tModLoader/tModLoader/releases/latest"
		local latest_release
		latest_release=$({
			curl -s "$release_url" 2>/dev/null || wget -q -O- "$release_url";
		} | grep '"tag_name":' | sort | tail -1 | sed -E 's/.*"([^"]+)".*/\1/') # Get latest release from github's api
		echo "$latest_release" # So functions calling this can consume the result since you can't return strings in bash :)
	fi
}

# Takes version number as first parameter
function download_release {
	local down_url="https://github.com/tModLoader/tModLoader/releases/download/$1/tModLoader.zip"
	echo "Downloading version $1"
	curl -s -LJO "$down_url" 2>/dev/null || wget -q --content-disposition "$down_url"
	echo "Unzipping tModLoader.zip"
	unzip -q tModLoader.zip
	rm tModLoader.zip
	echo "$1" > .ver
}

function install_tml_github {
	echo "Installing TML from Github"
	local ver="$(get_version)"

	# If .ver exists we're doing an update instead, compare versions to see if it's already installed and backup if it isn't
	if [[ -r .ver ]]; then
		local oldver="$(cat .ver)"
		if [[ "$ver" = "$oldver" ]]; then
			echo "Current tModLoader $ver is up to date!"
			return
		fi

		echo "New version of tModLoader $ver is available, current version is $oldver"

		# Backup old tML versions in case something implodes
		mkdir "$oldver"
		for file in *; do
			if ! [[ "$file" = "manage-tModLoaderServer.sh" ]] && ! [[ "$file" = v*.tar.gz ]]; then
				mv "$file" "$oldver"
			fi
		done

		# Delete all backups but the most recent if we aren't keeping them
		if ! keep_backups; then
			echo "Removing old backups"
			for file in v*.tar.gz; do
				rm "$file"
				echo "Removed $file"
			done
		fi

		echo "Compressing $oldver backup"
		tar czf "$oldver.tar.gz" "$oldver"/*
		rm -r "$oldver"
	fi

	download_release "$ver"
}

function install_tml_steam {
	echo "Installing TML from Steam"

	if ! [[ -v username ]]; then
		read -p "Please enter a Steam username to login with: " username
	fi

	# Installs tML, but all other steam assets will be in $HOME/Steam or $HOME/.steam
	eval "$steam_cmd +force_install_dir $folder/server +login $username +app_update 1281930 +quit"

	if [[ $? = "5" ]]; then # Only recurse when not being used in the docker container.
		if ! is_in_docker; then
			echo "Try entering password/2fa code again"
			install_tml_steam
		else
			exit 5
		fi
	fi
}

function install_tml {
	mkdir -p server
	pushd server
	if $steamcmd; then
		install_tml_steam
	else
		install_tml_github
	fi

	move_serverconfig
	popd

	# Make folder structure
	if ! is_in_docker; then
		echo "Creating folder structure"
		mkdir -p Mods Worlds logs
	fi

	# Install .NET
	root_dir="$folder/server"
	LogFile="/dev/null"
	if [[ -f "$root_dir/LaunchUtils/DotNetVersion.sh" ]]; then
		source "$root_dir/LaunchUtils/DotNetVersion.sh"
		chmod a+x "$root_dir/LaunchUtils/InstallDotNet.sh" && bash $_
	else
		echo ".NET could not be pre-installed due to missing scripts. It should install on server start."

		# TODO: Right now Docker hard fails. How should we get this part of the launch utils to previous versions of TML that don't have it?
		if is_in_docker; then
			exit 1
		fi
	fi
}

function install_workshop_mods {
	if ! [[ -d "$folder/Mods" ]]; then
		echo "A tModLoader server has not been installed yet, please run the install or install-tml command before installing mods"
		exit
	fi

	pushd "$folder/Mods"

	if ! [[ -r install.txt ]]; then
		echo "No workshop mods to install"
		return
	fi

	steamcmd=true
	verify_download_tools

	echo "Installing workshop mods"

	local steamcmd_command
	lines=$(cat install.txt)
	for line in $lines; do
		steamcmd_command="$steamcmd_command +workshop_download_item 1281930 $line"
	done

	eval "$steam_cmd +force_install_dir $folder +login anonymous $steamcmd_command +quit"

	popd

	echo "Done"
}

function print_help {
	echo \
"tML dedicated server installation and maintenance script

Usage: script.sh COMMAND [OPTIONS]

Options:
 -h|--help           Show command line help
 -v|--version        Display the current version of the management script
 -g|--github         Download tML from Github instead of using steamcmd
 -f|--folder         The folder containing all of your server data (Mods, Worlds, serverconfig.txt, etc..)
 -u|--username       The steam username to login use when downloading tML. Not required to download mods
 --tml-version       The version of tML to download from Github. Needs to be an exact tag name (eg. "v2022.06.96.4")
 --steamcmdpath      Custom steamcmd path for users who do not have a installation in PATH.
 --keepbackups       Will keep all tML backups instead of the most recent one when updating

Commands:
 install-tml         Installs tModLoader from Steam (or Github if --github is provided)
 install-mods        Installs any mods from install.txt, if present. Requires steamcmd
 install             Alias for install-tml install-mods
 update              Alias for install
 start [args]        Launches the server and passes through any extra args
"
	exit
}

# Set SteamCMD by default. Checks are done to ensure it's installed and the user will be notified if any issues arise
steamcmd=true
keep_backups=false
start_args=""

# Check for updates to the script if it's not running in a Docker container
update_script

if [ $# -eq 0 ]; then # Check for commands
	echo "No command supplied"
	print_help
fi

# Covers cases where you only want to provide -h or -v without a command
cmd="$1"
if [[ "${cmd:0:1}" != "-" ]]; then
	shift
fi

while [[ $# -gt 0 ]]; do
	case $1 in
		-h|--help)
			print_help
			;;
		-v|--version)
			echo "tML Maintenance Tool v$script_version"
			exit
			;;
		-g|--github)
			steamcmd=false
			;;
		-u|--username)
			username="$2"
			shift
			;;
		-f|--folder)
			folder="$2"
			shift
			;;
		--tml-version)
			if [[ -n "$2" ]]; then
				tmlversion="$2"
				steamcmd=false
				shift
			fi
			;;
		--steamcmdpath)
			steamcmd_path="$2"
			shift
			;;
		--keepbackups)
			keep_backups=true
			;;
		*)
			start_args="$start_args $1"
			;;
	esac
	shift
done

# Set folder to the script's folder if it isn't set
if ! [[ -v folder ]]; then
	echo "Setting folder to current directory"
	folder="$(dirname $(realpath "$0"))"
fi

mkdir -p "$folder" && pushd $_

case $cmd in
	install-mods)
		install_workshop_mods
		;;
	install-tml)
		verify_download_tools
		install_tml
		;;
	install|update)
		verify_download_tools
		install_tml
		install_workshop_mods
		;;
	docker)
		if ! is_in_docker; then
			echo "The script is not running in a docker container, so the 'docker' command cannot be used"
			exit 1
		fi

		# Make proper directories to bypass install_workshop_mods warnings
		mkdir -p Mods Worlds

		install_workshop_mods

		# Link the server folder to the Docker installation and cli args for debugging (if it exists)
		try_make_link "$HOME/server" "$folder/server"

		# Also symlink banlist
		try_make_link "$folder/banlist.txt" "$folder/server/banlist.txt"

		# Provide option to use custom argsConfig file
		if [[ -f "$folder/cli-argsConfig.txt" ]]; then
			ln -s "$folder/cli-argsConfig.txt" "$folder/server/cli-argsConfig.txt"
		fi

		;&
	start)
		if ! [[ -f "$folder/server/start-tModLoaderServer.sh" ]]; then
			echo "A tModLoader server is not installed yet, please run the install or install-tml command before starting a server"
			exit 1
		fi

		# Link logs to a more convenient place
		mkdir -p "$folder/logs"
		try_make_link "$folder/logs" "$folder/server/tModLoader-Logs"

		# Link workshop to tMod dir so we don't need to pass -steamworkshopfolder
		try_make_link "$folder/steamapps" "$folder/server/steamapps"

		cd "$folder/server" || exit
		chmod u+x start-tModLoaderServer.sh
		./start-tModLoaderServer.sh -config "$folder/serverconfig.txt" -nosteam -tmlsavedirectory "$folder" "$start_args"
		;;
	*)
		echo "Invalid Command: $1"
		print_help
		;;
esac

popd
