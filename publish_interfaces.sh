#!/usr/bin/env bash
export NUGET_SERVER=https://api.nuget.org/v3/index.json

rm -rf src/ZServer.Interfaces/bin/Release

dotnet build -c Release ZServer.sln
dotnet pack -c Release ZServer.sln
nuget push src/ZServer.Interfaces/bin/Release/*.nupkg -Source  https://api.nuget.org/v3/index.json