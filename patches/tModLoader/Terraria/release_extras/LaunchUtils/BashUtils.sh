#!/usr/bin/env bash

# Get root folder
root_dir="$(dirname "$(pwd -P)")"

# Read uname into a variable used in various places
_uname=$(uname)
_arch=$(uname -m)

# Sourced from dotnet-install.sh
# Check if a program is present or not
machine_has() {
	command -v "$1" > /dev/null 2>&1
	return $?
}

file_download() {
	if machine_has "curl"; then
		curl -sLo "$1" "$2"
	elif machine_has "wget"; then
		wget -q -O "$1" "$2"
	else
		echo "Missing dependency: neither curl nor wget was found."
		return 1 # @TODO: Should hard fail?
	fi
	return 0
}

# Call a script setting its permission right for execution
run_script() {
	chmod a+x "$1"

	# LD_PRELOAD="" fixes error messages from steams library linking polluting the stdout of all programs(in tree) run by run_script
	LD_PRELOAD="" "$@"
}
