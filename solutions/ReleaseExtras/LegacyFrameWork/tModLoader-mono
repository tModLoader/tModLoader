#!/bin/bash

TERM=xterm

# Move to script's directory
cd "`dirname "$0"`"

# Get the system architecture
UNAME=`uname`
BASENAME=`basename "$0"`
BASENAME=${BASENAME%-*} #remove anyting after a dash

if [ "$UNAME" == "Darwin" ]; then
	export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:./osx/
	export PATH=$PATH:/Library/Frameworks/Mono.framework/Versions/Current/Commands

	if [ "$STEAM_DYLD_INSERT_LIBRARIES" != "" ] && [ "$DYLD_INSERT_LIBRARIES" == "" ]; then
		export DYLD_INSERT_LIBRARIES="$STEAM_DYLD_INSERT_LIBRARIES"
	fi
else
	export LD_LIBRARY_PATH=lib:lib64
fi

# move all kickstart mono libraries to a sys/ folder to remove conflicts
mkdir sys 2> /dev/null
mv System*.dll* sys 2> /dev/null
mv WindowsBase.dll sys 2> /dev/null
mv Mono*.dll sys 2> /dev/null

# Run system mono
mono ${BASENAME}.exe $@
