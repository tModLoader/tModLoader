#!/bin/bash
# MonoKickstart Shell Script
# Written by Ethan "flibitijibibo" Lee

TERM=xterm

# Move to script's directory
cd "`dirname "$0"`"

# Get the system architecture
UNAME=`uname`
ARCH=`uname -m`
BASENAME=`basename "$0"`
BASENAME=${BASENAME%-*} #remove anything after a dash

if [ "$UNAME" == "Darwin" ]; then
	ext=osx
	
	export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:./osx/

	if [ "$STEAM_DYLD_INSERT_LIBRARIES" != "" ] && [ "$DYLD_INSERT_LIBRARIES" == "" ]; then
		export DYLD_INSERT_LIBRARIES="$STEAM_DYLD_INSERT_LIBRARIES"
	fi
else
	if [ "$ARCH" == "x86_64" ]; then
		ext=x86_64
	else
		ext=x86
	fi
	# arch override
	for arg in "$@" 
	do
		case "$arg" in
		-x86)	ext=x86
			;;
		-x64)	ext=x86_64
			;;
		esac
	done
fi

# tModLoader scripts for running with system mono put the kickstart libraries in a sys folder to avoid conflicts
mv sys/* . 2> /dev/null

KICKSTART=./${BASENAME}.bin.${ext}

export MONO_IOMAP=all
$KICKSTART $@

