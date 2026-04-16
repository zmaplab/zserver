FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-builder
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
RUN rm -rf src/ZServer.API/out/conf/appsettings.json

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS zserver
ENV LANG C.UTF-8
LABEL build-date=${BUILD_DATE}
WORKDIR /app
COPY --from=api-builder /app/src/ZServer.API/out .
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
RUN mkdir /app/shapes && mkdir /app/Fonts
ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["dotnet", "zapi.dll"]