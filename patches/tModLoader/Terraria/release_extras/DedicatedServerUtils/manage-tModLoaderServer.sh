#!/usr/bin/env bash

#shellcheck disable=2164

script_version="2.0.0.0"
script_url="https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh"

# Shut up both commands
function pushd {
	command pushd "$@" > /dev/null || return
}

function popd {
	command popd > /dev/null || return
}

# Returns true if an update is needed
function check_update {
	latest_script_version=$({
		curl -s "$script_url" 2>/dev/null || wget -q -O- "$script_url";
	} | grep "script_version=" | head -n1 | cut -d '"' -f2)

	if [[ "$script_version" != "$(echo -e "$script_version\n$latest_script_version" | sort -rV | head -n1)" ]]; then
		return 0
	fi

	return 1
}

function update_script {
	if check_update; then
		echo "Updating from version v$script_version to v$latest_script_version"
		curl -s -O "$script_url" 2>/dev/null || wget -q "$script_url"
		mv manage-tModLoaderServer.sh.1 manage-tModLoaderServer.sh
	else
		echo "No new script updates"
	fi

	exit
}

function verify_download_tools {
	# Check PATH and flags for required commands
	if $steamcmd; then
		if [[ -v steamcmd_path ]]; then
			if [[ -f "$steamcmd_path" ]]; then
				# TODO: Should any checks be done here?
				steam_cmd="$steamcmd_path"
				echo "steamcmd found in folder..."
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

# Gets version of TML to install from github, prioritizing --tml-version and tmlversion.txt
function get_version {
	if [[ -v tmlversion ]]; then
		echo "$tmlversion"
	elif [[ -r "$folder/tmlversion.txt" ]]; then
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
	if [[ -v install_dir ]]; then
		pushd "$install_dir"
	else
		pushd ~/tModLoader
	fi

	if ! [[ -r ".ver" ]]; then
		echo "tModLoader is not installed"
		exit 1
	fi

	local ver
	ver=$(get_version)
	local oldver
	oldver=$(cat .ver)
	if [[ "$ver" == "$oldver" ]]; then
		echo "No version change of tModLoader available"
		return
	fi

	echo "New version of tModLoader $ver is available, current version is $oldver"

	# Backup old tML versions in case something implodes
	mkdir "$oldver"
	for file in ./*;
	do
		if ! [[ "$file" == v.* ]] && ! [[ "$file" == "./$oldver" ]] && ! [[ "$file" == manage-tModLoaderServer.sh ]] && ! [[ "$file" == *.tar.gz ]]; then
			mv "$file" "$oldver"
		fi
	done

	echo "Compressing $oldver backup"
	tar czf "$oldver".tar.gz "$oldver"/*
	rm -r "$oldver"

	download_release "$ver"

	# Delete all backups but the most recent
	echo "Removing old backups"
	for file in ./v*.tar.gz;
	do
		if ! [[ "$file" == ./$oldver.tar.gz ]]; then
			rm "$file"
			echo "Removed $file"
		fi
	done

	popd
}

function install_tml_steam {
	if ! [[ -v username ]]; then
		read -p "Please enter a Steam username to login with: " username
	fi

	if [[ -v install_dir ]]; then
		eval "$steam_cmd +force_install_dir $install_dir +login $username +app_update 1281930 +quit" # tMod goes into the dir they want, everything else should be forced into ~/Steam
	else
		eval "$steam_cmd +login $username +app_update 1281930 +quit" # tMod goes into ~/Steam/steamapps/common/tModLoader
	fi

	if [[ $? == "5" ]]; then # Only recurse when not being used in the docker container.
		if ! [[ -f /.dockerenv ]]; then
			echo "Try entering password/2fa code again"
			steamcmd_install_tml
		else
			exit 5
		fi
	fi
}

function install_tml_github {
	if [[ -v install_dir ]]; then
		mkdir -p "$install_dir"
		pushd "$install_dir"
	else
		mkdir -p ~/tModLoader
		pushd ~/tModLoader
	fi

	if [[ -n "$(ls -A)" ]]; then
		echo "Install directory not empty, please choose an empty directory to install tML to using --install-dir or run update to update an existing installation"
		exit 1
	fi

	# Install tml from github, leave some file containing what version
	download_release "$(get_version)"

	popd
}

function install_workshop_mods {
	pushd "$folder"

	if ! [[ -v steam_cmd ]]; then
		echo "SteamCMD not found, no workshop mods will be installed or updated"
		return
	fi

	if ! [[ -r "install.txt" ]]; then
		echo "No workshop mods to install"
		return
	fi

	echo Installing workshop mods

	if [[ -v install_dir ]]; then
		steamcmd_command="+force_install_dir $install_dir"
	fi

	steamcmd_command="$steamcmd_command +login anonymous"

	lines=$(cat install.txt)
	for line in $lines
	do
		steamcmd_command="$steamcmd_command +workshop_download_item 1281930 $line"
	done

	eval "$steam_cmd $steamcmd_command +quit"
	popd
}

function copy_config {
	pushd "$folder"

	if [[ -f "serverconfig.txt" ]]; then
		echo "Copying serverconfig.txt to the install directory"
		if [[ -v install_dir ]]; then
			cp -f serverconfig.txt "$install_dir"
		elif $steamcmd; then
			cp -f serverconfig.txt ~/Steam/steamapps/common/tModLoader
		else
			cp -f serverconfig.txt ~/tModLoader
		fi
	fi

	popd
}

function install_mods {
	pushd "$folder"
	install_workshop_mods

	# TODO: should mods_path change if install_dir does?
	if [[ -v XDG_DATA_HOME ]]; then
		mods_path=$XDG_DATA_HOME/Terraria/tModLoader/Mods
	else
		mods_path=~/.local/share/Terraria/tModLoader/Mods
	fi

	mkdir -p "$mods_path"

	# If someone has local .tmod files this will install them
	if [[ -d "Mods" ]]; then
		local count
		count=$(ls -1 Mods/*.tmod 2>/dev/null | wc -l)
		if [[ "$count" -ne "0" ]]; then
			echo "Copying .tmod files to the mods directory"
			cp -f Mods/*.tmod "$mods_path"
		fi
	fi

	# Move enabled.json to the right place
	if [[ -f "enabled.json" ]]; then
		echo "Copying enabled.json to the mods directory"
		cp -f enabled.json "$mods_path"
	fi

	copy_config

	popd
}

function copy_worlds {
	pushd "$folder"

	if [[ -d "Worlds" ]]; then
		local count
		count=$(ls -1 Worlds/*.wld 2>/dev/null | wc -l)
		if [[ "$count" -ne "0" ]]; then
			echo "Copying .wld and .twld files to the worlds directory"
			mkdir -p ~/.local/share/Terraria/tModLoader/Worlds && cp Worlds/*.wld Worlds/*.twld "$_"
		fi
	fi

	popd
}

function print_version {
	echo "tML Maintenance Tool: v$script_version"

	if [[ -v install_dir ]]; then
		echo tML Install: "$(cat "$install_dir/.ver")"
	elif [[ -r ~/tModLoader/.ver ]]; then
		echo tML Install: "$(cat ~/tModLoader/.ver)"
	fi

	exit
}

# TODO: "clean" or "remove" command to cleanup the installation
function print_help {
	echo \
"tML dedicated server installation and maintenance script

Usage: script.sh COMMAND [OPTIONS]

Options:
 -h|--help           Show command line help.
 -v|--version        Display the current version of the tool and a tModLoader Github install.
 -g|--github         Use the binary off of Github instead of using steamcmd.
 -i|--install-dir    The folder to update/install to. When using steamcmd, make sure to use an absolute path. Default path is ~/tModLoader for Github, and ~/Steam/steamapps/common/tModLoader for steamcmd.
 -f|--folder         The folder containing all of your server data (Mods, Worlds, enabled.json, install.txt)
 --username          The steam username to login with. Only applies when using steamcmd.
 --tml-version       The version of tML to download. Only applies when using Github. This should be the exact tag off of Github (ex. v2022.06.96.4).
 --skip-tml          Only install/update mods.
 --skip-mods         Only install/update tModLoader.
 --steamcmdpath      Custom SteamCMD path for users who do not have a installation in PATH. This should point to the steamcmd.sh script

Commands:
 help                Equivalent to --help.
 install             Installs tModLoader and any mods provided. Will copy any world files, will not overwrite any existing ones.
 update              Updates an existing tModLoader installation and its mods.
 start               Launches the server with no updating or installing of mods. This should be run after one of the above commands.
 update-script       Update the script to the latest version on Github.
 docker              Runs the Docker-specific management command. It is not recommended to run this manually.

When running install or update, Folders 'Mods' and 'Worlds' as well as files enabled.json and install.txt will be checked for in the location of the script or in the directory specified by --folder."
	exit
}

# Set SteamCMD by default. Checks are done to ensure it's installed and the user will be notified if any issues arise
steamcmd=true
skip_mods=false
skip_tml=false

if [ $# -eq 0 ]; then # Check for no arguments
	echo "No command supplied"
	print_help
fi

cmd="$1"
shift

if [ $# -eq 0 ]; then # Check for no arguments
	echo "No arguments supplied"
	print_help
fi

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
		--username)
			username="$2"
			shift; shift
			;;
		-i|--install-dir)
			install_dir="$2"
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
		*)
			echo "Invalid Argument: $1"
			print_help
			;;
	esac
done

# Set folder to the script's folder if it isn't set
if ! [[ -v folder ]]; then
	echo "Setting folder to current directory"
	script_path=$(realpath "$0")
	folder="$(dirname "$script_path")"
fi

case $cmd in
	help)
		print_help
		;;
	install|update)
		verify_download_tools

		if ! $skip_tml; then
			if $steamcmd; then
				install_tml_steam
			elif [[ "$cmd" = "install" ]]; then
				install_tml_github
			else
				update_tml_github
			fi
		fi

		if ! $skip_mods; then
			install_mods
		fi
		;;
	update-script)
		update_script
		;;
	docker)
		verify_download_tools
		install_mods

		# Set --github and fallthrough so the server can be started properly
		steamcmd=false
		;&
	start)
		copy_worlds
		copy_config

		if [[ -v install_dir ]]; then
			cd "$install_dir" || exit
		elif $steamcmd; then
			cd ~/Steam/steamapps/common/tModLoader || exit
		else
			cd ~/tModLoader || exit
		fi

		chmod u+x start-tModLoaderServer.sh
		./start-tModLoaderServer.sh -nosteam
		;;
	*)
		echo "Invalid Command: $1"
		print_help
		;;
esac

# # TODO: Disable this in docker?
# if check_update; then
# 	read -t 5 -p "Script update available! Update now? (y/n): " update_now

# 	case $update_now in
# 		[Yy]*)
# 			echo "Updating now"
# 			update_script
# 			;;
# 		*)
# 			echo "Not updating"
# 			;;
# 	esac
# fi
