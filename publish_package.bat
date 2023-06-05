$NUGET_SERVER='https://api.nuget.org/v3/index.json'
Remove-Item 'src/ZMap/bin/Release' -Recurse
Remove-Item 'src/ZMap/bin/Release' -Recurse
Remove-Item 'src/ZMap.Renderer.SkiaSharp/bin/Release' -Recurse
Remove-Item 'src/ZMap.Source.Postgre/bin/Release' -Recurse
Remove-Item 'src/ZMap.Source.ShapeFile/bin/Release' -Recurse
Remove-Item 'src/ZMap.TileGrid/bin/Release' -Recurse
Remove-Item 'src/ZServer/bin/Release' -Recurse
Remove-Item 'src/ZServer.Grains/bin/Release' -Recurse
Remove-Item 'src/ZServer.Interfaces/bin/Release' -Recurse
Remove-Item 'src/ZMap.DynamicCompiler/bin/Release' -Recurse
Remove-Item 'src/ZMap/bin/Debug' -Recurse
Remove-Item 'src/ZMap/bin/Debug' -Recurse
Remove-Item 'src/ZMap.Renderer.SkiaSharp/bin/Debug' -Recurse
Remove-Item 'src/ZMap.Source.Postgre/bin/Debug' -Recurse
Remove-Item 'src/ZMap.Source.ShapeFile/bin/Debug' -Recurse
Remove-Item 'src/ZMap.TileGrid/bin/Debug' -Recurse
Remove-Item 'src/ZServer/bin/Debug' -Recurse
Remove-Item 'src/ZServer.Grains/bin/Debug' -Recurse
Remove-Item 'src/ZServer.Interfaces/bin/Debug' -Recurse
Remove-Item 'src/ZMap.DynamicCompiler/bin/Debug' -Recurse
dotnet build -c Release ZServer.sln
dotnet pack -c Release ZServer.sln
Remove-Item 'src/Client/bin/Release' -Recurse
Remove-Item 'src/Console/bin/Release' -Recurse
Remove-Item 'src/ZServer.Benchmark/bin/Release' -Recurse
Remove-Item 'src/ZServer.SiloHost/bin/Release' -Recurse
Remove-Item 'src/Client/bin/Debug' -Recurse
Remove-Item 'src/Console/bin/Debug' -Recurse
Remove-Item 'src/ZServer.Benchmark/bin/Debug' -Recurse
Remove-Item 'src/ZServer.SiloHost/bin/Debug' -Recurse
dotnet nuget push **/*.nupkg --source $NUGET_SERVER --api-key $env:NUGET_KEY --skip-duplicate 