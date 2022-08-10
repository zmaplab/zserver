#!/usr/bin/env bash
export NUGET_SERVER=https://api.nuget.org/v3/index.json

rm -rf src/ZMap/bin/Release
rm -rf src/ZMap.Renderer.SkiaSharp/bin/Release
rm -rf src/ZMap.SLD/bin/Release
rm -rf src/ZMap.Source.Postgre/bin/Release
rm -rf src/ZMap.Source.ShapeFile/bin/Release
rm -rf src/ZMap.TileGrid/bin/Release
rm -rf src/ZServer/bin/Release
rm -rf src/ZServer.Grains/bin/Release
rm -rf src/ZServer.Interfaces/bin/Release
rm -rf src/ZMap.DynamicCompiler/bin/Release

dotnet build -c Release ZServer.sln
dotnet pack -c Release ZServer.sln
nuget push src/ZMap/bin/Release/*.nupkg -Source $NUGET_SERVER
nuget push src/ZMap.Renderer.SkiaSharp/bin/Release/*.nupkg   -Source $NUGET_SERVER
nuget push src/ZMap.SLD/bin/Release/*.nupkg   -Source $NUGET_SERVER
nuget push src/ZMap.Source.Postgre/bin/Release/*.nupkg   -Source $NUGET_SERVER
nuget push src/ZMap.Source.ShapeFile/bin/Release/*.nupkg -Source $NUGET_SERVER  
nuget push src/ZMap.TileGrid/bin/Release/*.nupkg   -Source $NUGET_SERVER
nuget push src/ZServer/bin/Release/*.nupkg   -Source $NUGET_SERVER
nuget push src/ZServer.Grains/bin/Release/*.nupkg   -Source $NUGET_SERVER
nuget push src/ZServer.Interfaces/bin/Release/*.nupkg   -Source $NUGET_SERVER
nuget push src/ZMap.DynamicCompiler/bin/Release/*.nupkg   -Source $NUGET_SERVER