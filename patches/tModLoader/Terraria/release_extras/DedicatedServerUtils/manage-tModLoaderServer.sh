#!/usr/bin/env bash

#shellcheck disable=2164
#shellcheck disable=2155

# Only update the major version when a breaking change is introduced
script_version="4.0.0.1"
script_url="https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh"

# Shut up both commands
function pushd {
	command pushd "$@" > /dev/null || return
}

function popd {
	command popd > /dev/null || return
}

# Copied from BashUtils.sh so there's no need for a dependency on it
function machine_has {
	command -v "$1" > /dev/null 2>&1
	return $?
}

# There is seemingly no official documentation on this file but other "official" software does this same check.
# See: https://github.com/moby/moby/blob/v27.4.0/libnetwork/drivers/bridge/setup_bridgenetfiltering.go#L92-L95
function is_in_docker {
	if [[ -v ISDOCKER ]] || [[ -f /.dockerenv ]] || [[ -f /run/.containerenv ]]; then
		return 0
	fi
	return 1
}

function update_script {
	latest_script_version=$(curl -s "$script_url" 2>/dev/null | grep "script_version=" | head -n1 | cut -d '"' -f2)

	local new_version=$(echo -e "$script_version\n$latest_script_version" | sort -rV | head -n1)
	if [[ "$script_version" = "$new_version" ]]; then
		echo "No script update found"
		return
	fi

	if is_in_docker; then
		echo "The management script has been updated, please rebuild your Docker container to get the updated script!"
		return
	fi

	if [[ "${script_version:0:1}" != "${new_version:0:1}" ]]; then
		read -t 20 -p "A major version change has been detected (v$script_version -> v$new_version) Major versions mean incompatibilities with previous versions, so you should check the wiki for any updates to how the script works. Update anyways? (y/n): " update_major
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

	# Go to where the script currently is
	pushd "$(dirname "$(realpath "$0")")"

	echo "Updating from version v$script_version to v$latest_script_version"
	curl -s -O "$script_url" || exit 1
	mv manage-tModLoaderServer.sh.1 manage-tModLoaderServer.sh

	popd
}

# Check PATH and flags for required commands for tml/mod installation
function verify_steamcmd {
	# Prioritize an ENV Variable
	if [[ -v STEAMCMDPATH ]]; then
		if ! [[ -f "$STEAMCMDPATH" ]]; then
			echo "STEAMCMDPATH is set to a file that does not exist"
			exit 1
		fi
		steam_cmd="$STEAMCMDPATH"
		return
	fi

	if [[ -v steamcmd_path ]]; then
		if ! [[ -f "$steamcmd_path" ]]; then
			echo "--steamcmdpath is set to a file that does not exist"
			exit 1
		fi
		steam_cmd="$steamcmd_path"
		return
	fi

	steam_cmd=$(command -v steamcmd)
	if [[ -z "$steam_cmd" ]]; then
		echo "steamcmd could not be found in PATH, please install steamcmd or provide the STEAMCMDPATH environment variable"
		exit 1
	fi
}

function get_version {
	if [[ -v TMLVERSION ]]; then
		echo "$TMLVERSION"
	elif [[ -v tml_version ]]; then
		echo "$tml_version"
	elif [[ -r "$folder/Mods/tmlversion.txt" ]]; then
		# Format the tmlversion file appropriately, as it is missing padded 0's on months/days
		echo "v$(cat "$folder/Mods/tmlversion.txt" | sed -E "s/\.([0-9])\./\.0\1\./g")"
	else
		# Get the latest release if no other options are provided
		local release_url="https://api.github.com/repos/tModLoader/tModLoader/releases/latest"
		local latest_release
		latest_release=$(curl -s "$release_url" 2>/dev/null | grep '"tag_name":' | sort | tail -1 | sed -E 's/.*"([^"]+)".*/\1/') # Get latest release from github's api
		echo "$latest_release"
	fi
}

function install_tml_github {
	echo "Installing TML from Github"
	local ver="$(get_version)"

	# Allow nullglob so taht if "v*.tar.gz" matches no entries it doesnt try to remove the literal name
	shopt -s nullglob

	# If .ver exists we're doing an update instead, compare versions to see if it's already installed and backup if it isn't
	if [[ -r .ver ]]; then
		local oldver="$(cat .ver)"
		if [[ "$ver" = "$oldver" ]]; then
			echo "Current tModLoader version ($ver) is up to date!"
			return
		fi

		echo "New version $ver is wanted, current version is $oldver"

		# Backup old tML versions in case something implodes
		mkdir "$oldver"
		for file in *; do
			if ! [[ "$file" = "manage-tModLoaderServer.sh" ]] && ! [[ "$file" = v*.tar.gz ]] && ! [[ "$file" = "$oldver" ]]; then
				mv "$file" "$oldver" || exit 1
			fi
		done

		# Delete all backups but the most recent if we aren't keeping them
		if ! $keep_backups; then
			echo "Removing old backups"
			for file in v*.tar.gz; do
				rm "$file" || exit 1
				echo "Removed old version $file"
			done
		fi

		echo "Compressing $oldver backup"
		tar czf "$oldver.tar.gz" "$oldver"/*
		rm -r "$oldver"
	fi

	shopt -u nullglob

	echo "Downloading version $ver"
	curl -s -LJO "https://github.com/tModLoader/tModLoader/releases/download/$ver/tModLoader.zip" || exit 1
	echo "Unzipping tModLoader.zip"
	unzip -q tModLoader.zip
	rm tModLoader.zip
	echo "$ver" > .ver
}

function install_tml_steam {
	echo "Installing TML from Steam"

	if ! [[ -v username ]]; then
		echo "Provide the --username flag in order to download TML from Steam"
		exit 1
	fi

	# Installs tML, but all other steam assets will be in $HOME/Steam or $HOME/.steam
	eval "$steam_cmd +force_install_dir $folder/server +login $username +app_update 1281930 +quit"

	if [[ $? = "5" ]]; then
		echo "Try entering password/2fa code again"
		install_tml_steam
	fi
}

function install_tml {
	mkdir -p server
	pushd server

	if $github; then
		install_tml_github
	else
		verify_steamcmd
		install_tml_steam
	fi

	if [[ -f "$folder/serverconfig.txt" ]]; then
		if [[ -f "serverconfig.txt" ]]; then
			echo "Removing duplicate serverconfig.txt"
			rm serverconfig.txt
		fi
	fi

	popd

	if ! is_in_docker; then
		echo "Creating folder structure"
		mkdir -p Mods Worlds
	fi

	# Install .NET
	root_dir="$folder/server"
	LogFile="$folder/server/tModLoader-Logs/DotNet.log"
	if [[ -f "$root_dir/LaunchUtils/DotNetVersion.sh" ]]; then
		. "$root_dir/LaunchUtils/DotNetVersion.sh"
		chmod a+x "$root_dir/LaunchUtils/InstallDotNet.sh" && bash "$_"
	else
		echo "WARNING: .NET could not be pre-installed due to missing scripts. It should install on server start."
	fi
}

function install_workshop_mods {
	verify_steamcmd
	
	if ! [[ -d "$folder/Mods" ]]; then
		echo "Mods folder does not exist, please run install-tml command or create a 'Mods' folder before installing mods"
		exit
	fi

	pushd Mods

	if ! [[ -r install.txt ]]; then
		echo "No workshop mods to install"
		popd
		return
	fi

	echo "Installing workshop mods"

	local steamcmd_command
	lines=$(cat install.txt)
	for line in $lines; do
		steamcmd_command="$steamcmd_command +workshop_download_item 1281930 $line"
	done

	eval "$steam_cmd +force_install_dir $folder +login anonymous $steamcmd_command +quit"

	popd

	echo "Installed $(wc -l $lines) mods"
}

function print_help {
	echo \
"tML dedicated server installation and maintenance script

Usage: script.sh COMMAND [OPTIONS]

ENV Variables:
 STEAMCMDPATH        Custom path for the steamcmd binary if your package manager does not have it
 TMLVERSION          TML version to download. By default this is the latest release

Options:
 -h|--help           Show command line help
 -v|--version        Display the current version of the management script
 -g|--github         Download tML from Github instead of using steamcmd
 -f|--folder         The folder containing all of your server data (Mods, Worlds, serverconfig.txt, etc..)
 -u|--username       The steam username to login use when downloading tML. Not required to download mods
 --keepbackups       When installing with --github, keep all previous versions instead of deleting them when updating
 --tmlversion        Version of tModLoader to install. Only works if --github is provided. Functionally equivalent to the TMLVERSION env variable
 --steamcmdpath      Path to steamcmd.sh for Steam tModLoader mod installation. Functionally equivalent to the STEAMCMDPATH env variable
 --tml-version       DEPRECATED: Kept for compatibility, but the --tmlversion flag or the TMLVERSION environment variable should be used instead

Commands:
 install-tml         Installs tModLoader from Steam (or Github if --github is provided)
 install-mods        Installs any mods from install.txt, if present. Requires steamcmd
 install             Alias for install-tml install-mods
 start [args]        Launches the server and passes through any extra args
"
	exit
}

github=false
keep_backups=false
start_args=""

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
			echo "tML Dedicated Server Tool v$script_version"
			exit
			;;
		-g|--github)
			github=true
			;;
		-f|--folder)
			folder="$2"
			shift
			;;
		-u|--username)
			username="$2"
			shift
			;;
		--keepbackups)
			keep_backups=true
			;;
		--tmlversion|--tml-version)
			tml_version="$2"
			github=true
			shift
			;;
		--steamcmdpath)
			steamcmd_path="$2"
			shift
			;;
		*)
			start_args="$start_args $1"
			;;
	esac
	shift
done

if ! machine_has "curl"; then
	echo "curl must be installed for the management script to work"
	exit 1
fi

update_script

if ! [[ -v folder ]]; then
	echo "Setting folder to current directory"
	folder="$(dirname "$(realpath "$0")")"
fi

mkdir -p "$folder" && pushd "$_"

case $cmd in
	update-script)
		# NOOP because the script automatically checks for an update before thiss
		;;
	install-mods)
		install_workshop_mods
		;;
	install-tml)
		install_tml
		;;
	install)
		install_tml
		install_workshop_mods
		;;
	start)
		# Edge-case for ScriptCaller.sh where dotnet exists but TML logs don't yet
		export SKIP_DOTNET_LOGCHECK=1

		if is_in_docker; then
			mkdir -p "$folder/Mods" "$folder/Worlds"
			install_workshop_mods
		elif ! [[ -f "$folder/server/start-tModLoaderServer.sh" ]]; then
			echo "A tModLoader server is not installed yet, please run the install or install-tml command before starting a server"
			exit 1
		fi

		if is_in_docker; then
			cd "$HOME/server" || exit
			chmod +x ./LaunchUtils/ScriptCaller.sh
      			source ./LaunchUtils/ScriptCaller.sh -server -config "$folder/serverconfig.txt" -steamworkshopfolder "$folder/steamapps/workshop" -tmlsavedirectory "$folder" $start_args
		else
			cd "$folder/server" || exit
			chmod +x start-tModLoaderServer.sh
			./start-tModLoaderServer.sh -nosteam -config "$folder/serverconfig.txt" -steamworkshopfolder "$folder/steamapps/workshop" -tmlsavedirectory "$folder" "$start_args"
		fi
		;;
	*)
		echo "Invalid Command: $1"
		print_help
		;;
esac

popd
