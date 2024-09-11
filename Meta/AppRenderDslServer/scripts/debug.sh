
#!/usr/bin/env bash
set -e

# Builds src/RenderDSLServer/bin/Release/netcoreapp2.0/osx.10.11-x64/publish/RenderDSLServer
dotnet build -c Debug -r osx.10.11-x64 src/RenderDSLServer
echo 'Press F5 to debug the new build of F# language server'
