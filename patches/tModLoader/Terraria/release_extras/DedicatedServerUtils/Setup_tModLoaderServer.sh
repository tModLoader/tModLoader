#!/usr/bin/env bash

# 0.0.0.0 for testing purposes. Will be bumped to 1.0.0.0 before the pr
scriptVersion="0.0.0.0"

# Define functions used in script

# Shut up both commands
function pushd {
	command pushd "$@" > /dev/null
	if [[ $? != "0" ]]
	then
		exit
	fi
}

function popd {
	command popd "$@" > /dev/null
	if [[ $? != "0" ]]
	then
		exit
	fi
}

function updateScript {
	# throw new System.NotImplementedException()
}

function getLatestRelease {
	local latestRelease=`curl --silent "https://api.github.com/repos/tModLoader/tModLoader/releases/latest" | grep '"tag_name":' | sed -E 's/.*"([^"]+)".*/\1/'` # Get latest release from github's api
	echo $latestRelease # So functions calling this can consume the result since you can't return strings in bash
}

# Takes version number as first parameter
function downRelease {
	if [[ -v version ]]
	then
		$1=$version
	fi
	echo "Downloading version $1"
	wget --quiet "https://github.com/tModLoader/tModLoader/releases/download/$1/tModLoader.zip"
}

# Check $username is defined, exit if not
function checkUsername {
	if ! [[ -v username ]]
	then
		echo "Please enter a username to login with using --username"
		exit 1
	else
		return 0
	fi
}

# Installs or updates tML
function steamcmdtML {
		checkUsername
		if [[ -v folder ]]
		then
			steamcmd +force_install_dir $folder +login $username +app_update 1281930 +quit # tMod goes into the dir they want, everything else should be forced into ~/Steam
		else
			steamcmd +login $username +app_update 1281930 +quit # tMod goes into ~/Steam/steamapps/common/tModLoader
		fi

		if [[ $? == "5" ]]
		then
			echo "Try entering password/2fa code again"
			steamcmdtML
		fi
}

function installtML {
	if $steamcmd
	then
		steamcmdtML
	else
		# Install from github, defaults to ~/tModLoader
		if [[ -v folder ]]
		then
			mkdir -p $folder
			pushd $folder
		else
			mkdir -p ~/tModLoader
			pushd ~/tModLoader
		fi

		installDir=$(shopt -s nullglob dotglob; echo ./*)
		if (( ${#installDir} ))
		then
			echo "Install directory not empty, please choose an empty directory to install tML to using --folder or update an existing installation using --update"
			exit 1
		fi

		# Install tml from github, leave some file containing what version
		local ver=$(getLatestRelease)
		downRelease $ver
		echo "Unzipping tModLoader.zip"
		unzip -q tModLoader.zip
		rm tModLoader.zip
		echo $ver > .ver

		popd
	fi
}

function updatetML {
	if $steamcmd
	then
		steamcmdtML
	else
		if [[ -v folder ]]
		then
			pushd $folder
		else
			pushd ~/tModLoader
		fi

		if [[ -r ".ver" ]]
		then
			local ver=$(getLatestRelease)
			oldver=`cat .ver`
			if [[ $ver == $oldver ]]
			then
				echo "No new version of tModLoader available"
			else
				echo "New version of tModLoader $ver is available, current ver is $oldver"

				# Backup old tML versions in case something implodes
				mkdir $oldver
				for file in ./*;
				do
					if ! [[ "$file" == v.* ]] && ! [[ "$file" == Setup_tModLoaderServer.sh ]] && ! [[ "$file" == install.txt ]] && ! [[ "$file" == enabled.json ]] && ! [[ "$file" == *.tmod ]] && ! [[ "$file" == ./$oldver ]] && ! [[ "$file" == *.tar.gz ]]
					then
						mv "$file" "$oldver"
					fi
				done

				echo "Taring $oldver backup"
				tar czf $oldver.tar.gz $oldver/*
				rm -r $oldver

				downRelease $ver
				echo "Unzipping tModLoader.zip"
				unzip -q tModLoader.zip
				rm tModLoader.zip
				echo $ver > .ver

				# Delete all backups but the most recent
				echo "Removing old backups"
				for file in ./v*.tar.gz;
				do
					if ! [[ "$file" == ./$oldver.tar.gz ]]
					then
						rm $file
						echo "Removed $file"
					fi
				done
			fi
		else
			echo "tModLoader is not installed"
			exit 1
		fi

		popd
	fi
}

function updateWorkshopMods {
	if [[ -r ./install.txt ]]
	then
		echo Installing workshop mods

		if [[ -v folder ]]
		then
			steamcmdCommand="+force_install_dir $folder +login anonymous"
		else
			steamcmdCommand="+login anonymous"
		fi

		lines=$(cat ./install.txt)
		for line in $lines
		do
			steamcmdCommand="$steamcmdCommand +workshop_download_item 1281930 $line"
		done
		steamcmd $steamcmdCommand
	else
		echo "No workshop mods to install"
	fi
}

function installMods {
	if $steamcmd
	then
		updateWorkshopMods
	fi

	scriptPath=$(realpath $0) # Get path to the script, in case someone launched it from another directory
	pushd $(dirname $scriptPath)

	if [[ -v modsPath ]]
	then
		echo "Moving mods to $modsPath"
	elif [[ -v XDG_DATA_HOME ]]
	then
		modsPath=$XDG_DATA_HOME/Terraria/tModLoader/Mods
		echo "Moving mods to $modsPath"
	else
		modsPath=~/.local/share/Terraria/tModLoader/Mods
		echo "Moving mods to $modsPath"
	fi

	mkdir -p $modsPath

	# If someone has .tmod files this will install them
	if [[ -f "*.tmod" ]]
	then
		echo "Moving .tmod files to the Mods directory"
		mv *.tmod $modsPath
	fi

	# Move enabled.json to the right place
	if [[ -f "enabled.json" ]]
	then
		echo "Moving enabled.json to the Mods directory"
		mv enabled.json $modsPath
	fi

	popd
}

function printVersion {
	echo "tML Maintenance Tool: v$scriptVersion"

	if [[ -v folder ]]
	then
		echo tML Install: `cat $folder/.ver`
	elif [[ -r ~/tModLoader/.ver ]]
	then
		echo tML Install: `cat ~/tModLoader/.ver`
	fi

	exit 0
}

function printHelp {
	echo "
tML dedicated server installation and maintenance script

Options:
 -h|--help           Show command line help.
 -v|--version        Display the current version of the tool and a tModLoader Github install.
 -i|--install        Install tModLoader and mods.
 -u|--update         Update an existing tModLoader installation and mods.
 -g|--github         Use the binary off of Github instead of using steamcmd.
 -f|--folder         Choose the folder to update/install to. When using steamcmd, make sure to use an absolute path. Defaut path is ~/tModLoader when using Github, and ~/Steam/steamapps/common/tModLoader when using steamcmd.
 -m|--modspath       The path to your mods folder. Any .tmod files and enabled.json are sent here.
 --username          The steam username to login with. Only applies when using steamcmd.
 --tmlversion        The version of tML to download. Only applies when using Github. This should be the exact tag off of Github (ex. v2022.06.96.4).

When running --install and --update, enabled.json, install.txt, and any .tmod files will be checked for in the location of the script."
	exit
}

# Parse script arguments
install=false
update=false
steamcmd=true # Use steamcmd by default. If someone doesn't want to use steamcmd, they probably don't have it installed and since it'll exit, they can specify --github next time

if [ $# -eq 0 ] # Check for no arguments
then
	echo "No arguments supplied"
	printHelp
fi

while [[ $# -gt 0 ]];
do
	case $1 in
		-h|--help)
			printHelp
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
		-m|--modspath)
			modsPath="$2"
			shift; shift;
			;;
		--tmlversion)
			version="$2"
			shift; shift;
			;;
		-v|--version)
			printVersion
			;;
		*)
			echo "Argument not recognized: $1"
			printHelp
			;;
	esac
done

# Check that steamcmd exists on PATH
if $steamcmd
then
	if ! command -v steamcmd &> /dev/null
	then
		echo "steamcmd could not be found on the PATH, please install steamcmd"
		exit 1
	else
		echo "steamcmd found..."
	fi
else
	if ! command -v unzip &> /dev/null
	then
		echo "unzip could not be found on the PATH, please instal unzip"
		exit 1
	else
		echo "unzip found..."
	fi
fi

if $install
then
	installtML
	installMods
fi

if $update
then
	updatetML
	installMods
	updateScript
fi
