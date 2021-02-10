#!/usr/bin/env bash

# Clean and build in release
dotnet restore
dotnet clean
dotnet build -c Release

# Create NuGet package
dotnet pack src/Bloggine/Bloggine.csproj --no-build -c Release -o ./artifacts
