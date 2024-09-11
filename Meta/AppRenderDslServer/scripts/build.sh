#!/usr/bin/env bash
# Builds the plugin as build.vsix
# You will need dotnet core and vsce to run this script
set -e

# Needed once
npm install

# Build self-contained archives for windows, mac and linux
dotnet clean
dotnet publish -c Release -r win10-x64 src/RenderDSLServer
dotnet publish -c Release -r osx.10.11-x64 src/RenderDSLServer
dotnet publish -c Release -r linux-x64 src/RenderDSLServer

# Build vsix
vsce package -o build.vsix
code --install-extension build.vsix
