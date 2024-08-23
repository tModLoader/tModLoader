#!/bin/sh

if ! command -v git &> /dev/null
then
    echo "git not found in PATH"
    exit 1
fi

echo "Restoring git submodules"
git submodule update --init --recursive
if [ $? -ne 0 ]
then
    exit $?
fi

if ! command -v dotnet &> /dev/null
then
    echo "dotnet not found in PATH"
    exit 1
fi

echo "Building Setup.CLI.csproj"
dotnet build setup/CLI/Setup.CLI.csproj -c Release --output "setup/bin/Release/net8.0" -p:WarningLevel=0 -v q

./setup/bin/Release/net8.0/setup-cli $@

