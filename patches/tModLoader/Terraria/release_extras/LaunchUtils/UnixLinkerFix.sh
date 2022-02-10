#!/usr/bin/env bash
#Authors: covers1624, DarioDaf, Solxanich
# Provided for use in tModLoader deployment. 

#THIS FILE MUST BE SOURCED
#cd "$(dirname "$0")"
. ./BashUtils.sh

# The following is a workaround for the system's SDL2 library being preferred by the linkers for some reason.
# Additionally, something in dotnet is requesting 'libSDL2.so' (instead of 'libSDL2-2.0.so.0' that is specified in dependencies)
# without actually invoking managed NativeLibrary resolving events!

echo "Fixing .NET SDL PATH issues"

if [ "$_uname" = Darwin ]; then
	library_dir="$root_dir/Libraries/Native/OSX"
	export DYLD_LIBRARY_PATH="$library_dir"
	export VK_ICD_FILENAMES="$libary_dir/MoltenVK_icd.json"
	ln -sf "$library_dir/libSDL2-2.0.0.dylib" "$library_dir/libSDL2.dylib"
elif [[ "$_uname" == *"_NT"* ]]; then
	export PATH="$PATH;$root_dir/Libraries/Native/Windows/"
else
	library_dir="$root_dir/Libraries/Native/Linux"
	export LD_LIBRARY_PATH="$library_dir"
	ln -sf "$library_dir/libSDL2-2.0.so.0" "$library_dir/libSDL2.so"
fi
echo "Success!"
