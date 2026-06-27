#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#THIS FILE MUST BE SOURCED
#cd "$(dirname "$0")"
. ./BashUtils.sh

echo "Fixing Environment Issues" 2>&1 | tee -a "$LogFile"

# Remove old Steamworks.NET now that we have Steamworks.Net.AnyCPU
oldSteamworks="$root_dir/Libraries/steamworks.net"
if [ -f "oldSteamworks" ]; then
	rm -rf oldSteamworks
fi

newtonsoftRename="$root_dir/Libraries/Newtonsoft.Json"
if [ -d "$newtonsoftRename" ]; then
	mv -v "$newtonsoftRename" "$root_dir/Libraries/newtonsoft.json" 2>&1 | tee -a "$LogFile"
fi

# Process Dump On Crash Configuration Options. 
# DbgMini is a mini memory dump when dotnet crashes. 
#	Dump diagnostics is diagnostic logging for that dumping Process
# Enable Crash Report is a Unix based feature. Needs DbgMiniDumpName
#export DOTNET_DbgEnableMiniDump=1
#export DOTNET_DbgMiniDumpType=3
#export DOTNET_DbgMiniDumpName="$LaunchLogs/CoreDump.%t"
#export DOTNET_CreateDumpDiagnostics=1
#export DOTNET_CreateDumpLogToFile="$LaunchLogs/CoreDumpDiagnostics.log"
#export DOTNET_EnableCrashReport=1

# The following is a workaround for the system's SDL2 library being preferred by the linkers for some reason.
# Additionally, something in dotnet is requesting 'libSDL2.so' (instead of 'libSDL2-2.0.so.0' that is specified in dependencies)
# without actually invoking managed NativeLibrary resolving events!
if [ "$_uname" = Darwin ]; then
	library_dir="$root_dir/Libraries/Native/OSX"
	export DYLD_LIBRARY_PATH="$library_dir"
	export VK_ICD_FILENAMES="$library_dir/MoltenVK_icd.json"
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
	# This fixes issues with people clicking in the command prompt on Win10 & Win11
	echo "Windows Version $WINDOWS_MAJOR.$WINDOWS_MINOR" 2>&1 | tee -a "$LogFile"
	if [[ $WINDOWS_MAJOR -ge 10 ]]; then 
		./QuickEditDisable.exe 2>&1 | tee -a "$LogFile"
	elif [[ $WINDOWS_MAJOR -ge 6 ]]; then
		echo "Windows 7 to 8.1 detected. Modifying Environment"
		# Allows Dotnet8 to run on Windows 7 (6.1) to 8.1 (6.3). Configures it for all applications
		run_script ./Windows7Dotnet8Fix.bat 2>&1 | tee -a "$LogFile"
		export DOTNET_EnableWriteXorExecute=0
	fi

	# removes incompatible 1.3 64 files
	run_script ./Remove13_64Bit.sh  2>&1 | tee -a "$LogFile"

	# Fixes SDL2 link issues
	export PATH="$root_dir/Libraries/Native/Windows;$PATH"
else
	library_dir="$root_dir/Libraries/Native/Linux"
	export LD_LIBRARY_PATH="$library_dir"
	ln -sf "$library_dir/libSDL2-2.0.so.0" "$library_dir/libSDL2.so"
fi

# Detecting Proton usage which can break tModLoader game rendering as of Dec 2023.
if [[ "$WINDOWS_MAJOR" == "0" || ! -z "$WINEHOMEDIR" ]]; then
	echo "Proton has been detected. It is highly recommended to not use it as it causes all manner of issues. Please disable Proton and launch again. See https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#disable-proton for information on moving save data to the correct location." 2>&1 | tee -a "$LogFile"
fi

echo "Success!" 2>&1 | tee -a "$LogFile"
