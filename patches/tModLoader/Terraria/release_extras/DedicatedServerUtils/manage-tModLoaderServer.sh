#!/usr/bin/env bash

#shellcheck disable=2164

script_version="1.0.0.1"
script_url="https://raw.githubusercontent.com/tModLoader/tModLoader/1.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh"

# Define functions used in script

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

	if [[ "$script_version" != "`echo -e "$script_version\n$latest_script_version" | sort -rV | head -n1`" ]]; then
		return 0
	else
		return 1
	fi
}

function update_script {
	if check_update; then
		echo "Updating from version v$script_version to v$latest_script_version"
		curl -s -O "$script_url" 2>/dev/null || wget -q "$script_url"
		mv manage-tModLoaderServer.sh.1 manage-tModLoaderServer.sh
	else
		echo "No new script updates"
	fi

	exit 0
}

function get_latest_release {
	local release_url="https://api.github.com/repos/tModLoader/tModLoader/releases/latest"
	local latest_release=$({
		curl -s "$release_url" 2>/dev/null || wget -q -O- "$release_url";
	} | grep '"tag_name":' | sed -E 's/.*"([^"]+)".*/\1/') # Get latest release from github's api
	echo "$latest_release" # So functions calling this can consume the result since you can't return strings in bash :)
}

# Takes version number as first parameter
function down_release {
	local down_url="https://github.com/tModLoader/tModLoader/releases/download/$1/tModLoader.zip"
	if [[ -v version ]]; then
		set -- "$version"
	fi
	echo "Downloading version $1"
	curl -s -LJO "$down_url" 2>/dev/null || wget -q --content-disposition "$down_url"
}

# Check $username is defined, ask if not
function check_username {
	if ! [[ -v username ]]; then
		read -p "Please enter a Steam username to login with: " username
	fi
}

function copy_worlds {
	for file in *.wld; do
		[ -f "$file" ] || break
		twld="$(basename "$file" .wld).twld"
		echo "Copying $file and $twld"
		mkdir -p ~/.local/share/Terraria/tModLoader/Worlds/ && cp $file $twld $_  
	done
}

# Installs or updates tML via steamcmd
function steamcmd_install_tml {
		check_username
		if [[ -v folder ]]; then
			steamcmd +force_install_dir "$folder" +login "$username" +app_update 1281930 +quit # tMod goes into the dir they want, everything else should be forced into ~/Steam
		else
			steamcmd +login "$username" +app_update 1281930 +quit # tMod goes into ~/Steam/steamapps/common/tModLoader
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

# Install from github, defaults to ~/tModLoader
function github_install_tml {
	if [[ -v folder ]]; then
		mkdir -p "$folder"
		pushd "$folder"
	else
		mkdir -p ~/tModLoader
		pushd ~/tModLoader
	fi

	install_dir=$(shopt -s nullglob dotglob; echo ./*)
	if (( ${#install_dir} )); then
		echo "Install directory not empty, please choose an empty directory to install tML to using --folder or update an existing installation using --update"
		exit 1
	fi

	# Install tml from github, leave some file containing what version
	local ver
	ver=$(get_latest_release)
	down_release "$ver"
	echo "Unzipping tModLoader.zip"
	unzip -q tModLoader.zip
	rm tModLoader.zip
	echo "$ver" > .ver

	popd
}

function install_tml {
	if $steamcmd; then
		steamcmd_install_tml
	else
		github_install_tml
	fi

	copy_worlds
}

function github_update_tml {
	if [[ -v folder ]]; then
		pushd "$folder"
	else
		pushd ~/tModLoader
	fi

	if [[ -r ".ver" ]]; then
		local ver
		ver=$(get_latest_release)
		oldver=$(cat .ver)
		if [[ $ver == "$oldver" ]]; then
			echo "No new version of tModLoader available"
		else
			echo "New version of tModLoader $ver is available, current version is $oldver"

			# Backup old tML versions in case something implodes
			mkdir "$oldver"
			for file in ./*;
			do
				if ! [[ "$file" == v.* ]] && ! [[ "$file" == manage-tModLoaderServer.sh ]] && ! [[ "$file" == install.txt ]] && ! [[ "$file" == enabled.json ]] && ! [[ "$file" == *.tmod ]] && ! [[ "$file" == ./$oldver ]] && ! [[ "$file" == *.tar.gz ]]; then
					mv "$file" "$oldver"
				fi
			done

			echo "Compressing $oldver backup"
			tar czf "$oldver".tar.gz "$oldver"/*
			rm -r "$oldver"

			down_release "$ver"
			echo "Unzipping tModLoader.zip"
			unzip -q tModLoader.zip
			rm tModLoader.zip
			echo "$ver" > .ver

			# Delete all backups but the most recent
			echo "Removing old backups"
			for file in ./v*.tar.gz;
			do
				if ! [[ "$file" == ./$oldver.tar.gz ]]; then
					rm "$file"
					echo "Removed $file"
				fi
			done
		fi
	else
		echo "tModLoader is not installed"
		exit 1
	fi

	popd
}

function update_tml {
	if $steamcmd; then
		steamcmd_install_tml
	else
		github_update_tml
	fi
}

function update_workshop_mods {
	if [[ -r ./install.txt ]]; then
		echo Installing workshop mods

		if [[ -v folder ]]; then
			steamcmd_command="+force_install_dir $folder +login anonymous"
		else
			steamcmd_command="+login anonymous"
		fi

		lines=$(cat ./install.txt)
		for line in $lines
		do
			steamcmd_command="$steamcmd_command +workshop_download_item 1281930 $line"
		done
		steamcmd "$steamcmd_command +quit"
	else
		echo "No workshop mods to install"
	fi
}

function install_mods {
	script_path=$(realpath "$0") # Get path to the script, in case someone launched it from another directory
	if [[ -v checkdir ]]; then
		pushd "$checkdir"
	else
		pushd "$(dirname "$script_path")"
	fi

	if ! command -v steamcmd &> /dev/null; then
		echo "steamcmd not found on path, no workshop mods will be installed or updated"
	else
		update_workshop_mods
	fi

	if [[ -v mods_path ]]; then
		echo "Moving mods to $mods_path"
	elif [[ -v XDG_DATA_HOME ]]; then
		mods_path=$XDG_DATA_HOME/Terraria/tModLoader/Mods
		echo "Moving mods to $mods_path"
	else
		mods_path=~/.local/share/Terraria/tModLoader/Mods
		echo "Moving mods to $mods_path"
	fi

	mkdir -p $mods_path

	# If someone has .tmod files this will install them
	local count
	count=$(ls -1 ./*.tmod 2>/dev/null | wc -l)
	if [ $count != 0 ]; then
		echo "Copying .tmod files to the mods directory"
		cp ./*.tmod $mods_path
	fi

	# Move enabled.json to the right place
	if [[ -f "enabled.json" ]]; then
		echo "Copying enabled.json to the mods directory"
		cp enabled.json $mods_path
	fi

	if [[ -f "serverconfig.txt" ]]; then
		echo "Copying serverconfig.txt to the install directory"
		if [[ -v folder ]]; then
			cp serverconfig.txt $folder
		elif $steamcmd; then
			cp serverconfig.txt ~/Steam/steamapps/common/tModLoader
		else
			cp serverconfig.txt ~/tModLoader
		fi
	fi

	popd
}

function print_version {
	echo "tML Maintenance Tool: v$script_version"

	if [[ -v folder ]]; then
		echo tML Install: "$(cat "$folder/.ver")"
	elif [[ -r ~/tModLoader/.ver ]]; then
		echo tML Install: "$(cat ~/tModLoader/.ver)"
	fi

	exit 0
}

function print_help {
	echo \
"tML dedicated server installation and maintenance script

Options:
 -h|--help           Show command line help.
 -v|--version        Display the current version of the tool and a tModLoader Github install.
 -i|--install        Install tModLoader and mods. Will copy any world files, will not overwrite any existing ones.
 -u|--update         Update an existing tModLoader installation and mods.
 -g|--github         Use the binary off of Github instead of using steamcmd.
 -f|--folder         Choose the folder to update/install to. When using steamcmd, make sure to use an absolute path. Default path is ~/tModLoader when using Github, and ~/Steam/steamapps/common/tModLoader when using steamcmd.
 -m|--mods-path      The path to your mods folder. Any .tmod files and enabled.json are sent here.
 --username          The steam username to login with. Only applies when using steamcmd.
 --tml-version       The version of tML to download. Only applies when using Github. This should be the exact tag off of Github (ex. v2022.06.96.4).
 --update-script     Update the script to the latest version on Github.
 --worlds            Copy any world files, will not overwrite any existing ones.
 --mods-only         Only install/update mods.
 --no-mods           Don't install/update mods.
 --check-dir         Directory to check for enabled.json, install.txt, and any .tmod files.
 --start             Launch the game after running any other operations.

When running --install and --update, enabled.json, install.txt, and any .tmod files will be checked for in the location of the script or in the directory specified by --check-dir."
	exit
}

#
# ______       _                          _       _
#|  ____|     | |                        (_)     | |
#| |__   _ __ | |_ _ __ _   _ _ __   ___  _ _ __ | |_
#|  __| | '_ \| __| '__| | | | '_ \ / _ \| | '_ \| __|
#| |____| | | | |_| |  | |_| | |_) | (_) | | | | | |_
#|______|_| |_|\__|_|   \__, | .__/ \___/|_|_| |_|\__|
#                        __/ | |
#                       |___/|_|
#

# Parse script arguments
install=false
update=false
steamcmd=true # Use steamcmd by default. If someone doesn't want to use steamcmd, they probably don't have it installed and since it'll exit, they can specify --github next time
no_mods=false
mods_only=false
start=false

if [ $# -eq 0 ]; then # Check for no arguments
	echo "No arguments supplied"
	print_help
fi

while [[ $# -gt 0 ]]; do
	case $1 in
		-h|--help)
			print_help
			;;
		-i|--install)
			install=true
			shift
			;;
		-u|--update)
			update=true
			shift
			;;
		-g|--github)
			steamcmd=false
			shift
			;;
		--username)
			username="$2"
			shift; shift
			;;
		-f|--folder)
			folder="$2"
			shift; shift
			;;
		-m|--mods-path)
			mods_path="$2"
			shift; shift;
			;;
		--tml-version)
			version="$2"
			shift; shift
			;;
		-v|--version)
			print_version
			;;
		--update-script)
			update_script
			;;
		--worlds)
			copy_worlds
			shift
			;;
		--mods-only)
			mods_only=true
			shift
			;;
		--no-mods)
			no_mods=true
			shift
			;;
		--check-dir)
			checkdir="$2"
			shift; shift
			;;
		--start)
			start=true
			shift
			;;
		*)
			echo "Argument not recognized: $1"
			print_help
			;;
	esac
done

# Check that steamcmd exists on PATH
if $steamcmd; then
	if ! command -v steamcmd &> /dev/null
	then
		echo "steamcmd could not be found on the PATH, please install steamcmd"
		exit 1
	else
		echo "steamcmd found..."
	fi
else
	if ! command -v unzip &> /dev/null; then
		echo "unzip could not be found on the PATH, please install unzip"
		exit 1
	else
		echo "unzip found..."
	fi
fi

if $install; then
	if ! $mods_only; then install_tml; fi
	if ! $no_mods; then install_mods; fi
fi

if $update; then
	if ! $mods_only; then update_tml; fi
	if ! $no_mods; then install_mods; fi
fi

if check_update; then
	read -t 5 -p "Script update available! Update now? (y/n): " update_now

	case $update_now in
		[Yy]*)
			echo "Updating now"
			update_script
			;;
		*)
			echo "Not updating"
			;;
	esac
fi

if $start; then
	if [[ -v folder ]]; then
		cd $folder || exit
	elif $steamcmd; then
		cd ~/Steam/steamapps/common/tModLoader || exit
	else
		cd ~/tModLoader || exit
	fi

	chmod u+x start-tModLoaderServer.sh
	./start-tModLoaderServer.sh
fi
