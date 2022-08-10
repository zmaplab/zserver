FROM mcr.microsoft.com/dotnet/sdk:6.0 AS api-builder
WORKDIR /app
COPY . .
RUN sed -i s@/archive.ubuntu.com/@/mirrors.aliyun.com/@g /etc/apt/sources.list
RUN cd src/ZServer.SiloHost && dotnet restore
RUN cd src/ZServer.SiloHost && dotnet publish -c Release -o out
RUN rm -rf src/ZServer.SiloHost/out/appsettings.Nacos.json
RUN rm -rf src/ZServer.SiloHost/out/runtimes/linux-arm
RUN rm -rf src/ZServer.SiloHost/out/runtimes/linux-arm64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/linux-musl-x64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/osx
RUN rm -rf src/ZServer.SiloHost/out/runtimes/osx-x64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/win-arm64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/win-x64
RUN rm -rf src/ZServer.SiloHost/out/runtimes/win-x86
RUN rm -rf src/ZServer.SiloHost/out/shapes

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS zserver
ENV LANG C.UTF-8
WORKDIR /app
COPY --from=api-builder /app/src/ZServer.SiloHost/out .
ENTRYPOINT ["dotnet", "ZServer.SiloHost.dll"]
