FROM mcr.microsoft.com/dotnet/sdk:7.0 AS api-builder
WORKDIR /app
COPY . .
RUN sed -i s@/archive.ubuntu.com/@/mirrors.aliyun.com/@g /etc/apt/sources.list
RUN cd src/ZServer.API && dotnet restore
RUN cd src/ZServer.API && dotnet publish -c Release -o out
RUN rm -rf src/ZServer.SiloHost/out/appsettings.Nacos.json
RUN rm -rf src/ZServer.SiloHost/out/runtimes/linux-arm
RUN rm -rf src/ZServer.SiloHost/out/runtimes/linux-arm64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/linux-musl-x64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/osx
RUN rm -rf src/ZServer.SiloHost/out/runtimes/osx-x64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/win-arm64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/win-x64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/win-x86
RUN rm -rf src/ZServer.API/out/shapes
RUN rm -rf src/ZServer.API/out/Fonts

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS zserver
ENV LANG C.UTF-8
WORKDIR /app
COPY --from=api-builder /app/src/ZServer.API/out .
RUN mkdir /app/shapes && mkdir /app/Fonts
ENTRYPOINT ["dotnet", "zapi.dll"]
