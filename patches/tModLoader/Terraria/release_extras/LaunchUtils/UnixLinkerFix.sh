#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#THIS FILE MUST BE SOURCED
#cd "$(dirname "$0")"
. ./BashUtils.sh

# The following is a workaround for the system's SDL2 library being preferred by the linkers for some reason.
# Additionally, something in dotnet is requesting 'libSDL2.so' (instead of 'libSDL2-2.0.so.0' that is specified in dependencies)
# without actually invoking managed NativeLibrary resolving events!

echo "Fixing .NET SDL PATH issues" 2>&1 | tee -a "$LogFile"

unixSteamworks="$root_dir/Libraries/Steamworks.NET/20.1.0.0/Steamworks.NET.dll"
if [ -f "$unixSteamworks" ]; then
	rm $unixSteamworks
fi
steamworksRename="$root_dir/Libraries/Steamworks.NET"
if [ -d "$steamworksRename" ]; then
	mv -v "$steamworksRename" "$root_dir/Libraries/steamworks.net" 2>&1 | tee -a "$LogFile"
fi

if [ "$_uname" = Darwin ]; then
	library_dir="$root_dir/Libraries/Native/OSX"
	export DYLD_LIBRARY_PATH="$library_dir"
	export VK_ICD_FILENAMES="$libary_dir/MoltenVK_icd.json"
	ln -sf "$library_dir/libSDL2-2.0.0.dylib" "$library_dir/libSDL2.dylib"

	# El Capitan is a total idiot and wipes this variable out, making the
    # Steam overlay disappear. This sidesteps "System Integrity Protection"
    # and resets the variable with Valve's own variable (they provided this
    # fix by the way, thanks Valve!). Note that you will need to update your
    # launch configuration to the script location, NOT just the app location
    # (i.e. Kick.app/Contents/MacOS/Kick, not just Kick.app).
    # -flibit
    if [ "$STEAM_DYLD_INSERT_LIBRARIES" != "" ] && [ "$DYLD_INSERT_LIBRARIES" == "" ]; then
        export DYLD_INSERT_LIBRARIES="$STEAM_DYLD_INSERT_LIBRARIES"
    fi
elif [[ "$_uname" == *"_NT"* ]]; then
	export PATH="$root_dir/Libraries/Native/Windows;$PATH"
else
	library_dir="$root_dir/Libraries/Native/Linux"
	export LD_LIBRARY_PATH="$library_dir"
	ln -sf "$library_dir/libSDL2-2.0.so.0" "$library_dir/libSDL2.so"
fi
echo "Success!" 2>&1 | tee -a "$LogFile"
