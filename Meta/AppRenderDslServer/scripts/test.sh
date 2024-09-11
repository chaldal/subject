#!/usr/bin/env bash
# Restore test projects and run all tests

echo 'Running tests...'
set -e
dotnet test tests/LSP.Tests
dotnet test tests/RenderDSLServer.Tests
