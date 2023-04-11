FROM mcr.microsoft.com/dotnet/sdk:6.0 AS api-builder
WORKDIR /app
COPY . .
RUN cd src/ZServer.API && dotnet restore
RUN cd src/ZServer.API && dotnet publish -c Release -o out
RUN rm -rf src/ZServer.API/out/nacos.json
RUN rm -rf src/ZServer.API/out/zserver.json
RUN rm -rf src/ZServer.API/out/runtimes/linux-arm
RUN rm -rf src/ZServer.API/out/runtimes/linux-arm64
RUN rm -rf src/ZServer.API/out/runtimes/linux-musl-x64
RUN rm -rf src/ZServer.API/out/runtimes/osx
RUN rm -rf src/ZServer.API/out/runtimes/osx-x64
RUN rm -rf src/ZServer.API/out/runtimes/win-arm64
RUN rm -rf src/ZServer.API/out/runtimes/win-x64
RUN rm -rf src/ZServer.API/out/runtimes/win-x86
RUN rm -rf src/ZServer.API/out/shapes
RUN rm -rf src/ZServer.API/out/Fonts

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS zserver-api
WORKDIR /app
ENV LANG zh_CN.UTF-8
COPY --from=api-builder /app/src/ZServer.API/out .
RUN mkdir /app/shapes && mkdir /app/Fonts
ENTRYPOINT ["dotnet", "zapi.dll"]

