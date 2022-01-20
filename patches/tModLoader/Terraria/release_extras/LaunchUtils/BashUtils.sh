#!/usr/bin/env bash

# Read uname into a variable used in various places
_uname=$(uname)

# Get root folder
root_dir="$(dirname $(dirname "$0"))"

# Sourced from dotnet-install.sh
# Check if a program is present or not
machine_has() {
  eval $invocation

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
    return 123 # @TODO: Should hard fail?
  fi
  return 0
}

# Call a script setting its permission right for execution
run_script() {
  chmod +x $1
  $*
}
