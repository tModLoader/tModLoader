#!/usr/bin/env bash
cd "$(dirname "$0")"/..
dotnet tModLoader.dll -tModPorter $@
