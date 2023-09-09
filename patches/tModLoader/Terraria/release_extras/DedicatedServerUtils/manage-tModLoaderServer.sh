#!/usr/bin/env bash

#shellcheck disable=2164

script_version="3.0.0.0"
script_url="https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh"

# Shut up both commands
function pushd {
	command pushd "$@" > /dev/null || return
}

function popd {
	command popd > /dev/null || return
}

# NOTE: There is seemingly no official documentation on this file but other more "official" software does this same check.
# See: https://github.com/moby/moby/blob/v24.0.5/libnetwork/drivers/bridge/setup_bridgenetfiltering.go#L162-L165
function is_in_docker {
	if [[ -f /.dockerenv ]]; then
		return 0
	fi
	return 1
}

function update_script {
	if [[ -z "$1" ]]; then
		read -t 5 -p "Would you like to check for script updates? (y/n): " update_now
		if [[ "$update_now" = [Yy]* ]]; then
			echo "Checking for updates"
			update_script
		else
			echo "Not updating"
			return
		fi
	fi

	# Go to where the script currently is
	pushd "$(dirname $(realpath "$0"))"

	latest_script_version=$({
		curl -s "$script_url" 2>/dev/null || wget -q -O- "$script_url";
	} | grep "script_version=" | head -n1 | cut -d '"' -f2)

	local new_version=$(echo -e "$script_version\n$latest_script_version" | sort -rV | head -n1)
	if [[ "$script_version" = "$new_version" ]]; then
		echo "No version change detected"
		exit 0
	fi
	
	if [[ "${script_version:0:1}" != "${new_version:0:1}" ]]; then
		read -t 15 -p "A major version change has been detected ($script_version -> $new_version) Major versions mean incompatibilities with previous versions, so you should check the wiki for any updates to how the script works. Update anyways? (y/n): " update_major
		if [[ "$update_major" != [Yy]* ]]; then
			echo "Skipping major version update"
			exit 0
		fi
	fi

	echo "Updating from version v$script_version to v$latest_script_version"
	curl -s -O "$script_url" 2>/dev/null || wget -q "$script_url"
	mv manage-tModLoaderServer.sh.1 manage-tModLoaderServer.sh

	popd
}

# Check PATH and flags for required commands for tml/mod installation
function verify_download_tools {
	if [[ -v steamcmd_path ]]; then
		if [[ -f "$steamcmd_path" ]]; then
			# TODO: Should any checks be done here?
			steam_cmd="$steamcmd_path"
			echo "steamcmd found in folder..."
		else
			echo "steamcmd.sh was not found at the provided path, please make sure it exists"
		fi
	else
		steam_cmd=$(command -v steamcmd)
		if [[ -z "$steam_cmd" ]]; then
			echo "steamcmd could not be found in PATH, please install steamcmd or provide --steamcmdpath"
		else
			echo "steamcmd found in PATH..."
		fi
	fi

	if ! command -v unzip &> /dev/null; then
		echo "unzip could not be found on the PATH, please install unzip"
		exit 1
	else
		echo "unzip found..."
	fi
}

function install_dotnet {
	pushd server

	if ! [[ -r tModLoader.runtimeconfig.json ]]; then
		echo "tModLoader not installed or missing files. Quitting..."
		exit 1
	fi

	echo "Installing dotnet..."
	dotnet_version=$(sed -n 's/^.*"version": "\(.*\)"/\1/p' < $folder/server/tModLoader.runtimeconfig.json)
	export dotnet_version=${dotnet_version%$'\r'} # Remove carriage return, see ScriptCaller.sh
	export dotnet_dir="$folder/server/dotnet"
	if [[ -n "$IS_WSL" || -n "$WSL_DISTRO_NAME" ]]; then
		echo "wsl detected. Setting dotnet_dir=dotnet_wsl"
		export dotnet_dir="$folder/server/dotnet_wsl"
	fi
	export install_dir="$dotnet_dir/$dotnet_version"
	chmod +x "$folder/server/LaunchUtils/InstallNetFramework.sh" && bash $_

	popd
}

# Gets version of tML to install from github, prioritizing --tml-version and tmlversion.txt
function get_version {
	if [[ -v tmlversion ]]; then
		echo "$tmlversion"
	elif [[ -r "$folder/tmlversion.txt" ]]; then
		# Format the tmlversion file appropriately, as it is missing padded 0's on months/days
		echo "v$(cat $folder/tmlversion.txt | sed -E "s/\.([0-9])\./\.0\1\./g")"
	else
		# Get the latest release if no other options are provided
		local release_url="https://api.github.com/repos/tModLoader/tModLoader/releases"
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

function update_tml_github {
	pushd server

	if ! [[ -r .ver ]]; then
		echo "tModLoader is not installed"
		exit 1
	fi

	local ver="$(get_version)"
	local oldver="$(cat .ver)"
	if [[ "$ver" = "$oldver" ]]; then
		echo "No version change of tModLoader available"
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

	download_release "$ver"
	popd
}

function install_tml_github {
	pushd server

	# Check for an empty directory, skipping checks on any backed up versions
	# TODO: This throws an error in the Docker container. It's not important since no backups are kept but it should be fixed eventually.
	if [[ -n "$(ls -A --ignore='v*.tar.gz' .)" ]]; then
		echo "Install directory not empty, please make sure your $folder/server directory is empty or run update to update an existing installation"
		exit 1
	fi

	# Install tml from github, leave some file containing what version
	download_release "$(get_version)"
	popd
}

function install_tml_steam {
	if ! [[ -v steam_cmd ]]; then
		echo "SteamCMD not found, tModLoader cannot be installed from Steam"
		exit 1
	fi

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

function install_workshop_mods {
	# If the user forgot to move enabled.json, move it for them
	if [[ -r enabled.json ]]; then
		mkdir Mods 2>/dev/null && mv enabled.json Mods/enabled.json
	fi

	if ! [[ -r install.txt ]]; then
		echo "No workshop mods to install"
		return
	fi

	if ! [[ -v steam_cmd ]]; then
		echo "SteamCMD not found, no workshop mods will be installed or updated"
		return
	fi

	echo Installing workshop mods

	local steamcmd_command
	lines=$(cat install.txt)
	for line in $lines; do
		steamcmd_command="$steamcmd_command +workshop_download_item 1281930 $line"
	done

	eval "$steam_cmd +force_install_dir $folder +login anonymous $steamcmd_command +quit"
}

function print_version {
	echo "tML Maintenance Tool: v$script_version"

	if ! $steamcmd; then
		echo "tML Install: $(cat $folder/server/.ver 2>/dev/null || echo none)"
	fi

	exit
}

function print_help {
	echo \
"tML dedicated server installation and maintenance script

Usage: script.sh COMMAND [OPTIONS]

Options:
 -h|--help           Show command line help.
 -v|--version        Display the current version of the tool and a tModLoader Github install.
 -g|--github         Use the binary off of Github instead of using steamcmd.
 -f|--folder         The folder containing all of your server data (Mods, Mods/enabled.json, Worlds, install.txt).
 -u|--username       The steam username to login with. Only applies when using steamcmd.
 --tml-version       The version of tML to download. Only applies when using Github. This should be the exact tag off of Github (ex. v2022.06.96.4).
 --skip-tml          Only install/update mods.
 --skip-mods         Only install/update tModLoader.
 --steamcmdpath      Custom SteamCMD path for users who do not have a installation in PATH. This should point to the steamcmd.sh script.
 --keepbackups       Will save every tML backup instead of the most recent one.

Commands:
 help                Equivalent to --help.
 install             Installs tModLoader and any mods provided. Will copy any world files, will not overwrite any existing ones.
 update              Updates an existing tModLoader installation and its mods.
 start               Launches the server with no updating or installing of mods. This should be run after one of the above commands.
 uninstall           Uninstalls the current tML installation, removing ALL server files and workshop mods.
 update-script       Update the script to the latest version on Github.
"
	exit
}

# Set SteamCMD by default. Checks are done to ensure it's installed and the user will be notified if any issues arise
steamcmd=true
skip_mods=false
skip_tml=false
keep_backups=false

if [ $# -eq 0 ]; then # Check for commands
	echo "No command supplied"
	print_help
fi

cmd="$1"
shift

# TODO: Currently --version MUST be provided after --folder and --github to get a proper version print... which is inconvenient
while [[ $# -gt 0 ]]; do
	case $1 in
		-h|--help)
			print_help
			;;
		-v|--version)
			print_version
			;;
		-g|--github)
			steamcmd=false
			shift
			;;
		-u|--username)
			username="$2"
			shift; shift
			;;
		-f|--folder)
			folder="$2"
			shift; shift;
			;;
		--tml-version)
			tmlversion="$2"
			steamcmd=false
			shift; shift
			;;
		--skip-tml)
			skip_tml=true
			shift
			;;
		--skip-mods)
			skip_mods=true
			shift
			;;
		--steamcmdpath)
			steamcmd_path="$2"
			shift; shift
			;;
		--keepbackups)
			keep_backups=true
			shift
			;;
		*)
			echo "Invalid Argument: $1"
			print_help
			;;
	esac
done

# Set folder to the script's folder if it isn't set
if ! [[ -v folder ]]; then
	echo "Setting folder to current directory"
	folder="$(dirname $(realpath "$0"))"
fi

mkdir -p "$folder" && pushd "$folder"

case $cmd in
	help)
		print_help
		;;
	install|update)
		verify_download_tools

		if ! $skip_tml; then
			mkdir -p "$folder/server"
			if $steamcmd; then
				install_tml_steam
			elif [[ "$cmd" = "install" ]]; then
				install_tml_github
			else
				update_tml_github
			fi
		fi

		install_dotnet

		if ! $skip_mods; then
			install_workshop_mods
		fi
		;;
	uninstall)
		read -t 5 -p "This will delete ALL server files and workshop mods but will keep local Mod/World Data. Uninstall now? (y/n): " uninstall_now
		if [[ "$uninstall_now" = [Yy]* ]]; then
			echo "Uninstalled tML server"
			rm -r "$folder/server" "$folder/steamapps" "$folder/logs"
		else
			echo "Cancelled"
		fi
		;;
	update-script)
		update_script 1
		;;
	docker)
		if ! is_in_docker; then
			echo "The script is not running in a docker container, so the 'docker' command cannot be used"
			exit 1
		fi

		verify_download_tools
		install_workshop_mods

		# Link the server folder to the Docker installation and cli args for debugging (if it exists)
		if ! [[ -L "$folder/server" ]]; then
			ln -s "$HOME/server" "$folder/server"
		fi

		if [[ -f "$folder/cli-argsConfig.txt" ]]; then
			ln -s "$folder/cli-argsConfig.txt" "$folder/server/cli-argsConfig.txt"
		fi

		;&
	start)
		# Link logs to a more convenient place
		if ! [[ -d "$folder/logs" ]]; then
			mkdir -p "$folder/logs" && ln -s $_ "$folder/server/tModLoader-Logs"
		fi

		# Link workshop to tMod dir so we don't need to pass -steamworkshopfolder
		if ! [[ -L "$folder/server/steamapps" ]]; then
			ln -s "$folder/steamapps" "$folder/server/steamapps"
		fi

		cd "$folder/server" || exit
		chmod u+x start-tModLoaderServer.sh
		./start-tModLoaderServer.sh -config "$folder/serverconfig.txt" -nosteam -tmlsavedirectory "$folder"
		;;
	*)
		echo "Invalid Command: $1"
		print_help
		;;
esac

popd

# Check for updates to the script if it's not running in a Docker container
if ! is_in_docker; then
	update_script
fi
